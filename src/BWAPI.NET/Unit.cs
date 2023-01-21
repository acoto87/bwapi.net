using System;
using System.Collections.Generic;
using System.Linq;

namespace BWAPI.NET
{
    /// <summary>
    /// The {@link Unit} class is used to get information about individual units as well as issue
    /// orders to units. Each unit in the game has a unique {@link Unit} object, and {@link Unit} objects
    /// are not deleted until the end of the match (so you don't need to worry about unit pointers
    /// becoming invalid).
    /// <p>
    /// Every Unit in the game is either accessible or inaccessible. To determine if an AI can access
    /// a particular unit, BWAPI checks to see if {@link Flag#CompleteMapInformation} is enabled. So there
    /// are two cases to consider - either the flag is enabled, or it is disabled:
    /// <p>
    /// If {@link Flag#CompleteMapInformation} is disabled, then a unit is accessible if and only if it is visible.
    /// <p>
    /// Some properties of visible enemy units will not be made available to the AI (such as the
    /// contents of visible enemy dropships). If a unit is not visible, {@link Unit#exists} will return false,
    /// regardless of whether or not the unit exists. This is because absolutely no state information on
    /// invisible enemy units is made available to the AI. To determine if an enemy unit has been destroyed, the
    /// AI must watch for {@link BWEventListener#onUnitDestroy} messages from BWAPI, which is only called for visible units
    /// which get destroyed.
    /// <p>
    /// If {@link Flag#CompleteMapInformation} is enabled, then all units that exist in the game are accessible, and
    /// {@link Unit#exists} is accurate for all units. Similarly {@link BWEventListener#onUnitDestroy} messages are generated for all
    /// units that get destroyed, not just visible ones.
    /// <p>
    /// If a Unit is not accessible, then only the getInitial__ functions will be available to the AI.
    /// However for units that were owned by the player, {@link #getPlayer} and {@link #getType} will continue to work for units
    /// that have been destroyed.
    /// </summary>
    public sealed class Unit : IEquatable<Unit>, IComparable<Unit>
    {
        private static readonly HashSet<Order> gatheringGasOrders = new HashSet<Order>() { Order.Harvest1, Order.Harvest2, Order.MoveToGas, Order.WaitForGas, Order.HarvestGas, Order.ReturnGas, Order.ResetCollision };
        private static readonly HashSet<Order> gatheringMineralOrders = new HashSet<Order>() { Order.Harvest1, Order.Harvest2, Order.MoveToMinerals, Order.WaitForMinerals, Order.MiningMinerals, Order.ReturnMinerals, Order.ResetCollision };

        private readonly int _id;
        private readonly ClientData.UnitData _unitData;
        private readonly Game _game;

        // static
        private readonly UnitType _initialType;
        private readonly int _initialResources;
        private readonly int _initialHitPoints;
        private readonly Position _initialPosition;
        private readonly TilePosition _initialTilePosition;

        // variable
        private Position _position;
        private int _lastPositionUpdate = -1;
        private int _lastCommandFrame;
        private UnitCommand _lastCommand;

        // Don't make non-latcom users pay for latcom in memory usage
        private UnitSelf _self;

        public UnitSelf Self()
        {
            return _self ??= new UnitSelf();
        }

        public Unit(int id, ClientData.UnitData unitData, Game game)
        {
            _id = id;
            _unitData = unitData;
            _game = game;

            UpdatePosition(0);

            _initialType = GetUnitType();
            _initialResources = GetResources();
            _initialHitPoints = GetHitPoints();
            _initialPosition = GetPosition();
            _initialTilePosition = GetTilePosition();
        }

        private static bool ReallyGatheringGas(Unit targ, Player player)
        {
            return targ != null && targ.Exists() && targ.IsCompleted() && targ.GetPlayer() == player && targ.GetUnitType() != UnitType.Resource_Vespene_Geyser && (targ.GetUnitType().IsRefinery() || targ.GetUnitType().IsResourceDepot());
        }

        private static bool ReallyGatheringMinerals(Unit targ, Player player)
        {
            return targ != null && targ.Exists() && (targ.GetUnitType().IsMineralField() || (targ.IsCompleted() && targ.GetPlayer() == player && targ.GetUnitType().IsResourceDepot()));
        }

        /// <summary>
        /// Retrieves a unique identifier for this unit.
        /// </summary>
        /// <returns>An integer containing the unit's identifier.</returns>
        /// <remarks>@see#getReplayID</remarks>
        public int GetID()
        {
            return _id;
        }

        /// <summary>
        /// Checks if the Unit exists in the view of the BWAPI player.
        /// <p>
        /// This is used primarily to check if BWAPI has access to a specific unit, or if the
        /// unit is alive. This function is more general and would be synonymous to an isAlive
        /// function if such a function were necessary.
        /// </summary>
        /// <returns>true If the unit exists on the map and is visible according to BWAPI, false If the unit is not accessible or the unit is dead.
        /// <p>
        /// In the event that this function returns false, there are two cases to consider:
        /// 1. You own the unit. This means the unit is dead.
        /// 2. Another player owns the unit. This could either mean that you don't have access
        /// to the unit or that the unit has died. You can specifically identify dead units
        /// by polling onUnitDestroy.</returns>
        /// <remarks>
        /// @see#isVisible
        /// @see#isCompleted
        /// </remarks>
        public bool Exists()
        {
            return _unitData.GetExists();
        }

        /// <summary>
        /// Retrieves the unit identifier for this unit as seen in replay data.
        /// <p>
        /// This is only available if {@link Flag#CompleteMapInformation} is enabled.
        /// </summary>
        /// <returns>An integer containing the replay unit identifier.</returns>
        /// <remarks>@see#getID</remarks>
        public int GetReplayID()
        {
            return _unitData.GetReplayID();
        }

        /// <summary>
        /// Retrieves the player that owns this unit.
        /// </summary>
        /// <returns>The owning Player object. Returns {@link Game#neutral()} If the unit is a neutral unit or inaccessible.</returns>
        public Player GetPlayer()
        {
            return _game.GetPlayer(_unitData.GetPlayer());
        }

        /// <summary>
        /// Retrieves the unit's type.
        /// </summary>
        /// <returns>A {@link UnitType} objects representing the unit's type. Returns {@link UnitType#Unknown} if this unit is inaccessible or cannot be determined.</returns>
        /// <remarks>@see#getInitialType</remarks>
        public UnitType GetUnitType()
        {
            if (_game.IsLatComEnabled() && Self().Type.Valid(_game.GetFrameCount()))
            {
                return Self().Type.Get();
            }

            return _unitData.GetUnitType();
        }

        /// <summary>
        /// Retrieves the unit's position from the upper left corner of the map in pixels.
        /// The position returned is roughly the center if the unit.
        /// <p>
        /// The unit bounds are defined as this value plus/minus the values of
        /// {@link UnitType#dimensionLeft}, {@link UnitType#dimensionUp}, {@link UnitType#dimensionRight},
        /// and {@link UnitType#dimensionDown}, which is conveniently expressed in {@link Unit#getLeft},
        /// {@link Unit#getTop}, {@link Unit#getRight}, and {@link Unit#getBottom} respectively.
        /// </summary>
        /// <returns>{@link Position} object representing the unit's current position. Returns {@link Position#Unknown} if this unit is inaccessible.</returns>
        /// <remarks>
        /// @see#getTilePosition
        /// @see#getInitialPosition
        /// @see#getLeft
        /// @see#getTop
        /// </remarks>
        public Position GetPosition()
        {
            return _position;
        }

        public int GetX()
        {
            return GetPosition().x;
        }

        public int GetY()
        {
            return GetPosition().y;
        }

        /// <summary>
        /// Retrieves the unit's build position from the upper left corner of the map in
        /// tiles.
        /// <p>
        /// This tile position is the tile that is at the top left corner of the structure.
        /// </summary>
        /// <returns>{@link TilePosition} object representing the unit's current tile position. Returns {@link TilePosition#Unknown} if this unit is inaccessible.</returns>
        /// <remarks>
        /// @see#getPosition
        /// @see#getInitialTilePosition
        /// </remarks>
        public TilePosition GetTilePosition()
        {
            Position p = GetPosition();
            UnitType ut = GetUnitType();
            return new Position(Math.Abs(p.x - ut.TileWidth() * 32 / 2), Math.Abs(p.y - ut.TileHeight() * 32 / 2)).ToTilePosition();
        }

        /// <summary>
        /// Retrieves the unit's facing direction in radians.
        /// <p>
        /// A value of 0.0 means the unit is facing east.
        /// </summary>
        /// <returns>A double with the angle measure in radians.</returns>
        public double GetAngle()
        {
            return _unitData.GetAngle();
        }

        /// <summary>
        /// Retrieves the x component of the unit's velocity, measured in pixels per frame.
        /// </summary>
        /// <returns>A double that represents the velocity's x component.</returns>
        /// <remarks>@see#getVelocityY</remarks>
        public double GetVelocityX()
        {
            return _unitData.GetVelocityX();
        }

        /// <summary>
        /// Retrieves the y component of the unit's velocity, measured in pixels per frame.
        /// </summary>
        /// <returns>A double that represents the velocity's y component.</returns>
        /// <remarks>@see#getVelocityX</remarks>
        public double GetVelocityY()
        {
            return _unitData.GetVelocityY();
        }

        /// <summary>
        /// Retrieves the {@link Region} that the center of the unit is in.
        /// </summary>
        /// <returns>The {@link Region} object that contains this unit. Returns null if the unit is inaccessible.</returns>
        public Region GetRegion()
        {
            return _game.GetRegionAt(GetPosition());
        }

        /// <summary>
        /// Retrieves the X coordinate of the unit's left boundary, measured in pixels from
        /// the left side of the map.
        /// </summary>
        /// <returns>An integer representing the position of the left side of the unit.</returns>
        /// <remarks>
        /// @see#getTop
        /// @see#getRight
        /// @see#getBottom
        /// </remarks>
        public int GetLeft()
        {
            return GetX() - GetUnitType().DimensionLeft();
        }

        /// <summary>
        /// Retrieves the Y coordinate of the unit's top boundary, measured in pixels from
        /// the top of the map.
        /// </summary>
        /// <returns>An integer representing the position of the top side of the unit.</returns>
        /// <remarks>
        /// @see#getLeft
        /// @see#getRight
        /// @see#getBottom
        /// </remarks>
        public int GetTop()
        {
            return GetY() - GetUnitType().DimensionUp();
        }

        /// <summary>
        /// Retrieves the X coordinate of the unit's right boundary, measured in pixels from
        /// the left side of the map.
        /// </summary>
        /// <returns>An integer representing the position of the right side of the unit.</returns>
        /// <remarks>
        /// @see#getLeft
        /// @see#getTop
        /// @see#getBottom
        /// </remarks>
        public int GetRight()
        {
            return GetX() + GetUnitType().DimensionRight();
        }

        /// <summary>
        /// Retrieves the Y coordinate of the unit's bottom boundary, measured in pixels from
        /// the top of the map.
        /// </summary>
        /// <returns>An integer representing the position of the bottom side of the unit.</returns>
        /// <remarks>
        /// @see#getLeft
        /// @see#getTop
        /// @see#getRight
        /// </remarks>
        public int GetBottom()
        {
            return GetY() + GetUnitType().DimensionDown();
        }

        /// <summary>
        /// Retrieves the unit's current Hit Points (HP) as seen in the game.
        /// </summary>
        /// <returns>An integer representing the amount of hit points a unit currently has.
        /// <p>
        /// In Starcraft, a unit usually dies when its HP reaches 0. It is possible however, to
        /// have abnormal HP values in the Use Map Settings game type and as the result of a hack over
        /// Battle.net. Such values include units that have 0 HP (can't be killed conventionally)
        /// or even negative HP (death in one hit).</returns>
        /// <remarks>
        /// @seeUnitType#maxHitPoints
        /// @see#getShields
        /// @see#getInitialHitPoints
        /// </remarks>
        public int GetHitPoints()
        {
            int hitpoints = _unitData.GetHitPoints();
            if (_game.IsLatComEnabled() && Self().HitPoints.Valid(_game.GetFrameCount()))
            {
                return hitpoints + Self().HitPoints.Get();
            }

            return hitpoints;
        }

        /// <summary>
        /// Retrieves the unit's current Shield Points (Shields) as seen in the game.
        /// </summary>
        /// <returns>An integer representing the amount of shield points a unit currently has.</returns>
        /// <remarks>
        /// @seeUnitType#maxShields
        /// @see#getHitPoints
        /// </remarks>
        public int GetShields()
        {
            return _unitData.GetShields();
        }

        /// <summary>
        /// Retrieves the unit's current Energy Points (Energy) as seen in the game.
        /// </summary>
        /// <returns>An integer representing the amount of energy points a unit currently has.
        /// <p>
        /// Energy is required in order for units to use abilities.</returns>
        /// <remarks>@seeUnitType#maxEnergy</remarks>
        public int GetEnergy()
        {
            int energy = _unitData.GetEnergy();
            if (_game.IsLatComEnabled() && Self().Energy.Valid(_game.GetFrameCount()))
            {
                return energy + Self().Energy.Get();
            }

            return energy;
        }

        /// <summary>
        /// Retrieves the resource amount from a resource container, such as a Mineral Field
        /// and Vespene Geyser. If the unit is inaccessible, then the last known resource
        /// amount is returned.
        /// </summary>
        /// <returns>An integer representing the last known amount of resources remaining in this
        /// resource.</returns>
        /// <remarks>@see#getInitialResources</remarks>
        public int GetResources()
        {
            return _unitData.GetResources();
        }

        /// <summary>
        /// Retrieves a grouping index from a resource container. Other resource
        /// containers of the same value are considered part of one expansion location (group of
        /// resources that are close together).
        /// <p>
        /// This grouping method is explicitly determined by Starcraft itself and is used only
        /// by the internal AI.
        /// </summary>
        /// <returns>An integer with an identifier between 0 and 250 that determine which resources
        /// are grouped together to form an expansion.</returns>
        public int GetResourceGroup()
        {
            return _unitData.GetResourceGroup();
        }

        /// <summary>
        /// Retrieves the distance between this unit and a target position.
        /// <p>
        /// Distance is calculated from the edge of this unit, using Starcraft's own distance
        /// algorithm. Ignores collisions.
        /// </summary>
        /// <param name="target">A {@link Position} to calculate the distance to.</param>
        /// <returns>An integer representation of the number of pixels between this unit and the
        /// target.</returns>
        public int GetDistance(Position target)
        {
            // If this unit does not exist or target is invalid
            if (!Exists())
            {
                return int.MaxValue;
            }

            /////// Compute distance
            // compute x distance
            int xDist = GetLeft() - target.x;
            if (xDist < 0)
            {
                xDist = target.x - (GetRight() + 1);
                if (xDist < 0)
                {
                    xDist = 0;
                }
            }

            // compute y distance
            int yDist = GetTop() - target.y;
            if (yDist < 0)
            {
                yDist = target.y - (GetBottom() + 1);
                if (yDist < 0)
                {
                    yDist = 0;
                }
            }

            // compute actual distance
            return Position.Origin.GetApproxDistance(new Position(xDist, yDist));
        }

        public int GetDistance(Unit target)
        {
            // If this unit does not exist or target is invalid
            if (!Exists() || target == null || !target.Exists())
            {
                return int.MaxValue;
            }

            // If target is the same as the source
            if (this == target)
            {
                return 0;
            }

            /////// Compute distance
            // retrieve left/top/right/bottom values for calculations
            int left = target.GetLeft() - 1;
            int top = target.GetTop() - 1;
            int right = target.GetRight() + 1;
            int bottom = target.GetBottom() + 1;

            // compute x distance
            int xDist = GetLeft() - right;
            if (xDist < 0)
            {
                xDist = left - GetRight();
                if (xDist < 0)
                {
                    xDist = 0;
                }
            }

            // compute y distance
            int yDist = GetTop() - bottom;
            if (yDist < 0)
            {
                yDist = top - GetBottom();
                if (yDist < 0)
                {
                    yDist = 0;
                }
            }

            // compute actual distance
            return Position.Origin.GetApproxDistance(new Position(xDist, yDist));
        }

        /// <summary>
        /// Using data provided by Starcraft, checks if there is a path available from this
        /// unit to the given target.
        /// <p>
        /// This function only takes into account the terrain data, and does not include
        /// buildings when determining if a path is available. However, the complexity of this
        /// function is constant ( O(1) ), and no extensive calculations are necessary.
        /// <p>
        /// If the current unit is an air unit, then this function will always return true.
        /// <p>
        /// If the unit somehow gets stuck in unwalkable terrain, then this function may still
        /// return true if one of the unit's corners is on walkable terrain (i.e. if the unit is expected
        /// to return to the walkable terrain).
        /// </summary>
        /// <param name="target">A {@link Position} or a {@link Unit} that is used to determine if this unit has a path to the target.</param>
        /// <returns>true If there is a path between this unit and the target position, otherwise it will return false.</returns>
        /// <remarks>@seeGame#hasPath</remarks>
        public bool HasPath(Position target)
        {
            // Return true if this unit is an air unit
            return IsFlying() || _game.HasPath(GetPosition(), target) || _game.HasPath(new Position(GetLeft(), GetTop()), target) || _game.HasPath(new Position(GetRight(), GetTop()), target) || _game.HasPath(new Position(GetLeft(), GetBottom()), target) || _game.HasPath(new Position(GetRight(), GetBottom()), target);
        }

        public bool HasPath(Unit target)
        {
            return HasPath(target.GetPosition());
        }

        /// <summary>
        /// Retrieves the frame number that sent the last successful command.
        /// <p>
        /// This value is comparable to {@link Game#getFrameCount}.
        /// </summary>
        /// <returns>The frame number that sent the last successfully processed command to BWAPI.</returns>
        /// <remarks>
        /// @seeGame#getFrameCount
        /// @see#getLastCommand
        /// </remarks>
        public int GetLastCommandFrame()
        {
            return _lastCommandFrame;
        }

        /// <summary>
        /// Retrieves the last successful command that was sent to BWAPI.
        /// </summary>
        /// <returns>A {@link UnitCommand} object containing information about the command that was processed.</returns>
        /// <remarks>@see#getLastCommandFrame</remarks>
        public UnitCommand GetLastCommand()
        {
            return _lastCommand;
        }

        /// <summary>
        /// Retrieves the {@link Player} that last attacked this unit.
        /// </summary>
        /// <returns>Player object representing the player that last attacked this unit. Returns null if this unit was not attacked.</returns>
        public Player GetLastAttackingPlayer()
        {
            return _game.GetPlayer(_unitData.GetLastAttackerPlayer());
        }

        /// <summary>
        /// Retrieves the initial type of the unit. This is the type that the unit
        /// starts as in the beginning of the game. This is used to access the types of static neutral
        /// units such as mineral fields when they are not visible.
        /// </summary>
        /// <returns>{@link UnitType} of this unit as it was when it was created.
        /// Returns {@link UnitType#Unknown} if this unit was not a static neutral unit in the beginning of the game.</returns>
        public UnitType GetInitialType()
        {
            return _initialType;
        }

        /// <summary>
        /// Retrieves the initial position of this unit. This is the position that
        /// the unit starts at in the beginning of the game. This is used to access the positions of
        /// static neutral units such as mineral fields when they are not visible.
        /// </summary>
        /// <returns>{@link Position} indicating the unit's initial position when it was created.
        /// Returns {@link Position#Unknown} if this unit was not a static neutral unit in the beginning of
        /// the game.</returns>
        public Position GetInitialPosition()
        {
            return _initialPosition;
        }

        /// <summary>
        /// Retrieves the initial build tile position of this unit. This is the tile
        /// position that the unit starts at in the beginning of the game. This is used to access the
        /// tile positions of static neutral units such as mineral fields when they are not visible.
        /// The build tile position corresponds to the upper left corner of the unit.
        /// </summary>
        /// <returns>{@link TilePosition} indicating the unit's initial tile position when it was created.
        /// Returns {@link TilePosition#Unknown} if this unit was not a static neutral unit in the beginning of
        /// the game.</returns>
        public TilePosition GetInitialTilePosition()
        {
            return _initialTilePosition;
        }

        /// <summary>
        /// Retrieves the amount of hit points that this unit started off with at the
        /// beginning of the game. The unit must be neutral.
        /// </summary>
        /// <returns>Number of hit points that this unit started with.
        /// Returns 0 if this unit was not a neutral unit at the beginning of the game.
        /// <p>
        /// It is possible for the unit's initial hit points to differ from the maximum hit
        /// points.</returns>
        /// <remarks>@seeGame#getStaticNeutralUnits</remarks>
        public int GetInitialHitPoints()
        {
            return _initialHitPoints;
        }

        /// <summary>
        /// Retrieves the amount of resources contained in the unit at the beginning of the
        /// game. The unit must be a neutral resource container.
        /// </summary>
        /// <returns>Amount of resources that this unit started with.
        /// Returns 0 if this unit was not a neutral unit at the beginning of the game, or if this
        /// unit does not contain resources. It is possible that the unit simply contains 0 resources.</returns>
        /// <remarks>@seeGame#getStaticNeutralUnits</remarks>
        public int GetInitialResources()
        {
            return _initialResources;
        }

        /// <summary>
        /// Retrieves the number of units that this unit has killed in total.
        /// <p>
        /// The maximum amount of recorded kills per unit is 255.
        /// </summary>
        /// <returns>integer indicating this unit's kill count.</returns>
        public int GetKillCount()
        {
            return _unitData.GetKillCount();
        }

