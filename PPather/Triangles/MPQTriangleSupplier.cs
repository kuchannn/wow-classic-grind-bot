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

using Microsoft.Extensions.Logging;

using PPather;

using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

using Wmo;

using static System.MathF;
using static Wmo.MapTileFile;

namespace WowTriangles;

public sealed class MPQTriangleSupplier
{
    private readonly ILogger logger;
    private readonly StormDll.ArchiveSet archive;
    private readonly ModelManager modelmanager;
    private readonly WMOManager wmomanager;
    private readonly WDT wdt;
    private readonly WDTFile wdtf;

    private readonly float mapId;

    public MPQTriangleSupplier(ILogger logger, DataConfig dataConfig, float mapId)
    {
        this.logger = logger;
        this.mapId = mapId;

        archive = new StormDll.ArchiveSet(logger, GetArchiveNames(dataConfig));
        modelmanager = new ModelManager(archive);
        wmomanager = new WMOManager(archive, modelmanager);

        wdt = new WDT();
        wdtf = new WDTFile(archive, mapId, wdt, wmomanager, modelmanager, logger);

        // TODO: move this to WDTFile
        if (!wdtf.loaded)
        {
            wdt = null; // bad
            throw new Exception("Failed to set continent to: " + mapId);
        }
    }

    public void Clear()
    {
        archive.Close();
        modelmanager.Clear();
        wmomanager.Clear();
    }

    public static string[] GetArchiveNames(DataConfig dataConfig)
    {
        return Directory.GetFiles(dataConfig.MPQ, "*.MPQ");
    }

    [SkipLocalsInit]
    private void GetChunkData(TriangleCollection triangles, int chunk_x, int chunk_y)
    {
        if (triangles == null || wdtf == null || wdt == null)
            return;
        if (chunk_x < 0 || chunk_y < 0)
            return;
        if (chunk_x > 63 || chunk_y > 63)
            return;

        int index = chunk_y * WDT.SIZE + chunk_x;
        wdtf.LoadMapTile(chunk_x, chunk_y, index);

        MapTile mapTile = wdt.maptiles[index];
        if (!wdt.loaded[index])
            return;

        // Map tiles
        for (int i = 0; i < MapTile.SIZE * MapTile.SIZE; i++)
        {
            if (mapTile.hasChunk[i])
                AddTriangles(triangles, mapTile.chunks[i]);
        }

        // Map Tile - World objects
        for (int i = 0; i < mapTile.wmois.Length; i++)
        {
            WMOInstance wi = mapTile.wmois[i];
            AddTriangles(triangles, wi);
        }

        for (int i = 0; i < mapTile.modelis.Length; i++)
        {
            //if (mi.model.fileName.Contains("bridge"))
            //AddBoundingTriangles(triangles, mapTile.modelis[i]);

            AddDetailedTriangles(triangles, mapTile.modelis[i]);
        }

        wdt.loaded[index] = false;
    }

    [SkipLocalsInit]
    private static void GetChunkCoord(float x, float y, out int chunk_x, out int chunk_y)
    {
        float xOffset = ChunkReader.ZEROPOINT - y;
        float yOffset = ChunkReader.ZEROPOINT - x;

        chunk_x = (int)Round(xOffset / ChunkReader.TILESIZE) - 1;
        chunk_y = (int)Round(yOffset / ChunkReader.TILESIZE) - 1;
    }

    [SkipLocalsInit]
    public void GetTriangles(TriangleCollection tc, float min_x, float min_y, float max_x, float max_y)
    {
        for (int i = 0; i < wdt.gwmois.Length; i++)
        {
            AddTriangles(tc, wdt.gwmois[i]);
        }

        for (float x = min_x; x < max_x; x += ChunkReader.TILESIZE)
        {
            for (float y = min_y; y < max_y; y += ChunkReader.TILESIZE)
            {
                GetChunkCoord(x, y, out int chunk_x, out int chunk_y);
                GetChunkData(tc, chunk_x, chunk_y);
            }
        }
    }

