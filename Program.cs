using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace FinGameWorks
{
    class Program
    {
        private static Sdl2Window window;
        private static GraphicsDevice graphicDevice;
        private static CommandList commandList;
        private static ImGuiRenderer guiRender;
        private static Vector3 backgroundColor = new Vector3(0.2f);
        private static Assembly assembly;

        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 600, 400, WindowState.Normal, "LogMeow"),
                new GraphicsDeviceOptions(true, null, true),
                GraphicsBackend.Metal,
                out window,
                out graphicDevice);

            window.Resized += () =>
            {
                graphicDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                guiRender.WindowResized(window.Width, window.Height);
            };
            commandList = graphicDevice.ResourceFactory.CreateCommandList();
            guiRender = new ImGuiRenderer(graphicDevice, graphicDevice.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);

            window.Closed += () =>
            {
                Environment.Exit(0);
            };


            assembly = typeof(Program).Assembly;
            string[] names = assembly.GetManifestResourceNames();
            Console.WriteLine(String.Join(Environment.NewLine, names));
            LoadFont();
            
            // Main application loop
            while (window.Exists)
            {
                InputSnapshot snapshot = window.PumpEvents();
                if (!window.Exists) { break; }
                guiRender.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                SubmitUI();
                LogMeowWorkspace.Instance.draw();
                commandList.Begin();
                commandList.SetFramebuffer(graphicDevice.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, new RgbaFloat(backgroundColor.X, backgroundColor.Y, backgroundColor.Z, 1f));
                guiRender.Render(graphicDevice, commandList);
                commandList.End();
                graphicDevice.SubmitCommands(commandList);
                graphicDevice.SwapBuffers(graphicDevice.MainSwapchain);
            }

            graphicDevice.WaitForIdle();
            guiRender.Dispose();
            commandList.Dispose();
            graphicDevice.Dispose();
        }

        private static void LoadFont()
        {
            string fontPath = assembly.GetName().Name + ".Fonts.FiraCode-Retina.ttf";
            Console.WriteLine(fontPath);
            Stream fontStream = assembly.GetManifestResourceStream(fontPath);
            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int) fontStream.Length);
            fontStream.Close();
            unsafe
            {
                fixed (byte* pFontData = fontdata)
                {
                    ImGui.GetIO().FontAtlas.AddFontFromMemoryTTF((System.IntPtr) pFontData, fontdata.Length, 16);
                }
            }
        }
        private static bool imguiDebugWindowShown;
        private static bool imguiMetricsWindowShown;
        private static unsafe void SubmitUI()
        {
            Style style = ImGui.GetStyle();
            ImGui.StyleColorsDark(style);
            style.GrabRounding = 12;
            style.FrameRounding = 4;
            style.NativePtr->WindowBorderSize = 1.0f;
            style.NativePtr->ChildBorderSize = 1.0f;
            style.NativePtr->FrameBorderSize = 1.0f;
            style.NativePtr->PopupBorderSize = 1.0f;
            
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("LogMeow"))
                {
                    if (ImGui.MenuItem("IMGUI References", "COMMAND+R"))
                    {
                        imguiDebugWindowShown = true;
                    }
                    if (ImGui.MenuItem("IMGUI Metrics", "COMMAND+M"))
                    {
                        imguiMetricsWindowShown = true;
                    }
                    if (ImGui.MenuItem("Quit", "COMMAND+Q"))
                    {
                        Environment.Exit(0);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Widgets"))
                {
                    if (ImGui.MenuItem("ADB Panel", "P"))
                    {
                        LogMeowWorkspace.Instance.adbPanelWindowShown = true;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("FPS: " + ImGui.GetIO().GetNativePointer()->Framerate.ToString("F"), false))
                {
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu(System.DateTime.Now.ToShortDateString() + " " +System.DateTime.Now.ToLongTimeString(),false))
                {
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            if (imguiDebugWindowShown)
            {
                ImGuiNative.igShowDemoWindow(ref imguiDebugWindowShown);
            }

            if (imguiMetricsWindowShown)
            {
                ImGuiNative.igShowMetricsWindow(ref imguiMetricsWindowShown);
            }
        }

    }
}
