// --------------------------------------------------
// ReiPatcher - Program.Patchers.cs
// --------------------------------------------------

#region Usings

using System;
using System.IO;
using ReiPatcher.Patch;
using ReiPatcher.Utils;

#endregion

namespace ReiPatcher
{
    partial class Program
    {
        #region Static Methods

        private static void RunPatchers(PatchBase[] patchers, params PatcherArguments[] assemblies)
        {
            foreach (var def in assemblies)
            {
                ConsoleUtil.Print(
                    $"Assembly '{def.Assembly.Name.Name}'",
                    Console.BufferWidth,
                    true,
                    '-',
                    ConsoleColor.DarkGreen);
                foreach (var patcher in patchers)
                {
                    ConsoleUtil.Print(
                        $"Patcher: '{patcher.Name}'",
                        Console.BufferWidth,
                        true,
                        '-',
                        ConsoleColor.DarkGray);
                    try
                    {
                        if (patcher.CanPatch(def))
                        {
                            ConsoleUtil.Print($"Patching '{patcher.Name}'", color: ConsoleColor.DarkGreen);
                            patcher.Patch(def);
                            def.WasPatched = true;
                        }
                        else
                        {
                            ConsoleUtil.Print($"Skipping '{patcher.Name}'", color: ConsoleColor.DarkYellow);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtil.Print(
                            $"Error in Patcher '{patcher.Name}' at Assembly '{def.Assembly.FullName}'",
                            color: ConsoleColor.Red);
                        ConsoleUtil.Print(
                            $"  {ex.Message}",
                            color: ConsoleColor.Red);
                        Kill(ExitCode.NoPatchesApplied);
                    }
                }
                ConsoleUtil.Print("", Console.BufferWidth, true, '-', ConsoleColor.DarkGray);
            }
        }

        private static void RunPostPatch(PatchBase[] patchers)
        {
            foreach (var patcher in patchers)
            {
                try
                {
                    Console.WriteLine("Post-Patch '{0}'", patcher.Name);
                    patcher.PostPatch();
                }
                catch (Exception ex)
                {
                    ConsoleUtil.Print(
                        $"Error in Patcher '{patcher.Name}'",
                        color: ConsoleColor.Red);
                    ConsoleUtil.Print(
                        $"  {ex.Message}",
                        color: ConsoleColor.Red);
                    Kill(ExitCode.NoPatchesApplied);
                }
            }
        }

        private static void RunPrePatch(PatchBase[] patchers)
        {
            foreach (var patcher in patchers)
            {
                try
                {
                    Console.WriteLine("Pre-Patch '{0}'", patcher.Name);
                    patcher.PrePatch();
                }
                catch (Exception ex)
                {
                    ConsoleUtil.Print(
                        $"Error in Patcher '{patcher.Name}'",
                        color: ConsoleColor.Red);
                    ConsoleUtil.Print(
                        $"  {ex.Message}",
                        color: ConsoleColor.Red);
                    Kill(ExitCode.NoPatchesApplied);
                }
            }
        }

        private static void RunSaveAndBackup(PatcherArguments[] assemblies)
        {
            foreach (var ass in assemblies)
            {
                if (!ass.FromBackup && !ass.WasPatched)
                {
                    ConsoleUtil.Print
                        ($"Not Patched '{Path.GetFileName(ass.Location)}'", color: ConsoleColor.DarkYellow);
                    continue;
                }
                var attrs = AttributeUtil.GetPatchedAttributes(ass.Assembly);
                if (!ass.FromBackup && attrs.None(attribute => attribute.Info == "ReiPatcher"))
                {
                    var destFileName = string.Format(
                        "{0}.{1:" + BACKUP_DATE_FORMAT + "}.bak",
                        ass.Location,
                        DateTime.Now);

                    ConsoleUtil.Print
                        (
                            $"Creating Assembly Backup: '{Path.GetFileName(destFileName)}'",
                            color: ConsoleColor.DarkYellow);

                    File.Copy(ass.Location, destFileName);
                }
                if (attrs.None())
                    AttributeUtil.SetPatchedAttribute(ass.Assembly, "ReiPatcher");

                ConsoleUtil.Print($"Saving '{Path.GetFileName(ass.Location)}'", color: ConsoleColor.DarkGreen);
                ass.Assembly.Write(ass.Location);
            }
        }
        #endregion
    }
}
