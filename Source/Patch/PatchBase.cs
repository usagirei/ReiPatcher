// --------------------------------------------------
// ReiPatcher - PatchBase.cs
// --------------------------------------------------

#region Usings
using Mono.Cecil;

using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{

    /// <summary>
    ///     Base Class for Patches
    /// </summary>
    public abstract partial class PatchBase
    {
        #region Properties
        /// <summary>
        ///     Patch Name
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        ///     Patch Version
        /// </summary>
        public abstract string Version { get; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Determines if a <see cref="PatchBase" /> can Patch an <see cref="AssemblyDefinition" />
        /// </summary>
        /// <param name="args">Assembly</param>
        /// <returns></returns>
        public abstract bool CanPatch(PatcherArguments args);

        /// <summary>
        ///     Patches an <see cref="AssemblyDefinition" />
        /// </summary>
        /// <param name="args">Assembly</param>
        public abstract void Patch(PatcherArguments args);

        /// <summary>
        ///     Post Patching (After Save) Operations
        /// </summary>
        public virtual void PostPatch() {}

        /// <summary>
        ///     Pre Patching (After Loading) Operations
        /// </summary>
        public virtual void PrePatch() {}
        #endregion
    }

    partial class PatchBase
    {
        #region Public Methods
        /// <summary>
        ///     Gets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />s
        /// </summary>
        /// <param name="definition"></param>
        public PatchedAttribute[] GetPatchedAttributes(IMemberDefinition definition)
        {
            return AttributeUtil.GetPatchedAttributes(definition);
        }

        /// <summary>
        ///     Sets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(PropertyDefinition member, string info)
        {
            AttributeUtil.SetPatchedAttribute(member.Module, member, info);
        }

        /// <summary>
        ///     Sets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(MethodDefinition member, string info)
        {
            AttributeUtil.SetPatchedAttribute(member.Module, member, info);
        }

        /// <summary>
        ///     Sets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(TypeDefinition member, string info)
        {
            AttributeUtil.SetPatchedAttribute(member.Module, member, info);
        }

        /// <summary>
        ///     Sets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(FieldDefinition member, string info)
        {
            AttributeUtil.SetPatchedAttribute(member.Module, member, info);
        }

        /// <summary>
        ///     Sets a <see cref="IMemberDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(EventDefinition member, string info)
        {
            AttributeUtil.SetPatchedAttribute(member.Module, member, info);
        }
        #endregion
    }

}