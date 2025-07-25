using Core.Database;
using Core.GOAP;

using Game;

using Microsoft.Extensions.Logging;

using SharedLib.Extensions;
using SharedLib.NpcFinder;

using System;
using System.Buffers;
using System.Linq;
using System.Numerics;
using System.Threading;

using WowheadDB;

#pragma warning disable 162

namespace Core.Goals;

public sealed partial class AdhocNPCGoal : GoapGoal, IGoapEventListener, IRouteProvider, IDisposable
{
    private enum PathState
    {
        ApproachPathStart,
        FollowPath,
        Finished,
    }

    private const bool debug = false;

    private const int MAX_TIME_TO_REACH_MELEE = 10000;
    private const int TIMEOUT = 5000;

    private static readonly SearchValues<string> vendorNpcPattern =
        SearchValues.Create([NPCType.Vendor.ToStringF(), "Sell"], StringComparison.OrdinalIgnoreCase);

    public override float Cost => key.Cost;

    private readonly ILogger<AdhocNPCGoal> logger;
    private readonly ConfigurableInput input;
    private readonly KeyAction key;
    private readonly Wait wait;
    private readonly Navigation navigation;
    private readonly PlayerReader playerReader;
    private readonly AddonBits bits;
    private readonly StopMoving stopMoving;
    private readonly ClassConfiguration classConfig;
    private readonly NpcNameTargeting npcNameTargeting;
    private readonly IMountHandler mountHandler;
    private readonly CancellationToken token;
    private readonly ExecGameCommand execGameCommand;
    private readonly GossipReader gossipReader;
    private readonly AreaDB areaDB;

    private PathState pathState;

    private readonly bool tryFindClosestNPC;
    private NPC? npc;

    #region IRouteProvider

    public Vector3[] MapRoute()
    {
        return Array.Empty<Vector3>();
    }

    public Vector3[] PathingRoute()
    {
        return navigation.TotalRoute;
    }

    public bool HasNext()
    {
        return navigation.HasNext();
    }

    public Vector3 NextMapPoint()
    {
        return navigation.NextMapPoint();
    }

    public DateTime LastActive => navigation.LastActive;

    #endregion

    public AdhocNPCGoal(KeyAction key, ILogger<AdhocNPCGoal> logger, ConfigurableInput input,
        Wait wait, PlayerReader playerReader, GossipReader gossipReader, AddonBits bits,
        Navigation navigation, StopMoving stopMoving, AreaDB areaDB,
        NpcNameTargeting npcNameTargeting, ClassConfiguration classConfig,
        IMountHandler mountHandler, ExecGameCommand exec, CancellationTokenSource cts)
        : base(nameof(AdhocNPCGoal))
    {
        this.logger = logger;
        this.input = input;
        this.key = key;
        this.wait = wait;
        this.playerReader = playerReader;
        this.bits = bits;
        this.stopMoving = stopMoving;
        this.areaDB = areaDB;
        this.npcNameTargeting = npcNameTargeting;
        this.classConfig = classConfig;
        this.mountHandler = mountHandler;
        token = cts.Token;
        this.execGameCommand = exec;
        this.gossipReader = gossipReader;

        this.navigation = navigation;
        navigation.OnDestinationReached += Navigation_OnDestinationReached;
        navigation.OnWayPointReached += Navigation_OnWayPointReached;

        if (bool.TryParse(key.InCombat, out bool result))
        {
            if (!result)
                AddPrecondition(GoapKey.dangercombat, result);
            else
                AddPrecondition(GoapKey.incombat, result);
        }

        Keys = [key];

        tryFindClosestNPC = key.Path.Length == 0;
    }

    public void Dispose()
    {
        navigation.Dispose();
    }

    public override bool CanRun() => key.CanRun();

    public void OnGoapEvent(GoapEventArgs e)
    {
        if (e.GetType() == typeof(ResumeEvent))
        {
            navigation.ResetStuckParameters();
            MountIfPossible();
        }
        else if (e.GetType() == typeof(AbortEvent))
        {
            pathState = PathState.Finished;
        }
    }


    public override void OnEnter()
    {
        if (tryFindClosestNPC)
        {
            bool found = TryAutoSelectNPCAndSetPath();
            if (!found)
            {
                pathState = PathState.Finished;
                LogWarn("No NPC with the criteria!");
                return;
            }
        }

        input.PressClearTarget();
        stopMoving.Stop();

        SetClosestWaypoint();

        pathState = PathState.ApproachPathStart;

        if (key.DontSimplifyPath)
            navigation.SimplifyRouteToWaypoint = false;

        MountIfPossible();
    }

    public override void OnExit()
    {
        navigation.StopMovement();
        navigation.Stop();
        npcNameTargeting.ChangeNpcType(NpcNames.None);

        if (tryFindClosestNPC)
        {
            key.Path = [];
            npc = null;
        }
    }

