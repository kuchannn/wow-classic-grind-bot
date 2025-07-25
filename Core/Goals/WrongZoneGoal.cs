using Core.GOAP;

using Microsoft.Extensions.Logging;

using SharedLib.Extensions;

using System;
using System.Numerics;

using static System.MathF;

namespace Core.Goals;

public sealed class WrongZoneGoal : GoapGoal
{
    public override float Cost => 19f;

    private readonly ILogger<WrongZoneGoal> logger;
    private readonly ConfigurableInput input;
    private readonly PlayerReader playerReader;
    private readonly PlayerDirection playerDirection;
    private readonly StuckDetector stuckDetector;
    private readonly ClassConfiguration classConfig;

    private float lastDistance = 999;

    public DateTime LastActive { get; private set; }

    public WrongZoneGoal(ILogger<WrongZoneGoal> logger,
        PlayerReader playerReader, ConfigurableInput input,
        PlayerDirection playerDirection,
        StuckDetector stuckDetector, ClassConfiguration classConfig)
        : base(nameof(WrongZoneGoal))
    {
        this.playerReader = playerReader;
        this.input = input;
        this.playerDirection = playerDirection;
        this.logger = logger;
        this.stuckDetector = stuckDetector;
        this.classConfig = classConfig;

        AddPrecondition(GoapKey.incombat, false);
    }

    public override bool CanRun()
    {
        return playerReader.UIMapId.Value == classConfig.WrongZone.ZoneId;
    }

    public override void Update()
    {
        Vector3 exitMap = classConfig.WrongZone.ExitZoneLocation;

        input.StartForward(true);

        if ((DateTime.UtcNow - LastActive).TotalMilliseconds > 10000)
        {
            stuckDetector.SetTargetLocation(exitMap);
        }

        Vector3 playerMap = playerReader.MapPos;
        float mapDistance = playerMap.MapDistanceXYTo(exitMap);
        float heading = DirectionCalculator.CalculateMapHeading(playerMap, exitMap);

        if (lastDistance < mapDistance)
        {
            logger.LogInformation("Further away");
            playerDirection.SetDirection(heading, exitMap);
        }
        else if (!stuckDetector.IsGettingCloser())
        {
            input.StartForward(true);

            if (HasBeenActiveRecently())
            {
                stuckDetector.Update();
            }
            else
            {
                logger.LogInformation("Resuming movement");
            }
        }
        else
        {
            float diff1 = Abs(Tau + heading - playerReader.Direction) % Tau;
            float diff2 = Abs(heading - playerReader.Direction - Tau) % Tau;

            if (Min(diff1, diff2) > 0.3)
            {
                logger.LogInformation("Correcting direction");
                playerDirection.SetDirection(heading, exitMap);
            }
        }

        lastDistance = mapDistance;

        LastActive = DateTime.UtcNow;
    }

    private bool HasBeenActiveRecently()
    {
        return (DateTime.UtcNow - LastActive).TotalMilliseconds < 2000;
    }
}