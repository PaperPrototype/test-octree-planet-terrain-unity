using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using System;

[Serializable]
public class Chunk
{
    public static readonly int Resolution = 16;
    public static readonly int Height = 256;
    public static readonly int Count = Resolution * Height * Resolution;
    public static readonly Bounds Bounds = new Bounds(new Vector3(Resolution, Height, Resolution) - (Vector3.one / 2f), new Vector3(Resolution, Height, Resolution));

    private Minecraft minecraft;
    public Mesh mesh;
    public Bounds boundary;
    public NativeArray<byte> blocks;

    public Chunk(Minecraft minecraft, int3 coord)
    {
        var position = new Vector3(coord.x * Resolution, 0, coord.z * Resolution);
        var size = new Vector3(Resolution, Height, Resolution);
        var center = (size / 2f) - (Vector3.one / 2f);
        this.boundary = new Bounds(center + position, size);
        this.minecraft = minecraft;
    }

    ~Chunk()
    {
        blocks.Dispose();
    }

    public void Draw()
    {
        if (!mesh)
        {
            if (GeometryUtility.TestPlanesAABB(minecraft.frustrum, boundary))
            {
                Schedule(out JobCompleter completer);
                minecraft.toComplete.Add(completer);
            }
            return;
        }
        Graphics.DrawMesh(mesh, boundary.min, Quaternion.identity, minecraft.Material, 0);
    }

    private void Schedule(out JobCompleter jobCompleter)
    {
        // allocate mesh data array
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);

        // create job
        var job = new ChunkJob
        {
            position = boundary.min,
            bounds = Bounds,
            meshDataArray = meshDataArray,
        };

        jobCompleter = new JobCompleter(() =>
        {
            // schedule
            return job.Schedule();
        }, () =>
        {
            // complete job and set mesh
            mesh = new Mesh
            {
                name = "node_mesh",
                bounds = Bounds
            };
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        });
    }
}
