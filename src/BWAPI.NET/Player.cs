using System;
using System.Collections.Generic;
using System.Linq;

namespace BWAPI.NET
{
    /// <summary>
    /// The Player represents a unique controller in the game. Each player in
    /// a match will have his or her own player instance. There is also a neutral player which owns
    /// all the neutral units (such as mineral patches and vespene geysers).
    /// </summary>
    /// <remarks>
    /// @seePlayerType
    /// @seeRace
    /// </remarks>
    public class Player : IEquatable<Player>, IComparable<Player>
    {
        private readonly int _id;
        private readonly string _name;
        private readonly ClientData.PlayerData _playerData;
        private readonly Game _game;
        private readonly PlayerType _playerType;
        private readonly TilePosition _startLocation;
        private readonly Color _color;

        private PlayerSelf self = null;

        public PlayerSelf Self()
        {
            return self ??= new PlayerSelf();
        }

        public Player(int id, ClientData.PlayerData playerData, Game game)
        {
            _playerData = playerData;
            _game = game;
            _id = id;
            _name = playerData.GetName();
            _playerType = playerData.GetPlayerType();
            _startLocation = new TilePosition(playerData.GetStartLocationX(), playerData.GetStartLocationY());
            _color = new Color(playerData.GetColor());
        }

        /// <summary>
        /// Retrieves a unique ID that represents the player.
        /// </summary>
        /// <returns>An integer representing the ID of the player.</returns>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// Retrieves the name of the player.
        /// </summary>
        /// <returns>A String object containing the player's name.</returns>
        public string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Retrieves the set of all units that the player owns. This also includes
        /// incomplete units.
        /// </summary>
        /// <returns>Reference to a List<Unit> containing the units.
        /// <p>
        /// This does not include units that are loaded into transports, @Bunkers, @Refineries, @Assimilators, or @Extractors.</returns>
        public List<Unit> GetUnits()
        {
            return _game.GetAllUnits().Where(x => Equals(x.GetPlayer())).ToList();
        }

        /// <summary>
        /// Retrieves the race of the player. This allows you to change strategies
        /// against different races, or generalize some commands for yourself.
        /// </summary>
        /// <returns>The Race that the player is using.
        /// Returns {@link Race#Unknown} if the player chose {@link Race#Random} when the game started and they
        /// have not been seen.</returns>
        public Race GetRace()
        {
            return _playerData.GetRace();
        }

        /// <summary>
        /// Retrieves the player's controller type. This allows you to distinguish
        /// betweeen computer and human players.
        /// </summary>
        /// <returns>The {@link PlayerType} that identifies who is controlling a player.
        /// <p>
        /// Other players using BWAPI will be treated as a human player and return {@link PlayerType#Player}.</returns>
        public PlayerType GetPlayerType()
        {
            return _playerType;
        }

        /// <summary>
        /// Retrieves the player's force. A force is the team that the player is
        /// playing on.
        /// </summary>
        /// <returns>The {@link Force} object that the player is part of.</returns>
        public Force GetForce()
        {
            return _game.GetForce(_playerData.GetForce());
        }

        /// <summary>
        /// Checks if this player is allied to the specified player.
        /// </summary>
        /// <param name="player">The player to check alliance with.
        ///               Returns true if this player is allied with player, false if this player is not allied with player.
        ///               <p>
        ///               This function will also return false if this player is neutral or an observer, or
        ///               if player is neutral or an observer.</param>
        /// <remarks>@see#isEnemy</remarks>
        public bool IsAlly(Player player)
        {
            return player != null && !IsNeutral() && !player.IsNeutral() && !IsObserver() && !player.IsObserver() && _playerData.IsAlly(player.GetID());
        }

