using System;

using static System.Diagnostics.Stopwatch;

namespace Core;

public class TimeToKill : IDisposable
{
    private readonly PlayerReader playerReader;
    private readonly CombatLog combatLog;

    private int startGuid;
    private long startTime;
    private float startHealth;

    private float time = float.PositiveInfinity;
    public float Time
    {
        private set => time = value;
        get
        {
            Update();
            return time;
        }
    }

    public TimeToKill(PlayerReader playerReader, CombatLog combatLog)
    {
        this.playerReader = playerReader;
        this.combatLog = combatLog;

        this.combatLog.KillCredit += Reset;
    }

    public void Dispose()
    {
        this.combatLog.KillCredit -= Reset;
    }

    public void Update()
    {
        if (playerReader.TargetGuid == 0)
            return;

        if (startGuid == 0)
        {
            if (playerReader.TargetTarget is
                not UnitsTarget.Me and
                not UnitsTarget.Pet and
                not UnitsTarget.PartyOrPet)
            {
                return;
            }

            startGuid = playerReader.TargetGuid;
            startHealth = playerReader.TargetHealth();
            startTime = GetTimestamp();
        }
        else if (startGuid != playerReader.TargetGuid)
        {
            Reset();
            return;
        }

        float curHealth = playerReader.TargetHealth();

        time =
            curHealth /
            ((startHealth - curHealth) /
            (float)GetElapsedTime(startTime).TotalSeconds);
    }

    private void Reset()
    {
        startGuid = 0;
        startHealth = 0;
        startHealth = 0;
        time = float.PositiveInfinity;
    }
}
