using System;
using System.Linq;
using ImGuiNET;

namespace FinGameWorks
{
    public class LogMeowWorkspace
    {
        private static readonly Lazy<LogMeowWorkspace> lazy =
            new Lazy<LogMeowWorkspace>(() => new LogMeowWorkspace());

        public static LogMeowWorkspace Instance
        {
            get { return lazy.Value; }
        }

        private LogMeowWorkspace()
        {
            AdbManager adb = AdbManager.Instance;
        }
        
        public bool adbPanelWindowShown;
        public bool logCatWindowShown;

        public void draw()
        {
            if (adbPanelWindowShown)
            {
                ImGui.BeginWindow("ADB Devices", ref adbPanelWindowShown, WindowFlags.Default);

                ImGui.Columns(1,"Devices",true);
                ImGui.Separator();
//                foreach (Device device in AdbManager.Instance.listOfDevices)
//                {
//                    if (ImGui.Selectable(device.Model))
//                    {
//                    }
//                    ImGui.NextColumn();
//                }
                ImGui.Separator();
                
                ImGui.EndWindow();
            }
        }

    }
}