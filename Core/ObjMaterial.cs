using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ValveResourceFormat.ResourceTypes;

namespace EffigyMaker.Core
{
    /// <summary>
    /// Represents a material and texture that can be applied to an obj
    /// </summary>
    public class ObjMaterial
    {
        public string Name { get; set; }
        public float SpecularExponent { get; set; } = 10;
        public SKImage TextureImage { get; set; }

        public static ObjMaterial FromVMaterial(Material material, string name, BasicVpkFileLoader vpkLoader)
        {
            var mat = new ObjMaterial();

            mat.Name = name;
            mat.SpecularExponent = material.FloatParams?["g_flSpecularExponent"] ?? 10;

            // actual texture
            var filename = material.TextureParams["g_tColor"];
            var data = vpkLoader.LoadFile(filename + "_c");
            var bitmap = ((Texture)data.DataBlock).GenerateBitmap();
            SKBitmap flippedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                canvas.Scale(1, -1, 0, bitmap.Height / 2);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }
            bitmap = flippedBitmap;
            mat.TextureImage = SKImage.FromBitmap(bitmap);

            return mat;
        }

        /// <summary>
        /// Writes this material and any accompanying textures to a file
        /// </summary>
        /// <param name="path">The file name to write to, without any extensions</param>
        public void WriteToFiles(string path)
        {
            var file = Path.GetFileName(path);

            var mtlText = new List<string>();

            mtlText.Add("# Exported from EffigyMaker");
            mtlText.Add("# Material count 1");
            mtlText.Add("");
            mtlText.Add($"newmtl {Name}");
            mtlText.Add($"ns {SpecularExponent:0.######}");
            mtlText.Add($"Kd 1.000 1.000 1.000"); // mebbe get this and others from file? we do have stuff like "g_vSpecularColor"
            mtlText.Add("Ka 1.000000 1.000000 1.000000");
            mtlText.Add("Kd 0.800000 0.800000 0.800000");
            mtlText.Add("Ks 0.000000 0.000000 0.000000");
            mtlText.Add("Ke 0.000000 0.000000 0.000000");
            mtlText.Add("Ni 1.450000");
            mtlText.Add("d 1.000000");
            mtlText.Add("illum 2");
            mtlText.Add($"map_Kd {file}.png");

            File.WriteAllText(path + ".mtl", string.Join("\n", mtlText));

            File.WriteAllBytes(path + ".png", TextureImage.Encode(SKEncodedImageFormat.Png, 100).ToArray());
        }
    }
}
