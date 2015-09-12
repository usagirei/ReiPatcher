// --------------------------------------------------
// ReiPatcher - ConsoleUtil.cs
// --------------------------------------------------

#region Usings

using System;

#endregion

namespace ReiPatcher.Utils
{
    internal static class ConsoleUtil
    {
        public static void Print(
            string text,
            int width = -1,
            bool center = false,
            char pad = ' ',
            ConsoleColor color = ConsoleColor.Gray)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;

            width = width == -1
                        ? text.Length
                        : width;

            string padded = text.PadCenter(width, pad);

            Console.CursorLeft = center
                                     ? (Console.BufferWidth - padded.Length) / 2
                                     : 0;

            if (padded.Length % Console.BufferWidth == 0)
                Console.Write(padded);
            else
                Console.WriteLine(padded);

            Console.ForegroundColor = old;
        }
    }
}
