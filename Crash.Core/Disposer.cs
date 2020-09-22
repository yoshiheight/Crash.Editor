using System;
using System.Collections.Generic;

namespace Crash.Core
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Disposer : IDisposable
    {
        /// <summary></summary>
        private readonly List<IDisposable?> _list = new List<IDisposable?>();

        /// <summary>
        /// 
        /// </summary>
        public void Add<T>(T? obj)
            where T : class, IDisposable
        {
            _list.Add(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            DisposeAll(_list);
            _list.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Dispose<T>(T? obj)
            where T : class, IDisposable
        {
            obj?.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void DisposeAll<T>(IEnumerable<T?> collection)
            where T : class, IDisposable
        {
            foreach (var obj in collection)
            {
                obj?.Dispose();
            }
        }
    }
}
