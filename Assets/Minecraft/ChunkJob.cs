using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ChunkJob : IJob
{
    public Bounds bounds;
    public Mesh.MeshDataArray meshDataArray;
    public float3 position;

    public void Execute()
    {
        int indiceCount = Chunk.Count * 6 * 6; // 6 indicies per quad * 6 quads
        int vertexCount = Chunk.Count * 4 * 6; // 4 vertices per quad * 6 quads

        var indices = new NativeArray<uint>(indiceCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        var vertices = new NativeArray<float3>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        var normals = new NativeArray<float3>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.01f);
        noise.SetSeed(2376);

        int vertexOffset = 0;
        int indiceOffset = 0;

        for (int i = 0; i < Chunk.Count; i++)
        {
            int3 index = IndexUtilities.IndexToXyz(i, Chunk.Resolution, Chunk.Height);

            // if we are a solid block
            if (!IsAir(index.x, index.y, index.z, in noise))
            {
                // local mesh position of the voxel
                float3 pos = index;

                for (int side = 0; side < 6; side++)
                {
                    if (IsAir(index.x + Tables.NeighborOffset[side].x, index.y + Tables.NeighborOffset[side].y, index.z + Tables.NeighborOffset[side].z, in noise))
                    {
                        // vertices
                        vertices[vertexOffset + 0] = Tables.Vertices[Tables.BuildOrder[side][0]] + pos;
                        vertices[vertexOffset + 1] = Tables.Vertices[Tables.BuildOrder[side][1]] + pos;
                        vertices[vertexOffset + 2] = Tables.Vertices[Tables.BuildOrder[side][2]] + pos;
                        vertices[vertexOffset + 3] = Tables.Vertices[Tables.BuildOrder[side][3]] + pos;

                        // normals
                        normals[vertexOffset + 0] = Tables.Normals[side];
                        normals[vertexOffset + 1] = Tables.Normals[side];
                        normals[vertexOffset + 2] = Tables.Normals[side];
                        normals[vertexOffset + 3] = Tables.Normals[side];

                        // indices
                        indices[indiceOffset + 0] = (uint)vertexOffset + 0;
                        indices[indiceOffset + 1] = (uint)vertexOffset + 1;
                        indices[indiceOffset + 2] = (uint)vertexOffset + 2;
                        indices[indiceOffset + 3] = (uint)vertexOffset + 2;
                        indices[indiceOffset + 4] = (uint)vertexOffset + 1;
                        indices[indiceOffset + 5] = (uint)vertexOffset + 3;

                        // increment by 4 because we only added 4 vertices
                        vertexOffset += 4;

                        // increment by 6 because we added 6 int's to our triangles array
                        indiceOffset += 6;
                    }
                }
            }
        }

        // slice of valid data (otherwise mesh data is uneccessarily large)
        var indicesSlice = new NativeArray<uint>(indiceOffset, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        indices.Slice(0, (int)indiceOffset).CopyTo(indicesSlice);
        var verticesSlice = new NativeArray<float3>(vertexOffset, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertices.Slice(0, (int)vertexOffset).CopyTo(verticesSlice);
        var normalsSlice = new NativeArray<float3>(vertexOffset, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        normals.Slice(0, (int)vertexOffset).CopyTo(normalsSlice);

        // apply the mesh to the meshDataArray
        MeshingUtility.ApplyMesh(ref meshDataArray, ref indicesSlice, ref verticesSlice, ref normalsSlice, ref bounds);
    }

    public bool IsAir(int x, int y, int z, in FastNoiseLite noise)
    {
        // the voxels position in world coordinates
        float3 worldVoxelPosition = new float3(x, y, z) + position;

        float height = ((noise.GetNoise(worldVoxelPosition.x, worldVoxelPosition.z) + 1f) / 2f) * (Chunk.Height / 2);
        return worldVoxelPosition.y > height;
    }
}