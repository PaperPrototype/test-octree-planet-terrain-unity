using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public struct ChunkJob : IJob
{
    public Vector3 planetCenterPosition;
    public float planetRadius;

    public NativeArray<Vector3> vertices;
    public NativeArray<int> triangles;
    public NativeArray<int> vertexIndex;
    public NativeArray<int> triangleIndex;

    public int chunkResolution;
    public float nodeScale;
    public Vector3 worldNodePosition;
    public float voxelScale;

    private Vector3 centerOffset;
    private float normalizedVoxelScale;

    public void Execute()
    {
        // so we can offset the mesh to the the center of the gameObject
        centerOffset = Vector3.one * (nodeScale / 2);

        // nodeScale divided by the the number of voxels in the node
        normalizedVoxelScale = nodeScale / chunkResolution;

        FastNoiseLite noise = new FastNoiseLite();

        // reset variables
        vertexIndex[0] = 0;
        triangleIndex[0] = 0;

        for (int x = 0; x < chunkResolution; x++)
        {
            for (int y = 0; y < chunkResolution; y++)
            {
                for (int z = 0; z < chunkResolution; z++)
                {
                    if (IsSolid(x, y, z, noise))
                    {
                        DrawVoxel(x, y, z, noise);
                    }
                }
            }
        }
    }

    public void DrawVoxel(int x, int y, int z, FastNoiseLite noise)
    {
        // local mesh position of the voxel
        Vector3 pos = (new Vector3(x, y, z) * normalizedVoxelScale) - centerOffset;

        for (int side = 0; side < 6; side++)
        {
            if (!IsSolid(x + Tables.NeighborOffset[side].x, y + Tables.NeighborOffset[side].y, z + Tables.NeighborOffset[side].z, noise))
            {
                vertices[vertexIndex[0] + 0] = Tables.Vertices[Tables.BuildOrder[side, 0]] * normalizedVoxelScale + pos;
                vertices[vertexIndex[0] + 1] = Tables.Vertices[Tables.BuildOrder[side, 1]] * normalizedVoxelScale + pos;
                vertices[vertexIndex[0] + 2] = Tables.Vertices[Tables.BuildOrder[side, 2]] * normalizedVoxelScale + pos;
                vertices[vertexIndex[0] + 3] = Tables.Vertices[Tables.BuildOrder[side, 3]] * normalizedVoxelScale + pos;

                // get the correct triangle index
                triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                // increment by 4 because we only added 4 vertices
                vertexIndex[0] += 4;

                // increment by 6 because we added 6 int's to our triangles array
                triangleIndex[0] += 6;
            }
        }
    }

    /// <summary>
    /// Checks if a voxel is solid
    /// </summary>
    /// <param name="x">local voxel index</param>
    /// <param name="y">local voxel index</param>
    /// <param name="z">local voxel index</param>
    /// <param name="noise"></param>
    /// <returns></returns>
    private bool IsSolid(int x, int y, int z, FastNoiseLite noise)
    {
        // make outer voxels solid
        if (x < 0 || x > chunkResolution - 1 ||
            y < 0 || y > chunkResolution - 1 ||
            z < 0 || z > chunkResolution - 1)
        {
            return false;
        }

        // the voxels position in world coordinates
        Vector3 worldVoxelPosition = ((new Vector3(x, y, z) * normalizedVoxelScale) + worldNodePosition) - centerOffset;

        // a noise for distorting the surface of the planet
        float planetSurfaceDistortion = noise.GetNoise(worldVoxelPosition.x, worldVoxelPosition.y, worldVoxelPosition.z) * 10f;

        // make a sphere planet!
        float distance = Vector3.Distance(worldVoxelPosition, planetCenterPosition) + planetSurfaceDistortion;

        // above ground
        if (distance > planetRadius)
        {
            return false; // air
        }
        // below ground
        else
        {
            return true; // ground
        }
    }
}
