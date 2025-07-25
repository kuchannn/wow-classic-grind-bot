namespace Core;

public enum MissType
{
    NONE,
    ABSORB,
    BLOCK,
    DEFLECT,
    DODGE,
    EVADE,
    IMMUNE,
    MISS,
    PARRY,
    REFLECT,
    RESIST
}

public static class MissType_Extensions
{
    public static string ToStringF(this MissType value) => value switch
    {
        MissType.ABSORB => nameof(MissType.ABSORB),
        MissType.BLOCK => nameof(MissType.BLOCK),
        MissType.DEFLECT => nameof(MissType.DEFLECT),
        MissType.DODGE => nameof(MissType.DODGE),
        MissType.EVADE => nameof(MissType.EVADE),
        MissType.IMMUNE => nameof(MissType.IMMUNE),
        MissType.MISS => nameof(MissType.MISS),
        MissType.PARRY => nameof(MissType.PARRY),
        MissType.REFLECT => nameof(MissType.REFLECT),
        MissType.RESIST => nameof(MissType.RESIST),
        _ => nameof(MissType.NONE)
    };
}
