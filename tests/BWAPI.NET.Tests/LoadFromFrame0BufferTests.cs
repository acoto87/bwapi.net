using System.IO.Compression;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET.Tests;

public class LoadFromFrame0BufferTests
{
    private const string ResourcesFolder = "Resources";
    private const string TestFileName = "(2)Breaking Point.scx_frame0_buffer.bin";

    [Fact]
    public void LoadFromFrame0Buffer()
    {
        using var mmf = GetMemoryMappedFileForMap(Path.Combine(ResourcesFolder, TestFileName));
        using var gameViewAccessor = mmf.CreateViewAccessor(0, ClientData.GameData_.Size, MemoryMappedFileAccess.ReadWrite);
        var clientData = new ClientData(gameViewAccessor);
        var game = new Game(clientData);
        game.Init();

        Assert.True(game.Self().GetPlayerType() == PlayerType.Player);
        Assert.True(game.Self().Minerals() == 50);
    }

    private static MemoryMappedFile GetMemoryMappedFileForMap(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
        var mmf = MemoryMappedFile.CreateNew(null, ClientData.GameData_.Size, MemoryMappedFileAccess.ReadWrite);
        using var gameViewStream = mmf.CreateViewStream(0, ClientData.GameData_.Size);
        deflateStream.CopyTo(gameViewStream);
        return mmf;
    }
}