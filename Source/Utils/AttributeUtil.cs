// --------------------------------------------------
// ReiPatcher - AttributeUtil.cs
// --------------------------------------------------

#region Usings
using System;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using ReiPatcher.Patch;

#endregion

namespace ReiPatcher.Utils
{

    internal static class AttributeUtil
    {
        #region Fields
        private static readonly PatchedAttribute[] NoAttributes = new PatchedAttribute[0];
        #endregion

        #region Static Methods
        internal static PatchedAttribute[] GetPatchedAttributes(IMemberDefinition definition)
        {
            if (!definition.HasCustomAttributes)
                return NoAttributes;
            var attrs = definition.CustomAttributes.Where
                (def => def.AttributeType.Name == typeof (PatchedAttribute).Name)
                                  .ToList();

            var rps = attrs.Select(at => new PatchedAttribute(at.ConstructorArguments[0].Value as string));

            return attrs.Any()
                ? rps.ToArray()
                : NoAttributes;
        }

        internal static PatchedAttribute[] GetPatchedAttributes(AssemblyDefinition assembly)
        {
            if (!assembly.HasCustomAttributes)
                return NoAttributes;
            var attrs = assembly.CustomAttributes.Where
                (def => def.AttributeType.Name == typeof (PatchedAttribute).Name)
                                .ToList();

            var rps = attrs.Select(at => new PatchedAttribute(at.ConstructorArguments[0].Value as string));

            return attrs.Any()
                ? rps.ToArray()
                : NoAttributes;
        }

        internal static void SetPatchedAttribute(AssemblyDefinition assembly, string info)
        {
            var strType = assembly.MainModule.Import(typeof (String));
            var objType = assembly.MainModule.Import(typeof (object));

            var ctor = typeof (PatchedAttribute).GetConstructor(new[] {typeof (string)});
            var @ref = assembly.MainModule.Import(ctor);

            var cAttr = new CustomAttribute(@ref);
            cAttr.ConstructorArguments.Add(new CustomAttributeArgument(strType, info));

            assembly.CustomAttributes.Add(cAttr);
        }

        internal static void SetPatchedAttribute(ModuleDefinition module, IMemberDefinition member, string info)
        {
            var strType = module.Import(typeof (string));
            //TypeReference objType = module.Import(typeof (object));

            var ctor = typeof (PatchedAttribute).GetConstructor(new[] {typeof (string)});
            var @ref = module.Import(ctor);

            var cAttr = new CustomAttribute(@ref);
            cAttr.ConstructorArguments.Add(new CustomAttributeArgument(strType, info));

            member.CustomAttributes.Add(cAttr);
        }
        #endregion
    }

}