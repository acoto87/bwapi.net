namespace BWAPI.NET
{
    /// <summary>
    /// Size types are used by unit types in Broodwar to determine how much damage will be applied.
    /// This corresponds with <see cref="DamageType"/> for several different damage reduction applications.
    /// </summary>
    /// <remarks>
    /// <seealso cref="DamageType"/>
    /// <seealso cref="UnitType"/>
    /// <seealso cref="UnitSizeType"/>
    /// [View on Starcraft Campendium (Official Website)](http://classic.battle.net/scc/gs/damage.shtml)<br>
    /// </remarks>
    public enum UnitSizeType
    {
        Independent = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        None = 4,
        Unknown = 5
    }
}