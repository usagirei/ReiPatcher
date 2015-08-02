// --------------------------------------------------
// ReiPatcher - Program.Patchers.cs
// --------------------------------------------------

#region Usings

using System;
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
            string temp;
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
                        temp = string.Format
                            ("Error in Patcher '{0}' at Assembly '{1}'", patcher.Name, def.Assembly.FullName);
                        ConsoleUtil.Print(temp, color: ConsoleColor.Red);

                        temp = string.Format("  {0}", ex.Message);
                        ConsoleUtil.Print(temp, color: ConsoleColor.Red);
                        Kill(ExitCode.NoPatchesApplied);
                    }
                }
                ConsoleUtil.Print("", Console.BufferWidth, true, '-', ConsoleColor.DarkGray);
            }
        }

        private static void RunPostPatch(PatchBase[] patchers)
        {
            string temp;
            patchers.ForEach
                (
                    patcher =>
                    {
                        try
                        {
                            Console.WriteLine("Post-Patch '{0}'", patcher.Name);
                            patcher.PostPatch();
                        }
                        catch (Exception ex)
                        {
                            temp = string.Format("Error in Patcher '{0}'", patcher.Name);
                            ConsoleUtil.Print(temp, color: ConsoleColor.Red);
                            temp = string.Format("  {0}", ex.Message);
                            ConsoleUtil.Print(temp, color: ConsoleColor.Red);
                            Kill(ExitCode.NoPatchesApplied);
                        }
                    });
        }

        private static void RunPrePatch(PatchBase[] patchers)
        {
            string temp;
            patchers.ForEach
                (
                    patcher =>
                    {
                        try
                        {
                            Console.WriteLine("Pre-Patch '{0}'", patcher.Name);
                            patcher.PrePatch();
                        }
                        catch (Exception ex)
                        {
                            temp = string.Format("Error in Patcher '{0}'", patcher.Name);
                            ConsoleUtil.Print(temp, color: ConsoleColor.Red);
                            temp = string.Format("  {0}", ex.Message);
                            ConsoleUtil.Print(temp, color: ConsoleColor.Red);
                            Kill(ExitCode.NoPatchesApplied);
                        }
                    });
        }

        #endregion
    }
}