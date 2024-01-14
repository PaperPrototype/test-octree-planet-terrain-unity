using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class IndexUtilities
{
    /// <summary>
    /// Converts a 3D location to a 1D index
    /// </summary>
    /// <param name="xyz">The position of the point</param>
    /// <param name="width">The size of the "container" in the x-direction</param>
    /// <param name="height">The size of the "container" in the y-direction</param>
    /// <returns>The 1D representation of the specified location in the "container"</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int XyzToIndex(int3 xyz, int res)
    {
        return XyzToIndex(xyz.x, xyz.y, xyz.z, res);
    }

    /// <summary>
    /// Converts a 3D location to a 1D index
    /// </summary>
    /// <param name="x">The x value of the location</param>
    /// <param name="y">The y value of the location</param>
    /// <param name="z">The z value of the location</param>
    /// <param name="width">The size of the "container" in the x-direction</param>
    /// <param name="height">The size of the "container" in the y-direction</param>
    /// <returns>The 1D representation of the specified location in the "container"</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int XyzToIndex(int x, int y, int z, int res)
    {
        // return z * width * height + y * width + x;
        return (x * res * res) + (y * res) + z;
    }

    /// <summary>
    /// Converts a 1D index to a 3D location
    /// </summary>
    /// <param name="index">The 1D index in the "container"</param>
    /// <param name="width">The size of the "container" in the x-direction</param>
    /// <param name="height">The size of the "container" in the y-direction</param>
    /// <returns>The 3D representation of the specified index in the "container"</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 IndexToXyz(int index, int res)
    {
        int3 position = new int3(
            index % res,
            index / res % res,
            index / (res * res));
        return position;
    }
}