        /// <summary>
        /// Checks if this player is unallied to the specified player.
        /// </summary>
        /// <param name="player">The player to check alliance with.</param>
        /// <returns>true if this player is allied with player, false if this player is not allied with player .
        /// <p>
        /// This function will also return false if this player is neutral or an observer, or if
        /// player is neutral or an observer.</returns>
        /// <remarks>@see#isAlly</remarks>
        public bool IsEnemy(Player player)
        {
            return player != null && !IsNeutral() && !player.IsNeutral() && !IsObserver() && !player.IsObserver() && !_playerData.IsAlly(player.GetID());
        }

        /// <summary>
        /// Checks if this player is the neutral player.
        /// </summary>
        /// <returns>true if this player is the neutral player, false if this player is any other player.</returns>
        public bool IsNeutral()
        {
            return Equals(_game.Neutral());
        }

        /// <summary>
        /// Retrieve's the player's starting location.
        /// </summary>
        /// <returns>A {@link TilePosition} containing the position of the start location.
        /// Returns {@link TilePosition#None} if the player does not have a start location.
        /// Returns {@link TilePosition#Unknown} if an error occured while trying to retrieve the start
        /// location.</returns>
        /// <remarks>@seeGame#getStartLocations</remarks>
        public TilePosition GetStartLocation()
        {
            return _startLocation;
        }

        /// <summary>
        /// Checks if the player has achieved victory.
        /// </summary>
        /// <returns>true if this player has achieved victory, otherwise false</returns>
        public bool IsVictorious()
        {
            return _playerData.IsVictorious();
        }

        /// <summary>
        /// Checks if the player has been defeated.
        /// </summary>
        /// <returns>true if the player is defeated, otherwise false</returns>
        public bool IsDefeated()
        {
            return _playerData.IsDefeated();
        }

        /// <summary>
        /// Checks if the player has left the game.
        /// </summary>
        /// <returns>true if the player has left the game, otherwise false</returns>
        public bool LeftGame()
        {
            return _playerData.GetLeftGame();
        }

        /// <summary>
        /// Retrieves the current amount of minerals/ore that this player has.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Amount of minerals that the player currently has for spending.</returns>
        public int Minerals()
        {
            var minerals = _playerData.GetMinerals();
            if (_game.IsLatComEnabled() && Self().minerals.Valid(_game.GetFrameCount()))
            {
                return minerals + Self().minerals.Get();
            }

            return minerals;
        }

        /// <summary>
        /// Retrieves the current amount of vespene gas that this player has.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Amount of gas that the player currently has for spending.</returns>
        public int Gas()
        {
            var gas = _playerData.GetGas();
            if (_game.IsLatComEnabled() && Self().gas.Valid(_game.GetFrameCount()))
            {
                return gas + Self().gas.Get();
            }

            return gas;
        }

        /// <summary>
        /// Retrieves the cumulative amount of minerals/ore that this player has gathered
        /// since the beginning of the game, including the amount that the player starts the game
        /// with (if any).
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of minerals that the player has gathered.</returns>
        public int GatheredMinerals()
        {
            return _playerData.GetGatheredMinerals();
        }

        /// <summary>
        /// Retrieves the cumulative amount of vespene gas that this player has gathered since
        /// the beginning of the game, including the amount that the player starts the game with (if
        /// any).
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of gas that the player has gathered.</returns>
        public int GatheredGas()
        {
            return _playerData.GetGatheredGas();
        }

        /// <summary>
        /// Retrieves the cumulative amount of minerals/ore that this player has spent on
        /// repairing units since the beginning of the game. This function only applies to @Terran players.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of minerals that the player has spent repairing.</returns>
        public int RepairedMinerals()
        {
            return _playerData.GetRepairedMinerals();
        }

        /// <summary>
        /// Retrieves the cumulative amount of vespene gas that this player has spent on
        /// repairing units since the beginning of the game. This function only applies to @Terran players.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of gas that the player has spent repairing.</returns>
        public int RepairedGas()
        {
            return _playerData.GetRepairedGas();
        }

