using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EffigyMaker.Core
{
    /// <summary>
    /// Represents the mesh for an obj file
    /// </summary>
    public class ObjMesh
    {
        public List<Vector3> Positions { get; set; } = new List<Vector3>();
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
        public List<Vector2> TextureCoords { get; set; } = new List<Vector2>();
        public List<List<ObjFaceVertex>> Faces { get; set; } = new List<List<ObjFaceVertex>>();

        /// <summary>
        /// These aren't exported to the obj file, but we need them for doing animations
        /// THESE ARE INDEXES OF BONES
        /// </summary>
        public List<List<byte>> BlendIndices { get; set; } = new List<List<byte>>();
        public List<Vector4> BlendWeights { get; set; } = new List<Vector4>();

        /// <summary>
        /// The obj mesh as a string, ready to be written to a file
        /// </summary>
        /// <returns>The string to be written to a file</returns>
        public override string ToString()
        {
            var lines = new List<string>();

            lines.Add("# Exported from EffigyMaker");
            lines.Add("o Object.1");

            foreach (var vertex in Positions)
            {
                lines.Add($"v {vertex.X:0.######} {vertex.Y:0.######} {vertex.Z:0.######}");
            }

            foreach (var vertex in Normals)
            {
                lines.Add($"vn {vertex.X:0.######} {vertex.Y:0.######} {vertex.Z:0.######}");
            }

            foreach (var coord in TextureCoords)
            {
                lines.Add($"vt {coord.X:0.######} {coord.Y:0.######}");
            }

            foreach (var face in Faces)
            {
                lines.Add($"f {string.Join(" ", face)}");
            }
            lines.Add("\n"); // Add a few newlines at the end
            return string.Join("\n", lines);
        }
    }
}
