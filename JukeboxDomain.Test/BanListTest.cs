using AiAnnotations;

namespace JukeboxDomain.Test;

[AiGenerated]
public class BanListTest : IDisposable
{
    private readonly string filePath = Path.Combine(Path.GetTempPath(), $"banned_{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Ban_PersistsAcrossInstances()
    {
        new BanList(filePath).Ban(@"C:\Music\song.mp3");

        var reloaded = new BanList(filePath);

        Assert.True(reloaded.IsBanned(@"C:\Music\song.mp3"));
    }

    [Fact]
    public void Ban_SamePathDifferentCase_IsStoredOnce()
    {
        var banList = new BanList(filePath);

        banList.Ban(@"C:\Music\song.mp3");
        banList.Ban(@"C:\MUSIC\SONG.MP3");

        Assert.Single(banList.GetBannedSongs());
    }

    [Fact]
    public void Load_WithMissingFile_StartsEmpty()
    {
        var banList = new BanList(filePath);

        Assert.Empty(banList.GetBannedSongs());
    }

    [Fact]
    public void Load_WithCorruptJson_StartsEmpty()
    {
        File.WriteAllText(filePath, "{ not valid json");

        var banList = new BanList(filePath);

        Assert.Empty(banList.GetBannedSongs());
    }
}
