using System;
using System.Runtime.InteropServices;

namespace Editor.Interop
{
    public static class EngineBindings
    {
        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitializeGameEngine(IntPtr windowHandle);

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearScene();

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadGltfEntity(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string filePath,
            float px, float py, float pz,
            float rx, float ry, float rz,
            float sx, float sy, float sz);

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadCameraEntity(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            float px, float py, float pz,
            float rx, float ry, float rz,
            float fovDeg, float nearPlane, float farPlane);

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadDirectionalLightEntity(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            float px, float py, float pz,
            float rx, float ry, float rz,
            float r, float g, float b,
            float intensityLux);

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResizeEngineViewport(int width, int height);

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderEngineFrame();

        [DllImport("engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShutdownGameEngine();
    }
}