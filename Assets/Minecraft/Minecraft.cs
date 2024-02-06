using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Minecraft : MonoBehaviour
{
    [SerializeField] public Material Material;
    [SerializeField] public int Distance = 4;
    [SerializeField] public new Camera camera;

    public Dictionary<int3, Chunk> blocks = new Dictionary<int3, Chunk>();
    public List<JobCompleter> toComplete = new List<JobCompleter>();

    public Plane[] frustrum;

    void Start()
    {
        frustrum = GeometryUtility.CalculateFrustumPlanes(camera);
        for (int x = 0; x < Distance; x++)
        {
            for (int z = 0; z < Distance; z++)
            {
                int3 key = new int3(x, 0, z);
                var chunk = new Chunk(this, key);
                blocks.Add(key, chunk);
            }
        }
    }

    void Update()
    {
        frustrum = GeometryUtility.CalculateFrustumPlanes(camera);
        if (toComplete.Count > 0)
        {
            var w = new Stopwatch();
            w.Start();
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(toComplete.Count, Allocator.Temp);
            for (int i = 0; i < toComplete.Count; i++)
            {
                jobHandles[i] = toComplete[i].schedule();
            }
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();

            for (int i = 0; i < toComplete.Count; i++)
            {
                toComplete[i].onComplete();
            }

            toComplete.Clear();
            w.Stop();
            UnityEngine.Debug.Log(w.ElapsedMilliseconds + "ms");
        }

        foreach (var chunk in blocks.Values)
        {
            if (GeometryUtility.TestPlanesAABB(frustrum, chunk.boundary))
            {
                chunk.Draw();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (blocks.Count <= 0) return;
        foreach (var chunk in blocks.Values)
        {
            if (GeometryUtility.TestPlanesAABB(frustrum, chunk.boundary))
            {
                Gizmos.color = Color.blue;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireCube(chunk.boundary.center, chunk.boundary.size);
        }
    }
}
