#pragma once

#include <cstddef>
#include <cstdint>
#include <atomic>
#include <mutex>
#include <source_location>

namespace Engine::Core
{
    // -------------------------------------------------------------------------
    // MemTag — subsystem allocation category
    // -------------------------------------------------------------------------
    enum class MemTag : uint8_t
    {
        General  = 0,
        ECS,
        Graphics,
        Audio,
        Temp,

        Count    // sentinel — keep last
    };

    // -------------------------------------------------------------------------
    // MemStats — per-tag statistics snapshot (always accurate, all builds)
    // -------------------------------------------------------------------------
    struct MemStats
    {
        uint64_t CurrentBytes = 0;   // bytes currently live under this tag
        uint64_t PeakBytes    = 0;   // high-water mark since startup
        uint64_t AllocCount   = 0;   // total allocations made
        uint64_t FreeCount    = 0;   // total frees made
    };

    // -------------------------------------------------------------------------
    // AllocationRecord — per-pointer audit entry (HE_MEMORY_AUDIT builds only)
    // -------------------------------------------------------------------------
#ifdef HE_MEMORY_AUDIT
    struct AllocationRecord
    {
        void*                Ptr      = nullptr;
        size_t               Size     = 0;
        MemTag               Tag      = MemTag::General;
        std::source_location Location = {};
        bool                 Active   = false;   // slot occupied
    };
#endif

    // -------------------------------------------------------------------------
    // MemoryManager
    //
    // Meyer's singleton. All allocations funnel through Allocate/Free.
    // A 16-byte AllocationHeader is prepended to every allocation so that Free
    // can recover the size (for CurrentBytes accuracy) and the original raw
    // pointer (for correct handling of aligned allocations) without any lookup.
    //
    // Stats (CurrentBytes, PeakBytes, AllocCount, FreeCount) are tracked per
    // MemTag using std::atomics — lock-free on the hot path in all builds.
    //
    // When HE_MEMORY_AUDIT is defined (Debug builds), a fixed open-addressing
    // hash table records every live allocation with its source_location.
    // Call WalkLeaks() at shutdown to detect leaks with file/line context.
    // -------------------------------------------------------------------------
    class MemoryManager
    {
    public:
        /// Singleton access — thread-safe initialization (C++11 guarantee).
        static MemoryManager& Get();

        MemoryManager(const MemoryManager&)            = delete;
        MemoryManager& operator=(const MemoryManager&) = delete;
        MemoryManager(MemoryManager&&)                 = delete;
        MemoryManager& operator=(MemoryManager&&)      = delete;

        // ---- Core allocation API ----------------------------------------

        /// Allocate 'size' bytes tagged with 'tag'.
        /// Pass 'alignment' > 0 to request alignment stricter than the default
        /// (alignof(std::max_align_t) on the current platform).
        /// 'alignmentOffset' is the EASTL sub-offset hint; stored but currently
        /// not used by _aligned_malloc (matches legacy behaviour).
        [[nodiscard]]
        void* Allocate(
            size_t               size,
            MemTag               tag             = MemTag::General,
            size_t               alignment       = 0,
            size_t               alignmentOffset = 0,
            std::source_location location        = std::source_location::current());

        /// Free a pointer previously returned by Allocate.
        /// Passing nullptr is safe (no-op).
        void Free(void* ptr);

        // ---- Statistics API ---------------------------------------------

        /// Thread-safe snapshot of per-tag statistics.
        MemStats GetStats(MemTag tag) const;

        /// Sum of CurrentBytes across all tags.
        uint64_t GetTotalCurrentBytes() const;

        /// Print a formatted stats table via fmt to stdout.
        /// (Does not allocate — safe to call in diagnostics paths.)
        void DumpStats() const;

        // ---- Audit API (HE_MEMORY_AUDIT only) ---------------------------
#ifdef HE_MEMORY_AUDIT
        /// Walk all currently-live AllocationRecords.
        /// 'visitor' is called once per live record. Must not allocate inside.
        void WalkLeaks(void(*visitor)(const AllocationRecord&)) const;

