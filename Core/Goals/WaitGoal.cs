using Microsoft.Extensions.Logging;

namespace Core.Goals;

public sealed class WaitGoal : GoapGoal
{
    public override float Cost => 21;

    private readonly ILogger<WaitGoal> logger;
    private readonly Wait wait;

    public WaitGoal(ILogger<WaitGoal> logger, Wait wait)
        : base(nameof(WaitGoal))
    {
        this.logger = logger;
        this.wait = wait;
    }

    public override void OnEnter()
    {
        logger.LogInformation("...");
    }

    public override void Update()
    {
        wait.Update();
    }
}