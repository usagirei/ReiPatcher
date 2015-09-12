// --------------------------------------------------
// ReiPatcher - Program.ExitCode.cs
// --------------------------------------------------

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ExIni;

using ReiPatcher.Patch;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{
    partial class Program
    {
        private static void CallAndKill(Func<ExitCode> function, Func<ExitCode, bool> predicate = null)
        {
            if (predicate?.Invoke(function()) ?? true)
                Kill(function());
        }

        private static ExitCode CheckDirectories()
        {
            var patchesDir = RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
            if (!Directory.Exists(patchesDir))
            {
                ConsoleUtil.Print("Patches Directory not Found", color: ConsoleColor.Red);
                return ExitCode.DirectoryNotFound;
            }

            var assembliesDir = RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
            if (!Directory.Exists(assembliesDir))
            {
                ConsoleUtil.Print("Assemblies Directory not Found", color: ConsoleColor.Red);
                return ExitCode.DirectoryNotFound;
            }

            return ExitCode.Success;
        }

        private static ExitCode CheckFiles(params string[] files)
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                    continue;

                ConsoleUtil.Print($"File not Found: '{file}'", color: ConsoleColor.Red);
                return ExitCode.FileNotFound;
            }

            return ExitCode.Success;
        }

        private static void Kill(ExitCode code)
        {
            if (WaitUser)
            {
                Console.WriteLine("Press any key to continue. . .");
                Console.Read();
            }
            Environment.Exit((int) code);
        }

        private static ExitCode LoadPatchers(out PatchBase[] patchers)
        {
            //Debugger.Launch();
            var pType = typeof(PatchBase);
            var pDir = RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
            var pFiles = Directory.GetFiles(pDir, "*.dll", SearchOption.TopDirectoryOnly);
            var cd = Environment.CurrentDirectory;

            Console.WriteLine("Loading Patchers");

            var pList = new List<PatchBase>();
            foreach (var dll in pFiles)
            {
                try
                {
                    var dllPath = Path.Combine(cd, dll);
                    Console.WriteLine(dllPath);
                    Assembly ass = null;

                    ass = Assembly.LoadFrom(dllPath);

                    var pbs = (from t in ass.GetTypes() where pType.IsAssignableFrom(t) select t);
                    foreach (var pb in pbs)
                    {
                        if (!pb.IsClass || pb.IsAbstract || pb.IsInterface)
                            continue;
                        pList.Add(Activator.CreateInstance(pb) as PatchBase);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.ForegroundColor = ConsoleColor.Gray;
                    patchers = new PatchBase[0];
                    return ExitCode.InternalException;
                }
            }

            if (pList.Count == 0)
            {
                patchers = new PatchBase[0];
                Console.WriteLine("No Patches Found");
                return ExitCode.Success;
            }

            foreach (var patch in pList)
                Console.WriteLine("Loaded Patcher '{0} {1}'", patch.Name, patch.Version);

            patchers = pList.ToArray();
            return ExitCode.Success;
        }

        private static bool NotSuccess(ExitCode code) => code != ExitCode.Success;

        private static ExitCode PrintUsage()
        {
            Console.WriteLine("Switch Usage:");
            Console.WriteLine("{0} <Config> \t\tForce creation of new configuration file", ARG_FORCECREATE);
            Console.WriteLine("{0} <Config> \t\tUse or create of configuration file", ARG_USECREATE);
            Console.WriteLine("{0} \t\tWaits for user input after patching", ARG_WAIT);
            Console.WriteLine("{0} \t\tReloads latest Assembly backup", ARG_RELOAD);

            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            foreach (ExitCode val in Enum.GetValues(typeof(ExitCode)))
            {
                Console.WriteLine("{1: 0;-0} \t\t\t{0}", Enum.GetName(typeof(ExitCode), val), (int) val);
            }
            return ExitCode.Success;
        }

        private static ExitCode WriteDefaultIni(string path)
        {
            Console.WriteLine("Creating configuration file: '{0}'", RPConfig.ConfigFilePath);

            var ini = new IniFile();

            var main = ini[IniValues.MAIN];
            main.Comments.Append
                (
                 "Default Configuration file for ReiPatcher",
                    "You can use the $(REGISTRY_PATH) function in any of the Key Values to retrieve a Registry key string",
                    "You can use %ENVIRONMENT% in any of the Key Values to expand a environment variables",
                    "You may (re)define a environment variable by creating a comment in the ;@name=value format anywhere within this file");

            var mainPatches = main[IniValues.MAIN_PATCHES];
            mainPatches.Comments.Append("Directory to search for Patches");
            mainPatches.Value = "Patches";

            var mainAssemblies = main[IniValues.MAIN_ASSEMBLIES];
            mainAssemblies.Comments.Append("Directory to Look for Assemblies to Patch");
            mainAssemblies.Value = string.Empty;

            ini[IniValues.ASSEMBLIES].Comments.Append
                (
                 "Add .NET Assembly Entries Here",
                    "Absolute or Relative to ReiPatcher.AssembliesDir",
                    "In the Format <Name>=<Path>");

            var launch = ini[IniValues.LAUNCH];
            launch.Comments.Append("Configures Application to Start Post Patching (Launcher)");
            launch[IniValues.LAUNCH_EXE].Value = string.Empty;
            launch[IniValues.LAUNCH_ARG].Value = string.Empty;
            launch[IniValues.LAUNCH_DIR].Value = string.Empty;
            ini.Save(path);

            return ExitCode.Success;
        }

        #region Nested type: ExitCode

        private enum ExitCode
        {
            NoPatchesApplied = 1,
            Success = 0,
            DirectoryNotFound = -1,
            FileNotFound = -2,
            InternalException = -3,
            //NoPatchesFound = -3, // Not used anymore
        }

        #endregion

        #region Nested type: IniValues

        internal struct IniValues
        {
            public const string ASSEMBLIES = "Assemblies";
            public const string LAUNCH = "Launch";
            public const string LAUNCH_ARG = "Arguments";
            public const string LAUNCH_DIR = "Directory";
            public const string LAUNCH_EXE = "Executable";
            public const string MAIN = "ReiPatcher";
            public const string MAIN_ASSEMBLIES = "AssembliesDir";
            public const string MAIN_PATCHES = "PatchesDir";
        }

        #endregion
    }
}