        /// <summary>
        /// Retrieves the cumulative amount of minerals/ore that this player has gained from
        /// refunding (cancelling) units and structures.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of minerals that the player has received from refunds.</returns>
        public int RefundedMinerals()
        {
            return _playerData.GetRefundedMinerals();
        }

        /// <summary>
        /// Retrieves the cumulative amount of vespene gas that this player has gained from
        /// refunding (cancelling) units and structures.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of gas that the player has received from refunds.</returns>
        public int RefundedGas()
        {
            return _playerData.GetRefundedGas();
        }

        /// <summary>
        /// Retrieves the cumulative amount of minerals/ore that this player has spent,
        /// excluding repairs.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of minerals that the player has spent.</returns>
        public int SpentMinerals()
        {
            return GatheredMinerals() + RefundedMinerals() - Minerals() - RepairedMinerals();
        }

        /// <summary>
        /// Retrieves the cumulative amount of vespene gas that this player has spent,
        /// excluding repairs.
        /// <p>
        /// This function will return 0 if the player is inaccessible.
        /// </summary>
        /// <returns>Cumulative amount of gas that the player has spent.</returns>
        public int SpentGas()
        {
            return GatheredGas() + RefundedGas() - Gas() - RepairedGas();
        }

        public int SupplyTotal()
        {
            return SupplyTotal(GetRace());
        }

        /// <summary>
        /// Retrieves the total amount of supply the player has available for unit control.
        /// <p>
        /// In Starcraft programming, the managed supply values are double than what they appear
        /// in the game. The reason for this is because @Zerglings use 0.5 visible supply.
        /// <p>
        /// In Starcraft, the supply for each race is separate. Having a @Pylon and an @Overlord
        /// will not give you 32 supply. It will instead give you 16 @Protoss supply and 16 @Zerg
        /// supply.
        /// </summary>
        /// <param name="race">The race to query the total supply for. If this is omitted, then the player's current race will be used.</param>
        /// <returns>The total supply available for this player and the given race.</returns>
        /// <remarks>@see#supplyUsed</remarks>
        public int SupplyTotal(Race race)
        {
            return _playerData.GetSupplyTotal(race);
        }

        public int SupplyUsed()
        {
            return SupplyUsed(GetRace());
        }

        /// <summary>
        /// Retrieves the current amount of supply that the player is using for unit control.
        /// </summary>
        /// <param name="race">The race to query the used supply for. If this is omitted, then the player's current race will be used.</param>
        /// <returns>The supply that is in use for this player and the given race.</returns>
        /// <remarks>@see#supplyTotal</remarks>
        public int SupplyUsed(Race race)
        {
            var supplyUsed = _playerData.GetSupplyUsed(race);
            if (_game.IsLatComEnabled() && Self().supplyUsed[(int)race].Valid(_game.GetFrameCount()))
            {
                return supplyUsed + Self().supplyUsed[(int)race].Get();
            }

            return supplyUsed;
        }

        public int AllUnitCount()
        {
            return AllUnitCount(UnitType.AllUnits);
        }

        /// <summary>
        /// Retrieves the total number of units that the player has. If the
        /// information about the player is limited, then this function will only return the number
        /// of visible units.
        /// <p>
        /// While in-progress @Protoss and @Terran units will be counted, in-progress @Zerg units
        /// (i.e. inside of an egg) do not.
        /// </summary>
        /// <param name="unit">The unit type to query. UnitType macros are accepted. If this parameter is omitted, then it will use UnitType.AllUnits by default.</param>
        /// <returns>The total number of units of the given type that the player owns.</returns>
        /// <remarks>
        /// @see#visibleUnitCount
        /// @see#completedUnitCount
        /// @see#incompleteUnitCount
        /// </remarks>
        public int AllUnitCount(UnitType unit)
        {
            return _playerData.GetAllUnitCount(unit);
        }

        public int VisibleUnitCount()
        {
            return VisibleUnitCount(UnitType.AllUnits);
        }

