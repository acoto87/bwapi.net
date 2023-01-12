using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BWAPI.NET
{
    /// <summary>
    /// The {@link Game} class is implemented by BWAPI and is the primary means of obtaining all
    /// game state information from Starcraft Broodwar. Game state information includes all units,
    /// resources, players, forces, bullets, terrain, fog of war, regions, etc.
    /// </summary>
    public sealed class Game
    {
        private const int RegionDataSize = 5000;

        private static readonly int[][] _damageRatio = new[]
        {
            new[] { 0, 0, 0, 0, 0, 0 },
            new[] { 0, 128, 192, 256, 0, 0 },
            new[] { 0, 256, 128, 64, 0, 0 },
            new[] { 0, 256, 256, 256, 0, 0 },
            new[] { 0, 256, 256, 256, 0, 0 },
            new[] { 0, 0, 0, 0, 0, 0 },
            new[] { 0, 0, 0, 0, 0, 0 }
        };

        private static readonly bool[][] _psiFieldMask = new[]
        {
            new[] { false, false, false, false, false, true, true, true, true, true, true, false, false, false, false, false },
            new[] { false, false, true, true, true, true, true, true, true, true, true, true, true, true, false, false },
            new[] { false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new[] { false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false },
            new[] { false, false, true, true, true, true, true, true, true, true, true, true, true, true, false, false },
            new[] { false, false, false, false, false, true, true, true, true, true, true, false, false, false, false, false }
        };

        private readonly ClientData _clientData;

        private readonly HashSet<int> _visibleUnits;
        private List<Unit> _allUnits;
        private List<Unit> _staticMinerals;
        private List<Unit> _staticGeysers;
        private List<Unit> _staticNeutralUnits;
        private List<Player> allies;
        private List<Player> enemies;
        private List<Player> observers;

        // CONSTANT
        private Player[] _players;
        private Region[] _regions;
        private Force[] _forces;
        private Bullet[] _bullets;

        private List<Force> _forceSet;
        private List<Player> _playerSet;
        private List<Region> _regionSet;

        // CHANGING
        private Unit[] _units;

        //CACHED
        private int _randomSeed;
        private int _revision;
        private bool _debug;
        private Player _self;
        private Player _enemy;
        private Player _neutral;
        private bool _replay;
        private bool _multiplayer;
        private bool _battleNet;
        private List<TilePosition> _startLocations;
        private int _mapWidth;
        private int _mapHeight;
        private int _mapPixelWidth;
        private int _mapPixelHeight;
        private string _mapFileName;
        private string _mapPathName;
        private string _mapName;
        private string _mapHash;
        private bool[,] _buildable;
        private bool[,] _walkable;
        private int[,] _groundHeight;
        private short[,] _mapTileRegionID;
        private short[] _mapSplitTilesMiniTileMask;
        private short[] _mapSplitTilesRegion1;
        private short[] _mapSplitTilesRegion2;

        // USER DEFINED
        private TextSize textSize = TextSize.Default;
        private bool latcom = true;

        internal readonly ConnectedUnitCache _loadedUnitsCache;
        internal readonly ConnectedUnitCache _interceptorsCache;
        internal readonly ConnectedUnitCache _larvaCache;

        internal readonly SideEffectQueue sideEffects = new SideEffectQueue();

        public Game(ClientData clientData)
        {
            _clientData = clientData;

            _visibleUnits = new HashSet<int>();

            _loadedUnitsCache = new ConnectedUnitCache(this, x => x.GetTransport());
            _interceptorsCache = new ConnectedUnitCache(this, x => x.GetCarrier());
            _larvaCache = new ConnectedUnitCache(this, x => x.GetHatchery());
        }

        /*
         Call this method in EventHander::OnMatchStart
         */
        public void Init()
        {
            _visibleUnits.Clear();

            _loadedUnitsCache.Reset();
            _interceptorsCache.Reset();
            _larvaCache.Reset();

            int forceCount = _clientData.GameData.GetForceCount();
            _forces = new Force[forceCount];
            for (int id = 0; id < forceCount; id++)
            {
                _forces[id] = new Force(id, _clientData.GameData.GetForces(id), this);
            }
            _forceSet = _forces.ToList();

            int playerCount = _clientData.GameData.GetPlayerCount();
            _players = new Player[playerCount];
            for (int id = 0; id < playerCount; id++)
            {
                _players[id] = new Player(id, _clientData.GameData.GetPlayers(id), this);
            }
            _playerSet = _players.ToList();

            int bulletCount = 100;
            _bullets = new Bullet[bulletCount];
            for (int id = 0; id < bulletCount; id++)
            {
                _bullets[id] = new Bullet(id, _clientData.GameData.GetBullets(id), this);
            }

            int regionCount = _clientData.GameData.GetRegionCount();
            _regions = new Region[regionCount];
            for (int id = 0; id < regionCount; id++)
            {
                _regions[id] = new Region(_clientData.GameData.GetRegions(id), this);
            }
            _regionSet = _regions.ToList();

            foreach (Region region in _regions)
            {
                region.UpdateNeighbours();
            }

            _units = new Unit[10000];
            _randomSeed = _clientData.GameData.GetRandomSeed();
            _revision = _clientData.GameData.GetRevision();
            _debug = _clientData.GameData.IsDebug();
            _replay = _clientData.GameData.IsReplay();
            _neutral = _players[_clientData.GameData.GetNeutral()];
            _self = IsReplay() ? null : _players[_clientData.GameData.GetSelf()];
            _enemy = IsReplay() ? null : _players[_clientData.GameData.GetEnemy()];
            _multiplayer = _clientData.GameData.IsMultiplayer();
            _battleNet = _clientData.GameData.IsBattleNet();

            int startLocationsCount = _clientData.GameData.GetStartLocationCount();
            _startLocations = new List<TilePosition>();
            for (int i = 0; i < startLocationsCount; i++)
            {
                _startLocations.Add(new TilePosition(_clientData.GameData.GetStartLocations(i)));
            }

            _mapWidth = _clientData.GameData.GetMapWidth();
            _mapHeight = _clientData.GameData.GetMapHeight();
            _mapFileName = _clientData.GameData.GetMapFileName();
            _mapPathName = _clientData.GameData.GetMapPathName();
            _mapName = _clientData.GameData.GetMapName();
            _mapHash = _clientData.GameData.GetMapHash();

            _staticMinerals = new List<Unit>();
            _staticGeysers = new List<Unit>();
            _staticNeutralUnits = new List<Unit>();
            _allUnits = new List<Unit>();

            for (int id = 0; id < _clientData.GameData.GetInitialUnitCount(); id++)
            {
                Unit unit = new Unit(id, _clientData.GameData.GetUnits(id), this);

                //skip ghost units
                if (unit.GetInitialType() == UnitType.Terran_Marine && unit.GetInitialHitPoints() == 0)
                {
                    continue;
                }

                _units[id] = unit;
                _allUnits.Add(unit);

                if (unit.GetUnitType().IsMineralField())
                {
                    _staticMinerals.Add(unit);
                }

                if (unit.GetUnitType() == UnitType.Resource_Vespene_Geyser)
                {
                    _staticGeysers.Add(unit);
                }

                if (unit.GetPlayer().Equals(Neutral()))
                {
                    _staticNeutralUnits.Add(unit);
                }
            }

            _buildable = new bool[_mapWidth, _mapHeight];
            _groundHeight = new int[_mapWidth, _mapHeight];
            _mapTileRegionID = new short[_mapWidth, _mapHeight];

            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    _buildable[x, y] = _clientData.GameData.IsBuildable(x, y);
                    _groundHeight[x, y] = _clientData.GameData.GetGroundHeight(x, y);
                    _mapTileRegionID[x, y] = _clientData.GameData.GetMapTileRegionId(x, y);
                }
            }

            _walkable = new bool[_mapWidth * PointHelper.TileWalkFactor, _mapHeight * PointHelper.TileWalkFactor];

            for (int x = 0; x < _mapWidth * PointHelper.TileWalkFactor; x++)
            {
                for (int y = 0; y < _mapHeight * PointHelper.TileWalkFactor; y++)
                {
                    _walkable[x, y] = _clientData.GameData.IsWalkable(x, y);
                }
            }

            _mapSplitTilesMiniTileMask = new short[RegionDataSize];
            _mapSplitTilesRegion1 = new short[RegionDataSize];
            _mapSplitTilesRegion2 = new short[RegionDataSize];

            for (int i = 0; i < RegionDataSize; i++)
            {
                _mapSplitTilesMiniTileMask[i] = _clientData.GameData.GetMapSplitTilesMiniTileMask(i);
                _mapSplitTilesRegion1[i] = _clientData.GameData.GetMapSplitTilesRegion1(i);
                _mapSplitTilesRegion2[i] = _clientData.GameData.GetMapSplitTilesRegion2(i);
            }

            _mapPixelWidth = _mapWidth * PointHelper.TilePositionScale;
            _mapPixelHeight = _mapHeight * PointHelper.TilePositionScale;

            if (IsReplay())
            {
                enemies = new List<Player>();
                allies = new List<Player>();
                observers = new List<Player>();
            }
            else
            {
                enemies = _players.Where((p) => !p.Equals(Self()) && Self().IsEnemy(p)).ToList();
                allies = _players.Where((p) => !p.Equals(Self()) && Self().IsAlly(p)).ToList();
                observers = _players.Where((p) => !p.Equals(Self()) && p.IsObserver()).ToList();
            }

            SetLatCom(true);
        }

        private static bool HasPower(int x, int y, UnitType unitType, List<Unit> pylons)
        {
            if (unitType >= 0 && unitType < UnitType.None && (!unitType.RequiresPsi() || !unitType.IsBuilding()))
            {
                return true;
            }

            // Loop through all pylons for the current player
            foreach (Unit i in pylons)
            {
                if (!i.Exists() || !i.IsCompleted())
                {
                    continue;
                }

                Position p = i.GetPosition();
                if (Math.Abs(p.x - x) >= 256)
                {
                    continue;
                }

                if (Math.Abs(p.y - y) >= 160)
                {
                    continue;
                }

                if (_psiFieldMask[(y - p.y + 160) / 32][(x - p.x + 256) / 32])
                {
                    return true;
                }
            }

            return false;
        }

        public void UnitCreate(int id)
        {
            if (id >= _units.Length)
            {
                // rescale unit array if needed
                Unit[] largerUnitsArray = new Unit[2 * _units.Length];
                Array.Copy(_units, 0, largerUnitsArray, 0, _units.Length);
                _units = largerUnitsArray;
            }

            if (_units[id] == null)
            {
                Unit u = new Unit(id, _clientData.GameData.GetUnits(id), this);
                _units[id] = u;
            }
        }

        public void UnitShow(int id)
        {
            UnitCreate(id);
            _visibleUnits.Add(id);
        }

        public void UnitHide(int id)
        {
            _visibleUnits.Remove(id);
        }

        public void OnFrame(int frame)
        {
            if (frame > 0)
            {
                _allUnits = _visibleUnits.Select(x => _units[x]).ToList();
            }

            GetAllUnits().ForEach((u) => u.UpdatePosition(frame));
        }

        /// <summary>
        /// Retrieves the set of all teams/forces. Forces are commonly seen in @UMS
        /// game types and some others such as @TvB and the team versions of game types.
        /// </summary>
        /// <returns>List<Force> containing all forces in the game.</returns>
        public List<Force> GetForces()
        {
            return _forceSet;
        }

        /// <summary>
        /// Retrieves the set of all players in the match. This includes the neutral
        /// player, which owns all the resources and critters by default.
        /// </summary>
        /// <returns>List<Player> containing all players in the game.</returns>
        public List<Player> GetPlayers()
        {
            return _playerSet;
        }

        /// <summary>
        /// Retrieves the set of all accessible units.
        /// If {@link Flag#CompleteMapInformation} is enabled, then the set also includes units that are not
        /// visible to the player.
        /// <p>
        /// Units that are inside refineries are not included in this set.
        /// </summary>
        /// <returns>List<Unit> containing all known units in the game.</returns>
        public List<Unit> GetAllUnits()
        {
            return _allUnits;
        }

        /// <summary>
        /// Retrieves the set of all accessible @minerals in the game.
        /// </summary>
        /// <returns>List<Unit> containing @minerals</returns>
        public List<Unit> GetMinerals()
        {
            return GetAllUnits().Where((u) => u.GetUnitType().IsMineralField()).ToList();
        }

        /// <summary>
        /// Retrieves the set of all accessible @geysers in the game.
        /// </summary>
        /// <returns>List<Unit> containing @geysers</returns>
        public List<Unit> GetGeysers()
        {
            return GetAllUnits().Where((u) => u.GetUnitType() == UnitType.Resource_Vespene_Geyser).ToList();
        }

        /// <summary>
        /// Retrieves the set of all accessible neutral units in the game. This
        /// includes @minerals, @geysers, and @critters.
        /// </summary>
        /// <returns>List<Unit> containing all neutral units.</returns>
        public List<Unit> GetNeutralUnits()
        {
            return GetAllUnits().Where((u) => u.GetPlayer().Equals(Neutral())).ToList();
        }

        /// <summary>
        /// Retrieves the set of all @minerals that were available at the beginning of the
        /// game.
        /// <p>
        /// This set includes resources that have been mined out or are inaccessible.
        /// </summary>
        /// <returns>List<Unit> containing static @minerals</returns>
        public List<Unit> GetStaticMinerals()
        {
            return _staticMinerals;
        }

        /// <summary>
        /// Retrieves the set of all @geysers that were available at the beginning of the
        /// game.
        /// <p>
        /// This set includes resources that are inaccessible.
        /// </summary>
        /// <returns>List<Unit> containing static @geysers</returns>
        public List<Unit> GetStaticGeysers()
        {
            return _staticGeysers;
        }

        /// <summary>
        /// Retrieves the set of all units owned by the neutral player (resources, critters,
        /// etc.) that were available at the beginning of the game.
        /// <p>
        /// This set includes units that are inaccessible.
        /// </summary>
        /// <returns>List<Unit> containing static neutral units</returns>
        public List<Unit> GetStaticNeutralUnits()
        {
            return _staticNeutralUnits;
        }

        /// <summary>
        /// Retrieves the set of all accessible bullets.
        /// </summary>
        /// <returns>List<Bullet> containing all accessible {@link Bullet} objects.</returns>
        public List<Bullet> GetBullets()
        {
            return _bullets.Where(x => x.Exists()).ToList();
        }

        /// <summary>
        /// Retrieves the set of all accessible @Nuke dots.
        /// <p>
        /// Nuke dots are the red dots painted by a @Ghost when using the nuclear strike ability.
        /// </summary>
        /// <returns>Set of Positions giving the coordinates of nuke locations.</returns>
        public List<Position> GetNukeDots()
        {
            return Enumerable.Range(0, _clientData.GameData.GetNukeDotCount()).Select(id => new Position(_clientData.GameData.GetNukeDots(id))).ToList();
        }

        /// <summary>
        /// Retrieves the {@link Force} object associated with a given identifier.
        /// </summary>
        /// <param name="forceID">The identifier for the Force object.</param>
        /// <returns>{@link Force} object mapped to the given forceID. Returns null if the given identifier is invalid.</returns>
        public Force GetForce(int forceID)
        {
            if (forceID < 0 || forceID >= _forces.Length)
            {
                return null;
            }

            return _forces[forceID];
        }

        /// <summary>
        /// Retrieves the {@link Player} object associated with a given identifier.
        /// </summary>
        /// <param name="playerID">The identifier for the {@link Player} object.</param>
        /// <returns>{@link Player} object mapped to the given playerID. null if the given identifier is invalid.</returns>
        public Player GetPlayer(int playerID)
        {
            if (playerID < 0 || playerID >= _players.Length)
            {
                return null;
            }

            return _players[playerID];
        }

        /// <summary>
        /// Retrieves the {@link Unit} object associated with a given identifier.
        /// </summary>
        /// <param name="unitID">The identifier for the {@link Unit} object.</param>
        /// <returns>{@link Unit} object mapped to the given unitID. null if the given identifier is invalid.</returns>
        public Unit GetUnit(int unitID)
        {
            if (unitID < 0 || unitID >= _units.Length)
            {
                return null;
            }

            return _units[unitID];
        }

        /// <summary>
        /// Retrieves the {@link Region} object associated with a given identifier.
        /// </summary>
        /// <param name="regionID">The identifier for the {@link Region} object.</param>
        /// <returns>{@link Region} object mapped to the given regionID. Returns null if the given ID is invalid.</returns>
        public Region GetRegion(int regionID)
        {
            if (regionID < 0 || regionID >= _regions.Length)
            {
                return null;
            }

            return _regions[regionID];
        }

        /// <summary>
        /// Retrieves the {@link GameType} of the current game.
        /// </summary>
        /// <returns>{@link GameType} indicating the rules of the match.</returns>
        /// <remarks>@seeGameType</remarks>
        public GameType GetGameType()
        {
            return _clientData.GameData.GetGameType();
        }

        /// <summary>
        /// Retrieves the current latency setting that the game is set to. {@link Latency}
        /// indicates the delay between issuing a command and having it processed.
        /// </summary>
        /// <returns>The {@link Latency} setting of the game, which is of Latency.</returns>
        /// <remarks>@seeLatency</remarks>
        public Latency GetLatency()
        {
            return _clientData.GameData.GetLatency();
        }

        /// <summary>
        /// Retrieves the number of logical frames since the beginning of the match.
        /// If the game is paused, then getFrameCount will not increase.
        /// </summary>
        /// <returns>Number of logical frames that have elapsed since the game started as an integer.</returns>
        public int GetFrameCount()
        {
            return _clientData.GameData.GetFrameCount();
        }

        /// <summary>
        /// Retrieves the maximum number of logical frames that have been recorded in a
        /// replay. If the game is not a replay, then the value returned is undefined.
        /// </summary>
        /// <returns>The number of logical frames that the replay contains.</returns>
        public int GetReplayFrameCount()
        {
            return _clientData.GameData.GetReplayFrameCount();
        }

        /// <summary>
        /// Retrieves the logical frame rate of the game in frames per second (FPS).
        /// </summary>
        /// <returns>Logical frames per second that the game is currently running at as an integer.</returns>
        /// <remarks>@see#getAverageFPS</remarks>
        public int GetFPS()
        {
            return _clientData.GameData.GetFps();
        }

        /// <summary>
        /// Retrieves the average logical frame rate of the game in frames per second (FPS).
        /// </summary>
        /// <returns>Average logical frames per second that the game is currently running at as a
        /// double.</returns>
        /// <remarks>@see#getFPS</remarks>
        public double GetAverageFPS()
        {
            return _clientData.GameData.GetAverageFPS();
        }

        /// <summary>
        /// Retrieves the position of the user's mouse on the screen, in {@link Position} coordinates.
        /// </summary>
        /// <returns>{@link Position} indicating the location of the mouse. Returns {@link Position#Unknown} if {@link Flag#UserInput} is disabled.</returns>
        public Position GetMousePosition()
        {
            return new Position(_clientData.GameData.GetMouseX(), _clientData.GameData.GetMouseY());
        }

        /// <summary>
        /// Retrieves the state of the given mouse button.
        /// </summary>
        /// <param name="button">A {@link MouseButton} enum member indicating which button on the mouse to check.</param>
        /// <returns>A bool indicating the state of the given button. true if the button was pressed
        /// and false if it was not. Returns false always if {@link Flag#UserInput} is disabled.</returns>
        /// <remarks>@seeMouseButton</remarks>
        public bool GetMouseState(MouseButton button)
        {
            return _clientData.GameData.GetMouseState(button);
        }

        /// <summary>
        /// Retrieves the state of the given keyboard key.
        /// </summary>
        /// <param name="key">A {@link Key} enum member indicating which key on the keyboard to check.</param>
        /// <returns>A bool indicating the state of the given key. true if the key was pressed
        /// and false if it was not. Returns false always if {@link Flag#UserInput} is disabled.</returns>
        /// <remarks>@seeKey</remarks>
        public bool GetKeyState(Key key)
        {
            return _clientData.GameData.GetKeyState(key);
        }

        /// <summary>
        /// Retrieves the top left position of the viewport from the top left corner of the
        /// map, in pixels.
        /// </summary>
        /// <returns>{@link Position} containing the coordinates of the top left corner of the game's viewport. Returns {@link Position#Unknown} always if {@link Flag#UserInput} is disabled.</returns>
        /// <remarks>@see#setScreenPosition</remarks>
        public Position GetScreenPosition()
        {
            return new Position(_clientData.GameData.GetScreenX(), _clientData.GameData.GetScreenY());
        }

        public void SetScreenPosition(Position p)
        {
            SetScreenPosition(p.x, p.y);
        }

        /// <summary>
        /// Moves the top left corner of the viewport to the provided position relative to
        /// the map's origin (top left (0,0)).
        /// </summary>
        /// <param name="x">The x coordinate to move the screen to, in pixels.</param>
        /// <param name="y">The y coordinate to move the screen to, in pixels.</param>
        /// <remarks>@see#getScreenPosition</remarks>
        public void SetScreenPosition(int x, int y)
        {
            AddCommand(CommandType.SetScreenPosition, x, y);
        }

        /// <summary>
        /// Pings the minimap at the given position. Minimap pings are visible to
        /// allied players.
        /// </summary>
        /// <param name="x">The x coordinate to ping at, in pixels, from the map's origin (left).</param>
        /// <param name="y">The y coordinate to ping at, in pixels, from the map's origin (top).</param>
        public void PingMinimap(int x, int y)
        {
            AddCommand(CommandType.PingMinimap, x, y);
        }

        public void PingMinimap(Position p)
        {
            PingMinimap(p.x, p.y);
        }

        /// <summary>
        /// Checks if the state of the given flag is enabled or not.
        /// <p>
        /// Flags may only be enabled at the start of the match during the {@link BWEventListener#onStart}
        /// callback.
        /// </summary>
        /// <param name="flag">The {@link Flag} entry describing the flag's effects on BWAPI.</param>
        /// <returns>true if the given flag is enabled, false if the flag is disabled.</returns>
        /// <remarks>@seeFlag</remarks>
        public bool IsFlagEnabled(Flag flag)
        {
            return _clientData.GameData.GetFlags(flag);
        }

        /// <summary>
        /// Enables the state of a given flag.
        /// <p>
        /// Flags may only be enabled at the start of the match during the {@link BWEventListener#onStart}
        /// callback.
        /// </summary>
        /// <param name="flag">The {@link Flag} entry describing the flag's effects on BWAPI.</param>
        /// <remarks>@seeFlag</remarks>
        public void EnableFlag(Flag flag)
        {
            AddCommand(CommandType.EnableFlag, (int)flag, 1);
        }

        public List<Unit> GetUnitsOnTile(TilePosition tile)
        {
            return GetUnitsOnTile(tile.x, tile.y);
        }

        public List<Unit> GetUnitsOnTile(int tileX, int tileY)
        {
            return GetUnitsOnTile(tileX, tileY, (u) => true);
        }

        /// <summary>
        /// Retrieves the set of accessible units that are on a given build tile.
        /// </summary>
        /// <param name="tileX">The X position, in tiles.</param>
        /// <param name="tileY">The Y position, in tiles.</param>
        /// <param name="pred">A function predicate that indicates which units are included in the returned set.</param>
        /// <returns>A List<Unit> object consisting of all the units that have any part of them on the
        /// given build tile.</returns>
        public List<Unit> GetUnitsOnTile(int tileX, int tileY, UnitFilter pred)
        {
            return GetAllUnits().Where((u) =>
            {
                TilePosition tp = u.GetTilePosition();
                return tp.x == tileX && tp.y == tileY && pred(u);
            }).ToList();
        }

        public List<Unit> GetUnitsInRectangle(int left, int top, int right, int bottom)
        {
            return GetUnitsInRectangle(left, top, right, bottom, (u) => true);
        }

        /// <summary>
        /// Retrieves the set of accessible units that are in a given rectangle.
        /// </summary>
        /// <param name="left">The X coordinate of the left position of the bounding box, in pixels.</param>
        /// <param name="top">The Y coordinate of the top position of the bounding box, in pixels.</param>
        /// <param name="right">The X coordinate of the right position of the bounding box, in pixels.</param>
        /// <param name="bottom">The Y coordinate of the bottom position of the bounding box, in pixels.</param>
        /// <param name="pred">A function predicate that indicates which units are included in the returned set.</param>
        /// <returns>A List<Unit> object consisting of all the units that have any part of them within the
        /// given rectangle bounds.</returns>
        public List<Unit> GetUnitsInRectangle(int left, int top, int right, int bottom, UnitFilter pred)
        {
            return GetAllUnits().Where((u) => left <= u.GetRight() && top <= u.GetBottom() && right >= u.GetLeft() && bottom >= u.GetTop() && pred(u)).ToList();
        }

        public List<Unit> GetUnitsInRectangle(Position leftTop, Position rightBottom)
        {
            return GetUnitsInRectangle(leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, (u) => true);
        }

        public List<Unit> GetUnitsInRectangle(Position leftTop, Position rightBottom, UnitFilter pred)
        {
            return GetUnitsInRectangle(leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, pred);
        }

        public List<Unit> GetUnitsInRadius(int x, int y, int radius)
        {
            return GetUnitsInRadius(x, y, radius, (u) => true);
        }

        /// <summary>
        /// Retrieves the set of accessible units that are within a given radius of a
        /// position.
        /// </summary>
        /// <param name="x">The x coordinate of the center, in pixels.</param>
        /// <param name="y">The y coordinate of the center, in pixels.</param>
        /// <param name="radius">The radius from the center, in pixels, to include units.</param>
        /// <param name="pred">A function predicate that indicates which units are included in the returned set.</param>
        /// <returns>A List<Unit> object consisting of all the units that have any part of them within the
        /// given radius from the center position.</returns>
        public List<Unit> GetUnitsInRadius(int x, int y, int radius, UnitFilter pred)
        {
            return GetUnitsInRadius(new Position(x, y), radius, pred);
        }

        public List<Unit> GetUnitsInRadius(Position center, int radius)
        {
            return GetUnitsInRadius(center, radius, (u) => true);
        }

        public List<Unit> GetUnitsInRadius(Position center, int radius, UnitFilter pred)
        {
            return GetAllUnits().Where((u) => center.GetApproxDistance(u.GetPosition()) <= radius && pred(u)).ToList();
        }

        public Unit GetClosestUnitInRectangle(Position center, int left, int top, int right, int bottom)
        {
            return GetClosestUnitInRectangle(center, left, top, right, bottom, (u) => true);
        }

        /// <summary>
        /// Retrieves the closest unit to center that matches the criteria of the callback
        /// pred within an optional rectangle.
        /// </summary>
        /// <param name="center">The position to start searching for the closest unit.</param>
        /// <param name="pred">The {@link UnitFilter} predicate to determine which units should be included. This includes all units by default.</param>
        /// <param name="left">The left position of the rectangle. This value is 0 by default.</param>
        /// <param name="top">The top position of the rectangle. This value is 0 by default.</param>
        /// <param name="right">The right position of the rectangle. This value includes the entire map width by default.</param>
        /// <param name="bottom">The bottom position of the rectangle. This value includes the entire map height by default.</param>
        /// <remarks>@seeUnitFilter</remarks>
        public Unit GetClosestUnitInRectangle(Position center, int left, int top, int right, int bottom, UnitFilter pred)
        {
            return GetUnitsInRectangle(left, top, right, bottom, pred).MinBy((u) => u.GetDistance(center));
        }

        public Unit GetClosestUnit(Position center)
        {
            return GetClosestUnit(center, 999999);
        }

        public Unit GetClosestUnit(Position center, UnitFilter pred)
        {
            return GetClosestUnit(center, 999999, pred);
        }

        public Unit GetClosestUnit(Position center, int radius)
        {
            return GetClosestUnit(center, radius, (u) => true);
        }

        /// <summary>
        /// Retrieves the closest unit to center that matches the criteria of the callback
        /// pred within an optional radius.
        /// </summary>
        /// <param name="center">The position to start searching for the closest unit.</param>
        /// <param name="pred">The UnitFilter predicate to determine which units should be included. This includes all units by default.</param>
        /// <param name="radius">The radius to search in. If omitted, the entire map will be searched.</param>
        /// <returns>The desired unit that is closest to center. Returns null If a suitable unit was not found.</returns>
        /// <remarks>@seeUnitFilter</remarks>
        public Unit GetClosestUnit(Position center, int radius, UnitFilter pred)
        {
            return GetUnitsInRadius(center, radius, pred).MinBy((u) => u.GetDistance(center));
        }

        /// <summary>
        /// Retrieves the width of the map in build tile units.
        /// </summary>
        /// <returns>Width of the map in tiles.</returns>
        public int MapWidth()
        {
            return _mapWidth;
        }

        /// <summary>
        /// Retrieves the height of the map in build tile units.
        /// </summary>
        /// <returns>Height of the map in tiles.</returns>
        public int MapHeight()
        {
            return _mapHeight;
        }

        public int MapPixelWidth()
        {
            return _mapPixelWidth;
        }

        public int MapPixelHeight()
        {
            return _mapPixelHeight;
        }

        /// <summary>
        /// Retrieves the file name of the currently loaded map.
        /// </summary>
        /// <returns>Map file name as String object.</returns>
        /// <remarks>
        /// @see#mapPathName
        /// @see#mapName
        /// </remarks>
        public string MapFileName()
        {
            return _mapFileName;
        }

        /// <summary>
        /// Retrieves the full path name of the currently loaded map.
        /// </summary>
        /// <returns>Map file name as String object.</returns>
        /// <remarks>
        /// @see#mapFileName
        /// @see#mapName
        /// </remarks>
        public string MapPathName()
        {
            return _mapPathName;
        }

        /// <summary>
        /// Retrieves the title of the currently loaded map.
        /// </summary>
        /// <returns>Map title as String object.</returns>
        /// <remarks>
        /// @see#mapFileName
        /// @see#mapPathName
        /// </remarks>
        public string MapName()
        {
            return _mapName;
        }

        /// <summary>
        /// Calculates the SHA-1 hash of the currently loaded map file.
        /// </summary>
        /// <returns>String object containing SHA-1 hash.
        /// <p>
        /// Campaign maps will return a hash of their internal map chunk components(.chk), while
        /// standard maps will return a hash of their entire map archive (.scm,.scx).</returns>
        public string MapHash()
        {
            return _mapHash;
        }

        /// <summary>
        /// Checks if the given mini-tile position is walkable.
        /// <p>
        /// This function only checks if the static terrain is walkable. Its current occupied
        /// state is excluded from this check. To see if the space is currently occupied or not, then
        /// see {@link #getUnitsInRectangle}.
        /// </summary>
        /// <param name="walkX">The x coordinate of the mini-tile, in mini-tile units (8 pixels).</param>
        /// <param name="walkY">The y coordinate of the mini-tile, in mini-tile units (8 pixels).</param>
        /// <returns>true if the mini-tile is walkable and false if it is impassable for ground units.</returns>
        public bool IsWalkable(int walkX, int walkY)
        {
            return IsWalkable(new WalkPosition(walkX, walkY));
        }

        public bool IsWalkable(WalkPosition position)
        {
            return position.IsValid(this) && _walkable[position.x, position.y];
        }

        /// <summary>
        /// Returns the ground height at the given tile position.
        /// </summary>
        /// <param name="tileX">X position to query, in tiles</param>
        /// <param name="tileY">Y position to query, in tiles</param>
        /// <returns>The tile height as an integer. Possible values are:
        /// - 0: Low ground
        /// - 1: Low ground doodad
        /// - 2: High ground
        /// - 3: High ground doodad
        /// - 4: Very high ground
        /// - 5: Very high ground doodad
        /// .</returns>
        public int GetGroundHeight(int tileX, int tileY)
        {
            return IsValidTile(tileX, tileY) ? _groundHeight[tileX, tileY] : 0;
        }

        public int GetGroundHeight(TilePosition position)
        {
            return GetGroundHeight(position.x, position.y);
        }

        public bool IsBuildable(int tileX, int tileY)
        {
            return IsBuildable(tileX, tileY, false);
        }

        /// <summary>
        /// Checks if a given tile position is buildable. This means that, if all
        /// other requirements are met, a structure can be placed on this tile. This function uses
        /// static map data.
        /// </summary>
        /// <param name="tileX">The x value of the tile to check.</param>
        /// <param name="tileY">The y value of the tile to check.</param>
        /// <param name="includeBuildings">If this is true, then this function will also check if any visible structures are occupying the space. If this value is false, then it only checks the static map data for tile buildability. This value is false by default.</param>
        /// <returns>bool identifying if the given tile position is buildable (true) or not (false).
        /// If includeBuildings was provided, then it will return false if a structure is currently
        /// occupying the tile.</returns>
        public bool IsBuildable(int tileX, int tileY, bool includeBuildings)
        {
            return IsValidTile(tileX, tileY) && _buildable[tileX, tileY] && (!includeBuildings || !_clientData.GameData.IsOccupied(tileX, tileY));
        }

        public bool IsBuildable(TilePosition position)
        {
            return IsBuildable(position, false);
        }

        bool IsValidPosition(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _mapPixelWidth && y < _mapPixelHeight;
        }

        bool IsValidTile(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _mapWidth && y < _mapHeight;
        }

        public bool IsBuildable(TilePosition position, bool includeBuildings)
        {
            return IsBuildable(position.x, position.y, includeBuildings);
        }

        /// <summary>
        /// Checks if a given tile position is visible to the current player.
        /// </summary>
        /// <param name="tileX">The x value of the tile to check.</param>
        /// <param name="tileY">The y value of the tile to check.</param>
        /// <returns>bool identifying the visibility of the tile. If the given tile is visible, then
        /// the value is true. If the given tile is concealed by the fog of war, then this value will
        /// be false.</returns>
        public bool IsVisible(int tileX, int tileY)
        {
            return IsValidTile(tileX, tileY) && _clientData.GameData.IsVisible(tileX, tileY);
        }

        public bool IsVisible(TilePosition position)
        {
            return IsVisible(position.x, position.y);
        }

        /// <summary>
        /// Checks if a given tile position has been explored by the player. An
        /// explored tile position indicates that the player has seen the location at some point in the
        /// match, partially revealing the fog of war for the remainder of the match.
        /// </summary>
        /// <param name="tileX">The x tile coordinate to check.</param>
        /// <param name="tileY">The y tile coordinate to check.</param>
        /// <returns>true if the player has explored the given tile position (partially revealed fog), false if the tile position was never explored (completely black fog).</returns>
        /// <remarks>@see#isVisible</remarks>
        public bool IsExplored(int tileX, int tileY)
        {
            return IsValidTile(tileX, tileY) && _clientData.GameData.IsExplored(tileX, tileY);
        }

        public bool IsExplored(TilePosition position)
        {
            return IsExplored(position.x, position.y);
        }

        /// <summary>
        /// Checks if the given tile position has @Zerg creep on it.
        /// </summary>
        /// <param name="tileX">The x tile coordinate to check.</param>
        /// <param name="tileY">The y tile coordinate to check.</param>
        /// <returns>true if the given tile has creep on it, false if the given tile does not have creep, or if it is concealed by the fog of war.</returns>
        public bool HasCreep(int tileX, int tileY)
        {
            return IsValidTile(tileX, tileY) && _clientData.GameData.GetHasCreep(tileX, tileY);
        }

        public bool HasCreep(TilePosition position)
        {
            return HasCreep(position.x, position.y);
        }

        public bool HasPowerPrecise(int x, int y)
        {
            return HasPowerPrecise(new Position(x, y));
        }

        /// <summary>
        /// Checks if the given pixel position is powered by an owned @Protoss_Pylon for an
        /// optional unit type.
        /// </summary>
        /// <param name="x">The x pixel coordinate to check.</param>
        /// <param name="y">The y pixel coordinate to check.</param>
        /// <param name="unitType">Checks if the given {@link UnitType} requires power or not. If ommitted, then it will assume that the position requires power for any unit type.</param>
        /// <returns>true if the type at the given position will have power, false if the type at the given position will be unpowered.</returns>
        public bool HasPowerPrecise(int x, int y, UnitType unitType)
        {
            return IsValidPosition(x, y) && HasPower(x, y, unitType, Self().GetUnits().Where((u) => u.GetUnitType() == UnitType.Protoss_Pylon).ToList());
        }

        public bool HasPowerPrecise(Position position)
        {
            return HasPowerPrecise(position.x, position.y, UnitType.None);
        }

        public bool HasPowerPrecise(Position position, UnitType unitType)
        {
            return HasPowerPrecise(position.x, position.y, unitType);
        }

        public bool HasPower(TilePosition position)
        {
            return HasPower(position.x, position.y);
        }

        public bool HasPower(int tileX, int tileY)
        {
            return HasPower(tileX, tileY, UnitType.None);
        }

        public bool HasPower(TilePosition position, UnitType unitType)
        {
            return HasPower(position.x, position.y, unitType);
        }

        public bool HasPower(int tileX, int tileY, UnitType unitType)
        {
            if (unitType >= 0 && unitType < UnitType.None)
            {
                return HasPowerPrecise(tileX * 32 + unitType.TileWidth() * 16, tileY * 32 + unitType.TileHeight() * 16, unitType);
            }

            return HasPowerPrecise(tileY * 32, tileY * 32, UnitType.None);
        }

        public bool HasPower(int tileX, int tileY, int tileWidth, int tileHeight)
        {
            return HasPower(tileX, tileY, tileWidth, tileHeight, UnitType.Unknown);
        }

        /// <summary>
        /// Checks if the given tile position if powered by an owned @Protoss_Pylon for an
        /// optional unit type.
        /// </summary>
        /// <param name="tileX">The x tile coordinate to check.</param>
        /// <param name="tileY">The y tile coordinate to check.</param>
        /// <param name="unitType">Checks if the given UnitType will be powered if placed at the given tile position. If omitted, then only the immediate tile position is checked for power, and the function will assume that the location requires power for any unit type.</param>
        /// <returns>true if the type at the given tile position will receive power, false if the type will be unpowered at the given tile position.</returns>
        public bool HasPower(int tileX, int tileY, int tileWidth, int tileHeight, UnitType unitType)
        {
            return HasPowerPrecise(tileX * 32 + tileWidth * 16, tileY * 32 + tileHeight * 16, unitType);
        }

        public bool HasPower(TilePosition position, int tileWidth, int tileHeight)
        {
            return HasPower(position.x, position.y, tileWidth, tileHeight);
        }

        public bool HasPower(TilePosition position, int tileWidth, int tileHeight, UnitType unitType)
        {
            return HasPower(position.x, position.y, tileWidth, tileHeight, unitType);
        }

        public bool CanBuildHere(TilePosition position, UnitType type, Unit builder)
        {
            return CanBuildHere(position, type, builder, false);
        }

        public bool CanBuildHere(TilePosition position, UnitType type)
        {
            return CanBuildHere(position, type, null);
        }

        /// <summary>
        /// Checks if the given unit type can be built at the given build tile position.
        /// This function checks for creep, power, and resource distance requirements in addition to
        /// the tiles' buildability and possible units obstructing the build location.
        /// <p>
        /// If the type is an addon and a builer is provided, then the location of the addon will
        /// be placed 4 tiles to the right and 1 tile down from the given position. If the builder
        /// is not given, then the check for the addon will be conducted at position.
        /// <p>
        /// If type is UnitType.Special_Start_Location, then the area for a resource depot
        /// (@Command_Center, @Hatchery, @Nexus) is checked as normal, but any potential obstructions
        /// (existing structures, creep, units, etc.) are ignored.
        /// </summary>
        /// <param name="position">Indicates the tile position that the top left corner of the structure is intended to go.</param>
        /// <param name="type">The UnitType to check for.</param>
        /// <param name="builder">The intended unit that will build the structure. If specified, then this function will also check if there is a path to the build site and exclude the builder from the set of units that may be blocking the build site.</param>
        /// <param name="checkExplored">If this parameter is true, it will also check if the target position has been explored by the current player. This value is false by default, ignoring the explored state of the build site.</param>
        /// <returns>true indicating that the structure can be placed at the given tile position, and
        /// false if something may be obstructing the build location.</returns>
        public bool CanBuildHere(TilePosition position, UnitType type, Unit builder, bool checkExplored)
        {
            // lt = left top, rb = right bottom
            TilePosition lt = builder != null && type.IsAddon() ? position.Add(new TilePosition(4, 1)) : position;
            TilePosition rb = lt.Add(type.TileSize());

            // Map limit check
            if (!lt.IsValid(this) || !(rb.ToPosition().Subtract(new Position(1, 1)).IsValid(this)))
            {
                return false;
            }

            //if the getUnit is a refinery, we just need to check the set of geysers to see if the position
            //matches one of them (and the type is still vespene geyser)
            if (type.IsRefinery())
            {
                foreach (Unit g in GetGeysers())
                {
                    if (g.GetTilePosition().Equals(lt))
                    {
                        return !g.IsVisible() || g.GetUnitType() == UnitType.Resource_Vespene_Geyser;
                    }
                }

                return false;
            }

            // Tile buildability check
            for (int x = lt.x; x < rb.x; ++x)
            {
                for (int y = lt.y; y < rb.y; ++y)
                {
                    // Check if tile is buildable/unoccupied and explored.
                    if (!IsBuildable(x, y) || (checkExplored && !IsExplored(x, y)))
                    {
                        return false;
                    }
                }
            }

            // Check if builder is capable of reaching the building site
            if (builder != null)
            {
                if (!builder.GetUnitType().IsBuilding())
                {
                    if (!builder.HasPath(lt.ToPosition().Add(type.TileSize().ToPosition().Divide(2))))
                    {
                        return false;
                    }
                }
                else if (!builder.GetUnitType().IsFlyingBuilding() && type != UnitType.Zerg_Nydus_Canal && !type.IsFlagBeacon())
                {
                    return false;
                }
            }


            // Ground getUnit dimension check
            if (type != UnitType.Special_Start_Location)
            {
                Position targPos = lt.ToPosition().Add(type.TileSize().ToPosition().Divide(2));
                List<Unit> unitsInRect = GetUnitsInRectangle(targPos.Subtract(new Position(type.DimensionLeft(), type.DimensionUp())), targPos.Add(new Position(type.DimensionRight(), type.DimensionDown())), (u) => !u.IsFlying() && !u.IsLoaded() && (builder != u || type == UnitType.Zerg_Nydus_Canal));
                foreach (Unit u in unitsInRect)
                {

                    // Addons can be placed over units that can move, pushing them out of the way
                    if (!(type.IsAddon() && u.GetUnitType().CanMove()))
                    {
                        return false;
                    }
                }


                // Creep Check
                // Note: Zerg structures that don't require creep can still be placed on creep
                bool needsCreep = type.RequiresCreep();
                if (type.GetRace() != Race.Zerg || needsCreep)
                {
                    for (int x = lt.x; x < rb.x; ++x)
                    {
                        for (int y = lt.y; y < rb.y; ++y)
                        {
                            if (needsCreep != HasCreep(x, y))
                            {
                                return false;
                            }
                        }
                    }
                }


                // Power Check
                if (type.RequiresPsi() && !HasPower(lt, type))
                {
                    return false;
                }
            } //don't ignore units


            // Resource Check (CC, Nex, Hatch)
            if (type.IsResourceDepot())
            {
                foreach (Unit m in GetStaticMinerals())
                {
                    TilePosition tp = m.GetInitialTilePosition();
                    if ((IsVisible(tp) || IsVisible(tp.x + 1, tp.y)) && !m.Exists())
                    {
                        continue; // tile position is visible, but mineral is not => mineral does not exist
                    }

                    if (tp.x > lt.x - 5 && tp.y > lt.y - 4 && tp.x < lt.x + 7 && tp.y < lt.y + 6)
                    {
                        return false;
                    }
                }

                foreach (Unit g in GetStaticGeysers())
                {
                    TilePosition tp = g.GetInitialTilePosition();
                    if (tp.x > lt.x - 7 && tp.y > lt.y - 5 && tp.x < lt.x + 7 && tp.y < lt.y + 6)
                    {
                        return false;
                    }
                }
            }


            // A building can build an addon at a different location (i.e. automatically lifts (if not already lifted)
            // then lands at the new location before building the addon), so we need to do similar checks for the
            // location that the building will be when it builds the addon.
            if (builder != null && !builder.GetUnitType().IsAddon() && type.IsAddon())
            {
                return CanBuildHere(lt.Subtract(new TilePosition(4, 1)), builder.GetUnitType(), builder, checkExplored);
            }


            //if the build site passes all these tests, return true.
            return true;
        }

        public bool CanMake(UnitType type)
        {
            return CanMake(type, null);
        }

        /// <summary>
        /// Checks all the requirements in order to make a given unit type for the current
        /// player. These include resources, supply, technology tree, availability, and
        /// required units.
        /// </summary>
        /// <param name="type">The {@link UnitType} to check.</param>
        /// <param name="builder">The Unit that will be used to build/train the provided unit type. If this value is null or excluded, then the builder will be excluded in the check.</param>
        /// <returns>true indicating that the type can be made. If builder is provided, then it is
        /// only true if builder can make the type. Otherwise it will return false, indicating
        /// that the unit type can not be made.</returns>
        public bool CanMake(UnitType type, Unit builder)
        {
            Player pSelf = Self();

            // Error checking
            if (pSelf == null)
            {
                return false;
            }


            // Check if the unit type is available (UMS game)
            if (!pSelf.IsUnitAvailable(type))
            {
                return false;
            }


            // Get the required UnitType
            UnitType requiredType = type.WhatBuilds().GetKey();

            // do checks if a builder is provided
            if (builder != null)
            {

                // Check if the owner of the unit is you
                if (!pSelf.Equals(builder.GetPlayer()))
                {
                    return false;
                }

                UnitType builderType = builder.GetUnitType();
                if (type == UnitType.Zerg_Nydus_Canal && builderType == UnitType.Zerg_Nydus_Canal)
                {
                    if (!builder.IsCompleted())
                    {
                        return false;
                    }

                    return builder.GetNydusExit() == null;
                }


                // Check if this unit can actually build the unit type
                if (requiredType == UnitType.Zerg_Larva && builderType.ProducesLarva())
                {
                    if (builder.GetLarva().Count == 0)
                    {
                        return false;
                    }
                }
                else if (!builderType.Equals(requiredType))
                {
                    return false;
                }

                // Carrier/Reaver space checking
                int max_amt;
                switch (builderType)
                {
                    case UnitType.Protoss_Carrier:
                    case UnitType.Hero_Gantrithor:
                        {
                            // Get max interceptors
                            max_amt = 4;
                            if (pSelf.GetUpgradeLevel(UpgradeType.Carrier_Capacity) > 0 || builderType == UnitType.Hero_Gantrithor)
                            {
                                max_amt += 4;
                            }


                            // Check if there is room
                            if (builder.GetInterceptorCount() + builder.GetTrainingQueue().Count >= max_amt)
                            {
                                return false;
                            }

                            break;
                        }
                    case UnitType.Protoss_Reaver:
                    case UnitType.Hero_Warbringer:
                        {
                            // Get max scarabs
                            max_amt = 5;
                            if (pSelf.GetUpgradeLevel(UpgradeType.Reaver_Capacity) > 0 || builderType == UnitType.Hero_Warbringer)
                            {
                                max_amt += 5;
                            }


                            // check if there is room
                            if (builder.GetScarabCount() + builder.GetTrainingQueue().Count >= max_amt)
                            {
                                return false;
                            }

                            break;
                        }
                }
            } // if builder != nullptr

            // Check if player has enough minerals
            if (pSelf.Minerals() < type.MineralPrice())
            {
                return false;
            }

            // Check if player has enough gas
            if (pSelf.Gas() < type.GasPrice())
            {
                return false;
            }

            // Check if player has enough supplies
            Race typeRace = type.GetRace();
            int supplyRequired = type.SupplyRequired() * (type.IsTwoUnitsInOneEgg() ? 2 : 1);
            if (supplyRequired > 0 && pSelf.SupplyTotal(typeRace) < pSelf.SupplyUsed(typeRace) + supplyRequired - (requiredType.GetRace() == typeRace ? requiredType.SupplyRequired() : 0))
            {
                return false;
            }

            UnitType addon = UnitType.None;
            ReadOnlyDictionary<UnitType, int> reqUnits = type.RequiredUnits();
            foreach (UnitType ut in reqUnits.Keys)
            {
                if (ut.IsAddon())
                {
                    addon = ut;
                }

                if (!pSelf.HasUnitTypeRequirement(ut, reqUnits[ut]))
                {
                    return false;
                }
            }

            if (type.RequiredTech() != TechType.None && !pSelf.HasResearched(type.RequiredTech()))
            {
                return false;
            }

            return builder == null || addon == UnitType.None || addon.WhatBuilds().GetKey() != type.WhatBuilds().GetKey() || (builder.GetAddon() != null && builder.GetAddon().GetUnitType() == addon);
        }

        public bool CanResearch(TechType type, Unit unit)
        {
            return CanResearch(type, unit, true);
        }

        public bool CanResearch(TechType type)
        {
            return CanResearch(type, null);
        }

        /// <summary>
        /// Checks all the requirements in order to research a given technology type for the
        /// current player. These include resources, technology tree, availability, and
        /// required units.
        /// </summary>
        /// <param name="type">The {@link TechType} to check.</param>
        /// <param name="unit">The {@link Unit} that will be used to research the provided technology type. If this value is null or excluded, then the unit will be excluded in the check.</param>
        /// <param name="checkCanIssueCommandType">TODO fill this in</param>
        /// <returns>true indicating that the type can be researched. If unit is provided, then it is
        /// only true if unit can research the type. Otherwise it will return false, indicating
        /// that the technology can not be researched.</returns>
        public bool CanResearch(TechType type, Unit unit, bool checkCanIssueCommandType)
        {
            Player self = Self();

            // Error checking
            if (self == null)
            {
                return false;
            }

            if (unit != null)
            {
                if (!unit.GetPlayer().Equals(self))
                {
                    return false;
                }

                if (!unit.GetUnitType().IsSuccessorOf(type.WhatResearches()))
                {
                    return false;
                }

                if (checkCanIssueCommandType && (unit.IsLifted() || !unit.IsIdle() || !unit.IsCompleted()))
                {
                    return false;
                }
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

        public bool CanUpgrade(UpgradeType type, Unit unit)
        {
            return CanUpgrade(type, unit, true);
        }

        public bool CanUpgrade(UpgradeType type)
        {
            return CanUpgrade(type, null);
        }

        /// <summary>
        /// Checks all the requirements in order to upgrade a given upgrade type for the
        /// current player. These include resources, technology tree, availability, and
        /// required units.
        /// </summary>
        /// <param name="type">The {@link UpgradeType} to check.</param>
        /// <param name="unit">The {@link Unit} that will be used to upgrade the provided upgrade type. If this value is null or excluded, then the unit will be excluded in the check.</param>
        /// <param name="checkCanIssueCommandType">TODO fill this in</param>
        /// <returns>true indicating that the type can be upgraded. If unit is provided, then it is
        /// only true if unit can upgrade the type. Otherwise it will return false, indicating
        /// that the upgrade can not be upgraded.</returns>
        public bool CanUpgrade(UpgradeType type, Unit unit, bool checkCanIssueCommandType)
        {
            Player self = Self();
            if (self == null)
            {
                return false;
            }

            if (unit != null)
            {
                if (!unit.GetPlayer().Equals(self))
                {
                    return false;
                }

                if (!unit.GetUnitType().IsSuccessorOf(type.WhatUpgrades()))
                {
                    return false;
                }

                if (checkCanIssueCommandType && (unit.IsLifted() || !unit.IsIdle() || !unit.IsCompleted()))
                {
                    return false;
                }
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

        /// <summary>
        /// Retrieves the set of all starting locations for the current map. A
        /// starting location is essentially a candidate for a player's spawn point.
        /// </summary>
        /// <returns>A List<TilePosition> containing all the {@link TilePosition} objects that indicate a start
        /// location.</returns>
        /// <remarks>@seePlayer#getStartLocation</remarks>
        public List<TilePosition> GetStartLocations()
        {
            return _startLocations;
        }

        public static string FormatString(string str, params Text[] colors)
        {
            return colors.Length > 0 ? string.Format(str, colors) : str;
        }

        /// <summary>
        /// Prints text to the screen as a notification. This function allows text
        /// formatting using {@link Text#formatText}.
        /// <p>
        /// That text printed through this function is not seen by other players or in replays.
        /// </summary>
        /// <param name="string">String to print.</param>
        public void Printf(string str, params Text[] colors)
        {
            string formatted = FormatString(str, colors);
            AddCommand(CommandType.Printf, formatted, 0);
        }

        /// <summary>
        /// Sends a text message to all other players in the game.
        /// <p>
        /// In a single player game this function can be used to execute cheat codes.
        /// </summary>
        /// <param name="string">String to send.</param>
        /// <remarks>@see#sendTextEx</remarks>
        public void SendText(string @string, params Text[] colors)
        {
            SendTextEx(false, @string, colors);
        }

        /// <summary>
        /// An extended version of {@link #sendText} which allows messages to be forwarded to
        /// allies.
        /// </summary>
        /// <param name="toAllies">If this parameter is set to true, then the message is only sent to allied players, otherwise it will be sent to all players.</param>
        /// <param name="string">String to send.</param>
        /// <remarks>@see#sendText</remarks>
        public void SendTextEx(bool toAllies, string @string, params Text[] colors)
        {
            string formatted = FormatString(@string, colors);
            AddCommand(CommandType.SendText, formatted, toAllies ? 1 : 0);
        }

        /// <summary>
        /// Checks if the current client is inside a game.
        /// </summary>
        /// <returns>true if the client is in a game, and false if it is not.</returns>
        public bool IsInGame()
        {
            return _clientData.GameData.IsInGame();
        }

        /// <summary>
        /// Checks if the current client is inside a multiplayer game.
        /// </summary>
        /// <returns>true if the client is in a multiplayer game, and false if it is a single player
        /// game, a replay, or some other state.</returns>
        public bool IsMultiplayer()
        {
            return _multiplayer;
        }

        /// <summary>
        /// Checks if the client is in a game that was created through the Battle.net
        /// multiplayer gaming service.
        /// </summary>
        /// <returns>true if the client is in a multiplayer Battle.net game and false if it is not.</returns>
        public bool IsBattleNet()
        {
            return _battleNet;
        }

        /// <summary>
        /// Checks if the current game is paused. While paused, {@link BWEventListener#onFrame}
        /// will still be called.
        /// </summary>
        /// <returns>true if the game is paused and false otherwise</returns>
        /// <remarks>
        /// @see#pauseGame
        /// @see#resumeGame
        /// </remarks>
        public bool IsPaused()
        {
            return _clientData.GameData.IsPaused();
        }

        /// <summary>
        /// Checks if the client is watching a replay.
        /// </summary>
        /// <returns>true if the client is watching a replay and false otherwise</returns>
        public bool IsReplay()
        {
            return _replay;
        }

        /// <summary>
        /// Pauses the game. While paused, {@link BWEventListener#onFrame} will still be called.
        /// </summary>
        /// <remarks>@see#resumeGame</remarks>
        public void PauseGame()
        {
            AddCommand(CommandType.PauseGame, 0, 0);
        }

        /// <summary>
        /// Resumes the game from a paused state.
        /// </summary>
        /// <remarks>@see#pauseGame</remarks>
        public void ResumeGame()
        {
            AddCommand(CommandType.ResumeGame, 0, 0);
        }

        /// <summary>
        /// Leaves the current game by surrendering and enters the post-game statistics/score
        /// screen.
        /// </summary>
        public void LeaveGame()
        {
            AddCommand(CommandType.LeaveGame, 0, 0);
        }

        /// <summary>
        /// Restarts the match. Works the same as if the match was restarted from
        /// the in-game menu (F10). This option is only available in single player games.
        /// </summary>
        public void RestartGame()
        {
            AddCommand(CommandType.RestartGame, 0, 0);
        }

        /// <summary>
        /// Sets the number of milliseconds Broodwar spends in each frame. The
        /// default values are as follows:
        /// - Fastest: 42ms/frame
        /// - Faster: 48ms/frame
        /// - Fast: 56ms/frame
        /// - Normal: 67ms/frame
        /// - Slow: 83ms/frame
        /// - Slower: 111ms/frame
        /// - Slowest: 167ms/frame
        /// <p>
        /// Specifying a value of 0 will not guarantee that logical frames are executed as fast
        /// as possible. If that is the intention, use this in combination with #setFrameSkip.
        /// <p>
        /// Changing this value will cause the execution of @UMS scenario triggers to glitch.
        /// This will only happen in campaign maps and custom scenarios (non-melee).
        /// </summary>
        /// <param name="speed">The time spent per frame, in milliseconds. A value of 0 indicates that frames are executed immediately with no delay. Negative values will restore the default value as listed above.</param>
        /// <remarks>
        /// @see#setFrameSkip
        /// @see#getFPS
        /// </remarks>
        public void SetLocalSpeed(int speed)
        {
            AddCommand(CommandType.SetLocalSpeed, speed, 0);
        }

        /// <summary>
        /// Issues a given command to a set of units. This function automatically
        /// splits the set into groups of 12 and issues the same command to each of them. If a unit
        /// is not capable of executing the command, then it is simply ignored.
        /// </summary>
        /// <param name="units">A List<Unit> containing all the units to issue the command for.</param>
        /// <param name="command">A {@link UnitCommand} object containing relevant information about the command to be issued. The {@link Unit} object associated with the command will be ignored.</param>
        /// <returns>true if any one of the units in the List<Unit> were capable of executing the
        /// command, and false if none of the units were capable of executing the command.</returns>
        public bool IssueCommand(List<Unit> units, UnitCommand command)
        {
            return units.Select((u) => u.IssueCommand(command)).Aggregate(false, (a, b) => a | b);
        }

        /// <summary>
        /// Retrieves the set of units that are currently selected by the user outside of
        /// BWAPI. This function requires that{@link Flag#UserInput} be enabled.
        /// </summary>
        /// <returns>A List<Unit> containing the user's selected units. If {@link Flag#UserInput} is disabled,
        /// then this set is always empty.</returns>
        /// <remarks>@see#enableFlag</remarks>
        public List<Unit> GetSelectedUnits()
        {
            if (!IsFlagEnabled(Flag.UserInput))
            {
                return new List<Unit>();
            }

            return Enumerable.Range(0, _clientData.GameData.GetSelectedUnitCount()).Select((i) => _units[_clientData.GameData.GetSelectedUnits(i)]).ToList();
        }

        /// <summary>
        /// Retrieves the player object that BWAPI is controlling.
        /// </summary>
        /// <returns>Player object representing the current player. null if the current game is a replay.</returns>
        public Player Self()
        {
            return _self;
        }

        /// <summary>
        /// Retrieves the {@link Player} interface that represents the enemy player. If
        /// there is more than one enemy, and that enemy is destroyed, then this function will still
        /// retrieve the same, defeated enemy. If you wish to handle multiple opponents, see the
        /// {@link Game#enemies} function.
        /// </summary>
        /// <returns>Player interface representing an enemy player. Returns null if there is no enemy or the current game is a replay.</returns>
        /// <remarks>@see#enemies</remarks>
        public Player Enemy()
        {
            return _enemy;
        }

        /// <summary>
        /// Retrieves the {@link Player} object representing the neutral player.
        /// The neutral player owns all the resources and critters on the map by default.
        /// </summary>
        /// <returns>{@link Player} indicating the neutral player.</returns>
        public Player Neutral()
        {
            return _neutral;
        }

        /// <summary>
        /// Retrieves a set of all the current player's remaining allies.
        /// </summary>
        /// <returns>List<Player> containing all allied players.</returns>
        public List<Player> Allies()
        {
            return allies;
        }

        /// <summary>
        /// Retrieves a set of all the current player's remaining enemies.
        /// </summary>
        /// <returns>List<Player> containing all enemy players.</returns>
        public List<Player> Enemies()
        {
            return enemies;
        }

        /// <summary>
        /// Retrieves a set of all players currently observing the game. An observer
        /// is defined typically in a @UMS game type as not having any impact on the game. This means
        /// an observer cannot start with any units, and cannot have any active trigger actions that
        /// create units for it.
        /// </summary>
        /// <returns>List<Player> containing all currently active observer players</returns>
        public List<Player> Observers()
        {
            return observers;
        }

        public void DrawText(CoordinateType ctype, int x, int y, string @string, params Text[] colors)
        {
            string formatted = FormatString(@string, colors);
            AddShape(ShapeType.Text, ctype, x, y, 0, 0, formatted, (int)textSize, 0, false);
        }

        public void DrawTextMap(int x, int y, string @string, params Text[] colors)
        {
            DrawText(CoordinateType.Map, x, y, @string, colors);
        }

        public void DrawTextMap(Position p, string @string, params Text[] colors)
        {
            DrawTextMap(p.x, p.y, @string, colors);
        }

        public void DrawTextMouse(int x, int y, string @string, params Text[] colors)
        {
            DrawText(CoordinateType.Mouse, x, y, @string, colors);
        }

        public void DrawTextMouse(Position p, string @string, params Text[] colors)
        {
            DrawTextMouse(p.x, p.y, @string, colors);
        }

        public void DrawTextScreen(int x, int y, string @string, params Text[] colors)
        {
            DrawText(CoordinateType.Screen, x, y, @string, colors);
        }

        public void DrawTextScreen(Position p, string @string, params Text[] colors)
        {
            DrawTextScreen(p.x, p.y, @string, colors);
        }

        public void DrawBox(CoordinateType ctype, int left, int top, int right, int bottom, Color color)
        {
            DrawBox(ctype, left, top, right, bottom, color, false);
        }

        /// <summary>
        /// Draws a rectangle on the screen with the given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="left">The x coordinate, in pixels, relative to ctype, of the left edge of the rectangle.</param>
        /// <param name="top">The y coordinate, in pixels, relative to ctype, of the top edge of the rectangle.</param>
        /// <param name="right">The x coordinate, in pixels, relative to ctype, of the right edge of the rectangle.</param>
        /// <param name="bottom">The y coordinate, in pixels, relative to ctype, of the bottom edge of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="isSolid">If true, then the shape will be filled and drawn as a solid, otherwise it will be drawn as an outline. If omitted, this value will default to false.</param>
        public void DrawBox(CoordinateType ctype, int left, int top, int right, int bottom, Color color, bool isSolid)
        {
            AddShape(ShapeType.Box, ctype, left, top, right, bottom, 0, 0, color.id, isSolid);
        }

        public void DrawBoxMap(int left, int top, int right, int bottom, Color color)
        {
            DrawBox(CoordinateType.Map, left, top, right, bottom, color);
        }

        public void DrawBoxMap(int left, int top, int right, int bottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Map, left, top, right, bottom, color, isSolid);
        }

        public void DrawBoxMap(Position leftTop, Position rightBottom, Color color)
        {
            DrawBox(CoordinateType.Map, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color);
        }

        public void DrawBoxMap(Position leftTop, Position rightBottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Map, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color, isSolid);
        }

        public void DrawBoxMouse(int left, int top, int right, int bottom, Color color)
        {
            DrawBox(CoordinateType.Mouse, left, top, right, bottom, color);
        }

        public void DrawBoxMouse(int left, int top, int right, int bottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Mouse, left, top, right, bottom, color, isSolid);
        }

        public void DrawBoxMouse(Position leftTop, Position rightBottom, Color color)
        {
            DrawBox(CoordinateType.Mouse, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color);
        }

        public void DrawBoxMouse(Position leftTop, Position rightBottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Mouse, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color, isSolid);
        }

        public void DrawBoxScreen(int left, int top, int right, int bottom, Color color)
        {
            DrawBox(CoordinateType.Screen, left, top, right, bottom, color);
        }

        public void DrawBoxScreen(int left, int top, int right, int bottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Screen, left, top, right, bottom, color, isSolid);
        }

        public void DrawBoxScreen(Position leftTop, Position rightBottom, Color color)
        {
            DrawBox(CoordinateType.Screen, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color);
        }

        public void DrawBoxScreen(Position leftTop, Position rightBottom, Color color, bool isSolid)
        {
            DrawBox(CoordinateType.Screen, leftTop.x, leftTop.y, rightBottom.x, rightBottom.y, color, isSolid);
        }

        public void DrawTriangle(CoordinateType ctype, int ax, int ay, int bx, int by, int cx, int cy, Color color)
        {
            DrawTriangle(ctype, ax, ay, bx, by, cx, cy, color, false);
        }

        /// <summary>
        /// Draws a triangle on the screen with the given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="ax">The x coordinate, in pixels, relative to ctype, of the first point.</param>
        /// <param name="ay">The y coordinate, in pixels, relative to ctype, of the first point.</param>
        /// <param name="bx">The x coordinate, in pixels, relative to ctype, of the second point.</param>
        /// <param name="by">The y coordinate, in pixels, relative to ctype, of the second point.</param>
        /// <param name="cx">The x coordinate, in pixels, relative to ctype, of the third point.</param>
        /// <param name="cy">The y coordinate, in pixels, relative to ctype, of the third point.</param>
        /// <param name="color">The color of the triangle.</param>
        /// <param name="isSolid">If true, then the shape will be filled and drawn as a solid, otherwise it will be drawn as an outline. If omitted, this value will default to false.</param>
        public void DrawTriangle(CoordinateType ctype, int ax, int ay, int bx, int by, int cx, int cy, Color color, bool isSolid)
        {
            AddShape(ShapeType.Triangle, ctype, ax, ay, bx, by, cx, cy, color.id, isSolid);
        }

        public void DrawTriangleMap(int ax, int ay, int bx, int by, int cx, int cy, Color color)
        {
            DrawTriangle(CoordinateType.Map, ax, ay, bx, by, cx, cy, color);
        }

        public void DrawTriangleMap(int ax, int ay, int bx, int by, int cx, int cy, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Map, ax, ay, bx, by, cx, cy, color, isSolid);
        }

        public void DrawTriangleMap(Position a, Position b, Position c, Color color)
        {
            DrawTriangle(CoordinateType.Map, a.x, a.y, b.x, b.y, c.x, c.y, color);
        }

        public void DrawTriangleMap(Position a, Position b, Position c, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Map, a.x, a.y, b.x, b.y, c.x, c.y, color, isSolid);
        }

        public void DrawTriangleMouse(int ax, int ay, int bx, int by, int cx, int cy, Color color)
        {
            DrawTriangle(CoordinateType.Mouse, ax, ay, bx, by, cx, cy, color);
        }

        public void DrawTriangleMouse(int ax, int ay, int bx, int by, int cx, int cy, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Mouse, ax, ay, bx, by, cx, cy, color, isSolid);
        }

        public void DrawTriangleMouse(Position a, Position b, Position c, Color color)
        {
            DrawTriangle(CoordinateType.Mouse, a.x, a.y, b.x, b.y, c.x, c.y, color);
        }

        public void DrawTriangleMouse(Position a, Position b, Position c, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Mouse, a.x, a.y, b.x, b.y, c.x, c.y, color, isSolid);
        }

        public void DrawTriangleScreen(int ax, int ay, int bx, int by, int cx, int cy, Color color)
        {
            DrawTriangle(CoordinateType.Screen, ax, ay, bx, by, cx, cy, color);
        }

        public void DrawTriangleScreen(int ax, int ay, int bx, int by, int cx, int cy, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Screen, ax, ay, bx, by, cx, cy, color, isSolid);
        }

        public void DrawTriangleScreen(Position a, Position b, Position c, Color color)
        {
            DrawTriangle(CoordinateType.Screen, a.x, a.y, b.x, b.y, c.x, c.y, color);
        }

        public void DrawTriangleScreen(Position a, Position b, Position c, Color color, bool isSolid)
        {
            DrawTriangle(CoordinateType.Screen, a.x, a.y, b.x, b.y, c.x, c.y, color, isSolid);
        }

        public void DrawCircle(CoordinateType ctype, int x, int y, int radius, Color color)
        {
            DrawCircle(ctype, x, y, radius, color, false);
        }

        /// <summary>
        /// Draws a circle on the screen with the given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="x">The x coordinate, in pixels, relative to ctype.</param>
        /// <param name="y">The y coordinate, in pixels, relative to ctype.</param>
        /// <param name="radius">The radius of the circle, in pixels.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="isSolid">If true, then the shape will be filled and drawn as a solid, otherwise it will be drawn as an outline. If omitted, this value will default to false.</param>
        public void DrawCircle(CoordinateType ctype, int x, int y, int radius, Color color, bool isSolid)
        {
            AddShape(ShapeType.Circle, ctype, x, y, 0, 0, radius, 0, color.id, isSolid);
        }

        public void DrawCircleMap(int x, int y, int radius, Color color)
        {
            DrawCircle(CoordinateType.Map, x, y, radius, color);
        }

        public void DrawCircleMap(int x, int y, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Map, x, y, radius, color, isSolid);
        }

        public void DrawCircleMap(Position p, int radius, Color color)
        {
            DrawCircle(CoordinateType.Map, p.x, p.y, radius, color);
        }

        public void DrawCircleMap(Position p, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Map, p.x, p.y, radius, color, isSolid);
        }

        public void DrawCircleMouse(int x, int y, int radius, Color color)
        {
            DrawCircle(CoordinateType.Mouse, x, y, radius, color);
        }

        public void DrawCircleMouse(int x, int y, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Mouse, x, y, radius, color, isSolid);
        }

        public void DrawCircleMouse(Position p, int radius, Color color)
        {
            DrawCircle(CoordinateType.Mouse, p.x, p.y, radius, color);
        }

        public void DrawCircleMouse(Position p, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Mouse, p.x, p.y, radius, color, isSolid);
        }

        public void DrawCircleScreen(int x, int y, int radius, Color color)
        {
            DrawCircle(CoordinateType.Screen, x, y, radius, color);
        }

        public void DrawCircleScreen(int x, int y, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Screen, x, y, radius, color, isSolid);
        }

        public void DrawCircleScreen(Position p, int radius, Color color)
        {
            DrawCircle(CoordinateType.Screen, p.x, p.y, radius, color);
        }

        public void DrawCircleScreen(Position p, int radius, Color color, bool isSolid)
        {
            DrawCircle(CoordinateType.Screen, p.x, p.y, radius, color, isSolid);
        }

        public void DrawEllipse(CoordinateType ctype, int x, int y, int xrad, int yrad, Color color)
        {
            DrawEllipse(ctype, x, y, xrad, yrad, color, false);
        }

        /// <summary>
        /// Draws an ellipse on the screen with the given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="x">The x coordinate, in pixels, relative to ctype.</param>
        /// <param name="y">The y coordinate, in pixels, relative to ctype.</param>
        /// <param name="xrad">The x radius of the ellipse, in pixels.</param>
        /// <param name="yrad">The y radius of the ellipse, in pixels.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="isSolid">If true, then the shape will be filled and drawn as a solid, otherwise it will be drawn as an outline. If omitted, this value will default to false.</param>
        public void DrawEllipse(CoordinateType ctype, int x, int y, int xrad, int yrad, Color color, bool isSolid)
        {
            AddShape(ShapeType.Ellipse, ctype, x, y, 0, 0, xrad, yrad, color.id, isSolid);
        }

        public void DrawEllipseMap(int x, int y, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Map, x, y, xrad, yrad, color);
        }

        public void DrawEllipseMap(int x, int y, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Map, x, y, xrad, yrad, color, isSolid);
        }

        public void DrawEllipseMap(Position p, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Map, p.x, p.y, xrad, yrad, color);
        }

        public void DrawEllipseMap(Position p, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Map, p.x, p.y, xrad, yrad, color, isSolid);
        }

        public void DrawEllipseMouse(int x, int y, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Mouse, x, y, xrad, yrad, color);
        }

        public void DrawEllipseMouse(int x, int y, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Mouse, x, y, xrad, yrad, color, isSolid);
        }

        public void DrawEllipseMouse(Position p, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Mouse, p.x, p.y, xrad, yrad, color);
        }

        public void DrawEllipseMouse(Position p, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Mouse, p.x, p.y, xrad, yrad, color, isSolid);
        }

        public void DrawEllipseScreen(int x, int y, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Screen, x, y, xrad, yrad, color);
        }

        public void DrawEllipseScreen(int x, int y, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Mouse, x, y, xrad, yrad, color, isSolid);
        }

        public void DrawEllipseScreen(Position p, int xrad, int yrad, Color color)
        {
            DrawEllipse(CoordinateType.Mouse, p.x, p.y, xrad, yrad, color);
        }

        public void DrawEllipseScreen(Position p, int xrad, int yrad, Color color, bool isSolid)
        {
            DrawEllipse(CoordinateType.Mouse, p.x, p.y, xrad, yrad, color, isSolid);
        }

        /// <summary>
        /// Draws a dot on the map or screen with a given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="x">The x coordinate, in pixels, relative to ctype.</param>
        /// <param name="y">The y coordinate, in pixels, relative to ctype.</param>
        /// <param name="color">The color of the dot.</param>
        public void DrawDot(CoordinateType ctype, int x, int y, Color color)
        {
            AddShape(ShapeType.Dot, ctype, x, y, 0, 0, 0, 0, color.id, false);
        }

        public void DrawDotMap(int x, int y, Color color)
        {
            DrawDot(CoordinateType.Map, x, y, color);
        }

        public void DrawDotMap(Position p, Color color)
        {
            DrawDot(CoordinateType.Map, p.x, p.y, color);
        }

        public void DrawDotMouse(int x, int y, Color color)
        {
            DrawDot(CoordinateType.Mouse, x, y, color);
        }

        public void DrawDotMouse(Position p, Color color)
        {
            DrawDot(CoordinateType.Mouse, p.x, p.y, color);
        }

        public void DrawDotScreen(int x, int y, Color color)
        {
            DrawDot(CoordinateType.Screen, x, y, color);
        }

        public void DrawDotScreen(Position p, Color color)
        {
            DrawDot(CoordinateType.Screen, p.x, p.y, color);
        }

        /// <summary>
        /// Draws a line on the map or screen with a given color.
        /// </summary>
        /// <param name="ctype">The coordinate type. Indicates the relative position to draw the shape.</param>
        /// <param name="x1">The starting x coordinate, in pixels, relative to ctype.</param>
        /// <param name="y1">The starting y coordinate, in pixels, relative to ctype.</param>
        /// <param name="x2">The ending x coordinate, in pixels, relative to ctype.</param>
        /// <param name="y2">The ending y coordinate, in pixels, relative to ctype.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(CoordinateType ctype, int x1, int y1, int x2, int y2, Color color)
        {
            AddShape(ShapeType.Line, ctype, x1, y1, x2, y2, 0, 0, color.id, false);
        }

        public void DrawLineMap(int x1, int y1, int x2, int y2, Color color)
        {
            DrawLine(CoordinateType.Map, x1, y1, x2, y2, color);
        }

        public void DrawLineMap(Position a, Position b, Color color)
        {
            DrawLine(CoordinateType.Map, a.x, a.y, b.x, b.y, color);
        }

        public void DrawLineMouse(int x1, int y1, int x2, int y2, Color color)
        {
            DrawLine(CoordinateType.Mouse, x1, y1, x2, y2, color);
        }

        public void DrawLineMouse(Position a, Position b, Color color)
        {
            DrawLine(CoordinateType.Mouse, a.x, a.y, b.x, b.y, color);
        }

        public void DrawLineScreen(int x1, int y1, int x2, int y2, Color color)
        {
            DrawLine(CoordinateType.Screen, x1, y1, x2, y2, color);
        }

        public void DrawLineScreen(Position a, Position b, Color color)
        {
            DrawLine(CoordinateType.Screen, a.x, a.y, b.x, b.y, color);
        }

        /// <summary>
        /// Retrieves the maximum delay, in number of frames, between a command being issued
        /// and the command being executed by Broodwar.
        /// <p>
        /// In Broodwar, latency is used to keep the game synchronized between players without
        /// introducing lag.
        /// </summary>
        /// <returns>Difference in frames between commands being sent and executed.</returns>
        /// <remarks>
        /// @see#getLatencyTime
        /// @see#getRemainingLatencyFrames
        /// </remarks>
        public int GetLatencyFrames()
        {
            return _clientData.GameData.GetLatencyFrames();
        }

        /// <summary>
        /// Retrieves the maximum delay, in milliseconds, between a command being issued and
        /// the command being executed by Broodwar.
        /// </summary>
        /// <returns>Difference in milliseconds between commands being sent and executed.</returns>
        /// <remarks>
        /// @see#getLatencyFrames
        /// @see#getRemainingLatencyTime
        /// </remarks>
        public int GetLatencyTime()
        {
            return _clientData.GameData.GetLatencyTime();
        }

        /// <summary>
        /// Retrieves the number of frames it will take before a command sent in the current
        /// frame will be executed by the game.
        /// </summary>
        /// <returns>Number of frames until a command is executed if it were sent in the current
        /// frame.</returns>
        /// <remarks>
        /// @see#getRemainingLatencyTime
        /// @see#getLatencyFrames
        /// </remarks>
        public int GetRemainingLatencyFrames()
        {
            return _clientData.GameData.GetRemainingLatencyFrames();
        }

        /// <summary>
        /// Retrieves the number of milliseconds it will take before a command sent in the
        /// current frame will be executed by Broodwar.
        /// </summary>
        /// <returns>Amount of time, in milliseconds, until a command is executed if it were sent in
        /// the current frame.</returns>
        /// <remarks>
        /// @see#getRemainingLatencyFrames
        /// @see#getLatencyTime
        /// </remarks>
        public int GetRemainingLatencyTime()
        {
            return _clientData.GameData.GetRemainingLatencyTime();
        }

        /// <summary>
        /// Retrieves the current revision of BWAPI.
        /// </summary>
        /// <returns>The revision number of the current BWAPI interface.</returns>
        public int GetRevision()
        {
            return _revision;
        }

        /// <summary>
        /// Retrieves the debug state of the BWAPI build.
        /// </summary>
        /// <returns>true if the BWAPI module is a DEBUG build, and false if it is a RELEASE build.</returns>
        public bool IsDebug()
        {
            return _debug;
        }

        /// <summary>
        /// Checks the state of latency compensation.
        /// </summary>
        /// <returns>true if latency compensation is enabled, false if it is disabled.</returns>
        /// <remarks>@see#setLatCom</remarks>
        public bool IsLatComEnabled()
        {
            return latcom;
        }

        /// <summary>
        /// Changes the state of latency compensation. Latency compensation
        /// modifies the state of BWAPI's representation of units to reflect the implications of
        /// issuing a command immediately after the command was performed, instead of waiting
        /// consecutive frames for the results. Latency compensation is enabled by default.
        /// </summary>
        /// <param name="isEnabled">Set whether the latency compensation feature will be enabled (true) or disabled (false).</param>
        /// <remarks>@see#isLatComEnabled</remarks>
        public void SetLatCom(bool isEnabled)
        {
            // update shared memory
            _clientData.GameData.SetHasLatCom(isEnabled);

            // update internal memory
            latcom = isEnabled;

            // update server
            AddCommand(CommandType.SetLatCom, isEnabled ? 1 : 0, 0);
        }

        /// <summary>
        /// Retrieves the Starcraft instance number recorded by BWAPI to identify which
        /// Starcraft instance an AI module belongs to. The very first instance should
        /// return 0.
        /// </summary>
        /// <returns>An integer value representing the instance number.</returns>
        public int GetInstanceNumber()
        {
            return _clientData.GameData.GetInstanceID();
        }

        public int GetAPM()
        {
            return GetAPM(false);
        }

        /// <summary>
        /// Retrieves the Actions Per Minute (APM) that the bot is producing.
        /// </summary>
        /// <param name="includeSelects">If true, the return value will include selections as individual commands, otherwise it will exclude selections. This value is false by default.</param>
        /// <returns>The number of actions that the bot has executed per minute, on average.</returns>
        public int GetAPM(bool includeSelects)
        {
            return includeSelects ? _clientData.GameData.GetBotAPMSelects() : _clientData.GameData.GetBotAPMNoSelects();
        }

        /// <summary>
        /// Sets the number of graphical frames for every logical frame. This
        /// allows the game to step more logical frames per graphical frame, increasing the speed at
        /// which the game runs.
        /// </summary>
        /// <param name="frameSkip">Number of graphical frames per logical frame. If this value is 0 or less, then it will default to 1.</param>
        /// <remarks>@see#setLocalSpeed</remarks>
        public void SetFrameSkip(int frameSkip)
        {
            AddCommand(CommandType.SetFrameSkip, Math.Max(frameSkip, 1), 0);
        }

        /// <summary>
        /// Sets the alliance state of the current player with the target player.</summary>
        /// </summary>
        /// <param name="player">The target player to set alliance with.</param>
        /// <param name="allied">If true, the current player will ally the target player. If false, the current player
        ///                      will make the target player an enemy. This value is true by default.</param>
        /// <param name="alliedVictory">Sets the state of "allied victory". If true, the game will end in a victory if all
        ///                      allied players have eliminated their opponents. Otherwise, the game will only end if
        ///                      no other players are remaining in the game. This value is true by default.</param>
        public bool SetAlliance(Player player, bool allied, bool alliedVictory)
        {
            if (Self() == null || IsReplay() || player == null || player.Equals(Self()))
            {
                return false;
            }

            AddCommand(CommandType.SetAllies, player.GetID(), allied ? (alliedVictory ? 2 : 1) : 0);
            return true;
        }

        public bool SetAlliance(Player player, bool allied)
        {
            return SetAlliance(player, allied, true);
        }

        public bool SetAlliance(Player player)
        {
            return SetAlliance(player, true);
        }

        /// <summary>
        /// In a game, this function sets the vision of the current BWAPI player with the
        /// target player.
        /// <p>
        /// In a replay, this function toggles the visibility of the target player.
        /// </summary>
        /// <param name="player">The target player to toggle vision.</param>
        /// <param name="enabled">The vision state. If true, and in a game, the current player will enable shared vision
        ///                with the target player, otherwise it will unshare vision. If in a replay, the vision
        ///                of the target player will be shown, otherwise the target player will be hidden. This
        ///                value is true by default.</param>
        public bool SetVision(Player player, bool enabled)
        {
            if (player == null)
            {
                return false;
            }

            if (!IsReplay() && (Self() == null || player.Equals(Self())))
            {
                return false;
            }

            AddCommand(CommandType.SetVision, player.GetID(), enabled ? 1 : 0);
            return true;
        }

        /// <summary>
        /// Checks if the GUI is enabled.
        /// <p>
        /// The GUI includes all drawing functions of BWAPI, as well as screen updates from Broodwar.
        /// </summary>
        /// <returns>true if the GUI is enabled, and everything is visible, false if the GUI is disabled and drawing
        /// functions are rejected</returns>
        /// <remarks>@see#setGUI</remarks>
        public bool IsGUIEnabled()
        {
            return _clientData.GameData.GetHasGUI();
        }

        /// <summary>
        /// Sets the rendering state of the Starcraft GUI.
        /// <p>
        /// This typically gives Starcraft a very low graphical frame rate and disables all drawing functionality in BWAPI.
        /// </summary>
        /// <param name="enabled">A bool value that determines the state of the GUI. Passing false to this function
        ///                will disable the GUI, and true will enable it.</param>
        /// <remarks>@see#isGUIEnabled</remarks>
        public void SetGUI(bool enabled)
        {
            _clientData.GameData.SetHasGUI(enabled);

            //queue up command for server so it also applies the change
            AddCommand(CommandType.SetGui, enabled ? 1 : 0, 0);
        }

        /// <summary>
        /// Retrieves the amount of time (in milliseconds) that has elapsed when running the last AI
        /// module callback.
        /// <p>
        /// This is used by tournament modules to penalize AI modules that use too
        /// much processing time.
        /// </summary>
        /// <returns>Time in milliseconds spent in last AI module call. Returns 0 When called from an AI module.</returns>
        public int GetLastEventTime()
        {
            return 0;
        }

        /// <summary>
        /// Changes the map to the one specified.
        /// <p>
        /// Once restarted, the game will load the map that was provided.
        /// Changes do not take effect unless the game is restarted.
        /// </summary>
        /// <param name="mapFileName">A string containing the path and file name to the desired map.</param>
        /// <returns>Returns true if the function succeeded and has changed the map. Returns false if the function failed,
        /// does not have permission from the tournament module, failed to find the map specified, or received an invalid
        /// parameter.</returns>
        public bool SetMap(string mapFileName)
        {
            if (mapFileName == null || mapFileName.Length >= 260 || mapFileName[0] == 0)
            {
                return false;
            }

            AddCommand(CommandType.SetMap, mapFileName, 0);
            return true;
        }

        /// <summary>
        /// Sets the state of the fog of war when watching a replay.
        /// </summary>
        /// <param name="reveal">The state of the reveal all flag. If false, all fog of war will be enabled. If true,
        ///               then the fog of war will be revealed. It is true by default.</param>
        public bool SetRevealAll(bool reveal)
        {
            if (!IsReplay())
            {
                return false;
            }

            AddCommand(CommandType.SetRevealAll, reveal ? 1 : 0, 0);
            return true;
        }

        public bool SetRevealAll()
        {
            return SetRevealAll(true);
        }

        /// <summary>
        /// Checks if there is a path from source to destination. This only checks
        /// if the source position is connected to the destination position. This function does not
        /// check if all units can actually travel from source to destination. Because of this
        /// limitation, it has an O(1) complexity, and cases where this limitation hinders gameplay is
        /// uncommon at best.
        /// <p>
        /// If making queries on a unit, it's better to call {@link Unit#hasPath}, since it is
        /// a more lenient version of this function that accounts for some edge cases.
        /// </summary>
        /// <param name="source">The source position.</param>
        /// <param name="destination">The destination position.</param>
        /// <returns>true if there is a path between the two positions, and false if there is not.</returns>
        /// <remarks>@seeUnit#hasPath</remarks>
        public bool HasPath(Position source, Position destination)
        {
            if (source.IsValid(this) && destination.IsValid(this))
            {
                Region rgnA = GetRegionAt(source);
                Region rgnB = GetRegionAt(destination);
                return rgnA != null && rgnB != null && rgnA.GetRegionGroupID() == rgnB.GetRegionGroupID();
            }

            return false;
        }

        public void SetTextSize()
        {
            SetTextSize(TextSize.Default);
        }

        /// <summary>
        /// Sets the size of the text for all calls to {@link #drawText} following this one.
        /// </summary>
        /// <param name="size">The size of the text. This value is one of Text#Size. If this value is omitted, then a default value of {@link TextSize#Default} is used.</param>
        /// <remarks>@seeTextSize</remarks>
        public void SetTextSize(TextSize size)
        {
            textSize = size;
        }

        /// <summary>
        /// Retrieves current amount of time in seconds that the game has elapsed.
        /// </summary>
        /// <returns>Time, in seconds, that the game has elapsed as an integer.</returns>
        public int ElapsedTime()
        {
            return _clientData.GameData.GetElapsedTime();
        }

        /// <summary>
        /// Sets the command optimization level. Command optimization is a feature
        /// in BWAPI that tries to reduce the APM of the bot by grouping or eliminating unnecessary
        /// game actions. For example, suppose the bot told 24 @Zerglings to @Burrow. At command
        /// optimization level 0, BWAPI is designed to select each Zergling to burrow individually,
        /// which costs 48 actions. With command optimization level 1, it can perform the same
        /// behaviour using only 4 actions. The command optimizer also reduces the amount of bytes used
        /// for each action if it can express the same action using a different command. For example,
        /// Right_Click uses less bytes than Move.
        /// </summary>
        /// <param name="level">An integer representation of the aggressiveness for which commands are optimized. A lower level means less optimization, and a higher level means more optimization.
        ///              <p>
        ///              The values for level are as follows:
        ///              - 0: No optimization.
        ///              - 1: Some optimization.
        ///              - Is not detected as a hack.
        ///              - Does not alter behaviour.
        ///              - Units performing the following actions are grouped and ordered 12 at a time:
        ///              - Attack_Unit
        ///              - Morph (@Larva only)
        ///              - Hold_Position
        ///              - Stop
        ///              - Follow
        ///              - Gather
        ///              - Return_Cargo
        ///              - Repair
        ///              - Burrow
        ///              - Unburrow
        ///              - Cloak
        ///              - Decloak
        ///              - Siege
        ///              - Unsiege
        ///              - Right_Click_Unit
        ///              - Halt_Construction
        ///              - Cancel_Train (@Carrier and @Reaver only)
        ///              - Cancel_Train_Slot (@Carrier and @Reaver only)
        ///              - Cancel_Morph (for non-buildings only)
        ///              - Use_Tech
        ///              - Use_Tech_Unit
        ///              .
        ///              - The following order transformations are applied to allow better grouping:
        ///              - Attack_Unit becomes Right_Click_Unit if the target is an enemy
        ///              - Move becomes Right_Click_Position
        ///              - Gather becomes Right_Click_Unit if the target contains resources
        ///              - Set_Rally_Position becomes Right_Click_Position for buildings
        ///              - Set_Rally_Unit becomes Right_Click_Unit for buildings
        ///              - Use_Tech_Unit with Infestation becomes Right_Click_Unit if the target is valid
        ///              .
        ///              .
        ///              - 2: More optimization by grouping structures.
        ///              - Includes the optimizations made by all previous levels.
        ///              - May be detected as a hack by some replay utilities.
        ///              - Does not alter behaviour.
        ///              - Units performing the following actions are grouped and ordered 12 at a time:
        ///              - Attack_Unit (@Turrets, @Photon_Cannons, @Sunkens, @Spores)
        ///              - Train
        ///              - Morph
        ///              - Set_Rally_Unit
        ///              - Lift
        ///              - Cancel_Construction
        ///              - Cancel_Addon
        ///              - Cancel_Train
        ///              - Cancel_Train_Slot
        ///              - Cancel_Morph
        ///              - Cancel_Research
        ///              - Cancel_Upgrade
        ///              .
        ///              .
        ///              - 3: Extensive optimization
        ///              - Includes the optimizations made by all previous levels.
        ///              - Units may behave or move differently than expected.
        ///              - Units performing the following actions are grouped and ordered 12 at a time:
        ///              - Attack_Move
        ///              - Set_Rally_Position
        ///              - Move
        ///              - Patrol
        ///              - Unload_All
        ///              - Unload_All_Position
        ///              - Right_Click_Position
        ///              - Use_Tech_Position
        ///              .
        ///              .
        ///              - 4: Aggressive optimization
        ///              - Includes the optimizations made by all previous levels.
        ///              - Positions used in commands will be rounded to multiples of 32.
        ///              - @High_Templar and @Dark_Templar that merge into @Archons will be grouped and may
        ///              choose a different target to merge with. It will not merge with a target that
        ///              wasn't included.
        ///              .
        ///              .</param>
        public void SetCommandOptimizationLevel(int level)
        {
            AddCommand(CommandType.SetCommandOptimizerLevel, level, 0);
        }

        /// <summary>
        /// Returns the remaining countdown time. The countdown timer is used in @CTF and @UMS game types.
        /// </summary>
        /// <returns>Integer containing the time (in game seconds) on the countdown timer.</returns>
        public int CountdownTimer()
        {
            return _clientData.GameData.GetCountdownTimer();
        }

        /// <summary>
        /// Retrieves the set of all regions on the map.
        /// </summary>
        /// <returns>List<Region> containing all map regions.</returns>
        public List<Region> GetAllRegions()
        {
            return _regionSet;
        }

        /// <summary>
        /// Retrieves the region at a given position.
        /// </summary>
        /// <param name="x">The x coordinate, in pixels.</param>
        /// <param name="y">The y coordinate, in pixels.</param>
        /// <returns>the Region interface at the given position. Returns null if the provided position is not valid (i.e. not within the map bounds).</returns>
        /// <remarks>
        /// @see#getAllRegions
        /// @see#getRegion
        /// </remarks>
        public Region GetRegionAt(int x, int y)
        {
            return GetRegionAt(new Position(x, y));
        }

        public Region GetRegionAt(Position position)
        {
            if (!position.IsValid(this))
            {
                return null;
            }

            short idx = _mapTileRegionID[position.x / 32, position.y / 32];
            if ((idx & 0x2000) != 0)
            {
                int index = idx & 0x1FFF;
                if (index >= RegionDataSize)
                {
                    return null;
                }

                int minitileShift = ((position.x & 0x1F) / 8) + ((position.y & 0x1F) / 8) * 4;
                if (((_mapSplitTilesMiniTileMask[index] >> minitileShift) & 1) != 0)
                {
                    return GetRegion(_mapSplitTilesRegion2[index]);
                }
                else
                {
                    return GetRegion(_mapSplitTilesRegion1[index]);
                }
            }

            return GetRegion(idx);
        }

        public TilePosition GetBuildLocation(UnitType type, TilePosition desiredPosition, int maxRange)
        {
            return GetBuildLocation(type, desiredPosition, maxRange, false);
        }

        public TilePosition GetBuildLocation(UnitType type, TilePosition desiredPosition)
        {
            return GetBuildLocation(type, desiredPosition, 64);
        }

        /// <summary>
        /// Retrieves a basic build position just as the default Computer AI would.
        /// This allows users to find simple build locations without relying on external libraries.
        /// </summary>
        /// <param name="type">A valid UnitType representing the unit type to accomodate space for.</param>
        /// <param name="desiredPosition">A valid TilePosition containing the desired placement position.</param>
        /// <param name="maxRange">The maximum distance (in tiles) to build from desiredPosition.</param>
        /// <param name="creep">A special bool value that changes the behaviour of @Creep_Colony placement.</param>
        /// <returns>A TilePosition containing the location that the structure should be constructed at. Returns {@link TilePosition#Invalid} If a build location could not be found within maxRange.</returns>
        public TilePosition GetBuildLocation(UnitType type, TilePosition desiredPosition, int maxRange, bool creep)
        {
            return BuildingPlacer.GetBuildLocation(type, desiredPosition, maxRange, creep, this);
        }

        private int GetDamageFromImpl(UnitType fromType, UnitType toType, Player fromPlayer, Player toPlayer)
        {

            // Retrieve appropriate weapon
            WeaponType wpn = toType.IsFlyer() ? fromType.AirWeapon() : fromType.GroundWeapon();
            if (wpn == WeaponType.None || wpn == WeaponType.Unknown)
            {
                return 0;
            }


            // Get initial weapon damage
            int dmg = fromPlayer != null ? fromPlayer.Damage(wpn) : wpn.DamageAmount() * wpn.DamageFactor();

            // If we need to calculate using armor
            if (wpn.DamageType() != DamageType.Ignore_Armor && toPlayer != null)
            {
                dmg -= Math.Min(dmg, toPlayer.Armor(toType));
            }

            return dmg * _damageRatio[(int)wpn.DamageType()][(int)toType.Size()] / 256;
        }

        public int GetDamageFrom(UnitType fromType, UnitType toType, Player fromPlayer)
        {
            return GetDamageFrom(fromType, toType, fromPlayer, null);
        }

        public int GetDamageFrom(UnitType fromType, UnitType toType)
        {
            return GetDamageFrom(fromType, toType, null);
        }

        /// <summary>
        /// Calculates the damage received for a given player. It can be understood
        /// as the damage from fromType to toType. Does not include shields in calculation.
        /// Includes upgrades if players are provided.
        /// </summary>
        /// <param name="fromType">The unit type that will be dealing the damage.</param>
        /// <param name="toType">The unit type that will be receiving the damage.</param>
        /// <param name="fromPlayer">The player owner of the given type that will be dealing the damage. If omitted, then no player will be used to calculate the upgrades for fromType.</param>
        /// <param name="toPlayer">The player owner of the type that will be receiving the damage. If omitted, then this parameter will default to {@link #self}.</param>
        /// <returns>The amount of damage that fromType would deal to toType.</returns>
        /// <remarks>@see#getDamageTo</remarks>
        public int GetDamageFrom(UnitType fromType, UnitType toType, Player fromPlayer, Player toPlayer)
        {
            return GetDamageFromImpl(fromType, toType, fromPlayer, toPlayer ?? Self());
        }

        public int GetDamageTo(UnitType toType, UnitType fromType, Player toPlayer)
        {
            return GetDamageTo(toType, fromType, toPlayer, null);
        }

        public int GetDamageTo(UnitType toType, UnitType fromType)
        {
            return GetDamageTo(toType, fromType, null);
        }

        /// <summary>
        /// Calculates the damage dealt for a given player. It can be understood as
        /// the damage to toType from fromType. Does not include shields in calculation.
        /// Includes upgrades if players are provided.
        /// <p>
        /// This function is nearly the same as {@link #getDamageFrom}. The only difference is that
        /// the last parameter is intended to default to {@link #self}.
        /// </summary>
        /// <param name="toType">The unit type that will be receiving the damage.</param>
        /// <param name="fromType">The unit type that will be dealing the damage.</param>
        /// <param name="toPlayer">The player owner of the type that will be receiving the damage. If omitted, then no player will be used to calculate the upgrades for toType.</param>
        /// <param name="fromPlayer">The player owner of the given type that will be dealing the damage. If omitted, then this parameter will default to {@link #self}).</param>
        /// <returns>The amount of damage that fromType would deal to toType.</returns>
        /// <remarks>@see#getDamageFrom</remarks>
        public int GetDamageTo(UnitType toType, UnitType fromType, Player toPlayer, Player fromPlayer)
        {
            return GetDamageFromImpl(fromType, toType, fromPlayer ?? Self(), toPlayer);
        }

        /// <summary>
        /// Retrieves the initial random seed that was used in this game's creation.
        /// This is used to identify the seed that started this game, in case an error occurred, so
        /// that developers can deterministically reproduce the error. Works in both games and replays.
        /// </summary>
        /// <returns>This game's random seed.</returns>
        /// <remarks>@since4.2.0</remarks>
        public int GetRandomSeed()
        {
            return _randomSeed;
        }

        /// <summary>
        /// Convenience method for adding a unit command from raw arguments.
        /// </summary>
        public void AddUnitCommand(UnitCommandType type, int unit, int target, int x, int y, int extra)
        {
            EnqueueOrDo(SideEffect.AddUnitCommand(type, unit, target, x, y, extra));
        }

        /// <summary>
        /// Convenience method for adding a game command from raw arguments.
        /// </summary>
        public void AddCommand(CommandType type, int value1, int value2)
        {
            EnqueueOrDo(SideEffect.AddCommand(type, value1, value2));
        }

        /// <summary>
        /// Convenience method for adding a game command from raw arguments.
        /// </summary>
        public void AddCommand(CommandType type, string value1, int value2)
        {
            EnqueueOrDo(SideEffect.AddCommand(type, value1, value2));
        }

        /// <summary>
        /// Convenience method for adding a shape from raw arguments.
        /// </summary>
        public void AddShape(ShapeType type, CoordinateType coordType, int x1, int y1, int x2, int y2, int extra1, int extra2, int color, bool isSolid)
        {
            EnqueueOrDo(SideEffect.AddShape(type, coordType, x1, y1, x2, y2, extra1, extra2, color, isSolid));
        }

        /// <summary>
        /// Convenience method for adding a shape from raw arguments.
        /// </summary>
        public void AddShape(ShapeType type, CoordinateType coordType, int x1, int y1, int x2, int y2, string text, int extra2, int color, bool isSolid)
        {
            EnqueueOrDo(SideEffect.AddShape(type, coordType, x1, y1, x2, y2, text, extra2, color, isSolid));
        }

        /// <summary>
        /// Applies a side effect, either immediately (if operating synchronously)
        /// or by enqueuing it for later execution (if operating asynchronously).
        /// </summary>
        /// <param name="sideEffect"></param>
        public void EnqueueOrDo(SideEffect sideEffect)
        {
            sideEffect.Apply(_clientData.GameData);
        }

        public void SetAllUnits(List<Unit> units)
        {
            _allUnits = units.ToList();
        }

        public ClientData ClientData
        {
            get => _clientData;
        }
    }
}