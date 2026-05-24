using JukeboxDomain;

namespace JukeboxDomain.Test;

public class FavouritesTest : IDisposable
{
    private readonly string tempFile;

    public FavouritesTest()
    {
        tempFile = Path.Combine(Path.GetTempPath(), $"favourites_test_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void RecordPlay_NewSong_SetsCountToOne()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\song.mp3");

        var top = favourites.GetTopFavourites();

        Assert.Single(top);
        Assert.Equal(1, top[0].PlayCount);
        Assert.Equal(@"C:\Music\song.mp3", top[0].FullPath);
    }

    [Fact]
    public void RecordPlay_ExistingSong_IncrementsCount()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\song.mp3");
        favourites.RecordPlay(@"C:\Music\song.mp3");
        favourites.RecordPlay(@"C:\Music\song.mp3");

        var top = favourites.GetTopFavourites();

        Assert.Single(top);
        Assert.Equal(3, top[0].PlayCount);
    }

    [Fact]
    public void RecordPlay_PersistsToDisk()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music\song.mp3");

        // Load a new instance from the same file
        var reloaded = new Favourites(tempFile);
        var top = reloaded.GetTopFavourites();

        Assert.Single(top);
        Assert.Equal(1, top[0].PlayCount);
    }

    [Fact]
    public void GetTopFavourites_ReturnsInDescendingOrder()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\a.mp3");
        favourites.RecordPlay(@"C:\Music\b.mp3");
        favourites.RecordPlay(@"C:\Music\b.mp3");
        favourites.RecordPlay(@"C:\Music\c.mp3");
        favourites.RecordPlay(@"C:\Music\c.mp3");
        favourites.RecordPlay(@"C:\Music\c.mp3");

        var top = favourites.GetTopFavourites();

        Assert.Equal(3, top.Count);
        Assert.Equal(@"C:\Music\c.mp3", top[0].FullPath);
        Assert.Equal(3, top[0].PlayCount);
        Assert.Equal(@"C:\Music\b.mp3", top[1].FullPath);
        Assert.Equal(2, top[1].PlayCount);
        Assert.Equal(@"C:\Music\a.mp3", top[2].FullPath);
        Assert.Equal(1, top[2].PlayCount);
    }

    [Fact]
    public void GetTopFavourites_WhenEmpty_ReturnsEmptyList()
    {
        var favourites = new Favourites(tempFile);

        var top = favourites.GetTopFavourites();

        Assert.Empty(top);
    }

    [Fact]
    public void GetRandomFavourite_WhenEmpty_ReturnsNull()
    {
        var favourites = new Favourites(tempFile);

        var result = favourites.GetRandomFavourite();

        Assert.Null(result);
    }

    [Fact]
    public void GetRandomFavourite_WithEntries_ReturnsValidEntry()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music\song.mp3");

        var result = favourites.GetRandomFavourite();

        Assert.NotNull(result);
        Assert.Equal(@"C:\Music\song.mp3", result.FullPath);
        Assert.Equal(1, result.PlayCount);
    }

    [Fact]
    public void Trim_KeepsOnlyMaxFavourites()
    {
        var favourites = new Favourites(tempFile, maxFavourites: 3);

        // Build up 4 songs — each with distinct play counts higher than the next
        // d=4, c=3, b=2, a=1 — but we add them in order so trim happens correctly
        favourites.RecordPlay(@"C:\Music\d.mp3");
        favourites.RecordPlay(@"C:\Music\d.mp3");
        favourites.RecordPlay(@"C:\Music\d.mp3");
        favourites.RecordPlay(@"C:\Music\d.mp3");  // 4 plays
        favourites.RecordPlay(@"C:\Music\c.mp3");
        favourites.RecordPlay(@"C:\Music\c.mp3");
        favourites.RecordPlay(@"C:\Music\c.mp3");  // 3 plays
        favourites.RecordPlay(@"C:\Music\b.mp3");
        favourites.RecordPlay(@"C:\Music\b.mp3");  // 2 plays
        favourites.RecordPlay(@"C:\Music\a.mp3");  // 1 play — triggers trim (4 entries), a.mp3 is lowest

        var top = favourites.GetTopFavourites();

        Assert.Equal(3, top.Count);
        Assert.DoesNotContain(top, e => e.FullPath == @"C:\Music\a.mp3");
    }

    [Fact]
    public void Trim_KeepsHighestPlayCounts()
    {
        var favourites = new Favourites(tempFile, maxFavourites: 2);

        // Build up 3 songs with distinct counts, then the 3rd triggers trim
        favourites.RecordPlay(@"C:\Music\popular.mp3");
        favourites.RecordPlay(@"C:\Music\popular.mp3");
        favourites.RecordPlay(@"C:\Music\popular.mp3");  // 3 plays
        favourites.RecordPlay(@"C:\Music\medium.mp3");
        favourites.RecordPlay(@"C:\Music\medium.mp3");   // 2 plays
        favourites.RecordPlay(@"C:\Music\rare.mp3");     // 1 play — triggers trim, rare.mp3 removed

        var top = favourites.GetTopFavourites();

        Assert.Equal(2, top.Count);
        Assert.Equal(@"C:\Music\popular.mp3", top[0].FullPath);
        Assert.Equal(@"C:\Music\medium.mp3", top[1].FullPath);
    }

    [Fact]
    public void Load_WithCorruptJson_ReturnsEmptyDictionary()
    {
        File.WriteAllText(tempFile, "not valid json {{{");

        var favourites = new Favourites(tempFile);
        var top = favourites.GetTopFavourites();

        Assert.Empty(top);
    }

    [Fact]
    public void Load_WithMissingFile_ReturnsEmptyDictionary()
    {
        var favourites = new Favourites(@"Z:\nonexistent\path\favourites.json");
        var top = favourites.GetTopFavourites();

        Assert.Empty(top);
    }

    [Fact]
    public void RecordPlay_MultipleSongs_AllTracked()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\one.mp3");
        favourites.RecordPlay(@"C:\Music\two.mp3");
        favourites.RecordPlay(@"C:\Music\three.mp3");

        var top = favourites.GetTopFavourites();

        Assert.Equal(3, top.Count);
        Assert.All(top, entry => Assert.Equal(1, entry.PlayCount));
    }
}
