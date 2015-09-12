// --------------------------------------------------
// ReiPatcher - RPConfig.cs
// --------------------------------------------------

#region Usings

using System;
using System.IO;

using ExIni;

#endregion

namespace ReiPatcher
{
    /// <summary>
    ///     ReiPatcher Configuration Module
    /// </summary>
    public static class RPConfig
    {
        private static string _configFilePath;

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

        /// <summary>
        ///     Gets a Configuration file Value
        /// </summary>
        /// <param name="sec">Section</param>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static string GetConfig(string sec, string key)
        {
            return ConfigFile[sec][key].Value;
        }

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

        /// <summary>
        ///     Sets a Configuration file Value
        /// </summary>
        /// <param name="sec">Section</param>
        /// <param name="key">Key</param>
        /// <param name="val">Value</param>
        public static void SetConfig(string sec, string key, string val)
        {
            ConfigFile[sec][key].Value = val;
            Save();
        }
    }
}
