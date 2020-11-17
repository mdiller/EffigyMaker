using System;
using System.Collections.Generic;
using System.IO;
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

        public int VertexCount => Positions.Count;

        /// <summary>
        /// These aren't exported to the obj file, but we need them for doing animations
        /// </summary>
        public List<List<byte>> BlendIndices { get; set; } = new List<List<byte>>();
        public List<Vector4> BlendWeights { get; set; } = new List<Vector4>();

        public ObjMaterial Material { get; set; }

        /// <summary>
        /// Writes the obj file and any accompanying files to the given location
        /// </summary>
        /// <param name="path">The file name to write to, without any extensions</param>
        public void WriteToFiles(string path)
        {
            var file = Path.GetFileName(path);

            var lines = new List<string>();

            lines.Add("# Exported from EffigyMaker");
            lines.Add($"mtllib {file}.mtl");
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

            lines.Add($"usemtl {Material.Name}");
            foreach (var face in Faces)
            {
                lines.Add($"f {string.Join(" ", face)}");
            }

            File.WriteAllText(path + ".obj", string.Join("\n", lines));

            Material.WriteToFiles(path);
        }
    }
}
