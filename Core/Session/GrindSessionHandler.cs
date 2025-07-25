using Core.GOAP;

using Microsoft.Extensions.Logging;

using SharedLib;

using System;
using System.Threading;

namespace Core.Session;

public sealed class GrindSessionHandler : IGrindSessionHandler
{
    private readonly ILogger<GrindSessionHandler> logger;
    private readonly PlayerReader playerReader;
    private readonly SessionStat stats;
    private readonly IGrindSessionDAO grindSessionDAO;
    private readonly CancellationToken token;

    private readonly GrindSession session;
    private readonly Thread thread;

    private bool active;

    public GrindSessionHandler(ILogger<GrindSessionHandler> logger,
        DataConfig dataConfig, PlayerReader playerReader, SessionStat stats,
        IGrindSessionDAO grindSessionDAO, CancellationTokenSource<GoapAgent> cts)
    {
        this.logger = logger;
        this.playerReader = playerReader;
        this.stats = stats;
        this.grindSessionDAO = grindSessionDAO;
        token = cts.Token;

        session = new()
        {
            ExpList = ExperienceProvider.Get(dataConfig)
        };

        thread = new Thread(PeriodicSave);
        thread.Start();
    }

    public void Start(string path)
    {
        active = true;

        session.SessionId = Guid.NewGuid();
        session.PathName = path;
        session.PlayerClass = playerReader.Class;
        session.SessionStart = DateTime.Now;
        session.LevelFrom = playerReader.Level.Value;
        session.XpFrom = playerReader.PlayerXp.Value;
        session.MobsKilled = stats.Kills;
    }

    public void Stop(string reason, bool active)
    {
        this.active = active;

        session.SessionEnd = DateTime.Now;
        session.LevelTo = playerReader.Level.Value;
        session.XpTo = playerReader.PlayerXp.Value;
        session.Reason = reason;
        session.Death = stats.Deaths;
        session.MobsKilled = stats.Kills;

        if (session.MobsKilled > 0 && session.TotalTimeInMinutes > 0)
            Save();
    }

    private void Save()
    {
        grindSessionDAO.Save(session);
    }

    private void PeriodicSave()
    {
        while (!token.IsCancellationRequested)
        {
            if (active)
                Stop("auto save", true);

            token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Thread stopped!");
    }
}
