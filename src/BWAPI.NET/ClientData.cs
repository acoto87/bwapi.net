using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    public sealed class ClientData
    {
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly GameData_ _gameData;

        public ClientData(MemoryMappedViewAccessor accessor)
        {
            _accessor = accessor;
            _gameData = new GameData_(_accessor, 0);
        }

        public GameData_ GameData
        {
            get => _gameData;
        }

        public class GameData_
        {
            public const int Size = 33017048;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public GameData_(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public int GetClientVersion()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetClientVersion(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int GetRevision()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetRevision(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public bool IsDebug()
            {
                return _accessor.ReadByte(_offset + 8) != 0;
            }

            public void SetIsDebug(bool value)
            {
                _accessor.Write(_offset + 8, (byte)(value ? 1 : 0));
            }

            public int GetInstanceID()
            {
                return _accessor.ReadInt32(_offset + 12);
            }

            public void SetInstanceID(int value)
            {
                _accessor.Write(_offset + 12, value);
            }

            public int GetBotAPMNoSelects()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetBotAPMNoSelects(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetBotAPMSelects()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetBotAPMSelects(int value)
            {
                _accessor.Write(_offset + 20, value);
            }

            public int GetForceCount()
            {
                return _accessor.ReadInt32(_offset + 24);
            }

            public void SetForceCount(int value)
            {
                _accessor.Write(_offset + 24, value);
            }

            public ForceData GetForces(int i)
            {
                return new ForceData(_accessor, _offset + 28 + 32 * 1 * i);
            }

            public int GetPlayerCount()
            {
                return _accessor.ReadInt32(_offset + 188);
            }

            public void SetPlayerCount(int value)
            {
                _accessor.Write(_offset + 188, value);
            }

            public PlayerData GetPlayers(int i)
            {
                return new PlayerData(_accessor, _offset + 192 + 5788 * 1 * i);
            }

            public int GetInitialUnitCount()
            {
                return _accessor.ReadInt32(_offset + 69648);
            }

            public void SetInitialUnitCount(int value)
            {
                _accessor.Write(_offset + 69648, value);
            }

            public UnitData GetUnits(int i)
            {
                return new UnitData(_accessor, _offset + 69656 + 336 * 1 * i);
            }

            public int GetUnitArray(int i)
            {
                return _accessor.ReadInt32(_offset + 3429656 + 4 * 1 * i);
            }

            public void SetUnitArray(int i, int value)
            {
                _accessor.Write(_offset + 3429656 + 4 * 1 * i, value);
            }

            public BulletData GetBullets(int i)
            {
                return new BulletData(_accessor, _offset + 3436456 + 80 * 1 * i);
            }

            public int GetNukeDotCount()
            {
                return _accessor.ReadInt32(_offset + 3444456);
            }

            public void SetNukeDotCount(int value)
            {
                _accessor.Write(_offset + 3444456, value);
            }

            public Position GetNukeDots(int i)
            {
                return new Position(_accessor, _offset + 3444460 + 8 * 1 * i);
            }

            public GameType GetGameType()
            {
                return (GameType)_accessor.ReadInt32(_offset + 3446060);
            }

            public void SetGameType(GameType value)
            {
                _accessor.Write(_offset + 3446060, (int)value);
            }

            public Latency GetLatency()
            {
                return (Latency)_accessor.ReadInt32(_offset + 3446064);
            }

            public void SetLatency(Latency value)
            {
                _accessor.Write(_offset + 3446064, (int)value);
            }

            public int GetLatencyFrames()
            {
                return _accessor.ReadInt32(_offset + 3446068);
            }

            public void SetLatencyFrames(int value)
            {
                _accessor.Write(_offset + 3446068, value);
            }

            public int GetLatencyTime()
            {
                return _accessor.ReadInt32(_offset + 3446072);
            }

            public void SetLatencyTime(int value)
            {
                _accessor.Write(_offset + 3446072, value);
            }

            public int GetRemainingLatencyFrames()
            {
                return _accessor.ReadInt32(_offset + 3446076);
            }

            public void SetRemainingLatencyFrames(int value)
            {
                _accessor.Write(_offset + 3446076, value);
            }

            public int GetRemainingLatencyTime()
            {
                return _accessor.ReadInt32(_offset + 3446080);
            }

            public void SetRemainingLatencyTime(int value)
            {
                _accessor.Write(_offset + 3446080, value);
            }

            public bool GetHasLatCom()
            {
                return _accessor.ReadByte(_offset + 3446084) != 0;
            }

            public void SetHasLatCom(bool value)
            {
                _accessor.Write(_offset + 3446084, (byte)(value ? 1 : 0));
            }

            public bool GetHasGUI()
            {
                return _accessor.ReadByte(_offset + 3446085) != 0;
            }

            public void SetHasGUI(bool value)
            {
                _accessor.Write(_offset + 3446085, (byte)(value ? 1 : 0));
            }

            public int GetReplayFrameCount()
            {
                return _accessor.ReadInt32(_offset + 3446088);
            }

            public void SetReplayFrameCount(int value)
            {
                _accessor.Write(_offset + 3446088, value);
            }

            public int GetRandomSeed()
            {
                return _accessor.ReadInt32(_offset + 3446092);
            }

            public void SetRandomSeed(int value)
            {
                _accessor.Write(_offset + 3446092, value);
            }

            public int GetFrameCount()
            {
                return _accessor.ReadInt32(_offset + 3446096);
            }

            public void SetFrameCount(int value)
            {
                _accessor.Write(_offset + 3446096, value);
            }

            public int GetElapsedTime()
            {
                return _accessor.ReadInt32(_offset + 3446100);
            }

            public void SetElapsedTime(int value)
            {
                _accessor.Write(_offset + 3446100, value);
            }

            public int GetCountdownTimer()
            {
                return _accessor.ReadInt32(_offset + 3446104);
            }

            public void SetCountdownTimer(int value)
            {
                _accessor.Write(_offset + 3446104, value);
            }

            public int GetFps()
            {
                return _accessor.ReadInt32(_offset + 3446108);
            }

            public void SetFps(int value)
            {
                _accessor.Write(_offset + 3446108, value);
            }

            public double GetAverageFPS()
            {
                return _accessor.ReadDouble(_offset + 3446112);
            }

            public void SetAverageFPS(double value)
            {
                _accessor.Write(_offset + 3446112, value);
            }

            public int GetMouseX()
            {
                return _accessor.ReadInt32(_offset + 3446120);
            }

            public void SetMouseX(int value)
            {
                _accessor.Write(_offset + 3446120, value);
            }

            public int GetMouseY()
            {
                return _accessor.ReadInt32(_offset + 3446124);
            }

            public void SetMouseY(int value)
            {
                _accessor.Write(_offset + 3446124, value);
            }

            public bool GetMouseState(MouseButton mouseButton)
            {
                return _accessor.ReadByte(_offset + 3446128 + 1 * 1 * (int)mouseButton) != 0;
            }

            public void SetMouseState(MouseButton mouseButton, bool value)
            {
                _accessor.Write(_offset + 3446128 + 1 * 1 * (int)mouseButton, (byte)(value ? 1 : 0));
            }

            public bool GetKeyState(Key key)
            {
                return _accessor.ReadByte(_offset + 3446131 + 1 * 1 * (int)key) != 0;
            }

            public void SetKeyState(Key key, bool value)
            {
                _accessor.Write(_offset + 3446131 + 1 * 1 * (int)key, (byte)(value ? 1 : 0));
            }

            public int GetScreenX()
            {
                return _accessor.ReadInt32(_offset + 3446388);
            }

            public void SetScreenX(int value)
            {
                _accessor.Write(_offset + 3446388, value);
            }

            public int GetScreenY()
            {
                return _accessor.ReadInt32(_offset + 3446392);
            }

            public void SetScreenY(int value)
            {
                _accessor.Write(_offset + 3446392, value);
            }

            public bool GetFlags(Flag flag)
            {
                return _accessor.ReadByte(_offset + 3446396 + 1 * 1 * (int)flag) != 0;
            }

            public void SetFlags(Flag flag, bool value)
            {
                _accessor.Write(_offset + 3446396 + 1 * 1 * (int)flag, (byte)(value ? 1 : 0));
            }

            public int GetMapWidth()
            {
                return _accessor.ReadInt32(_offset + 3446400);
            }

            public void SetMapWidth(int value)
            {
                _accessor.Write(_offset + 3446400, value);
            }

            public int GetMapHeight()
            {
                return _accessor.ReadInt32(_offset + 3446404);
            }

            public void SetMapHeight(int value)
            {
                _accessor.Write(_offset + 3446404, value);
            }

            public string GetMapFileName()
            {
                return _accessor.ReadString(_offset + 3446408, 261);
            }

            public void SetMapFileName(string value)
            {
                _accessor.Write(_offset + 3446408, 261, value);
            }

            public string GetMapPathName()
            {
                return _accessor.ReadString(_offset + 3446669, 261);
            }

            public void SetMapPathName(string value)
            {
                _accessor.Write(_offset + 3446669, 261, value);
            }

            public string GetMapName()
            {
                return _accessor.ReadString(_offset + 3446930, 33);
            }

            public void SetMapName(string value)
            {
                _accessor.Write(_offset + 3446930, 33, value);
            }

            public string GetMapHash()
            {
                return _accessor.ReadString(_offset + 3446963, 41);
            }

            public void SetMapHash(string value)
            {
                _accessor.Write(_offset + 3446963, 41, value);
            }

            public int GetGroundHeight(int i, int j)
            {
                return _accessor.ReadInt32(_offset + 3447004 + 4 * 1 * j + 4 * 256 * i);
            }

            public void SetGetGroundHeight(int i, int j, int value)
            {
                _accessor.Write(_offset + 3447004 + 4 * 1 * j + 4 * 256 * i, value);
            }

            public bool IsWalkable(int i, int j)
            {
                return _accessor.ReadByte(_offset + 3709148 + 1 * 1 * j + 1 * 1024 * i) != 0;
            }

            public void SetIsWalkable(int i, int j, bool value)
            {
                _accessor.Write(_offset + 3709148 + 1 * 1 * j + 1 * 1024 * i, (byte)(value ? 1 : 0));
            }

            public bool IsBuildable(int i, int j)
            {
                return _accessor.ReadByte(_offset + 4757724 + 1 * 1 * j + 1 * 256 * i) != 0;
            }

            public void SetIsBuildable(int i, int j, bool value)
            {
                _accessor.Write(_offset + 4757724 + 1 * 1 * j + 1 * 256 * i, (byte)(value ? 1 : 0));
            }

            public bool IsVisible(int i, int j)
            {
                return _accessor.ReadByte(_offset + 4823260 + 1 * 1 * j + 1 * 256 * i) != 0;
            }

            public void SetIsVisible(int i, int j, bool value)
            {
                _accessor.Write(_offset + 4823260 + 1 * 1 * j + 1 * 256 * i, (byte)(value ? 1 : 0));
            }

            public bool IsExplored(int i, int j)
            {
                return _accessor.ReadByte(_offset + 4888796 + 1 * 1 * j + 1 * 256 * i) != 0;
            }

            public void SetIsExplored(int i, int j, bool value)
            {
                _accessor.Write(_offset + 4888796 + 1 * 1 * j + 1 * 256 * i, (byte)(value ? 1 : 0));
            }

            public bool GetHasCreep(int i, int j)
            {
                return _accessor.ReadByte(_offset + 4954332 + 1 * 1 * j + 1 * 256 * i) != 0;
            }

            public void SetHasCreep(int i, int j, bool value)
            {
                _accessor.Write(_offset + 4954332 + 1 * 1 * j + 1 * 256 * i, (byte)(value ? 1 : 0));
            }

            public bool IsOccupied(int i, int j)
            {
                return _accessor.ReadByte(_offset + 5019868 + 1 * 1 * j + 1 * 256 * i) != 0;
            }

            public void SetIsOccupied(int i, int j, bool value)
            {
                _accessor.Write(_offset + 5019868 + 1 * 1 * j + 1 * 256 * i, (byte)(value ? 1 : 0));
            }

            public short GetMapTileRegionId(int i, int j)
            {
                return _accessor.ReadInt16(_offset + 5085404 + 2 * 1 * j + 2 * 256 * i);
            }

            public void SetMapTileRegionId(int i, int j, short value)
            {
                _accessor.Write(_offset + 5085404 + 2 * 1 * j + 2 * 256 * i, value);
            }

            public short GetMapSplitTilesMiniTileMask(int i)
            {
                return _accessor.ReadInt16(_offset + 5216476 + 2 * 1 * i);
            }

            public void SetMapSplitTilesMiniTileMask(int i, short value)
            {
                _accessor.Write(_offset + 5216476 + 2 * 1 * i, value);
            }

            public short GetMapSplitTilesRegion1(int i)
            {
                return _accessor.ReadInt16(_offset + 5226476 + 2 * 1 * i);
            }

            public void SetMapSplitTilesRegion1(int i, short value)
            {
                _accessor.Write(_offset + 5226476 + 2 * 1 * i, value);
            }

            public short GetMapSplitTilesRegion2(int i)
            {
                return _accessor.ReadInt16(_offset + 5236476 + 2 * 1 * i);
            }

            public void SetMapSplitTilesRegion2(int i, short value)
            {
                _accessor.Write(_offset + 5236476 + 2 * 1 * i, value);
            }

            public int GetRegionCount()
            {
                return _accessor.ReadInt32(_offset + 5246476);
            }

            public void SetRegionCount(int value)
            {
                _accessor.Write(_offset + 5246476, value);
            }

            public RegionData GetRegions(int i)
            {
                return new RegionData(_accessor, _offset + 5246480 + 1068 * 1 * i);
            }

            public int GetStartLocationCount()
            {
                return _accessor.ReadInt32(_offset + 10586480);
            }

            public void SetStartLocationCount(int value)
            {
                _accessor.Write(_offset + 10586480, value);
            }

            public Position GetStartLocations(int i)
            {
                return new Position(_accessor, _offset + 10586484 + 8 * 1 * i);
            }

            public bool IsInGame()
            {
                return _accessor.ReadByte(_offset + 10586548) != 0;
            }

            public void SetIsInGame(bool value)
            {
                _accessor.Write(_offset + 10586548, (byte)(value ? 1 : 0));
            }

            public bool IsMultiplayer()
            {
                return _accessor.ReadByte(_offset + 10586549) != 0;
            }

            public void SetIsMultiplayer(bool value)
            {
                _accessor.Write(_offset + 10586549, (byte)(value ? 1 : 0));
            }

            public bool IsBattleNet()
            {
                return _accessor.ReadByte(_offset + 10586550) != 0;
            }

            public void SetIsBattleNet(bool value)
            {
                _accessor.Write(_offset + 10586550, (byte)(value ? 1 : 0));
            }

            public bool IsPaused()
            {
                return _accessor.ReadByte(_offset + 10586551) != 0;
            }

            public void SetIsPaused(bool value)
            {
                _accessor.Write(_offset + 10586551, (byte)(value ? 1 : 0));
            }

            public bool IsReplay()
            {
                return _accessor.ReadByte(_offset + 10586552) != 0;
            }

            public void SetIsReplay(bool value)
            {
                _accessor.Write(_offset + 10586552, (byte)(value ? 1 : 0));
            }

            public int GetSelectedUnitCount()
            {
                return _accessor.ReadInt32(_offset + 10586556);
            }

            public void SetSelectedUnitCount(int value)
            {
                _accessor.Write(_offset + 10586556, value);
            }

            public int GetSelectedUnits(int i)
            {
                return _accessor.ReadInt32(_offset + 10586560 + 4 * 1 * i);
            }

            public void SetSelectedUnits(int i, int value)
            {
                _accessor.Write(_offset + 10586560 + 4 * 1 * i, value);
            }

            public int GetSelf()
            {
                return _accessor.ReadInt32(_offset + 10586608);
            }

            public void SetSelf(int value)
            {
                _accessor.Write(_offset + 10586608, value);
            }

            public int GetEnemy()
            {
                return _accessor.ReadInt32(_offset + 10586612);
            }

            public void SetEnemy(int value)
            {
                _accessor.Write(_offset + 10586612, value);
            }

            public int GetNeutral()
            {
                return _accessor.ReadInt32(_offset + 10586616);
            }

            public void SetNeutral(int value)
            {
                _accessor.Write(_offset + 10586616, value);
            }

            public int GetEventCount()
            {
                return _accessor.ReadInt32(_offset + 10586620);
            }

            public void SetEventCount(int value)
            {
                _accessor.Write(_offset + 10586620, value);
            }

            public Event GetEvents(int i)
            {
                return new Event(_accessor, _offset + 10586624 + 12 * 1 * i);
            }

            public int GetEventStringCount()
            {
                return _accessor.ReadInt32(_offset + 10706624);
            }

            public void SetEventStringCount(int value)
            {
                _accessor.Write(_offset + 10706624, value);
            }

            public string GetEventStrings(int i)
            {
                return _accessor.ReadString(_offset + 10706628 + 1 * 256 * i, 256);
            }

            public void SetEventStrings(int i, string value)
            {
                _accessor.Write(_offset + 10706628 + 1 * 256 * i, 256, value);
            }

            public int GetStringCount()
            {
                return _accessor.ReadInt32(_offset + 10962628);
            }

            public void SetStringCount(int value)
            {
                _accessor.Write(_offset + 10962628, value);
            }

            public string GetStrings(int i)
            {
                return _accessor.ReadString(_offset + 10962632 + 1 * 1024 * i, 1024);
            }

            public void SetStrings(int i, string value)
            {
                _accessor.Write(_offset + 10962632 + 1 * 1024 * i, 1024, value);
            }

            public int GetShapeCount()
            {
                return _accessor.ReadInt32(_offset + 31442632);
            }

            public void SetShapeCount(int value)
            {
                _accessor.Write(_offset + 31442632, value);
            }

            public Shape GetShapes(int i)
            {
                return new Shape(_accessor, _offset + 31442636 + 40 * 1 * i);
            }

            public int GetCommandCount()
            {
                return _accessor.ReadInt32(_offset + 32242636);
            }

            public void SetCommandCount(int value)
            {
                _accessor.Write(_offset + 32242636, value);
            }

            public Command GetCommands(int i)
            {
                return new Command(_accessor, _offset + 32242640 + 12 * 1 * i);
            }

            public int GetUnitCommandCount()
            {
                return _accessor.ReadInt32(_offset + 32482640);
            }

            public void SetUnitCommandCount(int value)
            {
                _accessor.Write(_offset + 32482640, value);
            }

            public UnitCommand GetUnitCommands(int i)
            {
                return new UnitCommand(_accessor, _offset + 32482644 + 24 * 1 * i);
            }

            public int GetUnitSearchSize()
            {
                return _accessor.ReadInt32(_offset + 32962644);
            }

            public void SetUnitSearchSize(int value)
            {
                _accessor.Write(_offset + 32962644, value);
            }

            public UnitFinder GetXUnitSearch(int i)
            {
                return new UnitFinder(_accessor, _offset + 32962648 + 8 * 1 * i);
            }

            public UnitFinder GetYUnitSearch(int i)
            {
                return new UnitFinder(_accessor, _offset + 32989848 + 8 * 1 * i);
            }
        }

        public class UnitCommand
        {
            public const int Size = 24;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public UnitCommand(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public UnitCommandType GetTid()
            {
                return (UnitCommandType)_accessor.ReadInt32(_offset + 0);
            }

            public void SetTid(UnitCommandType type)
            {
                _accessor.Write(_offset + 0, (int)type);
            }

            public int GetUnitIndex()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetUnitIndex(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public int GetTargetIndex()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetTargetIndex(int value)
            {
                _accessor.Write(_offset + 8, value);
            }

            public int GetX()
            {
                return _accessor.ReadInt32(_offset + 12);
            }

            public void SetX(int value)
            {
                _accessor.Write(_offset + 12, value);
            }

            public int GetY()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetY(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetExtra()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetExtra(int value)
            {
                _accessor.Write(_offset + 20, value);
            }
        }

        public class Shape
        {
            public const int Size = 40;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public Shape(MemoryMappedViewAccessor accessor, int myOffset)
            {
                _accessor = accessor;
                _offset = myOffset;
            }

            public ShapeType GetShapeType()
            {
                return (ShapeType)_accessor.ReadInt32(_offset + 0);
            }

            public void SetShapeType(ShapeType value)
            {
                _accessor.Write(_offset + 0, (int)value);
            }

            public CoordinateType GetCtype()
            {
                return (CoordinateType)_accessor.ReadInt32(_offset + 4);
            }

            public void SetCtype(CoordinateType value)
            {
                _accessor.Write(_offset + 4, (int)value);
            }

            public int GetX1()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetX1(int value)
            {
                _accessor.Write(_offset + 8, value);
            }

            public int GetY1()
            {
                return _accessor.ReadInt32(_offset + 12);
            }

            public void SetY1(int value)
            {
                _accessor.Write(_offset + 12, value);
            }

            public int GetX2()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetX2(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetY2()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetY2(int value)
            {
                _accessor.Write(_offset + 20, value);
            }

            public int GetExtra1()
            {
                return _accessor.ReadInt32(_offset + 24);
            }

            public void SetExtra1(int value)
            {
                _accessor.Write(_offset + 24, value);
            }

            public int GetExtra2()
            {
                return _accessor.ReadInt32(_offset + 28);
            }

            public void SetExtra2(int value)
            {
                _accessor.Write(_offset + 28, value);
            }

            public int GetColor()
            {
                return _accessor.ReadInt32(_offset + 32);
            }

            public void SetColor(int value)
            {
                _accessor.Write(_offset + 32, value);
            }

            public bool IsSolid()
            {
                return _accessor.ReadByte(_offset + 36) != 0;
            }

            public void SetIsSolid(bool value)
            {
                _accessor.Write(_offset + 36, (byte)(value ? 1 : 0));
            }
        }

        public class Command
        {
            public const int Size = 12;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public Command(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public CommandType GetCommandType()
            {
                return (CommandType)_accessor.ReadInt32(_offset + 0);
            }

            public void SetCommandType(CommandType value)
            {
                _accessor.Write(_offset + 0, (int)value);
            }

            public int GetValue1()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetValue1(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public int GetValue2()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetValue2(int value)
            {
                _accessor.Write(_offset + 8, value);
            }
        }

        public class Position
        {
            public const int Size = 8;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public Position(MemoryMappedViewAccessor accessor, int myOffset)
            {
                _accessor = accessor;
                _offset = myOffset;
            }

            public int GetX()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetX(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int GetY()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetY(int value)
            {
                _accessor.Write(_offset + 4, value);
            }
        }

        public class Event
        {
            public const int Size = 12;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public Event(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public EventType GetEventType()
            {
                return (EventType)_accessor.ReadInt32(_offset + 0);
            }

            public void SetEventType(EventType value)
            {
                _accessor.Write(_offset + 0, (int)value);
            }

            public int GetV1()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetV1(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public int GetV2()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetV2(int value)
            {
                _accessor.Write(_offset + 8, value);
            }
        }

        public class RegionData
        {
            public const int Size = 1068;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public RegionData(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public int GetId()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetId(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int IslandID()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetIslandID(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public int GetCenter_x()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetCenter_x(int value)
            {
                _accessor.Write(_offset + 8, value);
            }

            public int GetCenter_y()
            {
                return _accessor.ReadInt32(_offset + 12);
            }

            public void SetCenter_y(int value)
            {
                _accessor.Write(_offset + 12, value);
            }

            public int GetPriority()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetPriority(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetLeftMost()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetLeftMost(int value)
            {
                _accessor.Write(_offset + 20, value);
            }

            public int GetRightMost()
            {
                return _accessor.ReadInt32(_offset + 24);
            }

            public void SetRightMost(int value)
            {
                _accessor.Write(_offset + 24, value);
            }

            public int GetTopMost()
            {
                return _accessor.ReadInt32(_offset + 28);
            }

            public void SetTopMost(int value)
            {
                _accessor.Write(_offset + 28, value);
            }

            public int GetBottomMost()
            {
                return _accessor.ReadInt32(_offset + 32);
            }

            public void SetBottomMost(int value)
            {
                _accessor.Write(_offset + 32, value);
            }

            public int GetNeighborCount()
            {
                return _accessor.ReadInt32(_offset + 36);
            }

            public void SetNeighborCount(int value)
            {
                _accessor.Write(_offset + 36, value);
            }

            public int GetNeighbors(int i)
            {
                return _accessor.ReadInt32(_offset + 40 + 4 * 1 * i);
            }

            public void SetNeighbors(int i, int value)
            {
                _accessor.Write(_offset + 40 + 4 * 1 * i, value);
            }

            public bool IsAccessible()
            {
                return _accessor.ReadByte(_offset + 1064) != 0;
            }

            public void SetIsAccessible(bool value)
            {
                _accessor.Write(_offset + 1064, (byte)(value ? 1 : 0));
            }

            public bool IsHigherGround()
            {
                return _accessor.ReadByte(_offset + 1065) != 0;
            }

            public void SetIsHigherGround(bool value)
            {
                _accessor.Write(_offset + 1065, (byte)(value ? 1 : 0));
            }
        }

        public class ForceData
        {
            public const int Size = 32;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public ForceData(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public string GetName()
            {
                return _accessor.ReadString(_offset + 0, 32);
            }

            public void SetName(string value)
            {
                _accessor.Write(_offset + 0, 32, value);
            }
        }

        public class PlayerData
        {
            public const int Size = 5788;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public PlayerData(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public string GetName()
            {
                return _accessor.ReadString(_offset + 0, 25);
            }

            public void SetName(string value)
            {
                _accessor.Write(_offset + 0, 25, value);
            }

            public Race GetRace()
            {
                return (Race)_accessor.ReadInt32(_offset + 28);
            }

            public void SetRace(Race value)
            {
                _accessor.Write(_offset + 28, (int)value);
            }

            public PlayerType GetPlayerType()
            {
                return (PlayerType)_accessor.ReadInt32(_offset + 32);
            }

            public void SetPlayerType(PlayerType value)
            {
                _accessor.Write(_offset + 32, (int)value);
            }

            public int GetForce()
            {
                return _accessor.ReadInt32(_offset + 36);
            }

            public void SetForce(int value)
            {
                _accessor.Write(_offset + 36, value);
            }

            public bool IsAlly(int i)
            {
                return _accessor.ReadByte(_offset + 40 + 1 * 1 * i) != 0;
            }

            public void SetIsAlly(int i, bool value)
            {
                _accessor.Write(_offset + 40 + 1 * 1 * i, (byte)(value ? 1 : 0));
            }

            public bool IsEnemy(int i)
            {
                return _accessor.ReadByte(_offset + 52 + 1 * 1 * i) != 0;
            }

            public void SetIsEnemy(int i, bool value)
            {
                _accessor.Write(_offset + 52 + 1 * 1 * i, (byte)(value ? 1 : 0));
            }

            public bool IsNeutral()
            {
                return _accessor.ReadByte(_offset + 64) != 0;
            }

            public void SetIsNeutral(bool value)
            {
                _accessor.Write(_offset + 64, (byte)(value ? 1 : 0));
            }

            public int GetStartLocationX()
            {
                return _accessor.ReadInt32(_offset + 68);
            }

            public void SetStartLocationX(int value)
            {
                _accessor.Write(_offset + 68, value);
            }

            public int GetStartLocationY()
            {
                return _accessor.ReadInt32(_offset + 72);
            }

            public void SetStartLocationY(int value)
            {
                _accessor.Write(_offset + 72, value);
            }

            public bool IsVictorious()
            {
                return _accessor.ReadByte(_offset + 76) != 0;
            }

            public void SetIsVictorious(bool value)
            {
                _accessor.Write(_offset + 76, (byte)(value ? 1 : 0));
            }

            public bool IsDefeated()
            {
                return _accessor.ReadByte(_offset + 77) != 0;
            }

            public void SetIsDefeated(bool value)
            {
                _accessor.Write(_offset + 77, (byte)(value ? 1 : 0));
            }

            public bool GetLeftGame()
            {
                return _accessor.ReadByte(_offset + 78) != 0;
            }

            public void SetLeftGame(bool value)
            {
                _accessor.Write(_offset + 78, (byte)(value ? 1 : 0));
            }

            public bool IsParticipating()
            {
                return _accessor.ReadByte(_offset + 79) != 0;
            }

            public void SetIsParticipating(bool value)
            {
                _accessor.Write(_offset + 79, (byte)(value ? 1 : 0));
            }

            public int GetMinerals()
            {
                return _accessor.ReadInt32(_offset + 80);
            }

            public void SetMinerals(int value)
            {
                _accessor.Write(_offset + 80, value);
            }

            public int GetGas()
            {
                return _accessor.ReadInt32(_offset + 84);
            }

            public void SetGas(int value)
            {
                _accessor.Write(_offset + 84, value);
            }

            public int GetGatheredMinerals()
            {
                return _accessor.ReadInt32(_offset + 88);
            }

            public void SetGatheredMinerals(int value)
            {
                _accessor.Write(_offset + 88, value);
            }

            public int GetGatheredGas()
            {
                return _accessor.ReadInt32(_offset + 92);
            }

            public void SetGatheredGas(int value)
            {
                _accessor.Write(_offset + 92, value);
            }

            public int GetRepairedMinerals()
            {
                return _accessor.ReadInt32(_offset + 96);
            }

            public void SetRepairedMinerals(int value)
            {
                _accessor.Write(_offset + 96, value);
            }

            public int GetRepairedGas()
            {
                return _accessor.ReadInt32(_offset + 100);
            }

            public void SetRepairedGas(int value)
            {
                _accessor.Write(_offset + 100, value);
            }

            public int GetRefundedMinerals()
            {
                return _accessor.ReadInt32(_offset + 104);
            }

            public void SetRefundedMinerals(int value)
            {
                _accessor.Write(_offset + 104, value);
            }

            public int GetRefundedGas()
            {
                return _accessor.ReadInt32(_offset + 108);
            }

            public void SetRefundedGas(int value)
            {
                _accessor.Write(_offset + 108, value);
            }

            public int GetSupplyTotal(Race race)
            {
                return _accessor.ReadInt32(_offset + 112 + 4 * 1 * (int)race);
            }

            public void SetSupplyTotal(Race race, int value)
            {
                _accessor.Write(_offset + 112 + 4 * 1 * (int)race, value);
            }

            public int GetSupplyUsed(Race race)
            {
                return _accessor.ReadInt32(_offset + 124 + 4 * 1 * (int)race);
            }

            public void SetSupplyUsed(Race race, int value)
            {
                _accessor.Write(_offset + 124 + 4 * 1 * (int)race, value);
            }

            public int GetAllUnitCount(UnitType unitType)
            {
                return _accessor.ReadInt32(_offset + 136 + 4 * 1 * (int)unitType);
            }

            public void SetAllUnitCount(UnitType unitType, int value)
            {
                _accessor.Write(_offset + 136 + 4 * 1 * (int)unitType, value);
            }

            public int GetVisibleUnitCount(UnitType unitType)
            {
                return _accessor.ReadInt32(_offset + 1072 + 4 * 1 * (int)unitType);
            }

            public void SetVisibleUnitCount(UnitType unitType, int value)
            {
                _accessor.Write(_offset + 1072 + 4 * 1 * (int)unitType, value);
            }

            public int GetCompletedUnitCount(UnitType unitType)
            {
                return _accessor.ReadInt32(_offset + 2008 + 4 * 1 * (int)unitType);
            }

            public void SetCompletedUnitCount(UnitType unitType, int value)
            {
                _accessor.Write(_offset + 2008 + 4 * 1 * (int)unitType, value);
            }

            public int GetDeadUnitCount(UnitType unitType)
            {
                return _accessor.ReadInt32(_offset + 2944 + 4 * 1 * (int)unitType);
            }

            public void SetDeadUnitCount(UnitType unitType, int value)
            {
                _accessor.Write(_offset + 2944 + 4 * 1 * (int)unitType, value);
            }

            public int GetKilledUnitCount(UnitType unitType)
            {
                return _accessor.ReadInt32(_offset + 3880 + 4 * 1 * (int)unitType);
            }

            public void SetKilledUnitCount(UnitType unitType, int value)
            {
                _accessor.Write(_offset + 3880 + 4 * 1 * (int)unitType, value);
            }

            public int GetUpgradeLevel(UpgradeType upgradeType)
            {
                return _accessor.ReadInt32(_offset + 4816 + 4 * 1 * (int)upgradeType);
            }

            public void SetUpgradeLevel(UpgradeType upgradeType, int value)
            {
                _accessor.Write(_offset + 4816 + 4 * 1 * (int)upgradeType, value);
            }

            public bool GetHasResearched(TechType techType)
            {
                return _accessor.ReadByte(_offset + 5068 + 1 * 1 * (int)techType) != 0;
            }

            public void SetHasResearched(TechType techType, bool value)
            {
                _accessor.Write(_offset + 5068 + 1 * 1 * (int)techType, (byte)(value ? 1 : 0));
            }

            public bool IsResearching(TechType techType)
            {
                return _accessor.ReadByte(_offset + 5115 + 1 * 1 * (int)techType) != 0;
            }

            public void SetIsResearching(TechType techType, bool value)
            {
                _accessor.Write(_offset + 5115 + 1 * 1 * (int)techType, (byte)(value ? 1 : 0));
            }

            public bool IsUpgrading(UpgradeType upgradeType)
            {
                return _accessor.ReadByte(_offset + 5162 + 1 * 1 * (int)upgradeType) != 0;
            }

            public void SetIsUpgrading(UpgradeType upgradeType, bool value)
            {
                _accessor.Write(_offset + 5162 + 1 * 1 * (int)upgradeType, (byte)(value ? 1 : 0));
            }

            public int GetColor()
            {
                return _accessor.ReadInt32(_offset + 5228);
            }

            public void SetColor(int value)
            {
                _accessor.Write(_offset + 5228, value);
            }

            public int GetTotalUnitScore()
            {
                return _accessor.ReadInt32(_offset + 5232);
            }

            public void SetTotalUnitScore(int value)
            {
                _accessor.Write(_offset + 5232, value);
            }

            public int GetTotalKillScore()
            {
                return _accessor.ReadInt32(_offset + 5236);
            }

            public void SetTotalKillScore(int value)
            {
                _accessor.Write(_offset + 5236, value);
            }

            public int GetTotalBuildingScore()
            {
                return _accessor.ReadInt32(_offset + 5240);
            }

            public void SetTotalBuildingScore(int value)
            {
                _accessor.Write(_offset + 5240, value);
            }

            public int GetTotalRazingScore()
            {
                return _accessor.ReadInt32(_offset + 5244);
            }

            public void SetTotalRazingScore(int value)
            {
                _accessor.Write(_offset + 5244, value);
            }

            public int GetCustomScore()
            {
                return _accessor.ReadInt32(_offset + 5248);
            }

            public void SetCustomScore(int value)
            {
                _accessor.Write(_offset + 5248, value);
            }

            public int GetMaxUpgradeLevel(UpgradeType upgradeType)
            {
                return _accessor.ReadInt32(_offset + 5252 + 4 * 1 * (int)upgradeType);
            }

            public void SetMaxUpgradeLevel(UpgradeType upgradeType, int value)
            {
                _accessor.Write(_offset + 5252 + 4 * 1 * (int)upgradeType, value);
            }

            public bool IsResearchAvailable(TechType techType)
            {
                return _accessor.ReadByte(_offset + 5504 + 1 * 1 * (int)techType) != 0;
            }

            public void SetIsResearchAvailable(TechType techType, bool value)
            {
                _accessor.Write(_offset + 5504 + 1 * 1 * (int)techType, (byte)(value ? 1 : 0));
            }

            public bool IsUnitAvailable(UnitType unitType)
            {
                return _accessor.ReadByte(_offset + 5551 + 1 * 1 * (int)unitType) != 0;
            }

            public void SetIsUnitAvailable(UnitType unitType, bool value)
            {
                _accessor.Write(_offset + 5551 + 1 * 1 * (int)unitType, (byte)(value ? 1 : 0));
            }
        }

        public class BulletData
        {
            public const int Size = 80;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public BulletData(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public int GetId()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetId(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int GetPlayer()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetPlayer(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public BulletType GetBulletType()
            {
                return (BulletType)_accessor.ReadInt32(_offset + 8);
            }

            public void SetBulletType(BulletType value)
            {
                _accessor.Write(_offset + 8, (int)value);
            }

            public int GetSource()
            {
                return _accessor.ReadInt32(_offset + 12);
            }

            public void SetSource(int value)
            {
                _accessor.Write(_offset + 12, value);
            }

            public int GetPositionX()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetPositionX(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetPositionY()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetPositionY(int value)
            {
                _accessor.Write(_offset + 20, value);
            }

            public double GetAngle()
            {
                return _accessor.ReadDouble(_offset + 24);
            }

            public void SetAngle(double value)
            {
                _accessor.Write(_offset + 24, value);
            }

            public double GetVelocityX()
            {
                return _accessor.ReadDouble(_offset + 32);
            }

            public void SetVelocityX(double value)
            {
                _accessor.Write(_offset + 32, value);
            }

            public double GetVelocityY()
            {
                return _accessor.ReadDouble(_offset + 40);
            }

            public void SetVelocityY(double value)
            {
                _accessor.Write(_offset + 40, value);
            }

            public int GetTarget()
            {
                return _accessor.ReadInt32(_offset + 48);
            }

            public void SetTarget(int value)
            {
                _accessor.Write(_offset + 48, value);
            }

            public int GetTargetPositionX()
            {
                return _accessor.ReadInt32(_offset + 52);
            }

            public void SetTargetPositionX(int value)
            {
                _accessor.Write(_offset + 52, value);
            }

            public int GetTargetPositionY()
            {
                return _accessor.ReadInt32(_offset + 56);
            }

            public void SetTargetPositionY(int value)
            {
                _accessor.Write(_offset + 56, value);
            }

            public int GetRemoveTimer()
            {
                return _accessor.ReadInt32(_offset + 60);
            }

            public void SetRemoveTimer(int value)
            {
                _accessor.Write(_offset + 60, value);
            }

            public bool GetExists()
            {
                return _accessor.ReadByte(_offset + 64) != 0;
            }

            public void SetExists(bool value)
            {
                _accessor.Write(_offset + 64, (byte)(value ? 1 : 0));
            }

            public bool IsVisible(int i)
            {
                return _accessor.ReadByte(_offset + 65 + 1 * 1 * i) != 0;
            }

            public void SetIsVisible(int i, bool value)
            {
                _accessor.Write(_offset + 65 + 1 * 1 * i, (byte)(value ? 1 : 0));
            }
        }

        public class UnitFinder
        {
            public const int Size = 8;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public UnitFinder(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public int GetUnitIndex()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetUnitIndex(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int GetSearchValue()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetSearchValue(int value)
            {
                _accessor.Write(_offset + 4, value);
            }
        }

        public class UnitData
        {
            public const int Size = 336;

            private readonly MemoryMappedViewAccessor _accessor;
            private readonly int _offset;

            public UnitData(MemoryMappedViewAccessor accessor, int offset)
            {
                _accessor = accessor;
                _offset = offset;
            }

            public int GetClearanceLevel()
            {
                return _accessor.ReadInt32(_offset + 0);
            }

            public void SetClearanceLevel(int value)
            {
                _accessor.Write(_offset + 0, value);
            }

            public int GetId()
            {
                return _accessor.ReadInt32(_offset + 4);
            }

            public void SetId(int value)
            {
                _accessor.Write(_offset + 4, value);
            }

            public int GetPlayer()
            {
                return _accessor.ReadInt32(_offset + 8);
            }

            public void SetPlayer(int value)
            {
                _accessor.Write(_offset + 8, value);
            }

            public UnitType GetUnitType()
            {
                return (UnitType)_accessor.ReadInt32(_offset + 12);
            }

            public void SetUnitType(UnitType value)
            {
                _accessor.Write(_offset + 12, (int)value);
            }

            public int GetPositionX()
            {
                return _accessor.ReadInt32(_offset + 16);
            }

            public void SetPositionX(int value)
            {
                _accessor.Write(_offset + 16, value);
            }

            public int GetPositionY()
            {
                return _accessor.ReadInt32(_offset + 20);
            }

            public void SetPositionY(int value)
            {
                _accessor.Write(_offset + 20, value);
            }

            public double GetAngle()
            {
                return _accessor.ReadDouble(_offset + 24);
            }

            public void SetAngle(double value)
            {
                _accessor.Write(_offset + 24, value);
            }

            public double GetVelocityX()
            {
                return _accessor.ReadDouble(_offset + 32);
            }

            public void SetVelocityX(double value)
            {
                _accessor.Write(_offset + 32, value);
            }

            public double GetVelocityY()
            {
                return _accessor.ReadDouble(_offset + 40);
            }

            public void SetVelocityY(double value)
            {
                _accessor.Write(_offset + 40, value);
            }

            public int GetHitPoints()
            {
                return _accessor.ReadInt32(_offset + 48);
            }

            public void SetHitPoints(int value)
            {
                _accessor.Write(_offset + 48, value);
            }

            public int GetLastHitPoints()
            {
                return _accessor.ReadInt32(_offset + 52);
            }

            public void SetLastHitPoints(int value)
            {
                _accessor.Write(_offset + 52, value);
            }

            public int GetShields()
            {
                return _accessor.ReadInt32(_offset + 56);
            }

            public void SetShields(int value)
            {
                _accessor.Write(_offset + 56, value);
            }

            public int GetEnergy()
            {
                return _accessor.ReadInt32(_offset + 60);
            }

            public void SetEnergy(int value)
            {
                _accessor.Write(_offset + 60, value);
            }

            public int GetResources()
            {
                return _accessor.ReadInt32(_offset + 64);
            }

            public void SetResources(int value)
            {
                _accessor.Write(_offset + 64, value);
            }

            public int GetResourceGroup()
            {
                return _accessor.ReadInt32(_offset + 68);
            }

            public void SetResourceGroup(int value)
            {
                _accessor.Write(_offset + 68, value);
            }

            public int GetKillCount()
            {
                return _accessor.ReadInt32(_offset + 72);
            }

            public void SetKillCount(int value)
            {
                _accessor.Write(_offset + 72, value);
            }

            public int GetAcidSporeCount()
            {
                return _accessor.ReadInt32(_offset + 76);
            }

            public void SetAcidSporeCount(int value)
            {
                _accessor.Write(_offset + 76, value);
            }

            public int GetScarabCount()
            {
                return _accessor.ReadInt32(_offset + 80);
            }

            public void SetScarabCount(int value)
            {
                _accessor.Write(_offset + 80, value);
            }

            public int GetInterceptorCount()
            {
                return _accessor.ReadInt32(_offset + 84);
            }

            public void SetInterceptorCount(int value)
            {
                _accessor.Write(_offset + 84, value);
            }

            public int GetSpiderMineCount()
            {
                return _accessor.ReadInt32(_offset + 88);
            }

            public void SetSpiderMineCount(int value)
            {
                _accessor.Write(_offset + 88, value);
            }

            public int GetGroundWeaponCooldown()
            {
                return _accessor.ReadInt32(_offset + 92);
            }

            public void SetGroundWeaponCooldown(int value)
            {
                _accessor.Write(_offset + 92, value);
            }

            public int GetAirWeaponCooldown()
            {
                return _accessor.ReadInt32(_offset + 96);
            }

            public void SetAirWeaponCooldown(int value)
            {
                _accessor.Write(_offset + 96, value);
            }

            public int GetSpellCooldown()
            {
                return _accessor.ReadInt32(_offset + 100);
            }

            public void SetSpellCooldown(int value)
            {
                _accessor.Write(_offset + 100, value);
            }

            public int GetDefenseMatrixPoints()
            {
                return _accessor.ReadInt32(_offset + 104);
            }

            public void SetDefenseMatrixPoints(int value)
            {
                _accessor.Write(_offset + 104, value);
            }

            public int GetDefenseMatrixTimer()
            {
                return _accessor.ReadInt32(_offset + 108);
            }

            public void SetDefenseMatrixTimer(int value)
            {
                _accessor.Write(_offset + 108, value);
            }

            public int GetEnsnareTimer()
            {
                return _accessor.ReadInt32(_offset + 112);
            }

            public void SetEnsnareTimer(int value)
            {
                _accessor.Write(_offset + 112, value);
            }

            public int GetIrradiateTimer()
            {
                return _accessor.ReadInt32(_offset + 116);
            }

            public void SetIrradiateTimer(int value)
            {
                _accessor.Write(_offset + 116, value);
            }

            public int GetLockdownTimer()
            {
                return _accessor.ReadInt32(_offset + 120);
            }

            public void SetLockdownTimer(int value)
            {
                _accessor.Write(_offset + 120, value);
            }

            public int GetMaelstromTimer()
            {
                return _accessor.ReadInt32(_offset + 124);
            }

            public void SetMaelstromTimer(int value)
            {
                _accessor.Write(_offset + 124, value);
            }

            public int GetOrderTimer()
            {
                return _accessor.ReadInt32(_offset + 128);
            }

            public void SetOrderTimer(int value)
            {
                _accessor.Write(_offset + 128, value);
            }

            public int GetPlagueTimer()
            {
                return _accessor.ReadInt32(_offset + 132);
            }

            public void SetPlagueTimer(int value)
            {
                _accessor.Write(_offset + 132, value);
            }

            public int GetRemoveTimer()
            {
                return _accessor.ReadInt32(_offset + 136);
            }

            public void SetRemoveTimer(int value)
            {
                _accessor.Write(_offset + 136, value);
            }

            public int GetStasisTimer()
            {
                return _accessor.ReadInt32(_offset + 140);
            }

            public void SetStasisTimer(int value)
            {
                _accessor.Write(_offset + 140, value);
            }

            public int GetStimTimer()
            {
                return _accessor.ReadInt32(_offset + 144);
            }

            public void SetStimTimer(int value)
            {
                _accessor.Write(_offset + 144, value);
            }

            public UnitType GetBuildType()
            {
                return (UnitType)_accessor.ReadInt32(_offset + 148);
            }

            public void SetBuildType(UnitType value)
            {
                _accessor.Write(_offset + 148, (int)value);
            }

            public int GetTrainingQueueCount()
            {
                return _accessor.ReadInt32(_offset + 152);
            }

            public void SetTrainingQueueCount(int value)
            {
                _accessor.Write(_offset + 152, value);
            }

            public int GetTrainingQueue(int i)
            {
                return _accessor.ReadInt32(_offset + 156 + 4 * 1 * i);
            }

            public void SetTrainingQueue(int i, int value)
            {
                _accessor.Write(_offset + 156 + 4 * 1 * i, value);
            }

            public int GetTech()
            {
                return _accessor.ReadInt32(_offset + 176);
            }

            public void SetTech(int value)
            {
                _accessor.Write(_offset + 176, value);
            }

            public int GetUpgrade()
            {
                return _accessor.ReadInt32(_offset + 180);
            }

            public void SetUpgrade(int value)
            {
                _accessor.Write(_offset + 180, value);
            }

            public int GetRemainingBuildTime()
            {
                return _accessor.ReadInt32(_offset + 184);
            }

            public void SetRemainingBuildTime(int value)
            {
                _accessor.Write(_offset + 184, value);
            }

            public int GetRemainingTrainTime()
            {
                return _accessor.ReadInt32(_offset + 188);
            }

            public void SetRemainingTrainTime(int value)
            {
                _accessor.Write(_offset + 188, value);
            }

            public int GetRemainingResearchTime()
            {
                return _accessor.ReadInt32(_offset + 192);
            }

            public void SetRemainingResearchTime(int value)
            {
                _accessor.Write(_offset + 192, value);
            }

            public int GetRemainingUpgradeTime()
            {
                return _accessor.ReadInt32(_offset + 196);
            }

            public void SetRemainingUpgradeTime(int value)
            {
                _accessor.Write(_offset + 196, value);
            }

            public int GetBuildUnit()
            {
                return _accessor.ReadInt32(_offset + 200);
            }

            public void SetBuildUnit(int value)
            {
                _accessor.Write(_offset + 200, value);
            }

            public int GetTarget()
            {
                return _accessor.ReadInt32(_offset + 204);
            }

            public void SetTarget(int value)
            {
                _accessor.Write(_offset + 204, value);
            }

            public int GetTargetPositionX()
            {
                return _accessor.ReadInt32(_offset + 208);
            }

            public void SetTargetPositionX(int value)
            {
                _accessor.Write(_offset + 208, value);
            }

            public int GetTargetPositionY()
            {
                return _accessor.ReadInt32(_offset + 212);
            }

            public void SetTargetPositionY(int value)
            {
                _accessor.Write(_offset + 212, value);
            }

            public int GetOrder()
            {
                return _accessor.ReadInt32(_offset + 216);
            }

            public void SetOrder(int value)
            {
                _accessor.Write(_offset + 216, value);
            }

            public int GetOrderTarget()
            {
                return _accessor.ReadInt32(_offset + 220);
            }

            public void SetOrderTarget(int value)
            {
                _accessor.Write(_offset + 220, value);
            }

            public int GetOrderTargetPositionX()
            {
                return _accessor.ReadInt32(_offset + 224);
            }

            public void SetOrderTargetPositionX(int value)
            {
                _accessor.Write(_offset + 224, value);
            }

            public int GetOrderTargetPositionY()
            {
                return _accessor.ReadInt32(_offset + 228);
            }

            public void SetOrderTargetPositionY(int value)
            {
                _accessor.Write(_offset + 228, value);
            }

            public int GetSecondaryOrder()
            {
                return _accessor.ReadInt32(_offset + 232);
            }

            public void SetSecondaryOrder(int value)
            {
                _accessor.Write(_offset + 232, value);
            }

            public int GetRallyPositionX()
            {
                return _accessor.ReadInt32(_offset + 236);
            }

            public void SetRallyPositionX(int value)
            {
                _accessor.Write(_offset + 236, value);
            }

            public int GetRallyPositionY()
            {
                return _accessor.ReadInt32(_offset + 240);
            }

            public void SetRallyPositionY(int value)
            {
                _accessor.Write(_offset + 240, value);
            }

            public int GetRallyUnit()
            {
                return _accessor.ReadInt32(_offset + 244);
            }

            public void SetRallyUnit(int value)
            {
                _accessor.Write(_offset + 244, value);
            }

            public int GetAddon()
            {
                return _accessor.ReadInt32(_offset + 248);
            }

            public void SetAddon(int value)
            {
                _accessor.Write(_offset + 248, value);
            }

            public int GetNydusExit()
            {
                return _accessor.ReadInt32(_offset + 252);
            }

            public void SetNydusExit(int value)
            {
                _accessor.Write(_offset + 252, value);
            }

            public int GetPowerUp()
            {
                return _accessor.ReadInt32(_offset + 256);
            }

            public void SetPowerUp(int value)
            {
                _accessor.Write(_offset + 256, value);
            }

            public int GetTransport()
            {
                return _accessor.ReadInt32(_offset + 260);
            }

            public void SetTransport(int value)
            {
                _accessor.Write(_offset + 260, value);
            }

            public int GetCarrier()
            {
                return _accessor.ReadInt32(_offset + 264);
            }

            public void SetCarrier(int value)
            {
                _accessor.Write(_offset + 264, value);
            }

            public int GetHatchery()
            {
                return _accessor.ReadInt32(_offset + 268);
            }

            public void SetHatchery(int value)
            {
                _accessor.Write(_offset + 268, value);
            }

            public bool GetExists()
            {
                return _accessor.ReadByte(_offset + 272) != 0;
            }

            public void SetExists(bool value)
            {
                _accessor.Write(_offset + 272, (byte)(value ? 1 : 0));
            }

            public bool GetHasNuke()
            {
                return _accessor.ReadByte(_offset + 273) != 0;
            }

            public void SetHasNuke(bool value)
            {
                _accessor.Write(_offset + 273, (byte)(value ? 1 : 0));
            }

            public bool IsAccelerating()
            {
                return _accessor.ReadByte(_offset + 274) != 0;
            }

            public void SetIsAccelerating(bool value)
            {
                _accessor.Write(_offset + 274, (byte)(value ? 1 : 0));
            }

            public bool IsAttacking()
            {
                return _accessor.ReadByte(_offset + 275) != 0;
            }

            public void SetIsAttacking(bool value)
            {
                _accessor.Write(_offset + 275, (byte)(value ? 1 : 0));
            }

            public bool IsAttackFrame()
            {
                return _accessor.ReadByte(_offset + 276) != 0;
            }

            public void SetIsAttackFrame(bool value)
            {
                _accessor.Write(_offset + 276, (byte)(value ? 1 : 0));
            }

            public bool IsBeingGathered()
            {
                return _accessor.ReadByte(_offset + 277) != 0;
            }

            public void SetIsBeingGathered(bool value)
            {
                _accessor.Write(_offset + 277, (byte)(value ? 1 : 0));
            }

            public bool IsBlind()
            {
                return _accessor.ReadByte(_offset + 278) != 0;
            }

            public void SetIsBlind(bool value)
            {
                _accessor.Write(_offset + 278, (byte)(value ? 1 : 0));
            }

            public bool IsBraking()
            {
                return _accessor.ReadByte(_offset + 279) != 0;
            }

            public void SetIsBraking(bool value)
            {
                _accessor.Write(_offset + 279, (byte)(value ? 1 : 0));
            }

            public bool IsBurrowed()
            {
                return _accessor.ReadByte(_offset + 280) != 0;
            }

            public void SetIsBurrowed(bool value)
            {
                _accessor.Write(_offset + 280, (byte)(value ? 1 : 0));
            }

            public int GetCarryResourceType()
            {
                return _accessor.ReadInt32(_offset + 284);
            }

            public void SetCarryResourceType(int value)
            {
                _accessor.Write(_offset + 284, value);
            }

            public bool IsCloaked()
            {
                return _accessor.ReadByte(_offset + 288) != 0;
            }

            public void SetIsCloaked(bool value)
            {
                _accessor.Write(_offset + 288, (byte)(value ? 1 : 0));
            }

            public bool IsCompleted()
            {
                return _accessor.ReadByte(_offset + 289) != 0;
            }

            public void SetIsCompleted(bool value)
            {
                _accessor.Write(_offset + 289, (byte)(value ? 1 : 0));
            }

            public bool IsConstructing()
            {
                return _accessor.ReadByte(_offset + 290) != 0;
            }

            public void SetIsConstructing(bool value)
            {
                _accessor.Write(_offset + 290, (byte)(value ? 1 : 0));
            }

            public bool IsDetected()
            {
                return _accessor.ReadByte(_offset + 291) != 0;
            }

            public void SetIsDetected(bool value)
            {
                _accessor.Write(_offset + 291, (byte)(value ? 1 : 0));
            }

            public bool IsGathering()
            {
                return _accessor.ReadByte(_offset + 292) != 0;
            }

            public void SetIsGathering(bool value)
            {
                _accessor.Write(_offset + 292, (byte)(value ? 1 : 0));
            }

            public bool IsHallucination()
            {
                return _accessor.ReadByte(_offset + 293) != 0;
            }

            public void SetIsHallucination(bool value)
            {
                _accessor.Write(_offset + 293, (byte)(value ? 1 : 0));
            }

            public bool IsIdle()
            {
                return _accessor.ReadByte(_offset + 294) != 0;
            }

            public void SetIsIdle(bool value)
            {
                _accessor.Write(_offset + 294, (byte)(value ? 1 : 0));
            }

            public bool IsInterruptible()
            {
                return _accessor.ReadByte(_offset + 295) != 0;
            }

            public void SetIsInterruptible(bool value)
            {
                _accessor.Write(_offset + 295, (byte)(value ? 1 : 0));
            }

            public bool IsInvincible()
            {
                return _accessor.ReadByte(_offset + 296) != 0;
            }

            public void SetIsInvincible(bool value)
            {
                _accessor.Write(_offset + 296, (byte)(value ? 1 : 0));
            }

            public bool IsLifted()
            {
                return _accessor.ReadByte(_offset + 297) != 0;
            }

            public void SetIsLifted(bool value)
            {
                _accessor.Write(_offset + 297, (byte)(value ? 1 : 0));
            }

            public bool IsMorphing()
            {
                return _accessor.ReadByte(_offset + 298) != 0;
            }

            public void SetIsMorphing(bool value)
            {
                _accessor.Write(_offset + 298, (byte)(value ? 1 : 0));
            }

            public bool IsMoving()
            {
                return _accessor.ReadByte(_offset + 299) != 0;
            }

            public void SetIsMoving(bool value)
            {
                _accessor.Write(_offset + 299, (byte)(value ? 1 : 0));
            }

            public bool IsParasited()
            {
                return _accessor.ReadByte(_offset + 300) != 0;
            }

            public void SetIsParasited(bool value)
            {
                _accessor.Write(_offset + 300, (byte)(value ? 1 : 0));
            }

            public bool IsSelected()
            {
                return _accessor.ReadByte(_offset + 301) != 0;
            }

            public void SetIsSelected(bool value)
            {
                _accessor.Write(_offset + 301, (byte)(value ? 1 : 0));
            }

            public bool IsStartingAttack()
            {
                return _accessor.ReadByte(_offset + 302) != 0;
            }

            public void SetIsStartingAttack(bool value)
            {
                _accessor.Write(_offset + 302, (byte)(value ? 1 : 0));
            }

            public bool IsStuck()
            {
                return _accessor.ReadByte(_offset + 303) != 0;
            }

            public void SetIsStuck(bool value)
            {
                _accessor.Write(_offset + 303, (byte)(value ? 1 : 0));
            }

            public bool IsTraining()
            {
                return _accessor.ReadByte(_offset + 304) != 0;
            }

            public void SetIsTraining(bool value)
            {
                _accessor.Write(_offset + 304, (byte)(value ? 1 : 0));
            }

            public bool IsUnderStorm()
            {
                return _accessor.ReadByte(_offset + 305) != 0;
            }

            public void SetIsUnderStorm(bool value)
            {
                _accessor.Write(_offset + 305, (byte)(value ? 1 : 0));
            }

            public bool IsUnderDarkSwarm()
            {
                return _accessor.ReadByte(_offset + 306) != 0;
            }

            public void SetIsUnderDarkSwarm(bool value)
            {
                _accessor.Write(_offset + 306, (byte)(value ? 1 : 0));
            }

            public bool IsUnderDWeb()
            {
                return _accessor.ReadByte(_offset + 307) != 0;
            }

            public void SetIsUnderDWeb(bool value)
            {
                _accessor.Write(_offset + 307, (byte)(value ? 1 : 0));
            }

            public bool IsPowered()
            {
                return _accessor.ReadByte(_offset + 308) != 0;
            }

            public void SetIsPowered(bool value)
            {
                _accessor.Write(_offset + 308, (byte)(value ? 1 : 0));
            }

            public bool IsVisible(int i)
            {
                return _accessor.ReadByte(_offset + 309 + 1 * 1 * i) != 0;
            }

            public void SetIsVisible(int i, bool value)
            {
                _accessor.Write(_offset + 309 + 1 * 1 * i, (byte)(value ? 1 : 0));
            }

            public int GetButtonset()
            {
                return _accessor.ReadInt32(_offset + 320);
            }

            public void SetButtonset(int value)
            {
                _accessor.Write(_offset + 320, value);
            }

            public int GetLastAttackerPlayer()
            {
                return _accessor.ReadInt32(_offset + 324);
            }

            public void SetLastAttackerPlayer(int value)
            {
                _accessor.Write(_offset + 324, value);
            }

            public bool GetRecentlyAttacked()
            {
                return _accessor.ReadByte(_offset + 328) != 0;
            }

            public void SetRecentlyAttacked(bool value)
            {
                _accessor.Write(_offset + 328, (byte)(value ? 1 : 0));
            }

            public int GetReplayID()
            {
                return _accessor.ReadInt32(_offset + 332);
            }

            public void SetReplayID(int value)
            {
                _accessor.Write(_offset + 332, value);
            }
        }
    }
}