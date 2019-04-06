using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using WMPLib;

namespace Yee
{
    class Program
    {
        private static Timer YeeTimer;
        private static WindowsMediaPlayer Player;

        private void SetupYeeTimer()
        {
            YeeTimer = new Timer(3000);
            YeeTimer.Elapsed += OnTime;
            YeeTimer.AutoReset = true;
            YeeTimer.Enabled = true;
        }

        private void OnTime(Object source, ElapsedEventArgs e)
        {
            if (GetConnectedDevices().Count() > 0)
            {
                Console.WriteLine("USB Detected");
                Console.WriteLine(@Directory.GetCurrentDirectory());
                YeeTimer.Enabled = false;
                Yee();
            }
        }

        public static List<DriveInfo> GetConnectedDevices()
        {
            //Put all connected drives into an array
            DriveInfo[] myDrives = DriveInfo.GetDrives();

            List<DriveInfo> Drives = new List<DriveInfo>();

            return myDrives.Where(info => (info.DriveType == DriveType.Removable && info.IsReady)).ToList();
        }

        private void Yee()
        {
            Process[] processes = Process.GetProcessesByName("wmplayer");
            if (processes.Length == 0)
            {
                Player = new WindowsMediaPlayer();
                if (Player.isOnline)
                {
                    Player.uiMode = "Full";
                    Player.enableContextMenu = false;
                    Player.settings.volume = 100;
                    Player.openPlayer(@Directory.GetCurrentDirectory() + "\\yee10HR\\yee.mp4");
                    Player.controls.play();
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        private void RegisterService()
        {
            try
            {
                var path = @Directory.GetCurrentDirectory() + "\\Yee.exe";
                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                p.StartInfo = info;
                p.Start();
                // We start StreamWriter to add multiple CMD lines instead of user input.
                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        // Add task scheduler and execute
                        sw.WriteLine("SchTasks /Create /SC DAILY /TN Yee /TR {0}", path);
                        sw.WriteLine("SchTasks /run /tn Yee");
                    }
                }
            }

            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        static void Main(string[] args)
        {

            Program UsbDetect = new Program();
            if (args.Length == 0)
            {
                ProcessStartInfo info = new ProcessStartInfo(@Directory.GetCurrentDirectory() + "\\Yee.exe");
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;
                info.CreateNoWindow = true;
                info.ErrorDialog = false;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                UsbDetect.SetupYeeTimer();
                while (true) { YeeTimer.Start(); }
            }
            else {
                if (args[0] == "InstallService")
                {
                    UsbDetect.RegisterService();
                }
            }
        }

    }
}
