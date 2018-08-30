using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImGuiNET;
using SharpAdbClient;

namespace FinGameWorks
{
    public class LogMeowWorkspaceView : Singleton<LogMeowWorkspaceView>
    {
        
        public bool adbPanelWindowShown;
        public bool logCatWindowShown;

        public DeviceData selecteDeviceData;

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
                        selecteDeviceData = device;
                    }
                    ImGui.NextColumn();
                }
                ImGui.Separator();
                ImGui.EndWindow();

            }

            if (logCatWindowShown)
            {
                List<String> stringList = new List<string>();
                try
                {
                    stringList = selecteDeviceData == null
                   ? new List<string>()
                   : (AdbManager.Instance.serialStringBuilderDict.ContainsKey(selecteDeviceData.Serial) ? AdbManager.Instance.serialStringBuilderDict[
                       selecteDeviceData.Serial] : new List<string>());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                ImGui.BeginWindow("ADB Logcat", ref logCatWindowShown, WindowFlags.Default);
                for (int i = 0; i < stringList.Count; i++)
                {
                    ImGui.TextUnformatted(stringList[i]);
                    ImGui.Separator();
                }
                ImGui.SetScrollHere(1.0f);
                ImGui.EndWindow();
            }
        }

    }
}