// --------------------------------------------------
// ReiPatcher - Program.ExitCode.cs
// --------------------------------------------------

#region Usings
using System;
using System.IO;
using System.Linq;
using System.Reflection;

using ReiPatcher.INI;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{

    partial class Program
    {
        #region Enums

        #region ExitCode
        private enum ExitCode
        {

            NoPatchesApplied = 1,
            Success = 0,
            NoPatchesFound = -3,
            DirectoryNotFound = -1,
            FileNotFound = -2,

        }
        #endregion

        #endregion

        #region Static Methods
        private static void CallAndKill(Func<ExitCode> function, Func<ExitCode, bool> predicate = null)
        {
            ExitCode exitCode = function();
            if (predicate != null)
            {
                if (predicate(exitCode))
                    Kill(exitCode);
            }
            else
            {
                Kill(exitCode);
            }
        }

        private static ExitCode CheckDirectories()
        {
            string patchesDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
            string assembliesDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
            if (!Directory.Exists(patchesDir))
            {
                ConsoleUtil.Print("Patches Directory not Found", color:ConsoleColor.Red);
                return ExitCode.DirectoryNotFound;
            }

            if (!Directory.Exists(assembliesDir))
            {
                ConsoleUtil.Print("Assemblies Directory not Found", color: ConsoleColor.Red);
                return ExitCode.DirectoryNotFound;
            }

            return ExitCode.Success;
        }

        private static ExitCode CheckFiles(params string[] files)
        {
            foreach (string file in files)
            {
                Console.WriteLine("File: '{0}'", Path.GetFileName(file));
                if (File.Exists(file))
                    continue;


                string temp = string.Format("File not Found: '{0}'", file);
                ConsoleUtil.Print(temp, color: ConsoleColor.Red);
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
            Type patchType = typeof (PatchBase);
            string patchesDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
            var patchDlls = Directory.GetFiles(patchesDir, "*.dll", SearchOption.TopDirectoryOnly);
            string local = Environment.CurrentDirectory;
            Console.WriteLine("Loading Patchers");
            var patches = (from dll in patchDlls
                           let ass = Assembly.LoadFile(Path.Combine(local, dll))
                           let iPatches =
                               (from type in ass.GetTypes() where patchType.IsAssignableFrom(type) select type)
                           from iPatch in iPatches
                           select Activator.CreateInstance(iPatch) as PatchBase).ToArray();

            if (patches.Length == 0)
            {
                patchers = null;
                Console.WriteLine("No Patches Found");
                return ExitCode.NoPatchesFound;
            }

            patches.ForEach(patch => Console.WriteLine("Loaded Patcher '{0} {1}'", patch.Name, patch.Version));

            patchers = patches;
            return ExitCode.Success;
        }

        private static ExitCode PrintUsage()
        {
            Console.WriteLine("Switch Usage:");
            Console.WriteLine("{0} <Config> \t\tForce creation of new configuration file", ARG_FORCECREATE);
            Console.WriteLine("{0} <Config> \t\tUse or create of configuration file", ARG_USECREATE);
            Console.WriteLine("{0} \t\tWaits for user input after patching", ARG_WAIT);
            Console.WriteLine("{0} \t\tReloads latest Assembly backup", ARG_RELOAD);

            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            foreach (ExitCode val in Enum.GetValues(typeof (ExitCode)))
            {
                Console.WriteLine("{1: 0;-0} \t\t\t{0}", Enum.GetName(typeof (ExitCode), val), (int) val);
            }
            return ExitCode.Success;
        }

        private static ExitCode WriteDefaultIni(string path)
        {
            Console.WriteLine("Creating configuration file: '{0}'", ConfigFilePath);

            IniFile ini = new IniFile();

            IniSection main = ini[IniValues.MAIN];
            main.Comments.Append
                ("Default Configuration file for ReiPatcher",
                 "You can use the $(REGISTRY_PATH) function in any of the Key Values to retrieve a Registry key string",
                 "You can use %ENVIRONMENT% in any of the Key Values to expand a environment variables",
                 "You may (re)define a environment variable by creating a comment in the ;@name=value format anywhere within this file");

            IniKey mainPatches = main[IniValues.MAIN_PATCHES];
            mainPatches.Comments.Append("Directory to search for Patches");
            mainPatches.Value = "Patches";

            IniKey mainAssemblies = main[IniValues.MAIN_ASSEMBLIES];
            mainAssemblies.Comments.Append("Directory to Look for Assemblies to Patch");
            mainAssemblies.Value = String.Empty;

            ini[IniValues.ASSEMBLIES].Comments.Append
                ("Add .NET Assembly Entries Here",
                 "Absolute or Relative to ReiPatcher.AssembliesDir",
                 "In the Format <Name>=<Path>");

            IniSection launch = ini[IniValues.LAUNCH];
            launch.Comments.Append("Configures Application to Start Post Patching (Launcher)");
            launch[IniValues.LAUNCH_EXE].Value = String.Empty;
            launch[IniValues.LAUNCH_ARG].Value = String.Empty;
            launch[IniValues.LAUNCH_DIR].Value = String.Empty;
            ini.Save(path);

            return ExitCode.Success;
        }
        #endregion

        #region Nested type: IniValues
        private struct IniValues
        {
            #region Constants
            public const string ASSEMBLIES = "Assemblies";

            public const string LAUNCH = "Launch";
            public const string LAUNCH_ARG = "Arguments";
            public const string LAUNCH_DIR = "Directory";
            public const string LAUNCH_EXE = "Executable";

            public const string MAIN = "ReiPatcher";
            public const string MAIN_ASSEMBLIES = "AssembliesDir";
            public const string MAIN_PATCHES = "PatchesDir";
            #endregion
        }
        #endregion
    }

}