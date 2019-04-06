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

        /** /
        啟動計時器，每三秒偵測一次USB Device
        /**/
        private void SetupYeeTimer()
        {
            YeeTimer = new Timer(3000);
            YeeTimer.Elapsed += OnTime;
            YeeTimer.AutoReset = true;
            YeeTimer.Enabled = true;
        }

        /** /
        定義計時器執行Function
        /**/
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

        /** /
        獲取已連接USB儲存裝置
        /**/
        public static List<DriveInfo> GetConnectedDevices()
        {
            //Put all connected drives into an array
            DriveInfo[] myDrives = DriveInfo.GetDrives();

            List<DriveInfo> Drives = new List<DriveInfo>();

            return myDrives.Where(info => (info.DriveType == DriveType.Removable && info.IsReady)).ToList();
        }

        /** /
        WMP初始化、播放影片
        /**/
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

        /** /
        將自身註冊到工作排程器並執行
        /**/
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

        /** /
        檢查某服務是否已經存在
        /**/
        bool DoesServiceExist(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(serviceName));
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
