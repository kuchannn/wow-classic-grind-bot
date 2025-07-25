namespace Core;

public enum Form
{
    None = 0,

    Druid_Bear = 1,
    Druid_Aquatic = 2,
    Druid_Cat = 3,
    Druid_Travel = 4,
    Druid_Moonkin = 5,
    Druid_Flight = 6,
    Druid_Cat_Prowl = 7,

    Priest_Shadowform = 8,

    Rogue_Stealth = 9,
    Rogue_Vanish = 10,

    Shaman_GhostWolf = 11,

    Warrior_BattleStance = 12,
    Warrior_DefensiveStance = 13,
    Warrior_BerserkerStance = 14,

    Paladin_Devotion_Aura = 15,
    Paladin_Retribution_Aura = 16,
    Paladin_Concentration_Aura = 17,
    Paladin_Shadow_Resistance_Aura = 18,
    Paladin_Frost_Resistance_Aura = 19,
    Paladin_Fire_Resistance_Aura = 20,
    Paladin_Sanctity_Aura = 21,
    Paladin_Crusader_Aura = 22,

    DeathKnight_Blood_Presence = 23,
    DeathKnight_Frost_Presence = 24,
    DeathKnight_Unholy_Presence = 25,
}

public static class Form_Extension
{
    public static string ToStringF(this Form value) => value switch
    {
        Form.None => nameof(Form.None),
        Form.Druid_Bear => nameof(Form.Druid_Bear),
        Form.Druid_Aquatic => nameof(Form.Druid_Aquatic),
        Form.Druid_Cat => nameof(Form.Druid_Cat),
        Form.Druid_Travel => nameof(Form.Druid_Travel),
        Form.Druid_Moonkin => nameof(Form.Druid_Moonkin),
        Form.Druid_Flight => nameof(Form.Druid_Flight),
        Form.Druid_Cat_Prowl => nameof(Form.Druid_Cat_Prowl),
        Form.Priest_Shadowform => nameof(Form.Priest_Shadowform),
        Form.Rogue_Stealth => nameof(Form.Rogue_Stealth),
        Form.Rogue_Vanish => nameof(Form.Rogue_Vanish),
        Form.Shaman_GhostWolf => nameof(Form.Shaman_GhostWolf),
        Form.Warrior_BattleStance => nameof(Form.Warrior_BattleStance),
        Form.Warrior_DefensiveStance => nameof(Form.Warrior_DefensiveStance),
        Form.Warrior_BerserkerStance => nameof(Form.Warrior_BerserkerStance),
        Form.Paladin_Devotion_Aura => nameof(Form.Paladin_Devotion_Aura),
        Form.Paladin_Retribution_Aura => nameof(Form.Paladin_Retribution_Aura),
        Form.Paladin_Concentration_Aura => nameof(Form.Paladin_Concentration_Aura),
        Form.Paladin_Shadow_Resistance_Aura => nameof(Form.Paladin_Shadow_Resistance_Aura),
        Form.Paladin_Frost_Resistance_Aura => nameof(Form.Paladin_Frost_Resistance_Aura),
        Form.Paladin_Fire_Resistance_Aura => nameof(Form.Paladin_Fire_Resistance_Aura),
        Form.Paladin_Sanctity_Aura => nameof(Form.Paladin_Sanctity_Aura),
        Form.Paladin_Crusader_Aura => nameof(Form.Paladin_Crusader_Aura),
        Form.DeathKnight_Blood_Presence => nameof(Form.DeathKnight_Blood_Presence),
        Form.DeathKnight_Frost_Presence => nameof(Form.DeathKnight_Frost_Presence),
        Form.DeathKnight_Unholy_Presence => nameof(Form.DeathKnight_Unholy_Presence),
        _ => nameof(Form.None)
    };
}
