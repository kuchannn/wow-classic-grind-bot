using Core.GOAP;

using Game;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;

namespace Core.Goals;

public sealed class ReloggerGoal : GoapGoal
{
    public override float Cost => 0.1f; // Highest priority when disconnected

    private readonly ILogger<ReloggerGoal> logger;
    private readonly Wait wait;
    private readonly ConfigurableInput input;
    private readonly IWowScreen screen;
    private readonly PlayerReader playerReader;
    private readonly ClassConfiguration classConfig;

    private DateTime disconnectTime;
    private ReloggerState state;
    private int attemptCount;

    private enum ReloggerState
    {
        DetectingDisconnect,
        ClickingReconnect,
        WaitingForLogin,
        EnteringCredentials,
        SelectingCharacter,
        EnteringWorld,
        Completed
    }

    public ReloggerGoal(ILogger<ReloggerGoal> logger, Wait wait,
        ConfigurableInput input, IWowScreen screen, PlayerReader playerReader,
        ClassConfiguration classConfig)
        : base(nameof(ReloggerGoal))
    {
        this.logger = logger;
        this.wait = wait;
        this.input = input;
        this.screen = screen;
        this.playerReader = playerReader;
        this.classConfig = classConfig;

        AddPrecondition(GoapKey.isdisconnected, true);
        AddEffect(GoapKey.isdisconnected, false);
    }

    public override bool CanRun() => classConfig.EnableRelogger;

    public override void OnEnter()
    {
        logger.LogInformation("Disconnect detected, starting relogger sequence");
        disconnectTime = DateTime.UtcNow;
        state = ReloggerState.DetectingDisconnect;
        attemptCount++;

        // Bring WoW window to foreground
        input.SetForegroundWindow();
    }

    public override void Update()
    {
        switch (state)
        {
            case ReloggerState.DetectingDisconnect:
                HandleDetectingDisconnect();
                break;
            case ReloggerState.ClickingReconnect:
                HandleClickingReconnect();
                break;
            case ReloggerState.WaitingForLogin:
                HandleWaitingForLogin();
                break;
            case ReloggerState.EnteringCredentials:
                HandleEnteringCredentials();
                break;
            case ReloggerState.SelectingCharacter:
                HandleSelectingCharacter();
                break;
            case ReloggerState.EnteringWorld:
                HandleEnteringWorld();
                break;
            case ReloggerState.Completed:
                // Goal will be deactivated by GOAP planner once isdisconnected = false
                break;
        }
    }

    private void HandleDetectingDisconnect()
    {
        // Wait a moment for UI to stabilize
        if ((DateTime.UtcNow - disconnectTime).TotalSeconds < 2)
            return;

        logger.LogDebug("Looking for disconnect dialog or login screen");
        state = ReloggerState.ClickingReconnect;
    }

    private void HandleClickingReconnect()
    {
        // Try clicking common disconnect dialog button positions
        // These coordinates are typical for disconnect/reconnect buttons
        var reconnectAreas = new[]
        {
            (x: screen.Width / 2, y: screen.Height / 2 + 50), // Center-bottom area
            (x: screen.Width / 2 - 100, y: screen.Height / 2), // Center-left
            (x: screen.Width / 2 + 100, y: screen.Height / 2), // Center-right
        };

        foreach (var area in reconnectAreas)
        {
            logger.LogDebug($"Clicking potential reconnect button at {area.x}, {area.y}");
            input.LeftClickMouse(area.x, area.y);
            wait.Update(500); // Short delay between clicks
        }

        // Also try pressing Enter key which often accepts dialogs
        input.PressRandom(ConsoleKey.Enter);

        state = ReloggerState.WaitingForLogin;
    }

    private void HandleWaitingForLogin()
    {
        // Wait for login screen to appear
        if ((DateTime.UtcNow - disconnectTime).TotalSeconds < 10)
            return;

        logger.LogDebug("Assuming login screen is ready");
        state = ReloggerState.EnteringCredentials;
    }

    private void HandleEnteringCredentials()
    {
        // Note: This is a basic implementation that assumes saved credentials
        // For security, actual credentials should come from secure configuration
        logger.LogDebug("Attempting to log in (assuming saved credentials)");
        
        // Press Enter to try with saved credentials first
        input.PressRandom(ConsoleKey.Enter);
        wait.Update(1000);
        
        // If that didn't work, credentials would need to be entered here
        // This would require additional configuration for username/password
        
        state = ReloggerState.SelectingCharacter;
    }

    private void HandleSelectingCharacter()
    {
        // Wait for character selection screen
        wait.Update(3000);
        
        logger.LogDebug("Attempting to select character and enter world");
        
        // Try pressing Enter to select the first/current character
        input.PressRandom(ConsoleKey.Enter);
        wait.Update(1000);
        
        // Press Enter again to enter world
        input.PressRandom(ConsoleKey.Enter);
        
        state = ReloggerState.EnteringWorld;
    }

    private void HandleEnteringWorld()
    {
        // Wait for world to load and addon data to become available
        wait.Update(5000);
        
        // Check if we're back in game by seeing if addon data is fresh
        if (playerReader.GlobalTime.ElapsedMs() < 1000)
        {
            logger.LogInformation($"Successfully reconnected after {attemptCount} attempt(s)");
            state = ReloggerState.Completed;
            attemptCount = 0;
        }
        else if ((DateTime.UtcNow - disconnectTime).TotalSeconds > classConfig.ReloggerTimeoutSeconds)
        {
            // Timeout - retry the process
            logger.LogWarning("Reconnection attempt timed out, retrying");
            disconnectTime = DateTime.UtcNow;
            state = ReloggerState.DetectingDisconnect;
            
            if (attemptCount > classConfig.ReloggerMaxAttempts)
            {
                logger.LogError($"Too many reconnection attempts ({attemptCount}), giving up");
                state = ReloggerState.Completed;
            }
        }
    }
}