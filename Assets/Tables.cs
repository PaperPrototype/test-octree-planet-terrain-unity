using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Tables
{
    public static float TerrainMaxHeight = 32f;

    public static Vector3[] Offsets = new Vector3[8]
    {
        // clockwise
        // top
        //          x   y   z
        new Vector3(1f, 1f, 1f), // top right
        new Vector3(0f, 1f, 1f), // bot right
        new Vector3(0f, 1f, 0f), // bot left
        new Vector3(1f, 1f, 0f), // top left
        
        // clockwise
        // bottom
        //          x   y   z
        new Vector3(1f, 0f, 1f), // top right
        new Vector3(0f, 0f, 1f), // bot right
        new Vector3(0f, 0f, 0f), // bot left
        new Vector3(1f, 0f, 0f), // top left
    };

    public static readonly Vector3 VertexOffset = new Vector3(0.5f, 0.5f, 0.5f);

    public static readonly Vector3[] Vertices = new Vector3[8]
    {
        new Vector3(-0.5f, -0.5f, -0.5f) + VertexOffset,
        new Vector3(0.5f, -0.5f, -0.5f) + VertexOffset,
        new Vector3(0.5f, 0.5f, -0.5f) + VertexOffset,
        new Vector3(-0.5f, 0.5f, -0.5f) + VertexOffset,
        new Vector3(-0.5f, -0.5f, 0.5f) + VertexOffset,
        new Vector3(0.5f, -0.5f, 0.5f) + VertexOffset,
        new Vector3(0.5f, 0.5f, 0.5f) + VertexOffset,
        new Vector3(-0.5f, 0.5f, 0.5f) + VertexOffset,
    };

    public static readonly int[,] BuildOrder = new int[6, 4]
    {
        // right, left, up, down, front, back

        // 0 1 2 2 1 3 <- triangle order
        
        {1, 2, 5, 6}, // right face
        {4, 7, 0, 3}, // left face
        
        {3, 7, 2, 6}, // up face
        {1, 5, 0, 4}, // down face
        
        {5, 6, 4, 7}, // front face
        {0, 3, 1, 2}, // back face
    };


    /// <summary>
    /// Voxel neighbor offsets.
    /// </summary>
    public static readonly int3[] NeighborOffset = new int3[6]
    {
        new int3(1, 0, 0),  // right
        new int3(-1, 0, 0), // left
        new int3(0, 1, 0),  // up
        new int3(0, -1, 0), // down
        new int3(0, 0, 1),  // front
        new int3(0, 0, -1), // back
    };
}
