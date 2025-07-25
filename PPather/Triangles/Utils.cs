/*
  This file is part of ppather.

    PPather is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PPather is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with ppather.  If not, see <http://www.gnu.org/licenses/>.

    Copyright Pontus Borg 2008

 */

using System.Numerics;
using System.Runtime.CompilerServices;

using static System.MathF;
using static System.Numerics.Vector3;

namespace WowTriangles;

public static class Utils
{
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SegmentTriangleIntersect(
        in Vector3 p0, in Vector3 p1,
        in Vector3 t0, in Vector3 t1, in Vector3 t2,
        out Vector3 I)
    {
        Vector3 u = t1 - t0; // triangle vector 1
        Vector3 v = t2 - t0; // triangle vector 2
        Vector3 n = Cross(u, v); // triangle normal

        Vector3 dir = p1 - p0; // ray direction vector
        Vector3 w0 = p0 - t0;
        float a = -Dot(n, w0);
        float b = Dot(n, dir);

        // Avoid repeating Dot(n, dir)
        if (Abs(b) < float.Epsilon)
        {
            I = default;
            return false; // parallel
        }

        // get intersect point of ray with triangle plane
        float r = a / b;
        if (r < 0.0f || r > 1.0f)
        {
            I = default;
            return false; // outside of segment bounds
        }

        I = p0 + (dir * r); // intersect point of line and plane

        // Avoid re-calculating things by merging conditions
        float uu = Dot(u, u);
        float uv = Dot(u, v);
        float vv = Dot(v, v);
        Vector3 w = I - t0;
        float wu = Dot(w, u);
        float wv = Dot(w, v);
        float D = uv * uv - uu * vv;

        // Parametric coordinates test
        float s = (uv * wv - vv * wu) / D;
        if (s < 0.0f || s > 1.0f) return false;

        float t = (uv * wu - uu * wv) / D;
        return !(t < 0.0f || (s + t) > 1.0f);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PointDistanceToSegment(in Vector3 p0, in Vector3 x1, in Vector3 x2)
    {
        Vector3 L = x2 - x1; // the segment vector
        float l2 = Dot(L, L); // square length of the segment
        Vector3 D = p0 - x1; // vector from point to segment start
        float d = Dot(D, L); // projection factor [x2-x1].[p0-x1]lear

        // Optimized return for closest segment point
        if (d < 0.0f) return D.Length();
        return ((d > l2 ? D - L : D - (L * (d / l2))).Length());
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetTriangleNormal(
        in Vector3 t0, in Vector3 t1, in Vector3 t2, out Vector3 normal)
    {
        normal = Normalize(Cross(t1 - t0, t2 - t0));
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PointDistanceToTriangle(
        in Vector3 p0,
        in Vector3 t0, in Vector3 t1, in Vector3 t2)
    {
        Vector3 u = Subtract(t1, t0); // triangle vector 1
        Vector3 v = Subtract(t2, t0); // triangle vector 2
        Vector3 n = Cross(u, v); // triangle normal
        n *= -1E6f;

        if (SegmentTriangleIntersect(p0, n, t0, t1, t2, out Vector3 intersect))
        {
            return Subtract(intersect, p0).Length();
        }

        float d0 = PointDistanceToSegment(p0, t0, t1);
        float d1 = PointDistanceToSegment(p0, t1, t2);
        float d2 = PointDistanceToSegment(p0, t2, t0);

        return Min3(d0, d1, d2);
    }

    // From the book "Real-Time Collision Detection" by Christer Ericson, page 169
    // See also the published Errata
    // http://realtimecollisiondetection.net/books/rtcd/errata/
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TriangleBoxIntersect(
        in Vector3 a, in Vector3 b, in Vector3 c,
        in Vector3 boxCenter, in Vector3 boxExtents)
    {
        Vector3 v0 = a - boxCenter;
        Vector3 v1 = b - boxCenter;
        Vector3 v2 = c - boxCenter;

        Vector3 f0 = v1 - v0;
        Vector3 f1 = v2 - v1;
        Vector3 f2 = v0 - v2;

        return
            AxesIntersectTriangleBox(v0, v1, v2, boxExtents, f0, f1, f2) &&
            TriangleVerticesInsideBox(v0, v1, v2, boxExtents) &&
            TrianglePlaneIntersectBox(f0, f1, v0, boxExtents);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AxesIntersectTriangleBox(
        in Vector3 v0, in Vector3 v1, in Vector3 v2,
        in Vector3 boxExtents,
        in Vector3 f0, in Vector3 f1, in Vector3 f2)
    {
        float r, p0, p1, p2;

        // Axis 1: Cross product of triangle edge f0 with the X, Y, Z axes
        p0 = v0.Z * f0.Y - v0.Y * f0.Z;
        p1 = v1.Z * f0.Y - v1.Y * f0.Z;
        p2 = v2.Z * f0.Y - v2.Y * f0.Z;
        r = boxExtents.Y * Abs(f0.Z) + boxExtents.Z * Abs(f0.Y);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.X * f0.Z - v0.Z * f0.X;
        p1 = v1.X * f0.Z - v1.Z * f0.X;
        p2 = v2.X * f0.Z - v2.Z * f0.X;
        r = boxExtents.X * Abs(f0.Z) + boxExtents.Z * Abs(f0.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.Y * f0.X - v0.X * f0.Y;
        p1 = v1.Y * f0.X - v1.X * f0.Y;
        p2 = v2.Y * f0.X - v2.X * f0.Y;
        r = boxExtents.X * Abs(f0.Y) + boxExtents.Y * Abs(f0.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        // Axis 2: Cross product of triangle edge f1 with the X, Y, Z axes
        p0 = v0.Z * f1.Y - v0.Y * f1.Z;
        p1 = v1.Z * f1.Y - v1.Y * f1.Z;
        p2 = v2.Z * f1.Y - v2.Y * f1.Z;
        r = boxExtents.Y * Abs(f1.Z) + boxExtents.Z * Abs(f1.Y);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.X * f1.Z - v0.Z * f1.X;
        p1 = v1.X * f1.Z - v1.Z * f1.X;
        p2 = v2.X * f1.Z - v2.Z * f1.X;
        r = boxExtents.X * Abs(f1.Z) + boxExtents.Z * Abs(f1.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.Y * f1.X - v0.X * f1.Y;
        p1 = v1.Y * f1.X - v1.X * f1.Y;
        p2 = v2.Y * f1.X - v2.X * f1.Y;
        r = boxExtents.X * Abs(f1.Y) + boxExtents.Y * Abs(f1.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        // Axis 3: Cross product of triangle edge f2 with the X, Y, Z axes
        p0 = v0.Z * f2.Y - v0.Y * f2.Z;
        p1 = v1.Z * f2.Y - v1.Y * f2.Z;
        p2 = v2.Z * f2.Y - v2.Y * f2.Z;
        r = boxExtents.Y * Abs(f2.Z) + boxExtents.Z * Abs(f2.Y);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.X * f2.Z - v0.Z * f2.X;
        p1 = v1.X * f2.Z - v1.Z * f2.X;
        p2 = v2.X * f2.Z - v2.Z * f2.X;
        r = boxExtents.X * Abs(f2.Z) + boxExtents.Z * Abs(f2.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        p0 = v0.Y * f2.X - v0.X * f2.Y;
        p1 = v1.Y * f2.X - v1.X * f2.Y;
        p2 = v2.Y * f2.X - v2.X * f2.Y;
        r = boxExtents.X * Abs(f2.Y) + boxExtents.Y * Abs(f2.X);
        if (Max3(p0, p1, p2) < -r || Min3(p0, p1, p2) > r) return false;

        return true;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TriangleVerticesInsideBox(
        in Vector3 v0, in Vector3 v1, in Vector3 v2,
        in Vector3 boxExtents)
    {
        return
            !(Max3(v0.X, v1.X, v2.X) < -boxExtents.X || Min3(v0.X, v1.X, v2.X) > boxExtents.X) &&
            !(Max3(v0.Y, v1.Y, v2.Y) < -boxExtents.Y || Min3(v0.Y, v1.Y, v2.Y) > boxExtents.Y) &&
            !(Max3(v0.Z, v1.Z, v2.Z) < -boxExtents.Z || Min3(v0.Z, v1.Z, v2.Z) > boxExtents.Z);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TrianglePlaneIntersectBox(
        in Vector3 f0, in Vector3 f1,
        in Vector3 v0,
        in Vector3 boxExtents)
    {
        Vector3 planeNormal = Cross(f0, f1);
        float planeDistance = Dot(planeNormal, v0);

        float r =
            (boxExtents.X * Abs(planeNormal.X)) +
            (boxExtents.Y * Abs(planeNormal.Y)) +
            (boxExtents.Z * Abs(planeNormal.Z));

        return planeDistance <= r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min3(float a, float b, float c)
    {
        return Min(a, Min(b, c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min4(float a, float b, float c, float d)
    {
        return Min(Min(a, b), Min(c, d));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max3(float a, float b, float c)
    {
        return Max(a, Max(b, c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max4(float a, float b, float c, float d)
    {
        return Max(Max(a, b), Max(c, d));
    }
}