        /// <summary>
        /// Retrieves the number of acid spores that this unit is inflicted with.
        /// </summary>
        /// <returns>Number of acid spores on this unit.</returns>
        public int GetAcidSporeCount()
        {
            return _unitData.GetAcidSporeCount();
        }

        /// <summary>
        /// Retrieves the number of interceptors that this unit manages. This
        /// function is only for the @Carrier and its hero.
        /// <p>
        /// This number may differ from the number of units returned from #getInterceptors. This
        /// occurs for cases in which you can see the number of enemy interceptors in the Carrier HUD,
        /// but don't actually have access to the individual interceptors.
        /// </summary>
        /// <returns>Number of interceptors in this unit.</returns>
        /// <remarks>@see#getInterceptors</remarks>
        public int GetInterceptorCount()
        {
            return _unitData.GetInterceptorCount();
        }

        /// <summary>
        /// Retrieves the number of scarabs that this unit has for use. This
        /// function is only for the @Reaver.
        /// </summary>
        /// <returns>Number of scarabs this unit has ready.</returns>
        public int GetScarabCount()
        {
            return _unitData.GetScarabCount();
        }

        /// <summary>
        /// Retrieves the amount of @mines this unit has available. This function
        /// is only for the @Vulture.
        /// </summary>
        /// <returns>Number of spider mines available for placement.</returns>
        public int GetSpiderMineCount()
        {
            return _unitData.GetSpiderMineCount();
        }

        /// <summary>
        /// Retrieves the unit's ground weapon cooldown. This value decreases every
        /// frame, until it reaches 0. When the value is 0, this indicates that the unit is capable of
        /// using its ground weapon, otherwise it must wait until it reaches 0.
        /// <p>
        /// This value will vary, because Starcraft adds an additional random value between
        /// (-1) and (+2) to the unit's weapon cooldown.
        /// </summary>
        /// <returns>Number of frames needed for the unit's ground weapon to become available again.</returns>
        public int GetGroundWeaponCooldown()
        {
            return _unitData.GetGroundWeaponCooldown();
        }

        /// <summary>
        /// Retrieves the unit's air weapon cooldown. This value decreases every
        /// frame, until it reaches 0. When the value is 0, this indicates that the unit is capable of
        /// using its air weapon, otherwise it must wait until it reaches 0.
        /// <p>
        /// This value will vary, because Starcraft adds an additional random value between
        /// (-1) and (+2) to the unit's weapon cooldown.
        /// </summary>
        /// <returns>Number of frames needed for the unit's air weapon to become available again.</returns>
        public int GetAirWeaponCooldown()
        {
            return _unitData.GetAirWeaponCooldown();
        }

        /// <summary>
        /// Retrieves the unit's ability cooldown. This value decreases every frame,
        /// until it reaches 0. When the value is 0, this indicates that the unit is capable of using
        /// one of its special abilities, otherwise it must wait until it reaches 0.
        /// <p>
        /// This value will vary, because Starcraft adds an additional random value between
        /// (-1) and (+2) to the unit's ability cooldown.
        /// </summary>
        /// <returns>Number of frames needed for the unit's abilities to become available again.</returns>
        public int GetSpellCooldown()
        {
            return _unitData.GetSpellCooldown();
        }

        /// <summary>
        /// Retrieves the amount of hit points remaining on the @matrix created by a @Science_Vessel.
        /// The @matrix ability starts with 250 hit points when it is used.
        /// </summary>
        /// <returns>Number of hit points remaining on this unit's @matrix.</returns>
        /// <remarks>
        /// @see#getDefenseMatrixTimer
        /// @see#isDefenseMatrixed
        /// </remarks>
        public int GetDefenseMatrixPoints()
        {
            return _unitData.GetDefenseMatrixPoints();
        }

