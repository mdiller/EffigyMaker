using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EffigyMaker.Core
{
    public static class ExtensionUtils
    {
        public static List<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .ToList()
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
