using System;
using System.Collections.Generic;
using System.Text;

namespace EffigyMaker.Core
{
    /// <summary>
    /// The data that makes up each vertex in an obj face
    /// </summary>
    public class ObjFaceVertex
    {
        public int? PositionIndex { get; set; }
        public int? NormalIndex { get; set; }
        public int? TextureCoordinateIndex { get; set; }

        /// <summary>
        /// The face as a string to be put in an obj file
        /// </summary>
        /// <returns>The constructed obj string</returns>
        public override string ToString()
        {
            var result = "";

            result += PositionIndex + 1;
            if (TextureCoordinateIndex.HasValue)
            {
                result += $"/{TextureCoordinateIndex + 1}";
            }
            if (NormalIndex.HasValue)
            {
                if (!TextureCoordinateIndex.HasValue)
                {
                    result += "/";
                }
                result += $"/{NormalIndex + 1}";
            }

            return result;
        }
    }
}
