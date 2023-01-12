namespace BWAPI.NET
{
    /// <summary>
    /// The <see cref="Race"/> enum is used to get information about a particular race.
    /// For example, the default worker and supply provider <see cref="UnitType"/>.
    /// As you should already know, Starcraft has three races: @Terran, @Protoss, and @Zerg.
    /// </summary>
    /// <remarks>
    /// <seealso cref="UnitTypeHelper.GetRace(UnitType)"/>
    /// <seealso cref="Player.GetRace"/>
    /// </remarks>
    public enum Race
    {
        Zerg = 0,
        Terran = 1,
        Protoss = 2,
        Other = 3,
        Unused = 4,
        Select = 5,
        Random = 6,
        None = 7,
        Unknown = 8
    }
}