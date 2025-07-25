using Core.GOAP;

using Game;

using Microsoft.Extensions.Logging;

using SharedLib.Extensions;
using SharedLib.NpcFinder;

using System;
using System.Linq;
using System.Numerics;
using System.Threading;

#pragma warning disable 162

namespace Core.Goals;

public sealed class FollowRouteGoal : GoapGoal, IGoapEventListener, IRouteProvider, IEditedRouteReceiver, IDisposable
{
    public const float DEFAULT_COST = 20f;
    public const float COST_OFFSET = 0.1f;

    private readonly float cost;
    public override float Cost => cost;
    public override bool CanRun() => pathSettings.CanRun();

    private const bool debug = false;

    private readonly ILogger<FollowRouteGoal> logger;
    private readonly ConfigurableInput input;
    private readonly Wait wait;
    private readonly PlayerReader playerReader;
    private readonly AddonBits bits;
    private readonly ClassConfiguration classConfig;
    private readonly IMountHandler mountHandler;
    private readonly Navigation navigation;

    private readonly IBlacklist targetBlacklist;
    private readonly TargetFinder targetFinder;
    private const NpcNames NpcNameToFind = NpcNames.Enemy | NpcNames.Neutral;

    private const int MIN_TIME_TO_START_CYCLE_PROFESSION = 5000;
    private const int CYCLE_PROFESSION_PERIOD = 8000;

    private readonly ManualResetEventSlim sideActivityManualReset;
    private readonly Thread? sideActivityThread;
    private CancellationTokenSource sideActivityCts;

    private readonly PathSettings pathSettings;

    private Vector3[] mapRoute
    {
        get => pathSettings.Path;
        set => pathSettings.Path = value;
    }

    private DateTime onEnterTime;

    #region IRouteProvider

    public DateTime LastActive => navigation.LastActive;

    public Vector3[] MapRoute() => mapRoute;

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

    #endregion

    public FollowRouteGoal(
        float cost,
        PathSettings pathSettings,
        ILogger<FollowRouteGoal> logger,
        ConfigurableInput input, Wait wait, PlayerReader playerReader,
        AddonBits bits,
        ClassConfiguration classConfig,
        Navigation navigation,
        IMountHandler mountHandler, TargetFinder targetFinder,
        IBlacklist targetBlacklist)
    : base("Follow " + System.IO.Path.GetFileNameWithoutExtension(pathSettings.FileName))
    {
        this.cost = cost;

        this.logger = logger;
        this.input = input;
        this.wait = wait;
        this.classConfig = classConfig;
        this.playerReader = playerReader;
        this.bits = bits;
        this.pathSettings = pathSettings;
        this.mountHandler = mountHandler;
        this.targetFinder = targetFinder;
        this.targetBlacklist = targetBlacklist;

        if (pathSettings.Requirements.Count > 0)
        {
            Keys = [
             new KeyAction() {
                RequirementsRuntime = pathSettings.RequirementsRuntime,
                Name = "Follow " + System.IO.Path.GetFileNameWithoutExtension(pathSettings.FileName)
            }];
        }

        pathSettings.Finished = () => !navigation.HasWaypoint();

        this.navigation = navigation;
        navigation.OnPathCalculated += Navigation_OnPathCalculated;
        navigation.OnDestinationReached += Navigation_OnDestinationReached;
        navigation.OnWayPointReached += Navigation_OnWayPointReached;

        if (classConfig.Mode == Mode.AttendedGather)
        {
            AddPrecondition(GoapKey.dangercombat, false);
            navigation.OnAnyPointReached += Navigation_OnWayPointReached;
        }
        else
        {
            if (classConfig.Loot)
            {
                AddPrecondition(GoapKey.incombat, false);
            }

            AddPrecondition(GoapKey.damagedone, false);
            AddPrecondition(GoapKey.damagetaken, false);

            AddPrecondition(GoapKey.producedcorpse, false);
            AddPrecondition(GoapKey.consumecorpse, false);
        }

        sideActivityCts = new();
        sideActivityManualReset = new(false);

        if (classConfig.Mode == Mode.AttendedGather)
        {
            if (classConfig.GatherFindKeyConfig.Length > 1)
            {
                sideActivityThread = new(Thread_AttendedGather);
                sideActivityThread.Start();
            }
        }
        else
        {
            sideActivityThread = new(Thread_LookingForTarget);
            sideActivityThread.Start();
        }
    }

    public void Dispose()
    {
        navigation.Dispose();

        sideActivityCts.Cancel();
        sideActivityManualReset.Set();
    }

    private void Abort()
    {
        if (!targetBlacklist.Is())
            navigation.StopMovement();

        navigation.Stop();

        sideActivityManualReset.Reset();
        targetFinder.Reset();
    }

    private void Resume()
    {
        onEnterTime = DateTime.UtcNow;

        if (sideActivityCts.IsCancellationRequested)
        {
            sideActivityCts = new();
        }
        sideActivityManualReset.Set();

        if (!navigation.HasWaypoint())
        {
            RefillWaypoints(true);
        }
        else
        {
            navigation.Resume();
        }

        if (playerReader.Class != UnitClass.Druid)
            MountIfPossible();
    }

    public void OnGoapEvent(GoapEventArgs e)
    {
        if (e.GetType() == typeof(AbortEvent))
        {
            Abort();
        }
        else if (e.GetType() == typeof(ResumeEvent))
        {
            Resume();
        }
    }

    public override void OnEnter() => Resume();

    public override void OnExit() => Abort();

