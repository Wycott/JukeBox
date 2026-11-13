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
    public void GetRandomFavourite_PlaysEveryFavouriteBeforeRepeating()
    {
        var favourites = new Favourites(tempFile);
        var paths = Enumerable.Range(1, 10).Select(i => $@"C:\Music\song{i}.mp3").ToList();

        foreach (var path in paths)
        {
            favourites.RecordPlay(path);
        }

        for (var cycle = 0; cycle < 3; cycle++)
        {
            var picked = Enumerable.Range(0, paths.Count)
                .Select(_ => favourites.GetRandomFavourite()!.FullPath)
                .ToList();

            Assert.Equal(paths.OrderBy(p => p), picked.OrderBy(p => p));
        }
    }

    [Fact]
    public void GetRandomFavourite_NewCycle_DoesNotRepeatLastPick()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music.mp3");
        favourites.RecordPlay(@"C:\Music.mp3");

        var previous = favourites.GetRandomFavourite()!.FullPath;

        for (var i = 0; i < 20; i++)
        {
            var next = favourites.GetRandomFavourite()!.FullPath;
            Assert.NotEqual(previous, next);
            previous = next;
        }
    }

    [Fact]
    public void GetRandomFavourite_ReturnsRankByPlayCount()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music\only.mp3");
        favourites.RecordPlay(@"C:\Music\only.mp3");

        var result = favourites.GetRandomFavourite();

        Assert.Equal(1, result!.Rank);
    }

    [Fact]
    public void GetTopFavourites_AssignsRanksMostPlayedFirst()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music\low.mp3");
        favourites.RecordPlay(@"C:\Music\high.mp3");
        favourites.RecordPlay(@"C:\Music\high.mp3");
        favourites.RecordPlay(@"C:\Music\high.mp3");
        favourites.RecordPlay(@"C:\Music\mid.mp3");
        favourites.RecordPlay(@"C:\Music\mid.mp3");

        var top = favourites.GetTopFavourites();

        Assert.Equal([@"C:\Music\high.mp3", @"C:\Music\mid.mp3", @"C:\Music\low.mp3"], top.Select(e => e.FullPath));
        Assert.Equal([1, 2, 3], top.Select(e => e.Rank));
    }

    [Fact]
    public void RecordPlay_OverMax_StillAddsSong()
    {
        var favourites = new Favourites(tempFile, maxFavourites: 2);

        favourites.RecordPlay(@"C:\Music\one.mp3");
        favourites.RecordPlay(@"C:\Music\two.mp3");
        favourites.RecordPlay(@"C:\Music\three.mp3");

        Assert.Equal(3, favourites.Count);
        Assert.Equal(2, favourites.MaxFavourites);
        Assert.Contains(favourites.GetTopFavourites(), e => e.FullPath == @"C:\Music\three.mp3");
    }

    [Fact]
    public void TrimLeastPlayed_RemovesAllEntriesWithLowestCount()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\popular.mp3");
        favourites.RecordPlay(@"C:\Music\popular.mp3");
        favourites.RecordPlay(@"C:\Music\popular.mp3");  // 3 plays
        favourites.RecordPlay(@"C:\Music\medium.mp3");
        favourites.RecordPlay(@"C:\Music\medium.mp3");   // 2 plays
        favourites.RecordPlay(@"C:\Music\once-a.mp3");   // 1 play
        favourites.RecordPlay(@"C:\Music\once-b.mp3");   // 1 play

        var removed = favourites.TrimLeastPlayed();

        Assert.Equal([@"C:\Music\once-a.mp3", @"C:\Music\once-b.mp3"], removed.Select(e => e.FullPath).Order());
        Assert.All(removed, e => Assert.Equal(1, e.PlayCount));
        Assert.Equal([@"C:\Music\popular.mp3", @"C:\Music\medium.mp3"], favourites.GetTopFavourites().Select(e => e.FullPath));
    }

    [Fact]
    public void TrimLeastPlayed_RemovesOnlyOneTierPerCall()
    {
        var favourites = new Favourites(tempFile);

        favourites.RecordPlay(@"C:\Music\twice.mp3");
        favourites.RecordPlay(@"C:\Music\twice.mp3");
        favourites.RecordPlay(@"C:\Music\once.mp3");

        favourites.TrimLeastPlayed();
        var secondTrim = favourites.TrimLeastPlayed();

        Assert.Single(secondTrim);
        Assert.Equal(@"C:\Music\twice.mp3", secondTrim[0].FullPath);
        Assert.Equal(0, favourites.Count);
    }

    [Fact]
    public void TrimLeastPlayed_PersistsToDisk()
    {
        var favourites = new Favourites(tempFile);
        favourites.RecordPlay(@"C:\Music\keep.mp3");
        favourites.RecordPlay(@"C:\Music\keep.mp3");
        favourites.RecordPlay(@"C:\Music\drop.mp3");

        favourites.TrimLeastPlayed();
        var reloaded = new Favourites(tempFile);

        Assert.Equal([@"C:\Music\keep.mp3"], reloaded.GetTopFavourites().Select(e => e.FullPath));
    }

    [Fact]
    public void TrimLeastPlayed_WhenEmpty_ReturnsEmpty()
    {
        var favourites = new Favourites(tempFile);

        Assert.Empty(favourites.TrimLeastPlayed());
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
