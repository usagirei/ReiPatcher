// --------------------------------------------------
// ReiPatcher - PatcherArguments.cs
// --------------------------------------------------

#region Usings

using System;

using Mono.Cecil;

#endregion

namespace ReiPatcher.Patch
{
    /// <summary>
    ///     Patcher Arguments
    /// </summary>
    public class PatcherArguments : EventArgs
    {
        /// <summary>
        ///     Mono.Cecil <see cref="AssemblyDefinition" />
        /// </summary>
        public AssemblyDefinition Assembly { get; private set; }

        /// <summary>
        ///     Is Backup Assembly
        /// </summary>
        public bool FromBackup { get; private set; }

        /// <summary>
        ///     Assembly Locations
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        ///     Assembly Was Patched
        /// </summary>
        public bool WasPatched { get; set; }

        /// <summary>
        ///     Insantiates a new <see cref="PatcherArguments" />
        /// </summary>
        /// <param name="def">Definition</param>
        /// <param name="loc">Location</param>
        /// <param name="fromBackup">Loaded from Backup</param>
        public PatcherArguments(AssemblyDefinition def, string loc, bool fromBackup)
        {
            Location = loc;
            Assembly = def;
            FromBackup = fromBackup;
            WasPatched = false;
        }
    }
}
