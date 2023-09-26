using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class Node
{
    public Octree octree;

    public Node parent;
    public Node[] children;
    public int divisions;
    public Vector3 offset;
    public GameObject gameObject;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private NativeArray<Vector3> vertices;
    private NativeArray<int> triangles;
    private NativeArray<int> vertexIndex;
    private NativeArray<int> triangleIndex;

    private bool isScheduled = false; // job is not currently scheduled
    private bool needsDrawn = true; // draw once

    private JobHandle jobHandle;
    private ChunkJob chunkJob;

    public Node(Octree octree, Node parent, Vector3 offset, int divisions)
    {
        this.octree = octree;
        this.parent = parent;
        this.offset = offset;
        this.divisions = divisions;
        this.gameObject = GameObject.Instantiate(octree.chunkPrefab);

        this.meshFilter = gameObject.GetComponent<MeshFilter>();
        this.meshRenderer = gameObject.GetComponent<MeshRenderer>();

        vertices = new NativeArray<Vector3>(24 * octree.chunkResolution * octree.chunkResolution * octree.chunkResolution, Allocator.Persistent);
        triangles = new NativeArray<int>(36 * octree.chunkResolution * octree.chunkResolution * octree.chunkResolution, Allocator.Persistent);
        vertexIndex = new NativeArray<int>(1, Allocator.Persistent);
        triangleIndex = new NativeArray<int>(1, Allocator.Persistent);

        var nodePosition = NodePosition();
        gameObject.transform.position = nodePosition;
        gameObject.name = nodePosition.ToString();
    }

    public void Clear()
    {
        meshFilter.mesh = null;
        needsDrawn = true;
    }

    public void Dispose()
    {
        if (isScheduled)
        {
            // complete job so we can free its memory
            jobHandle.Complete();
        }

        GameObject.Destroy(gameObject);
        vertices.Dispose();
        triangles.Dispose();
        vertexIndex.Dispose();
        triangleIndex.Dispose();
    }

    public void Schedule()
    {
        // if we are a leaf node
        if (IsLeaf() && needsDrawn && !isScheduled)
        {
            // create job
            chunkJob = new ChunkJob();

            chunkJob.planetCenterPosition = octree.root.NodePosition();
            chunkJob.planetRadius = octree.planetRadius;

            chunkJob.vertices = vertices;
            chunkJob.triangles = triangles;
            chunkJob.vertexIndex = vertexIndex;
            chunkJob.triangleIndex = triangleIndex;

            chunkJob.chunkResolution = octree.chunkResolution;
            chunkJob.nodeScale = NodeScale();
            chunkJob.voxelScale = octree.voxelScale;
            chunkJob.worldNodePosition = NodePosition();

            // schedule
            jobHandle = chunkJob.Schedule();

            isScheduled = true;
            needsDrawn = false;
        }
    }

    public void Complete()
    {
        if (isScheduled)
        {
            // complete job and set mesh
            jobHandle.Complete();
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            isScheduled = false;
        }
    }

    public bool TryComplete()
    {
        if (isScheduled && jobHandle.IsCompleted)
        {
            // complete job and set mesh
            jobHandle.Complete();
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            isScheduled = false;

            return true;
        }

        return false;
    }

    public bool IsLeaf()
    {
        if (children == null)
        {
            return true;
        }

        return false;
    }

    public int NodeResolution()
    {
        return (int)Mathf.Pow(2, (divisions - 1));
    }

    public float NodeScale()
    {
        return octree.voxelScale * octree.chunkResolution * NodeResolution();
    }

    public Vector3 NodePosition()
    {
        if (parent == null)
        {
            return this.offset;
        }

        float nodeScale = NodeScale();

        // position = (offset * scale)     // our offset position
        //              - one (scale / 2)  // 
        return (offset * nodeScale) - (Vector3.one * (nodeScale / 2))
            + parent.NodePosition();
    }
}
