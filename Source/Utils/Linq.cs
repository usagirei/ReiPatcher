// --------------------------------------------------
// ReiPatcher - Linq.cs
// --------------------------------------------------

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ReiPatcher.Utils
{

    /// <summary>
    ///     LINQ Extension
    /// </summary>
    public static class Linq
    {
        #region Public Static Methods
        /// <summary>
        ///     LINQ Foreach Extension
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="function"></param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> function)
        {
            foreach (TSource s in source)
                function(s);
        }

        /// <summary>
        ///     LINQ !Any Static Extension
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        /// <summary>
        ///     LINQ !Any Static Extension
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }
        #endregion
    }

}