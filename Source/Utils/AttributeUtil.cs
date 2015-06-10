// --------------------------------------------------
// ReiPatcher - AttributeUtil.cs
// --------------------------------------------------

#region Usings
using System;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

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
            TypeReference strType = assembly.MainModule.Import(typeof (String));
            TypeReference objType = assembly.MainModule.Import(typeof (object));

            ConstructorInfo ctor = typeof (PatchedAttribute).GetConstructor(new[] {typeof (string)});
            MethodReference @ref = assembly.MainModule.Import(ctor);

            CustomAttribute cAttr = new CustomAttribute(@ref);
            cAttr.ConstructorArguments.Add(new CustomAttributeArgument(strType, info));

            assembly.CustomAttributes.Add(cAttr);
        }

        internal static void SetPatchedAttribute(ModuleDefinition module, IMemberDefinition member, string info)
        {
            TypeReference strType = module.Import(typeof (String));
            //TypeReference objType = module.Import(typeof (object));

            ConstructorInfo ctor = typeof (PatchedAttribute).GetConstructor(new[] {typeof (string)});
            MethodReference @ref = module.Import(ctor);

            CustomAttribute cAttr = new CustomAttribute(@ref);
            cAttr.ConstructorArguments.Add(new CustomAttributeArgument(strType, info));

            member.CustomAttributes.Add(cAttr);
        }
        #endregion
    }

}