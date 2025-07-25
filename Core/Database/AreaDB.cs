using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SharedLib.Extensions;

using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

using WowheadDB;

using static System.IO.File;
using static System.IO.Path;

namespace Core.Database;

public enum NPCType
{
    None,
    Flightmaster,
    Innkeeper,
    Repair,
    Vendor,
    Trainer
}

public static class NPCType_Extension
{
    public static string ToStringF(this NPCType value) => value switch
    {
        NPCType.None => nameof(NPCType.None),
        NPCType.Innkeeper => nameof(NPCType.Innkeeper),
        NPCType.Flightmaster => nameof(NPCType.Flightmaster),
        NPCType.Repair => nameof(NPCType.Repair),
        NPCType.Vendor => nameof(NPCType.Vendor),
        NPCType.Trainer => nameof(NPCType.Trainer),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}

public sealed class AreaDB : IDisposable
{
    private readonly ILogger logger;
    private readonly DataConfig dataConfig;

    private readonly CancellationToken token;
    private readonly ManualResetEventSlim resetEvent;
    private readonly Thread thread;

    private readonly JsonSerializerSettings npcJsonSettings = new()
    {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    private int areaId = -1;

    public FrozenDictionary<string, Vector3> NpcWorldLocations { private set; get; } = FrozenDictionary<string, Vector3>.Empty;
    public Area? CurrentArea { private set; get; }

    public event Action? Changed;

    public AreaDB(ILogger logger, DataConfig dataConfig,
        CancellationTokenSource cts)
    {
        this.logger = logger;
        this.dataConfig = dataConfig;
        token = cts.Token;
        resetEvent = new();

        thread = new(ReadArea);
        thread.Start();
    }

    public void Dispose()
    {
        resetEvent.Set();
    }

    public void Update(int areaId)
    {
        if (this.areaId == areaId)
            return;

        this.areaId = areaId;
        resetEvent.Set();
    }

    private void ReadArea()
    {
        resetEvent.Wait();

        while (!token.IsCancellationRequested)
        {
            try
            {
                CurrentArea = JsonConvert.DeserializeObject<Area>(
                    ReadAllText(Join(dataConfig.ExpArea, $"{areaId}.json")));

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3>>(
                    ReadAllText(Join(dataConfig.NpcLocations, $"{areaId}.json")), npcJsonSettings);

                NpcWorldLocations = data != null
                    ? data.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
                    : FrozenDictionary<string, Vector3>.Empty;

                Changed?.Invoke();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e.StackTrace);
            }

            resetEvent.Reset();
            resetEvent.Wait();
        }
    }

    public ReadOnlySpan<NPC> GetNPCsByType(NPCType type)
    {
        if (CurrentArea == null)
            return [];

        return type switch
        {
            NPCType.Flightmaster => CollectionsMarshal.AsSpan(CurrentArea.flightmaster),
            NPCType.Innkeeper => CollectionsMarshal.AsSpan(CurrentArea.innkeeper),
            NPCType.Repair => CollectionsMarshal.AsSpan(CurrentArea.repair),
            NPCType.Vendor => CollectionsMarshal.AsSpan(CurrentArea.vendor),
            NPCType.Trainer => CollectionsMarshal.AsSpan(CurrentArea.trainer),
            NPCType.None => GetAllNPCs(),
            _ => []
        };
    }

    private ReadOnlySpan<NPC> GetAllNPCs()
    {
        if (CurrentArea == null)
            return [];

        List<NPC>[] collections = [
            CurrentArea.flightmaster,
            CurrentArea.innkeeper,
            CurrentArea.repair,
            CurrentArea.vendor,
            CurrentArea.trainer
        ];

        int total = 0;
        foreach (var col in collections)
            total += col?.Count ?? 0;

        ArrayPool<NPC> pooler = ArrayPool<NPC>.Shared;
        NPC[] result = pooler.Rent(total);
        int offset = 0;

        foreach (var col in collections)
        {
            if (col == null)
            {
                continue;
            }

            col.CopyTo(result, offset);
            offset += col.Count;
        }

        pooler.Return(result);
        return result.AsSpan(0, offset);
    }

    public bool TryGetNearestNPC(
        PlayerFaction faction,
        NPCType type,
        Vector3 playerPosW,
        string[] allowedNames,
        [MaybeNullWhen(false)] out NPC npc,
        out Vector3 pos)
    {
        npc = default;
        pos = default;

        float distance = float.MaxValue;

        ReadOnlySpan<NPC> npcs = GetNPCsByType(type);
        for (int i = 0; i < npcs.Length; i++)
        {
            NPC n = npcs[i];

            // Those Vendor NPCS whom are class specific
            // Demon Trainer example
            if (type != NPCType.Trainer &&
                n.description != null &&
                n.description.Contains(NPCType.Trainer.ToStringF()))
            {
                continue;
            }

            if (allowedNames.Length != 0 && !allowedNames.Contains(n.name))
                continue;

            if (!NpcWorldLocations.TryGetValue(n.name, out Vector3 worldPos))
                continue;

            float d = playerPosW.WorldDistanceXYTo(worldPos);
            if (d < distance && FriendlyToPlayer(n, faction))
            {
                pos = worldPos;
                distance = d;
                npc = n;
            }
        }

        if (npc != null)
        {
            if (npc.MapCoords == null || npc.MapCoords.Length == 0)
            {
                npc = default;
                return false;
            }

            var firstMapCoord = npc.MapCoords[0];
            float worldZ = pos.Z;
            pos = new(firstMapCoord.X, firstMapCoord.Y, worldZ);
        }

        return npc != null;

        static bool FriendlyToPlayer(NPC npc, PlayerFaction playerFaction) =>
            playerFaction switch
            {
                PlayerFaction.Alliance => npc.reactalliance == 1,
                PlayerFaction.Horde => npc.reacthorde == 1,
                _ => false
            };
    }
}