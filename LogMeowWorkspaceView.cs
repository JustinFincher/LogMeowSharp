﻿using System;
using System.Linq;
using ImGuiNET;
using SharpAdbClient;

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
                foreach (DeviceData device in Singleton<AdbManager>.Instance.listOfDevices)
                {
                    if (ImGui.Selectable(device.Model + "-" + device.Name + "-" + device.Serial))
                    {
                        ConsoleOutputReceiver receiver = AdbManager.Instance.getAdbLogcat(device);
                        Console.WriteLine(receiver.ToString());
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