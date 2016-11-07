using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace SRCDShandler
{
    class Program
    {
        public static string SRCDSPath = null;
        public static Process SRCDS;
        public static IntPtr MainWindowHandle;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Init() {
            bool success = File.Exists(Properties.Settings.Default.SRCDSPath);
            bool pathSuccess = (Path.GetExtension(Properties.Settings.Default.SRCDSPath) == ".exe");
            if (!success || !pathSuccess)
            {
                Console.WriteLine("Please input a valid executable file path.");
                string path = Console.ReadLine();
                bool success2 = File.Exists(path);
                bool pathSuccess2 = (Path.GetExtension(path) == ".exe");
                if (!success2 || !pathSuccess2)
                {
                    Init();
                    return;
                } else
                {
                    Properties.Settings.Default.SRCDSPath = path;
                    Properties.Settings.Default.Save();
                    Init();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Initialization successful!");
                RunSRCDS();
            }
        }
        static void RunSRCDS()
        {
            SRCDSPath = Properties.Settings.Default.SRCDSPath;
            Process process = new Process();
            process.StartInfo.FileName = SRCDSPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = "-console -game garrysmod +map ttt_mc_jondome +gamemode terrortown +maxplayers 16";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            SRCDS = process;

            MainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(MainWindowHandle, 2);

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                RunSRCDS();
                return;
            } else
            {
                ShowWindow(MainWindowHandle, 9);
                Console.WriteLine("SRCDS has been properly shut down. Press any key to close SRCDShandler.");
                Console.ReadKey();
            }
        }
        static void Main(string[] args)
        {
            Init();
        }
    }
}
