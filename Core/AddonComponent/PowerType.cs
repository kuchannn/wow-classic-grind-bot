namespace Core;

// offset by 2 to avoid negative numbers
public enum PowerType
{
    HealthCost,
    None,
    Mana,
    Rage,
    Focus,
    Energy,
    Happiness,
    Runes,
    RunicPower,
    SoulShards,
    LunarPower,
    HolyPower,
    Alternate,
    Maelstrom,
    Chi,
    Insanity,
    ComboPoints,
    Obsolete2,
    ArcaneCharges,
    Fury,
    Pain,
    Essence,
    RuneBlood,
    RuneFrost,
    RuneUnholy,
    NumPowerTypes
}

public static class PowerType_Extension
{
    public static string ToStringF(this PowerType value) => value switch
    {
        PowerType.HealthCost => nameof(PowerType.HealthCost),
        PowerType.None => nameof(PowerType.None),
        PowerType.Mana => nameof(PowerType.Mana),
        PowerType.Rage => nameof(PowerType.Rage),
        PowerType.Focus => nameof(PowerType.Focus),
        PowerType.Energy => nameof(PowerType.Energy),
        PowerType.Happiness => nameof(PowerType.Happiness),
        PowerType.Runes => nameof(PowerType.Runes),
        PowerType.RunicPower => nameof(PowerType.RunicPower),
        PowerType.SoulShards => nameof(PowerType.SoulShards),
        PowerType.LunarPower => nameof(PowerType.LunarPower),
        PowerType.HolyPower => nameof(PowerType.HolyPower),
        PowerType.Alternate => nameof(PowerType.Alternate),
        PowerType.Maelstrom => nameof(PowerType.Maelstrom),
        PowerType.Chi => nameof(PowerType.Chi),
        PowerType.Insanity => nameof(PowerType.Insanity),
        PowerType.ComboPoints => nameof(PowerType.ComboPoints),
        PowerType.Obsolete2 => nameof(PowerType.Obsolete2),
        PowerType.ArcaneCharges => nameof(PowerType.ArcaneCharges),
        PowerType.Fury => nameof(PowerType.Fury),
        PowerType.Pain => nameof(PowerType.Pain),
        PowerType.Essence => nameof(PowerType.Essence),
        PowerType.RuneBlood => nameof(PowerType.RuneBlood),
        PowerType.RuneFrost => nameof(PowerType.RuneFrost),
        PowerType.RuneUnholy => nameof(PowerType.RuneUnholy),
        PowerType.NumPowerTypes => nameof(PowerType.NumPowerTypes),
        _ => nameof(PowerType.None),
    };
}
