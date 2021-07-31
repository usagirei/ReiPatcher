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
using System.Threading;

using Mono.Cecil;

using ReiPatcher.Patch;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{
    internal partial class Program
    {
        private const string ARG_FORCECREATE = "-fc";
        private const string ARG_RELOAD = "-r";
        private const string ARG_USECREATE = "-c";
        private const string ARG_WAIT = "-w";
        private const string BACKUP_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";

        public static string AssembliesDir => RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_ASSEMBLIES].Value;
        public static bool ForceCreate { get; set; }
        public static bool LoadBackups { get; set; }
        public static string PatchesDir => RPConfig.ConfigFile[IniValues.MAIN][IniValues.MAIN_PATCHES].Value;
        public static bool WaitUser { get; set; }

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
            var dir = Path.GetDirectoryName(ass);
            var dll = Path.GetFileName(ass);

            if (LoadBackups)
            {
                var backup = (from file in Directory.GetFiles(dir, "*.bak")
                              let fName = Path.GetFileName(file)
                              where fName.StartsWith(dll)
                              let clip = Path.GetFileNameWithoutExtension(file)
                                             .Substring(dll.Length + 1)
                              let date = DateTime.ParseExact(clip, BACKUP_DATE_FORMAT, CultureInfo.InvariantCulture)
                              orderby date descending
                              select file).FirstOrDefault();

                if (!string.IsNullOrEmpty(backup))
                {
                    ConsoleUtil.Print($"Loading '{dll}' From Backup", color: ConsoleColor.DarkYellow);
                    return new PatcherArguments(ReadAssembly(backup), ass, true);
                }
            }
            Console.WriteLine("Loading '{0}'", dll);
            return new PatcherArguments(ReadAssembly(ass), ass, false);
        }

        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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
            if (!File.Exists(RPConfig.ConfigFilePath) || ForceCreate)
                CallAndKill(() => WriteDefaultIni(RPConfig.ConfigFilePath));

            ConsoleUtil.Print($"Loading configuration file: '{RPConfig.ConfigFilePath}'", color: ConsoleColor.DarkYellow);
            CallAndKill(CheckDirectories, NotSuccess);

            PrintSplitter("Loading Patchers");
            PatchBase[] patchers = null;
            CallAndKill(() => LoadPatchers(out patchers), NotSuccess);

            // Shouldn't ever be null, howerver...
            if (patchers != null)
            {
                PrintSplitter("Pre-Patch");
                RunPrePatch(patchers);

                PrintSplitter("Loading Assemblies");
                var dlls = ListAssemblies();
                CallAndKill(() => CheckFiles(dlls), NotSuccess);
                var assemblies = dlls.Select(LoadAssembly).ToArray();

                PrintSplitter("Patching");
                RunPatchers(patchers, assemblies);

                RunSaveAndBackup(assemblies);

                PrintSplitter("Post-Patch");
                RunPostPatch(patchers);

                
            }

            string exe = RPConfig.ConfigFile["Launch"]["Executable"].Value;
            if (string.IsNullOrEmpty(exe))
            {
                PrintSplitter("Finished");
                Kill(ExitCode.Success);
            }

            string wd = RPConfig.ConfigFile["Launch"]["Directory"].Value;
            wd = string.IsNullOrEmpty(wd)
                     ? Path.GetDirectoryName(exe)
                     : wd;

            var arg = RPConfig.ConfigFile["Launch"]["Arguments"].Value;
            var psi = new ProcessStartInfo(exe, arg);
            if (wd != null)
                psi.WorkingDirectory = wd;

            PrintSplitter("Launch");
            
            Console.WriteLine("Target:\t{0}", exe);
            Console.WriteLine("Arguments:\t{0}", arg);
            Console.WriteLine("Working Dir:\t{0}", wd);

            Process.Start(psi);

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

        private static void PrintHeader()
        {
            var version = Assembly.GetExecutingAssembly()
                                  .GetName();

            var description = Assembly.GetExecutingAssembly()
                                      .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                                      .Cast<AssemblyDescriptionAttribute>()
                                      .FirstOrDefault();

            var informational = Assembly.GetExecutingAssembly()
                                        .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                                        .Cast<AssemblyInformationalVersionAttribute>()
                                        .FirstOrDefault();

            var copyright = Assembly.GetExecutingAssembly()
                                    .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
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
                ConsoleUtil.Print(informational.InformationalVersion, Console.BufferWidth, true, color: ConsoleColor.DarkGray);
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
            {
                var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(AssembliesDir);
                resolver.AddSearchDirectory(PatchesDir);
                resolver.AddSearchDirectory(Path.GetDirectoryName(path));
                var @params = new ReaderParameters()
                {
                    AssemblyResolver = resolver
                };
                return AssemblyDefinition.ReadAssembly(fs, @params);
            }
        }
    }
}
