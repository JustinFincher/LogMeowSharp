using System;
using System.Linq;
using ImGuiNET;
using Managed.Adb;

namespace FinGameWorks
{
    public class LogMeowWorkspaceView : Singleton<LogMeowWorkspaceView>
    {
        
        public bool adbPanelWindowShown;
        public bool logCatWindowShown;

        public void draw()
        {
            if (adbPanelWindowShown)
            {
                ImGui.BeginWindow("ADB Devices", ref adbPanelWindowShown, WindowFlags.Default);

                ImGui.Columns(1,"Devices",true);
                ImGui.Separator();
                foreach (Device device in Singleton<AdbManager>.Instance.listOfDevices)
                {
                    if (ImGui.Selectable(device.Model + "-" + device.AvdName + "-" + device.SerialNumber))
                    {
                    }
                    ImGui.NextColumn();
                }
                ImGui.Separator();
                
                ImGui.EndWindow();
            }

            if (logCatWindowShown)
            {
                
            }
        }

    }
}