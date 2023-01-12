using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    public readonly struct GameTable
    {
        public const int MaxGameInstances = 8;
        public const int Size = MaxGameInstances * GameInstance.Size;

        public readonly GameInstance[] GameInstances;

        public GameTable(MemoryMappedViewAccessor gameTableViewAccessor)
        {
            GameInstances = new GameInstance[MaxGameInstances];
            for (var i = 0; i < MaxGameInstances; i++)
            {
                var serverProcessID = gameTableViewAccessor.ReadInt32(GameInstance.Size * i);
                var isConnected = gameTableViewAccessor.ReadByte(GameInstance.Size * i + 4) != 0;
                var lastKeepAliveTime = gameTableViewAccessor.ReadInt32(GameInstance.Size * i + 4 + 4);
                GameInstances[i] = new GameInstance(serverProcessID, isConnected, lastKeepAliveTime);
            }
        }
    }

    public readonly struct GameInstance
    {
        public const int Size = 4 + 4 + 4;

        public readonly int ServerProcessID;
        public readonly bool IsConnected;
        public readonly int LastKeepAliveTime;

        public GameInstance(int serverProcessID, bool isConnected, int lastKeepAliveTime)
        {
            ServerProcessID = serverProcessID;
            IsConnected = isConnected;
            LastKeepAliveTime = lastKeepAliveTime;
        }
    }
}