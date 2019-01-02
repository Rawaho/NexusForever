﻿namespace NexusForever.WorldServer.Game.Entity.Static
{
    public enum Property
    {
        Strength                                    = 0,
        Dexterity                                   = 1,
        Technology                                  = 2,
        Magic                                       = 3,
        Wisdom                                      = 4,
        BaseFocusPool                               = 5,
        RatingFocusRecovery                         = 6,
        BaseHealth                                  = 7,
        HealthRegenMultiplier                       = 8,
        /// <summary>
        /// Endurance
        /// </summary>
        ResourceMax0                                = 9,
        /// <summary>
        /// Kinetic Energy (Warrior), Psi Points (Esper), Volatile Energy (Engineer), Medic Cores (Medic)
        /// </summary>
        ResourceMax1                                = 10,
        ResourceMax2                                = 11,
        /// <summary>
        /// Suit Power (Stalker)
        /// </summary>
        ResourceMax3                                = 12,
        /// <summary>
        /// Spell Power (Spellslinger),
        /// </summary>
        ResourceMax4                                = 13,
        ResourceMax5                                = 14,
        ResourceMax6                                = 15,
        ResourceRegenMultiplier0                    = 16,
        ResourceRegenMultiplier1                    = 17,
        ResourceRegenMultiplier2                    = 18,
        ResourceRegenMultiplier3                    = 19,
        ResourceRegenMultiplier4                    = 20,
        ResourceRegenMultiplier5                    = 21,
        ResourceRegenMultiplier6                    = 22,
        InterruptArmorAfterCCRechargeTime           = 23,
        InterruptArmorAfterCCRechargeCount          = 24,
        RatingLifesteal                             = 25,
        ResistPhysical                              = 26,
        ResistTech                                  = 27,
        ResistMagic                                 = 28,
        RatingMultiHitChance                        = 29,
        KillingSpreeOutOfCombatGracePeriodMS        = 30,
        RatingAvoidReduce                           = 32,
        RatingAvoidIncrease                         = 33,
        RatingCritChanceIncrease                    = 34,
        AssaultRating                               = 35,
        SupportRating                               = 36,
        RatingCriticalMitigation                    = 37,
        ResourceMax7                                = 38,
        ResourceRegenMultiplier7                    = 39,
        Stamina                                     = 40,
        ShieldCapacityMax                           = 41,
        Armor                                       = 42,
        RatingCritChanceDecrease                    = 43,
        InterruptArmorThreshold                     = 44,
        InterruptArmorRechargeTime                  = 45,
        InterruptArmorRechargeCount                 = 46,
        RatingCritSeverityIncrease                  = 47,
        PvPOffensiveRating                          = 48,
        PvPDefensiveRating                          = 49,
        ResourceMax8                                = 50,
        ResourceRegenMultiplier8                    = 51,
        ResourceMax9                                = 52,
        ResourceRegenMultiplier9                    = 53,
        ResourceMax10                               = 54,
        ResourceRegenMultiplier10                   = 55,
        RatingIntensity                             = 56,
        RatingVigor                                 = 57,
        RatingGlanceChance                          = 58,
        RatingArmorPierce                           = 59,
        RatingMultiHitAmount                        = 60,
        RatingGlanceAmount                          = 61,
        RatingDamageReflectAmount                   = 62,
        RatingDamageReflectChance                   = 63,
        RatingCCResilience                          = 64,
        MoveSpeedMultiplier                         = 100, 
        BaseAvoidChance                             = 101,
        BaseCritChance                              = 102,
        DamageMitigationPctOffset                   = 103,
        BaseAvoidReduceChance                       = 104,
        BaseAvoidCritChance                         = 105,
        StealthDetectionModifier                    = 106,
        BaseFocusRecoveryInCombat                   = 107,
        BaseFocusRecoveryOutofCombat                = 108,
        SeeThroughStealth                           = 109,
        FrictionMax                                 = 110,
        HealToShieldConversion                      = 111,
        BaseMultiHitAmount                          = 112,
        PvPXPMultiplier                             = 114,
        SpellMechanicEnergyRegenOrDecayMultiplier   = 117,
        SpellMechanicEnergyDecayOverdriveMultiplier = 118,
        IgnoreArmorBase                             = 122,
        IgnoreShieldBase                            = 123,
        MaxThreatVsCreature                         = 124,
        BreathDecay                                 = 125,
        CCPower                                     = 126,
        CriticalHitSeverityMultiplier               = 127,
        JumpHeight                                  = 129,
        GravityMultiplier                           = 130,
        XpMultiplier                                = 131,
        ThreatMultiplier                            = 132,
        NPCSpellExecutionFreqMultiplier             = 133,
        FallingDamageMultiplier                     = 134,
        FocusCostModifier                           = 135,
        CooldownReductionModifier                   = 136,
        BaseLifesteal                               = 137,
        DamageDealtMultiplierMelee                  = 138,
        DamageDealtMultiplierRanged                 = 139,
        DamageDealtMultiplierSpell                  = 140,
        DamageDealtMultiplierPhysical               = 141,
        DamageDealtMultiplierTech                   = 142,
        DamageDealtMultiplierMagic                  = 143,
        DamageMitigationPctOffsetPhysical           = 144,
        DamageMitigationPctOffsetTech               = 145,
        DamageMitigationPctOffsetMagic              = 146,
        BaseTenacity                                = 147,
        BaseHope                                    = 148,
        BaseToughness                               = 149,
        DamageTakenOffsetPhysical                   = 150,
        DamageTakenOffsetTech                       = 151,
        DamageTakenOffsetMagic                      = 152,
        Heroism                                     = 153,
        BaseMultiHitChance                          = 154,
        BaseDamageReflectAmount                     = 155,
        BaseDamageReflectChance                     = 156,
        BaseCriticalMitigation                      = 157,
        BaseIntensity                               = 158,
        DamageTakenMultiplierPhysical               = 159,
        DamageTakenMultiplierTech                   = 160,
        DamageTakenMultiplierMagic                  = 161,
        HealingMultiplierOutgoing                   = 162,
        HealingMultiplierIncoming                   = 163,
        BaseGlanceChance                            = 164,
        CCDurationModifier                          = 165,
        PvPOffensePctOffset                         = 166,
        PvPDefensePctOffset                         = 167,
        RatingTenacity                              = 168,
        RatingHope                                  = 169,
        RatingToughness                             = 170,
        KillSpreeCCVMulitplier                      = 171,
        FocusFinalMultiplier                        = 172,
        Menace                                      = 173,
        ShieldMitigationMax                         = 175,
        ShieldRegenPct                              = 176,
        ShieldTickTime                              = 177,
        ShieldRebootTime                            = 178,
        ShieldDamageTypes                           = 179,
        SlowFallMultiplier                          = 180,
        PathXpMultiplier                            = 181,
        ScientistScanBotThoroughnessMultiplier      = 182,
        ScientistScanBotScanTimeMultiplier          = 183,
        ScientistScanBotRangeMultiplier             = 184,
        ScientistScanBotHealthMultiplier            = 185,
        ScientistScanBotHealthRegenMultiplier       = 186,
        ScientistScanBotSpeedMultiplier             = 187,
        SettlerImprovementTimeMultiplier            = 188,
        CreatureScientistScanMultiplier             = 189,
        ScientistScanBotCooldownMultiplier          = 190,
        MountSpeedMultiplier                        = 191,
        BaseGlanceAmount                            = 195,
        BaseVigor                                   = 196
    }
}
