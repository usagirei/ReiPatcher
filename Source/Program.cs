// --------------------------------------------------
// ReiPatcher - Program.cs
// --------------------------------------------------

#region Usings
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using ReiPatcher.INI;

#endregion

namespace ReiPatcher
{

    internal partial class Program
    {
        #region Constants
        private const string ARG_FORCECREATE = "-fc";
        private const string ARG_USECREATE = "-c";
        private const string ARG_WAIT = "-w";
        private const string ARG_RELOAD = "-r";
        private const string BACKUP_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";
        #endregion

        #region Fields
        private static string _configFilePath;
        #endregion

        #region Static Properties
        public static IniFile ConfigFile { get; set; }

        public static bool LoadBackups { get; set; }

        public static string ConfigFilePath
        {
            get { return _configFilePath; }
            set
            {
                bool endsWith = value.EndsWith(".ini", StringComparison.InvariantCultureIgnoreCase);
                _configFilePath = endsWith
                    ? value
                    : value + ".ini";
            }
        }

        public static bool ForceCreate { get; set; }

        public static bool WaitUser { get; set; }
        #endregion

        #region Entry Point
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
#if DEBUG
            args = new[] {"-c", "Default", "-w"};
#endif
            PrintHeader();
            WriteSplitter(true);
            ParseArguments(args);

            if (args.Length == 0 || ConfigFilePath == null)
                CallAndKill(PrintUsage);

            if (!File.Exists(ConfigFilePath) || ForceCreate)
                CallAndKill(() => WriteDefaultIni(ConfigFilePath));

            WriteColored(ConsoleColor.Yellow, "Loading configuration file: '{0}'", ConfigFilePath);
            ConfigFile = IniFile.FromFile(ConfigFilePath);

            CallAndKill(CheckDirectories, code => code != ExitCode.Success);

            PatchBase[] patchers = null;
            CallAndKill(() => LoadPatchers(out patchers), code => code != ExitCode.Success);

            WriteSplitter();

            Console.WriteLine("Checking Assemblies");
            var dlls = GetAssemblies();
            CallAndKill(() => CheckFiles(dlls), code => code != ExitCode.Success);

            WriteSplitter();
            Console.WriteLine("Loading Assemblies");
            var assemblies = dlls.Select(LoadAssembly)
                                 .ToArray();
            WriteSplitter();

            RunPrePatch(patchers);

            RunPatchers(patchers, assemblies);

            foreach (var ass in assemblies)
            {
                var attrs = AttributeUtil.GetPatchedAttributes(ass.Assembly);
                if (!ass.FromBackup && attrs.None())
                {
                    string destFileName = string.Format
                        ("{0}.{1:" + BACKUP_DATE_FORMAT + "}.bak", ass.Location, DateTime.Now);
                    WriteColored
                        (ConsoleColor.Yellow, "Creating Assembly FromBackup: '{0}'", Path.GetFileName(destFileName));
                    File.Copy(ass.Location, destFileName);
                }

                AttributeUtil.SetPatchedAttribute(ass.Assembly, "Patched");
                Console.WriteLine("Saving '{0}'", Path.GetFileName(ass.Location));
                ass.Assembly.Write(ass.Location);
            }

            RunPostPatch(patchers);

            WriteSplitter();

            Console.WriteLine("Finished");

            Kill(ExitCode.Success);
        }

        private static PatcherArguments LoadAssembly(string ass)
        {
            var dir = Path.GetDirectoryName(ass);
            var dll = Path.GetFileName(ass);

            if (LoadBackups)
            {
                var backups = (from backup in Directory.GetFiles(dir, "*.bak")
                               let fName = Path.GetFileName(backup)
                               where fName.StartsWith(dll)
                               let clip = Path.GetFileNameWithoutExtension(backup)
                                              .Substring(dll.Length + 1)
                               let date = DateTime.ParseExact(clip, BACKUP_DATE_FORMAT, CultureInfo.InvariantCulture)
                               orderby date descending
                               select backup).ToArray();

                if (backups.Any())
                {
                    var first = backups.First();
                    WriteColored(ConsoleColor.Yellow, "Loading '{0}' From Backup", dll);
                    return new PatcherArguments(AssemblyDefinition.ReadAssembly(first), ass, true);
                }
            }
            Console.WriteLine("Loading '{0}'", dll);
            return new PatcherArguments(AssemblyDefinition.ReadAssembly(ass), ass, false);
        }
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

