namespace BWAPI.NET
{
    /// <summary>
    /// Damage types are used in Broodwar to determine the amount of damage that will be done to a unit.
    /// This corresponds with <see cref="UnitSizeType"/> to determine the damage done to a unit.
    /// </summary>
    /// <remarks>
    /// <seealso cref="WeaponType"/>
    /// <seealso cref="DamageType"/>
    /// <seealso cref="UnitSizeType"/>
    /// [View on Liquipedia](http://wiki.teamliquid.net/starcraft/Damage_Type)<br>
    /// [View on Starcraft Campendium (Official Website)](http://classic.battle.net/scc/gs/damage.shtml)<br>
    /// [View on Starcraft Wikia](http://starcraft.wikia.com/wiki/Damage_types)<br>
    /// </remarks>
    public enum DamageType
    {
        Independent = 0,
        Explosive = 1,
        Concussive = 2,
        Normal = 3,
        Ignore_Armor = 4,
        None = 5,
        Unknown = 6
    }
}