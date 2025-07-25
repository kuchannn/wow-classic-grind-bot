namespace Core;

public enum UnitClass
{
    None,
    Warrior,
    Paladin,
    Hunter,
    Rogue,
    Priest,
    DeathKnight,
    Shaman,
    Mage,
    Warlock,
    Monk,
    Druid,
    DemonHunter
}

public static class UnitClass_Extension
{
    public static string ToStringF(this UnitClass value) => value switch
    {
        UnitClass.None => nameof(UnitClass.None),
        UnitClass.Warrior => nameof(UnitClass.Warrior),
        UnitClass.Paladin => nameof(UnitClass.Paladin),
        UnitClass.Hunter => nameof(UnitClass.Hunter),
        UnitClass.Rogue => nameof(UnitClass.Rogue),
        UnitClass.Priest => nameof(UnitClass.Priest),
        UnitClass.DeathKnight => nameof(UnitClass.DeathKnight),
        UnitClass.Shaman => nameof(UnitClass.Shaman),
        UnitClass.Mage => nameof(UnitClass.Mage),
        UnitClass.Warlock => nameof(UnitClass.Warlock),
        UnitClass.Monk => nameof(UnitClass.Monk),
        UnitClass.Druid => nameof(UnitClass.Druid),
        UnitClass.DemonHunter => nameof(UnitClass.DemonHunter),
        _ => nameof(UnitClass.None)
    };
}