        private static string[] GetAssemblies()
        {
            string assDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
            return (from key in ConfigFile[IniValues.ASSEMBLIES].Keys
                    let prefix = key.Value.EndsWith(".dll")
                    let dll = prefix
                        ? key.Value
                        : key.Value + ".dll"
                    let rooted = Path.IsPathRooted(dll)
                    let fullPath = rooted
                        ? dll
                        : Path.Combine(assDir, dll)
                    select fullPath).ToArray();
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

        private static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case ARG_FORCECREATE:
                        ForceCreate = true;
                        goto case ARG_USECREATE;
                    case ARG_USECREATE:
                        ConfigFilePath = args[++i];
                        break;
                    case ARG_WAIT:
                        WaitUser = true;
                        break;
                    case ARG_RELOAD:
                        LoadBackups = true;
                        break;
                }
            }
        }

        private static void PrintHeader()
        {
            AssemblyName ver = Assembly.GetExecutingAssembly()
                                       .GetName();

            AssemblyDescriptionAttribute description = Assembly.GetExecutingAssembly()
                                                               .GetCustomAttributes
                (typeof (AssemblyDescriptionAttribute), false)
                                                               .Cast<AssemblyDescriptionAttribute>()
                                                               .FirstOrDefault();
            AssemblyInformationalVersionAttribute informational = Assembly.GetExecutingAssembly()
                                                                          .GetCustomAttributes
                (typeof (AssemblyInformationalVersionAttribute), false)
                                                                          .Cast<AssemblyInformationalVersionAttribute>()
                                                                          .FirstOrDefault();

            WriteCentered("{0} {1}", ver.Name, ver.Version);
            if (description != null)
            {
                WriteCentered(description.Description);
            }
            if (informational != null)
            {
                WriteColoredCenter(ConsoleColor.DarkGray, informational.InformationalVersion);
            }
            
        }

        private static void RunPatchers(PatchBase[] patchers, params PatcherArguments[] assemblies)
        {
            foreach (PatcherArguments def in assemblies)
            {
                foreach (PatchBase patcher in patchers)
                {
                    try
                    {
                        if (patcher.CanPatch(def))
                        {
                            Console.WriteLine("Patching '{0}'", patcher.Name);
                            patcher.Patch(def);
                        }
                        else
                        {
                            WriteColored(ConsoleColor.DarkGray, "Skipping '{0}'", patcher.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteColored
                            (ConsoleColor.Red,
                             "Error in Patcher '{0}' at Assembly '{1}'",
                             patcher.Name,
                             def.Assembly.FullName);
                        WriteColored(ConsoleColor.Red, "  {0}", ex.Message);
                        Kill(ExitCode.NoPatchesApplied);
                    }
                }
            }
        }

        private static void RunPostPatch(PatchBase[] patchers)
        {
            patchers.ForEach
                (patcher =>
                {
                    try
                    {
                        Console.WriteLine("Post-Patch '{0}'", patcher.Name);
                        patcher.PostPatch();
                    }
                    catch (Exception ex)
                    {
                        WriteColored(ConsoleColor.Red, "Error in Patcher '{0}'", patcher.Name);
                        WriteColored(ConsoleColor.Red, "  {0}", ex.Message);
                        Kill(ExitCode.NoPatchesApplied);
                    }
                });
        }

        private static void RunPrePatch(PatchBase[] patchers)
        {
            patchers.ForEach
                (patcher =>
                {
                    try
                    {
                        Console.WriteLine("Pre-Patch '{0}'", patcher.Name);
                        patcher.PrePatch();
                    }
                    catch (Exception ex)
                    {
                        WriteColored(ConsoleColor.Red, "Error in Patcher '{0}'", patcher.Name);
                        WriteColored(ConsoleColor.Red, "  {0}", ex.Message);
                        Kill(ExitCode.NoPatchesApplied);
                    }
                });
        }

        private static void WriteColoredCenter(ConsoleColor color, string format, params object[] args)
        {
            string s = string.Format(format, args);
            int offset = (Console.BufferWidth - s.Length) / 2;
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.CursorLeft = offset;
            Console.WriteLine(s);
            Console.ForegroundColor = old;
        }

        private static void WriteCentered(string format, params object[] args)
        {
            string s = string.Format(format, args);
            int offset = (Console.BufferWidth - s.Length) / 2;
            Console.CursorLeft = offset;
            Console.WriteLine(s);
        }

        private static void WriteColored(ConsoleColor color, string format, params object[] args)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = old;
        }

        private static void WriteSplitter(bool full = false)
        {
            if (full)
            {
                Console.Write(new string('-', Console.BufferWidth));
            }
            else
            {
                Console.Write(new string(' ', Console.BufferWidth / 4));
                Console.WriteLine(new string('-', Console.BufferWidth / 2));
            }
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
        private static ExitCode CheckDirectories()
        {
            string patchesDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
            string assembliesDir = ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
            if (!Directory.Exists(patchesDir))
            {
                WriteColored(ConsoleColor.Red, "Patches Directory not Found");
                return ExitCode.DirectoryNotFound;
            }

            if (!Directory.Exists(assembliesDir))
            {
                WriteColored(ConsoleColor.Red, "Assemblies Directory not Found");
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

                WriteColored(ConsoleColor.Red, "File not Found: '{0}'", file);
                return ExitCode.FileNotFound;
            }

            return ExitCode.Success;
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
    }

}