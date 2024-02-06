using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

public static class MeshingUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyMesh(ref Mesh.MeshDataArray meshDataArray, ref NativeArray<uint> indices, ref NativeArray<float3> vertices, ref NativeArray<float3> normals, ref Bounds bounds, IndexFormat indexFormat = IndexFormat.UInt32)
    {
        Mesh.MeshData meshData = meshDataArray[0];

        // Describe mesh data layout
        int vertexAttributeCount = 2;
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        vertexAttributes[0] = new VertexAttributeDescriptor(
            VertexAttribute.Position, dimension: 3, stream: 0
        );
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3, stream: 1
        );
        meshData.SetVertexBufferParams(vertices.Length, vertexAttributes);

        // Set Vertex data
        meshData.GetVertexData<float3>(0).CopyFrom(vertices);
        meshData.GetVertexData<float3>(1).CopyFrom(normals);

        // Set Indice data
        meshData.SetIndexBufferParams(indices.Length, indexFormat);
        NativeArray<uint> triangleIndices = meshData.GetIndexData<uint>();
        triangleIndices.CopyFrom(indices);

        // Set submesh
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length)
        {
            bounds = bounds,
            vertexCount = vertices.Length,
        }, MeshUpdateFlags.DontRecalculateBounds);
    }

}