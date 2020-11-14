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
        /// The obj mesh as a string, ready to be written to a file
        /// </summary>
        /// <returns>The string to be written to a file</returns>
        public override string ToString()
        {
            var lines = new List<string>();

            foreach (var vertex in Positions)
            {
                lines.Add($"v {vertex.X} {vertex.Y} {vertex.Z}");
            }

            foreach (var vertex in Normals)
            {
                lines.Add($"vn {vertex.X} {vertex.Y} {vertex.Z}");
            }

            foreach (var coord in TextureCoords)
            {
                lines.Add($"vt {coord.X} {coord.Y}");
            }

            foreach (var face in Faces)
            {
                lines.Add($"f {string.Join(" ", face)}");
            }

            return string.Join("\n", lines);
        }
    }
}
