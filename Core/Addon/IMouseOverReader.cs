namespace Core;

public interface IMouseOverReader
{
    int MouseOverLevel { get; }
    UnitClassification MouseOverClassification { get; }
    int MouseOverId { get; }
    int MouseOverGuid { get; }
}
