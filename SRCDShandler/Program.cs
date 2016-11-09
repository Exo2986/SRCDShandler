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
        public static string SRCDSArgs = null;

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
            changePath.RefVar = "SRCDSPath";
            changePath.OnCommandRun = delegate (string[] args)
            {
                string path = String.Join(" ", args);
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
            changeArgs.RefVar = "SRCDSArgs";
            changeArgs.OnCommandRun = delegate (string[] args)
            {
                if (String.IsNullOrWhiteSpace(String.Join(" ", args)))
                {
                    Console.WriteLine("SRCDS launch arguments have been reset to default.");
                    Properties.Settings.Default.SRCDSArgs = "-game garrysmod +map gm_construct +gamemode sandbox +maxplayers 16";
                    SRCDSArgs = Properties.Settings.Default.SRCDSArgs;
                } else
                {
                    Console.WriteLine("SRCDS launch arguments have successfully been set.");
                    Properties.Settings.Default.SRCDSArgs = String.Join(" ", args);
                    SRCDSArgs = Properties.Settings.Default.SRCDSArgs;
                }
                Properties.Settings.Default.Save();
            };
            Commands.Add(changeArgs);

            Command shouldMinimize = new Command("handlerminimize");
            shouldMinimize.RefVar = "HandlerMinimize";
            shouldMinimize.OnCommandRun = delegate (string[] args)
            {
                if (args[0].ToLowerInvariant() == "true" || args[0].ToLowerInvariant() == "false" || args[0].ToLowerInvariant() == "1" || args[0].ToLowerInvariant() == "0")
                {
                    Properties.Settings.Default.HandlerMinimize = (args[0].ToLowerInvariant() == "true" || args[0].ToLowerInvariant() == "1");
                    Properties.Settings.Default.Save();
                }
                return;
            };
            Commands.Add(shouldMinimize);

            Command srcdsRestart = new Command("srcdsrestart");
            srcdsRestart.OnCommandRun = delegate (string[] args)
            {
                SRCDS.Kill();
                return;
            };
            Commands.Add(srcdsRestart);

            Command printValue = new Command("printvalue");
            printValue.OnCommandRun = delegate (string[] args)
            {
                Command command = Commands.Find(x => {
                    if (Regex.IsMatch(args[0], "^" + x.Identifier))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
                if (command != null && !String.IsNullOrWhiteSpace(command.Identifier) && !String.IsNullOrWhiteSpace(command.RefVar) && Properties.Settings.Default[command.RefVar] != null)
                {
                    Console.WriteLine(Properties.Settings.Default[command.RefVar]);
                }
            };
            Commands.Add(printValue);

            Command quit = new Command("quit");
            quit.OnCommandRun = delegate (string[] args)
            {
                if (!SRCDS.HasExited)
                {
                    SRCDS.CloseMainWindow();
                }
                Environment.Exit(0);
            };
            Commands.Add(quit);
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
            if (SRCDSPath == null)
            {
                SRCDSPath = Properties.Settings.Default.SRCDSPath;
            }
            if (SRCDSArgs == null)
            {
                SRCDSArgs = Properties.Settings.Default.SRCDSArgs;
            }
            Process process = new Process();
            process.StartInfo.FileName = SRCDSPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = "-console " + SRCDSArgs;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            SRCDS = process;

            if (Properties.Settings.Default.HandlerMinimize)
            {
                MainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(MainWindowHandle, 2);
            }

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
            List<string> launchArgs = new List<string>();
            launchArgs.Add("srcdsargs");
            launchArgs.Add("srcdspath");
            int index = 0;
            //Console.WriteLine(String.Join(" ", args));
            foreach(string arg in args)
            {
                foreach(string launchArg in launchArgs)
                {
                    if (arg == "-" + launchArg)
                    {
                        switch(launchArg)
                        {
                            case "srcdsargs":
                                string pattern = arg + "\\s\"[^\"]+\"";
                                Match match = Regex.Match(String.Join(" ", args), pattern);
                                if (match.Success)
                                {
                                    string sargs = match.Value;
                                    sargs = sargs.Replace(arg + " ", "");
                                    sargs = sargs.Replace("\"", "");
                                    SRCDSArgs = sargs;
                                }
                                break;
                            case "srcdspath":
                                pattern = arg + "\\s\"[^\"]+\"";
                                match = Regex.Match(String.Join(" ", args), pattern);
                                if (match.Success)
                                {
                                    string spath = match.Value;
                                    spath = spath.Replace(arg + " ", "");
                                    spath = spath.Replace("\"", "");
                                    bool success = File.Exists(spath);
                                    bool pathSuccess = Path.GetExtension(spath) == ".exe";
                                    if (success && pathSuccess)
                                    {
                                        SRCDSPath = spath;
                                    }
                                }
                                break;
                        }
                    }
                }
                index++;
            }
            Init();
        }
    }
}