        /// <summary>
        /// Retrieves the total number of strictly visible units that the player has, even if
        /// information on the player is unrestricted.
        /// </summary>
        /// <param name="unit">The unit type to query. UnitType macros are accepted. If this parameter is omitted, then it will use UnitType.AllUnits by default.</param>
        /// <returns>The total number of units of the given type that the player owns, and is visible
        /// to the BWAPI player.</returns>
        /// <remarks>
        /// @see#allUnitCount
        /// @see#completedUnitCount
        /// @see#incompleteUnitCount
        /// </remarks>
        public int VisibleUnitCount(UnitType unit)
        {
            return _playerData.GetVisibleUnitCount(unit);
        }

        public int CompletedUnitCount()
        {
            return CompletedUnitCount(UnitType.AllUnits);
        }

        /// <summary>
        /// Retrieves the number of completed units that the player has. If the
        /// information about the player is limited, then this function will only return the number of
        /// visible completed units.
        /// </summary>
        /// <param name="unit">The unit type to query. UnitType macros are accepted. If this parameter is omitted, then it will use UnitType.AllUnits by default.</param>
        /// <returns>The number of completed units of the given type that the player owns.</returns>
        /// <remarks>
        /// @see#allUnitCount
        /// @see#visibleUnitCount
        /// @see#incompleteUnitCount
        /// </remarks>
        public int CompletedUnitCount(UnitType unit)
        {
            return _playerData.GetCompletedUnitCount(unit);
        }

        public int IncompleteUnitCount()
        {
            return AllUnitCount() - CompletedUnitCount();
        }

        /// <summary>
        /// Retrieves the number of incomplete units that the player has. If the
        /// information about the player is limited, then this function will only return the number of
        /// visible incomplete units.
        /// <p>
        /// This function is a macro for allUnitCount() - completedUnitCount().
        /// <p>
        /// Incomplete @Zerg units inside of eggs are not counted.
        /// </summary>
        /// <param name="unit">The unit type to query. UnitType macros are accepted. If this parameter is omitted, then it will use UnitType.AllUnits by default.</param>
        /// <returns>The number of incomplete units of the given type that the player owns.</returns>
        /// <remarks>
        /// @see#allUnitCount
        /// @see#visibleUnitCount
        /// @see#completedUnitCount
        /// </remarks>
        public int IncompleteUnitCount(UnitType unit)
        {
            return AllUnitCount(unit) - CompletedUnitCount(unit);
        }

        public int DeadUnitCount()
        {
            return DeadUnitCount(UnitType.AllUnits);
        }

        /// <summary>
        /// Retrieves the number units that have died for this player.
        /// </summary>
        /// <param name="unit">The unit type to query. {@link UnitType} macros are accepted. If this parameter is omitted, then it will use {@link UnitType#AllUnits} by default.</param>
        /// <returns>The total number of units that have died throughout the game.</returns>
        public int DeadUnitCount(UnitType unit)
        {
            return _playerData.GetDeadUnitCount(unit);
        }

        public int KilledUnitCount()
        {
            return KilledUnitCount(UnitType.AllUnits);
        }

        /// <summary>
        /// Retrieves the number units that the player has killed.
        /// </summary>
        /// <param name="unit">The unit type to query. UnitType macros are accepted. If this parameter is omitted, then it will use {@link UnitType#AllUnits} by default.</param>
        /// <returns>The total number of units that the player has killed throughout the game.</returns>
        public int KilledUnitCount(UnitType unit)
        {
            return _playerData.GetKilledUnitCount(unit);
        }

        /// <summary>
        /// Retrieves the current upgrade level that the player has attained for a given
        /// upgrade type.
        /// </summary>
        /// <param name="upgrade">The UpgradeType to query.</param>
        /// <returns>The number of levels that the upgrade has been upgraded for this player.</returns>
        /// <remarks>
        /// @seeUnit#upgrade
        /// @see#getMaxUpgradeLevel
        /// </remarks>
        public int GetUpgradeLevel(UpgradeType upgrade)
        {
            return _playerData.GetUpgradeLevel(upgrade);
        }

