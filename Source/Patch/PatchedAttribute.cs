// --------------------------------------------------
// ReiPatcher - PatchedAttribute.cs
// --------------------------------------------------

#region Usings

using System;

#endregion

namespace ReiPatcher.Patch
{
    /// <summary>
    ///     <see cref="PatchedAttribute" /> Attribute
    /// </summary>
    [AttributeUsage(ATTRIBUTE_TARGETS, AllowMultiple = true)]
    public class PatchedAttribute : Attribute
    {
        private const AttributeTargets ATTRIBUTE_TARGETS = AttributeTargets.All;

        /// <summary>
        ///     Information
        /// </summary>
        public string Info;

        /// <summary>
        ///     Instantiates a PatcherAttribute
        /// </summary>
        /// <param name="info"></param>
        public PatchedAttribute(string info)
        {
            Info = info;
        }
    }
}
