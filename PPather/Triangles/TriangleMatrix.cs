/*
 *  Part of PPather
 *  Copyright Pontus Borg 2008
 *
 */

using Microsoft.Extensions.Logging;

using PPather.Triangles.Data;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading;
using System.Threading.Tasks;

using static WowTriangles.Utils;

namespace WowTriangles;

public sealed class TriangleMatrix
{
    private const float resolution = 8.0f;
    private const float halfResoltion = resolution / 2f;

    private const int CellCapacity = 4096;
    private const int ACount = 128;

    private readonly SparseFloatMatrix2D<List<int>> matrix = new(resolution, CellCapacity);

    public int Count => matrix.Count;

    [SkipLocalsInit]
    public TriangleMatrix(TriangleCollection tc)
    {
        int triangleCount = tc.TriangleCount;
        var LocalDict = new ThreadLocal<Dictionary<int, List<int>>>(() => new Dictionary<int, List<int>>(ACount), true);
        var m = matrix;

        Parallel.For(0, triangleCount, index =>
        {
            var tSpan = tc.TrianglesSpan;
            var vSpan = tc.VerteciesSpan;

            TriangleCollection.GetTriangleVertices(tSpan, vSpan, index,
                out Vector3 v0,
                out Vector3 v1,
                out Vector3 v2,
                out _);

            float minx = Min3(v0.X, v1.X, v2.X);
            float maxx = Max3(v0.X, v1.X, v2.X);
            float miny = Min3(v0.Y, v1.Y, v2.Y);
            float maxy = Max3(v0.Y, v1.Y, v2.Y);

            Vector3 box_center;
            Vector3 box_halfsize;
            box_halfsize.X = halfResoltion;
            box_halfsize.Y = halfResoltion;
            box_halfsize.Z = 1E6f;

            int startx = m.LocalToGrid(minx);
            int endx = m.LocalToGrid(maxx);
            int starty = m.LocalToGrid(miny);
            int endy = m.LocalToGrid(maxy);

            var localDict = LocalDict.Value;

            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    float grid_x = m.GridToLocal(x);
                    float grid_y = m.GridToLocal(y);
                    box_center.X = grid_x + halfResoltion;
                    box_center.Y = grid_y + halfResoltion;
                    box_center.Z = 0;

                    if (!TriangleBoxIntersect(v0, v1, v2, box_center, box_halfsize))
                    {
                        continue;
                    }

                    int key = m.GetKey(grid_x, grid_y);

                    ref List<int> list = ref CollectionsMarshal.GetValueRefOrAddDefault(localDict, key, out bool exists);
                    if (!exists)
                    {
                        localDict[key] = list = new List<int>(ACount);
                    }

                    list.Add(index);
                }
            }
        });

        foreach (var localDict in LocalDict.Values)
        {
            foreach (var kvp in localDict)
            {
                ref List<int> list = ref CollectionsMarshal.GetValueRefOrAddDefault(m.Dict, kvp.Key, out bool exists);
                if (!exists)
                {
                    list = kvp.Value;
                    m.Add(kvp.Key, list);
                }
                else
                {
                    list.AddRange(kvp.Value);
                }
            }
        }

        LocalDict.Dispose();
    }

    public void Clear()
    {
        foreach (List<int> list in matrix.GetAllElements())
        {
            list.Clear();
        }

        matrix.Clear();
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<int> GetAllCloseTo(float x, float y, float range)
    {
        int collectionSize = matrix.CalculateSize(x - range, y - range, x + range, y + range);

        var collectionPooler = ArrayPool<List<int>>.Shared;
        List<int>[] collection = collectionPooler.Rent(collectionSize);
        Memory<List<int>> collectionMem = collection.AsMemory();

        (int collectionCount, int totalSize) = matrix.GetAllInSquare(collectionMem, x - range, y - range, x + range, y + range);

        var intPooler = ArrayPool<int>.Shared;
        int[] elements = intPooler.Rent(totalSize);
        Span<int> outputSpan = elements.AsSpan();

        int c = 0;
        for (int i = 0; i < collectionCount; i++)
        {
            ReadOnlySpan<int> fromSpan = CollectionsMarshal.AsSpan(collectionMem.Span[i]);
            fromSpan.CopyTo(outputSpan.Slice(c, fromSpan.Length));
            c += fromSpan.Length;
        }

        collectionPooler.Return(collection);
        intPooler.Return(elements);

        return outputSpan[..totalSize];
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<int> GetAllInSquare(float x0, float y0, float x1, float y1)
    {
        int collectionSize = matrix.CalculateSize(x0, y0, x1, y1);

        var collectionPooler = ArrayPool<List<int>>.Shared;
        List<int>[] collection = collectionPooler.Rent(collectionSize);
        Memory<List<int>> collectionMem = collection.AsMemory();

        (int collectionCount, int totalSize) = matrix.GetAllInSquare(collectionMem, x0, y0, x1, y1);

        var intPooler = ArrayPool<int>.Shared;
        int[] elements = intPooler.Rent(totalSize);
        Span<int> outputSpan = elements.AsSpan();

        int c = 0;
        for (int i = 0; i < collectionCount; i++)
        {
            ReadOnlySpan<int> fromSpan = CollectionsMarshal.AsSpan(collectionMem.Span[i]);
            fromSpan.CopyTo(outputSpan.Slice(c, fromSpan.Length));
            c += fromSpan.Length;
        }

        collectionPooler.Return(collection);
        intPooler.Return(elements);

        return outputSpan[..totalSize];
    }
}