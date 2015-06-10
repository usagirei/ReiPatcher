// --------------------------------------------------
// ReiPatcher - ReiPatcherAttribute.cs
// --------------------------------------------------

#region Usings
using System;

#endregion

namespace ReiPatcher
{

    /// <summary>
    ///     PatcherAttribute
    /// </summary>
    [AttributeUsage(ATTRIBUTE_TARGETS, AllowMultiple = true)]
    public class ReiPatcherAttribute : Attribute
    {
        #region Constants
        private const AttributeTargets ATTRIBUTE_TARGETS = AttributeTargets.All;
        #endregion

        #region Fields
        /// <summary>
        ///     Information
        /// </summary>
        public string Info;
        #endregion

        #region (De)Constructors
        /// <summary>
        ///     Instantiates a PatcherAttribute
        /// </summary>
        /// <param name="info"></param>
        public ReiPatcherAttribute(string info)
        {
            Info = info;
        }
        #endregion
    }

}