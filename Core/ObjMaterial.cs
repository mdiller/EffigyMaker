using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Clones the current ObjMaterial
        /// </summary>
        /// <returns>The cloned objMaterial</returns>
        public ObjMaterial Clone()
        {
            var mat = new ObjMaterial();
            mat.Name = Name;
            mat.SpecularExponent = SpecularExponent;
            mat.TextureImage = TextureImage;
            return mat;
        }

        /// <summary>
        /// Creates an ObjMaterial from a valve material
        /// </summary>
        /// <param name="material">The material to create from</param>
        /// <param name="name">The name of this new material</param>
        /// <param name="vpkLoader">A vpk file loader</param>
        /// <returns>The created ObjMaterial</returns>
        public static ObjMaterial FromVMaterial(Material material, string name, BasicVpkFileLoader vpkLoader)
        {
            var mat = new ObjMaterial();

            mat.Name = name;
            mat.SpecularExponent = material.FloatParams?["g_flSpecularExponent"] ?? 10;

            // actual texture
            var filename = material.TextureParams["g_tColor"];
            var data = vpkLoader.LoadFile(filename + "_c");
            var bitmap = ((Texture)data.DataBlock).GenerateBitmap();
            mat.TextureImage = SKImage.FromBitmap(bitmap.FlipVertically());

            return mat;
        }


        /// <summary>
        /// Stacks the list of images vertically, stretching them to all have the same width
        /// Note that we stack them bottom-up (by reversing image order), because uv coordinate system has v=0 starting at the bottom
        /// </summary>
        /// <param name="images">The images to stack</param>
        /// <returns>The stacked images</returns>
        public static SKImage StackImages(List<SKImage> images)
        {
            images.Reverse();
            SKBitmap newBitmap = new SKBitmap(images.Max(i => i.Width), images.Select(i => i.Height).Aggregate((h1, h2) => h1 + h2));
            using (SKCanvas canvas = new SKCanvas(newBitmap))
            {
                canvas.Clear();
                var h = 0;
                foreach (var img in images)
                {
                    var bitmap = SKBitmap.FromImage(img).Resize(new SKSizeI(newBitmap.Width, img.Height), SKFilterQuality.High);
                    canvas.DrawBitmap(bitmap, new SKPoint(0, h));
                    h += img.Height;
                }
            }
            return SKImage.FromBitmap(newBitmap);
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
