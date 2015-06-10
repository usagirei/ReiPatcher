// --------------------------------------------------
// ReiPatcher - PatcherArguments.cs
// --------------------------------------------------

#region Usings
using System;

using Mono.Cecil;

#endregion

namespace ReiPatcher
{

    /// <summary>
    ///     Patcher Arguments
    /// </summary>
    public class PatcherArguments : EventArgs
    {
        #region Fields
        private AssemblyDefinition _assembly;
        private string _location;
        private bool _fromBackup;
        #endregion

        #region Properties
        /// <summary>
        ///     Mono.Cecil <see cref="AssemblyDefinition" />
        /// </summary>
        public AssemblyDefinition Assembly
        {
            get { return _assembly; }
            private set { _assembly = value; }
        }

        /// <summary>
        ///     Assembly Locations
        /// </summary>
        public string Location
        {
            get { return _location; }
            private set { _location = value; }
        }

        /// <summary>
        ///     Is Backup Assembly
        /// </summary>
        public bool FromBackup
        {
            get { return _fromBackup; }
            private set { _fromBackup = value; }
        }
        #endregion

        #region (De)Constructors
        /// <summary>
        ///     Insantiates a new <see cref="PatcherArguments" />
        /// </summary>
        /// <param name="def">Definition</param>
        /// <param name="loc">Location</param>
        /// <param name="fromBackup">Loaded from Backup</param>
        public PatcherArguments(AssemblyDefinition def, string loc, bool fromBackup)
        {
            _location = loc;
            _assembly = def;
            _fromBackup = fromBackup;
        }
        #endregion
    }

}