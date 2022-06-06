using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Octree : MonoBehaviour
{
    [Space]
    public int maxNodeCreationsPerFrame = 2;
    public int maxMilliseconds = 30;

    [Space]
    public float planetRadius = 2000f;

    [Space]
    public float voxelScale = 1f;
    public int chunkResolution = 16;

    [Space]
    public Transform priority;
    public GameObject chunkPrefab;

    [Space]
    public float innerRadiusPadding = 0;
    public float radius = 1;
    public int divisions = 5;

    [HideInInspector]
    public Node root;
    private int nodeResolution;
    private float nodeScale;
    private double worldVoxelsCount;

    private Stopwatch timer;
    private bool finishedDrawingCurrentlyScheduledNodes = true;

    private void Start()
    {
        nodeResolution = (int)Mathf.Pow(2, (divisions - 1));
        nodeScale = voxelScale * chunkResolution * nodeResolution;

        Vector3 offset = Vector3.up;
        Vector3 pos = (offset * nodeScale) - (Vector3.one * (nodeScale / 2));
        root = new Node(this, null, pos, divisions);
        
        UnityEngine.Debug.Log("nodeResolution: " + nodeResolution);
        UnityEngine.Debug.Log("nodeScale: " + nodeResolution);

        worldVoxelsCount = (double)((double)Mathf.Pow(nodeResolution, 3) * (double)Mathf.Pow(chunkResolution, 3));
        UnityEngine.Debug.Log("world voxels count: " + worldVoxelsCount);
    }

    private void Update()
    {
        timer = new Stopwatch();
        timer.Start();

        Traverse(root, 0);

        if (finishedDrawingCurrentlyScheduledNodes)
        {
            Schedule(root);
            finishedDrawingCurrentlyScheduledNodes = false;
        } else
        {
            if (TryComplete(root))
            {
                finishedDrawingCurrentlyScheduledNodes = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (root != null)
        {
            DrawNodeGizmos(root);
            DrawDebugRadiuses();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("World Voxels Count: " + worldVoxelsCount);
    }
    
    public void Traverse(Node node, int creations)
    {
        if (creations > maxNodeCreationsPerFrame)
        {
            return;
        }

        // if we created new nodes
        bool createdNewNodes = false;

        // if we are not a deepest leaf node node
        if (node.divisions > 1)
        {
            // if we are in the allowed radius
            float distance = Vector3.Distance(priority.position, node.NodePosition());
            float allowedDistance = (voxelScale * chunkResolution * innerRadiusPadding) + (radius * node.NodeScale());

            // if distance is inside of allowed distance
            if (distance < allowedDistance)
            {
                // this is no longer a leaf node we need to clear it
                node.Clear();
                
                // only create children if they don't already exist
                if (node.children == null)
                {
                    // we are creating new nodes
                    createdNewNodes = true;

                    node.children = new Node[8];
                    for (int i = 0; i < 8; i++)
                    {
                        node.children[i] = new Node(this, node, Tables.Offsets[i], node.divisions - 1);
                    }
                }
            }
            // (outside of radius) we should not have children
            // no children means: IsLeaf == true
            else
            {
                // delete children
                DeleteChildren(node);
            }

            if (createdNewNodes)
            {
                creations += 1;
            }

            // if have NOT deleted the children
            // recurse
            if (node.children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Traverse(node.children[i], creations);
                }
            }
        }
    }

    public bool TryComplete(Node node)
    {
        if (timer.Elapsed.Milliseconds > maxMilliseconds)
        {
            return node.TryComplete();
        }

        node.TryComplete();

        if (node.children == null)
        {
            return true;
        }

        // if completed children
        bool completed = true;

        // recurse
        for (int i = 0; i < node.children.Length; i++)
        {
            if (!TryComplete(node.children[i]))
            {
                completed = false;
            }
        }

        return completed;
    }

    public void Schedule(Node node)
    {
        node.Schedule();

        if (node.children == null)
        {
            return;
        }

        // recurse
        for (int i = 0; i < node.children.Length; i++)
        {
            Schedule(node.children[i]);
        }
    }

    public void DeleteChildren(Node node)
    {
        // prevent from infinite recursion
        if (node.children == null)
        {
            return;
        }

        // recurse
        for (int i = 0; i < node.children.Length; i++)
        {
            DeleteChildren(node.children[i]);
        }

        // do deletion
        for (int i = 0; i < node.children.Length; i++)
        {
            // dispose of gameobject and mesh arrays
            node.children[i].Dispose();
        }
        node.children = null;
    }

    public void DrawNodeGizmos(Node node)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(node.NodePosition(), Vector3.one * node.NodeScale());
        Gizmos.DrawSphere(node.NodePosition(), 0.5f);

        // if children exist
        if (node.children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                DrawNodeGizmos(node.children[i]);
            }
        }
    }

    public void DrawDebugRadiuses()
    {
        DrawRadiuses(divisions);
    }

    private void DrawRadiuses(int divisions)
    {
        float nodeResolution = (int)Mathf.Pow(2, (divisions - 1));
        float nodeScale = this.voxelScale * this.chunkResolution * nodeResolution;

        float innerRadius = (this.innerRadiusPadding * this.voxelScale * this.chunkResolution);
        float radius = this.radius * nodeScale;

        float allowedRadius = innerRadius + radius;

        // leaf node
        if (divisions < 2)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(this.priority.position, allowedRadius);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.priority.position, allowedRadius);

            DrawRadiuses(divisions - 1);
        }

    }
}