    [SkipLocalsInit]
    private static void AddTriangles(TriangleCollection tc, MapChunk c)
    {
        Span<int> vertices = stackalloc int[9 * 9];
        Span<int> verticesMid = stackalloc int[8 * 8];

        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                ChunkGetCoordForPoint(c, row, col, out float x, out float y, out float z);
                int index = tc.AddVertex(x, y, z);
                vertices[row * 9 + col] = index;
            }
        }

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                ChunkGetCoordForMiddlePoint(c, row, col, out float x, out float y, out float z);
                int index = tc.AddVertex(x, y, z);
                verticesMid[row * 8 + col] = index;
            }
        }

        const int totalCells = 8 * 8;

        for (int cell = 0; cell < totalCells; cell++)
        {
            int row = cell / 8;
            int col = cell % 8;

            if (c.IsHole(col, row))
            {
                continue;
            }

            int rowIndex9 = row * 9;
            int rowIndexMid = row * 8;

            // Precompute indices for vertices
            int v0 = vertices[rowIndex9 + col];
            int v1 = vertices[(row + 1) * 9 + col];
            int v2 = vertices[(row + 1) * 9 + col + 1];
            int v3 = vertices[rowIndex9 + col + 1];
            int vMid = verticesMid[rowIndexMid + col];

            // Add triangles using precomputed indices
            tc.AddTriangle(v0, v1, vMid, TriangleType.Terrain);
            tc.AddTriangle(v1, v2, vMid, TriangleType.Terrain);
            tc.AddTriangle(v2, v3, vMid, TriangleType.Terrain);
            tc.AddTriangle(v3, v0, vMid, TriangleType.Terrain);
        }

        if (!c.haswater)
        {
            return;
        }

        // paint the water
        for (int col = 0; col < LiquidData.HEIGHT_SIZE; col++)
        {
            for (int row = 0; row < LiquidData.HEIGHT_SIZE; row++)
            {
                int ii = row * LiquidData.HEIGHT_SIZE + col;

                ChunkGetCoordForPoint(c, row, col, out float x, out float y, out float z);
                float height = Math.Max(c.water_height[ii], c.water_height1);

                int index = tc.AddVertex(x, y, height);

                vertices[row * LiquidData.HEIGHT_SIZE + col] = index;
            }
        }

        for (int col = 0; col < LiquidData.FLAG_SIZE; col++)
        {
            for (int row = 0; row < LiquidData.FLAG_SIZE; row++)
            {
                int ii = row * LiquidData.FLAG_SIZE + col;

                if (c.legacyWater && c.water_flags[ii] == 15) // causing holes in the water!
                    continue;

                int v0 = vertices[row * LiquidData.HEIGHT_SIZE + col];
                int v1 = vertices[(row + 1) * LiquidData.HEIGHT_SIZE + col];
                int v2 = vertices[(row + 1) * LiquidData.HEIGHT_SIZE + col + 1];
                int v3 = vertices[row * LiquidData.HEIGHT_SIZE + col + 1];

                tc.AddTriangle(v0, v1, v3, TriangleType.Water);
                tc.AddTriangle(v1, v2, v3, TriangleType.Water);
            }
        }
    }

    [SkipLocalsInit]
    private static void AddTriangles(TriangleCollection tc, WMOInstance wi)
    {
        float dx = wi.pos.X;
        float dy = wi.pos.Y;
        float dz = wi.pos.Z;

        float dir_x = wi.dir.Z;
        float dir_y = wi.dir.Y - 90;
        float dir_z = -wi.dir.X;

        int maxVertices = 0;
        WMORoot wmo = wi.wmo;
        for (int gi = 0; gi < wmo.groups.Length; gi++)
        {
            WMOGroup g = wmo.groups[gi];
            maxVertices = Math.Max(maxVertices, (int)g.nVertices);
        }

        Span<int> vertices = stackalloc int[maxVertices];

        for (int gi = 0; gi < wmo.groups.Length; gi++)
        {
            WMOGroup g = wmo.groups[gi];

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;

            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;

            for (int i = 0; i < g.nVertices; i++)
            {
                int off = i * 3;

                float x = g.vertices[off];
                float y = g.vertices[off + 2];
                float z = g.vertices[off + 1];

                Rotate(z, y, dir_x, out z, out y);
                Rotate(x, y, dir_z, out x, out y);
                Rotate(x, z, dir_y, out x, out z);

                float xx = x + dx;
                float yy = y + dy;
                float zz = -z + dz;

                float finalx = ChunkReader.ZEROPOINT - zz;
                float finaly = ChunkReader.ZEROPOINT - xx;
                float finalz = yy;

                vertices[i] = tc.AddVertex(finalx, finaly, finalz);

                if (finalx < minx) { minx = finalx; }
                if (finaly < miny) { miny = finalx; }
                if (finalz < minz) { minz = finalx; }

                if (finalx > maxx) { maxx = finalx; }
                if (finaly > maxy) { maxy = finalx; }
                if (finalz > maxz) { maxz = finalx; }
            }

            for (int i = 0; i < g.nTriangles; i++)
            {
                Mopy flag = (Mopy)g.materials[i];

                bool isRenderFace = (flag & Mopy.WMO_MATERIAL_RENDER) != 0 && (flag & Mopy.WMO_MATERIAL_DETAIL) == 0;
                bool isCollision = (flag & Mopy.WMO_MATERIAL_COLLISION) != 0 || isRenderFace;

                if (!isCollision)
                    continue;

                int off = i * 3;
                int i0 = vertices[g.triangles[off]];
                int i1 = vertices[g.triangles[off + 1]];
                int i2 = vertices[g.triangles[off + 2]];

                tc.AddTriangle(i0, i1, i2, TriangleType.Object);
            }
        }

        /*
        int doodadset = wi.doodadset;
        if (doodadset < wmo.nDoodadSets)
        {
            uint firstDoodad = wmo.doodads[doodadset].firstInstance;
            uint nDoodads = wmo.doodads[doodadset].nInstances;

            for (uint i = 0; i < nDoodads; i++)
            {
                uint d = firstDoodad + i;
                ModelInstance mi = wmo.doodadInstances[d];
                if (mi != null)
                {
                    //logger.WriteLine("I got model " + mi.model.fileName + " at " + mi.pos);
                    //AddTrianglesGroupDoodads(s, mi, wi.dir, wi.pos, 0.0f); // DOes not work :(
                }
            }
        }
        */
    }

    private static void AddTrianglesGroupDoodads(TriangleCollection s, ModelInstance mi, Vector3 world_dir, Vector3 world_off, float rot)
    {
        float dx = mi.pos.X;
        float dy = mi.pos.Y;
        float dz = mi.pos.Z;

        Rotate(dx, dz, rot + 90f, out dx, out dz);

        dx += world_off.X;
        dy += world_off.Y;
        dz += world_off.Z;

        Quaternion q;
        q.X = mi.dir.Z;
        q.Y = mi.dir.X;
        q.Z = mi.dir.Y;
        q.W = mi.w;

        Matrix4x4 rotMatrix = Matrix4x4.CreateFromQuaternion(q);

        Model m = mi.model;

        if (m.boundingTriangles == null)
        {
            return;
        }

        int nBoundingVertices = m.boundingVertices.Length / 3;

        Span<int> vertices = stackalloc int[nBoundingVertices];

        for (int i = 0; i < nBoundingVertices; i++)
        {
            int off = i * 3;
            float x = m.boundingVertices[off];
            float y = m.boundingVertices[off + 2];
            float z = m.boundingVertices[off + 1];
            x *= mi.scale;
            y *= mi.scale;
            z *= -mi.scale;

            Vector3 pos = new(x, y, z);
            Vector3 new_pos = Vector3.Transform(pos, rotMatrix);
            x = pos.X;
            y = pos.Y;
            z = pos.Z;

            float dir_x = world_dir.Z;
            float dir_y = world_dir.Y - 90;
            float dir_z = -world_dir.X;

            Rotate(z, y, dir_x, out z, out y);
            Rotate(x, y, dir_z, out x, out y);
            Rotate(x, z, dir_y, out x, out z);

            float xx = x + dx;
            float yy = y + dy;
            float zz = -z + dz;

            float finalx = ChunkReader.ZEROPOINT - zz;
            float finaly = ChunkReader.ZEROPOINT - xx;
            float finalz = yy;
            vertices[i] = s.AddVertex(finalx, finaly, finalz);
        }

        int nBoundingTriangles = m.boundingTriangles.Length / 3;
        for (uint i = 0; i < nBoundingTriangles; i++)
        {
            uint off = i * 3;
            int v0 = vertices[m.boundingTriangles[off]];
            int v1 = vertices[m.boundingTriangles[off + 1]];
            int v2 = vertices[m.boundingTriangles[off + 2]];
            s.AddTriangle(v0, v2, v1, TriangleType.Model);
        }
    }

    [SkipLocalsInit]
    private static void AddDetailedTriangles(TriangleCollection s, ModelInstance mi)
    {
        Model m = mi.model;
        if (m.boundingTriangles == null)
        {
            return;
        }

        float dx = mi.pos.X;
        float dy = mi.pos.Y;
        float dz = mi.pos.Z;

        float dir_x = mi.dir.Z;
        float dir_y = mi.dir.Y - 90; // -90 is correct!
        float dir_z = -mi.dir.X;

        int nBoundingVertices = m.boundingVertices.Length / 3;

        Span<int> vertices = stackalloc int[nBoundingVertices];

        for (int i = 0; i < nBoundingVertices; i++)
        {
            int off = i * 3;
            float x = m.boundingVertices[off];
            float y = m.boundingVertices[off + 2];
            float z = m.boundingVertices[off + 1];

            Rotate(z, y, dir_x, out z, out y);
            Rotate(x, y, dir_z, out x, out y);
            Rotate(x, z, dir_y, out x, out z);

            x *= mi.scale;
            y *= mi.scale;
            z *= mi.scale;

            float xx = x + dx;
            float yy = y + dy;
            float zz = -z + dz;

            float finalx = ChunkReader.ZEROPOINT - zz;
            float finaly = ChunkReader.ZEROPOINT - xx;
            float finalz = yy;

            vertices[i] = s.AddVertex(finalx, finaly, finalz);
        }

        int nBoundingTriangles = m.boundingTriangles.Length / 3;
        for (uint i = 0; i < nBoundingTriangles; i++)
        {
            uint off = i * 3;
            int v0 = vertices[m.boundingTriangles[off]];
            int v1 = vertices[m.boundingTriangles[off + 1]];
            int v2 = vertices[m.boundingTriangles[off + 2]];
            s.AddTriangle(v0, v1, v2, TriangleType.Model);
        }
    }

    [SkipLocalsInit]
    private static void AddBoundingTriangles(TriangleCollection s, ModelInstance mi)
    {
        Model m = mi.model;
        if (m.boundingTriangles == null)
        {
            return;
        }

        float dx = mi.pos.X;
        float dy = mi.pos.Y;
        float dz = mi.pos.Z;

        float dir_x = mi.dir.Z;
        float dir_y = mi.dir.Y - 90; // -90 is correct!
        float dir_z = -mi.dir.X;

        // Calculate bounding box
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        for (int i = 0; i < m.boundingVertices.Length / 3; i++)
        {
            int off = i * 3;
            float x = m.boundingVertices[off];
            float y = m.boundingVertices[off + 2];
            float z = m.boundingVertices[off + 1];

            Rotate(z, y, dir_x, out z, out y);
            Rotate(x, y, dir_z, out x, out y);
            Rotate(x, z, dir_y, out x, out z);

            x *= mi.scale;
            y *= mi.scale;
            z *= mi.scale;

            float xx = x + dx;
            float yy = y + dy;
            float zz = -z + dz;

            float finalx = ChunkReader.ZEROPOINT - zz;
            float finaly = ChunkReader.ZEROPOINT - xx;
            float finalz = yy;

            if (finalx < minX) minX = finalx;
            if (finaly < minY) minY = finaly;
            if (finalz < minZ) minZ = finalz;

            if (finalx > maxX) maxX = finalx;
            if (finaly > maxY) maxY = finaly;
            if (finalz > maxZ) maxZ = finalz;
        }

        // Add bounding box triangles
        int v0 = s.AddVertex(minX, minY, minZ);
        int v1 = s.AddVertex(maxX, minY, minZ);
        int v2 = s.AddVertex(maxX, maxY, minZ);
        int v3 = s.AddVertex(minX, maxY, minZ);
        int v4 = s.AddVertex(minX, minY, maxZ);
        int v5 = s.AddVertex(maxX, minY, maxZ);
        int v6 = s.AddVertex(maxX, maxY, maxZ);
        int v7 = s.AddVertex(minX, maxY, maxZ);

        // Bottom face
        s.AddTriangle(v0, v1, v2, TriangleType.Model);
        s.AddTriangle(v0, v2, v3, TriangleType.Model);

        // Top face
        s.AddTriangle(v4, v5, v6, TriangleType.Model);
        s.AddTriangle(v4, v6, v7, TriangleType.Model);

        // Front face
        s.AddTriangle(v0, v1, v5, TriangleType.Model);
        s.AddTriangle(v0, v5, v4, TriangleType.Model);

        // Back face
        s.AddTriangle(v3, v2, v6, TriangleType.Model);
        s.AddTriangle(v3, v6, v7, TriangleType.Model);

        // Left face
        s.AddTriangle(v0, v3, v7, TriangleType.Model);
        s.AddTriangle(v0, v7, v4, TriangleType.Model);

        // Right face
        s.AddTriangle(v1, v2, v6, TriangleType.Model);
        s.AddTriangle(v1, v6, v5, TriangleType.Model);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ChunkGetCoordForPoint(MapChunk c, int row, int col,
                                      out float x, out float y, out float z)
    {
        int off = ((row * 17) + col) * 3;
        x = ChunkReader.ZEROPOINT - c.vertices[off + 2];
        y = ChunkReader.ZEROPOINT - c.vertices[off];
        z = c.vertices[off + 1];
    }

    [SkipLocalsInit]
    private static void ChunkGetCoordForMiddlePoint(MapChunk c, int row, int col,
                                        out float x, out float y, out float z)
    {
        int off = (9 + (row * 17) + col) * 3;
        x = ChunkReader.ZEROPOINT - c.vertices[off + 2];
        y = ChunkReader.ZEROPOINT - c.vertices[off];
        z = c.vertices[off + 1];
    }

    [SkipLocalsInit]
    public static void Rotate(float x, float y, float angle, out float nx, out float ny)
    {
        float rot = angle / 360.0f * Tau;
        float c_y = Cos(rot);
        float s_y = Sin(rot);

        nx = (c_y * x) - (s_y * y);
        ny = (s_y * x) + (c_y * y);
    }
}