        /// <summary>
        /// Checks if the player has already researched a given technology.
        /// </summary>
        /// <param name="tech">The {@link TechType} to query.</param>
        /// <returns>true if the player has obtained the given tech, or false if they have not</returns>
        /// <remarks>
        /// @see#isResearching
        /// @seeUnit#research
        /// @see#isResearchAvailable
        /// </remarks>
        public bool HasResearched(TechType tech)
        {
            return _playerData.GetHasResearched(tech);
        }

        /// <summary>
        /// Checks if the player is researching a given technology type.
        /// </summary>
        /// <param name="tech">The {@link TechType} to query.</param>
        /// <returns>true if the player is currently researching the tech, or false otherwise</returns>
        /// <remarks>
        /// @seeUnit#research
        /// @see#hasResearched
        /// </remarks>
        public bool IsResearching(TechType tech)
        {
            if (_game.IsLatComEnabled() && Self().isResearching[(int)tech].Valid(_game.GetFrameCount()))
            {
                return Self().isResearching[(int)tech].Get();
            }

            return _playerData.IsResearching(tech);
        }

        /// <summary>
        /// Checks if the player is upgrading a given upgrade type.
        /// </summary>
        /// <param name="upgrade">The upgrade type to query.</param>
        /// <returns>true if the player is currently upgrading the given upgrade, false otherwise</returns>
        /// <remarks>@seeUnit#upgrade</remarks>
        public bool IsUpgrading(UpgradeType upgrade)
        {
            if (_game.IsLatComEnabled() && Self().isUpgrading[(int)upgrade].Valid(_game.GetFrameCount()))
            {
                return Self().isUpgrading[(int)upgrade].Get();
            }

            return _playerData.IsUpgrading(upgrade);
        }

        /// <summary>
        /// Retrieves the color value of the current player.
        /// </summary>
        /// <returns>{@link Color} object that represents the color of the current player.</returns>
        public Color GetColor()
        {
            return _color;
        }

        /// <summary>
        /// Retrieves the control code character that changes the color of text messages to
        /// represent this player.
        /// </summary>
        /// <returns>character code to use for text in Broodwar.</returns>
        public Text GetTextColor()
        {
            return _color.id switch
            {
                111 => Text.BrightRed,
                165 => Text.Blue,
                159 => Text.Teal,
                164 => Text.Purple,
                156 => Text.Orange,
                19 => Text.Brown,
                84 => Text.PlayerWhite,
                135 => Text.PlayerYellow,
                185 => Text.DarkGreen,
                136 => Text.LightYellow,
                134 => Text.Tan,
                51 => Text.GreyBlue,
                _ => Text.Default,
            };
        }

        /// <summary>
        /// Retrieves the maximum amount of energy that a unit type will have, taking the
        /// player's energy upgrades into consideration.
        /// </summary>
        /// <param name="unit">The {@link UnitType} to retrieve the maximum energy for.</param>
        /// <returns>Maximum amount of energy that the given unit type can have.</returns>
        public int MaxEnergy(UnitType unit)
        {
            var energy = unit.MaxEnergy();
            if (unit == UnitType.Protoss_Arbiter && GetUpgradeLevel(UpgradeType.Khaydarin_Core) > 0 ||
                unit == UnitType.Protoss_Corsair && GetUpgradeLevel(UpgradeType.Argus_Jewel) > 0 ||
                unit == UnitType.Protoss_Dark_Archon && GetUpgradeLevel(UpgradeType.Argus_Talisman) > 0 ||
                unit == UnitType.Protoss_High_Templar && GetUpgradeLevel(UpgradeType.Khaydarin_Amulet) > 0 ||
                unit == UnitType.Terran_Ghost && GetUpgradeLevel(UpgradeType.Moebius_Reactor) > 0 ||
                unit == UnitType.Terran_Battlecruiser && GetUpgradeLevel(UpgradeType.Colossus_Reactor) > 0 ||
                unit == UnitType.Terran_Science_Vessel && GetUpgradeLevel(UpgradeType.Titan_Reactor) > 0 ||
                unit == UnitType.Terran_Wraith && GetUpgradeLevel(UpgradeType.Apollo_Reactor) > 0 ||
                unit == UnitType.Terran_Medic && GetUpgradeLevel(UpgradeType.Caduceus_Reactor) > 0 ||
                unit == UnitType.Zerg_Defiler && GetUpgradeLevel(UpgradeType.Metasynaptic_Node) > 0 ||
                unit == UnitType.Zerg_Queen && GetUpgradeLevel(UpgradeType.Gamete_Meiosis) > 0)
            {
                energy += 50;
            }

            return energy;
        }

