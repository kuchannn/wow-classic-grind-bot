namespace Core.GOAP;

public enum GoapKey
{
    hastarget,
    dangercombat,
    damagetaken,
    damagedone,
    damagetakenordone,
    targetisalive,
    targettargetsus,
    incombat,
    pethastarget,
    ismounted,
    withinpullrange,
    incombatrange,
    pulled,
    isdead,
    shouldloot,
    shouldgather,
    producedcorpse,
    consumecorpse,
    isswimming,
    itemsbroken,
    gathering,
    targethostile,
    hasfocus,
    focushastarget,
    consumablecorpsenearby,
    LENGTH
}

public static class GoapKey_Extension
{
    private const string unknown = "Unknown";

    private static string ToStringTrue(GoapKey value) => value switch
    {
        GoapKey.hastarget => "Target",
        GoapKey.dangercombat => "Danger",
        GoapKey.damagetaken => "Damage Taken",
        GoapKey.damagedone => "Damage Done",
        GoapKey.targetisalive => "Target alive",
        GoapKey.targettargetsus => "Targets us",
        GoapKey.incombat => "Combat",
        GoapKey.pethastarget => "Pet target",
        GoapKey.ismounted => "Mounted",
        GoapKey.withinpullrange => "Pull range",
        GoapKey.incombatrange => "Combat range",
        GoapKey.pulled => "Pulled",
        GoapKey.isdead => "Dead",
        GoapKey.shouldloot => "Loot",
        GoapKey.shouldgather => "Gather",
        GoapKey.producedcorpse => "Killing blow",
        GoapKey.consumecorpse => "Consume Corpse",
        GoapKey.isswimming => "Swimming",
        GoapKey.itemsbroken => "Broken",
        GoapKey.gathering => "Gathering",
        GoapKey.hasfocus => "Focus",
        GoapKey.focushastarget => "Focus Target",
        GoapKey.targethostile => "Target Hostile",
        GoapKey.damagetakenordone => "Damage Taken or Done",
        GoapKey.consumablecorpsenearby => "Consume Corpse nearby",
        _ => unknown
    };

    private static string ToStringFalse(GoapKey value) => value switch
    {
        GoapKey.hastarget => "!Target",
        GoapKey.dangercombat => "!Danger",
        GoapKey.damagetaken => "!Damage Taken",
        GoapKey.damagedone => "!Damage Done",
        GoapKey.targetisalive => "!Target alive",
        GoapKey.targettargetsus => "!Targets us",
        GoapKey.incombat => "!Combat",
        GoapKey.pethastarget => "!Pet target",
        GoapKey.ismounted => "!Mounted",
        GoapKey.withinpullrange => "!Pull range",
        GoapKey.incombatrange => "!Combat range",
        GoapKey.pulled => "!Pulled",
        GoapKey.isdead => "!Dead",
        GoapKey.shouldloot => "!Loot",
        GoapKey.shouldgather => "!Gather",
        GoapKey.producedcorpse => "!Killing blow",
        GoapKey.consumecorpse => "!Consume Corpse",
        GoapKey.isswimming => "!Swimming",
        GoapKey.itemsbroken => "!Broken",
        GoapKey.gathering => "!Gathering",
        GoapKey.hasfocus => "!Focus",
        GoapKey.focushastarget => "!Focus Target",
        GoapKey.targethostile => "!Target Hostile",
        GoapKey.damagetakenordone => "!Damage Taken or Done",
        GoapKey.consumablecorpsenearby => "!Consume Corpse nearby",
        _ => unknown
    };

    public static string ToStringF(this GoapKey key, bool state)
    {
        return state ? ToStringTrue(key) : ToStringFalse(key);
    }
}