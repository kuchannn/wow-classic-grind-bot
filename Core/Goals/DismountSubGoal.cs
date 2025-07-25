using Core.GOAP;

namespace Core.Goals;

public sealed class DismountSubGoal : GoapGoal
{
    public override float Cost => 0.5f;

    private readonly IMountHandler mountHandler;

    public DismountSubGoal(IMountHandler mountHandler)
        : base(nameof(DismountSubGoal))
    {
        this.mountHandler = mountHandler;

        AddPrecondition(GoapKey.ismounted, true);
        AddEffect(GoapKey.ismounted, false);
    }

    public override void Update()
    {
        mountHandler.Dismount();
    }
}