    public override void Update()
    {
        if (bits.Drowning())
            input.PressJump();

        if (pathState != PathState.Finished)
            navigation.Update();

        wait.Update();
    }


    private void SetClosestWaypoint()
    {
        Vector3 playerMap = playerReader.MapPos;

        Span<Vector3> pathMap = stackalloc Vector3[key.Path.Length];
        key.Path.CopyTo(pathMap);

        float mapDistanceToFirst = playerMap.MapDistanceXYTo(pathMap[0]);
        float mapDistanceToLast = playerMap.MapDistanceXYTo(pathMap[^1]);

        int closestIndex = 0;
        Vector3 mapClosestPoint = Vector3.Zero;
        float distance = float.MaxValue;

        for (int i = 0; i < pathMap.Length; i++)
        {
            Vector3 p = pathMap[i];
            float d = playerMap.MapDistanceXYTo(p);
            if (d < distance)
            {
                distance = d;
                closestIndex = i;
                mapClosestPoint = p;
            }
        }

        navigation.SimplifyRouteToWaypoint = false;

        if (mapClosestPoint == pathMap[0] || mapClosestPoint == pathMap[^1])
        {
            navigation.SetWayPoints(pathMap);
        }
        else
        {
            Span<Vector3> points = pathMap[closestIndex..];
            navigation.SetWayPoints(points);
        }
    }

    private void Navigation_OnWayPointReached()
    {
        if (pathState is PathState.ApproachPathStart)
        {
            LogDebug("1 Reached the start point of the path.");
        }
    }

    private void Navigation_OnDestinationReached()
    {
        if (pathState != PathState.ApproachPathStart || token.IsCancellationRequested)
            return;

        LogDebug("Reached defined path end");
        stopMoving.Stop();

        input.PressClearTarget();
        wait.Update();

        if (tryFindClosestNPC && npc != null)
        {
            execGameCommand.Run($"/target {npc.name}");
            wait.Update();
        }

        bool found = bits.Target();

        if (bits.SoftInteract() &&
            !bits.SoftInteract_Hostile())
        {
            input.PressInteract();
            wait.Update();

            LogWarn($"Soft Interact found NPC with id {playerReader.SoftInteract_Id}");

            found = MoveToTargetAndReached();
        }

        if (!found && !input.KeyboardOnly)
        {
            npcNameTargeting.ChangeNpcType(NpcNames.Friendly | NpcNames.Neutral);
            npcNameTargeting.WaitForUpdate();

            ReadOnlySpan<CursorType> types = [
                CursorType.Vendor,
                CursorType.Repair,
                CursorType.Innkeeper,
                CursorType.Speak
            ];

            found = npcNameTargeting.FindBy(types, token);
            wait.Update();

            if (!found)
            {
                LogWarn($"No target found by cursor({CursorType.Vendor.ToStringF()}, {CursorType.Repair.ToStringF()}, {CursorType.Innkeeper.ToStringF()})!");
            }
        }

        if (!found)
        {
            Log($"Use KeyAction.Key macro to acquire target");
            input.PressRandom(key);
            wait.Update();
        }

        wait.Until(400, bits.Target);
        if (!bits.Target())
        {
            LogWarn("No target found! Turn left to find NPC");
            input.PressFixed(input.TurnLeftKey, 250, token);
            return;
        }

        Log($"Found Target!");
        input.PressInteract();
        wait.Update();

        if (!OpenMerchantWindow())
            return;

        wait.Fixed(classConfig.VendorOpenDelayMs);

        input.PressRandom(ConsoleKey.Escape, InputDuration.DefaultPress);
        input.PressClearTarget();
        wait.Update();

        Span<Vector3> reverseMapPath = stackalloc Vector3[key.Path.Length];
        key.Path.CopyTo(reverseMapPath);
        reverseMapPath.Reverse();
        navigation.SetWayPoints(reverseMapPath);

        pathState++;

        LogDebug("Go back reverse to the start point of the path.");
        navigation.ResetStuckParameters();

        // At this point the BagsFull is false
        // which mean it it would exit the Goal
        // instead keep it trapped to follow the route back
        while (navigation.HasWaypoint() &&
            !token.IsCancellationRequested &&
            pathState == PathState.FollowPath)
        {
            navigation.Update();
            wait.Update();
        }

        pathState = PathState.Finished;

        LogDebug("2 Reached the start point of the path.");
        stopMoving.Stop();

        if (key.DontSimplifyPath)
            navigation.SimplifyRouteToWaypoint = true;

        MountIfPossible();
    }

    private bool MoveToTargetAndReached()
    {
        wait.While(input.Approach.OnCooldown);

        float elapsedMs = wait.Until(MAX_TIME_TO_REACH_MELEE,
            bits.NotMoving, input.PressApproachOnCooldown);

        //LogReachedCorpse(logger, bits.Target(), elapsedMs);

        return bits.Target() && playerReader.MinRangeZero();
    }

