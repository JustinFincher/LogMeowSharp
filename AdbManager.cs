using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ImGuiNET;
using SharpAdbClient;

namespace FinGameWorks
{
    public class AdbManager
    {
//        public List<Device> listOfDevices = new List<Device>();
        private Timer adbRefreshTimer;
        private string adbOutPath;


        public void LoadADB()
        {
            Assembly assembly = GetType().Assembly;
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows);
            string adbPath = assembly.GetName().Name + ".Executable." + (isWindows ? "adb.exe" : "adb");
            Console.WriteLine(adbPath);
            Stream adbStream = assembly.GetManifestResourceStream(adbPath);
            byte[] adbdata = new byte[adbStream.Length];
            adbStream.Read(adbdata, 0, (int) adbStream.Length);
            adbStream.Close();
            adbOutPath = Path.Combine(Directory.GetParent(assembly.Location).FullName,
                (isWindows ? "adb.exe" : "adb"));
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
        }

        private static readonly Lazy<AdbManager> lazy =
            new Lazy<AdbManager>(() => new AdbManager());

        public static AdbManager Instance
        {
            get { return lazy.Value; }
        }

        private AdbManager()
        {
            Console.WriteLine("Adb Manager");
            LoadADB();
            AdbServer server = new AdbServer();
            server.StartServer(adbOutPath, true);

//            Timer adbRefreshTimer = new Timer(state =>
//            {
//                listOfDevices = AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress).ToList();
//                Console.WriteLine("listOfDevices");
//                Console.WriteLine(String.Join(Environment.NewLine, listOfDevices.Select(device => device.Model)));
//            }, null, 0, 2000);
        }
        
    }
}