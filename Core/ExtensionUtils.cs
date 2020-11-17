using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EffigyMaker.Core
{
    public static class ExtensionUtils
    {
        /// <summary>
        /// Splits the given enumberable into chunks of the given size
        /// </summary>
        /// <typeparam name="T">The type of the enumerable</typeparam>
        /// <param name="source">The enumerable to chunk</param>
        /// <param name="chunkSize">The size of chunks to create</param>
        /// <returns>The chunks as a list of lists</returns>
        public static List<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .ToList()
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// Flips the given bitmap vertically
        /// </summary>
        /// <param name="bitmap">The bitmap to flip</param>
        /// <returns>The flipped bitmap</returns>
        public static SKBitmap FlipVertically(this SKBitmap bitmap)
        {
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas canvas = new SKCanvas(newBitmap))
            {
                canvas.Clear();
                canvas.Scale(1, -1, 0, bitmap.Height / 2);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }
            return newBitmap;
        }

        ///// <summary>
        ///// Resizes the given bitmap
        ///// </summary>
        ///// <param name="bitmap">The bitmap to resize</param>
        ///// <returns>The resized bitmap</returns>
        //public static SKBitmap Resize(this SKBitmap bitmap, int newWidth, int newHeight)
        //{
        //    SKBitmap newBitmap = new SKBitmap(newWidth, newHeight);
        //    using (SKCanvas canvas = new SKCanvas(newBitmap))
        //    {
        //        canvas.Clear();
        //        canvas.Scale(1, -1, 0, bitmap.Height / 2);
        //        canvas.DrawBitmap(bitmap, new SKPoint());
        //    }
        //    return newBitmap;
        //}
    }
}
