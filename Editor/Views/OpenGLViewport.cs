using Editor.Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Editor.Views
{
    public class OpenGLViewport : HwndHost
    {
        private const int WindowStyleChild = 0x40000000;
        private const int WindowStyleVisible = 0x10000000;

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(
            int extendedStyle, string className, string windowName, int style,
            int positionX, int positionY, int width, int height,
            IntPtr parentHandle, IntPtr menuHandle, IntPtr instanceHandle, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr windowHandle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        private IntPtr viewportWindowHandle;
        private bool isDesignMode;

        public OpenGLViewport()
        {
            isDesignMode = DesignerProperties.GetIsInDesignMode(this);
        }

        static OpenGLViewport()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "engine.dll");

                if (!System.IO.File.Exists(dllPath))
                {
                    MessageBox.Show("DLL not found: " + dllPath);
                }
                else
                {
                    try
                    {
                        NativeLibrary.Load(dllPath);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef parentWindowHandle)
        {
            viewportWindowHandle = CreateWindowEx(
                0, "static", "OpenGL Viewport", WindowStyleChild | WindowStyleVisible,
                0, 0, 800, 600,
                parentWindowHandle.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (!isDesignMode)
            {
                EngineBindings.InitializeGameEngine(viewportWindowHandle);
                System.Windows.Media.CompositionTarget.Rendering += OnRenderFrame;
            }

            return new HandleRef(this, viewportWindowHandle);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (viewportWindowHandle != IntPtr.Zero)
            {
                int w = Math.Max(1, (int)finalSize.Width);
                int h = Math.Max(1, (int)finalSize.Height);
                const uint SWP_NOZORDER = 0x0004;
                const uint SWP_NOACTIVATE = 0x0010;
                SetWindowPos(viewportWindowHandle, IntPtr.Zero, 0, 0, w, h, SWP_NOZORDER | SWP_NOACTIVATE);
                if (!isDesignMode)
                    EngineBindings.ResizeEngineViewport(w, h);
            }
            return finalSize;
        }

        protected override void DestroyWindowCore(HandleRef windowHandle)
        {
            if (!isDesignMode)
            {
                System.Windows.Media.CompositionTarget.Rendering -= OnRenderFrame;
                EngineBindings.ShutdownGameEngine();
            }

            DestroyWindow(windowHandle.Handle);
        }

        private void OnRenderFrame(object? sender, EventArgs eventArguments)
        {
            if (!isDesignMode)
            {
                EngineBindings.RenderEngineFrame();
            }
        }
    }
}