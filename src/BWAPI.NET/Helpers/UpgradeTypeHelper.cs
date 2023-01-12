using System;
using System.Collections.ObjectModel;

namespace BWAPI.NET
{
    public static class UpgradeTypeHelper
    {
        private static readonly int[] _defaultMineralCostBase = { 100, 100, 150, 150, 150, 100, 150, 100, 100, 100, 100, 100, 100, 100, 100, 200, 150, 100, 200, 150, 100, 150, 200, 150, 200, 150, 150, 100, 200, 150, 150, 150, 150, 150, 150, 200, 200, 200, 150, 150, 150, 100, 200, 100, 150, 0, 0, 100, 100, 150, 150, 150, 150, 200, 100, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultMineralCostFactor = { 75, 75, 75, 75, 75, 75, 75, 75, 75, 50, 50, 50, 75, 50, 75, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultTimeCostBase = { 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 1500, 1500, 0, 2500, 2500, 2500, 2500, 2500, 2400, 2000, 2000, 1500, 1500, 1500, 1500, 2500, 2500, 2500, 2000, 2500, 2500, 2500, 2000, 2000, 2500, 2500, 2500, 1500, 2500, 0, 0, 2500, 2500, 2500, 2500, 2500, 2000, 2000, 2000, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultTimeCostFactor = { 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] _defaultMaxRepeats = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly UnitType[] _whatUpgrades = { UnitType.Terran_Engineering_Bay, UnitType.Terran_Armory, UnitType.Terran_Armory, UnitType.Zerg_Evolution_Chamber, UnitType.Zerg_Spire, UnitType.Protoss_Forge, UnitType.Protoss_Cybernetics_Core, UnitType.Terran_Engineering_Bay, UnitType.Terran_Armory, UnitType.Terran_Armory, UnitType.Zerg_Evolution_Chamber, UnitType.Zerg_Evolution_Chamber, UnitType.Zerg_Spire, UnitType.Protoss_Forge, UnitType.Protoss_Cybernetics_Core, UnitType.Protoss_Forge, UnitType.Terran_Academy, UnitType.Terran_Machine_Shop, UnitType.None, UnitType.Terran_Science_Facility, UnitType.Terran_Covert_Ops, UnitType.Terran_Covert_Ops, UnitType.Terran_Control_Tower, UnitType.Terran_Physics_Lab, UnitType.Zerg_Lair, UnitType.Zerg_Lair, UnitType.Zerg_Lair, UnitType.Zerg_Spawning_Pool, UnitType.Zerg_Spawning_Pool, UnitType.Zerg_Hydralisk_Den, UnitType.Zerg_Hydralisk_Den, UnitType.Zerg_Queens_Nest, UnitType.Zerg_Defiler_Mound, UnitType.Protoss_Cybernetics_Core, UnitType.Protoss_Citadel_of_Adun, UnitType.Protoss_Robotics_Support_Bay, UnitType.Protoss_Robotics_Support_Bay, UnitType.Protoss_Robotics_Support_Bay, UnitType.Protoss_Observatory, UnitType.Protoss_Observatory, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Fleet_Beacon, UnitType.Protoss_Fleet_Beacon, UnitType.Protoss_Fleet_Beacon, UnitType.Protoss_Arbiter_Tribunal, UnitType.None, UnitType.None, UnitType.Protoss_Fleet_Beacon, UnitType.None, UnitType.Protoss_Templar_Archives, UnitType.None, UnitType.Terran_Academy, UnitType.Zerg_Ultralisk_Cavern, UnitType.Zerg_Ultralisk_Cavern, UnitType.Terran_Machine_Shop, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None };
        private static readonly UnitType[][] _requirements =
        {
            new []{ UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.Zerg_Hive, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.Terran_Armory, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None },
            new []{ UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Zerg_Lair, UnitType.Zerg_Lair, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Fleet_Beacon, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Zerg_Lair, UnitType.Zerg_Lair, UnitType.Zerg_Lair, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Fleet_Beacon, UnitType.Protoss_Cybernetics_Core, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None },
            new []{ UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Zerg_Hive, UnitType.Zerg_Hive, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Fleet_Beacon, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Terran_Science_Facility, UnitType.Zerg_Hive, UnitType.Zerg_Hive, UnitType.Zerg_Hive, UnitType.Protoss_Templar_Archives, UnitType.Protoss_Fleet_Beacon, UnitType.Protoss_Cybernetics_Core, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None, UnitType.None }
        };
        private static readonly UnitType[] _infantryArmor = { UnitType.Terran_Marine, UnitType.Terran_Ghost, UnitType.Terran_SCV, UnitType.Hero_Gui_Montag, UnitType.Terran_Civilian, UnitType.Hero_Sarah_Kerrigan, UnitType.Hero_Jim_Raynor_Marine, UnitType.Terran_Firebat, UnitType.Terran_Medic, UnitType.Hero_Samir_Duran, UnitType.Hero_Alexei_Stukov };
        private static readonly UnitType[] _vehiclePlating = { UnitType.Terran_Vulture, UnitType.Terran_Goliath, UnitType.Terran_Siege_Tank_Tank_Mode, UnitType.Hero_Alan_Schezar, UnitType.Hero_Jim_Raynor_Vulture, UnitType.Hero_Edmund_Duke_Tank_Mode, UnitType.Hero_Edmund_Duke_Siege_Mode, UnitType.Terran_Siege_Tank_Siege_Mode };
        private static readonly UnitType[] _shipPlating = { UnitType.Terran_Wraith, UnitType.Terran_Science_Vessel, UnitType.Terran_Dropship, UnitType.Terran_Battlecruiser, UnitType.Hero_Tom_Kazansky, UnitType.Hero_Magellan, UnitType.Hero_Arcturus_Mengsk, UnitType.Hero_Hyperion, UnitType.Hero_Norad_II, UnitType.Terran_Valkyrie, UnitType.Hero_Gerard_DuGalle };
        private static readonly UnitType[] _carapace = { UnitType.Zerg_Larva, UnitType.Zerg_Egg, UnitType.Zerg_Zergling, UnitType.Zerg_Hydralisk, UnitType.Zerg_Ultralisk, UnitType.Zerg_Broodling, UnitType.Zerg_Drone, UnitType.Zerg_Defiler, UnitType.Hero_Torrasque, UnitType.Zerg_Infested_Terran, UnitType.Hero_Infested_Kerrigan, UnitType.Hero_Unclean_One, UnitType.Hero_Hunter_Killer, UnitType.Hero_Devouring_One, UnitType.Zerg_Cocoon, UnitType.Zerg_Lurker_Egg, UnitType.Zerg_Lurker, UnitType.Hero_Infested_Duran };
        private static readonly UnitType[] _flyerCarapace = { UnitType.Zerg_Overlord, UnitType.Zerg_Mutalisk, UnitType.Zerg_Guardian, UnitType.Zerg_Queen, UnitType.Zerg_Scourge, UnitType.Hero_Matriarch, UnitType.Hero_Kukulza_Mutalisk, UnitType.Hero_Kukulza_Guardian, UnitType.Hero_Yggdrasill, UnitType.Zerg_Devourer };
        private static readonly UnitType[] _protossArmor = { UnitType.Protoss_Dark_Templar, UnitType.Protoss_Dark_Archon, UnitType.Protoss_Probe, UnitType.Protoss_Zealot, UnitType.Protoss_Dragoon, UnitType.Protoss_High_Templar, UnitType.Protoss_Archon, UnitType.Hero_Dark_Templar, UnitType.Hero_Zeratul, UnitType.Hero_Tassadar_Zeratul_Archon, UnitType.Hero_Fenix_Zealot, UnitType.Hero_Fenix_Dragoon, UnitType.Hero_Tassadar, UnitType.Hero_Warbringer, UnitType.Protoss_Reaver, UnitType.Hero_Aldaris };
        private static readonly UnitType[] _protossPlating = { UnitType.Protoss_Corsair, UnitType.Protoss_Shuttle, UnitType.Protoss_Scout, UnitType.Protoss_Arbiter, UnitType.Protoss_Carrier, UnitType.Protoss_Interceptor, UnitType.Hero_Mojo, UnitType.Hero_Gantrithor, UnitType.Protoss_Observer, UnitType.Hero_Danimoth, UnitType.Hero_Artanis, UnitType.Hero_Raszagal };
        private static readonly UnitType[] _infantryWeapons = { UnitType.Terran_Marine, UnitType.Hero_Jim_Raynor_Marine, UnitType.Terran_Ghost, UnitType.Hero_Sarah_Kerrigan, UnitType.Terran_Firebat, UnitType.Hero_Gui_Montag, UnitType.Special_Wall_Flame_Trap, UnitType.Special_Right_Wall_Flame_Trap, UnitType.Hero_Samir_Duran, UnitType.Hero_Alexei_Stukov, UnitType.Hero_Infested_Duran };
        private static readonly UnitType[] _vehicleWeapons = { UnitType.Terran_Vulture, UnitType.Hero_Jim_Raynor_Vulture, UnitType.Terran_Goliath, UnitType.Hero_Alan_Schezar, UnitType.Terran_Siege_Tank_Tank_Mode, UnitType.Terran_Siege_Tank_Siege_Mode, UnitType.Hero_Edmund_Duke_Tank_Mode, UnitType.Hero_Edmund_Duke_Siege_Mode, UnitType.Special_Floor_Missile_Trap, UnitType.Special_Floor_Gun_Trap, UnitType.Special_Wall_Missile_Trap, UnitType.Special_Right_Wall_Missile_Trap };
        private static readonly UnitType[] _shipWeapons = { UnitType.Terran_Wraith, UnitType.Hero_Tom_Kazansky, UnitType.Terran_Battlecruiser, UnitType.Hero_Hyperion, UnitType.Hero_Norad_II, UnitType.Hero_Arcturus_Mengsk, UnitType.Hero_Gerard_DuGalle, UnitType.Terran_Valkyrie };
        private static readonly UnitType[] _zergMeleeAtk = { UnitType.Zerg_Zergling, UnitType.Hero_Devouring_One, UnitType.Hero_Infested_Kerrigan, UnitType.Zerg_Ultralisk, UnitType.Hero_Torrasque, UnitType.Zerg_Broodling };
        private static readonly UnitType[] _zergRangeAtk = { UnitType.Zerg_Hydralisk, UnitType.Hero_Hunter_Killer, UnitType.Zerg_Lurker };
        private static readonly UnitType[] _zergFlyerAtk = { UnitType.Zerg_Mutalisk, UnitType.Hero_Kukulza_Mutalisk, UnitType.Hero_Kukulza_Guardian, UnitType.Zerg_Guardian, UnitType.Zerg_Devourer };
        private static readonly UnitType[] _protossGrndWpn = { UnitType.Protoss_Zealot, UnitType.Hero_Fenix_Zealot, UnitType.Protoss_Dragoon, UnitType.Hero_Fenix_Dragoon, UnitType.Hero_Tassadar, UnitType.Hero_Aldaris, UnitType.Protoss_Archon, UnitType.Hero_Tassadar_Zeratul_Archon, UnitType.Hero_Dark_Templar, UnitType.Hero_Zeratul, UnitType.Protoss_Dark_Templar };
        private static readonly UnitType[] _protossAirWpn = { UnitType.Protoss_Scout, UnitType.Hero_Mojo, UnitType.Protoss_Arbiter, UnitType.Hero_Danimoth, UnitType.Protoss_Interceptor, UnitType.Protoss_Carrier, UnitType.Protoss_Corsair, UnitType.Hero_Artanis };
        private static readonly UnitType[] _shields = { UnitType.Protoss_Corsair, UnitType.Protoss_Dark_Templar, UnitType.Protoss_Dark_Archon, UnitType.Protoss_Probe, UnitType.Protoss_Zealot, UnitType.Protoss_Dragoon, UnitType.Protoss_High_Templar, UnitType.Protoss_Archon, UnitType.Protoss_Shuttle, UnitType.Protoss_Scout, UnitType.Protoss_Arbiter, UnitType.Protoss_Carrier, UnitType.Protoss_Interceptor, UnitType.Hero_Dark_Templar, UnitType.Hero_Zeratul, UnitType.Hero_Tassadar_Zeratul_Archon, UnitType.Hero_Fenix_Zealot, UnitType.Hero_Fenix_Dragoon, UnitType.Hero_Tassadar, UnitType.Hero_Mojo, UnitType.Hero_Warbringer, UnitType.Hero_Gantrithor, UnitType.Protoss_Reaver, UnitType.Protoss_Observer, UnitType.Hero_Danimoth, UnitType.Hero_Aldaris, UnitType.Hero_Artanis, UnitType.Hero_Raszagal };
        private static readonly UnitType[] _shells = { UnitType.Terran_Marine };
        private static readonly UnitType[] _ionThrusters = { UnitType.Terran_Vulture };
        private static readonly UnitType[] _titanReactor = { UnitType.Terran_Science_Vessel };
        private static readonly UnitType[] _ghostUpgrades = { UnitType.Terran_Ghost };
        private static readonly UnitType[] _apolloReactor = { UnitType.Terran_Wraith };
        private static readonly UnitType[] _colossusReactor = { UnitType.Terran_Battlecruiser };
        private static readonly UnitType[] _overlordUpgrades = { UnitType.Zerg_Overlord };
        private static readonly UnitType[] _zerglingUpgrades = { UnitType.Zerg_Zergling };
        private static readonly UnitType[] _hydraliskUpgrades = { UnitType.Zerg_Hydralisk };
        private static readonly UnitType[] _gameteMeiosis = { UnitType.Zerg_Queen };
        private static readonly UnitType[] _metasynapticNode = { UnitType.Zerg_Defiler };
        private static readonly UnitType[] _singularityCharge = { UnitType.Protoss_Dragoon };
        private static readonly UnitType[] _legEnhancements = { UnitType.Protoss_Zealot };
        private static readonly UnitType[] _reaverUpgrades = { UnitType.Protoss_Reaver };
        private static readonly UnitType[] _graviticDrive = { UnitType.Protoss_Shuttle };
        private static readonly UnitType[] _observerUpgrades = { UnitType.Protoss_Observer };
        private static readonly UnitType[] _khaydarinAmulet = { UnitType.Protoss_High_Templar };
        private static readonly UnitType[] _scoutUpgrades = { UnitType.Protoss_Scout };
        private static readonly UnitType[] _carrierCapacity = { UnitType.Protoss_Carrier };
        private static readonly UnitType[] _khaydarinCore = { UnitType.Protoss_Arbiter };
        private static readonly UnitType[] _argusJewel = { UnitType.Protoss_Corsair };
        private static readonly UnitType[] _argusTalisman = { UnitType.Protoss_Dark_Archon };
        private static readonly UnitType[] _caduceusReactor = { UnitType.Terran_Medic };
        private static readonly UnitType[] _ultraliskUpgrades = { UnitType.Zerg_Ultralisk };
        private static readonly UnitType[] _charonBoosters = { UnitType.Terran_Goliath };
        private static readonly UnitType[] _upgrade60 = { UnitType.Terran_Vulture_Spider_Mine, UnitType.Critter_Ursadon, UnitType.Critter_Scantid, UnitType.Critter_Rhynadon, UnitType.Critter_Ragnasaur, UnitType.Critter_Kakaru, UnitType.Critter_Bengalaas, UnitType.Special_Cargo_Ship, UnitType.Special_Mercenary_Gunship, UnitType.Terran_SCV, UnitType.Protoss_Probe, UnitType.Zerg_Drone, UnitType.Zerg_Infested_Terran, UnitType.Zerg_Scourge };
        private static readonly UnitType[][] _upgradeWhatUses =
        {
            _infantryArmor,
            _vehiclePlating,
            _shipPlating,
            _carapace,
            _flyerCarapace,
            _protossArmor,
            _protossPlating,
            _infantryWeapons,
            _vehicleWeapons,
            _shipWeapons,
            _zergMeleeAtk,
            _zergRangeAtk,
            _zergFlyerAtk,
            _protossGrndWpn,
            _protossAirWpn,
            _shields,
            _shells,
            _ionThrusters,
            Array.Empty<UnitType>(),
            _titanReactor,
            _ghostUpgrades,
            _ghostUpgrades,
            _apolloReactor,
            _colossusReactor,
            _overlordUpgrades,
            _overlordUpgrades,
            _overlordUpgrades,
            _zerglingUpgrades,
            _zerglingUpgrades,
            _hydraliskUpgrades,
            _hydraliskUpgrades,
            _gameteMeiosis,
            _metasynapticNode,
            _singularityCharge,
            _legEnhancements,
            _reaverUpgrades,
            _reaverUpgrades,
            _graviticDrive,
            _observerUpgrades,
            _observerUpgrades,
            _khaydarinAmulet,
            _scoutUpgrades,
            _scoutUpgrades,
            _carrierCapacity,
            _khaydarinCore,
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            _argusJewel,
            Array.Empty<UnitType>(),
            _argusTalisman,
            Array.Empty<UnitType>(),
            _caduceusReactor,
            _ultraliskUpgrades,
            _ultraliskUpgrades,
            _charonBoosters,
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>(),
            _upgrade60,
            Array.Empty<UnitType>(),
            Array.Empty<UnitType>()
        };
        private static readonly Race[] upgradeRaces = { Race.Terran, Race.Terran, Race.Terran, Race.Zerg, Race.Zerg, Race.Protoss, Race.Protoss, Race.Terran, Race.Terran, Race.Terran, Race.Zerg, Race.Zerg, Race.Zerg, Race.Protoss, Race.Protoss, Race.Protoss, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Terran, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Zerg, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.Protoss, Race.None, Race.None, Race.Protoss, Race.None, Race.Protoss, Race.None, Race.Terran, Race.Zerg, Race.Zerg, Race.Terran, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.None, Race.Unknown };

        /// <summary>
        /// Gets the race the upgrade is for.
        /// For example, UpgradeType.Terran_Infantry_Armor.getRace() will return {@link Race#Terran}.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static Race GetRace(this UpgradeType upgradeType)
        {
            return upgradeRaces[(int)upgradeType];
        }

        /// <summary>
        /// Gets the mineral price for the upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int MineralPrice(this UpgradeType upgradeType)
        {
            return MineralPrice(upgradeType, 1);
        }

        /// <summary>
        /// Gets the mineral price for the upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        /// <param name="level">The next upgrade level. Upgrades start at level 0.</param>
        public static int MineralPrice(this UpgradeType upgradeType, int level)
        {
            return _defaultMineralCostBase[(int)upgradeType] + Math.Max(0, level - 1) * MineralPriceFactor(upgradeType);
        }

        /// <summary>
        /// The amount that the mineral price increases for each additional upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int MineralPriceFactor(this UpgradeType upgradeType)
        {
            return _defaultMineralCostFactor[(int)upgradeType];
        }

        /// <summary>
        /// Gets the vespene gas price for the first upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int GasPrice(this UpgradeType upgradeType)
        {
            return MineralPrice(upgradeType);
        }

        /// <summary>
        /// Gets the vespene gas price for the upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        /// <param name="level">The next upgrade level. Upgrades start at level 0.</param>
        public static int GasPrice(this UpgradeType upgradeType, int level)
        {
            return MineralPrice(upgradeType, level);
        }

        /// <summary>
        /// Gets the amount that the vespene gas price increases for each additional upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int GasPriceFactor(this UpgradeType upgradeType)
        {
            return MineralPriceFactor(upgradeType);
        }

        /// <summary>
        /// Gets the number of frames needed to research the first upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int UpgradeTime(this UpgradeType upgradeType)
        {
            return UpgradeTime(upgradeType, 1);
        }

        /// <summary>
        /// Gets the number of frames needed to research the first upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        /// <param name="level">The next upgrade level. Upgrades start at level 0.</param>
        public static int UpgradeTime(this UpgradeType upgradeType, int level)
        {
            return _defaultTimeCostBase[(int)upgradeType] + Math.Max(0, level - 1) * UpgradeTimeFactor(upgradeType);
        }

        /// <summary>
        /// Gets the number of frames that the upgrade time increases for each additional upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int UpgradeTimeFactor(this UpgradeType upgradeType)
        {
            return _defaultTimeCostFactor[(int)upgradeType];
        }

        /// <summary>
        /// Gets the type of unit that researches the upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static UnitType WhatUpgrades(this UpgradeType upgradeType)
        {
            return _whatUpgrades[(int)upgradeType];
        }

        /// <summary>
        /// Gets the set of units that are affected by this upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static ReadOnlyCollection<UnitType> WhatUses(this UpgradeType upgradeType)
        {
            return _upgradeWhatUses[(int)upgradeType].AsReadOnly();
        }

        /// <summary>
        /// Gets the maximum number of times the upgrade can be researched.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static int MaxRepeats(this UpgradeType upgradeType)
        {
            return _defaultMaxRepeats[(int)upgradeType];
        }

        /// <summary>
        /// Returns the type of unit that is required for the upgrade. The player
        /// must have at least one of these units completed in order to start upgrading this upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        public static UnitType WhatsRequired(this UpgradeType upgradeType)
        {
            return WhatsRequired(upgradeType, 1);
        }

        /// <summary>
        /// Returns the type of unit that is required for the upgrade. The player
        /// must have at least one of these units completed in order to start upgrading this upgrade.
        /// </summary>
        /// <param name="upgradeType">The upgrade type.</param>
        /// <param name="level">The next upgrade level. Upgrades start at level 0.</param>
        public static UnitType WhatsRequired(this UpgradeType upgradeType, int level)
        {
            return level >= 1 && level <= 3 ? _requirements[level - 1][(int)upgradeType] : UnitType.None;
        }
    }
}