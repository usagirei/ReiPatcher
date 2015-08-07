// --------------------------------------------------
// ReiPatcher - PatchBase.cs
// --------------------------------------------------

#region Usings
using Mono.Cecil;

using ReiPatcher.Utils;

#endregion

namespace ReiPatcher.Patch
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
        public virtual string Name => GetType().Assembly.GetName().Name;

        /// <summary>
        ///     Patch Version
        /// </summary>
        public virtual string Version => GetType().Assembly.GetName().Version.ToString();

        /// <summary>
        ///     Assemblies Directory
        /// </summary>
        public string AssembliesDir => Program.AssembliesDir;
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
        ///     Gets a <see cref="AssemblyDefinition" /> <see cref="PatchedAttribute" />s
        /// </summary>
        /// <param name="definition"></param>
        public PatchedAttribute[] GetPatchedAttributes(AssemblyDefinition definition)
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

        /// <summary>
        ///     Sets a <see cref="AssemblyDefinition" /> <see cref="PatchedAttribute" />
        /// </summary>
        /// <param name="assembly">Member</param>
        /// <param name="info">Tag</param>
        public void SetPatchedAttribute(AssemblyDefinition assembly, string info)
        {
            AttributeUtil.SetPatchedAttribute(assembly, info);
        }
        #endregion
    }

}