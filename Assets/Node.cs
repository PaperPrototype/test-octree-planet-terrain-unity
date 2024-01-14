using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

    public bool IsScheduled
    {
        get
        {
            return isScheduled;
        }
    }

    private bool isScheduled = false; // job is not currently scheduled
    private bool needsDrawn = true; // draw once

    public Node(Octree octree, Node parent, Vector3 offset, int divisions)
    {
        this.octree = octree;
        this.parent = parent;
        this.offset = offset;
        this.divisions = divisions;
        this.gameObject = GameObject.Instantiate(octree.chunkPrefab);
        if (parent != null)
        {
            this.gameObject.transform.parent = parent.gameObject.transform;
        }

        this.meshFilter = gameObject.GetComponent<MeshFilter>();
        this.meshRenderer = gameObject.GetComponent<MeshRenderer>();

        var nodePosition = NodePosition();
        gameObject.transform.position = nodePosition;
        gameObject.name = nodePosition.ToString();
    }

    public void Clear()
    {
        meshFilter.mesh = null;
        needsDrawn = true;
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    public bool TrySchedule(out JobCompleter jobCompleter)
    {
        // if we are a leaf node
        if (IsLeaf() && needsDrawn && !isScheduled)
        {
            // allocate mesh data array
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);

            // variables
            var bounds = new Bounds(Vector3.zero, Vector3.one * NodeScale());

            // create job
            var chunkJob = new ChunkJob();
            chunkJob.meshDataArray = meshDataArray;
            chunkJob.bounds = bounds;
            chunkJob.planetCenterPosition = octree.root.NodePosition();
            chunkJob.planetRadius = octree.planetRadius;
            chunkJob.chunkResolution = octree.chunkResolution;
            chunkJob.nodeScale = NodeScale();
            chunkJob.voxelScale = octree.voxelScale;
            chunkJob.worldNodePosition = NodePosition();

            jobCompleter = new JobCompleter(() =>
            {
                isScheduled = true;
                needsDrawn = false;

                // schedule
                return chunkJob.Schedule();
            }, () =>
            {
                Debug.Log("completed");
                // complete job and set mesh
                Mesh mesh = new Mesh
                {
                    name = "node_mesh",
                    bounds = bounds
                };
                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
                meshFilter.mesh = mesh;
                isScheduled = false;
            });
            return true;
        }

        jobCompleter = null;
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
