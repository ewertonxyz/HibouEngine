#include "Core/EnginePCH.h"
#include <new>
#include <cstdlib>   // malloc, free

// Memory.h is included transitively via EnginePCH.h.
// This file implements MemoryManager and the EASTL allocator hooks.

// =============================================================================
// Internal helpers
// =============================================================================
namespace
{
#ifdef HE_MEMORY_AUDIT
    /// Guards against re-entrant Allocate calls on the same thread.
    /// Re-entrancy can occur if std::mutex construction internally allocates
    /// (a known CRT edge-case). Stats are always safe (atomics), but the audit
    /// table insert must be skipped to avoid deadlock.
    thread_local bool t_InAllocation = false;

    struct ReentrancyGuard
    {
        bool m_WasIn;
        ReentrancyGuard()  : m_WasIn(t_InAllocation) { t_InAllocation = true; }
        ~ReentrancyGuard()                            { t_InAllocation = m_WasIn; }
        bool IsReentrant() const { return m_WasIn; }
    };
#endif
} // anonymous namespace

// =============================================================================
// Engine::Core::MemoryManager implementation
// =============================================================================
namespace Engine::Core
{

// ---- Singleton --------------------------------------------------------------

MemoryManager& MemoryManager::Get()
{
    static MemoryManager s_Instance; // C++11: thread-safe first-time init
    return s_Instance;
}

MemoryManager::MemoryManager()
{
    // TagCounters are zero-initialized via their atomic member initializers.
    // The audit table (if present) is zero-initialized by the struct defaults.
    // No heap allocations permitted here — we may be called before any
    // allocator infrastructure is ready.
}

// ---- Allocate ---------------------------------------------------------------

void* MemoryManager::Allocate(size_t size, MemTag tag,
                               size_t alignment, size_t /*alignmentOffset*/,
                               [[maybe_unused]] std::source_location location)
{
    // Effective alignment must be at least alignof(AllocationHeader) so that
    // the header struct itself is safely readable at (userPtr - sizeof(Header)).
    const size_t effectiveAlign =
        (alignment > alignof(AllocationHeader)) ? alignment : alignof(AllocationHeader);

    // Over-allocate: we need enough room to write the header AND align the
    // user pointer to effectiveAlign.
    //
    //   raw  -->  [ ...slack... | AllocationHeader (24B) | user data ]
    //                                                     ^-- userAddr
    //
    // The slack is at most (effectiveAlign - 1) bytes.
    const size_t totalSize = sizeof(AllocationHeader) + effectiveAlign - 1 + size;

    void* const raw = malloc(totalSize);
    if (!raw) return nullptr;

    // Find the first address >= (raw + sizeof(Header)) that is aligned.
    const uintptr_t rawAddr   = reinterpret_cast<uintptr_t>(raw);
    const uintptr_t headerEnd = rawAddr + sizeof(AllocationHeader);
    const uintptr_t userAddr  = (headerEnd + effectiveAlign - 1) & ~(effectiveAlign - 1);

    // Write header in the 24 bytes immediately before the user pointer.
    AllocationHeader* const header = reinterpret_cast<AllocationHeader*>(userAddr) - 1;
    header->RawPtr = raw;
    header->Size   = size;
    header->Tag    = tag;

    // Update per-tag stats (lock-free).
    auto& counters = m_TagCounters[static_cast<size_t>(tag)];
    counters.AllocCount.fetch_add(1, std::memory_order_relaxed);
    const uint64_t current =
        counters.CurrentBytes.fetch_add(size, std::memory_order_relaxed) + size;
    UpdatePeak(counters, current);

#ifdef HE_MEMORY_AUDIT
    ReentrancyGuard guard;
    if (!guard.IsReentrant())
        AuditInsert(reinterpret_cast<void*>(userAddr), size, tag, location);
#endif

    return reinterpret_cast<void*>(userAddr);
}

// ---- Free -------------------------------------------------------------------

void MemoryManager::Free(void* ptr)
{
    if (!ptr) return;

    // Recover size, tag, and original raw pointer from the embedded header.
    const AllocationHeader* const header =
        reinterpret_cast<const AllocationHeader*>(ptr) - 1;

    const size_t size  = header->Size;
    const MemTag tag   = header->Tag;
    void*        raw   = header->RawPtr;

    // Update stats.
    auto& counters = m_TagCounters[static_cast<size_t>(tag)];
    counters.CurrentBytes.fetch_sub(size, std::memory_order_relaxed);
    counters.FreeCount.fetch_add(1, std::memory_order_relaxed);

#ifdef HE_MEMORY_AUDIT
    AuditRemove(ptr);
#endif

    free(raw);
}

// ---- UpdatePeak (CAS loop) --------------------------------------------------

void MemoryManager::UpdatePeak(TagCounters& counters, uint64_t newValue)
{
    uint64_t peak = counters.PeakBytes.load(std::memory_order_relaxed);
    while (newValue > peak)
    {
        if (counters.PeakBytes.compare_exchange_weak(
                peak, newValue,
                std::memory_order_relaxed,
                std::memory_order_relaxed))
            break;
        // Another thread updated peak — re-read and retry.
    }
}

// ---- GetStats ---------------------------------------------------------------

MemStats MemoryManager::GetStats(MemTag tag) const
{
    const auto& c = m_TagCounters[static_cast<size_t>(tag)];
    MemStats s;
    s.CurrentBytes = c.CurrentBytes.load(std::memory_order_relaxed);
    s.PeakBytes    = c.PeakBytes.load(std::memory_order_relaxed);
    s.AllocCount   = c.AllocCount.load(std::memory_order_relaxed);
    s.FreeCount    = c.FreeCount.load(std::memory_order_relaxed);
    return s;
}

// ---- GetTotalCurrentBytes ---------------------------------------------------

uint64_t MemoryManager::GetTotalCurrentBytes() const
{
    uint64_t total = 0;
    for (size_t i = 0; i < static_cast<size_t>(MemTag::Count); ++i)
        total += m_TagCounters[i].CurrentBytes.load(std::memory_order_relaxed);
    return total;
}

// ---- DumpStats --------------------------------------------------------------

void MemoryManager::DumpStats() const
{
    static constexpr const char* k_TagNames[] =
        { "General", "ECS", "Graphics", "Audio", "Temp" };
    static_assert(std::size(k_TagNames) == static_cast<size_t>(MemTag::Count));

    fmt::print("=== MemoryManager Stats ===\n");
    for (size_t i = 0; i < static_cast<size_t>(MemTag::Count); ++i)
    {
        const auto& c = m_TagCounters[i];
        fmt::print("  {:8s}: current={:>10} B  peak={:>10} B  "
                   "allocs={:>8}  frees={:>8}\n",
            k_TagNames[i],
            c.CurrentBytes.load(std::memory_order_relaxed),
            c.PeakBytes.load(std::memory_order_relaxed),
            c.AllocCount.load(std::memory_order_relaxed),
            c.FreeCount.load(std::memory_order_relaxed));
    }
    fmt::print("  Total current: {} B\n", GetTotalCurrentBytes());
    fmt::print("===========================\n");
}

// =============================================================================
// Audit table implementation (HE_MEMORY_AUDIT builds only)
// =============================================================================
#ifdef HE_MEMORY_AUDIT

uint32_t MemoryManager::AuditSlot(void* ptr) const
{
    // Shift right by 4 to account for typical allocation alignment,
    // then take modulo to land in the table.
    const uintptr_t h = reinterpret_cast<uintptr_t>(ptr) >> 4;
    return static_cast<uint32_t>(h % k_AuditCapacity);
}

void MemoryManager::AuditInsert(void* ptr, size_t size, MemTag tag,
                                 std::source_location loc)
{
    std::lock_guard lock(m_AuditMutex);
    const uint32_t start = AuditSlot(ptr);
    for (uint32_t i = 0; i < k_AuditCapacity; ++i)
    {
        const uint32_t idx = (start + i) % k_AuditCapacity;
        if (!m_AuditTable[idx].Active)
        {
            m_AuditTable[idx] = AllocationRecord{ ptr, size, tag, loc, true };
            return;
        }
    }
    // Table full — stats remain accurate; this record is silently dropped.
}

void MemoryManager::AuditRemove(void* ptr)
{
    std::lock_guard lock(m_AuditMutex);
    const uint32_t start = AuditSlot(ptr);
    // Linear probe: scan the full ring to handle tombstone chains correctly.
    for (uint32_t i = 0; i < k_AuditCapacity; ++i)
    {
        const uint32_t idx = (start + i) % k_AuditCapacity;
        auto& rec = m_AuditTable[idx];
        if (rec.Active && rec.Ptr == ptr)
        {
            rec.Active = false;
            return;
        }
    }
    // Record not found — the allocation may have been made before the audit
    // table was active (e.g., very early CRT allocations). Safe to ignore.
}

void MemoryManager::WalkLeaks(void(*visitor)(const AllocationRecord&)) const
{
    std::lock_guard lock(m_AuditMutex);
    for (uint32_t i = 0; i < k_AuditCapacity; ++i)
    {
        if (m_AuditTable[i].Active)
            visitor(m_AuditTable[i]);
    }
}

uint32_t MemoryManager::LiveAllocationCount() const
{
    std::lock_guard lock(m_AuditMutex);
    uint32_t count = 0;
    for (uint32_t i = 0; i < k_AuditCapacity; ++i)
        count += m_AuditTable[i].Active ? 1u : 0u;
    return count;
}

#endif // HE_MEMORY_AUDIT

} // namespace Engine::Core


// =============================================================================
// EASTL Allocator Hooks
//
// EASTL requires these two operator new[] overloads to be defined in global
// scope. They delegate to MemoryManager, routing EASTL container allocations
// through the same managed path as all other engine allocations.
//
// The pName/file/line parameters supplied by EASTL are intentionally ignored;
// source_location::current() points to this call site, which is acceptable —
// EASTL-internal call site tracking is lower priority than engine-side tracking.
// =============================================================================

void* operator new[](size_t size, const char* /*pName*/, int /*flags*/,
                     unsigned /*debugFlags*/, const char* /*file*/, int /*line*/)
{
    return Engine::Core::MemoryManager::Get().Allocate(
        size,
        Engine::Core::MemTag::General);
}

void* operator new[](size_t size, size_t alignment, size_t alignmentOffset,
                     const char* /*pName*/, int /*flags*/,
                     unsigned /*debugFlags*/, const char* /*file*/, int /*line*/)
{
    return Engine::Core::MemoryManager::Get().Allocate(
        size,
        Engine::Core::MemTag::General,
        alignment,
        alignmentOffset);
}
