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
using System.Text.RegularExpressions;

namespace SRCDShandler
{
    class Program
    {
        public static string SRCDSPath = null;
        public static Process SRCDS;
        public static IntPtr MainWindowHandle;
        public static bool ShouldReadCommands = true;
        public static List<Command> Commands = new List<Command>();

        public delegate void AsyncReadCommands();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Init() {
            Console.Title = "SRCDShandler";
            InitCommands();
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
                AsyncReadCommands read = new AsyncReadCommands(ReadCommands);
                read.BeginInvoke(null, null);
                RunSRCDS();
            }
        }
        static void InitCommands()
        {
            Command changePath = new Command("srcdspath");
            changePath.OnCommandRun = delegate (string[] args)
            {
                string path = args[0];
                bool success = File.Exists(path);
                bool pathSuccess = (Path.GetExtension(path) == ".exe");
                if (!success || !pathSuccess)
                {
                    Console.WriteLine("Specified file is not an executable.");
                    return;
                }
                else
                {
                    Console.WriteLine("Successfully set SRCDS path.");
                    Properties.Settings.Default.SRCDSPath = path;
                    Properties.Settings.Default.Save();
                    SRCDSPath = Properties.Settings.Default.SRCDSPath;
                    return;
                }
            };
            Commands.Add(changePath);

            Command changeArgs = new Command("srcdsargs");
            changeArgs.OnCommandRun = delegate (string[] args)
            {
                if (String.IsNullOrWhiteSpace(String.Join(" ", args)))
                {
                    Console.WriteLine("SRCDS launch arguments have been reset to default.");
                    Properties.Settings.Default.SRCDSArgs = "-game garrysmod +map gm_construct +gamemode sandbox +maxplayers 16";
                } else
                {
                    Console.WriteLine("SRCDS launch arguments have successfully been set.");
                    Properties.Settings.Default.SRCDSArgs = String.Join(" ", args);
                }
                Properties.Settings.Default.Save();
            };
            Commands.Add(changeArgs);

            Command srcdsRestart = new Command("srcdsrestart");
            srcdsRestart.OnCommandRun = delegate (string[] args)
            {
                SRCDS.Kill();
                return;
            };
            Commands.Add(srcdsRestart);
        }
        static void ReadCommands()
        { 
            while (ShouldReadCommands)
            {
                string input = Console.ReadLine();
                if (!String.IsNullOrWhiteSpace(input))
                {
                    Command command = Commands.Find(x => {
                        if (Regex.IsMatch(input, "^" + x.Identifier))
                        {
                            return true;
                        } else
                        {
                            return false;
                        }
                    });
                    if (command != null && !String.IsNullOrWhiteSpace(command.Identifier))
                    {
                        input = Regex.Replace(input, "^" + command.Identifier + "\\s*", "");
                        string[] args = input.Split();
                        command.OnCommandRun(args);
                    }
                }
            }
        }
        static void RunSRCDS()
        {
            SRCDSPath = Properties.Settings.Default.SRCDSPath;
            Process process = new Process();
            process.StartInfo.FileName = SRCDSPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = "-console " + Properties.Settings.Default.SRCDSArgs;
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
                ShouldReadCommands = false;
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
