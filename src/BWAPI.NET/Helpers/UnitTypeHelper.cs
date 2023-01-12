using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BWAPI.NET
{
    public static partial class UnitTypeHelper
    {
        private static readonly int _tileWidth = 0;
        private static readonly int _tileHeight = 1;
        private static readonly int _left = 2;
        private static readonly int _up = 3;
        private static readonly int _right = 4;
        private static readonly int _down = 5;

        /// <summary>
        /// Retrieves the maximum unit width from the set of all units. Used
        /// internally to search through unit positions efficiently.
        /// </summary>
        public static int MaxUnitWidth()
        {
            return Enum.GetValues(typeof(UnitType)).Cast<UnitType>().Max(x => x.Width());
        }

        /**
        * Retrieves the maximum unit height from the set of all units. Used
        * internally to search through unit positions efficiently.
        *
        * @return The maximum height of all unit types, in pixels.
        */
        public static int MaxUnitHeight()
        {
            return Enum.GetValues(typeof(UnitType)).Cast<UnitType>().Max(x => x.Height());
        }

        /**
        * Retrieves the {@link Race} that the unit type belongs to.
        *
        * @return {@link Race} indicating the race that owns this unit type.
        * Returns {@link Race#None} indicating that the unit type does not belong to any particular race (a
        * critter for example).
        */
        public static Race GetRace(this UnitType unitType)
        {
            return _unitRace[(int)unitType];
        }

        /**
        * Obtains the source unit type that is used to build or train this unit type, as well as the
        * amount of them that are required.
        *
        * @return std#pair in which the first value is the {@link UnitType} that builds this unit type, and
        * the second value is the number of those types that are required (this value is 2 for @Archons, and 1 for all other types).
        * Returns pair({@link UnitType#None},0) If this unit type cannot be made by the player.
        */
        public static Pair<UnitType, int> WhatBuilds(this UnitType unitType)
        {
            // Retrieve the type
            UnitType type = _whatBuilds[(int)unitType];
            int count = 1;
            // Set count to 0 if there is no whatBuilds and 2 if it's an archon
            if (type == UnitType.None)
            {
                count = 0;
            }
            else if (unitType == UnitType.Protoss_Archon || unitType == UnitType.Protoss_Dark_Archon)
            {
                count = 2;
            }
            // Return the desired pair
            return new Pair<UnitType, int>(type, count);
        }

        /**
        * Retrieves the immediate technology tree requirements to make this unit type.
        *
        * @return Map containing a UnitType to number mapping of UnitTypes required.
        */
        public static ReadOnlyDictionary<UnitType, int> RequiredUnits(this UnitType unitType)
        {
            return _reqUnitsMap[(int)unitType].AsReadOnly();
        }

        /**
        * Identifies the required {@link TechType} in order to create certain units.
        * <p>
        * The only unit that requires a technology is the @Lurker, which needs @Lurker_Aspect.
        *
        * @return {@link TechType} indicating the technology that must be researched in order to create this
        * unit type.
        * Returns {@link TechType#None} If creating this unit type does not require a technology to be
        * researched.
        */
        public static TechType RequiredTech(this UnitType unitType)
        {
            return unitType == UnitType.Zerg_Lurker || unitType == UnitType.Zerg_Lurker_Egg ? TechType.Lurker_Aspect : TechType.None;
        }

        /**
        * Retrieves the cloaking technology associated with certain units.
        *
        * @return {@link TechType} referring to the cloaking technology that this unit type uses as an
        * ability.
        * Returns {@link TechType#None} If this unit type does not have an active cloak ability.
        */
        public static TechType CloakingTech(this UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Terran_Ghost:
                case UnitType.Hero_Alexei_Stukov:
                case UnitType.Hero_Infested_Duran:
                case UnitType.Hero_Infested_Kerrigan:
                case UnitType.Hero_Sarah_Kerrigan:
                case UnitType.Hero_Samir_Duran:
                    return TechType.Personnel_Cloaking;
                case UnitType.Terran_Wraith:
                case UnitType.Hero_Tom_Kazansky:
                    return TechType.Cloaking_Field;
                default:
                    return TechType.None;
            }
        }

        /**
        * Retrieves the set of abilities that this unit can use, provided it is available to you in
        * the game.
        *
        * @return List of TechTypes containing ability information.
        */
        public static ReadOnlyCollection<TechType> Abilities(this UnitType unitType)
        {
            return _unitTechs[(int)unitType].AsReadOnly();
        }

        /**
        * Retrieves the set of upgrades that this unit can use to enhance its fighting ability.
        *
        * @return List of UpgradeTypes containing upgrade types that will impact this unit type.
        */
        public static ReadOnlyCollection<UpgradeType> Upgrades(this UnitType unitType)
        {
            return _upgrades[(int)unitType].AsReadOnly();
        }

        /**
        * Retrieves the upgrade type used to increase the armor of this unit type. For each upgrade,
        * this unit type gains +1 additional armor.
        *
        * @return {@link UpgradeType} indicating the upgrade that increases this unit type's armor amount.
        */
        public static UpgradeType ArmorUpgrade(this UnitType unitType)
        {
            return _armorUpgrade[(int)unitType];
        }

        /**
        * Retrieves the default maximum amount of hit points that this unit type can have.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return Integer indicating the maximum amount of hit points for this unit type.
        */
        public static int MaxHitPoints(this UnitType unitType)
        {
            return _defaultMaxHP[(int)unitType];
        }

        /**
        * Retrieves the default maximum amount of shield points that this unit type can have.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return Integer indicating the maximum amount of shield points for this unit type.
        * Returns 0 if this unit type does not have shields.
        */
        public static int MaxShields(this UnitType unitType)
        {
            return _defaultMaxSP[(int)unitType];
        }

        /**
        * Retrieves the maximum amount of energy this unit type can have by default.
        *
        * @return Integer indicating the maximum amount of energy for this unit type.
        * Retunrs 0 ff this unit does not gain energy for abilities.
        */
        public static int MaxEnergy(this UnitType unitType)
        {
            return IsSpellcaster(unitType) ? IsHero(unitType) ? 250 : 200 : 0;
        }

        /**
        * Retrieves the default amount of armor that the unit type starts with, excluding upgrades.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return The amount of armor the unit type has.
        */
        public static int Armor(this UnitType unitType)
        {
            return _defaultArmorAmount[(int)unitType];
        }

        /**
        * Retrieves the default mineral price of purchasing the unit.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return Mineral cost of the unit.
        */
        public static int MineralPrice(this UnitType unitType)
        {
            return _defaultMineralCost[(int)unitType];
        }

        /**
        * Retrieves the default vespene gas price of purchasing the unit.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return Vespene gas cost of the unit.
        */
        public static int GasPrice(this UnitType unitType)
        {
            return _defaultGasCost[(int)unitType];
        }

        /**
        * Retrieves the default time, in frames, needed to train, morph, or build the unit.
        * <p>
        * This value may not necessarily match the value seen in the @UMS game type.
        *
        * @return Number of frames needed in order to build the unit.
        * @see Unit#getRemainingBuildTime
        */
        public static int BuildTime(this UnitType unitType)
        {
            return _defaultTimeCost[(int)unitType];
        }

        /**
        * Retrieves the amount of supply that this unit type will use when created. It will use the
        * supply pool that is appropriate for its Race.
        * <p>
        * In Starcraft programming, the managed supply values are double than what they appear
        * in the game. The reason for this is because @Zerglings use 0.5 visible supply.
        *
        * @return Integer containing the supply required to build this unit.
        * @see #supplyProvided
        * @see Player#supplyTotal
        * @see Player#supplyUsed
        */
        public static int SupplyRequired(this UnitType unitType)
        {
            return _unitSupplyRequired[(int)unitType];
        }

        /**
        * Retrieves the amount of supply that this unit type produces for its appropriate Race's
        * supply pool.
        * <p>
        * In Starcraft programming, the managed supply values are double than what they appear
        * in the game. The reason for this is because @Zerglings use 0.5 visible supply.
        *
        * @see #supplyRequired
        * @see Player#supplyTotal
        * @see Player#supplyUsed
        */
        public static int SupplyProvided(this UnitType unitType)
        {
            return _unitSupplyProvided[(int)unitType];
        }

        /**
        * Retrieves the amount of space required by this unit type to fit inside a @Bunker or @Transport.
        *
        * @return Amount of space required by this unit type for transport.
        * Returns 255 If this unit type can not be transported.
        * @see #spaceProvided
        */
        public static int SpaceRequired(this UnitType unitType)
        {
            return _unitSpaceRequired[(int)unitType];
        }

        /**
        * Retrieves the amount of space provided by this @Bunker or @Transport for unit
        * transportation.
        *
        * @return The number of slots provided by this unit type.
        * @see #spaceRequired
        */
        public static int SpaceProvided(this UnitType unitType)
        {
            return _unitSpaceProvided[(int)unitType];
        }

        /**
        * Retrieves the amount of score points awarded for constructing this unit type. This value is
        * used for calculating scores in the post-game score screen.
        *
        * @return Number of points awarded for constructing this unit type.
        * @see #destroyScore
        */
        public static int BuildScore(this UnitType unitType)
        {
            return _unitBuildScore[(int)unitType];
        }

        /**
        * Retrieves the amount of score points awarded for killing this unit type. This value is
        * used for calculating scores in the post-game score screen.
        *
        * @return Number of points awarded for killing this unit type.
        * @see #buildScore
        */
        public static int DestroyScore(this UnitType unitType)
        {
            return _unitDestroyScore[(int)unitType];
        }

        /**
        * Retrieves the UnitSizeType of this unit, which is used in calculations along with weapon
        * damage types to determine the amount of damage that will be dealt to this type.
        *
        * @return {@link UnitSizeType} indicating the conceptual size of the unit type.
        * @see WeaponType#damageType()
        */
        public static UnitSizeType Size(this UnitType unitType)
        {
            return _unitSize[(int)unitType];
        }

        /**
        * Retrieves the width of this unit type, in tiles. Used for determining the tile size of
        * structures.
        *
        * @return Width of this unit type, in tiles.
        */
        public static int TileWidth(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_tileWidth];
        }

        /**
        * Retrieves the height of this unit type, in tiles. Used for determining the tile size of
        * structures.
        *
        * @return Height of this unit type, in tiles.
        */
        public static int TileHeight(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_tileHeight];
        }

        /**
        * Retrieves the tile size of this unit type. Used for determining the tile size of
        * structures.
        *
        * @return {@link TilePosition} containing the width (x) and height (y) of the unit type, in tiles.
        */
        public static TilePosition TileSize(this UnitType unitType)
        {
            return new TilePosition(TileWidth(unitType), TileHeight(unitType));
        }

        /// <summary>
        /// Retrieves the distance from the center of the unit type to its left edge.
        /// </summary>
        /// <returns>Distance to this unit type's left edge from its center, in pixels.</returns>
        public static int DimensionLeft(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_left];
        }

        /// <summary>
        /// Retrieves the distance from the center of the unit type to its top edge.
        /// </summary>
        /// <returns>Distance to this unit type's top edge from its center, in pixels.</returns>
        public static int DimensionUp(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_up];
        }

        /// <summary>
        /// Retrieves the distance from the center of the unit type to its right edge.
        /// </summary>
        /// <returns>Distance to this unit type's right edge from its center, in pixels.</returns>
        public static int DimensionRight(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_right];
        }

        /// <summary>
        /// Retrieves the distance from the center of the unit type to its bottom edge.
        /// </summary>
        /// <returns>Distance to this unit type's bottom edge from its center, in pixels.</returns>
        public static int DimensionDown(this UnitType unitType)
        {
            return _unitDimensions[(int)unitType][_down];
        }

        /// <summary>
        /// A macro for retrieving the width of the unit type, which is calculated using dimensionLeft + dimensionRight + 1.
        /// </summary>
        /// <returns>Width of the unit, in pixels.</returns>
        public static int Width(this UnitType unitType)
        {
            return DimensionLeft(unitType) + 1 + DimensionRight(unitType);
        }

        /// <summary>
        /// A macro for retrieving the height of the unit type, which is calculated using dimensionUp + dimensionDown + 1.
        /// </summary>
        /// <returns>Height of the unit, in pixels.</returns>
        public static int Height(this UnitType unitType)
        {
            return DimensionUp(unitType) + 1 + DimensionDown(unitType);
        }

        /**
        * Retrieves the range at which this unit type will start targeting enemy units.
        *
        * @return Distance at which this unit type begins to seek out enemy units, in pixels.
        */
        public static int SeekRange(this UnitType unitType)
        {
            return _seekRangeTiles[(int)unitType] * 32;
        }

        /**
        * Retrieves the sight range of this unit type.
        *
        * @return Sight range of this unit type, measured in pixels.
        */
        public static int SightRange(this UnitType unitType)
        {
            return _sightRangeTiles[(int)unitType] * 32;
        }

        /**
        * Retrieves this unit type's weapon type used when attacking targets on the ground.
        *
        * @return {@link WeaponType} used as this unit type's ground weapon.
        * @see #maxGroundHits
        * @see #airWeapon
        */
        public static WeaponType GroundWeapon(this UnitType unitType)
        {
            return _groundWeapon[(int)unitType];
        }

        /**
        * Retrieves the maximum number of hits this unit can deal to a ground target using its
        * ground weapon. This value is multiplied by the ground weapon's damage to calculate the
        * unit type's damage potential.
        *
        * @return Maximum number of hits given to ground targets.
        * @see #groundWeapon
        * @see #maxAirHits
        */
        public static int MaxGroundHits(this UnitType unitType)
        {
            return _groundWeaponHits[(int)unitType];
        }

        /**
        * Retrieves this unit type's weapon type used when attacking targets in the air.
        *
        * @return WeaponType used as this unit type's air weapon.
        * @see #maxAirHits
        * @see #groundWeapon
        */
        public static WeaponType AirWeapon(this UnitType unitType)
        {
            return _airWeapon[(int)unitType];
        }

        /**
        * Retrieves the maximum number of hits this unit can deal to a flying target using its
        * air weapon. This value is multiplied by the air weapon's damage to calculate the
        * unit type's damage potential.
        *
        * @return Maximum number of hits given to air targets.
        * @see #airWeapon
        * @see #maxGroundHits
        */
        public static int MaxAirHits(this UnitType unitType)
        {
            return _airWeaponHits[(int)unitType];
        }

        /**
        * Retrieves this unit type's top movement speed with no upgrades.
        * <p>
        * That some units have inconsistent movement and this value is sometimes an
        * approximation.
        *
        * @return The approximate top speed, in pixels per frame, as a double. For liftable @Terran
        * structures, this function returns their movement speed while lifted.
        */
        public static double TopSpeed(this UnitType unitType)
        {
            return _unitTopSpeeds[(int)unitType];
        }

        /**
        * Retrieves the unit's acceleration amount.
        *
        * @return How fast the unit can accelerate to its top speed.
        */
        public static int Acceleration(this UnitType unitType)
        {
            return _unitAcceleration[(int)unitType];
        }

        /**
        * Retrieves the unit's halting distance. This determines how fast a unit
        * can stop moving.
        *
        * @return A halting distance value.
        */
        public static int HaltDistance(this UnitType unitType)
        {
            return _unitHaltDistance[(int)unitType];
        }

        /**
        * Retrieves a unit's turning radius. This determines how fast a unit can
        * turn.
        *
        * @return A turn radius value.
        */
        public static int TurnRadius(this UnitType unitType)
        {
            return _unitTurnRadius[(int)unitType];
        }

        /**
        * Determines if a unit can train other units. For example,
        * UnitType.Terran_Barracks.canProduce() will return true, while
        * UnitType.Terran_Marine.canProduce() will return false. This is also true for two
        * non-structures: @Carrier (can produce interceptors) and @Reaver (can produce scarabs).
        *
        * @return true if this unit type can have a production queue, and false otherwise.
        */
        public static bool CanProduce(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _producesUnits) != 0;
        }

        /**
        * Checks if this unit is capable of attacking.
        * <p>
        * This function returns false for units that can only inflict damage via special
        * abilities, such as the @High_Templar.
        *
        * @return true if this unit type is capable of damaging other units with a standard attack,
        * and false otherwise.
        */
        public static bool CanAttack(this UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Protoss_Carrier:
                case UnitType.Hero_Gantrithor:
                case UnitType.Protoss_Reaver:
                case UnitType.Hero_Warbringer:
                case UnitType.Terran_Nuclear_Missile:
                    return true;
                case UnitType.Special_Independant_Starport:
                    return false;
                default:
                    return AirWeapon(unitType) != WeaponType.None || GroundWeapon(unitType) != WeaponType.None;
            }
        }

        /**
        * Checks if this unit type is capable of movement.
        * <p>
        * Buildings will return false, including @Terran liftable buildings which are capable
        * of moving when lifted.
        *
        * @return true if this unit can use a movement command, and false if they cannot move.
        */
        public static bool CanMove(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _autoAttackAndMove) != 0;
        }

        /**
        * Checks if this unit type is a flying unit. Flying units ignore ground pathing and
        * collisions.
        *
        * @return true if this unit type is in the air by default, and false otherwise.
             */
        public static bool IsFlyer(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _flyer) != 0;
        }

        /**
        * Checks if this unit type can regenerate hit points. This generally applies to @Zerg units.
        *
        * @return true if this unit type regenerates its hit points, and false otherwise.
        */
        public static bool RegeneratesHP(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _regeneratesHP) != 0;
        }

        /**
        * Checks if this unit type has the capacity to store energy and use it for special abilities.
        *
        * @return true if this unit type generates energy, and false if it does not have an energy
        * pool.
        */
        public static bool IsSpellcaster(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _spellcaster) != 0;
        }

        /**
        * Checks if this unit type is permanently cloaked. This means the unit type is always
        * cloaked and requires a detector in order to see it.
        *
        * @return true if this unit type is permanently cloaked, and false otherwise.
        */
        public static bool HasPermanentCloak(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _permanentCloak) != 0;
        }

        /**
        * Checks if this unit type is invincible by default. Invincible units
        * cannot take damage.
        *
        * @return true if this unit type is invincible, and false if it is vulnerable to attacks.
        */
        public static bool IsInvincible(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _invincible) != 0;
        }

        /**
        * Checks if this unit is an organic unit. The organic property is required for some abilities
        * such as @Heal.
        *
        * @return true if this unit type has the organic property, and false otherwise.
        */
        public static bool IsOrganic(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _organicUnit) != 0;
        }

        /**
        * Checks if this unit is mechanical. The mechanical property is required for some actions
        * such as @Repair.
        *
        * @return true if this unit type has the mechanical property, and false otherwise.
        */
        public static bool IsMechanical(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _mechanical) != 0;
        }

        /**
        * Checks if this unit is robotic. The robotic property is applied
        * to robotic units such as the @Probe which prevents them from taking damage from @Irradiate.
        *
        * @return true if this unit type has the robotic property, and false otherwise.
        */
        public static bool IsRobotic(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _roboticUnit) != 0;
        }

        /**
        * Checks if this unit type is capable of detecting units that are cloaked or burrowed.
        *
        * @return true if this unit type is a detector by default, false if it does not have this
        * property
        */
        public static bool IsDetector(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _detector) != 0;
        }

        /**
        * Checks if this unit type is capable of storing resources such as @minerals. Resources
        * are harvested from resource containers.
        *
        * @return true if this unit type may contain resources that can be harvested, false
        * otherwise.
        */
        public static bool IsResourceContainer(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _resourceContainer) != 0;
        }

        /**
        * Checks if this unit type is a resource depot. Resource depots must be placed a certain
        * distance from resources. Resource depots are typically the main building for any
        * particular race. Workers will return resources to the nearest resource depot.
        *
        * @return true if the unit type is a resource depot, false if it is not.
        */
        public static bool IsResourceDepot(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _resourceDepot) != 0;
        }

        /**
        * Checks if this unit type is a refinery. A refinery is a structure that is placed on top of
        * a @geyser . Refinery types are @refinery , @extractor , and @assimilator.
        *
        * @return true if this unit type is a refinery, and false if it is not.
        */
        public static bool IsRefinery(this UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Terran_Refinery:
                case UnitType.Zerg_Extractor:
                case UnitType.Protoss_Assimilator:
                    return true;
                default:
                    return false;
            }
        }

        /**
        * Checks if this unit type is a worker unit. Worker units can harvest resources and build
        * structures. Worker unit types include the @SCV , @probe, and @drone.
        *
        * @return true if this unit type is a worker, and false if it is not.
        */
        public static bool IsWorker(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _worker) != 0;
        }

        /**
        * Checks if this structure is powered by a psi field. Structures powered
        * by psi can only be placed near a @Pylon. If the @Pylon is destroyed, then this unit will
        * lose power.
        *
        * @return true if this unit type can only be placed in a psi field, false otherwise.
        */
        public static bool RequiresPsi(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _requiresPsi) != 0;
        }

        /**
        * Checks if this structure must be placed on @Zerg creep.
        *
        * @return true if this unit type requires creep, false otherwise.
        */
        public static bool RequiresCreep(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _creepBuilding) != 0;
        }

        /**
        * Checks if this unit type spawns two units when being hatched from an @Egg.
        * This is only applicable to @Zerglings and @Scourges.
        *
        * @return true if morphing this unit type will spawn two of them, and false if only one
        * is spawned.
        */
        public static bool IsTwoUnitsInOneEgg(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _twoUnitsIn1Egg) != 0;
        }

        /**
        * Checks if this unit type has the capability to use the @Burrow technology when it
        * is researched.
        * <p>
        * The @Lurker can burrow even without researching the ability.
        *
        * @return true if this unit can use the @Burrow ability, and false otherwise.
        * @see TechType#Burrowing
        */
        public static bool IsBurrowable(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _burrowable) != 0;
        }

        /**
        * Checks if this unit type has the capability to use a cloaking ability when it
        * is researched. This applies only to @Wraiths and @Ghosts, and does not include
        * units which are permanently cloaked.
        *
        * @return true if this unit has a cloaking ability, false otherwise.
        * @see #hasPermanentCloak
        * @see TechType#Cloaking_Field
        * @see TechType#Personnel_Cloaking
        */
        public static bool IsCloakable(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _cloakable) != 0;
        }

        /**
        * Checks if this unit is a structure. This includes @Mineral_Fields and @Vespene_Geysers.
        *
        * @return true if this unit is a building, and false otherwise.
        */
        public static bool IsBuilding(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _building) != 0;
        }

        /**
        * Checks if this unit is an add-on. Add-ons are attachments used by some @Terran structures such as the @Comsat_Station.
        *
        * @return true if this unit is an add-on, and false otherwise.
        */
        public static bool IsAddon(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _addon) != 0;
        }

        /**
        * Checks if this structure has the capability to use the lift-off command.
        *
        * @return true if this unit type is a flyable building, false otherwise.
        */
        public static bool IsFlyingBuilding(this UnitType unitType)
        {
            return (_unitFlags[(int)unitType] & _flyingBuilding) != 0;
        }

        /**
        * Checks if this unit type is a neutral type, such as critters and resources.
        *
        * @return true if this unit is intended to be neutral, and false otherwise.
        */
        public static bool IsNeutral(this UnitType unitType)
        {
            return GetRace(unitType) == Race.None && (IsCritter(unitType) || IsResourceContainer(unitType) || IsSpell(unitType));
        }

        /**
        * Checks if this unit type is a hero. Heroes are types that the player
        * cannot obtain normally, and are identified by the white border around their icon when
        * selected with a group.
        * <p>
        * There are two non-hero units included in this set, the @Civilian and @Dark_Templar_Hero.
        *
        * @return true if this unit type is a hero type, and false otherwise.
        */
        public static bool IsHero(this UnitType unitType)
        {
            return ((_unitFlags[(int)unitType] & _hero) != 0) || unitType == UnitType.Hero_Dark_Templar || unitType == UnitType.Terran_Civilian;
        }

        /**
        * Checks if this unit type is a powerup. Powerups can be picked up and
        * carried by workers. They are usually only seen in campaign maps and @Capture_the_flag.
        *
        * @return true if this unit type is a powerup type, and false otherwise.
        */
        public static bool IsPowerup(this UnitType unitType)
        {
            return unitType == UnitType.Powerup_Uraj_Crystal || unitType == UnitType.Powerup_Khalis_Crystal || (unitType >= UnitType.Powerup_Flag && unitType < UnitType.None);
        }

        /**
        * Checks if this unit type is a beacon. Each race has exactly one beacon
        * each. They are {@link UnitType#Special_Zerg_Beacon}, {@link UnitType#Special_Terran_Beacon}, and
        * {@link UnitType#Special_Protoss_Beacon}.
        *
        * @return true if this unit type is one of the three race beacons, and false otherwise.
        * @see #isFlagBeacon
        */
        public static bool IsBeacon(this UnitType unitType)
        {
            return unitType == UnitType.Special_Zerg_Beacon || unitType == UnitType.Special_Terran_Beacon || unitType == UnitType.Special_Protoss_Beacon;
        }

        /**
        * Checks if this unit type is a flag beacon. Each race has exactly one
        * flag beacon each. They are {@link UnitType#Special_Zerg_Flag_Beacon},
        * {@link UnitType#Special_Terran_Flag_Beacon}, and {@link UnitType#Special_Protoss_Flag_Beacon}.
        * Flag beacons spawn a @Flag after some ARBITRARY I FORGOT AMOUNT OF FRAMES.
        *
        * @return true if this unit type is one of the three race flag beacons, and false otherwise.
        * @see #isBeacon
        */
        public static bool IsFlagBeacon(this UnitType unitType)
        {
            return unitType == UnitType.Special_Zerg_Flag_Beacon ||
                   unitType == UnitType.Special_Terran_Flag_Beacon ||
                   unitType == UnitType.Special_Protoss_Flag_Beacon;
        }

        /**
        * Checks if this structure is special and cannot be obtained normally within the
        * game.
        *
        * @return true if this structure is a special building, and false otherwise.
        */
        public static bool IsSpecialBuilding(this UnitType unitType)
        {
            return IsBuilding(unitType) && WhatBuilds(unitType).GetValue() == 0 && unitType != UnitType.Zerg_Infested_Command_Center;
        }

        /**
        * Identifies if this unit type is used to complement some @abilities.
        * These include {@link UnitType#Spell_Dark_Swarm}, {@link UnitType#Spell_Disruption_Web}, and
        * {@link UnitType#Spell_Scanner_Sweep}, which correspond to {@link TechType#Dark_Swarm},
        * {@link TechType#Disruption_Web}, and {@link TechType#Scanner_Sweep} respectively.
        *
        * @return true if this unit type is used for an ability, and false otherwise.
        */
        public static bool IsSpell(this UnitType unitType)
        {
            return unitType == UnitType.Spell_Dark_Swarm ||
                   unitType == UnitType.Spell_Disruption_Web ||
                   unitType == UnitType.Spell_Scanner_Sweep;
        }

        /**
        * Checks if this structure type produces creep. That is, the unit type
        * spreads creep over a wide area so that @Zerg structures can be placed on it.
        *
        * @return true if this unit type spreads creep.
        * @since 4.1.2
        */
        public static bool ProducesCreep(this UnitType unitType)
        {
            return ProducesLarva(unitType) ||
                   unitType == UnitType.Zerg_Creep_Colony ||
                   unitType == UnitType.Zerg_Spore_Colony ||
                   unitType == UnitType.Zerg_Sunken_Colony;
        }

        /**
        * Checks if this unit type produces larva. This is essentially used to
        * check if the unit type is a @Hatchery, @Lair, or @Hive.
        *
        * @return true if this unit type produces larva.
        */
        public static bool ProducesLarva(this UnitType unitType)
        {
            return unitType == UnitType.Zerg_Hatchery ||
                   unitType == UnitType.Zerg_Lair ||
                   unitType == UnitType.Zerg_Hive;
        }

        /**
        * Checks if this unit type is a mineral field and contains a resource amount.
        * This indicates that the unit type is either {@link UnitType#Resource_Mineral_Field},
        * {@link UnitType#Resource_Mineral_Field_Type_2}, or {@link UnitType#Resource_Mineral_Field_Type_3}.
        *
        * @return true if this unit type is a mineral field resource.
        */
        public static bool IsMineralField(this UnitType unitType)
        {
            return unitType == UnitType.Resource_Mineral_Field ||
                   unitType == UnitType.Resource_Mineral_Field_Type_2 ||
                   unitType == UnitType.Resource_Mineral_Field_Type_3;
        }

        /**
        * Checks if this unit type is a neutral critter.
        *
        * @return true if this unit type is a critter, and false otherwise.
        */
        public static bool IsCritter(this UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Critter_Bengalaas:
                case UnitType.Critter_Kakaru:
                case UnitType.Critter_Ragnasaur:
                case UnitType.Critter_Rhynadon:
                case UnitType.Critter_Scantid:
                case UnitType.Critter_Ursadon:
                    return true;
                default:
                    return false;
            }
        }

        /**
             * Checks if this unit type is capable of constructing an add-on. An add-on is an extension
             * or attachment for <em>Terran</em> structures, specifically the <em>Command_Center</em>, <em>Factory</em>,
             * <em>Starport</em>, and <em>Science_Facility</em>.
             *
             * @return true if this unit type can construct an add-on, and false if it can not.
             * @see #isAddon
             */
        public static bool CanBuildAddon(this UnitType unitType)
        {
            return unitType == UnitType.Terran_Command_Center ||
                   unitType == UnitType.Terran_Factory ||
                   unitType == UnitType.Terran_Starport ||
                   unitType == UnitType.Terran_Science_Facility;
        }

        /**
        * Retrieves the set of units that this unit type is capable of creating.
        * This includes training, constructing, warping, and morphing.
        * <p>
        * Some maps have special parameters that disable construction of units that are otherwise
        * normally available. Use {@link Player#isUnitAvailable} to determine if a unit type is
        * actually available in the current game for a specific player.
        *
        * @return List of UnitTypes containing the units it can build.
        * @see Player#isUnitAvailable
        * @since 4.1.2
        */
        public static ReadOnlyCollection<UnitType> BuildsWhat(this UnitType unitType)
        {
            return _buildsWhat[(int)unitType].AsReadOnly();
        }

        /**
         * Retrieves the set of technologies that this unit type is capable of researching.
         * <p>
         * Some maps have special parameters that disable certain technologies. Use
         * {@link Player#isResearchAvailable} to determine if a technology is actually available in the
         * current game for a specific player.
         *
         * @return List of TechTypes containing the technology types that can be researched.
         * @see Player#isResearchAvailable
         * @since 4.1.2
         */
        public static ReadOnlyCollection<TechType> ResearchesWhat(this UnitType unitType)
        {
            return _researchesWhat[(int)unitType].AsReadOnly();
        }

        /**
        * Retrieves the set of upgrades that this unit type is capable of upgrading.
        * <p>
        * Some maps have special upgrade limitations. Use {@link Player#getMaxUpgradeLevel}
        * to check if an upgrade is available.
        *
        * @return List of UpgradeTypes containing the upgrade types that can be upgraded.
        * @see Player#getMaxUpgradeLevel
        * @since 4.1.2
        */
        public static ReadOnlyCollection<UpgradeType> UpgradesWhat(this UnitType unitType)
        {
            return _upgradesWhat[(int)unitType].AsReadOnly();
        }

        /**
        * Checks if the current type is equal to the provided type, or a successor of the
        * provided type. For example, a Hive is a successor of a Hatchery, since it can
        * still research the @Burrow technology.
        *
        * @param type The unit type to check.
        * @see TechType#whatResearches()
        * @see UpgradeType#whatUpgrades()
        * @since 4.2.0
        */
        public static bool IsSuccessorOf(this UnitType @this, UnitType type)
        {
            if (@this == type)
            {
                return true;
            }

            switch (type)
            {
                case UnitType.Zerg_Hatchery:
                    return @this == UnitType.Zerg_Lair || @this == UnitType.Zerg_Hive;
                case UnitType.Zerg_Lair:
                    return @this == UnitType.Zerg_Hive;
                case UnitType.Zerg_Spire:
                    return @this == UnitType.Zerg_Greater_Spire;
                default:
                    return false;
            }
        }
    }
}