// --------------------------------------------------
// ReiPatcher - Program.Patchers.cs
// --------------------------------------------------

#region Usings
using System;
using System.Runtime.CompilerServices;

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
            foreach (PatcherArguments def in assemblies)
            {
                foreach (PatchBase patcher in patchers)
                {
                    try
                    {
                        if (patcher.CanPatch(def))
                        {
                            Console.WriteLine("Patching '{0}'", patcher.Name);
                            patcher.Patch(def);
                        }
                        else
                        {
                            temp = string.Format("Skipping '{0}'", patcher.Name);
                            ConsoleUtil.Print(temp, color: ConsoleColor.DarkGray);
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
            }
        }

        private static void RunPostPatch(PatchBase[] patchers)
        {
            string temp;
            patchers.ForEach
                (patcher =>
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
                (patcher =>
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