        /// <summary>
        /// Retrieves the time, in frames, that the @matrix will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>
        /// @see#getDefenseMatrixPoints
        /// @see#isDefenseMatrixed
        /// </remarks>
        public int GetDefenseMatrixTimer()
        {
            return _unitData.GetDefenseMatrixTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @ensnare will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isEnsnared</remarks>
        public int GetEnsnareTimer()
        {
            return _unitData.GetEnsnareTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @irradiate will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isIrradiated</remarks>
        public int GetIrradiateTimer()
        {
            return _unitData.GetIrradiateTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @lockdown will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isLockedDown()</remarks>
        public int GetLockdownTimer()
        {
            return _unitData.GetLockdownTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @maelstrom will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isMaelstrommed</remarks>
        public int GetMaelstromTimer()
        {
            return _unitData.GetMaelstromTimer();
        }

        /// <summary>
        /// Retrieves an internal timer used for the primary order. Its use is
        /// specific to the order type that is currently assigned to the unit.
        /// </summary>
        /// <returns>A value used as a timer for the primary order.</returns>
        /// <remarks>@see#getOrder</remarks>
        public int GetOrderTimer()
        {
            return _unitData.GetOrderTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @plague will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isPlagued</remarks>
        public int GetPlagueTimer()
        {
            return _unitData.GetPlagueTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, until this temporary unit is destroyed or
        /// removed. This is used to determine the remaining time for the following units
        /// that were created by abilities:
        /// - @hallucination
        /// - @broodling
        /// - @swarm
        /// - @dweb
        /// - @scanner
        /// .
        /// Once this value reaches 0, the unit is destroyed.
        /// </summary>
        public int GetRemoveTimer()
        {
            return _unitData.GetRemoveTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @stasis will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isPlagued</remarks>
        public int GetStasisTimer()
        {
            return _unitData.GetStasisTimer();
        }

        /// <summary>
        /// Retrieves the time, in frames, that @stim will remain active on the current
        /// unit.
        /// </summary>
        /// <returns>Number of frames remaining until the effect is removed.</returns>
        /// <remarks>@see#isPlagued</remarks>
        public int GetStimTimer()
        {
            if (_game.IsLatComEnabled() && Self().StimTimer.Valid(_game.GetFrameCount()))
            {
                return Self().StimTimer.Get();
            }

            return _unitData.GetStimTimer();
        }

        /// <summary>
        /// Retrieves the building type that a @worker is about to construct. If
        /// the unit is morphing or is an incomplete structure, then this returns the {@link UnitType} that it
        /// will become when it has completed morphing/constructing.
        /// </summary>
        /// <returns>{@link UnitType} indicating the type that a @worker is about to construct, or an
        /// incomplete unit will be when completed.</returns>
        public UnitType GetBuildType()
        {
            if (_game.IsLatComEnabled() && Self().BuildType.Valid(_game.GetFrameCount()))
            {
                return Self().BuildType.Get();
            }

            return _unitData.GetBuildType();
        }

        /// <summary>
        /// Retrieves the list of unit types queued up to be trained.
        /// </summary>
        /// <returns>a List<UnitType> containing all the types that are in this factory's training
        /// queue, from oldest to most recent.</returns>
        /// <remarks>
        /// @see#train
        /// @see#cancelTrain
        /// @see#isTraining
        /// </remarks>
        public List<UnitType> GetTrainingQueue()
        {
            return Enumerable.Range(0, GetTrainingQueueCount()).Select(x => GetTrainingQueueAt(x)).ToList();
        }

        /// <summary>
        /// Retrieves a unit type from a specific index in the queue of units this unit is training.
        ///
        /// This method does not have a direct analog in the BWAPI client API.
        /// It exists as a more performant alternative to getTrainingQueue().
        /// </summary>
        public UnitType GetTrainingQueueAt(int i)
        {
            if (_game.IsLatComEnabled() && Self().TrainingQueue[i].Valid(_game.GetFrameCount()))
            {
                return Self().TrainingQueue[i].Get();
            }

            return (UnitType)_unitData.GetTrainingQueue(i);
        }

        /// <summary>
        /// Retrieves the number of units in this unit's training queue.
        ///
        /// This method does not have a direct analog in the BWAPI client API.
        /// It exists as a more performant alternative to getTrainingQueue().
        /// </summary>
        public int GetTrainingQueueCount()
        {
            int count = _unitData.GetTrainingQueueCount();
            if (_game.IsLatComEnabled() && Self().TrainingQueueCount.Valid(_game.GetFrameCount()))
            {
                return count + Self().TrainingQueueCount.Get();
            }

            return count;
        }

        /// <summary>
        /// Retrieves the technology that this unit is currently researching.
        /// </summary>
        /// <returns>{@link TechType} indicating the technology being researched by this unit.
        /// Returns {@link TechType#None} if this unit is not researching anything.</returns>
        /// <remarks>
        /// @see#research
        /// @see#cancelResearch
        /// @see#isResearching
        /// @see#getRemainingResearchTime
        /// </remarks>
        public TechType GetTech()
        {
            if (_game.IsLatComEnabled() && Self().Tech.Valid(_game.GetFrameCount()))
            {
                return Self().Tech.Get();
            }

            return (TechType)_unitData.GetTech();
        }

        /// <summary>
        /// Retrieves the upgrade that this unit is currently upgrading.
        /// </summary>
        /// <returns>{@link UpgradeType} indicating the upgrade in progress by this unit.
        /// Returns {@link UpgradeType#None} if this unit is not upgrading anything.</returns>
        /// <remarks>
        /// @see#upgrade
        /// @see#cancelUpgrade
        /// @see#isUpgrading
        /// @see#getRemainingUpgradeTime
        /// </remarks>
        public UpgradeType GetUpgrade()
        {
            if (_game.IsLatComEnabled() && Self().Upgrade.Valid(_game.GetFrameCount()))
            {
                return Self().Upgrade.Get();
            }

            return (UpgradeType)_unitData.GetUpgrade();
        }

        /// <summary>
        /// Retrieves the remaining build time for a unit or structure that is being trained
        /// or constructed.
        /// </summary>
        /// <returns>Number of frames remaining until the unit's completion.</returns>
        public int GetRemainingBuildTime()
        {
            if (_game.IsLatComEnabled() && Self().RemainingBuildTime.Valid(_game.GetFrameCount()))
            {
                return Self().RemainingBuildTime.Get();
            }

            return _unitData.GetRemainingBuildTime();
        }

        /// <summary>
        /// Retrieves the remaining time, in frames, of the unit that is currently being
        /// trained.
        /// <p>
        /// If the unit is a @Hatchery, @Lair, or @Hive, this retrieves the amount of time until
        /// the next larva spawns.
        /// </summary>
        /// <returns>Number of frames remaining until the current training unit becomes completed, or
        /// the number of frames remaining until the next larva spawns.
        /// Returns 0 if the unit is not training or has three larvae.
        /// <p>
        /// + @see #train</returns>
        /// <remarks>@see#getTrainingQueue</remarks>
        public int GetRemainingTrainTime()
        {
            if (_game.IsLatComEnabled() && Self().RemainingTrainTime.Valid(_game.GetFrameCount()))
            {
                return Self().RemainingTrainTime.Get();
            }

            return _unitData.GetRemainingTrainTime();
        }

        /// <summary>
        /// Retrieves the amount of time until the unit is done researching its currently
        /// assigned {@link TechType}.
        /// </summary>
        /// <returns>The remaining research time, in frames, for the current technology being
        /// researched by this unit.
        /// Returns 0 if the unit is not researching anything.</returns>
        /// <remarks>
        /// @see#research
        /// @see#cancelResearch
        /// @see#isResearching
        /// @see#getTech
        /// </remarks>
        public int GetRemainingResearchTime()
        {
            if (_game.IsLatComEnabled() && Self().RemainingResearchTime.Valid(_game.GetFrameCount()))
            {
                return Self().RemainingResearchTime.Get();
            }

            return _unitData.GetRemainingResearchTime();
        }

        /// <summary>
        /// Retrieves the amount of time until the unit is done upgrading its current upgrade.
        /// </summary>
        /// <returns>The remaining upgrade time, in frames, for the current upgrade.
        /// Returns 0 if the unit is not upgrading anything.</returns>
        /// <remarks>
        /// @see#upgrade
        /// @see#cancelUpgrade
        /// @see#isUpgrading
        /// @see#getUpgrade
        /// </remarks>
        public int GetRemainingUpgradeTime()
        {
            if (_game.IsLatComEnabled() && Self().RemainingUpgradeTime.Valid(_game.GetFrameCount()))
            {
                return Self().RemainingUpgradeTime.Get();
            }

            return _unitData.GetRemainingUpgradeTime();
        }

        /// <summary>
        /// Retrieves the unit currently being trained, or the corresponding paired unit for @SCVs
        /// and @Terran structures, depending on the context.
        /// For example, if this unit is a @Factory under construction, this function will return the @SCV
        /// that is constructing it. If this unit is a @SCV, then it will return the structure it
        /// is currently constructing. If this unit is a @Nexus, and it is training a @Probe, then the
        /// probe will be returned.
        /// <p>
        /// BUG: This will return an incorrect unit when called on @Reavers.
        /// </summary>
        /// <returns>Paired build unit that is either constructing this unit, structure being constructed by
        /// this unit, or the unit that is being trained by this structure.
        /// Returns null if there is no unit constructing this one, or this unit is not constructing
        /// another unit.</returns>
        public Unit GetBuildUnit()
        {
            if (_game.IsLatComEnabled() && Self().BuildUnit.Valid(_game.GetFrameCount()))
            {
                return _game.GetUnit(Self().BuildUnit.Get());
            }

            return _game.GetUnit(_unitData.GetBuildUnit());
        }

        /// <summary>
        /// Generally returns the appropriate target unit after issuing an order that accepts
        /// a target unit (i.e. attack, repair, gather, etc.). To get a target that has been
        /// acquired automatically without issuing an order, use {@link #getOrderTarget}.
        /// </summary>
        /// <returns>Unit that is currently being targeted by this unit.</returns>
        /// <remarks>@see#getOrderTarget</remarks>
        public Unit GetTarget()
        {
            if (_game.IsLatComEnabled() && Self().Target.Valid(_game.GetFrameCount()))
            {
                return _game.GetUnit(Self().Target.Get());
            }

            return _game.GetUnit(_unitData.GetTarget());
        }

        /// <summary>
        /// Retrieves the target position the unit is moving to, provided a valid path to the
        /// target position exists.
        /// </summary>
        /// <returns>Target position of a movement action.</returns>
        public Position GetTargetPosition()
        {
            if (_game.IsLatComEnabled() && Self().TargetPositionX.Valid(_game.GetFrameCount()))
            {
                return new Position(Self().TargetPositionX.Get(), Self().TargetPositionY.Get());
            }

            return new Position(_unitData.GetTargetPositionX(), _unitData.GetTargetPositionY());
        }

        /// <summary>
        /// Retrieves the primary Order that the unit is assigned. Primary orders
        /// are distinct actions such as {@link Order#AttackUnit} and {@link Order#PlayerGuard}.
        /// </summary>
        /// <returns>The primary {@link Order} that the unit is executing.</returns>
        public Order GetOrder()
        {
            if (_game.IsLatComEnabled() && Self().Order.Valid(_game.GetFrameCount()))
            {
                return Self().Order.Get();
            }

            return (Order)_unitData.GetOrder();
        }

        /// <summary>
        /// Retrieves the secondary Order that the unit is assigned. Secondary
        /// orders are run in the background as a sub-order. An example would be {@link Order#TrainFighter},
        /// because a @Carrier can move and train fighters at the same time.
        /// </summary>
        /// <returns>The secondary {@link Order} that the unit is executing.</returns>
        public Order GetSecondaryOrder()
        {
            if (_game.IsLatComEnabled() && Self().SecondaryOrder.Valid(_game.GetFrameCount()))
            {
                return Self().SecondaryOrder.Get();
            }

            return (Order)_unitData.GetSecondaryOrder();
        }

        /// <summary>
        /// Retrieves the unit's primary order target. This is usually set when the
        /// low level unit AI acquires a new target automatically. For example if an enemy @Probe
        /// comes in range of your @Marine, the @Marine will start attacking it, and getOrderTarget
        /// will be set in this case, but not getTarget.
        /// </summary>
        /// <returns>The {@link Unit} that this unit is currently targetting.</returns>
        /// <remarks>
        /// @see#getTarget
        /// @see#getOrder
        /// </remarks>
        public Unit GetOrderTarget()
        {
            if (_game.IsLatComEnabled() && Self().OrderTarget.Valid(_game.GetFrameCount()))
            {
                return _game.GetUnit(Self().OrderTarget.Get());
            }

            return _game.GetUnit(_unitData.GetOrderTarget());
        }

        /// <summary>
        /// Retrieves the target position for the unit's order. For example, when
        /// {@link Order#Move} is assigned, {@link #getTargetPosition} returns the end of the unit's path, but this
        /// returns the location that the unit is trying to move to.
        /// </summary>
        /// <returns>{@link Position} that this unit is currently targetting.</returns>
        /// <remarks>
        /// @see#getTargetPosition
        /// @see#getOrder
        /// </remarks>
        public Position GetOrderTargetPosition()
        {
            if (_game.IsLatComEnabled() && Self().OrderTargetPositionX.Valid(_game.GetFrameCount()))
            {
                return new Position(Self().OrderTargetPositionX.Get(), Self().OrderTargetPositionY.Get());
            }

            return new Position(_unitData.GetOrderTargetPositionX(), _unitData.GetOrderTargetPositionY());
        }

        /// <summary>
        /// Retrieves the position the structure is rallying units to once they are
        /// completed.
        /// </summary>
        /// <returns>{@link Position} that a completed unit coming from this structure will travel to.
        /// Returns {@link Position#None} If this building does not produce units.
        /// <p>
        /// If {@link #getRallyUnit} is valid, then this value is ignored.</returns>
        /// <remarks>
        /// @see#setRallyPoint
        /// @see#getRallyUnit
        /// </remarks>
        public Position GetRallyPosition()
        {
            if (_game.IsLatComEnabled() && Self().RallyPositionX.Valid(_game.GetFrameCount()))
            {
                return new Position(Self().RallyPositionX.Get(), Self().RallyPositionY.Get());
            }

            return new Position(_unitData.GetRallyPositionX(), _unitData.GetRallyPositionY());
        }

        /// <summary>
        /// Retrieves the unit the structure is rallying units to once they are completed.
        /// Units will then follow the targetted unit.
        /// </summary>
        /// <returns>{@link Unit} that a completed unit coming from this structure will travel to.
        /// Returns null if the structure is not rallied to a unit or it does not produce units.
        /// <p>
        /// A rallied unit takes precedence over a rallied position. That is if the return value
        /// is valid(non-null), then getRallyPosition is ignored.</returns>
        /// <remarks>
        /// @see#setRallyPoint
        /// @see#getRallyPosition
        /// </remarks>
        public Unit GetRallyUnit()
        {
            if (_game.IsLatComEnabled() && Self().RallyUnit.Valid(_game.GetFrameCount()))
            {
                return _game.GetUnit(Self().RallyUnit.Get());
            }

            return _game.GetUnit(_unitData.GetRallyUnit());
        }

        /// <summary>
        /// Retrieves the add-on that is attached to this unit.
        /// </summary>
        /// <returns>Unit interface that represents the add-on that is attached to this unit.
        /// Returns null if this unit does not have an add-on.</returns>
        public Unit GetAddon()
        {
            return _game.GetUnit(_unitData.GetAddon());
        }

        /// <summary>
        /// Retrieves the @Nydus_Canal that is attached to this one. Every @Nydus_Canal
        /// can place a "Nydus Exit" which, when connected, can be travelled through by @Zerg units.
        /// </summary>
        /// <returns>{@link Unit} object representing the @Nydus_Canal connected to this one.
        /// Returns null if the unit is not a @Nydus_Canal, is not owned, or has not placed a Nydus
        /// Exit.</returns>
        public Unit GetNydusExit()
        {
            return _game.GetUnit(_unitData.GetNydusExit());
        }

        /// <summary>
        /// Retrieves the power-up that the worker unit is holding. Power-ups are
        /// special units such as the @Flag in the @CTF game type, which can be picked up by worker
        /// units.
        /// <p>
        /// If your bot is strictly melee/1v1, then this method is not necessary.
        /// </summary>
        /// <returns>The {@link Unit} object that represents the power-up.
        /// Returns null if the unit is not carrying anything.</returns>
        public Unit GetPowerUp()
        {
            return _game.GetUnit(_unitData.GetPowerUp());
        }

        /// <summary>
        /// Retrieves the @Transport or @Bunker unit that has this unit loaded inside of it.
        /// </summary>
        /// <returns>Unit object representing the @Transport containing this unit.
        /// Returns null if this unit is not in a @Transport.</returns>
        public Unit GetTransport()
        {
            return _game.GetUnit(_unitData.GetTransport());
        }

        /// <summary>
        /// Retrieves the set of units that are contained within this @Bunker or @Transport.
        /// </summary>
        /// <returns>A List<Unit> object containing all of the units that are loaded inside of the
        /// current unit.</returns>
        public List<Unit> GetLoadedUnits()
        {
            if (GetUnitType().SpaceProvided() < 1)
            {
                return new List<Unit>();
            }

            return _game._loadedUnitsCache.GetConnected(this);
        }

        /// <summary>
        /// Retrieves the remaining unit-space available for @Bunkers and @Transports.
        /// </summary>
        /// <returns>The number of spots available to transport a unit.</returns>
        /// <remarks>@see#getLoadedUnits</remarks>
        public int GetSpaceRemaining()
        {
            int space = GetUnitType().SpaceProvided();

            // Decrease the space for each loaded unit
            foreach (Unit u in GetLoadedUnits())
            {
                space -= u.GetUnitType().SpaceRequired();
            }

            return Math.Max(space, 0);
        }

        /// <summary>
        /// Retrieves the parent @Carrier that owns this @Interceptor.
        /// </summary>
        /// <returns>The parent @Carrier unit that has ownership of this one.
        /// Returns null if the current unit is not an @Interceptor.</returns>
        public Unit GetCarrier()
        {
            return _game.GetUnit(_unitData.GetCarrier());
        }

        /// <summary>
        /// Retrieves the set of @Interceptors controlled by this unit. This is
        /// intended for @Carriers and its hero.
        /// </summary>
        /// <returns>List<Unit> containing @Interceptor units owned by this carrier.</returns>
        /// <remarks>@see#getInterceptorCount</remarks>
        public List<Unit> GetInterceptors()
        {
            if (GetUnitType() != UnitType.Protoss_Carrier && GetUnitType() != UnitType.Hero_Gantrithor)
            {
                return new List<Unit>();
            }

            return _game._interceptorsCache.GetConnected(this);
        }

        /// <summary>
        /// Retrieves the parent @Hatchery, @Lair, or @Hive that owns this particular unit.
        /// This is intended for @Larvae.
        /// </summary>
        /// <returns>Hatchery unit that has ownership of this larva.
        /// Returns null if the current unit is not a @Larva or has no parent.</returns>
        /// <remarks>@see#getLarva</remarks>
        public Unit GetHatchery()
        {
            return _game.GetUnit(_unitData.GetHatchery());
        }

        /// <summary>
        /// Retrieves the set of @Larvae that were spawned by this unit.
        /// Only @Hatcheries, @Lairs, and @Hives are capable of spawning @Larvae. This is like clicking the
        /// "Select Larva" button and getting the selection of @Larvae.
        /// </summary>
        /// <returns>List<Unit> containing @Larva units owned by this unit. The set will be empty if
        /// there are none.</returns>
        /// <remarks>@see#getHatchery</remarks>
        public List<Unit> GetLarva()
        {
            if (!GetUnitType().ProducesLarva())
            {
                return new List<Unit>();
            }

            return _game._larvaCache.GetConnected(this);
        }

        public List<Unit> GetUnitsInRadius(int radius)
        {
            return GetUnitsInRadius(radius, (u) => true);
        }

        /// <summary>
        /// Retrieves the set of all units in a given radius of the current unit.
        /// <p>
        /// Takes into account this unit's dimensions. Can optionally specify a filter that is composed
        /// using BWAPI Filter semantics to include only specific units (such as only ground units, etc.)
        /// </summary>
        /// <param name="radius">The radius, in pixels, to search for units.</param>
        /// <param name="pred">The composed function predicate to include only specific (desired) units in the set. Defaults to null, which means no filter.</param>
        /// <returns>A List<Unit> containing the set of units that match the given criteria.</returns>
        /// <remarks>
        /// @seeGame#getClosestUnit
        /// @see#getUnitsInWeaponRange
        /// @seeGame#getUnitsInRadius
        /// @seeGame#getUnitsInRectangle
        /// </remarks>
        public List<Unit> GetUnitsInRadius(int radius, UnitFilter pred)
        {
            if (!Exists())
            {
                return new List<Unit>();
            }

            return _game.GetUnitsInRectangle(GetLeft() - radius, GetTop() - radius, GetRight() + radius, GetBottom() + radius, (u) => GetDistance(u) <= radius && pred(u));
        }

        public List<Unit> GetUnitsInWeaponRange(WeaponType weapon)
        {
            return GetUnitsInWeaponRange(weapon, (u) => true);
        }

        /// <summary>
        /// Obtains the set of units within weapon range of this unit.
        /// </summary>
        /// <param name="weapon">The weapon type to use as a filter for distance and units that can be hit by it.</param>
        /// <param name="pred">A predicate used as an additional filter. If omitted, no additional filter is used.</param>
        /// <remarks>
        /// @see#getUnitsInRadius
        /// @seeGame#getClosestUnit
        /// @seeGame#getUnitsInRadius
        /// @seeGame#getUnitsInRectangle
        /// </remarks>
        public List<Unit> GetUnitsInWeaponRange(WeaponType weapon, UnitFilter pred)
        {

            // Return if this unit does not exist
            if (!Exists())
            {
                return new List<Unit>();
            }

            int max = GetPlayer().WeaponMaxRange(weapon);
            return _game.GetUnitsInRectangle(GetLeft() - max, GetTop() - max, GetRight() + max, GetBottom() + max, (u) =>
            {
                if (!pred(u))
                {
                    return false;
                }

                if (u == this || u.IsInvincible())
                {
                    return false;
                }

                int dist = GetDistance(u);
                if ((weapon.MinRange() != 0 && dist < weapon.MinRange()) || dist > max)
                {
                    return false;
                }

                UnitType ut = u.GetUnitType();
                return (!weapon.TargetsOwn() || u.GetPlayer().Equals(GetPlayer())) && (weapon.TargetsAir() || u.IsFlying()) && (weapon.TargetsGround() || !u.IsFlying()) && (!weapon.TargetsMechanical() || !ut.IsMechanical()) && (!weapon.TargetsOrganic() || !ut.IsOrganic()) && (!weapon.TargetsNonBuilding() || ut.IsBuilding()) && (!weapon.TargetsNonRobotic() || ut.IsRobotic()) && (!weapon.TargetsOrgOrMech() || (!ut.IsOrganic() && !ut.IsMechanical()));
            });
        }

        /// <summary>
        /// Checks if the current unit is housing a @Nuke. This is only available
        /// for @Silos.
        /// </summary>
        /// <returns>true if this unit has a @Nuke ready, and false if there is no @Nuke.</returns>
        public bool HasNuke()
        {
            return _unitData.GetHasNuke();
        }

        /// <summary>
        /// Checks if the current unit is accelerating.
        /// </summary>
        /// <returns>true if this unit is accelerating, and false otherwise</returns>
        public bool IsAccelerating()
        {
            return _unitData.IsAccelerating();
        }

        /// <summary>
        /// Checks if this unit is currently attacking something.
        /// </summary>
        /// <returns>true if this unit is attacking another unit, and false if it is not.</returns>
        public bool IsAttacking()
        {
            return _unitData.IsAttacking();
        }

        /// <summary>
        /// Checks if this unit is currently playing an attack animation. Issuing
        /// commands while this returns true may interrupt the unit's next attack sequence.
        /// </summary>
        /// <returns>true if this unit is currently running an attack frame, and false if interrupting
        /// the unit is feasible.
        /// <p>
        /// This function is only available to some unit types, specifically those that play
        /// special animations when they attack.</returns>
        public bool IsAttackFrame()
        {
            return _unitData.IsAttackFrame();
        }

        /// <summary>
        /// Checks if the current unit is being constructed. This is mostly
        /// applicable to Terran structures which require an SCV to be constructing a structure.
        /// </summary>
        /// <returns>true if this is either a Protoss structure, Zerg structure, or Terran structure
        /// being constructed by an attached SCV.
        /// false if this is either completed, not a structure, or has no SCV constructing it</returns>
        /// <remarks>
        /// @see#build
        /// @see#cancelConstruction
        /// @see#haltConstruction
        /// @see#isConstructing
        /// </remarks>
        public bool IsBeingConstructed()
        {
            if (IsMorphing())
            {
                return true;
            }

            if (IsCompleted())
            {
                return false;
            }

            if (GetUnitType().GetRace() != Race.Terran)
            {
                return true;
            }

            return GetBuildUnit() != null;
        }

        /// <summary>
        /// Checks this @Mineral_Field or @Refinery is currently being gathered from.
        /// </summary>
        /// <returns>true if this unit is a resource container and being harvested by a worker, and
        /// false otherwise</returns>
        public bool IsBeingGathered()
        {
            return _unitData.IsBeingGathered();
        }

        /// <summary>
        /// Checks if this unit is currently being healed by a @Medic or repaired by a @SCV.
        /// </summary>
        /// <returns>true if this unit is being healed, and false otherwise.</returns>
        public bool IsBeingHealed()
        {
            return GetUnitType().GetRace() == Race.Terran && IsCompleted() && GetHitPoints() > _unitData.GetLastHitPoints();
        }

        /// <summary>
        /// Checks if this unit is currently blinded by a @Medic 's @Optical_Flare ability.
        /// Blinded units have reduced sight range and cannot detect other units.
        /// </summary>
        /// <returns>true if this unit is blind, and false otherwise</returns>
        public bool IsBlind()
        {
            return _unitData.IsBlind();
        }

        /// <summary>
        /// Checks if the current unit is slowing down to come to a stop.
        /// </summary>
        /// <returns>true if this unit is breaking, false if it has stopped or is still moving at full
        /// speed.</returns>
        public bool IsBraking()
        {
            return _unitData.IsBraking();
        }

        /// <summary>
        /// Checks if the current unit is burrowed, either using the @Burrow ability, or is
        /// an armed @Spider_Mine.
        /// </summary>
        /// <returns>true if this unit is burrowed, and false otherwise</returns>
        /// <remarks>
        /// @see#burrow
        /// @see#unburrow
        /// </remarks>
        public bool IsBurrowed()
        {
            return _unitData.IsBurrowed();
        }

        public bool IsCarrying()
        {
            return IsCarryingGas() || IsCarryingMinerals();
        }

        /// <summary>
        /// Checks if this worker unit is carrying some vespene gas.
        /// </summary>
        /// <returns>true if this is a worker unit carrying vespene gas, and false if it is either
        /// not a worker, or not carrying gas.</returns>
        /// <remarks>
        /// @see#returnCargo
        /// @see#isGatheringGas
        /// @see#isCarryingMinerals
        /// </remarks>
        public bool IsCarryingGas()
        {
            return _unitData.GetCarryResourceType() == 1;
        }

        /// <summary>
        /// Checks if this worker unit is carrying some minerals.
        /// </summary>
        /// <returns>true if this is a worker unit carrying minerals, and false if it is either
        /// not a worker, or not carrying minerals.</returns>
        /// <remarks>
        /// @see#returnCargo
        /// @see#isGatheringMinerals
        /// @see#isCarryingMinerals
        /// </remarks>
        public bool IsCarryingMinerals()
        {
            return _unitData.GetCarryResourceType() == 2;
        }

        /// <summary>
        /// Checks if this unit is currently @cloaked.
        /// </summary>
        /// <returns>true if this unit is cloaked, and false if it is visible.</returns>
        /// <remarks>
        /// @see#cloak
        /// @see#decloak
        /// </remarks>
        public bool IsCloaked()
        {
            return _unitData.IsCloaked();
        }

        /// <summary>
        /// Checks if this unit has finished being constructed, trained, morphed, or warped
        /// in, and can now receive orders.
        /// </summary>
        /// <returns>true if this unit is completed, and false if it is under construction or inaccessible.</returns>
        public bool IsCompleted()
        {
            if (_game.IsLatComEnabled() && Self().IsCompleted.Valid(_game.GetFrameCount()))
            {
                return Self().IsCompleted.Get();
            }

            return _unitData.IsCompleted();
        }

        /// <summary>
        /// Checks if a unit is either constructing something or moving to construct something.
        /// </summary>
        /// <returns>true when a unit has been issued an order to build a structure and is moving to
        /// the build location, or is currently constructing something.</returns>
        /// <remarks>
        /// @see#isBeingConstructed
        /// @see#build
        /// @see#cancelConstruction
        /// @see#haltConstruction
        /// </remarks>
        public bool IsConstructing()
        {
            if (_game.IsLatComEnabled() && Self().IsConstructing.Valid(_game.GetFrameCount()))
            {
                return Self().IsConstructing.Get();
            }

            return _unitData.IsConstructing();
        }

        /// <summary>
        /// Checks if this unit has the @matrix effect.
        /// </summary>
        /// <returns>true if the @matrix ability was used on this unit, and false otherwise.</returns>
        public bool IsDefenseMatrixed()
        {
            return GetDefenseMatrixTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit is visible or revealed by a detector unit. If this
        /// is false and #isVisible is true, then the unit is only partially visible and requires a
        /// detector in order to be targetted.
        /// </summary>
        /// <returns>true if this unit is detected, and false if it needs a detector unit nearby in
        /// order to see it.</returns>
        public bool IsDetected()
        {
            return _unitData.IsDetected();
        }

        /// <summary>
        /// Checks if the @Queen ability @Ensnare has been used on this unit.
        /// </summary>
        /// <returns>true if the unit is ensnared, and false if it is not</returns>
        public bool IsEnsnared()
        {
            return GetEnsnareTimer() != 0;
        }

        /// <summary>
        /// This macro function checks if this unit is in the air. That is, the unit is
        /// either a flyer or a flying building.
        /// </summary>
        /// <returns>true if this unit is in the air, and false if it is on the ground</returns>
        /// <remarks>
        /// @seeUnitType#isFlyer
        /// @seeUnit#isLifted
        /// </remarks>
        public bool IsFlying()
        {
            return GetUnitType().IsFlyer() || IsLifted();
        }

        /// <summary>
        /// Checks if this unit is following another unit. When a unit is following
        /// another unit, it simply moves where the other unit does, and does not attack enemies when
        /// it is following.
        /// </summary>
        /// <returns>true if this unit is following another unit, and false if it is not</returns>
        /// <remarks>
        /// @see#follow
        /// @see#getTarget
        /// </remarks>
        public bool IsFollowing()
        {
            return GetOrder() == Order.Follow;
        }

        bool IsGathering()
        {
            if (_game.IsLatComEnabled() && Self().IsGathering.Valid(_game.GetFrameCount()))
            {
                return Self().IsGathering.Get();
            }

            return _unitData.IsGathering();
        }

        /// <summary>
        /// Checks if this unit is currently gathering gas. That is, the unit is
        /// either moving to a refinery, waiting to enter a refinery, harvesting from the refinery, or
        /// returning gas to a resource depot.
        /// </summary>
        /// <returns>true if this unit is harvesting gas, and false if it is not</returns>
        /// <remarks>@see#isCarryingGas</remarks>
        public bool IsGatheringGas()
        {
            if (!IsGathering())
            {
                return false;
            }

            Order order = GetOrder();
            if (!gatheringGasOrders.Contains(order))
            {
                return false;
            }

            if (order == Order.ResetCollision)
            {
                return _unitData.GetCarryResourceType() == 1;
            }


            //return true if BWOrder is WaitForGas, HarvestGas, or ReturnGas
            if (order == Order.WaitForGas || order == Order.HarvestGas || order == Order.ReturnGas)
            {
                return true;
            }


            //if BWOrder is MoveToGas, Harvest1, or Harvest2 we need to do some additional checks to make sure the unit is really gathering
            return ReallyGatheringGas(GetTarget(), GetPlayer()) || ReallyGatheringGas(GetOrderTarget(), GetPlayer());
        }

        /// <summary>
        /// Checks if this unit is currently harvesting minerals. That is, the unit
        /// is either moving to a @mineral_field, waiting to mine, mining minerals, or returning
        /// minerals to a resource depot.
        /// </summary>
        /// <returns>true if this unit is gathering minerals, and false if it is not</returns>
        /// <remarks>@see#isCarryingMinerals</remarks>
        public bool IsGatheringMinerals()
        {
            if (!IsGathering())
            {
                return false;
            }

            Order order = GetOrder();
            if (!gatheringMineralOrders.Contains(order))
            {
                return false;
            }

            if (order == Order.ResetCollision)
            {
                return _unitData.GetCarryResourceType() == 2;
            }


            //return true if BWOrder is WaitForMinerals, MiningMinerals, or ReturnMinerals
            if (order == Order.WaitForMinerals || order == Order.MiningMinerals || order == Order.ReturnMinerals)
            {
                return true;
            }


            //if BWOrder is MoveToMinerals, Harvest1, or Harvest2 we need to do some additional checks to make sure the unit is really gathering
            return ReallyGatheringMinerals(GetTarget(), GetPlayer()) || ReallyGatheringMinerals(GetOrderTarget(), GetPlayer());
        }

        /// <summary>
        /// Checks if this unit is a hallucination. Hallucinations are created by
        /// the @High_Templar using the @Hallucination ability. Enemy hallucinations are unknown if
        /// {@link Flag#CompleteMapInformation} is disabled. Hallucinations have a time limit until they are
        /// destroyed (see {@link Unit#getRemoveTimer}).
        /// </summary>
        /// <returns>true if the unit is a hallucination and false otherwise.</returns>
        /// <remarks>@see#getRemoveTimer</remarks>
        public bool IsHallucination()
        {
            return _unitData.IsHallucination();
        }

        /// <summary>
        /// Checks if the unit is currently holding position. A unit that is holding
        /// position will attack other units, but will not chase after them.
        /// </summary>
        /// <returns>true if this unit is holding position, and false if it is not.</returns>
        /// <remarks>@see#holdPosition</remarks>
        public bool IsHoldingPosition()
        {
            return GetOrder() == Order.HoldPosition;
        }

        /// <summary>
        /// Checks if this unit is running an idle order. This function is
        /// particularly useful when checking for units that aren't doing any tasks that you assigned.
        /// <p>
        /// A unit is considered idle if it is <b>not</b> doing any of the following:
        /// - Training
        /// - Constructing
        /// - Morphing
        /// - Researching
        /// - Upgrading
        /// <p>
        /// In <b>addition</b> to running one of the following orders:
        /// - Order.PlayerGuard: Player unit idle.
        /// - Order.Guard: Generic unit idle.
        /// - Order.Stop
        /// - Order.PickupIdle
        /// - Order.Nothing: Structure/generic idle.
        /// - Order.Medic: Medic idle.
        /// - Order.Carrier: Carrier idle.
        /// - Order.Reaver: Reaver idle.
        /// - Order.Critter: Critter idle.
        /// - Order.Neutral: Neutral unit idle.
        /// - Order.TowerGuard: Turret structure idle.
        /// - Order.Burrowed: Burrowed unit idle.
        /// - Order.NukeTrain
        /// - Order.Larva: Larva idle.
        /// </summary>
        /// <returns>true if this unit is idle, and false if this unit is performing any action, such
        /// as moving or attacking</returns>
        /// <remarks>@seeUnit#stop</remarks>
        public bool IsIdle()
        {
            if (_game.IsLatComEnabled() && Self().IsIdle.Valid(_game.GetFrameCount()))
            {
                return Self().IsIdle.Get();
            }

            return _unitData.IsIdle();
        }

        /// <summary>
        /// Checks if the unit can be interrupted.
        /// </summary>
        /// <returns>true if this unit can be interrupted, or false if this unit is uninterruptable</returns>
        public bool IsInterruptible()
        {
            return _unitData.IsInterruptible();
        }

        /// <summary>
        /// Checks the invincibility state for this unit.
        /// </summary>
        /// <returns>true if this unit is currently invulnerable, and false if it is vulnerable</returns>
        public bool IsInvincible()
        {
            return _unitData.IsInvincible();
        }

        /// <summary>
        /// Checks if the target unit can immediately be attacked by this unit in the current
        /// frame.
        /// </summary>
        /// <param name="target">The target unit to use in this check.</param>
        /// <returns>true if target is within weapon range of this unit's appropriate weapon, and
        /// false otherwise.
        /// Returns false if target is invalid, inaccessible, too close, too far, or this unit does
        /// not have a weapon that can attack target.</returns>
        public bool IsInWeaponRange(Unit target)
        {

            // Preliminary checks
            if (!Exists() || target == null || !target.Exists() || this == target)
            {
                return false;
            }


            // Store the types as locals
            UnitType thisType = GetUnitType();

            // Obtain the weapon type
            WeaponType wpn = target.IsFlying() ? thisType.AirWeapon() : thisType.GroundWeapon();

            // Return if there is no weapon type
            if (wpn == WeaponType.None || wpn == WeaponType.Unknown)
            {
                return false;
            }


            // Retrieve the min and max weapon ranges
            int minRange = wpn.MinRange();
            int maxRange = GetPlayer().WeaponMaxRange(wpn);

            // Check if the distance to the unit is within the weapon range
            int distance = GetDistance(target);
            return (minRange == 0 || minRange < distance) && distance <= maxRange;
        }

        /// <summary>
        /// Checks if this unit is irradiated by a @Science_Vessel 's @Irradiate ability.
        /// </summary>
        /// <returns>true if this unit is irradiated, and false otherwise</returns>
        /// <remarks>@see#getIrradiateTimer</remarks>
        public bool IsIrradiated()
        {
            return GetIrradiateTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit is a @Terran building and lifted off the ground.
        /// This function generally implies getType().isBuilding() and isCompleted() both
        /// return true.
        /// </summary>
        /// <returns>true if this unit is a @Terran structure lifted off the ground.</returns>
        /// <remarks>@see#isFlying</remarks>
        public bool IsLifted()
        {
            return _unitData.IsLifted();
        }

        /// <summary>
        /// Checks if this unit is currently loaded into another unit such as a @Transport.
        /// </summary>
        /// <returns>true if this unit is loaded in another one, and false otherwise</returns>
        /// <remarks>
        /// @see#load
        /// @see#unload
        /// @see#unloadAll
        /// </remarks>
        public bool IsLoaded()
        {
            return GetTransport() != null;
        }

        /// <summary>
        /// Checks if this unit is currently @locked by a @Ghost.
        /// </summary>
        /// <returns>true if this unit is locked down, and false otherwise</returns>
        /// <remarks>@see#getLockdownTimer</remarks>
        public bool IsLockedDown()
        {
            return GetLockdownTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit has been @Maelstrommed by a @Dark_Archon.
        /// </summary>
        /// <returns>true if this unit is maelstrommed, and false otherwise</returns>
        /// <remarks>@see#getMaelstromTimer</remarks>
        public bool IsMaelstrommed()
        {
            return GetMaelstromTimer() != 0;
        }

        /// <summary>
        /// Finds out if the current unit is morphing or not. @Zerg units and
        /// structures often have the ability to #morph into different types of units. This function
        /// allows you to identify when this process is occurring.
        /// </summary>
        /// <returns>true if the unit is currently morphing, false if the unit is not morphing</returns>
        /// <remarks>
        /// @see#morph
        /// @see#cancelMorph
        /// @see#getBuildType
        /// @see#getRemainingBuildTime
        /// </remarks>
        public bool IsMorphing()
        {
            if (_game.IsLatComEnabled() && Self().IsMorphing.Valid(_game.GetFrameCount()))
            {
                return Self().IsMorphing.Get();
            }

            return _unitData.IsMorphing();
        }

        /// <summary>
        /// Checks if this unit is currently moving.
        /// </summary>
        /// <returns>true if this unit is moving, and false if it is not</returns>
        /// <remarks>@see#stop</remarks>
        public bool IsMoving()
        {
            if (_game.IsLatComEnabled() && Self().IsMoving.Valid(_game.GetFrameCount()))
            {
                return Self().IsMoving.Get();
            }

            return _unitData.IsMoving();
        }

        /// <summary>
        /// Checks if this unit has been parasited by some other player.
        /// </summary>
        /// <returns>true if this unit is inflicted with @parasite, and false if it is clean</returns>
        public bool IsParasited()
        {
            return _unitData.IsParasited();
        }

        /// <summary>
        /// Checks if this unit is patrolling between two positions.
        /// </summary>
        /// <returns>true if this unit is patrolling and false if it is not</returns>
        /// <remarks>@see#patrol</remarks>
        public bool IsPatrolling()
        {
            return GetOrder() == Order.Patrol;
        }

        /// <summary>
        /// Checks if this unit has been been @plagued by a @defiler.
        /// </summary>
        /// <returns>true if this unit is inflicted with @plague and is taking damage, and false if it
        /// is clean</returns>
        /// <remarks>@see#getPlagueTimer</remarks>
        public bool IsPlagued()
        {
            return GetPlagueTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit is repairing or moving to @repair another unit.
        /// This is only applicable to @SCVs.
        /// </summary>
        /// <returns>true if this unit is currently repairing or moving to @repair another unit, and
        /// false if it is not</returns>
        public bool IsRepairing()
        {
            return GetOrder() == Order.Repair;
        }

        /// <summary>
        /// Checks if this unit is a structure that is currently researching a technology.
        /// See TechTypes for a complete list of technologies in Broodwar.
        /// </summary>
        /// <returns>true if this structure is researching a technology, false otherwise</returns>
        /// <remarks>
        /// @see#research
        /// @see#cancelResearch
        /// @see#getTech
        /// @see#getRemainingResearchTime
        /// </remarks>
        public bool IsResearching()
        {
            return GetOrder() == Order.ResearchTech;
        }

        /// <summary>
        /// Checks if this unit has been selected in the user interface. This
        /// function is only available if the flag Flag#UserInput is enabled.
        /// </summary>
        /// <returns>true if this unit is currently selected, and false if this unit is not selected</returns>
        /// <remarks>@seeGame#getSelectedUnits</remarks>
        public bool IsSelected()
        {
            return _unitData.IsSelected();
        }

        /// <summary>
        /// Checks if this unit is currently @sieged. This is only applicable to @Siege_Tanks.
        /// </summary>
        /// <returns>true if the unit is in siege mode, and false if it is either not in siege mode or
        /// not a @Siege_Tank</returns>
        /// <remarks>
        /// @see#siege
        /// @see#unsiege
        /// </remarks>
        public bool IsSieged()
        {
            UnitType t = GetUnitType();
            return t == UnitType.Terran_Siege_Tank_Siege_Mode || t == UnitType.Hero_Edmund_Duke_Siege_Mode;
        }

        /// <summary>
        /// Checks if the unit is starting to attack.
        /// </summary>
        /// <returns>true if this unit is starting an attack.</returns>
        /// <remarks>
        /// @see#attack
        /// @see#getGroundWeaponCooldown
        /// @see#getAirWeaponCooldown
        /// </remarks>
        public bool IsStartingAttack()
        {
            return _unitData.IsStartingAttack();
        }

        /// <summary>
        /// Checks if this unit is inflicted with @Stasis by an @Arbiter.
        /// </summary>
        /// <returns>true if this unit is locked in a @Stasis and is unable to move, and false if it
        /// is free.
        /// <p>
        /// This function does not necessarily imply that the unit is invincible, since there
        /// is a feature in the @UMS game type that allows stasised units to be vulnerable.</returns>
        /// <remarks>@see#getStasisTimer</remarks>
        public bool IsStasised()
        {
            return GetStasisTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit is currently under the influence of a @Stim_Pack.
        /// </summary>
        /// <returns>true if this unit has used a stim pack, false otherwise</returns>
        /// <remarks>@see#getStimTimer</remarks>
        public bool IsStimmed()
        {
            return GetStimTimer() != 0;
        }

        /// <summary>
        /// Checks if this unit is currently trying to resolve a collision by randomly moving
        /// around.
        /// </summary>
        /// <returns>true if this unit is currently stuck and trying to resolve a collision, and false
        /// if this unit is free</returns>
        public bool IsStuck()
        {
            return _unitData.IsStuck();
        }

        /// <summary>
        /// Checks if this unit is training a new unit. For example, a @Barracks
        /// training a @Marine.
        /// <p>
        /// It is possible for a unit to remain in the training queue with no progress. In that
        /// case, this function will return false because of supply or unit count limitations.
        /// </summary>
        /// <returns>true if this unit is currently training another unit, and false otherwise.</returns>
        /// <remarks>
        /// @see#train
        /// @see#getTrainingQueue
        /// @see#cancelTrain
        /// @see#getRemainingTrainTime
        /// </remarks>
        public bool IsTraining()
        {
            if (_game.IsLatComEnabled() && Self().IsTraining.Valid(_game.GetFrameCount()))
            {
                return Self().IsTraining.Get();
            }

            return _unitData.IsTraining();
        }

        /// <summary>
        /// Checks if the current unit is being attacked. Has a small delay before
        /// this returns false
        /// again when the unit is no longer being attacked.
        /// </summary>
        /// <returns>true if this unit has been attacked within the past few frames, and false
        /// if it has not</returns>
        public bool IsUnderAttack()
        {
            return _unitData.GetRecentlyAttacked();
        }

        /// <summary>
        /// Checks if this unit is under the cover of a @Dark_Swarm.
        /// </summary>
        /// <returns>true if this unit is protected by a @Dark_Swarm, and false if it is not</returns>
        public bool IsUnderDarkSwarm()
        {
            return _unitData.IsUnderDarkSwarm();
        }

        /// <summary>
        /// Checks if this unit is currently being affected by a @Disruption_Web.
        /// </summary>
        /// <returns>true if this unit is under the effects of @Disruption_Web.</returns>
        public bool IsUnderDisruptionWeb()
        {
            return _unitData.IsUnderDWeb();
        }

        /// <summary>
        /// Checks if this unit is currently taking damage from a @Psi_Storm.
        /// </summary>
        /// <returns>true if this unit is losing hit points from a @Psi_Storm, and false otherwise.</returns>
        public bool IsUnderStorm()
        {
            return _unitData.IsUnderStorm();
        }

        /// <summary>
        /// Checks if this unit has power. Most structures are powered by default,
        /// but @Protoss structures require a @Pylon to be powered and functional.
        /// </summary>
        /// <returns>true if this unit has power or is inaccessible, and false if this unit is
        /// unpowered.</returns>
        /// <remarks>@since4.0.1 Beta (previously isUnpowered)</remarks>
        public bool IsPowered()
        {
            return _unitData.IsPowered();
        }

        /// <summary>
        /// Checks if this unit is a structure that is currently upgrading an upgrade.
        /// See UpgradeTypes for a full list of upgrades in Broodwar.
        /// </summary>
        /// <returns>true if this structure is upgrading, false otherwise</returns>
        /// <remarks>
        /// @see#upgrade
        /// @see#cancelUpgrade
        /// @see#getUpgrade
        /// @see#getRemainingUpgradeTime
        /// </remarks>
        public bool IsUpgrading()
        {
            return GetOrder() == Order.Upgrade;
        }

        public bool IsVisible()
        {
            return IsVisible(_game.Self());
        }

        /// <summary>
        /// Checks if this unit is visible.
        /// </summary>
        /// <param name="player">The player to check visibility for. If this parameter is omitted, then the BWAPI player obtained from {@link Game#self()} will be used.</param>
        /// <returns>true if this unit is visible to the specified player, and false if it is not.
        /// <p>
        /// If the {@link Flag#CompleteMapInformation} flag is enabled, existing units hidden by the
        /// fog of war will be accessible, but isVisible will still return false.</returns>
        /// <remarks>@see#exists</remarks>
        public bool IsVisible(Player player)
        {
            return _unitData.IsVisible(player.GetID());
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is
        /// unable to be targetted as the target unit of an unspecified command.
        /// </summary>
        /// <returns>true if BWAPI was unable to determine whether the unit can be a target, false if an error occurred and the unit can not be a target.</returns>
        /// <remarks>@seeUnit#canTargetUnit</remarks>
        public bool IsTargetable()
        {
            if (!Exists())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!IsCompleted() && !ut.IsBuilding() && !IsMorphing() && ut != UnitType.Protoss_Archon && ut != UnitType.Protoss_Dark_Archon)
            {
                return false;
            }

            return ut != UnitType.Spell_Scanner_Sweep && ut != UnitType.Spell_Dark_Swarm && ut != UnitType.Spell_Disruption_Web && ut != UnitType.Special_Map_Revealer;
        }

        /// <summary>
        /// This function issues a command to the unit(s), however it is used for interfacing
        /// only, and is recommended to use one of the more specific command functions when writing an
        /// AI.
        /// </summary>
        /// <param name="command">A {@link UnitCommand} containing command parameters such as the type, position, target, etc.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @seeUnitCommandType
        /// @seeUnit#canIssueCommand
        /// </remarks>
        public bool IssueCommand(UnitCommand command)
        {
            if (!CanIssueCommand(command))
            {
                return false;
            }

            command._unit = this;

            // If using train or morph on a hatchery, automatically switch selection to larva
            // (assuming canIssueCommand ensures that there is a larva)
            if ((command._type == UnitCommandType.Train || command._type == UnitCommandType.Morph) && GetUnitType().ProducesLarva() && command.GetUnitType().WhatBuilds().GetKey() == UnitType.Zerg_Larva)
            {
                foreach (Unit larva in GetLarva())
                {
                    if (!larva.IsConstructing() && larva.IsCompleted() && larva.CanCommand())
                    {
                        command._unit = larva;
                        break;
                    }
                }

                if (command._unit == this)
                {
                    return false;
                }
            }

            if (_game.IsLatComEnabled())
            {
                new CommandTemp(command, _game).Execute();
            }

            _game.AddUnitCommand(command.GetUnitCommandType(), command.GetUnit().GetID(), command.GetTarget() != null ? command.GetTarget().GetID() : -1, command._x, command._y, command._extra);
            _lastCommandFrame = _game.GetFrameCount();
            _lastCommand = command;
            return true;
        }

        public bool Attack(Position target)
        {
            return IssueCommand(UnitCommand.Attack(this, target));
        }

        public bool Attack(Unit target)
        {
            return IssueCommand(UnitCommand.Attack(this, target));
        }

        /// <summary>
        /// Orders the unit(s) to attack move to the specified position.
        /// </summary>
        /// <param name="target">A {@link Position} to designate as the target. The unit will perform an Attack Move command.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <p>
        /// A @Medic will use Heal Move instead of attack.</returns>
        /// <remarks>@seeUnit#canAttack</remarks>
        public bool Attack(Position target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Attack(this, target, shiftQueueCommand));
        }

        public bool Attack(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Attack(this, target, shiftQueueCommand));
        }

        public bool Build(UnitType type)
        {
            return IssueCommand(UnitCommand.Train(this, type));
        }

        /// <summary>
        /// Orders the worker unit(s) to construct a structure at a target position.
        /// </summary>
        /// <param name="type">The {@link UnitType} to build.</param>
        /// <param name="target">A {@link TilePosition} to specify the build location, specifically the upper-left corner of the location. If the target is not specified, then the function call will be redirected to the train command.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <p>
        /// You must have sufficient resources and meet the necessary requirements in order to
        /// build a structure.</returns>
        /// <remarks>
        /// @seeUnit#train
        /// @seeUnit#cancelConstruction
        /// @seeUnit#canBuild
        /// </remarks>
        public bool Build(UnitType type, TilePosition target)
        {
            return IssueCommand(UnitCommand.Build(this, target, type));
        }

        /// <summary>
        /// Orders the @Terran structure(s) to construct an add-on.
        /// </summary>
        /// <param name="type">The add-on {@link UnitType} to construct.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <p>
        /// You must have sufficient resources and meet the necessary requirements in order to
        /// build a structure.</returns>
        /// <remarks>
        /// @seeUnit#build
        /// @seeUnit#cancelAddon
        /// @seeUnit#canBuildAddon
        /// </remarks>
        public bool BuildAddon(UnitType type)
        {
            return IssueCommand(UnitCommand.BuildAddon(this, type));
        }

        /// <summary>
        /// Orders the unit(s) to add a UnitType to its training queue, or morphs into the
        /// {@link UnitType} if it is @Zerg.
        /// </summary>
        /// <param name="type">The {@link UnitType} to train.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <p>
        /// You must have sufficient resources, supply, and meet the necessary requirements in
        /// order to train a unit.
        /// <p>
        /// This command is also used for training @Interceptors and @Scarabs.
        /// If you call this using a @Hatchery, @Lair, or @Hive, then it will automatically
        /// pass the command to one of its @Larvae.</returns>
        /// <remarks>
        /// @seeUnit#build
        /// @seeUnit#morph
        /// @seeUnit#cancelTrain
        /// @seeUnit#isTraining
        /// @seeUnit#canTrain
        /// </remarks>
        public bool Train(UnitType type)
        {
            return IssueCommand(UnitCommand.Train(this, type));
        }

        /// <summary>
        /// Orders the unit(s) to morph into a different {@link UnitType}.
        /// </summary>
        /// <param name="type">The {@link UnitType} to morph into.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @seeUnit#build
        /// @seeUnit#morph
        /// @seeUnit#canMorph
        /// </remarks>
        public bool Morph(UnitType type)
        {
            return IssueCommand(UnitCommand.Morph(this, type));
        }

        /// <summary>
        /// Orders the unit to research the given tech type.
        /// </summary>
        /// <param name="tech">The {@link TechType} to research.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#cancelResearch
        /// @see#isResearching
        /// @see#getRemainingResearchTime
        /// @see#getTech
        /// @see#canResearch
        /// </remarks>
        public bool Research(TechType tech)
        {
            return IssueCommand(UnitCommand.Research(this, tech));
        }

        /// <summary>
        /// Orders the unit to upgrade the given upgrade type.
        /// </summary>
        /// <param name="upgrade">The {@link UpgradeType} to upgrade.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#cancelUpgrade
        /// @see#isUpgrading
        /// @see#getRemainingUpgradeTime
        /// @see#getUpgrade
        /// @see#canUpgrade
        /// </remarks>
        public bool Upgrade(UpgradeType upgrade)
        {
            return IssueCommand(UnitCommand.Upgrade(this, upgrade));
        }

        /// <summary>
        /// Orders the unit to set its rally position.
        /// </summary>
        /// <param name="target">The target position that this structure will rally completed units to.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#getRallyPosition
        /// @see#getRallyUnit
        /// @see#canSetRallyPoint
        /// @see#canSetRallyPosition
        /// @see#canSetRallyUnit
        /// </remarks>
        public bool SetRallyPoint(Position target)
        {
            return IssueCommand(UnitCommand.SetRallyPoint(this, target));
        }

        /// <summary>
        /// Orders the unit to set its rally position to the specified unit.
        /// </summary>
        /// <param name="target">The target unit that this structure will rally completed units to.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that the command would fail.</returns>
        /// <remarks>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <see cref="GetRallyPosition"/>, <see cref="GetRallyUnit"/>, <see cref="CanSetRallyPoint"/>, <see cref="CanSetRallyPosition"/>, <see cref="CanSetRallyUnit"/>
        /// </remarks>
        public bool SetRallyPoint(Unit target)
        {
            return IssueCommand(UnitCommand.SetRallyPoint(this, target));
        }

        /// <summary>
        /// Orders the unit to move from its current position to the specified position.
        /// </summary>
        /// <param name="target">The target position to move to.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isMoving
        /// @see#canMove
        /// </remarks>
        public bool Move(Position target, bool shiftQueueCommand = false)
        {
            return IssueCommand(UnitCommand.Move(this, target, shiftQueueCommand));
        }

        public bool Patrol(Position target)
        {
            return IssueCommand(UnitCommand.Patrol(this, target));
        }

        /// <summary>
        /// Orders the unit to patrol between its current position and the specified position.
        /// While patrolling, units will attack and chase enemy units that they encounter, and then
        /// return to its patrol route. @Medics will automatically heal units and then return to their
        /// patrol route.
        /// </summary>
        /// <param name="target">The position to patrol to.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isPatrolling
        /// @see#canPatrol
        /// </remarks>
        public bool Patrol(Position target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Patrol(this, target, shiftQueueCommand));
        }

        public bool HoldPosition()
        {
            return IssueCommand(UnitCommand.HoldPosition(this));
        }

        /// <summary>
        /// Orders the unit to hold its position.
        /// </summary>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#canHoldPosition
        /// @see#isHoldingPosition
        /// </remarks>
        public bool HoldPosition(bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.HoldPosition(this, shiftQueueCommand));
        }

        public bool Stop()
        {
            return IssueCommand(UnitCommand.Stop(this));
        }

        /// <summary>
        /// Orders the unit to stop.
        /// </summary>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#canStop
        /// @see#isIdle
        /// </remarks>
        public bool Stop(bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Stop(this, shiftQueueCommand));
        }

        public bool Follow(Unit target)
        {
            return IssueCommand(UnitCommand.Follow(this, target));
        }

        /// <summary>
        /// Orders the unit to follow the specified unit. Units that are following
        /// other units will not perform any other actions such as attacking. They will ignore attackers.
        /// </summary>
        /// <param name="target">The target unit to start following.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isFollowing
        /// @see#canFollow
        /// @see#getOrderTarget
        /// </remarks>
        public bool Follow(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Follow(this, target, shiftQueueCommand));
        }

        public bool Gather(Unit target)
        {
            return IssueCommand(UnitCommand.Gather(this, target));
        }

        /// <summary>
        /// Orders the unit to gather the specified unit (must be mineral or refinery type).
        /// </summary>
        /// <param name="target">The target unit to gather from.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isGatheringGas
        /// @see#isGatheringMinerals
        /// @see#canGather
        /// </remarks>
        public bool Gather(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Gather(this, target, shiftQueueCommand));
        }

        public bool ReturnCargo()
        {
            return IssueCommand(UnitCommand.ReturnCargo(this));
        }

        /// <summary>
        /// Orders the unit to return its cargo to a nearby resource depot such as a Command
        /// Center. Only workers that are carrying minerals or gas can be ordered to return
        /// cargo.
        /// </summary>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isCarryingGas
        /// @see#isCarryingMinerals
        /// @see#canReturnCargo
        /// </remarks>
        public bool ReturnCargo(bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.ReturnCargo(this, shiftQueueCommand));
        }

        public bool Repair(Unit target)
        {
            return IssueCommand(UnitCommand.Repair(this, target));
        }

        /// <summary>
        /// Orders the unit to repair the specified unit. Only Terran SCVs can be
        /// ordered to repair, and the target must be a mechanical @Terran unit or building.
        /// </summary>
        /// <param name="target">The unit to repair.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isRepairing
        /// @see#canRepair
        /// </remarks>
        public bool Repair(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Repair(this, target, shiftQueueCommand));
        }

        /// <summary>
        /// Orders the unit to burrow. Either the unit must be a @Lurker, or the
        /// unit must be a @Zerg ground unit that is capable of @Burrowing, and @Burrow technology
        /// must be researched.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#unburrow
        /// @see#isBurrowed
        /// @see#canBurrow
        /// </remarks>
        public bool Burrow()
        {
            return IssueCommand(UnitCommand.Burrow(this));
        }

        /// <summary>
        /// Orders a burrowed unit to unburrow.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#burrow
        /// @see#isBurrowed
        /// @see#canUnburrow
        /// </remarks>
        public bool Unburrow()
        {
            return IssueCommand(UnitCommand.Unburrow(this));
        }

        /// <summary>
        /// Orders the unit to cloak.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#decloak
        /// @see#isCloaked
        /// @see#canCloak
        /// </remarks>
        public bool Cloak()
        {
            return IssueCommand(UnitCommand.Cloak(this));
        }

        /// <summary>
        /// Orders a cloaked unit to decloak.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#cloak
        /// @see#isCloaked
        /// @see#canDecloak
        /// </remarks>
        public bool Decloak()
        {
            return IssueCommand(UnitCommand.Decloak(this));
        }

        /// <summary>
        /// Orders the unit to siege. Only works for @Siege_Tanks.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#unsiege
        /// @see#isSieged
        /// @see#canSiege
        /// </remarks>
        public bool Siege()
        {
            return IssueCommand(UnitCommand.Siege(this));
        }

        /// <summary>
        /// Orders the unit to unsiege. Only works for sieged @Siege_Tanks.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#siege
        /// @see#isSieged
        /// @see#canUnsiege
        /// </remarks>
        public bool Unsiege()
        {
            return IssueCommand(UnitCommand.Unsiege(this));
        }

        /// <summary>
        /// Orders the unit to lift. Only works for liftable @Terran structures.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#land
        /// @see#isLifted
        /// @see#canLift
        /// </remarks>
        public bool Lift()
        {
            return IssueCommand(UnitCommand.Lift(this));
        }

        /// <summary>
        /// Orders the unit to land. Only works for @Terran structures that are
        /// currently lifted.
        /// </summary>
        /// <param name="target">The tile position to land this structure at.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#lift
        /// @see#isLifted
        /// @see#canLand
        /// </remarks>
        public bool Land(TilePosition target)
        {
            return IssueCommand(UnitCommand.Land(this, target));
        }

        public bool Load(Unit target)
        {
            return IssueCommand(UnitCommand.Load(this, target));
        }

        /// <summary>
        /// Orders the unit to load the target unit. Only works if this unit is a @Transport or @Bunker type.
        /// </summary>
        /// <param name="target">The target unit to load into this @Transport or @Bunker.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#unload
        /// @see#unloadAll
        /// @see#getLoadedUnits
        /// @see#isLoaded
        /// </remarks>
        public bool Load(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.Load(this, target, shiftQueueCommand));
        }

        /// <summary>
        /// Orders the unit to unload the target unit. Only works for @Transports
        /// and @Bunkers.
        /// </summary>
        /// <param name="target">Unloads the target unit from this @Transport or @Bunker.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#load
        /// @see#unloadAll
        /// @see#getLoadedUnits
        /// @see#isLoaded
        /// @see#canUnload
        /// @see#canUnloadAtPosition
        /// </remarks>
        public bool Unload(Unit target)
        {
            return IssueCommand(UnitCommand.Unload(this, target));
        }

        public bool UnloadAll()
        {
            return IssueCommand(UnitCommand.UnloadAll(this));
        }

        public bool UnloadAll(bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.UnloadAll(this, shiftQueueCommand));
        }

        public bool UnloadAll(Position target)
        {
            return IssueCommand(UnitCommand.UnloadAll(this, target));
        }

        /// <summary>
        /// Orders the unit to unload all loaded units at the unit's current position.
        /// Only works for @Transports and @Bunkers.
        /// </summary>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#load
        /// @see#unload
        /// @see#getLoadedUnits
        /// @see#isLoaded
        /// @see#canUnloadAll
        /// @see#canUnloadAtPosition
        /// </remarks>
        public bool UnloadAll(Position target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.UnloadAll(this, target, shiftQueueCommand));
        }

