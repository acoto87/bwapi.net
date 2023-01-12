namespace BWAPI.NET
{
    /// <summary>
    /// The <see cref="TechType"/> (or Technology Type, also referred to as an Ability) represents a Unit's ability
    /// which can be researched with <see cref="Unit.Research(TechType)"/> or used with <see cref="Unit.UseTech(TechType)"/>.
    /// In order for a <see cref="Unit"/> to use its own specialized ability, it must first be available and researched.
    /// </summary>
    public enum TechType
    {
        Stim_Packs = 0,
        Lockdown = 1,
        EMP_Shockwave = 2,
        Spider_Mines = 3,
        Scanner_Sweep = 4,
        Tank_Siege_Mode = 5,
        Defensive_Matrix = 6,
        Irradiate = 7,
        Yamato_Gun = 8,
        Cloaking_Field = 9,
        Personnel_Cloaking = 10,
        Burrowing = 11,
        Infestation = 12,
        Spawn_Broodlings = 13,
        Dark_Swarm = 14,
        Plague = 15,
        Consume = 16,
        Ensnare = 17,
        Parasite = 18,
        Psionic_Storm = 19,
        Hallucination = 20,
        Recall = 21,
        Stasis_Field = 22,
        Archon_Warp = 23,
        Restoration = 24,
        Disruption_Web = 25,
        Unused_26 = 26,
        Mind_Control = 27,
        Dark_Archon_Meld = 28,
        Feedback = 29,
        Optical_Flare = 30,
        Maelstrom = 31,
        Lurker_Aspect = 32,
        Unused_33 = 33,
        Healing = 34,
        None = 44,
        Nuclear_Strike = 45,
        Unknown = 46
    }
}