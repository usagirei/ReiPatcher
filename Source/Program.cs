// --------------------------------------------------
// ReiPatcher - Program.cs
// --------------------------------------------------

#region Usings
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using Mono.Cecil;

using ReiPatcher.INI;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{

    internal partial class Program
    {
        #region Constants
        private const string ARG_FORCECREATE = "-fc";
        private const string ARG_RELOAD = "-r";
        private const string ARG_USECREATE = "-c";
        private const string ARG_WAIT = "-w";
        private const string BACKUP_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";
        #endregion

        #region Fields
        private static string _configFilePath;
        #endregion

        #region Static Properties
        public static IniFile ConfigFile { get; set; }

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
        public static bool LoadBackups { get; set; }

        public static bool WaitUser { get; set; }
        #endregion

        #region Event Handlers
        private static Assembly GitAssemblyResolve(object sender, ResolveEventArgs e)
        {
            String resourceName = "ReiPatcher.DLL." + new AssemblyName(e.Name).Name + ".dll";
            using (Stream s = Assembly.GetExecutingAssembly()
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
#if DEBUG
            args = new[] {"-c", "Default", "-w"};
#endif

            AppDomain.CurrentDomain.AssemblyResolve += GitAssemblyResolve;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;

            Main_ParseArguments(args);

            if (args.Length == 0 || ConfigFilePath == null)
                CallAndKill(PrintUsage);

            PrintHeader();
            ConsoleUtil.Print(String.Empty, Console.BufferWidth, false, '-');
            Main_Internal(args);
        }

        private static void Main_Internal(string[] args)
        {
            string temp;
            if (!File.Exists(ConfigFilePath) || ForceCreate)
                CallAndKill(() => WriteDefaultIni(ConfigFilePath));

            temp = string.Format("Loading configuration file: '{0}'", ConfigFilePath);
            ConsoleUtil.Print(temp, color: ConsoleColor.Yellow);

            ConfigFile = IniFile.FromFile(ConfigFilePath);

            CallAndKill(CheckDirectories, code => code != ExitCode.Success);

            PatchBase[] patchers = null;
            CallAndKill(() => LoadPatchers(out patchers), code => code != ExitCode.Success);

            PrintSplitter("Checking Assemblies");

            var dlls = ListAssemblies();
            CallAndKill(() => CheckFiles(dlls), code => code != ExitCode.Success);

            PrintSplitter("Loading Assemblies");
            var assemblies = dlls.Select(LoadAssembly)
                                 .ToArray();

            PrintSplitter("Executing Patchers");

            RunPrePatch(patchers);

            RunPatchers(patchers, assemblies);

            foreach (PatcherArguments ass in assemblies)
            {
                var attrs = AttributeUtil.GetPatchedAttributes(ass.Assembly);
                if (!ass.FromBackup && attrs.None())
                {
                    string destFileName = string.Format
                        ("{0}.{1:" + BACKUP_DATE_FORMAT + "}.bak", ass.Location, DateTime.Now);

                    temp = String.Format("Creating Assembly FromBackup: '{0}'", Path.GetFileName(destFileName));
                    ConsoleUtil.Print(temp, color: ConsoleColor.Yellow);

                    File.Copy(ass.Location, destFileName);
                }

                AttributeUtil.SetPatchedAttribute(ass.Assembly, "Patched");
                Console.WriteLine("Saving '{0}'", Path.GetFileName(ass.Location));
                ass.Assembly.Write(ass.Location);
            }

            RunPostPatch(patchers);

            PrintSplitter("Finished");

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
        #endregion

        #region Static Methods
        private static string[] ListAssemblies()
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

                    string temp = string.Format("Loading '{0}' From Backup", dll);
                    ConsoleUtil.Print(temp, color: ConsoleColor.Yellow);

                    return new PatcherArguments(AssemblyDefinition.ReadAssembly(first), ass, true);
                }
            }
            Console.WriteLine("Loading '{0}'", dll);
            return new PatcherArguments(AssemblyDefinition.ReadAssembly(ass), ass, false);
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

            string temp = string.Format("{0} {1}", version.Name, version.Version);
            ConsoleUtil.Print(temp, Console.BufferWidth, true);

            if (description != null)
            {
                ConsoleUtil.Print(description.Description, Console.BufferWidth, true);
            }
            if (informational != null)
            {
                ConsoleUtil.Print
                    (informational.InformationalVersion, Console.BufferWidth, true, color: ConsoleColor.DarkGray);
            }
        }

        private static void PrintSplitter(string s = "")
        {
            ConsoleUtil.Print(s, Console.BufferWidth / 2, true, '-');
        }
        #endregion
    }

}