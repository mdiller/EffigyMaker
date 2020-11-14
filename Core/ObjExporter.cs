using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.Blocks;
using ValveResourceFormat.ResourceTypes.ModelAnimation;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.IO;

namespace EffigyMaker.Core
{
    using VMaterial = ValveResourceFormat.ResourceTypes.Material;
    using VMesh = ValveResourceFormat.ResourceTypes.Mesh;
    using VModel = ValveResourceFormat.ResourceTypes.Model;

    /// <summary>
    /// For exporting Obj files
    /// This class heavily inspired by https://github.com/SteamDatabase/ValveResourceFormat/blob/master/ValveResourceFormat/IO/GltfModelExporter.cs
    /// </summary>
    public class ObjExporter
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="fileLoader">The fileloader for the vpk</param>
        public ObjExporter(IFileLoader fileLoader)
        {
            FileLoader = fileLoader;
        }

        public IFileLoader FileLoader { get; set; }

        /// <summary>
        /// Exports the specified model as an obj file with a texture
        /// </summary>
        /// <param name="vpkPath">The path to the dota vpk</param>
        /// <param name="modelPath">The path within the vpk to the model we want to export</param>
        public void ExportModelAsObj(string modelPath)
        {
            // exporter.ExportToFile(file.GetFileName(), outputFile, (Model)resource.DataBlock);
            var resource = FileLoader.LoadFile(modelPath);

            if (resource.ResourceType != ResourceType.Model)
            {
                throw new ArgumentException("Passed in path must be a path to a model");
            }

            var model = (VModel)resource.DataBlock;

            // Add meshes and their skeletons
            var meshes = LoadModelMeshes(model);
            // TODO: mebbe figure out what to do with the other meshes/skeletons (lod?) in the future
            var mesh = meshes[0];
            var skeleton = model.GetSkeleton(0);


        }

        /// <summary>
        /// (Method directly copied from https://github.com/SteamDatabase/ValveResourceFormat/blob/master/ValveResourceFormat/IO/GltfModelExporter.cs)
        /// Create a combined list of referenced and embedded meshes. Importantly retains the
        /// refMeshes order so it can be used for getting skeletons.
        /// </summary>
        /// <param name="model">The model to get the meshes from.</param>
        /// <returns>A tuple of meshes and their names.</returns>
        private (VMesh Mesh, string Name)[] LoadModelMeshes(VModel model)
        {
            var refMeshes = model.GetRefMeshes().ToArray();
            var meshes = new (VMesh, string)[refMeshes.Length];

            var embeddedMeshIndex = 0;
            var embeddedMeshes = model.GetEmbeddedMeshes().ToArray();

            for (var i = 0; i < meshes.Length; i++)
            {
                var meshReference = refMeshes[i];
                if (meshReference == null)
                {
                    // If refmesh is null, take an embedded mesh
                    meshes[i] = (embeddedMeshes[embeddedMeshIndex++], $"Embedded Mesh {embeddedMeshIndex}");
                }
                else
                {
                    // Load mesh from file
                    var meshResource = FileLoader.LoadFile(meshReference + "_c");
                    if (meshResource == null)
                    {
                        continue;
                    }

                    var nodeName = Path.GetFileNameWithoutExtension(meshReference);
                    var mesh = new VMesh(meshResource);
                    meshes[i] = (mesh, nodeName);
                }
            }

            return meshes;
        }
    }
}
