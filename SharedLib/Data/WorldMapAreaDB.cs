﻿using Newtonsoft.Json;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SharedLib;

public sealed class WorldMapAreaDB
{
    private readonly FrozenDictionary<int, WorldMapArea> wmas;

    public IEnumerable<WorldMapArea> Values => wmas.Values;

    public WorldMapAreaDB(DataConfig dataConfig)
    {
        ReadOnlySpan<WorldMapArea> span =
            JsonConvert.DeserializeObject<WorldMapArea[]>(
                File.ReadAllText(
                    Path.Join(dataConfig.ExpDbc, "WorldMapArea.json")));

        Dictionary<int, WorldMapArea> wmas = [];
        for (int i = 0; i < span.Length; i++)
            wmas.Add(span[i].UIMapId, span[i]);

        this.wmas = wmas.ToFrozenDictionary();
    }

    public int GetAreaId(int uiMap)
    {
        return wmas.TryGetValue(uiMap, out WorldMapArea map) ? map.AreaID : -1;
    }

    public int GetMapId(int uiMap)
    {
        return wmas.TryGetValue(uiMap, out WorldMapArea map) ? map.MapID : -1;
    }

    public bool TryGet(int uiMap, out WorldMapArea wma)
    {
        return wmas.TryGetValue(uiMap, out wma);
    }

    //

    public static Vector3 ToWorld_FlipXY(Vector3 map, in WorldMapArea wma)
    {
        return new Vector3(wma.ToWorldX(map.Y), wma.ToWorldY(map.X), map.Z);
    }

    public Vector3 ToWorld_FlipXY(int uiMap, Vector3 map)
    {
        return wmas.TryGetValue(uiMap, out WorldMapArea wma)
            ? new Vector3(wma.ToWorldX(map.Y), wma.ToWorldY(map.X), map.Z)
            : Vector3.Zero;
    }

    public void ToWorldXY_FlipXY(int uiMap, Vector3[] map)
    {
        WorldMapArea wma = wmas[uiMap];
        for (int i = 0; i < map.Length; i++)
        {
            Vector3 p = map[i];
            map[i] = new Vector3(wma.ToWorldX(p.Y), wma.ToWorldY(p.X), p.Z);
        }
    }

    //

    public Vector3 ToMap_FlipXY(Vector3 world, float mapId, int uiMap)
    {
        WorldMapArea wma = GetWorldMapArea(world.X, world.Y, (int)mapId, uiMap);
        return new Vector3(wma.ToMapY(world.Y), wma.ToMapX(world.X), world.Z);
    }

    public static Vector3 ToMap_FlipXY(Vector3 world, in WorldMapArea wma)
    {
        return new Vector3(wma.ToMapY(world.Y), wma.ToMapX(world.X), world.Z);
    }

    public void ToMap_FlipXY(int uiMap, Span<Vector3> worlds)
    {
        if (!TryGet(uiMap, out WorldMapArea wma))
            return;

        for (int i = 0; i < worlds.Length; i++)
        {
            Vector3 world = worlds[i];
            worlds[i] = new Vector3(wma.ToMapY(world.Y), wma.ToMapX(world.X), world.Z);
        }
    }

    //

    public WorldMapArea GetWorldMapArea(float worldX, float worldY, int mapId, int uiMap)
    {
        IEnumerable<WorldMapArea> maps =
            wmas.Values.Where(ContainsWorldPosAndMapId);

        bool ContainsWorldPosAndMapId(WorldMapArea i) =>
                worldX <= i.LocTop &&
                worldX >= i.LocBottom &&
                worldY <= i.LocLeft &&
                worldY >= i.LocRight &&
                i.MapID == mapId;

        if (!maps.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(wmas), $"Failed to find map area for spot {worldX}, {worldY}, {mapId}");
        }

        if (maps.Count() > 1)
        {
            // sometimes we end up with 2 map areas which a coord could be in which is rather unhelpful. e.g. Silithus and Feralas overlap.
            // If we are in a zone and not moving between then the mapHint should take care of the issue
            // otherwise we are not going to be able to work out which zone we are actually in...

            if (uiMap > 0)
            {
                return maps.First(ByUIMapId);
                bool ByUIMapId(WorldMapArea m) => m.UIMapId == uiMap;
            }
            throw new ArgumentOutOfRangeException(nameof(wmas), $"Found many map areas for spot {worldX}, {worldY}, {mapId} : {string.Join(", ", maps.Select(s => s.AreaName))}");
        }

        return maps.First();
    }

}
