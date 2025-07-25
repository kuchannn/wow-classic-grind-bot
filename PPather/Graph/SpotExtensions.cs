using System;
using System.Numerics;

namespace PPather.Graph;

public static class SpotExtensions
{
    public static Vector3[] ToVecArray(this ReadOnlySpan<Spot> spot)
    {
        Vector3[] result = new Vector3[spot.Length];
        for (int i = 0; i < spot.Length; i++)
        {
            result[i] = spot[i].Loc;
        }
        return result;
    }
}
