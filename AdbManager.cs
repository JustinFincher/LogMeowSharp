using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ImGuiNET;
using Managed.Adb;

namespace FinGameWorks
{
    public class AdbManager : Singleton<AdbManager>
    {
        public List<Device> listOfDevices = new List<Device>();
        public int adbVersion;
        private Timer adbRefreshTimer;
        private string adbOutPath;
        private bool isWindows;
        private Assembly assembly;

        //public void LoadADB()
        //{
        //    string adbPath = assembly.GetName().Name + ".Executable." + (isWindows ? "adb.exe" : "adb");
        //    Console.WriteLine(adbPath);
        //    Stream adbStream = assembly.GetManifestResourceStream(adbPath);
        //    byte[] adbdata = new byte[adbStream.Length];
        //    adbStream.Read(adbdata, 0, (int) adbStream.Length);
        //    adbStream.Close();
        //    Console.WriteLine(adbOutPath);
        //    try
        //    {
        //        File.WriteAllBytes(adbOutPath, adbdata);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        //public void UnloadADB()
        //{
        //    foreach (var process in Process.GetProcessesByName("adb"))
        //    {
        //        process.Kill();
        //    }
        //    try
        //    {
        //        File.Delete(adbOutPath);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        public void Init()
        {
        }

        public AdbManager()
        {

            //assembly = GetType().Assembly;
            //isWindows = System.Runtime.InteropServices.RuntimeInformation
            //    .IsOSPlatform(OSPlatform.Windows);
            //adbOutPath = Path.Combine(Directory.GetParent(assembly.Location).FullName,
            //    (isWindows ? "adb.exe" : "adb"));
            //UnloadADB();
            //LoadADB();


            AndroidDebugBridge.CreateBridge(@"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe", true);
            //AdbServer server = new AdbServer();
            //StartServerResult serverResult = server.StartServer(adbOutPath, true);
            //adbVersion = AdbClient.Instance.GetAdbVersion();
            adbRefreshTimer = new Timer(state =>
            {
                try
                {
                    listOfDevices = AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress).ToList();
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
        
    }
}