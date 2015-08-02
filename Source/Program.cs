// --------------------------------------------------
// ReiPatcher - Program.cs
// --------------------------------------------------

#region Usings
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using ExIni;

using Mono.Cecil;

using ReiPatcher.Patch;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{

    /// <summary>
    ///     ReiPatcher Configuration Module
    /// </summary>
    public static class RPConfig
    {
        #region Static Fields
        private static string _configFilePath;
        #endregion

        #region Static Properties
        /// <summary>
        ///     Configuration File
        /// </summary>
        public static IniFile ConfigFile { get; set; }

        /// <summary>
        ///     Configuration File Path
        /// </summary>
        public static string ConfigFilePath
        {
            get { return _configFilePath; }
            set
            {
                bool endsWith = value.EndsWith(".ini", StringComparison.InvariantCultureIgnoreCase);
                _configFilePath = endsWith
                                      ? value
                                      : value + ".ini";
                try
                {
                    ConfigFile = IniFile.FromFile(_configFilePath);
                }
                catch
                {
                    ConfigFile = new IniFile();
                }
            }
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        ///     Requests an Assembly to be Patched
        /// </summary>
        /// <param name="name"></param>
        public static void RequestAssembly(string name)
        {
            ConfigFile[Program.IniValues.ASSEMBLIES][Path.GetFileNameWithoutExtension(name)].Value = name;
            Save();
        }

        /// <summary>
        ///     Save Configuration File
        /// </summary>
        public static void Save()
        {
            ConfigFile.Save(ConfigFilePath);
        }
        #endregion
    }

    internal partial class Program
    {
        #region Constants
        private const string ARG_FORCECREATE = "-fc";
        private const string ARG_RELOAD = "-r";
        private const string ARG_USECREATE = "-c";
        private const string ARG_WAIT = "-w";
        private const string BACKUP_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";
        #endregion

        #region Static Properties
        public static string AssembliesDir => RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
        public static bool ForceCreate { get; set; }
        public static bool LoadBackups { get; set; }
        public static string PatchesDir => RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
        public static bool WaitUser { get; set; }
        #endregion

        #region Event Handlers
        private static Assembly AssemblyResolve(object sender, ResolveEventArgs e)
        {
            var dllName = new AssemblyName(e.Name).Name + ".dll";
            var fPath = Path.Combine(AssembliesDir, dllName);
            if (File.Exists(fPath))
            {
                var assData = File.ReadAllBytes(fPath);
                return Assembly.Load(assData);
            }

            dllName = new AssemblyName(e.Name).Name + ".dll";
            fPath = Path.Combine(PatchesDir, dllName);
            if (File.Exists(fPath))
            {
                var assData = File.ReadAllBytes(fPath);
                return Assembly.Load(assData);
            }

            var resourceName = "ReiPatcher.DLL." + dllName;
            using (var s = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream(resourceName))
            {
                if (s == null)
                    throw new MissingManifestResourceException("Resource '" + resourceName + "' not Found");

                var assemblyData = new Byte[s.Length];
                s.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
        #endregion

        #region Entry Point
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;

            Main_ParseArguments(args);

            PrintHeader();

            PrintSplitter();

            if (args.Length == 0 || RPConfig.ConfigFilePath == null)
                CallAndKill(PrintUsage);

            Main_Internal(args);
        }

        private static void Main_Internal(string[] args)
        {
            string temp;
            if (!File.Exists(RPConfig.ConfigFilePath) || ForceCreate)
                CallAndKill(() => WriteDefaultIni(RPConfig.ConfigFilePath));

            temp = string.Format("Loading configuration file: '{0}'", RPConfig.ConfigFilePath);
            ConsoleUtil.Print(temp, color: ConsoleColor.DarkYellow);

            CallAndKill(CheckDirectories, code => code != ExitCode.Success);

            PrintSplitter("Loading Patchers");

            PatchBase[] patchers = null;

            CallAndKill(() => LoadPatchers(out patchers), code => code != ExitCode.Success);


            if (patchers != null)
            {

                PrintSplitter("Pre-Patch");
                RunPrePatch(patchers);

                PrintSplitter("Loading Assemblies");
                var dlls = ListAssemblies();
                CallAndKill(() => CheckFiles(dlls), code => code != ExitCode.Success);
                var assemblies = dlls.Select(LoadAssembly)
                                     .ToArray();

                PrintSplitter("Patching");
                RunPatchers(patchers, assemblies);

                foreach (var ass in assemblies)
                {
                    if (!ass.FromBackup && !ass.WasPatched)
                    {
                        ConsoleUtil.Print
                            ($"Not Patched '{Path.GetFileName(ass.Location)}'", color: ConsoleColor.DarkYellow);
                        continue;
                    }
                    var attrs = AttributeUtil.GetPatchedAttributes(ass.Assembly);
                    if (!ass.FromBackup && attrs.None(attribute => attribute.Info == "ReiPatcher"))
                    {
                        string destFileName = string.Format
                            ("{0}.{1:" + BACKUP_DATE_FORMAT + "}.bak", ass.Location, DateTime.Now);

                        ConsoleUtil.Print
                            ($"Creating Assembly Backup: '{Path.GetFileName(destFileName)}'",
                             color: ConsoleColor.DarkYellow);

                        File.Copy(ass.Location, destFileName);
                    }
                    if (attrs.None())
                        AttributeUtil.SetPatchedAttribute(ass.Assembly, "ReiPatcher");

                    ConsoleUtil.Print($"Saving '{Path.GetFileName(ass.Location)}'", color: ConsoleColor.DarkGreen);
                    ass.Assembly.Write(ass.Location);
                }

                PrintSplitter("Post-Patch");
                RunPostPatch(patchers);

                PrintSplitter("Finished");

            }

            string exe = RPConfig.ConfigFile["Launch"]["Executable"].Value;
            if (string.IsNullOrEmpty(exe))
                Kill(ExitCode.Success);

            string wd = RPConfig.ConfigFile["Launch"]["Directory"].Value;
            wd = string.IsNullOrEmpty(wd)
                     ? Path.GetDirectoryName(exe)
                     : wd;
            var arg = RPConfig.ConfigFile["Launch"]["Arguments"].Value;
            var psi = new ProcessStartInfo(exe, arg);
            if (wd != null)
                psi.WorkingDirectory = wd;
            Process.Start(psi);

            Kill(ExitCode.Success);
        }

        private static void Main_ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case ARG_FORCECREATE:
                        ForceCreate = true;
                        goto case ARG_USECREATE;
                    case ARG_USECREATE:
                        RPConfig.ConfigFilePath = args[++i];
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
        #endregion

        #region Static Methods
        private static string[] ListAssemblies()
        {
            return (from key in RPConfig.ConfigFile[IniValues.ASSEMBLIES].Keys
                    let prefix = key.Value.EndsWith(".dll")
                    let dll = prefix
                                  ? key.Value
                                  : key.Value + ".dll"
                    let rooted = Path.IsPathRooted(dll)
                    let fullPath = rooted
                                       ? dll
                                       : Path.Combine(AssembliesDir, dll)
                    select fullPath).ToArray();
        }

        private static PatcherArguments LoadAssembly(string ass)
        {
            string dir = Path.GetDirectoryName(ass);
            string dll = Path.GetFileName(ass);

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
                    string first = backups.First();

                    ConsoleUtil.Print($"Loading '{dll}' From Backup", color: ConsoleColor.DarkYellow);

                    return new PatcherArguments(ReadAssembly(first), ass, true);
                }
            }
            Console.WriteLine("Loading '{0}'", dll);
            return new PatcherArguments(ReadAssembly(ass), ass, false);
        }

        private static void PrintHeader()
        {
            AssemblyName version = Assembly.GetExecutingAssembly()
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

            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly()
                                                           .GetCustomAttributes
                (typeof (AssemblyCopyrightAttribute), false)
                                                           .Cast<AssemblyCopyrightAttribute>().FirstOrDefault();

            ConsoleUtil.Print($"{version.Name} {version.Version}", Console.BufferWidth, true);

            if (description != null)
            {
                ConsoleUtil.Print(description.Description, Console.BufferWidth, true);
            }
            if (copyright != null)
            {
                ConsoleUtil.Print(copyright.Copyright, Console.BufferWidth, true);
            }
            if (informational != null)
            {
                ConsoleUtil.Print
                    (informational.InformationalVersion, Console.BufferWidth, true, color: ConsoleColor.DarkGray);
            }
        }

        private static void PrintSplitter(string s = "")
        {
            s = s.Length > 0
                    ? " " + s + " "
                    : string.Empty;
            ConsoleUtil.Print(s, Console.BufferWidth - 2, true, '=');
        }

        private static AssemblyDefinition ReadAssembly(string path)
        {
            using (FileStream fs = File.OpenRead(path))
                return AssemblyDefinition.ReadAssembly(fs);
        }
        #endregion
    }

}