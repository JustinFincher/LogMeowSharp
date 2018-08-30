using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using SharpAdbClient;
using SharpAdbClient.Logs;

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
        private DeviceMonitor deviceMonitor;
        const int LOG_ITEM_CAP = 100;

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
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }, null, 0, 2000);
            deviceMonitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
            deviceMonitor.DeviceConnected += (sender, args) =>
            {
                addAdbLogToQueue(args.Device);
            };
            deviceMonitor.DeviceDisconnected += (sender, args) =>
            {
                removeAdbLogFromQuene(args.Device);
            };  
            deviceMonitor.DeviceChanged += (sender, args) =>
            {

            };
            deviceMonitor.Start();
        }

        public Dictionary<string, CancellationTokenSource> serialTaskDict = new Dictionary<string, CancellationTokenSource>();

        public Dictionary<string, List<string>> serialStringBuilderDict = new Dictionary<string, List<string>>();

        private Task getAdbLogcatTask(DeviceData deviceData, Action<LogEntry> messageCallback,
            CancellationTokenSource cancellationTokenSource)
        {
            //LogId[] logNames = new[] { LogId.Main, LogId.Crash, LogId.Events };
            return AdbClient.Instance.RunLogServiceAsync(deviceData, messageCallback, cancellationTokenSource.Token);
        }

        private void runAdbLogcatTask(DeviceData deviceData, Action<LogEntry> messageCallback)
        {
            if (serialTaskDict.ContainsKey(deviceData.Serial))
            {
                return;
            }
            var tokenSource = new CancellationTokenSource();
            Task task = getAdbLogcatTask(deviceData, messageCallback, tokenSource);
            serialTaskDict.Add(deviceData.Serial, tokenSource);
        }

        private void stopAdbLogcatTask(DeviceData deviceData)
        {
            if (serialTaskDict.ContainsKey(deviceData.Serial))
            {
                serialTaskDict[deviceData.Serial].Cancel();
            }
        }

        private void beginAdbLog(DeviceData deviceData, List<string> logEntries)
        {
            runAdbLogcatTask(deviceData, entry =>
            {
                String text = Encoding.UTF8.GetString(entry.Data, 0, entry.Data.Length);

                Console.WriteLine(text);

                foreach (string s in text.GetLines(true))
                {
                    if (!String.IsNullOrWhiteSpace(s) && !String.IsNullOrEmpty(s))
                    {

                        if (logEntries.Count >= LOG_ITEM_CAP)
                        {
                            logEntries.RemoveAt(0);
                        }
                        logEntries.Add(s);
                    }
                }

            });
        }

        public void addAdbLogToQueue(DeviceData deviceData)
        {
            List<string> logEntries = new List<string>();
            serialStringBuilderDict.Add(deviceData.Serial, logEntries);
            beginAdbLog(deviceData, logEntries);
        }

        public void removeAdbLogFromQuene(DeviceData deviceData)
        {
            stopAdbLogcatTask(deviceData);
            serialStringBuilderDict.Remove(deviceData.Serial);
        }
    }
}