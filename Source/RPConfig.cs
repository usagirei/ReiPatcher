using System;
using System.IO;
using ExIni;

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
                try
                {
                    _configFilePath = value.EndsWith(".ini", StringComparison.InvariantCultureIgnoreCase)
                        ? value
                        : value + ".ini";
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
}