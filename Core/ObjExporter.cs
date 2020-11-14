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
    using static ValveResourceFormat.Blocks.VBIB;
    using VMaterial = ValveResourceFormat.ResourceTypes.Material;
    using VMesh = ValveResourceFormat.ResourceTypes.Mesh;
    using VModel = ValveResourceFormat.ResourceTypes.Model;

    /// <summary>
    /// For exporting Obj files
    /// This class heavily inspired by https://github.com/SteamDatabase/ValveResourceFormat/blob/master/ValveResourceFormat/IO/GltfModelExporter.cs
    /// </summary>
    public class ObjExporter
    {
        // NOTE: Swaps Y and Z axes - (source engine up is Z, usually Y is up)
        // Also divides by 100. Source units are real big
        // https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#coordinate-system-and-units
        private readonly Matrix4x4 TRANSFORMSOURCETOSTANDARD = Matrix4x4.CreateScale(0.0254f) * Matrix4x4.CreateFromYawPitchRoll(0, (float)Math.PI / -2f, 0);

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
            var mesh = meshes[0].Mesh;
            var skeleton = model.GetSkeleton(0);

            var objMesh = new ObjMesh();

            var data = mesh.GetData();
            var vbib = mesh.VBIB;

            foreach (var sceneObject in data.GetArray("m_sceneObjects"))
            {
                foreach (var drawCall in sceneObject.GetArray("m_drawCalls"))
                {
                    var startingVertexCount = objMesh.Positions.Count;
                    var vertexBufferInfo = drawCall.GetArray("m_vertexBuffers")[0]; // In what situation can we have more than 1 vertex buffer per draw call?
                    var vertexBufferIndex = (int)vertexBufferInfo.GetIntegerProperty("m_hBuffer");
                    var vertexBuffer = vbib.VertexBuffers[vertexBufferIndex];

                    var indexBufferInfo = drawCall.GetSubCollection("m_indexBuffer");
                    var indexBufferIndex = (int)indexBufferInfo.GetIntegerProperty("m_hBuffer");
                    var indexBuffer = vbib.IndexBuffers[indexBufferIndex];

                    // Set vertex attributes
                    foreach (var attribute in vertexBuffer.Attributes)
                    {
                        var buffer = ReadAttributeBuffer(vertexBuffer, attribute);
                        var numComponents = buffer.Length / vertexBuffer.Count;

                        if (attribute.Name == "POSITION")
                        {
                            var vectors = ToVector3Array(buffer);
                            objMesh.Positions.AddRange(vectors);
                        }

                        if (attribute.Name == "NORMAL")
                        {
                            if (VMesh.IsCompressedNormalTangent(drawCall))
                            {
                                var vectors = ToVector4Array(buffer);
                                var (normals, tangents) = DecompressNormalTangents(vectors);
                                objMesh.Normals.AddRange(normals);
                            }
                            else
                            {
                                var vectors = ToVector3Array(buffer);
                                objMesh.Normals.AddRange(vectors);
                            }
                            continue;
                        }

                        if (attribute.Name == "TEXCOORD")
                        {
                            if (numComponents != 2)
                            {
                                // ignore textcoords that arent 2
                                continue;
                            }
                            var vectors = ToVector2Array(buffer);
                            objMesh.TextureCoords.AddRange(vectors);
                        }
                    }

                    // Set index buffer
                    var startIndex = (int)drawCall.GetIntegerProperty("m_nStartIndex");
                    var indexCount = (int)drawCall.GetIntegerProperty("m_nIndexCount");
                    var indices = ReadIndices(indexBuffer, startIndex, indexCount);
                    var newFaces = indices.ChunkBy(3).Select(idxList => idxList.Select(idx => new ObjFaceVertex(idx + startingVertexCount)).ToList()).ToList();
                    objMesh.Faces.AddRange(newFaces);

                    // TODO: do stuff with materials here
                    var materialPath = drawCall.GetProperty<string>("m_material");
                    var materialResource = FileLoader.LoadFile(materialPath + "_c");
                    var renderMaterial = (VMaterial)materialResource.DataBlock;
                }
            }

            objMesh.Positions = objMesh.Positions.Select(v => v = Vector3.Transform(v, TRANSFORMSOURCETOSTANDARD)).ToList();
            objMesh.Normals = objMesh.Normals.Select(v => v = Vector3.TransformNormal(v, TRANSFORMSOURCETOSTANDARD)).ToList();

            var val = objMesh.Faces.Select(f => f.Max(d => d.PositionIndex)).Max();

            File.WriteAllText("out.obj", objMesh.ToString());
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


        private static float[] ReadAttributeBuffer(VertexBuffer buffer, VertexAttribute attribute)
            => Enumerable.Range(0, (int)buffer.Count)
                .SelectMany(i => VBIB.ReadVertexAttribute(i, buffer, attribute))
                .ToArray();

        private static int[] ReadIndices(IndexBuffer indexBuffer, int start, int count)
        {
            var indices = new int[count];

            var byteCount = count * (int)indexBuffer.Size;
            var byteStart = start * (int)indexBuffer.Size;

            if (indexBuffer.Size == 4)
            {
                System.Buffer.BlockCopy(indexBuffer.Buffer, byteStart, indices, 0, byteCount);
            }
            else if (indexBuffer.Size == 2)
            {
                var shortIndices = new ushort[count];
                System.Buffer.BlockCopy(indexBuffer.Buffer, byteStart, shortIndices, 0, byteCount);
                indices = Array.ConvertAll(shortIndices, i => (int)i);
            }

            return indices;
        }

        private static (Vector3[] Normals, Vector4[] Tangents) DecompressNormalTangents(Vector4[] compressedNormalsTangents)
        {
            var normals = new Vector3[compressedNormalsTangents.Length];
            var tangents = new Vector4[compressedNormalsTangents.Length];

            for (var i = 0; i < normals.Length; i++)
            {
                // Undo-normalization
                var compressedNormal = compressedNormalsTangents[i] * 255f;
                var decompressedNormal = DecompressNormal(new Vector2(compressedNormal.X, compressedNormal.Y));
                var decompressedTangent = DecompressTangent(new Vector2(compressedNormal.Z, compressedNormal.W));

                // Swap Y and Z axes
                normals[i] = new Vector3(decompressedNormal.X, decompressedNormal.Z, decompressedNormal.Y);
                tangents[i] = new Vector4(decompressedTangent.X, decompressedTangent.Z, decompressedTangent.Y, decompressedTangent.W);
            }

            return (normals, tangents);
        }

        private static Vector3 DecompressNormal(Vector2 compressedNormal)
        {
            var inputNormal = compressedNormal;
            var outputNormal = Vector3.Zero;

            float x = inputNormal.X - 128.0f;
            float y = inputNormal.Y - 128.0f;
            float z;

            float zSignBit = x < 0 ? 1.0f : 0.0f;           // z and t negative bits (like slt asm instruction)
            float tSignBit = y < 0 ? 1.0f : 0.0f;
            float zSign = -((2 * zSignBit) - 1);          // z and t signs
            float tSign = -((2 * tSignBit) - 1);

            x = (x * zSign) - zSignBit;                           // 0..127
            y = (y * tSign) - tSignBit;
            x = x - 64;                                     // -64..63
            y = y - 64;

            float xSignBit = x < 0 ? 1.0f : 0.0f;   // x and y negative bits (like slt asm instruction)
            float ySignBit = y < 0 ? 1.0f : 0.0f;
            float xSign = -((2 * xSignBit) - 1);          // x and y signs
            float ySign = -((2 * ySignBit) - 1);

            x = ((x * xSign) - xSignBit) / 63.0f;             // 0..1 range
            y = ((y * ySign) - ySignBit) / 63.0f;
            z = 1.0f - x - y;

            float oolen = 1.0f / (float)Math.Sqrt((x * x) + (y * y) + (z * z));   // Normalize and
            x *= oolen * xSign;                 // Recover signs
            y *= oolen * ySign;
            z *= oolen * zSign;

            outputNormal.X = x;
            outputNormal.Y = y;
            outputNormal.Z = z;

            return outputNormal;
        }

        private static Vector4 DecompressTangent(Vector2 compressedTangent)
        {
            var outputNormal = DecompressNormal(compressedTangent);
            var tSign = compressedTangent.Y - 128.0f < 0 ? -1.0f : 1.0f;

            return new Vector4(outputNormal.X, outputNormal.Y, outputNormal.Z, tSign);
        }

        private static Vector3[] ToVector3Array(float[] buffer)
        {
            var vectorArray = new Vector3[buffer.Length / 3];

            for (var i = 0; i < vectorArray.Length; i++)
            {
                vectorArray[i] = new Vector3(buffer[i * 3], buffer[(i * 3) + 1], buffer[(i * 3) + 2]);
            }

            return vectorArray;
        }

        private static Vector2[] ToVector2Array(float[] buffer)
        {
            var vectorArray = new Vector2[buffer.Length / 2];

            for (var i = 0; i < vectorArray.Length; i++)
            {
                vectorArray[i] = new Vector2(buffer[i * 2], buffer[(i * 2) + 1]);
            }

            return vectorArray;
        }

        private static Vector4[] ToVector4Array(float[] buffer)
        {
            var vectorArray = new Vector4[buffer.Length / 4];

            for (var i = 0; i < vectorArray.Length; i++)
            {
                vectorArray[i] = new Vector4(buffer[i * 4], buffer[(i * 4) + 1], buffer[(i * 4) + 2], buffer[(i * 4) + 3]);
            }

            return vectorArray;
        }

        // https://github.com/KhronosGroup/glTF-Validator/blob/master/lib/src/errors.dart
        private const float UnitLengthThresholdVec3 = 0.00674f;

        private static Vector4[] FixZeroLengthVectors(Vector4[] vectorArray)
        {
            for (var i = 0; i < vectorArray.Length; i++)
            {
                var vec = vectorArray[i];

                if (Math.Abs(new Vector3(vec.X, vec.Y, vec.Z).Length() - 1.0f) > UnitLengthThresholdVec3)
                {
                    vectorArray[i] = -Vector4.UnitZ;
                    vectorArray[i].W = vec.W;

                    Console.Error.WriteLine($"The exported model contains a non-zero unit vector which was replaced with {vectorArray[i]} for exporting purposes.");
                }
            }

            return vectorArray;
        }

        private static Vector3[] FixZeroLengthVectors(Vector3[] vectorArray)
        {
            for (var i = 0; i < vectorArray.Length; i++)
            {
                if (Math.Abs(vectorArray[i].Length() - 1.0f) > UnitLengthThresholdVec3)
                {
                    vectorArray[i] = -Vector3.UnitZ;

                    Console.Error.WriteLine($"The exported model contains a non-zero unit vector which was replaced with {vectorArray[i]} for exporting purposes.");
                }
            }

            return vectorArray;
        }
    }
}