    private void MountIfPossible()
    {
        float totalDistance = VectorExt.TotalDistance<Vector3>(navigation.TotalRoute, VectorExt.WorldDistanceXY);

        if (classConfig.UseMount && mountHandler.CanMount() &&
            (MountHandler.ShouldMount(totalDistance) ||
            (navigation.TotalRoute.Length > 0 &&
            mountHandler.ShouldMount(navigation.TotalRoute[^1]))
            ))
        {
            Log("Mount up");
            mountHandler.MountUp();
            navigation.ResetStuckParameters();
        }
    }

    private bool OpenMerchantWindow()
    {
        float e = wait.Until(TIMEOUT, gossipReader.GossipStartOrMerchantWindowOpened);
        if (gossipReader.MerchantWindowOpened())
        {
            LogWarn($"Gossip no options! {e}ms");
        }
        else
        {
            e = wait.Until(TIMEOUT, gossipReader.GossipEnd);
            if (e < 0)
            {
                LogWarn($"Gossip - {nameof(gossipReader.GossipEnd)} not fired after {e}ms");
                return false;
            }
            else
            {
                if (gossipReader.Gossips.TryGetValue(Gossip.Vendor, out int orderNum))
                {
                    Log($"Picked {orderNum}th for {Gossip.Vendor.ToStringF()}");
                    execGameCommand.Run($"/run SelectGossipOption({orderNum})--");
                }
                else
                {
                    LogWarn($"Target({playerReader.TargetId}) has no {Gossip.Vendor.ToStringF()} option!");
                    return false;
                }
            }
        }

        Log($"Merchant window opened after {e}ms");

        // Optional delay before pressing custom macro key (e.g. allows merchant list to populate)
        if (key.BeforeCastDelay > 0)
        {
            // Simple sleep to ensure delay even if cancellation token is unsignaled
            System.Threading.Thread.Sleep(key.BeforeCastDelay);
        }

        // Execute custom macro bound to this KeyAction (sell / buy etc.)
        input.PressRandom(key);

        e = wait.Until(TIMEOUT, gossipReader.MerchantWindowSelling);
        if (e >= 0)
        {
            Log($"Merchant sell grey items started after {e}ms");

            e = wait.Until(TIMEOUT, gossipReader.MerchantWindowSellingFinished);
            if (e >= 0)
            {
                Log($"Merchant sell grey items finished, took {e}ms");
                return true;
            }
            else
            {
                Log($"Merchant sell grey items timeout! Too many items to sell?! Increase {nameof(TIMEOUT)} - {e}ms");
                return true;
            }
        }
        else
        {
            Log($"Merchant sell nothing! {e}ms");
            return true;
        }
    }

    private bool TryAutoSelectNPCAndSetPath()
    {
        NPCType npcType = NPCType.None;

        ReadOnlySpan<char> name = key.Name;

        if (name.Contains(NPCType.Repair.ToStringF(), StringComparison.OrdinalIgnoreCase))
            npcType = NPCType.Repair;
        else if (name.Contains(NPCType.Innkeeper.ToStringF(), StringComparison.OrdinalIgnoreCase))
            npcType = NPCType.Innkeeper;
        else if (name.Contains(NPCType.Flightmaster.ToStringF(), StringComparison.OrdinalIgnoreCase))
            npcType = NPCType.Flightmaster;
        else if (name.Contains(NPCType.Trainer.ToStringF(), StringComparison.OrdinalIgnoreCase))
            npcType = NPCType.Trainer;
        else if (name.ContainsAny(vendorNpcPattern))
            npcType = NPCType.Vendor;

        if (areaDB.CurrentArea == null)
        {
            return false;
        }

        string[] allowedNames = [];

        // TODO: faction specific filter?
        // try to detect pattern
        // [TYPE][ ][npc1 | npc2 | npc3]
        int separator = name.IndexOf(' ');
        if (separator != -1)
        {
            allowedNames = name[(separator + 1)..]
                .ToString()
                .Split('|', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (allowedNames.Length > 0)
                logger.LogInformation($"Search for {npcType} like {string.Join(',', allowedNames)}");
        }

        if (!areaDB.TryGetNearestNPC(playerReader.Faction, npcType, playerReader.WorldPos, allowedNames, out npc, out Vector3 pos))
        {
            return false;
        }

        Vector3 mapPos = pos;
        key.Path = [mapPos];

        LogFoundCloesestNPCByType(logger, npc.name, npcType.ToStringF(), mapPos);

        return true;
    }


    private void Log(string text)
    {
        logger.LogInformation(text);
    }

    private void LogDebug(string text)
    {
        if (debug)
            logger.LogDebug(text);
    }

    private void LogWarn(string text)
    {
        logger.LogWarning(text);
    }


    #region Logging

    [LoggerMessage(
        EventId = 0300,
        Level = LogLevel.Information,
        Message = "Closest NPC found {type} {name} at {pos}")]
    static partial void LogFoundCloesestNPCByType(ILogger logger, string name, string type, Vector3 pos);


    #endregion
}