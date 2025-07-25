using Core.GOAP;
using Microsoft.Extensions.Logging;

namespace Core.Goals;

public sealed class TargetNearestTargetGoal : GoapGoal
{
    public override float Cost => 4.2f;

    private readonly ILogger<TargetNearestTargetGoal> logger;
    private readonly ConfigurableInput input;
    private readonly Wait wait;

    public TargetNearestTargetGoal(ILogger<TargetNearestTargetGoal> logger, ConfigurableInput input, Wait wait)
        : base(nameof(TargetNearestTargetGoal))
    {
        this.logger = logger;
        this.input = input;
        this.wait = wait;

        AddPrecondition(GoapKey.incombat, true);
        AddPrecondition(GoapKey.hastarget, false);

        AddEffect(GoapKey.hastarget, true);
    }

    public override void Update()
    {
        input.PressNearestTarget();
        wait.Update();
    }
} 