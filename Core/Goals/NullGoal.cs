namespace Core.Goals;

public sealed class NullGoal : GoapGoal
{
    public override float Cost => 0;

    public NullGoal() : base(nameof(NullGoal)) { }
}