        /// <summary>
        /// Retrieves the top speed of a unit type, taking the player's speed upgrades into
        /// consideration.
        /// </summary>
        /// <param name="unit">The {@link UnitType} to retrieve the top speed for.</param>
        /// <returns>Top speed of the provided unit type for this player.</returns>
        public double TopSpeed(UnitType unit)
        {
            var speed = unit.TopSpeed();
            if (unit == UnitType.Terran_Vulture && GetUpgradeLevel(UpgradeType.Ion_Thrusters) > 0 ||
                unit == UnitType.Zerg_Overlord && GetUpgradeLevel(UpgradeType.Pneumatized_Carapace) > 0 ||
                unit == UnitType.Zerg_Zergling && GetUpgradeLevel(UpgradeType.Metabolic_Boost) > 0 ||
                unit == UnitType.Zerg_Hydralisk && GetUpgradeLevel(UpgradeType.Muscular_Augments) > 0 ||
                unit == UnitType.Protoss_Zealot && GetUpgradeLevel(UpgradeType.Leg_Enhancements) > 0 ||
                unit == UnitType.Protoss_Shuttle && GetUpgradeLevel(UpgradeType.Gravitic_Drive) > 0 ||
                unit == UnitType.Protoss_Observer && GetUpgradeLevel(UpgradeType.Gravitic_Boosters) > 0 ||
                unit == UnitType.Protoss_Scout && GetUpgradeLevel(UpgradeType.Gravitic_Thrusters) > 0 ||
                unit == UnitType.Zerg_Ultralisk && GetUpgradeLevel(UpgradeType.Anabolic_Synthesis) > 0)
            {
                if (unit == UnitType.Protoss_Scout)
                {
                    speed += 427 / 256;
                }
                else
                {
                    speed *= 1.5;
                }

                if (speed < 853 / 256)
                {
                    speed = 853 / 256;
                } //acceleration *= 2;

                //turnRadius *= 2;
            }

            return speed;
        }

        /// <summary>
        /// Retrieves the maximum weapon range of a weapon type, taking the player's weapon
        /// upgrades into consideration.
        /// </summary>
        /// <param name="weapon">The {@link WeaponType} to retrieve the maximum range for.</param>
        /// <returns>Maximum range of the given weapon type for units owned by this player.</returns>
        public int WeaponMaxRange(WeaponType weapon)
        {
            var range = weapon.MaxRange();
            if (weapon == WeaponType.Gauss_Rifle && GetUpgradeLevel(UpgradeType.U_238_Shells) > 0 || weapon == WeaponType.Needle_Spines && GetUpgradeLevel(UpgradeType.Grooved_Spines) > 0)
            {
                range += 1 * 32;
            }
            else if (weapon == WeaponType.Phase_Disruptor && GetUpgradeLevel(UpgradeType.Singularity_Charge) > 0)
            {
                range += 2 * 32;
            }
            else if (weapon == WeaponType.Hellfire_Missile_Pack && GetUpgradeLevel(UpgradeType.Charon_Boosters) > 0)
            {
                range += 3 * 32;
            }

            return range;
        }