    public override void Update()
    {
        if (bits.Target() && bits.Target_Dead())
        {
            Log("Has target but its dead.");
            input.PressClearTarget();
            wait.Update();

            if (bits.Target())
            {
                SendGoapEvent(ScreenCaptureEvent.Default);
                LogWarning($"Unable to clear target! Check Bindpad settings!");
            }
        }

        if (bits.Drowning())
        {
            input.PressJump();
        }

        if (bits.Combat() && classConfig.Mode != Mode.AttendedGather) { return; }

        if (!sideActivityCts.IsCancellationRequested)
        {
            navigation.Update(sideActivityCts.Token);
        }
        else
        {
            if (!bits.Target())
            {
                LogWarning($"{nameof(sideActivityCts)} is cancelled but needs to be restarted!");
                sideActivityCts = new();
                sideActivityManualReset.Set();
            }
        }

        RandomJump();

        wait.Update();
    }

    private void Thread_LookingForTarget()
    {
        sideActivityManualReset.Wait();

        while (!sideActivityCts.IsCancellationRequested)
        {
            if (pathSettings.CanRunSideActivity() &&
                targetFinder.Search(NpcNameToFind, bits.Target_NotDead, sideActivityCts.Token))
            {
                if (bits.Target() && targetBlacklist.Is())
                {
                    Log("Blacklisted target found, clearing target");
                    input.PressClearTarget();
                    wait.Update();
                }
                else
                {
                    Log("Found target!");
                    sideActivityCts.Cancel();
                    sideActivityManualReset.Reset();
                }
            }

            wait.Update();
            sideActivityManualReset.Wait();
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("LookingForTarget Thread stopped!");
    }

    private void Thread_AttendedGather()
    {
        sideActivityManualReset.Wait();

        while (!sideActivityCts.IsCancellationRequested)
        {
            if ((DateTime.UtcNow - onEnterTime).TotalMilliseconds > MIN_TIME_TO_START_CYCLE_PROFESSION)
            {
                AlternateGatherTypes();
            }
            sideActivityCts.Token.WaitHandle.WaitOne(CYCLE_PROFESSION_PERIOD);
            sideActivityManualReset.Wait();
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("AttendedGather Thread stopped!");
    }

    private void AlternateGatherTypes()
    {
        var oldestKey = classConfig.GatherFindKeyConfig.MaxBy(x => x.SinceLastClickMs);
        if (!playerReader.IsCasting() &&
            oldestKey?.SinceLastClickMs > CYCLE_PROFESSION_PERIOD)
        {
            logger.LogInformation($"[{oldestKey.Key}] {oldestKey.Name} pressed for {InputDuration.DefaultPress}ms");
            input.PressRandom(oldestKey);
            oldestKey.SetClicked();
        }
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

    #region Refill rules

    private void Navigation_OnPathCalculated()
    {
        MountIfPossible();
    }

    private void Navigation_OnDestinationReached()
    {
        if (debug)
            LogDebug("Navigation_OnDestinationReached");

        RefillWaypoints(false);
        MountIfPossible();
    }

    private void Navigation_OnWayPointReached()
    {
        MountIfPossible();
    }

    public void RefillWaypoints(bool onlyClosest)
    {
        Log($"{nameof(RefillWaypoints)} - findClosest:{onlyClosest} - ThereAndBack:{pathSettings.PathThereAndBack}");

        Vector3 playerMap = playerReader.MapPos;

        Span<Vector3> pathMap = stackalloc Vector3[mapRoute.Length];
        mapRoute.CopyTo(pathMap);

        float mapDistanceToFirst = playerMap.MapDistanceXYTo(pathMap[0]);
        float mapDistanceToLast = playerMap.MapDistanceXYTo(pathMap[^1]);

        if (mapDistanceToLast < mapDistanceToFirst)
        {
            pathMap.Reverse();
        }

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

        if (onlyClosest)
        {
            if (debug)
                LogDebug($"{nameof(RefillWaypoints)}: Closest wayPoint: {mapClosestPoint}");

            navigation.SetWayPoints(stackalloc Vector3[1] { mapClosestPoint });

            return;
        }

        if (mapClosestPoint == pathMap[0] || mapClosestPoint == pathMap[^1])
        {
            if (pathSettings.PathThereAndBack)
            {
                navigation.SetWayPoints(pathMap);
            }
            else
            {
                pathMap.Reverse();
                navigation.SetWayPoints(pathMap);
            }
        }
        else
        {
            Span<Vector3> points = pathMap[closestIndex..];
            Log($"{nameof(RefillWaypoints)} - Set destination from closest to nearest endpoint - with {points.Length} waypoints");
            navigation.SetWayPoints(points);
        }
    }

    #endregion

    public void ReceivePath(Vector3[] oldMap, Vector3[] newMap)
    {
        // TODO: Cheap way to avoid override all FollowRouteGoal
        // to the same path
        if (mapRoute.SequenceEqual(oldMap))
        {
            this.mapRoute = newMap;
        }
    }

    private void RandomJump()
    {
        if (bits.Grounded() &&
            (DateTime.UtcNow - onEnterTime).TotalSeconds > 5 &&
            classConfig.Jump.SinceLastClickMs > Random.Shared.Next(10_000, 25_000))
        {
            Log("Random jump");
            input.PressJump();
        }
    }

    private void LogDebug(string text)
    {
        logger.LogDebug(text);
    }

    private void LogWarning(string text)
    {
        logger.LogWarning(text);
    }

    private void Log(string text)
    {
        logger.LogInformation(text);
    }
}