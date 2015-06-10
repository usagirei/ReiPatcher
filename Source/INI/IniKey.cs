// --------------------------------------------------
// ReiPatcher - IniKey.cs
// --------------------------------------------------

#region Usings
using System;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Win32;

#endregion

namespace ReiPatcher.INI
{

    public class IniKey
    {
        #region Fields
        private IniComment _comments;
        private string _key;
        private string _value;
        #endregion

        #region Properties
        public IniComment Comments
        {
            get { return _comments; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string RawValue
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Value
        {
            get { return Resolve(_value); }
            set { _value = value; }
        }
        #endregion

        #region (De)Constructors
        public IniKey(string key, string value = null)
        {
            Key = key;
            Value = value;

            _comments = new IniComment();
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return String.Format("{0}={1}", Key, RawValue);
        }

        public void SetIfNull(string value)
        {
            if (string.IsNullOrEmpty(Value))
                Value = value;
        }
        #endregion

        #region Static Methods
        private static string GetEnvironment(string env)
        {
            return Environment.ExpandEnvironmentVariables(env);
        }

        private static string GetRegistry(string path)
        {
            string keyName = Path.GetDirectoryName(path);
            string valueName = Path.GetFileName(path);
            if (String.IsNullOrEmpty(keyName))
                return null;
            object regVal = Registry.GetValue(keyName, valueName, String.Empty);

            return regVal != null
                ? regVal.ToString()
                : null;
        }

        private static string Resolve(string value)
        {
            if (value == null)
                return null;

            Regex reg = new Regex(@"\$\((?<reg>.*)\)");
            Regex env = new Regex(@"%.*%");
            while (reg.IsMatch(value) || env.IsMatch(value))
            {
                value = reg.Replace(value, match => GetRegistry(match.Groups["reg"].Value));
                value = GetEnvironment(value);
            }
            return value;
        }
        #endregion
    }

}