        /// <summary>
        /// Retrieves the sight range of a unit type, taking the player's sight range
        /// upgrades into consideration.
        /// </summary>
        /// <param name="unit">The {@link UnitType} to retrieve the sight range for.</param>
        /// <returns>Sight range of the provided unit type for this player.</returns>
        public int SightRange(UnitType unit)
        {
            var range = unit.SightRange();
            if (unit == UnitType.Terran_Ghost && GetUpgradeLevel(UpgradeType.Ocular_Implants) > 0 ||
                unit == UnitType.Zerg_Overlord && GetUpgradeLevel(UpgradeType.Antennae) > 0 ||
                unit == UnitType.Protoss_Observer && GetUpgradeLevel(UpgradeType.Sensor_Array) > 0 ||
                unit == UnitType.Protoss_Scout && GetUpgradeLevel(UpgradeType.Apial_Sensors) > 0)
            {
                range = 11 * 32;
            }

            return range;
        }

        /// <summary>
        /// Retrieves the weapon cooldown of a unit type, taking the player's attack speed
        /// upgrades into consideration.
        /// </summary>
        /// <param name="unit">The {@link UnitType} to retrieve the damage cooldown for.</param>
        /// <returns>Weapon cooldown of the provided unit type for this player.</returns>
        public int WeaponDamageCooldown(UnitType unit)
        {
            var cooldown = unit.GroundWeapon().DamageCooldown();
            if (unit == UnitType.Zerg_Zergling && GetUpgradeLevel(UpgradeType.Adrenal_Glands) > 0)
            {
                // Divide cooldown by 2
                cooldown /= 2;

                // Prevent cooldown from going out of bounds
                cooldown = Math.Min(Math.Max(cooldown, 5), 250);
            }

            return cooldown;
        }

        /// <summary>
        /// Calculates the armor that a given unit type will have, including upgrades.
        /// </summary>
        /// <param name="unit">The unit type to calculate armor for, using the current player's upgrades.</param>
        /// <returns>The amount of armor that the unit will have with the player's upgrades.</returns>
        public int Armor(UnitType unit)
        {
            var armor = unit.Armor();
            armor += GetUpgradeLevel(unit.ArmorUpgrade());
            if ((unit == UnitType.Zerg_Ultralisk && GetUpgradeLevel(UpgradeType.Chitinous_Plating) > 0) ||
                unit == UnitType.Hero_Torrasque)
            {
                armor += 2;
            }

            return armor;
        }

        /// <summary>
        /// Calculates the damage that a given weapon type can deal, including upgrades.
        /// </summary>
        /// <param name="wpn">The weapon type to calculate for.</param>
        /// <returns>The amount of damage that the weapon deals with this player's upgrades.</returns>
        public int Damage(WeaponType wpn)
        {
            var dmg = wpn.DamageAmount();
            dmg += GetUpgradeLevel(wpn.UpgradeType()) * wpn.DamageBonus();
            dmg *= wpn.DamageFactor();
            return dmg;
        }

        /// <summary>
        /// Retrieves the total unit score, as seen in the end-game score screen.
        /// </summary>
        /// <returns>The player's unit score.</returns>
        public int GetUnitScore()
        {
            return _playerData.GetTotalUnitScore();
        }

        /// <summary>
        /// Retrieves the total kill score, as seen in the end-game score screen.
        /// </summary>
        /// <returns>The player's kill score.</returns>
        public int GetKillScore()
        {
            return _playerData.GetTotalKillScore();
        }

        /// <summary>
        /// Retrieves the total building score, as seen in the end-game score screen.
        /// </summary>
        /// <returns>The player's building score.</returns>
        public int GetBuildingScore()
        {
            return _playerData.GetTotalBuildingScore();
        }