        public bool RightClick(Position target)
        {
            return IssueCommand(UnitCommand.RightClick(this, target));
        }

        public bool RightClick(Unit target)
        {
            return IssueCommand(UnitCommand.RightClick(this, target));
        }

        /// <summary>
        /// Performs a right click action as it would work in StarCraft.
        /// </summary>
        /// <param name="target">The target position to right click.</param>
        /// <param name="shiftQueueCommand">If this value is true, then the order will be queued instead of immediately executed. If this value is omitted, then the order will be executed immediately by default.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#canRightClick
        /// @see#canRightClickPosition
        /// @see#canRightClickUnit
        /// </remarks>
        public bool RightClick(Position target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.RightClick(this, target, shiftQueueCommand));
        }

        public bool RightClick(Unit target, bool shiftQueueCommand)
        {
            return IssueCommand(UnitCommand.RightClick(this, target, shiftQueueCommand));
        }

        /// <summary>
        /// Orders a @SCV to stop constructing a structure. This leaves the
        /// structure in an incomplete state until it is either cancelled, razed, or completed by
        /// another @SCV.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isConstructing
        /// @see#canHaltConstruction
        /// </remarks>
        public bool HaltConstruction()
        {
            return IssueCommand(UnitCommand.HaltConstruction(this));
        }

