using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ImGuiNET;
using SharpAdbClient;

namespace FinGameWorks
{
    public class AdbManager : Singleton<AdbManager>
    {
        public List<DeviceData> listOfDevices = new List<DeviceData>();
        public int adbVersion;
        private Timer adbRefreshTimer;
        private string adbOutPath;
        private bool isWindows;
        private Assembly assembly;

        public void LoadADB()
        {
            string adbPath = assembly.GetName().Name + ".Executable." + (isWindows ? "adb.exe" : "adb");
            Console.WriteLine(adbPath);
            Stream adbStream = assembly.GetManifestResourceStream(adbPath);
            byte[] adbdata = new byte[adbStream.Length];
            adbStream.Read(adbdata, 0, (int) adbStream.Length);
            adbStream.Close();
            Console.WriteLine(adbOutPath);
            try
            {
                File.WriteAllBytes(adbOutPath, adbdata);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (!isWindows)
            {
                Process process = new System.Diagnostics.Process();
                ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "chmod";
                startInfo.Arguments = "+x adb";
                startInfo.WorkingDirectory = Directory.GetParent(assembly.Location).FullName;
                process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine(output);
            }
        }

        public void UnloadADB()
        {
            foreach (var process in Process.GetProcessesByName("adb"))
            {
                process.Kill();
            }
            try
            {
                File.Delete(adbOutPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Init()
        {
        }

        public AdbManager()
        {
            
            assembly = GetType().Assembly;
            isWindows = System.Runtime.InteropServices.RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows);
            adbOutPath = Path.Combine(Directory.GetParent(assembly.Location).FullName,
                (isWindows ? "adb.exe" : "adb"));
            UnloadADB();
            LoadADB();
            AdbServer server = new AdbServer();
            StartServerResult serverResult = server.StartServer(adbOutPath, true);
            adbVersion = AdbClient.Instance.GetAdbVersion();
            adbRefreshTimer = new Timer(state =>
            {
                try
                {
                    listOfDevices = AdbClient.Instance.GetDevices();
//                    Console.WriteLine("listOfDevices");
//                    Console.WriteLine(String.Join(Environment.NewLine, listOfDevices.Select(device => device.Name)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, null, 0, 2000);
        }

        public ConsoleOutputReceiver getAdbLogcat(DeviceData deviceData)
        {
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("logcat", deviceData, receiver);
            return receiver;
        }

    }
}