        /// <summary>
        /// Retrieves the total razing score, as seen in the end-game score screen.
        /// </summary>
        /// <returns>The player's razing score.</returns>
        public int GetRazingScore()
        {
            return _playerData.GetTotalRazingScore();
        }

        /// <summary>
        /// Retrieves the player's custom score. This score is used in @UMS game
        /// types.
        /// </summary>
        /// <returns>The player's custom score.</returns>
        public int GetCustomScore()
        {
            return _playerData.GetCustomScore();
        }

        /// <summary>
        /// Checks if the player is an observer player, typically in a @UMS observer
        /// game. An observer player does not participate in the game.
        /// </summary>
        /// <returns>true if the player is observing, or false if the player is capable of playing in
        /// the game.</returns>
        public bool IsObserver()
        {
            return !_playerData.IsParticipating();
        }

        /// <summary>
        /// Retrieves the maximum upgrades available specific to the player. This
        /// value is only different from UpgradeType#maxRepeats in @UMS games.
        /// </summary>
        /// <param name="upgrade">The {@link UpgradeType} to retrieve the maximum upgrade level for.</param>
        /// <returns>Maximum upgrade level of the given upgrade type.</returns>
        public int GetMaxUpgradeLevel(UpgradeType upgrade)
        {
            return _playerData.GetMaxUpgradeLevel(upgrade);
        }

        /// <summary>
        /// Checks if a technology can be researched by the player. Certain
        /// technologies may be disabled in @UMS game types.
        /// </summary>
        /// <param name="tech">The {@link TechType} to query.</param>
        /// <returns>true if the tech type is available to the player for research.</returns>
        public bool IsResearchAvailable(TechType tech)
        {
            return _playerData.IsResearchAvailable(tech);
        }

        /// <summary>
        /// Checks if a unit type can be created by the player. Certain unit types
        /// may be disabled in @UMS game types.
        /// </summary>
        /// <param name="unit">The {@link UnitType} to check.</param>
        /// <returns>true if the unit type is available to the player.</returns>
        public bool IsUnitAvailable(UnitType unit)
        {
            return _playerData.IsUnitAvailable(unit);
        }

        public bool HasUnitTypeRequirement(UnitType unit)
        {
            return HasUnitTypeRequirement(unit, 1);
        }

        /// <summary>
        /// Verifies that this player satisfies a unit type requirement.
        /// This verifies complex type requirements involving morphable @Zerg structures. For example,
        /// if something requires a @Spire, but the player has (or is in the process of morphing) a @Greater_Spire,
        /// this function will identify the requirement. It is simply a convenience function
        /// that performs all of the requirement checks.
        /// </summary>
        /// <param name="unit">The UnitType to check.</param>
        /// <param name="amount">The amount of units that are required.</param>
        /// <returns>true if the unit type requirements are met, and false otherwise.</returns>
        /// <remarks>@since4.1.2</remarks>
        public bool HasUnitTypeRequirement(UnitType unit, int amount)
        {
            if (unit == UnitType.None)
            {
                return true;
            }

            return unit switch
            {
                UnitType.Zerg_Hatchery => CompletedUnitCount(UnitType.Zerg_Hatchery) + AllUnitCount(UnitType.Zerg_Lair) + AllUnitCount(UnitType.Zerg_Hive) >= amount,
                UnitType.Zerg_Lair => CompletedUnitCount(UnitType.Zerg_Lair) + AllUnitCount(UnitType.Zerg_Hive) >= amount,
                UnitType.Zerg_Spire => CompletedUnitCount(UnitType.Zerg_Spire) + AllUnitCount(UnitType.Zerg_Greater_Spire) >= amount,
                _ => CompletedUnitCount(unit) >= amount,
            };
        }

        public bool Equals(Player other)
        {
            return _id == other._id;
        }

        public override bool Equals(object o)
        {
            return o is Player other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(Player other)
        {
            return _id - other._id;
        }
    }
}