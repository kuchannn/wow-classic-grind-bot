using Core.GOAP;
using Microsoft.Extensions.Logging;

namespace Core.Goals;

public sealed class TargetLastTargetGoal : GoapGoal
{
    public override float Cost => 4.1f;

    private readonly ILogger<TargetLastTargetGoal> logger;
    private readonly ConfigurableInput input;
    private readonly Wait wait;

    public TargetLastTargetGoal(ILogger<TargetLastTargetGoal> logger, ConfigurableInput input, Wait wait)
        : base(nameof(TargetLastTargetGoal))
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
        input.PressLastTarget();
        wait.Update();
    }
} 