        /// <summary>
        /// Orders this unit to cancel and refund itself from begin constructed.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#isBeingConstructed
        /// @see#build
        /// @see#canCancelConstruction
        /// </remarks>
        public bool CancelConstruction()
        {
            return IssueCommand(UnitCommand.CancelConstruction(this));
        }

        /// <summary>
        /// Orders this unit to cancel and refund an add-on that is being constructed.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#canCancelAddon
        /// @see#buildAddon
        /// </remarks>
        public bool CancelAddon()
        {
            return IssueCommand(UnitCommand.CancelAddon(this));
        }

        public bool CancelTrain()
        {
            return IssueCommand(UnitCommand.CancelTrain(this));
        }

        /// <summary>
        /// Orders the unit to remove the specified unit from its training queue.
        /// </summary>
        /// <param name="slot">Identifies the slot that will be cancelled. If the specified value is at least 0, then the unit in the corresponding slot from the list provided by {@link #getTrainingQueue} will be cancelled. If the value is either omitted or -2, then the last slot is cancelled.
        ///             <p>
        ///             The value of slot is passed directly to Broodwar. Other negative values have no
        ///             effect.</param>
        /// <remarks>
        /// @see#train
        /// @see#cancelTrain
        /// @see#isTraining
        /// @see#getTrainingQueue
        /// @see#canCancelTrain
        /// @see#canCancelTrainSlot
        /// </remarks>
        public bool CancelTrain(int slot)
        {
            return IssueCommand(UnitCommand.CancelTrain(this, slot));
        }

        /// <summary>
        /// Orders this unit to cancel and refund a unit that is morphing.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#morph
        /// @see#isMorphing
        /// @see#canCancelMorph
        /// </remarks>
        public bool CancelMorph()
        {
            return IssueCommand(UnitCommand.CancelMorph(this));
        }

        /// <summary>
        /// Orders this unit to cancel and refund a research that is in progress.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#research
        /// @see#isResearching
        /// @see#getTech
        /// @see#canCancelResearch
        /// </remarks>
        public bool CancelResearch()
        {
            return IssueCommand(UnitCommand.CancelResearch(this));
        }

        /// <summary>
        /// Orders this unit to cancel and refund an upgrade that is in progress.
        /// </summary>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.</returns>
        /// <remarks>
        /// @see#upgrade
        /// @see#isUpgrading
        /// @see#getUpgrade
        /// @see#canCancelUpgrade
        /// </remarks>
        public bool CancelUpgrade()
        {
            return IssueCommand(UnitCommand.CancelUpgrade(this));
        }

        public bool UseTech(TechType tech)
        {
            return IssueCommand(UnitCommand.UseTech(this, tech));
        }

        /// <summary>
        /// Orders the unit to use a technology.
        /// </summary>
        /// <param name="tech">The technology type to use.</param>
        /// <param name="target">If specified, indicates the target location to use the tech on.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.</returns>
        /// <remarks>
        /// @see#canUseTechWithOrWithoutTarget
        /// @see#canUseTech
        /// @see#canUseTechWithoutTarget
        /// @see#canUseTechUnit
        /// @see#canUseTechPosition
        /// @seeTechType
        /// </remarks>
        public bool UseTech(TechType tech, Position target)
        {
            return IssueCommand(UnitCommand.UseTech(this, tech, target));
        }

        public bool UseTech(TechType tech, Unit target)
        {
            return IssueCommand(UnitCommand.UseTech(this, tech, target));
        }

