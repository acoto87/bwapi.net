using System.IO.Compression;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET.Tests;

public class LoadFromFrame0BufferTests
{
    private const string ResourcesFolder = "Resources";

    [Theory]
    [InlineData("(2)Astral Balance.scm_frame0_buffer.bin")]
    [InlineData("(2)Breaking Point.scx_frame0_buffer.bin")]
    [InlineData("(2)Isolation.scx_frame0_buffer.bin")]
    [InlineData("(3)Stepping Stones.scm_frame0_buffer.bin")]
    [InlineData("(4)Arctic Station.scx_frame0_buffer.bin")]
    [InlineData("(4)Space Debris.scm_frame0_buffer.bin")]
    [InlineData("(5)Twilight Star.scx_frame0_buffer.bin")]
    [InlineData("(6)Sapphire Isles.scx_frame0_buffer.bin")]
    [InlineData("(7)Black Lotus.scx_frame0_buffer.bin")]
    [InlineData("(8)Frozen Sea.scx_frame0_buffer.bin")]
    public void LoadFromFrame0Buffer(string testMapName)
    {
        using var mmf = GetMemoryMappedFileForMap(Path.Combine(ResourcesFolder, testMapName));
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