        /// Count of live records in the audit table.
        uint32_t LiveAllocationCount() const;
#endif

    private:
        MemoryManager();
        ~MemoryManager() = default;

        // ------------------------------------------------------------------
        // AllocationHeader — prepended to every raw allocation.
        //
        //   [ AllocationHeader (24 bytes) | ... padding ... | user data ]
        //                                                    ^--- returned ptr
        //
        // sizeof = 24, alignof = 8.  Because we always over-allocate by
        // (sizeof(AllocationHeader) + effectiveAlign - 1) bytes, the user
        // pointer is guaranteed to be aligned to effectiveAlign.
        // ------------------------------------------------------------------
        struct AllocationHeader
        {
            void*    RawPtr;      // original malloc pointer — used by Free
            size_t   Size;        // user-requested size     — used for stats
            MemTag   Tag;         // subsystem tag           — used for stats
            uint8_t  _pad[7];     // explicit padding to reach sizeof = 24
        };
        static_assert(sizeof(AllocationHeader)  == 24);
        static_assert(alignof(AllocationHeader) ==  8);

        // ------------------------------------------------------------------
        // Per-tag counters — cache-line isolated to prevent false sharing.
        // ------------------------------------------------------------------
        struct alignas(64) TagCounters
        {
            std::atomic<uint64_t> CurrentBytes { 0 };
            std::atomic<uint64_t> PeakBytes    { 0 };
            std::atomic<uint64_t> AllocCount   { 0 };
            std::atomic<uint64_t> FreeCount    { 0 };
        };

        TagCounters m_TagCounters[static_cast<size_t>(MemTag::Count)];

        /// CAS loop to update PeakBytes without locking.
        void UpdatePeak(TagCounters& counters, uint64_t newValue);

        // ------------------------------------------------------------------
        // Audit table (HE_MEMORY_AUDIT builds only)
        // ------------------------------------------------------------------
#ifdef HE_MEMORY_AUDIT
        static constexpr uint32_t k_AuditCapacity = 65536; // ~3 MB static footprint

        AllocationRecord   m_AuditTable[k_AuditCapacity];
        mutable std::mutex m_AuditMutex;

        uint32_t AuditSlot(void* ptr) const;
        void     AuditInsert(void* ptr, size_t size, MemTag tag, std::source_location loc);
        void     AuditRemove(void* ptr);
#endif
    };

} // namespace Engine::Core


// =============================================================================
// Allocation macros
// =============================================================================

/// Raw allocation with tag (no explicit alignment).
#define HE_ALLOC(size, tag) \
    Engine::Core::MemoryManager::Get().Allocate((size), (tag))

/// Raw aligned allocation with tag.
#define HE_ALLOC_ALIGNED(size, tag, alignment) \
    Engine::Core::MemoryManager::Get().Allocate((size), (tag), (alignment))

/// Free a pointer returned by HE_ALLOC / HE_ALLOC_ALIGNED / HE_New.
#define HE_FREE(ptr) \
    Engine::Core::MemoryManager::Get().Free(ptr)


// =============================================================================
// Typed helpers — construct / destroy via the memory manager
// =============================================================================

/// Allocate sizeof(T), then placement-new with forwarded arguments.
template<typename T, typename... Args>
[[nodiscard]] T* HE_New(Engine::Core::MemTag tag, Args&&... args)
{
    void* mem = Engine::Core::MemoryManager::Get().Allocate(
        sizeof(T), tag, alignof(T));
    return ::new(mem) T(std::forward<Args>(args)...);
}

/// Call ~T(), then free through the memory manager.
template<typename T>
void HE_Delete(T* ptr)
{
    if (ptr)
    {
        ptr->~T();
        Engine::Core::MemoryManager::Get().Free(ptr);
    }
}