        /// <summary>
        /// Moves a @Flag_Beacon to a different location. This is only used for @CTF
        /// or @UMS game types.
        /// </summary>
        /// <param name="target">The target tile position to place the @Flag_Beacon.</param>
        /// <returns>true if the command was passed to Broodwar, and false if BWAPI determined that
        /// the command would fail.
        /// <p>
        /// There is a small chance for a command to fail after it has been passed to Broodwar.
        /// <p>
        /// This command is only available for the first 10 minutes of the game, as in Broodwar.</returns>
        /// <remarks>@see#canPlaceCOP</remarks>
        public bool PlaceCOP(TilePosition target)
        {
            return IssueCommand(UnitCommand.PlaceCOP(this, target));
        }

        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanBuildUnitType, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanIssueCommand(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanBuildUnitType, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanBuildUnitType, bool checkCanTargetUnit)
        {
            return CanIssueCommand(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanBuildUnitType, checkCanTargetUnit, true);
        }

        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanBuildUnitType)
        {
            return CanIssueCommand(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanBuildUnitType, true);
        }

        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits)
        {
            return CanIssueCommand(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, true);
        }

        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions)
        {
            return CanIssueCommand(command, checkCanUseTechPositionOnPositions, true);
        }

        public bool CanIssueCommand(UnitCommand command)
        {
            return CanIssueCommand(command, true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute the given command. If you
        /// are calling this function repeatedly (e.g. to generate a collection of valid commands),
        /// you can avoid repeating the same kinds of checks by specifying false for some of the
        /// optional boolean arguments. Make sure that the state hasn't changed since the check was
        /// done though (eg a new frame/event, or a command issued). Also see the more specific functions.
        /// </summary>
        /// <param name="command">A {@link UnitCommand} to check.</param>
        /// <param name="checkCanUseTechPositionOnPositions">Only used if the command type is {@link UnitCommandType#Use_Tech_Position}. A boolean for whether to perform cheap checks for whether the unit is unable to target any positions using the command's {@link TechType} (i.e. regardless of what the other command parameters are). You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCanUseTechUnitOnUnits">Only used if the command type is {@link UnitCommandType#Use_Tech_Unit}. A boolean for whether to perform cheap checks for whether the unit is unable to target any units using the command's {@link TechType} (i.e. regardless of what the other command parameters are). You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCanBuildUnitType">Only used if the command type is {@link UnitCommandType#Build}. A boolean for whether to perform cheap checks for whether the unit is unable to build the specified {@link UnitType} (i.e. regardless of what the other command parameters are). You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCanTargetUnit">Only used for command types that can target a unit. A boolean for whether to perform {@link Unit#canTargetUnit} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCanIssueCommandType">A boolean for whether to perform {@link Unit#canIssueCommandType} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCommandibility">A boolean for whether to perform {@link Unit#canCommand} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <returns>true if BWAPI determined that the command is valid, false if an error occurred and the command is invalid.</returns>
        /// <remarks>
        /// @seeUnitCommandType
        /// @seeUnit#canCommand
        /// @seeUnit#canIssueCommandType
        /// @seeUnit#canTargetUnit
        /// </remarks>
        public bool CanIssueCommand(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanBuildUnitType, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitCommandType ct = command._type;
            if (checkCanIssueCommandType && !CanIssueCommandType(ct, false))
            {
                return false;
            }

            return ct switch
            {
                UnitCommandType.Attack_Move => true,
                UnitCommandType.Attack_Unit => CanAttackUnit(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Build => CanBuild(command.GetUnitType(), new TilePosition(command._x, command._y), checkCanBuildUnitType, false, false),
                UnitCommandType.Build_Addon => CanBuildAddon(command.GetUnitType(), false, false),
                UnitCommandType.Train => CanTrain(command.GetUnitType(), false, false),
                UnitCommandType.Morph => CanMorph(command.GetUnitType(), false, false),
                UnitCommandType.Research => _game.CanResearch(command.GetTechType(), this, false),
                UnitCommandType.Upgrade => _game.CanUpgrade(command.GetUpgradeType(), this, false),
                UnitCommandType.Set_Rally_Position => true,
                UnitCommandType.Set_Rally_Unit => CanSetRallyUnit(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Move => true,
                UnitCommandType.Patrol => true,
                UnitCommandType.Hold_Position => true,
                UnitCommandType.Stop => true,
                UnitCommandType.Follow => CanFollow(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Gather => CanGather(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Return_Cargo => true,
                UnitCommandType.Repair => CanRepair(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Burrow => true,
                UnitCommandType.Unburrow => true,
                UnitCommandType.Cloak => true,
                UnitCommandType.Decloak => true,
                UnitCommandType.Siege => true,
                UnitCommandType.Unsiege => true,
                UnitCommandType.Lift => true,
                UnitCommandType.Land => CanLand(new TilePosition(command._x, command._y), false, false),
                UnitCommandType.Load => CanLoad(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Unload => CanUnload(command._target, checkCanTargetUnit, false, false, false),
                UnitCommandType.Unload_All => true,
                UnitCommandType.Unload_All_Position => CanUnloadAllPosition(command.GetTargetPosition(), false, false),
                UnitCommandType.Right_Click_Position => true,
                UnitCommandType.Right_Click_Unit => CanRightClickUnit(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Halt_Construction => true,
                UnitCommandType.Cancel_Construction => true,
                UnitCommandType.Cancel_Addon => true,
                UnitCommandType.Cancel_Train => true,
                UnitCommandType.Cancel_Train_Slot => CanCancelTrainSlot(command._extra, false, false),
                UnitCommandType.Cancel_Morph => true,
                UnitCommandType.Cancel_Research => true,
                UnitCommandType.Cancel_Upgrade => true,
                UnitCommandType.Use_Tech => CanUseTechWithoutTarget((TechType)command._extra, false, false),
                UnitCommandType.Use_Tech_Unit => CanUseTechUnit((TechType)command._extra, command._target, checkCanTargetUnit, checkCanUseTechUnitOnUnits, false, false),
                UnitCommandType.Use_Tech_Position => CanUseTechPosition((TechType)command._extra, command.GetTargetPosition(), checkCanUseTechPositionOnPositions, false, false),
                UnitCommandType.Place_COP => CanPlaceCOP(new TilePosition(command._x, command._y), false, false),
                _ => true,
            };
        }

        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanIssueCommandGrouped(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanIssueCommandGrouped(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanTargetUnit)
        {
            return CanIssueCommandGrouped(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, checkCanTargetUnit, true);
        }

        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits)
        {
            return CanIssueCommandGrouped(command, checkCanUseTechPositionOnPositions, checkCanUseTechUnitOnUnits, true);
        }

        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions)
        {
            return CanIssueCommandGrouped(command, checkCanUseTechPositionOnPositions, true);
        }

        public bool CanIssueCommandGrouped(UnitCommand command)
        {
            return CanIssueCommandGrouped(command, true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute the given command as part of a List<Unit>
        /// (even if none of the units in the List<Unit> are able to execute the command individually).
        /// The reason this function exists is because some commands are valid for an individual unit
        /// but not for those individuals as a group (e.g. buildings, critters) and some commands are
        /// only valid for a unit if it is commanded as part of a unit group, e.g.:
        /// 1. attackMove/attackUnit for a List<Unit>, some of which can't attack, e.g. @High_Templar.
        /// This is supported simply for consistency with BW's behaviour - you
        /// could issue move command(s) individually instead.
        /// 2. attackMove/move/patrol/rightClickPosition for air unit(s) + e.g. @Larva, as part of
        /// the air stacking technique. This is supported simply for consistency with BW's
        /// behaviour - you could issue move/patrol/rightClickPosition command(s) for them
        /// individually instead.
        /// <p>
        /// BWAPI allows the following special cases to command a unit individually (rather than
        /// only allowing it to be commanded as part of a List<Unit>). These commands are not available
        /// to a user in BW when commanding units individually, but BWAPI allows them for convenience:
        /// - attackMove for @Medic, which is equivalent to Heal Move.
        /// - holdPosition for burrowed @Lurker, for ambushes.
        /// - stop for @Larva, to move it to a different side of the @Hatchery / @Lair / @Hive (e.g.
        /// so that @Drones morphed later morph nearer to minerals/gas).
        /// </summary>
        /// <remarks>
        /// @seeUnitCommandType
        /// @seeUnit#canIssueCommand
        /// @seeUnit#canCommandGrouped
        /// @seeUnit#canIssueCommandTypeGrouped
        /// @seeUnit#canTargetUnit
        /// </remarks>
        public bool CanIssueCommandGrouped(UnitCommand command, bool checkCanUseTechPositionOnPositions, bool checkCanUseTechUnitOnUnits, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            UnitCommandType ct = command._type;
            if (checkCanIssueCommandType && !CanIssueCommandTypeGrouped(ct, false, false))
            {
                return false;
            }

            return ct switch
            {
                UnitCommandType.Attack_Move => true,
                UnitCommandType.Attack_Unit => CanAttackUnitGrouped(command._target, checkCanTargetUnit, false, false, false),
                UnitCommandType.Build => false,
                UnitCommandType.Build_Addon => false,
                UnitCommandType.Train => CanTrain(command.GetUnitType(), false, false),
                UnitCommandType.Morph => CanMorph(command.GetUnitType(), false, false),
                UnitCommandType.Research => false,
                UnitCommandType.Upgrade => false,
                UnitCommandType.Set_Rally_Position => false,
                UnitCommandType.Set_Rally_Unit => false,
                UnitCommandType.Move => true,
                UnitCommandType.Patrol => true,
                UnitCommandType.Hold_Position => true,
                UnitCommandType.Stop => true,
                UnitCommandType.Follow => CanFollow(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Gather => CanGather(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Return_Cargo => true,
                UnitCommandType.Repair => CanRepair(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Burrow => true,
                UnitCommandType.Unburrow => true,
                UnitCommandType.Cloak => true,
                UnitCommandType.Decloak => true,
                UnitCommandType.Siege => true,
                UnitCommandType.Unsiege => true,
                UnitCommandType.Lift => false,
                UnitCommandType.Land => false,
                UnitCommandType.Load => CanLoad(command._target, checkCanTargetUnit, false, false),
                UnitCommandType.Unload => false,
                UnitCommandType.Unload_All => false,
                UnitCommandType.Unload_All_Position => CanUnloadAllPosition(command.GetTargetPosition(), false, false),
                UnitCommandType.Right_Click_Position => true,
                UnitCommandType.Right_Click_Unit => CanRightClickUnitGrouped(command._target, checkCanTargetUnit, false, false, false),
                UnitCommandType.Halt_Construction => true,
                UnitCommandType.Cancel_Construction => false,
                UnitCommandType.Cancel_Addon => false,
                UnitCommandType.Cancel_Train => false,
                UnitCommandType.Cancel_Train_Slot => false,
                UnitCommandType.Cancel_Morph => true,
                UnitCommandType.Cancel_Research => false,
                UnitCommandType.Cancel_Upgrade => false,
                UnitCommandType.Use_Tech => CanUseTechWithoutTarget((TechType)command._extra, false, false),
                UnitCommandType.Use_Tech_Unit => CanUseTechUnit((TechType)command._extra, command._target, checkCanTargetUnit, checkCanUseTechUnitOnUnits, false, false),
                UnitCommandType.Use_Tech_Position => CanUseTechPosition((TechType)command._extra, command.GetTargetPosition(), checkCanUseTechPositionOnPositions, false, false),
                UnitCommandType.Place_COP => false,
                _ => true,
            };
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is unable to
        /// execute any commands (eg the unit is stasised).
        /// </summary>
        /// <returns>true if BWAPI was unable to determine whether the unit can be commanded, false if an error occurred and the unit can not be commanded.</returns>
        /// <remarks>@seeUnit#canIssueCommand</remarks>
        public bool CanCommand()
        {
            if (!Exists() || !GetPlayer().Equals(_game.Self()))
            {
                return false;
            }


            // Global can be ordered check
            if (IsLockedDown() || IsMaelstrommed() || IsStasised() || !IsPowered() || GetOrder() == Order.ZergBirth || IsLoaded())
            {
                if (!GetUnitType().ProducesLarva())
                {
                    return false;
                }
                else
                {
                    foreach (Unit larva in GetLarva())
                    {
                        if (larva.CanCommand())
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            UnitType uType = GetUnitType();
            if (uType == UnitType.Protoss_Interceptor || uType == UnitType.Terran_Vulture_Spider_Mine || uType == UnitType.Spell_Scanner_Sweep || uType == UnitType.Special_Map_Revealer)
            {
                return false;
            }

            if (IsCompleted() && (uType == UnitType.Protoss_Pylon || uType == UnitType.Terran_Supply_Depot || uType.IsResourceContainer() || uType == UnitType.Protoss_Shield_Battery || uType == UnitType.Terran_Nuclear_Missile || uType.IsPowerup() || (uType.IsSpecialBuilding() && !uType.IsFlagBeacon())))
            {
                return false;
            }

            return IsCompleted() || uType.IsBuilding() || IsMorphing();
        }

        public bool CanCommandGrouped()
        {
            return CanCommandGrouped(true);
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is unable to
        /// execute any commands as part of a List<Unit> (eg buildings, critters).
        /// </summary>
        /// <returns>true if BWAPI was unable to determine whether the unit can be commanded grouped, false if an error occurred and the unit can not be commanded grouped.</returns>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canIssueCommand
        /// </remarks>
        public bool CanCommandGrouped(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return !GetUnitType().IsBuilding() && !GetUnitType().IsCritter();
        }

        public bool CanIssueCommandType(UnitCommandType ct)
        {
            return CanIssueCommandType(ct, true);
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is unable to
        /// execute the given command type (i.e. regardless of what other possible command parameters
        /// could be).
        /// </summary>
        /// <param name="ct">A {@link UnitCommandType}.</param>
        /// <param name="checkCommandibility">A boolean for whether to perform {@link Unit#canCommand} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <returns>true if BWAPI was unable to determine whether the command type is invalid, false if an error occurred and the command type is invalid.</returns>
        /// <remarks>
        /// @seeUnitCommandType
        /// @seeUnit#canIssueCommand
        /// </remarks>
        public bool CanIssueCommandType(UnitCommandType ct, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            switch (ct)
            {
                case UnitCommandType.Attack_Move:
                    return CanAttackMove(false);
                case UnitCommandType.Attack_Unit:
                    return CanAttackUnit(false);
                case UnitCommandType.Build:
                    return CanBuild(false);
                case UnitCommandType.Build_Addon:
                    return CanBuildAddon(false);
                case UnitCommandType.Train:
                    return CanTrain(false);
                case UnitCommandType.Morph:
                    return CanMorph(false);
                case UnitCommandType.Research:
                    return CanResearch(false);
                case UnitCommandType.Upgrade:
                    return CanUpgrade(false);
                case UnitCommandType.Set_Rally_Position:
                    return CanSetRallyPosition(false);
                case UnitCommandType.Set_Rally_Unit:
                    return CanSetRallyUnit(false);
                case UnitCommandType.Move:
                    return CanMove(false);
                case UnitCommandType.Patrol:
                    return CanPatrol(false);
                case UnitCommandType.Hold_Position:
                    return CanHoldPosition(false);
                case UnitCommandType.Stop:
                    return CanStop(false);
                case UnitCommandType.Follow:
                    return CanFollow(false);
                case UnitCommandType.Gather:
                    return CanGather(false);
                case UnitCommandType.Return_Cargo:
                    return CanReturnCargo(false);
                case UnitCommandType.Repair:
                    return CanRepair(false);
                case UnitCommandType.Burrow:
                    return CanBurrow(false);
                case UnitCommandType.Unburrow:
                    return CanUnburrow(false);
                case UnitCommandType.Cloak:
                    return CanCloak(false);
                case UnitCommandType.Decloak:
                    return CanDecloak(false);
                case UnitCommandType.Siege:
                    return CanSiege(false);
                case UnitCommandType.Unsiege:
                    return CanUnsiege(false);
                case UnitCommandType.Lift:
                    return CanLift(false);
                case UnitCommandType.Land:
                    return CanLand(false);
                case UnitCommandType.Load:
                    return CanLoad(false);
                case UnitCommandType.Unload:
                    return CanUnload(false);
                case UnitCommandType.Unload_All:
                    return CanUnloadAll(false);
                case UnitCommandType.Unload_All_Position:
                    return CanUnloadAllPosition(false);
                case UnitCommandType.Right_Click_Position:
                    return CanRightClickPosition(false);
                case UnitCommandType.Right_Click_Unit:
                    return CanRightClickUnit(false);
                case UnitCommandType.Halt_Construction:
                    return CanHaltConstruction(false);
                case UnitCommandType.Cancel_Construction:
                    return CanCancelConstruction(false);
                case UnitCommandType.Cancel_Addon:
                    return CanCancelAddon(false);
                case UnitCommandType.Cancel_Train:
                    return CanCancelTrain(false);
                case UnitCommandType.Cancel_Train_Slot:
                    return CanCancelTrainSlot(false);
                case UnitCommandType.Cancel_Morph:
                    return CanCancelMorph(false);
                case UnitCommandType.Cancel_Research:
                    return CanCancelResearch(false);
                case UnitCommandType.Cancel_Upgrade:
                    return CanCancelUpgrade(false);
                case UnitCommandType.Use_Tech:
                case UnitCommandType.Use_Tech_Unit:
                case UnitCommandType.Use_Tech_Position:
                    return CanUseTechWithOrWithoutTarget(false);
                case UnitCommandType.Place_COP:
                    return CanPlaceCOP(false);
            }

            return true;
        }

        public bool CanIssueCommandTypeGrouped(UnitCommandType ct, bool checkCommandibilityGrouped)
        {
            return CanIssueCommandTypeGrouped(ct, checkCommandibilityGrouped, true);
        }

        public bool CanIssueCommandTypeGrouped(UnitCommandType ct)
        {
            return CanIssueCommandTypeGrouped(ct, true);
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is unable to
        /// execute the given command type (i.e. regardless of what other possible command parameters
        /// could be) as part of a List<Unit>.
        /// </summary>
        /// <param name="ct">A {@link UnitCommandType}.</param>
        /// <param name="checkCommandibilityGrouped">A boolean for whether to perform {@link Unit#canCommandGrouped} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <param name="checkCommandibility">A boolean for whether to perform {@link Unit#canCommand} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <returns>true if BWAPI was unable to determine whether the command type is invalid, false if an error occurred and the command type is invalid.</returns>
        /// <remarks>
        /// @seeUnitCommandType
        /// @seeUnit#canIssueCommandGrouped
        /// </remarks>
        public bool CanIssueCommandTypeGrouped(UnitCommandType ct, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            switch (ct)
            {
                case UnitCommandType.Attack_Move:
                    return CanAttackMoveGrouped(false, false);
                case UnitCommandType.Attack_Unit:
                    return CanAttackUnitGrouped(false, false);
                case UnitCommandType.Build:
                    return false;
                case UnitCommandType.Build_Addon:
                    return false;
                case UnitCommandType.Train:
                    return CanTrain(false);
                case UnitCommandType.Morph:
                    return CanMorph(false);
                case UnitCommandType.Research:
                    return false;
                case UnitCommandType.Upgrade:
                    return false;
                case UnitCommandType.Set_Rally_Position:
                    return false;
                case UnitCommandType.Set_Rally_Unit:
                    return false;
                case UnitCommandType.Move:
                    return CanMoveGrouped(false, false);
                case UnitCommandType.Patrol:
                    return CanPatrolGrouped(false, false);
                case UnitCommandType.Hold_Position:
                    return CanHoldPosition(false);
                case UnitCommandType.Stop:
                    return CanStop(false);
                case UnitCommandType.Follow:
                    return CanFollow(false);
                case UnitCommandType.Gather:
                    return CanGather(false);
                case UnitCommandType.Return_Cargo:
                    return CanReturnCargo(false);
                case UnitCommandType.Repair:
                    return CanRepair(false);
                case UnitCommandType.Burrow:
                    return CanBurrow(false);
                case UnitCommandType.Unburrow:
                    return CanUnburrow(false);
                case UnitCommandType.Cloak:
                    return CanCloak(false);
                case UnitCommandType.Decloak:
                    return CanDecloak(false);
                case UnitCommandType.Siege:
                    return CanSiege(false);
                case UnitCommandType.Unsiege:
                    return CanUnsiege(false);
                case UnitCommandType.Lift:
                    return false;
                case UnitCommandType.Land:
                    return false;
                case UnitCommandType.Load:
                    return CanLoad(false);
                case UnitCommandType.Unload:
                    return false;
                case UnitCommandType.Unload_All:
                    return false;
                case UnitCommandType.Unload_All_Position:
                    return CanUnloadAllPosition(false);
                case UnitCommandType.Right_Click_Position:
                    return CanRightClickPositionGrouped(false, false);
                case UnitCommandType.Right_Click_Unit:
                    return CanRightClickUnitGrouped(false, false);
                case UnitCommandType.Halt_Construction:
                    return CanHaltConstruction(false);
                case UnitCommandType.Cancel_Construction:
                    return false;
                case UnitCommandType.Cancel_Addon:
                    return false;
                case UnitCommandType.Cancel_Train:
                    return false;
                case UnitCommandType.Cancel_Train_Slot:
                    return false;
                case UnitCommandType.Cancel_Morph:
                    return CanCancelMorph(false);
                case UnitCommandType.Cancel_Research:
                    return false;
                case UnitCommandType.Cancel_Upgrade:
                    return false;
                case UnitCommandType.Use_Tech:
                case UnitCommandType.Use_Tech_Unit:
                case UnitCommandType.Use_Tech_Position:
                    return CanUseTechWithOrWithoutTarget(false);
                case UnitCommandType.Place_COP:
                    return false;
            }

            return true;
        }

        public bool CanTargetUnit(Unit targetUnit)
        {
            if (targetUnit == null || !targetUnit.Exists())
            {
                return false;
            }

            UnitType targetType = targetUnit.GetUnitType();
            if (!targetUnit.IsCompleted() && !targetType.IsBuilding() && !targetUnit.IsMorphing() && targetType != UnitType.Protoss_Archon && targetType != UnitType.Protoss_Dark_Archon)
            {
                return false;
            }

            return targetType != UnitType.Spell_Scanner_Sweep && targetType != UnitType.Spell_Dark_Swarm && targetType != UnitType.Spell_Disruption_Web && targetType != UnitType.Special_Map_Revealer;
        }

        /// <summary>
        /// Performs some cheap checks to attempt to quickly detect whether the unit is unable to
        /// use the given unit as the target unit of an unspecified command.
        /// </summary>
        /// <param name="targetUnit">A target unit for an unspecified command.</param>
        /// <param name="checkCommandibility">A boolean for whether to perform {@link Unit#canCommand} as a check. You can set this to false if you know this check has already just been performed.</param>
        /// <returns>true if BWAPI was unable to determine whether the unit can target the given unit, false if an error occurred and the unit can not target the given unit.</returns>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#isTargetable
        /// </remarks>
        public bool CanTargetUnit(Unit targetUnit, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanTargetUnit(targetUnit);
        }

        public bool CanAttack()
        {
            return CanAttack(true);
        }

        public bool CanAttack(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanAttackMove(false) || CanAttackUnit(false);
        }

        public bool CanAttack(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttack(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttack(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttack(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttack(Position target, bool checkCanTargetUnit)
        {
            return CanAttack(target, checkCanTargetUnit, true);
        }

        public bool CanAttack(Unit target, bool checkCanTargetUnit)
        {
            return CanAttack(target, checkCanTargetUnit, true);
        }

        public bool CanAttack(Position target)
        {
            return CanAttack(target, true);
        }

        public bool CanAttack(Unit target)
        {
            return CanAttack(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an attack command to attack-move or attack a unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#attack
        /// @seeUnit#canAttackMove
        /// @seeUnit#canAttackUnit
        /// </remarks>
        #pragma warning disable IDE0060
        public bool CanAttack(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        #pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanAttackMove(false);
        }

        public bool CanAttack(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            return CanAttackUnit(target, checkCanTargetUnit, checkCanIssueCommandType, false);
        }

        public bool CanAttackGrouped(bool checkCommandibilityGrouped)
        {
            return CanAttackGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanAttackGrouped()
        {
            return CanAttackGrouped(true);
        }

        public bool CanAttackGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            return CanAttackMoveGrouped(false, false) || CanAttackUnitGrouped(false, false);
        }

        public bool CanAttackGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanAttackGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanAttackGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttackGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttackGrouped(Position target, bool checkCanTargetUnit)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, true);
        }

        public bool CanAttackGrouped(Unit target, bool checkCanTargetUnit)
        {
            return CanAttackGrouped(target, checkCanTargetUnit, true);
        }

        public bool CanAttackGrouped(Position target)
        {
            return CanAttackGrouped(target, true);
        }

        public bool CanAttackGrouped(Unit target)
        {
            return CanAttackGrouped(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an attack command to attack-move or attack a unit,
        /// as part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canAttack
        /// </remarks>
        #pragma warning disable IDE0060
        public bool CanAttackGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped, bool checkCommandibility)
        #pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            return CanAttackMoveGrouped(false, false) || CanAttackUnitGrouped(false, false);
        }

        public bool CanAttackGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            return CanAttackUnitGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, false, false);
        }

        public bool CanAttackMove()
        {
            return CanAttackMove(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute an attack command to attack-move.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#attack
        /// </remarks>
        public bool CanAttackMove(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return (GetUnitType() == UnitType.Terran_Medic || CanAttackUnit(false)) && CanMove(false);
        }

        public bool CanAttackMoveGrouped(bool checkCommandibilityGrouped)
        {
            return CanAttackMoveGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanAttackMoveGrouped()
        {
            return CanAttackMoveGrouped(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute an attack command to attack-move, as part of a
        /// List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canAttackMove
        /// </remarks>
        public bool CanAttackMoveGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            UnitType ut = GetUnitType();
            return ut.CanMove() || ut == UnitType.Terran_Siege_Tank_Siege_Mode || ut == UnitType.Zerg_Cocoon || ut == UnitType.Zerg_Lurker_Egg;
        }

        public bool CanAttackUnit()
        {
            return CanAttackUnit(true);
        }

        public bool CanAttackUnit(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (ut.GroundWeapon() == WeaponType.None && ut.AirWeapon() == WeaponType.None)
            {
                if (ut == UnitType.Protoss_Carrier || ut == UnitType.Hero_Gantrithor)
                {
                    if (GetInterceptorCount() <= 0)
                    {
                        return false;
                    }
                }
                else if (ut == UnitType.Protoss_Reaver || ut == UnitType.Hero_Warbringer)
                {
                    if (GetScarabCount() <= 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (ut == UnitType.Zerg_Lurker)
            {
                if (!IsBurrowed())
                {
                    return false;
                }
            }
            else if (IsBurrowed())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            return GetOrder() != Order.ConstructingBuilding;
        }

        public bool CanAttackUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttackUnit(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttackUnit(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanAttackUnit(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanAttackUnit(Unit targetUnit)
        {
            return CanAttackUnit(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an attack command to attack a unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#attack
        /// </remarks>
        public bool CanAttackUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanAttackUnit(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            if (targetUnit.IsInvincible())
            {
                return false;
            }

            UnitType type = GetUnitType();
            bool targetInAir = targetUnit.IsFlying();
            WeaponType weapon = targetInAir ? type.AirWeapon() : type.GroundWeapon();
            if (weapon == WeaponType.None)
            {
                switch (type)
                {
                    case UnitType.Protoss_Carrier:
                    case UnitType.Hero_Gantrithor:
                        break;
                    case UnitType.Protoss_Reaver:
                    case UnitType.Hero_Warbringer:
                        if (targetInAir)
                        {
                            return false;
                        }

                        break;
                    default:
                        return false;
                }
            }

            if (!type.CanMove() && !IsInWeaponRange(targetUnit))
            {
                return false;
            }

            if (type == UnitType.Zerg_Lurker && !IsInWeaponRange(targetUnit))
            {
                return false;
            }

            return !Equals(targetUnit);
        }

        public bool CanAttackUnitGrouped(bool checkCommandibilityGrouped)
        {
            return CanAttackUnitGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanAttackUnitGrouped()
        {
            return CanAttackUnitGrouped(true);
        }

        public bool CanAttackUnitGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (!IsInterruptible())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.CanMove() && ut != UnitType.Terran_Siege_Tank_Siege_Mode)
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            if (GetUnitType() == UnitType.Zerg_Lurker)
            {
                if (!IsBurrowed())
                {
                    return false;
                }
            }
            else if (IsBurrowed())
            {
                return false;
            }

            return GetOrder() != Order.ConstructingBuilding;
        }

        public bool CanAttackUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanAttackUnitGrouped(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanAttackUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanAttackUnitGrouped(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanAttackUnitGrouped(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanAttackUnitGrouped(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanAttackUnitGrouped(Unit targetUnit)
        {
            return CanAttackUnitGrouped(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an attack command to attack a unit,
        /// as part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canAttackUnit
        /// </remarks>
        public bool CanAttackUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandTypeGrouped, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (checkCanIssueCommandTypeGrouped && !CanAttackUnitGrouped(false, false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            if (IsInvincible())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (ut == UnitType.Zerg_Lurker && !IsInWeaponRange(targetUnit))
            {
                return false;
            }

            if (ut == UnitType.Zerg_Queen && (targetUnit.GetUnitType() != UnitType.Terran_Command_Center || targetUnit.GetHitPoints() >= 750 || targetUnit.GetHitPoints() <= 0))
            {
                return false;
            }

            return !Equals(targetUnit);
        }

        public bool CanBuild()
        {
            return CanBuild(true);
        }

        public bool CanBuild(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (IsConstructing() || !IsCompleted() || (ut.IsBuilding() && !IsIdle()))
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanBuild(UnitType uType, bool checkCanIssueCommandType)
        {
            return CanBuild(uType, checkCanIssueCommandType, true);
        }

        public bool CanBuild(UnitType uType)
        {
            return CanBuild(uType, true);
        }

        public bool CanBuild(UnitType uType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanBuild(false))
            {
                return false;
            }

            if (!_game.CanMake(uType, this))
            {
                return false;
            }

            if (!uType.IsBuilding())
            {
                return false;
            }

            return GetAddon() == null;
        }

        public bool CanBuild(UnitType uType, TilePosition tilePos, bool checkTargetUnitType, bool checkCanIssueCommandType)
        {
            return CanBuild(uType, tilePos, checkTargetUnitType, checkCanIssueCommandType, true);
        }

        public bool CanBuild(UnitType uType, TilePosition tilePos, bool checkTargetUnitType)
        {
            return CanBuild(uType, tilePos, checkTargetUnitType, true);
        }

        public bool CanBuild(UnitType uType, TilePosition tilePos)
        {
            return CanBuild(uType, tilePos, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a build command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#build
        /// </remarks>
        public bool CanBuild(UnitType uType, TilePosition tilePos, bool checkTargetUnitType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanBuild(false))
            {
                return false;
            }

            if (checkTargetUnitType && !CanBuild(uType, false, false))
            {
                return false;
            }

            if (!tilePos.IsValid(_game))
            {
                return false;
            }

            return _game.CanBuildHere(tilePos, uType, this, true);
        }

        public bool CanBuildAddon()
        {
            return CanBuildAddon(true);
        }

        public bool CanBuildAddon(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (IsConstructing() || !IsCompleted() || IsLifted() || (GetUnitType().IsBuilding() && !IsIdle()))
            {
                return false;
            }

            if (GetAddon() != null)
            {
                return false;
            }

            return GetUnitType().CanBuildAddon();
        }

        public bool CanBuildAddon(UnitType uType, bool checkCanIssueCommandType)
        {
            return CanBuildAddon(uType, checkCanIssueCommandType, true);
        }

        public bool CanBuildAddon(UnitType uType)
        {
            return CanBuildAddon(uType, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a buildAddon command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#buildAddon
        /// </remarks>
        public bool CanBuildAddon(UnitType uType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanBuildAddon(uType, false))
            {
                return false;
            }

            if (!_game.CanMake(uType, this))
            {
                return false;
            }

            if (!uType.IsAddon())
            {
                return false;
            }

            return _game.CanBuildHere(GetTilePosition(), uType, this);
        }

        public bool CanTrain()
        {
            return CanTrain(true);
        }

        public bool CanTrain(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (ut.ProducesLarva())
            {
                if (!IsConstructing() && IsCompleted())
                {
                    return true;
                }

                foreach (Unit larva in GetLarva())
                {
                    if (!larva.IsConstructing() && larva.IsCompleted() && larva.CanCommand())
                    {
                        return true;
                    }
                }

                return false;
            }

            if (IsConstructing() || !IsCompleted() || IsLifted())
            {
                return false;
            }

            if (!ut.CanProduce() && ut != UnitType.Terran_Nuclear_Silo && ut != UnitType.Zerg_Hydralisk && ut != UnitType.Zerg_Mutalisk && ut != UnitType.Zerg_Creep_Colony && ut != UnitType.Zerg_Spire && ut != UnitType.Zerg_Larva)
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanTrain(UnitType uType, bool checkCanIssueCommandType)
        {
            return CanTrain(uType, checkCanIssueCommandType, true);
        }

        public bool CanTrain(UnitType uType)
        {
            return CanTrain(uType, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a train command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#train
        /// </remarks>
        public bool CanTrain(UnitType uType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanTrain(false))
            {
                return false;
            }

            Unit thisUnit = this;
            if (GetUnitType().ProducesLarva())
            {
                if (uType.WhatBuilds().GetKey() == UnitType.Zerg_Larva)
                {
                    bool foundCommandableLarva = false;
                    foreach (Unit larva in GetLarva())
                    {
                        if (larva.CanTrain(true))
                        {
                            foundCommandableLarva = true;
                            thisUnit = larva;
                            break;
                        }
                    }

                    if (!foundCommandableLarva)
                    {
                        return false;
                    }
                }
                else if (IsConstructing() || !IsCompleted())
                {
                    return false;
                }
            }

            if (!_game.CanMake(uType, thisUnit))
            {
                return false;
            }

            if (uType.IsAddon() || (uType.IsBuilding() && !thisUnit.GetUnitType().IsBuilding()))
            {
                return false;
            }

            return uType != UnitType.Zerg_Larva && uType != UnitType.Zerg_Egg && uType != UnitType.Zerg_Cocoon;
        }

        public bool CanMorph()
        {
            return CanMorph(true);
        }

        public bool CanMorph(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (ut.ProducesLarva())
            {
                if (!IsConstructing() && IsCompleted() && (!ut.IsBuilding() || IsIdle()))
                {
                    return true;
                }

                foreach (Unit larva in GetLarva())
                {
                    if (!larva.IsConstructing() && larva.IsCompleted() && larva.CanCommand())
                    {
                        return true;
                    }
                }

                return false;
            }

            if (IsConstructing() || !IsCompleted() || (ut.IsBuilding() && !IsIdle()))
            {
                return false;
            }

            if (ut != UnitType.Zerg_Hydralisk && ut != UnitType.Zerg_Mutalisk && ut != UnitType.Zerg_Creep_Colony && ut != UnitType.Zerg_Spire && ut != UnitType.Zerg_Hatchery && ut != UnitType.Zerg_Lair && ut != UnitType.Zerg_Hive && ut != UnitType.Zerg_Larva)
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanMorph(UnitType uType, bool checkCanIssueCommandType)
        {
            return CanMorph(uType, checkCanIssueCommandType, true);
        }

        public bool CanMorph(UnitType uType)
        {
            return CanMorph(uType, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a morph command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#morph
        /// </remarks>
        public bool CanMorph(UnitType uType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanMorph(false))
            {
                return false;
            }

            Unit thisUnit = this;
            if (GetUnitType().ProducesLarva())
            {
                if (uType.WhatBuilds().GetKey() == UnitType.Zerg_Larva)
                {
                    bool foundCommandableLarva = false;
                    foreach (Unit larva in GetLarva())
                    {
                        if (larva.CanMorph(true))
                        {
                            foundCommandableLarva = true;
                            thisUnit = larva;
                            break;
                        }
                    }

                    if (!foundCommandableLarva)
                    {
                        return false;
                    }
                }
                else if (IsConstructing() || !IsCompleted() || (GetUnitType().IsBuilding() && !IsIdle()))
                {
                    return false;
                }
            }

            if (!_game.CanMake(uType, thisUnit))
            {
                return false;
            }

            return uType != UnitType.Zerg_Larva && uType != UnitType.Zerg_Egg && uType != UnitType.Zerg_Cocoon;
        }

        public bool CanResearch()
        {
            return CanResearch(true);
        }

        public bool CanResearch(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return !IsLifted() && IsIdle() && IsCompleted();
        }

        public bool CanResearch(TechType type)
        {
            return CanResearch(type, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a research command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#research
        /// </remarks>
        public bool CanResearch(TechType type, bool checkCanIssueCommandType)
        {
            Player self = _game.Self();
            if (!GetPlayer().Equals(self))
            {
                return false;
            }

            if (!GetUnitType().IsSuccessorOf(type.WhatResearches()))
            {
                return false;
            }

            if (checkCanIssueCommandType && (IsLifted() || !IsIdle() || !IsCompleted()))
            {
                return false;
            }

            if (self.IsResearching(type))
            {
                return false;
            }

            if (self.HasResearched(type))
            {
                return false;
            }

            if (!self.IsResearchAvailable(type))
            {
                return false;
            }

            if (self.Minerals() < type.MineralPrice())
            {
                return false;
            }

            if (self.Gas() < type.GasPrice())
            {
                return false;
            }

            return self.HasUnitTypeRequirement(type.RequiredUnit());
        }

        public bool CanUpgrade()
        {
            return CanUpgrade(true);
        }

        public bool CanUpgrade(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return !IsLifted() && IsIdle() && IsCompleted();
        }

        public bool CanUpgrade(UpgradeType type)
        {
            return CanUpgrade(type, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an upgrade command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#upgrade
        /// </remarks>
        public bool CanUpgrade(UpgradeType type, bool checkCanIssueCommandType)
        {
            Player self = _game.Self();
            if (!GetPlayer().Equals(self))
            {
                return false;
            }

            if (!GetUnitType().IsSuccessorOf(type.WhatUpgrades()))
            {
                return false;
            }

            if (checkCanIssueCommandType && (IsLifted() || !IsIdle() || !IsCompleted()))
            {
                return false;
            }

            if (!self.HasUnitTypeRequirement(type.WhatUpgrades()))
            {
                return false;
            }

            int nextLvl = self.GetUpgradeLevel(type) + 1;
            if (!self.HasUnitTypeRequirement(type.WhatsRequired(nextLvl)))
            {
                return false;
            }

            if (self.IsUpgrading(type))
            {
                return false;
            }

            if (self.GetUpgradeLevel(type) >= self.GetMaxUpgradeLevel(type))
            {
                return false;
            }

            if (self.Minerals() < type.MineralPrice(nextLvl))
            {
                return false;
            }

            return self.Gas() >= type.GasPrice(nextLvl);
        }

        public bool CanSetRallyPoint()
        {
            return CanSetRallyPoint(true);
        }

        public bool CanSetRallyPoint(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanSetRallyPosition(false) || CanSetRallyUnit(false);
        }

        public bool CanSetRallyPoint(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanSetRallyPoint(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanSetRallyPoint(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanSetRallyPoint(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanSetRallyPoint(Position target, bool checkCanTargetUnit)
        {
            return CanSetRallyPoint(target, checkCanTargetUnit, true);
        }

        public bool CanSetRallyPoint(Unit target, bool checkCanTargetUnit)
        {
            return CanSetRallyPoint(target, checkCanTargetUnit, true);
        }

        public bool CanSetRallyPoint(Position target)
        {
            return CanSetRallyPoint(target, true);
        }

        public bool CanSetRallyPoint(Unit target)
        {
            return CanSetRallyPoint(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a setRallyPoint command to a
        /// position or unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#setRallyPoint
        /// @seeUnit#canSetRallyPosition
        /// @seeUnit#canSetRallyUnit
        /// </remarks>
        #pragma warning disable IDE0060
        public bool CanSetRallyPoint(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        #pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanSetRallyPosition(false);
        }

        public bool CanSetRallyPoint(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            return CanSetRallyUnit(target, checkCanTargetUnit, checkCanIssueCommandType, false);
        }

        public bool CanSetRallyPosition()
        {
            return CanSetRallyPosition(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a setRallyPoint command to a position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#setRallyPoint
        /// </remarks>
        public bool CanSetRallyPosition(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().CanProduce() || !GetUnitType().IsBuilding())
            {
                return false;
            }

            return !IsLifted();
        }

        public bool CanSetRallyUnit()
        {
            return CanSetRallyUnit(true);
        }

        public bool CanSetRallyUnit(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().CanProduce() || !GetUnitType().IsBuilding())
            {
                return false;
            }

            return !IsLifted();
        }

        public bool CanSetRallyUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanSetRallyUnit(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanSetRallyUnit(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanSetRallyUnit(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanSetRallyUnit(Unit targetUnit)
        {
            return CanSetRallyUnit(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a setRallyPoint command to a unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#setRallyPoint
        /// </remarks>
        public bool CanSetRallyUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanSetRallyUnit(false))
            {
                return false;
            }

            return !checkCanTargetUnit || CanTargetUnit(targetUnit, false);
        }

        public bool CanMove()
        {
            return CanMove(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a move command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#move
        /// </remarks>
        public bool CanMove(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding())
            {
                if (!IsInterruptible())
                {
                    return false;
                }

                if (!GetUnitType().CanMove())
                {
                    return false;
                }

                if (IsBurrowed())
                {
                    return false;
                }

                if (GetOrder() == Order.ConstructingBuilding)
                {
                    return false;
                }

                if (ut == UnitType.Zerg_Larva)
                {
                    return false;
                }
            }
            else
            {
                if (!ut.IsFlyingBuilding())
                {
                    return false;
                }

                if (!IsLifted())
                {
                    return false;
                }
            }

            return IsCompleted();
        }

        public bool CanMoveGrouped(bool checkCommandibilityGrouped)
        {
            return CanMoveGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanMoveGrouped()
        {
            return CanMoveGrouped(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a move command, as part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canMove
        /// </remarks>
        public bool CanMoveGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (!GetUnitType().CanMove())
            {
                return false;
            }

            return IsCompleted() || IsMorphing();
        }

        public bool CanPatrol()
        {
            return CanPatrol(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a patrol command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#patrol
        /// </remarks>
        public bool CanPatrol(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanMove(false);
        }

        public bool CanPatrolGrouped(bool checkCommandibilityGrouped)
        {
            return CanPatrolGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanPatrolGrouped()
        {
            return CanPatrolGrouped(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a patrol command, as part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canPatrol
        /// </remarks>
        public bool CanPatrolGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            return CanMoveGrouped(false, false);
        }

        public bool CanFollow()
        {
            return CanFollow(true);
        }

        public bool CanFollow(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanMove(false);
        }

        public bool CanFollow(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanFollow(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanFollow(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanFollow(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanFollow(Unit targetUnit)
        {
            return CanFollow(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a follow command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#follow
        /// </remarks>
        public bool CanFollow(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanFollow(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            return targetUnit != this;
        }

        public bool CanGather()
        {
            return CanGather(true);
        }

        public bool CanGather(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (!ut.IsWorker())
            {
                return false;
            }

            if (GetPowerUp() != null)
            {
                return false;
            }

            if (IsHallucination())
            {
                return false;
            }

            if (IsBurrowed())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            return GetOrder() != Order.ConstructingBuilding;
        }

        public bool CanGather(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanGather(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanGather(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanGather(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanGather(Unit targetUnit)
        {
            return CanGather(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a gather command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#gather
        /// </remarks>
        public bool CanGather(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanGather(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            UnitType uType = targetUnit.GetUnitType();
            if (!uType.IsResourceContainer() || uType == UnitType.Resource_Vespene_Geyser)
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            if (!HasPath(GetPosition()))
            {
                return false;
            }

            return !uType.IsRefinery() || GetPlayer().Equals(_game.Self());
        }

        public bool CanReturnCargo()
        {
            return CanReturnCargo(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a returnCargo command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#returnCargo
        /// </remarks>
        public bool CanReturnCargo(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (!ut.IsWorker())
            {
                return false;
            }

            if (!IsCarryingGas() && !IsCarryingMinerals())
            {
                return false;
            }

            if (IsBurrowed())
            {
                return false;
            }

            return GetOrder() != Order.ConstructingBuilding;
        }

        public bool CanHoldPosition()
        {
            return CanHoldPosition(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a holdPosition command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#holdPosition
        /// </remarks>
        public bool CanHoldPosition(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding())
            {
                if (!ut.CanMove())
                {
                    return false;
                }

                if (IsBurrowed() && ut != UnitType.Zerg_Lurker)
                {
                    return false;
                }

                if (GetOrder() == Order.ConstructingBuilding)
                {
                    return false;
                }

                if (ut == UnitType.Zerg_Larva)
                {
                    return false;
                }
            }
            else
            {
                if (!ut.IsFlyingBuilding())
                {
                    return false;
                }

                if (!IsLifted())
                {
                    return false;
                }
            }

            return IsCompleted();
        }

        public bool CanStop()
        {
            return CanStop(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a stop command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#stop
        /// </remarks>
        public bool CanStop(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (IsBurrowed() && ut != UnitType.Zerg_Lurker)
            {
                return false;
            }

            return !ut.IsBuilding() || IsLifted() || ut == UnitType.Protoss_Photon_Cannon || ut == UnitType.Zerg_Sunken_Colony || ut == UnitType.Zerg_Spore_Colony || ut == UnitType.Terran_Missile_Turret;
        }

        public bool CanRepair()
        {
            return CanRepair(true);
        }

        public bool CanRepair(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!IsInterruptible())
            {
                return false;
            }

            if (GetUnitType() != UnitType.Terran_SCV)
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            if (IsHallucination())
            {
                return false;
            }

            return GetOrder() != Order.ConstructingBuilding;
        }

        public bool CanRepair(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRepair(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRepair(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanRepair(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanRepair(Unit targetUnit)
        {
            return CanRepair(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a repair command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#repair
        /// </remarks>
        public bool CanRepair(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanRepair(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            UnitType targType = targetUnit.GetUnitType();
            if (targType.GetRace() != Race.Terran || !targType.IsMechanical())
            {
                return false;
            }

            if (targetUnit.GetHitPoints() == targType.MaxHitPoints())
            {
                return false;
            }

            if (!targetUnit.IsCompleted())
            {
                return false;
            }

            return targetUnit != this;
        }

        public bool CanBurrow()
        {
            return CanBurrow(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a burrow command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#burrow
        /// </remarks>
        public bool CanBurrow(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanUseTechWithoutTarget(TechType.Burrowing, true, false);
        }

        public bool CanUnburrow()
        {
            return CanUnburrow(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute an unburrow command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unburrow
        /// </remarks>
        public bool CanUnburrow(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsBurrowable())
            {
                return false;
            }

            return IsBurrowed() && GetOrder() != Order.Unburrowing;
        }

        public bool CanCloak()
        {
            return CanCloak(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cloak command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cloak
        /// </remarks>
        public bool CanCloak(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanUseTechWithoutTarget(GetUnitType().CloakingTech(), true, false);
        }

        public bool CanDecloak()
        {
            return CanDecloak(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a decloak command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#decloak
        /// </remarks>
        public bool CanDecloak(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (GetUnitType().CloakingTech() == TechType.None)
            {
                return false;
            }

            return GetSecondaryOrder() == Order.Cloak;
        }

        public bool CanSiege()
        {
            return CanSiege(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a siege command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#siege
        /// </remarks>
        public bool CanSiege(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanUseTechWithoutTarget(TechType.Tank_Siege_Mode, true, false);
        }

        public bool CanUnsiege()
        {
            return CanUnsiege(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute an unsiege command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unsiege
        /// </remarks>
        public bool CanUnsiege(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!IsSieged())
            {
                return false;
            }

            Order order = GetOrder();
            if (order == Order.Sieging || order == Order.Unsieging)
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanLift()
        {
            return CanLift(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a lift command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#lift
        /// </remarks>
        public bool CanLift(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsFlyingBuilding())
            {
                return false;
            }

            if (IsLifted())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            return IsIdle();
        }

        public bool CanLand()
        {
            return CanLand(true);
        }

        public bool CanLand(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsFlyingBuilding())
            {
                return false;
            }

            return IsLifted();
        }

        public bool CanLand(TilePosition target, bool checkCanIssueCommandType)
        {
            return CanLand(target, checkCanIssueCommandType, true);
        }

        public bool CanLand(TilePosition target)
        {
            return CanLand(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a land command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#land
        /// </remarks>
        public bool CanLand(TilePosition target, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanLand(checkCommandibility))
            {
                return false;
            }

            return _game.CanBuildHere(target, GetUnitType(), null, true);
        }

        public bool CanLoad()
        {
            return CanLoad(true);
        }

        public bool CanLoad(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            if (ut == UnitType.Zerg_Overlord && _game.Self().GetUpgradeLevel(UpgradeType.Ventral_Sacs) == 0)
            {
                return false;
            }

            if (IsBurrowed())
            {
                return false;
            }

            if (GetOrder() == Order.ConstructingBuilding)
            {
                return false;
            }

            return ut != UnitType.Zerg_Larva;
        }

        public bool CanLoad(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanLoad(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanLoad(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanLoad(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanLoad(Unit targetUnit)
        {
            return CanLoad(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a load command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#load
        /// </remarks>
        public bool CanLoad(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanLoad(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            Player self = _game.Self();

            //target must also be owned by self
            if (!targetUnit.GetPlayer().Equals(self))
            {
                return false;
            }

            if (targetUnit.IsLoaded() || !targetUnit.IsCompleted())
            {
                return false;
            }


            // verify upgrade for Zerg Overlord
            if (targetUnit.GetUnitType() == UnitType.Zerg_Overlord && self.GetUpgradeLevel(UpgradeType.Ventral_Sacs) == 0)
            {
                return false;
            }

            int thisUnitSpaceProvided = GetUnitType().SpaceProvided();
            int targetSpaceProvided = targetUnit.GetUnitType().SpaceProvided();
            if (thisUnitSpaceProvided <= 0 && targetSpaceProvided <= 0)
            {
                return false;
            }

            Unit unitToBeLoaded = thisUnitSpaceProvided > 0 ? targetUnit : this;
            UnitType unitToBeLoadedType = unitToBeLoaded.GetUnitType();
            if (!unitToBeLoadedType.CanMove() || unitToBeLoadedType.IsFlyer() || unitToBeLoadedType.SpaceRequired() > 8)
            {
                return false;
            }

            if (!unitToBeLoaded.IsCompleted())
            {
                return false;
            }

            if (unitToBeLoaded.IsBurrowed())
            {
                return false;
            }

            Unit unitThatLoads = thisUnitSpaceProvided > 0 ? this : targetUnit;
            if (unitThatLoads.IsHallucination())
            {
                return false;
            }

            UnitType unitThatLoadsType = unitThatLoads.GetUnitType();
            if (unitThatLoadsType == UnitType.Terran_Bunker)
            {
                if (!unitToBeLoadedType.IsOrganic() || unitToBeLoadedType.GetRace() != Race.Terran)
                {
                    return false;
                }

                if (!unitToBeLoaded.HasPath(unitThatLoads.GetPosition()))
                {
                    return false;
                }
            }

            int freeSpace = thisUnitSpaceProvided > 0 ? thisUnitSpaceProvided : targetSpaceProvided;
            foreach (Unit u in unitThatLoads.GetLoadedUnits())
            {
                int requiredSpace = u.GetUnitType().SpaceRequired();
                if (requiredSpace > 0 && requiredSpace < 8)
                {
                    freeSpace -= requiredSpace;
                }
            }

            return unitToBeLoadedType.SpaceRequired() <= freeSpace;
        }

        public bool CanUnloadWithOrWithoutTarget()
        {
            return CanUnloadWithOrWithoutTarget(true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an unload command or unloadAll at
        /// current position command or unloadAll at a different position command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unload
        /// @seeUnit#unloadAll
        /// </remarks>
        public bool CanUnloadWithOrWithoutTarget(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            UnitType ut = GetUnitType();
            if (!ut.IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (GetLoadedUnits().Count == 0)
            {
                return false;
            }


            // Check overlord tech
            if (ut == UnitType.Zerg_Overlord && _game.Self().GetUpgradeLevel(UpgradeType.Ventral_Sacs) == 0)
            {
                return false;
            }

            return ut.SpaceProvided() > 0;
        }

        public bool CanUnloadAtPosition(Position targDropPos, bool checkCanIssueCommandType)
        {
            return CanUnloadAtPosition(targDropPos, checkCanIssueCommandType, true);
        }

        public bool CanUnloadAtPosition(Position targDropPos)
        {
            return CanUnloadAtPosition(targDropPos, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an unload command or unloadAll at
        /// current position command or unloadAll at a different position command, for a given
        /// position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unload
        /// @seeUnit#unloadAll
        /// </remarks>
        public bool CanUnloadAtPosition(Position targDropPos, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUnloadWithOrWithoutTarget(false))
            {
                return false;
            }

            if (GetUnitType() != UnitType.Terran_Bunker)
            {
                if (!new WalkPosition(targDropPos.x / 8, targDropPos.y / 8).IsValid(_game))
                {
                    return false;
                }
                else
                    return _game.IsWalkable(targDropPos.x / 8, targDropPos.y / 8);
            }

            return true;
        }

        public bool CanUnload()
        {
            return CanUnload(true);
        }

        public bool CanUnload(bool checkCommandibility)
        {
            return CanUnloadAtPosition(GetPosition(), true, checkCommandibility);
        }

        public bool CanUnload(Unit targetUnit, bool checkCanTargetUnit, bool checkPosition, bool checkCanIssueCommandType)
        {
            return CanUnload(targetUnit, checkCanTargetUnit, checkPosition, checkCanIssueCommandType, true);
        }

        public bool CanUnload(Unit targetUnit, bool checkCanTargetUnit, bool checkPosition)
        {
            return CanUnload(targetUnit, checkCanTargetUnit, checkPosition, true);
        }

        public bool CanUnload(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanUnload(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanUnload(Unit targetUnit)
        {
            return CanUnload(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an unload command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unload
        /// </remarks>
        public bool CanUnload(Unit targetUnit, bool checkCanTargetUnit, bool checkPosition, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUnloadWithOrWithoutTarget(false))
            {
                return false;
            }

            if (checkPosition && !CanUnloadAtPosition(GetPosition(), false, false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            if (!targetUnit.IsLoaded())
            {
                return false;
            }

            return Equals(targetUnit.GetTransport());
        }

        public bool CanUnloadAll()
        {
            return CanUnloadAll(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute an unloadAll command for the current position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unloadAll
        /// </remarks>
        public bool CanUnloadAll(bool checkCommandibility)
        {
            return CanUnloadAtPosition(GetPosition(), true, checkCommandibility);
        }

        public bool CanUnloadAllPosition()
        {
            return CanUnloadAllPosition(true);
        }

        public bool CanUnloadAllPosition(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!CanUnloadWithOrWithoutTarget(false))
            {
                return false;
            }

            return GetUnitType() != UnitType.Terran_Bunker;
        }

        public bool CanUnloadAllPosition(Position targDropPos, bool checkCanIssueCommandType)
        {
            return CanUnloadAllPosition(targDropPos, checkCanIssueCommandType, true);
        }

        public bool CanUnloadAllPosition(Position targDropPos)
        {
            return CanUnloadAllPosition(targDropPos, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute an unloadAll command for a different
        /// position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#unloadAll
        /// </remarks>
        public bool CanUnloadAllPosition(Position targDropPos, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUnloadAllPosition(false))
            {
                return false;
            }

            return CanUnloadAtPosition(targDropPos, false, false);
        }

        public bool CanRightClick()
        {
            return CanRightClick(true);
        }

        public bool CanRightClick(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanRightClickPosition(false) || CanRightClickUnit(false);
        }

        public bool CanRightClick(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClick(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClick(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClick(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClick(Position target, bool checkCanTargetUnit)
        {
            return CanRightClick(target, checkCanTargetUnit, true);
        }

        public bool CanRightClick(Unit target, bool checkCanTargetUnit)
        {
            return CanRightClick(target, checkCanTargetUnit, true);
        }

        public bool CanRightClick(Position target)
        {
            return CanRightClick(target, true);
        }

        public bool CanRightClick(Unit target)
        {
            return CanRightClick(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a rightClick command to a position
        /// or unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#rightClick
        /// @seeUnit#canRightClickPosition
        /// @seeUnit#canRightClickUnit
        /// </remarks>
#pragma warning disable IDE0060
        public bool CanRightClick(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
#pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanRightClickPosition(false);
        }

        public bool CanRightClick(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            return CanRightClickUnit(target, checkCanTargetUnit, checkCanIssueCommandType, false);
        }

        public bool CanRightClickGrouped(bool checkCommandibilityGrouped)
        {
            return CanRightClickGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanRightClickGrouped()
        {
            return CanRightClickGrouped(true);
        }

        public bool CanRightClickGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            return CanRightClickPositionGrouped(false, false) || CanRightClickUnitGrouped(false, false);
        }

        public bool CanRightClickGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanRightClickGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanRightClickGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClickGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClickGrouped(Position target, bool checkCanTargetUnit)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, true);
        }

        public bool CanRightClickGrouped(Unit target, bool checkCanTargetUnit)
        {
            return CanRightClickGrouped(target, checkCanTargetUnit, true);
        }

        public bool CanRightClickGrouped(Position target)
        {
            return CanRightClickGrouped(target, true);
        }

        public bool CanRightClickGrouped(Unit target)
        {
            return CanRightClickGrouped(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a rightClick command to a position
        /// or unit, as part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canRightClickUnit
        /// </remarks>
#pragma warning disable IDE0060
        public bool CanRightClickGrouped(Position target, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped, bool checkCommandibility)
#pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            return CanRightClickPositionGrouped(false, false);
        }

        public bool CanRightClickGrouped(Unit target, bool checkCanTargetUnit, bool checkCanIssueCommandTypeGrouped, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            return CanRightClickUnitGrouped(target, checkCanTargetUnit, checkCanIssueCommandTypeGrouped, false, false);
        }

        public bool CanRightClickPosition()
        {
            return CanRightClickPosition(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a rightClick command for a position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#rightClick
        /// </remarks>
        public bool CanRightClickPosition(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            return CanMove(false) || CanSetRallyPosition(false);
        }

        public bool CanRightClickPositionGrouped(bool checkCommandibilityGrouped)
        {
            return CanRightClickPositionGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanRightClickPositionGrouped()
        {
            return CanRightClickPositionGrouped(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a rightClick command for a position, as part of
        /// a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canRightClick
        /// </remarks>
        public bool CanRightClickPositionGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (!IsInterruptible())
            {
                return false;
            }

            return CanMoveGrouped(false, false);
        }

        public bool CanRightClickUnit()
        {
            return CanRightClickUnit(true);
        }

        public bool CanRightClickUnit(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            return CanFollow(false) || CanAttackUnit(false) || CanLoad(false) || CanSetRallyUnit(false);
        }

        public bool CanRightClickUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClickUnit(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClickUnit(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanRightClickUnit(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanRightClickUnit(Unit targetUnit)
        {
            return CanRightClickUnit(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a rightClick command to a unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#rightClick
        /// </remarks>
        public bool CanRightClickUnit(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanRightClickUnit(false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            if (!targetUnit.GetPlayer().IsNeutral() && GetPlayer().IsEnemy(targetUnit.GetPlayer()) && !CanAttackUnit(targetUnit, false, true, false))
            {
                return false;
            }

            return CanFollow(targetUnit, false, true, false) || CanLoad(targetUnit, false, true, false) || CanSetRallyUnit(targetUnit, false, true, false);
        }

        public bool CanRightClickUnitGrouped(bool checkCommandibilityGrouped)
        {
            return CanRightClickUnitGrouped(checkCommandibilityGrouped, true);
        }

        public bool CanRightClickUnitGrouped()
        {
            return CanRightClickUnitGrouped(true);
        }

        public bool CanRightClickUnitGrouped(bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (!IsInterruptible())
            {
                return false;
            }

            return CanFollow(false) || CanAttackUnitGrouped(false, false) || CanLoad(false);
        }

        public bool CanRightClickUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped)
        {
            return CanRightClickUnitGrouped(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, checkCommandibilityGrouped, true);
        }

        public bool CanRightClickUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType)
        {
            return CanRightClickUnitGrouped(targetUnit, checkCanTargetUnit, checkCanIssueCommandType, true);
        }

        public bool CanRightClickUnitGrouped(Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanRightClickUnitGrouped(targetUnit, checkCanTargetUnit, true);
        }

        public bool CanRightClickUnitGrouped(Unit targetUnit)
        {
            return CanRightClickUnitGrouped(targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a rightClick command to a unit, as
        /// part of a List<Unit>.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommandGrouped
        /// @seeUnit#canRightClickUnit
        /// </remarks>
        public bool CanRightClickUnitGrouped(Unit targetUnit, bool checkCanTargetUnit, bool checkCanIssueCommandType, bool checkCommandibilityGrouped, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCommandibilityGrouped && !CanCommandGrouped(false))
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanRightClickUnitGrouped(false, false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            if (!targetUnit.GetPlayer().IsNeutral() && GetPlayer().IsEnemy(targetUnit.GetPlayer()) && !CanAttackUnitGrouped(targetUnit, false, true, false, false))
            {
                return false;
            }

            return CanFollow(targetUnit, false, true, false) || CanLoad(targetUnit, false, true, false);
        }

        public bool CanHaltConstruction()
        {
            return CanHaltConstruction(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a haltConstruction command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#haltConstruction
        /// </remarks>
        public bool CanHaltConstruction(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return GetOrder() == Order.ConstructingBuilding;
        }

        //------------------------------------------- CAN CANCEL CONSTRUCTION ------------------------------------
        public bool CanCancelConstruction()
        {
            return CanCancelConstruction(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelConstruction command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelConstruction
        /// </remarks>
        public bool CanCancelConstruction(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsBuilding())
            {
                return false;
            }

            return !IsCompleted() && (GetUnitType() != UnitType.Zerg_Nydus_Canal || GetNydusExit() == null);
        }

        public bool CanCancelAddon()
        {
            return CanCancelAddon(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelAddon command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelAddon
        /// </remarks>
        public bool CanCancelAddon(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            Unit addon = GetAddon();
            return addon != null && !addon.IsCompleted();
        }

        public bool CanCancelTrain()
        {
            return CanCancelTrain(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelTrain command for any slot.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelTrain
        /// </remarks>
        public bool CanCancelTrain(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return IsTraining();
        }

        public bool CanCancelTrainSlot()
        {
            return CanCancelTrainSlot(true);
        }

        public bool CanCancelTrainSlot(bool checkCommandibility)
        {
            return CanCancelTrain(checkCommandibility);
        }

        public bool CanCancelTrainSlot(int slot, bool checkCanIssueCommandType)
        {
            return CanCancelTrainSlot(slot, checkCanIssueCommandType, true);
        }

        public bool CanCancelTrainSlot(int slot)
        {
            return CanCancelTrainSlot(slot, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a cancelTrain command for an
        /// unspecified slot.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelTrain
        /// </remarks>
        public bool CanCancelTrainSlot(int slot, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanCancelTrainSlot(false))
            {
                return false;
            }

            return IsTraining() && slot >= 0 && GetTrainingQueue().Count > slot;
        }

        public bool CanCancelMorph()
        {
            return CanCancelMorph(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelMorph command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelMorph
        /// </remarks>
        public bool CanCancelMorph(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!IsMorphing() || (!IsCompleted() && GetUnitType() == UnitType.Zerg_Nydus_Canal && GetNydusExit() != null))
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanCancelResearch()
        {
            return CanCancelResearch(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelResearch command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelResearch
        /// </remarks>
        public bool CanCancelResearch(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return GetOrder() == Order.ResearchTech;
        }

        public bool CanCancelUpgrade()
        {
            return CanCancelUpgrade(true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a cancelUpgrade command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#cancelUpgrade
        /// </remarks>
        public bool CanCancelUpgrade(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return GetOrder() == Order.Upgrade;
        }

        public bool CanUseTechWithOrWithoutTarget()
        {
            return CanUseTechWithOrWithoutTarget(true);
        }

        public bool CanUseTechWithOrWithoutTarget(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsBuilding() && !IsInterruptible())
            {
                return false;
            }

            if (!IsCompleted())
            {
                return false;
            }

            return !IsHallucination();
        }

        public bool CanUseTechWithOrWithoutTarget(TechType tech, bool checkCanIssueCommandType)
        {
            return CanUseTechWithOrWithoutTarget(tech, checkCanIssueCommandType, true);
        }

        public bool CanUseTechWithOrWithoutTarget(TechType tech)
        {
            return CanUseTechWithOrWithoutTarget(tech, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a useTech command without a target or
        /// or a useTech command with a target position or a useTech command with a target unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#useTech
        /// </remarks>
        public bool CanUseTechWithOrWithoutTarget(TechType tech, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            UnitType ut = GetUnitType();

            // researched check
            if (!ut.IsHero() && !_game.Self().HasResearched(tech) && ut != UnitType.Zerg_Lurker)
            {
                return false;
            }


            // energy check
            if (GetEnergy() < tech.EnergyCost())
            {
                return false;
            }


            // unit check
            if (tech != TechType.Burrowing && !tech.WhatUses().Contains(ut))
            {
                return false;
            }

            Order order = GetOrder();
            switch (tech)
            {
                case TechType.Spider_Mines:
                    return GetSpiderMineCount() > 0;
                case TechType.Tank_Siege_Mode:
                    return !IsSieged() && order != Order.Sieging && order != Order.Unsieging;
                case TechType.Cloaking_Field:
                case TechType.Personnel_Cloaking:
                    return GetSecondaryOrder() != Order.Cloak;
                case TechType.Burrowing:
                    return ut.IsBurrowable() && !IsBurrowed() && order != Order.Burrowing && order != Order.Unburrowing;
                case TechType.None:
                    return false;
                case TechType.Nuclear_Strike:
                    return GetPlayer().CompletedUnitCount(UnitType.Terran_Nuclear_Missile) != 0;
                case TechType.Unknown:
                    return false;
            }

            return true;
        }

        public bool CanUseTech(TechType tech, Position target, bool checkCanTargetUnit, bool checkTargetsType, bool checkCanIssueCommandType)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, checkTargetsType, checkCanIssueCommandType, true);
        }

        public bool CanUseTech(TechType tech, Unit target, bool checkCanTargetUnit, bool checkTargetsType, bool checkCanIssueCommandType)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, checkTargetsType, checkCanIssueCommandType, true);
        }

        public bool CanUseTech(TechType tech, Position target, bool checkCanTargetUnit, bool checkTargetsType)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, checkTargetsType, true);
        }

        public bool CanUseTech(TechType tech, Unit target, bool checkCanTargetUnit, bool checkTargetsType)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, checkTargetsType, true);
        }

        public bool CanUseTech(TechType tech, Position target, bool checkCanTargetUnit)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, true);
        }

        public bool CanUseTech(TechType tech, Unit target, bool checkCanTargetUnit)
        {
            return CanUseTech(tech, target, checkCanTargetUnit, true);
        }

        public bool CanUseTech(TechType tech, Position target)
        {
            return CanUseTech(tech, target, true);
        }

        public bool CanUseTech(TechType tech, Unit target)
        {
            return CanUseTech(tech, target, true);
        }

        public bool CanUseTech(TechType tech)
        {
            return CanUseTech(tech, (Unit)null);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a useTech command for a specified position or
        /// unit (only specify null if the TechType does not target another position/unit).
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#useTech
        /// @seeUnit#canUseTechWithoutTarget
        /// @seeUnit#canUseTechUnit
        /// @seeUnit#canUseTechPosition
        /// </remarks>
#pragma warning disable IDE0060
        public bool CanUseTech(TechType tech, Position target, bool checkCanTargetUnit, bool checkTargetsType, bool checkCanIssueCommandType, bool checkCommandibility)
#pragma warning restore IDE0060
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            return CanUseTechPosition(tech, target, checkTargetsType, checkCanIssueCommandType, false);
        }

        public bool CanUseTech(TechType tech, Unit target, bool checkCanTargetUnit, bool checkTargetsType, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (target == null)
            {
                return CanUseTechWithoutTarget(tech, checkCanIssueCommandType, false);
            }

            return CanUseTechUnit(tech, target, checkCanTargetUnit, checkTargetsType, checkCanIssueCommandType, false);
        }

        public bool CanUseTechWithoutTarget(TechType tech, bool checkCanIssueCommandType)
        {
            return CanUseTechWithoutTarget(tech, checkCanIssueCommandType, true);
        }

        public bool CanUseTechWithoutTarget(TechType tech)
        {
            return CanUseTechWithoutTarget(tech, true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a useTech command without a target.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#useTech
        /// </remarks>
        public bool CanUseTechWithoutTarget(TechType tech, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            if (!CanUseTechWithOrWithoutTarget(tech, false, false))
            {
                return false;
            }

            return !tech.TargetsUnit() && !tech.TargetsPosition() && tech != TechType.None && tech != TechType.Unknown && tech != TechType.Lurker_Aspect;
        }

        public bool CanUseTechUnit(TechType tech, bool checkCanIssueCommandType)
        {
            return CanUseTechUnit(tech, checkCanIssueCommandType, true);
        }

        public bool CanUseTechUnit(TechType tech)
        {
            return CanUseTechUnit(tech, true);
        }

        public bool CanUseTechUnit(TechType tech, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            if (!CanUseTechWithOrWithoutTarget(tech, false, false))
            {
                return false;
            }

            return tech.TargetsUnit();
        }

        public bool CanUseTechUnit(TechType tech, Unit targetUnit, bool checkCanTargetUnit, bool checkTargetsUnits, bool checkCanIssueCommandType)
        {
            return CanUseTech(tech, targetUnit, checkCanTargetUnit, checkTargetsUnits, checkCanIssueCommandType, true);
        }

        public bool CanUseTechUnit(TechType tech, Unit targetUnit, bool checkCanTargetUnit, bool checkTargetsUnits)
        {
            return CanUseTech(tech, targetUnit, checkCanTargetUnit, checkTargetsUnits, true);
        }

        public bool CanUseTechUnit(TechType tech, Unit targetUnit, bool checkCanTargetUnit)
        {
            return CanUseTech(tech, targetUnit, checkCanTargetUnit, true);
        }

        public bool CanUseTechUnit(TechType tech, Unit targetUnit)
        {
            return CanUseTech(tech, targetUnit, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a useTech command with an unspecified
        /// target unit.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#useTech
        /// </remarks>
        public bool CanUseTechUnit(TechType tech, Unit targetUnit, bool checkCanTargetUnit, bool checkTargetsUnits, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            if (checkTargetsUnits && !CanUseTechUnit(tech, false, false))
            {
                return false;
            }

            if (checkCanTargetUnit && !CanTargetUnit(targetUnit, false))
            {
                return false;
            }

            UnitType targetType = targetUnit.GetUnitType();
            switch (tech)
            {
                case TechType.Archon_Warp:
                {
                    if (targetType != UnitType.Protoss_High_Templar)
                    {
                        return false;
                    }

                    if (!GetPlayer().Equals(targetUnit.GetPlayer()))
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Dark_Archon_Meld:
                {
                    if (targetType != UnitType.Protoss_Dark_Templar)
                    {
                        return false;
                    }

                    if (!GetPlayer().Equals(targetUnit.GetPlayer()))
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Consume:
                {
                    if (!GetPlayer().Equals(targetUnit.GetPlayer()))
                    {
                        return false;
                    }

                    if (targetType.GetRace() != Race.Zerg || targetType == UnitType.Zerg_Larva)
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Spawn_Broodlings:
                {
                    if ((!targetType.IsOrganic() && !targetType.IsMechanical()) || targetType.IsRobotic() || targetType.IsFlyer())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Lockdown:
                {
                    if (!targetType.IsMechanical())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Healing:
                {
                    if (targetUnit.GetHitPoints() == targetType.MaxHitPoints())
                    {
                        return false;
                    }

                    if (!targetType.IsOrganic() || targetType.IsFlyer())
                    {
                        return false;
                    }

                    if (!targetUnit.GetPlayer().IsNeutral() && GetPlayer().IsEnemy(GetPlayer()))
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Mind_Control:
                {
                    if (GetPlayer().Equals(targetUnit.GetPlayer()))
                    {
                        return false;
                    }

                    if (targetType == UnitType.Protoss_Interceptor || targetType == UnitType.Terran_Vulture_Spider_Mine || targetType == UnitType.Zerg_Lurker_Egg || targetType == UnitType.Zerg_Cocoon || targetType == UnitType.Zerg_Larva || targetType == UnitType.Zerg_Egg)
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Feedback:
                {
                    if (!targetType.IsSpellcaster())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Infestation:
                {
                    if (targetType != UnitType.Terran_Command_Center || targetUnit.GetHitPoints() >= 750 || targetUnit.GetHitPoints() <= 0)
                    {
                        return false;
                    }

                    break;
                }
            }

            switch (tech)
            {
                case TechType.Archon_Warp:
                case TechType.Dark_Archon_Meld:
                {
                    if (!HasPath(targetUnit.GetPosition()))
                    {
                        return false;
                    }

                    if (targetUnit.IsHallucination())
                    {
                        return false;
                    }

                    if (targetUnit.IsMaelstrommed())
                    {
                        return false;
                    }

                    if (targetUnit.IsStasised())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Parasite:
                case TechType.Irradiate:
                case TechType.Optical_Flare:
                case TechType.Spawn_Broodlings:
                case TechType.Lockdown:
                case TechType.Defensive_Matrix:
                case TechType.Hallucination:
                case TechType.Healing:
                case TechType.Restoration:
                case TechType.Mind_Control:
                case TechType.Consume:
                case TechType.Feedback:
                case TechType.Yamato_Gun:
                {
                    if (targetUnit.IsStasised())
                    {
                        return false;
                    }

                    break;
                }
            }

            switch (tech)
            {
                case TechType.Yamato_Gun:
                {
                    if (targetUnit.IsInvincible())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Parasite:
                case TechType.Irradiate:
                case TechType.Optical_Flare:
                case TechType.Spawn_Broodlings:
                case TechType.Lockdown:
                case TechType.Defensive_Matrix:
                case TechType.Hallucination:
                case TechType.Healing:
                case TechType.Restoration:
                case TechType.Mind_Control:
                {
                    if (targetUnit.IsInvincible())
                    {
                        return false;
                    }

                    if (targetType.IsBuilding())
                    {
                        return false;
                    }

                    break;
                }
                case TechType.Consume:
                case TechType.Feedback:
                {
                    if (targetType.IsBuilding())
                    {
                        return false;
                    }

                    break;
                }
            }

            return targetUnit != this;
        }

        public bool CanUseTechPosition(TechType tech, bool checkCanIssueCommandType)
        {
            return CanUseTechPosition(tech, checkCanIssueCommandType, true);
        }

        public bool CanUseTechPosition(TechType tech)
        {
            return CanUseTechPosition(tech, true);
        }

        public bool CanUseTechPosition(TechType tech, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            if (!CanUseTechWithOrWithoutTarget(tech, false, false))
            {
                return false;
            }

            return tech.TargetsPosition();
        }

        public bool CanUseTechPosition(TechType tech, Position target, bool checkTargetsPositions, bool checkCanIssueCommandType)
        {
            return CanUseTechPosition(tech, target, checkTargetsPositions, checkCanIssueCommandType, true);
        }

        public bool CanUseTechPosition(TechType tech, Position target, bool checkTargetsPositions)
        {
            return CanUseTechPosition(tech, target, checkTargetsPositions, true);
        }

        public bool CanUseTechPosition(TechType tech, Position target)
        {
            return CanUseTechPosition(tech, target, true);
        }

        /// <summary>
        /// Checks whether the unit is able to execute a useTech command with an unspecified target
        /// position.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#useTech
        /// </remarks>
        public bool CanUseTechPosition(TechType tech, Position target, bool checkTargetsPositions, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanUseTechWithOrWithoutTarget(false))
            {
                return false;
            }

            if (checkTargetsPositions && !CanUseTechPosition(tech, false, false))
            {
                return false;
            }

            return tech != TechType.Spider_Mines || HasPath(target);
        }

        public bool CanPlaceCOP()
        {
            return CanPlaceCOP(true);
        }

        public bool CanPlaceCOP(bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (!GetUnitType().IsFlagBeacon())
            {
                return false;
            }

            return _unitData.GetButtonset() != 228 && GetOrder() == Order.CTFCOPInit;
        }

        public bool CanPlaceCOP(TilePosition target, bool checkCanIssueCommandType)
        {
            return CanPlaceCOP(target, checkCanIssueCommandType, true);
        }

        public bool CanPlaceCOP(TilePosition target)
        {
            return CanPlaceCOP(target, true);
        }

        /// <summary>
        /// Cheap checks for whether the unit is able to execute a placeCOP command.
        /// </summary>
        /// <remarks>
        /// @seeUnit#canIssueCommand
        /// @seeUnit#placeCOP
        /// </remarks>
        public bool CanPlaceCOP(TilePosition target, bool checkCanIssueCommandType, bool checkCommandibility)
        {
            if (checkCommandibility && !CanCommand())
            {
                return false;
            }

            if (checkCanIssueCommandType && !CanPlaceCOP(checkCommandibility))
            {
                return false;
            }

            return _game.CanBuildHere(target, GetUnitType(), this, true);
        }

        public bool Equals(Unit other)
        {
            return _id == other._id;
        }

        public override bool Equals(object o)
        {
            return o is Unit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(Unit other)
        {
            return _id - other._id;
        }

        public void UpdatePosition(int frame)
        {
            if (frame > _lastPositionUpdate)
            {
                _lastPositionUpdate = frame;
                _position = new Position(_unitData.GetPositionX(), _unitData.GetPositionY());
            }
        }
    }
}