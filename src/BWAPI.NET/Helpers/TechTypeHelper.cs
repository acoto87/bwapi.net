using System;
using System.Collections.ObjectModel;

namespace BWAPI.NET
{
    /// <summary>
    /// Helper methods for <see cref="TechType"/>.
    /// </summary>
    public static class TechTypeHelper
    {
        private const int TARG_UNIT = 1;
        private const int TARG_POS = 2;
        private const int TARG_BOTH = 3;

        private static readonly int[] _defaultMineralCost = { 100, 200, 200, 100, 0, 150, 0, 200, 100, 150, 100, 100, 0, 100, 0, 200, 100, 100, 0, 200, 150, 150, 150, 0, 100, 200, 0, 200, 0, 100, 100, 100, 200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultTimeCost = { 1200, 1500, 1800, 1200, 0, 1200, 0, 1200, 1800, 1500, 1200, 1200, 0, 1200, 0, 1500, 1500, 1200, 0, 1800, 1200, 1800, 1500, 0, 1200, 1200, 0, 1800, 0, 1800, 1800, 1500, 1800, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultEnergyCost = { 0, 100, 100, 0, 50, 0, 100, 75, 150, 25, 25, 0, 0, 150, 100, 150, 0, 75, 75, 75, 100, 150, 100, 0, 50, 125, 0, 150, 0, 50, 75, 100, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly UnitType[] _whatResearches = { UnitType.Terran_Academy, UnitType.Terran_Covert_Ops, UnitType.Terran_Science_Facility, UnitType.Terran_Machine_Shop, UnitType.None, UnitType.Terran_Machine_Shop, UnitType.None, UnitType.Terran_Science_Facility, UnitType.Terran_Physics_Lab, UnitType.Terran_Control_Tower, UnitType.Terran_Covert_Ops, UnitType.Zerg_Hatchery, UnitType.None, UnitType.Zerg_Queens_Nest, UnitType.None, UnitType.Zerg_Defiler_Mound, UnitType.Zerg_Defiler_Mound, UnitType.Zerg_Queens_Nest, UnitType.None, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Arbiter_Tribunal, UnitType.Protoss_Arbiter_Tribunal, UnitType.None, UnitType.Terran_Academy, UnitType.Protoss_Fleet_Beacon, UnitType.None, UnitType.Protoss_Templar_Archives, UnitType.None, UnitType.None, UnitType.Terran_Academy, UnitType.Protoss_Templar_Archives, UnitType.Zerg_Hydralisk_Den, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.Unknown };
        private static readonly Race[] _techRaces = { Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Terran, Race.Protoss, Race.None, Race.Protoss, Race.Protoss, Race.Protoss, Race.Terran, Race.Protoss, Race.Zerg, Race.None, Race.Terran, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.Terran, Race.Unknown };
        private static readonly WeaponType[] _techWeapons = { WeaponType.None, WeaponType.Lockdown, WeaponType.EMP_Shockwave, WeaponType.Spider_Mines, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.Irradiate, WeaponType.Yamato_Gun, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.Spawn_Broodlings, WeaponType.Dark_Swarm, WeaponType.Plague, WeaponType.Consume, WeaponType.Ensnare, WeaponType.Parasite, WeaponType.Psionic_Storm, WeaponType.None, WeaponType.None, WeaponType.Stasis_Field, WeaponType.None, WeaponType.Restoration, WeaponType.Disruption_Web, WeaponType.None, WeaponType.Mind_Control, WeaponType.None, WeaponType.Feedback, WeaponType.Optical_Flare, WeaponType.Maelstrom, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.None, WeaponType.Nuclear_Strike, WeaponType.Unknown };
        private static readonly int[] _techTypeFlags = { 0, TARG_UNIT, TARG_BOTH, TARG_POS, TARG_BOTH, 0, TARG_UNIT, TARG_UNIT, TARG_UNIT, 0, 0, 0, TARG_UNIT, TARG_UNIT, TARG_BOTH, TARG_BOTH, TARG_UNIT, TARG_BOTH, TARG_UNIT, TARG_BOTH, TARG_UNIT, TARG_BOTH, TARG_BOTH, TARG_UNIT, TARG_UNIT, TARG_BOTH, 0, TARG_UNIT, TARG_UNIT, TARG_UNIT, TARG_UNIT, TARG_BOTH, 0, 0, TARG_BOTH, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, TARG_BOTH, 0 };
        private static readonly Order[] _techOrders = { Order.None, Order.CastLockdown, Order.CastEMPShockwave, Order.PlaceMine, Order.CastScannerSweep, Order.None, Order.CastDefensiveMatrix, Order.CastIrradiate, Order.FireYamatoGun, Order.None, Order.None, Order.None, Order.CastInfestation, Order.CastSpawnBroodlings, Order.CastDarkSwarm, Order.CastPlague, Order.CastConsume, Order.CastEnsnare, Order.CastParasite, Order.CastPsionicStorm, Order.CastHallucination, Order.CastRecall, Order.CastStasisField, Order.None, Order.CastRestoration, Order.CastDisruptionWeb, Order.None, Order.CastMindControl, Order.None, Order.CastFeedback, Order.CastOpticalFlare, Order.CastMaelstrom, Order.None, Order.None, Order.MedicHeal, Order.None, Order.None, Order.None, Order.None, Order.None, Order.None, Order.None, Order.None, Order.None, Order.None, Order.NukePaint, Order.Unknown };
        private static readonly UnitType[][] _techWhatUses =
        {
            new []{ UnitType.Terran_Marine, UnitType.Terran_Firebat, UnitType.Hero_Jim_Raynor_Marine, UnitType.Hero_Gui_Montag },
            new []{ UnitType.Terran_Ghost, UnitType.Hero_Alexei_Stukov, UnitType.Hero_Infested_Duran, UnitType.Hero_Samir_Duran, UnitType.Hero_Sarah_Kerrigan },
            new []{ UnitType.Terran_Science_Vessel, UnitType.Hero_Magellan },
            new []{ UnitType.Terran_Vulture, UnitType.Hero_Jim_Raynor_Vulture },
            new []{ UnitType.Terran_Comsat_Station },
            new []{ UnitType.Terran_Siege_Tank_Tank_Mode, UnitType.Terran_Siege_Tank_Siege_Mode, UnitType.Hero_Edmund_Duke_Tank_Mode, UnitType.Hero_Edmund_Duke_Siege_Mode },
            new []{ UnitType.Terran_Science_Vessel, UnitType.Hero_Magellan },
            new []{ UnitType.Terran_Science_Vessel, UnitType.Hero_Magellan },
            new []{ UnitType.Terran_Battlecruiser, UnitType.Hero_Gerard_DuGalle, UnitType.Hero_Hyperion, UnitType.Hero_Norad_II },
            new []{ UnitType.Terran_Wraith, UnitType.Hero_Tom_Kazansky },
            new []{ UnitType.Terran_Ghost, UnitType.Hero_Alexei_Stukov, UnitType.Hero_Infested_Duran, UnitType.Hero_Samir_Duran, UnitType.Hero_Sarah_Kerrigan, UnitType.Hero_Infested_Kerrigan },
            new []{ UnitType.Zerg_Zergling, UnitType.Zerg_Hydralisk, UnitType.Zerg_Drone, UnitType.Zerg_Defiler, UnitType.Zerg_Infested_Terran, UnitType.Hero_Unclean_One, UnitType.Hero_Hunter_Killer, UnitType.Hero_Devouring_One, UnitType.Zerg_Lurker },
            new []{ UnitType.Zerg_Queen, UnitType.Hero_Matriarch },
            new []{ UnitType.Zerg_Queen, UnitType.Hero_Matriarch },
            new []{ UnitType.Zerg_Defiler, UnitType.Hero_Unclean_One },
            new []{ UnitType.Zerg_Defiler, UnitType.Hero_Unclean_One },
            new []{ UnitType.Zerg_Defiler, UnitType.Hero_Unclean_One, UnitType.Hero_Infested_Kerrigan, UnitType.Hero_Infested_Duran },
            new []{ UnitType.Zerg_Queen, UnitType.Hero_Matriarch, UnitType.Hero_Infested_Kerrigan },
            new []{ UnitType.Zerg_Queen, UnitType.Hero_Matriarch },
            new []{ UnitType.Protoss_High_Templar, UnitType.Hero_Tassadar, UnitType.Hero_Infested_Kerrigan },
            new []{ UnitType.Protoss_High_Templar, UnitType.Hero_Tassadar },
            new []{ UnitType.Protoss_Arbiter, UnitType.Hero_Danimoth },
            new []{ UnitType.Protoss_Arbiter, UnitType.Hero_Danimoth },
            new []{ UnitType.Protoss_High_Templar },
            new []{ UnitType.Terran_Medic },
            new []{ UnitType.Protoss_Corsair, UnitType.Hero_Raszagal },
            Array.Empty<UnitType>(),
            new []{ UnitType.Protoss_Dark_Archon },
            new []{ UnitType.Protoss_Dark_Templar },
            new []{ UnitType.Protoss_Dark_Archon },
            new []{ UnitType.Terran_Medic },
            new []{ UnitType.Protoss_Dark_Archon },
            new []{ UnitType.Zerg_Hydralisk },
            Array.Empty<UnitType>(),
            new []{ UnitType.Terran_Medic },
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            new []{ UnitType.Terran_Ghost },
            Array.Empty<UnitType>()
        };

        /// <summary>
        /// Retrieves the race that is required to research or use the <see cref="TechType"/>.
        /// There is an exception where <seealso cref="UnitType.Hero_Infested_Kerrigan"/> can use <seealso cref="TechType.Psionic_Storm"/>.
        /// This does not apply to the behavior of this function.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns><see cref="Race"/> object indicating which race is designed to use this technology type.</returns>
        public static Race GetRace(this TechType techType)
        {
            return _techRaces[(int)techType];
        }

        /// <summary>
        /// Retrieves the mineral cost of researching this technology.
        /// </summary>
        /// <returns>Amount of minerals needed in order to research this technology.</returns>
        public static int MineralPrice(this TechType techType)
        {
            return _defaultMineralCost[(int)techType];
        }

        /// <summary>
        /// Retrieves the vespene gas cost of researching this technology.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>Amount of vespene gas needed in order to research this technology.</returns>
        public static int GasPrice(this TechType techType)
        {
            return MineralPrice(techType);
        }

        /// <summary>
        /// Retrieves the number of frames needed to research the tech type.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>The time, in frames, it will take for the research to complete.</returns>
        /// <remarks>
        /// <seealso cref="Unit.GetRemainingResearchTime"/>
        /// </remarks>
        public static int ResearchTime(this TechType techType)
        {
            return _defaultTimeCost[(int)techType];
        }

        /// <summary>
        /// Retrieves the amount of energy needed to use this {@link TechType} as an ability.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>Energy cost of the ability.</returns>
        /// <remarks>
        /// <seealso cref="Unit.GetEnergy"/>
        /// </remarks>
        public static int EnergyCost(this TechType techType)
        {
            return _defaultEnergyCost[(int)techType];
        }

        /// <summary>
        /// Retrieves the <see cref="UnitType"/> that can research this technology.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>
        /// The <see cref="UnitType"/> that is able to research the technology in the game.
        /// Returns <see cref="UnitType.None"/> if the technology/ability is either provided for free or never available.
        /// </returns>
        public static UnitType WhatResearches(this TechType techType)
        {
            return _whatResearches[(int)techType];
        }

        /// <summary>
        /// Retrieves the weapon that is attached to this tech type.
        /// A technology's <see cref="WeaponType"/> is used to indicate the range and behaviour of the ability when used by a <see cref="Unit"/>.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>
        /// The <see cref="WeaponType"/> containing information about the ability's behavior.
        /// Returns <see cref="WeaponType.None"/> if there is no corresponding WeaponType.
        /// </returns>
        public static WeaponType GetWeapon(this TechType techType)
        {
            return _techWeapons[(int)techType];
        }

        /// <summary>
        /// Checks if this ability can be used on other units.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>true if the ability can be used on other units, and false if it can not.</returns>
        public static bool TargetsUnit(this TechType techType)
        {
            return (_techTypeFlags[(int)techType] & TARG_UNIT) != 0;
        }

        /// <summary>
        /// Checks if this ability can be used on the terrain (ground).
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>true if the ability can be used on the terrain.</returns>
        public static bool TargetsPosition(this TechType techType)
        {
            return (_techTypeFlags[(int)techType] & TARG_POS) != 0;
        }

        /// <summary>
        /// Retrieves the set of all UnitTypes that are capable of using this ability.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>Set of <seealso cref="UnitType"/> that can use this ability when researched.</returns>
        public static ReadOnlyCollection<UnitType> WhatUses(this TechType techType)
        {
            return _techWhatUses[(int)techType].AsReadOnly();
        }

        /// <summary>
        /// Retrieves the <see cref="Order"/> that a Unit uses when using this ability.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>The <see cref="Order"/> representing the action a Unit uses to perform this ability</returns>
        public static Order GetOrder(this TechType techType)
        {
            return _techOrders[(int)techType];
        }

        /// <summary>
        /// Retrieves the <see cref="UnitType"/> required to research this technology.
        /// The required unit type must be a completed unit owned by the player researching the technology.
        /// </summary>
        /// <param name="techType">The tech type.</param>
        /// <returns>
        /// The <see cref="UnitType"/> that is needed to research this tech type.
        /// Returns <see cref="UnitType.None"/> if no unit is required to research this tech type.
        /// </returns>
        /// <remarks>
        /// <seealso cref="Player.CompletedUnitCount"/>.
        /// Since 4.1.2.
        /// </remarks>
        public static UnitType RequiredUnit(this TechType techType)
        {
            return techType == TechType.Lurker_Aspect ? UnitType.Zerg_Lair : UnitType.None;
        }
    }
}