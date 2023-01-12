using System;

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

    public static class RaceHelper
    {
        private static readonly UnitType[] _workerTypes = { UnitType.Zerg_Drone, UnitType.Terran_SCV, UnitType.Protoss_Probe, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown, UnitType.None, UnitType.Unknown };
        private static readonly UnitType[] _baseTypes = { UnitType.Zerg_Hatchery, UnitType.Terran_Command_Center, UnitType.Protoss_Nexus, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown, UnitType.None, UnitType.Unknown };
        private static readonly UnitType[] _refineryTypes = { UnitType.Zerg_Extractor, UnitType.Terran_Refinery, UnitType.Protoss_Assimilator, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown, UnitType.None, UnitType.Unknown };
        private static readonly UnitType[] _transportTypes = { UnitType.Zerg_Overlord, UnitType.Terran_Dropship, UnitType.Protoss_Shuttle, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown, UnitType.None, UnitType.Unknown };
        private static readonly UnitType[] _supplyTypes = { UnitType.Zerg_Overlord, UnitType.Terran_Supply_Depot, UnitType.Protoss_Pylon, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown, UnitType.None, UnitType.Unknown };

        /// <summary>
        /// Retrieves the default worker type for this <seealso cref="Race"/>.
        /// In Starcraft, workers are the units that are used to construct structures.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns><seealso cref="UnitType"/> of the worker that this race uses.</returns>
        public static UnitType getWorker(this Race race)
        {
            return _workerTypes[(int)race];
        }

        /// <summary>
        /// Retrieves the default resource depot <seealso cref="UnitType"/> that workers of this race can
        /// construct and return resources to.
        /// In Starcraft, the center is the very first structure of the Race's technology
        /// tree. Also known as its base of operations or resource depot.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns><seealso cref="UnitType"/> of the center that this race uses</returns>
        public static UnitType getResourceDepot(this Race race)
        {
            return _baseTypes[(int)race];
        }

        /// <summary>
        /// Retrieves the default resource depot <seealso cref="UnitType"/> that workers of this race can
        /// construct and return resources to.
        /// In Starcraft, the center is the very first structure of the Race's technology
        /// tree. Also known as its base of operations or resource depot.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns><seealso cref="UnitType"/> of the center that this race uses</returns>
        [Obsolete("As of 4.2.0 due to naming inconsistency. Use #getResourceDepot instead. See https://github.com/bwapi/bwapi/issues/621 for more information.")]
        public static UnitType getCenter(this Race race)
        {
            return getResourceDepot(race);
        }

        /// <summary>
        /// Retrieves the default structure UnitType for this Race that is used to harvest gas from @Geysers.
        /// In Starcraft, you must first construct a structure over a @Geyser in order to begin harvesting Vespene Gas.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns><seealso cref="UnitType"/> of the structure used to harvest gas.</returns>
        public static UnitType getRefinery(this Race race)
        {
            return _refineryTypes[(int)race];
        }

        /// <summary>
        /// Retrieves the default transport <seealso cref="UnitType"/> for this race that is used to transport ground units across the map.
        /// In Starcraft, transports will allow you to carry ground units over unpassable terrain.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>{@link UnitType} for transportation.</returns>
        public static UnitType getTransport(this Race race)
        {
            return _transportTypes[(int)race];
        }

        /// <summary>
        /// Retrieves the default supply provider {@link UnitType} for this race that is used to  construct units.
        /// In Starcraft, training, morphing, or warping in units requires that the player has sufficient supply available for their Race.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>{@link UnitType} that provides the player with supply.</returns>
        public static UnitType getSupplyProvider(this Race race)
        {
            return _supplyTypes[(int)race];
        }
    }
}