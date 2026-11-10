using AiAnnotations;
using JukeboxInterfaces;
using Moq;

namespace JukeboxLibrary.Test;

internal static class SongPlayerMocks
{
    // Engine tests assume playback succeeds unless a test configures the mock itself.
    public static Mock<ISongPlayer> WithSuccessfulPlayback(Mock<ISongPlayer>? songPlayer)
    {
        var mock = songPlayer ?? new Mock<ISongPlayer>();

        if (!mock.Setups.Any())
        {
            mock.Setup(x => x.PlaySong(It.IsAny<string>())).Returns(true);
        }

        return mock;
    }
}

[AiGenerated]
public class JukeboxEngineTest
{
    private static JukeboxEngine CreateEngine(
        Mock<ISongSources>? songSources = null,
        Mock<ISongList>? songList = null,
        Mock<ISongPlayer>? songPlayer = null,
        Mock<IDisplay>? display = null,
        Mock<IConsoleEngine>? console = null,
        Mock<IFavourites>? favourites = null,
        Mock<IBanList>? banList = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            SongPlayerMocks.WithSuccessfulPlayback(songPlayer).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object,
            (banList ?? new Mock<IBanList>()).Object);
    }

    [Fact]
    public void Start_ShowsFlowerBox()
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        consoleMock.Setup(x => x.ReadLine()).Returns(":q");

        var engine = CreateEngine(display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.FlowerBox(), Times.Once);
    }

    [Fact]
    public void Start_WithValidPattern_FindsAndPlaysSong()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.FullPath).Returns("test.mp3");
        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : ":q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);

        var engine = CreateEngine(songSourcesMock, songListMock, songPlayerMock, displayMock, consoleMock, favouritesMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "test"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test.mp3"), Times.Once);
        favouritesMock.Verify(x => x.RecordPlay("test.mp3"), Times.Once);
    }

    [Fact]
    public void Start_WhenPlaybackFails_DoesNotRecordFavouriteButRemoveTargetsFailedSong()
    {
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.FullPath).Returns("broken.mp3");
        songMock.Setup(x => x.ShortenedPath).Returns("broken");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });
        songPlayerMock.Setup(x => x.PlaySong(It.IsAny<string>())).Returns(false);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => "broken",
            1 => ":r",
            _ => ":q"
        });
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);

        var engine = CreateEngine(songList: songListMock, songPlayer: songPlayerMock, display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songPlayerMock.Verify(x => x.PlaySong("broken.mp3"), Times.Once);
        favouritesMock.Verify(x => x.RecordPlay(It.IsAny<string>()), Times.Never);
        favouritesMock.Verify(x => x.Remove("broken.mp3"), Times.Once);
        displayMock.Verify(x => x.WriteError("No song has been played yet"), Times.Never);
    }

    [Fact]
    public void Start_WhenGoodSongThenFailedSong_BanTargetsFailedSong()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();
        var banListMock = new Mock<IBanList>();

        var goodPath = Path.Combine("C:", "Music", "good.mp3");
        var brokenPath = Path.Combine("C:", "Music", "broken.mp3");
        favouritesMock.SetupSequence(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(goodPath, 5))
            .Returns(new FavouriteEntry(brokenPath, 5));
        songPlayerMock.Setup(x => x.PlaySong(goodPath)).Returns(true);
        songPlayerMock.Setup(x => x.PlaySong(brokenPath)).Returns(false);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => ":f",
            1 => ":n",
            2 => ":b",
            _ => ":q"
        });

        var engine = CreateEngine(songPlayer: songPlayerMock, console: consoleMock, favourites: favouritesMock, banList: banListMock);
        engine.Start();

        banListMock.Verify(x => x.Ban(brokenPath), Times.Once);
        banListMock.Verify(x => x.Ban(goodPath), Times.Never);
    }

    [Fact]
    public void Start_WithF_WhenFavouriteFileIsMissing_RemovesItFromFavourites()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        var missingPath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.mp3");
        favouritesMock.Setup(x => x.GetRandomFavourite()).Returns(new FavouriteEntry(missingPath, 5));
        songPlayerMock.Setup(x => x.PlaySong(It.IsAny<string>())).Returns(false);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":f" : ":q");

        var engine = CreateEngine(songPlayer: songPlayerMock, display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        favouritesMock.Verify(x => x.Remove(missingPath), Times.Once);
        favouritesMock.Verify(x => x.RecordPlay(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithF_WhenExistingFavouriteFailsToPlay_KeepsIt()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        var existingPath = Path.Combine(Path.GetTempPath(), $"exists_{Guid.NewGuid():N}.mp3");
        File.WriteAllText(existingPath, "not audio");

        try
        {
            favouritesMock.Setup(x => x.GetRandomFavourite()).Returns(new FavouriteEntry(existingPath, 5));
            songPlayerMock.Setup(x => x.PlaySong(It.IsAny<string>())).Returns(false);

            var callCount = 0;
            consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":f" : ":q");

            var engine = CreateEngine(songPlayer: songPlayerMock, console: consoleMock, favourites: favouritesMock);
            engine.Start();

            favouritesMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
        }
        finally
        {
            File.Delete(existingPath);
        }
    }

    [Fact]
    public void Start_WithEmptyPattern_StaysInRequestState()
    {
        var songListMock = new Mock<ISongList>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ < 2 ? "" : ":q");

        var engine = CreateEngine(songList: songListMock, console: consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithNoSongsFound_ShowsError()
    {
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : ":q");

        var engine = CreateEngine(songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("Not Found!"), Times.Once);
    }

    [Fact]
    public void Start_UserSkipsSong_ReturnsToPromptWithoutError()
    {
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : ":q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Cancel);

        var engine = CreateEngine(songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_UserRejectsSong_ContinuesToNextSong()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var song1Mock = new Mock<ISong>();
        var song2Mock = new Mock<ISong>();

        song1Mock.Setup(x => x.ShortenedPath).Returns("test1");
        song1Mock.Setup(x => x.FullPath).Returns("test1.mp3");
        song2Mock.Setup(x => x.ShortenedPath).Returns("test2");
        song2Mock.Setup(x => x.FullPath).Returns("test2.mp3");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { song1Mock.Object, song2Mock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : ":q");

        var songCallCount = 0;
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(() => songCallCount++ == 0 ? SongResponse.No : SongResponse.Yes);

        var engine = CreateEngine(songPlayer: songPlayerMock, songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.IsThisTheRightSong("test1"), Times.Once);
        displayMock.Verify(x => x.IsThisTheRightSong("test2"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test2.mp3"), Times.Once);
    }

    [Fact]
    public void Start_WithCancellationToken_ExitsGracefully()
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        using var cts = new CancellationTokenSource();
        consoleMock.Setup(x => x.ReadLine()).Callback(() => cts.Cancel()).Returns("");

        var engine = CreateEngine(display: displayMock, console: consoleMock);
        engine.Start(cts.Token);

        displayMock.Verify(x => x.FlowerBox(), Times.Once);
    }

    [Fact]
    public void Start_ShowTitleBox_CallsDisplaySongCounts()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var consoleMock = new Mock<IConsoleEngine>();
        consoleMock.Setup(x => x.ReadLine()).Returns(":q");

        var engine = CreateEngine(songSources: songSourcesMock, console: consoleMock);
        engine.Start();

        songSourcesMock.Verify(x => x.DisplaySongCounts(), Times.Once);
    }

    [Fact]
    public void Start_WithUppercaseQ_ExitsGracefully()
    {
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        consoleMock.Setup(x => x.ReadLine()).Returns(":Q");

        var engine = CreateEngine(songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.FlowerBox(), Times.Once);
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithNullReadLine_StaysInRequestState()
    {
        var songListMock = new Mock<ISongList>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? null : ":q");

        var engine = CreateEngine(songList: songListMock, console: consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_UserRejectsAllSongs_ShowsNothingSelectedError()
    {
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : ":q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.No);

        var engine = CreateEngine(songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("Nothing selected!"), Times.Once);
    }

    [Fact]
    public void Start_WhitespaceOnlyPattern_DoesNotSearch()
    {
        var songListMock = new Mock<ISongList>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "   " : ":q");

        var engine = CreateEngine(songList: songListMock, console: consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }
}

public class JukeboxEngineNextPatternTest
{
    private static JukeboxEngine CreateEngine(
        Mock<ISongSources>? songSources = null,
        Mock<ISongList>? songList = null,
        Mock<ISongPlayer>? songPlayer = null,
        Mock<IDisplay>? display = null,
        Mock<IConsoleEngine>? console = null,
        Mock<IFavourites>? favourites = null,
        Mock<IBanList>? banList = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            SongPlayerMocks.WithSuccessfulPlayback(songPlayer).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object,
            (banList ?? new Mock<IBanList>()).Object);
    }

    [Fact]
    public void Start_WithN_RepeatsLastPattern()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.FullPath).Returns("love_song.mp3");
        songMock.Setup(x => x.ShortenedPath).Returns("love_song");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => "love",
            1 => ":n",
            _ => ":q"
        });

        var engine = CreateEngine(songSourcesMock, songListMock, songPlayerMock, displayMock, consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "love"), Times.Exactly(2));
    }

    [Fact]
    public void Start_WithN_WhenNoLastPattern_ShowsError()
    {
        var songListMock = new Mock<ISongList>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":n" : ":q");

        var engine = CreateEngine(songList: songListMock, display: displayMock, console: consoleMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("No previous pattern"), Times.Once);
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithUppercaseN_RepeatsLastPattern()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var consoleMock = new Mock<IConsoleEngine>();

        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => "@stones",
            1 => ":N",
            _ => ":q"
        });

        var engine = CreateEngine(songSourcesMock, songListMock, console: consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "@stones"), Times.Exactly(2));
    }
}

public class JukeboxEngineFavouritesTest
{
    private static JukeboxEngine CreateEngine(
        Mock<ISongSources>? songSources = null,
        Mock<ISongList>? songList = null,
        Mock<ISongPlayer>? songPlayer = null,
        Mock<IDisplay>? display = null,
        Mock<IConsoleEngine>? console = null,
        Mock<IFavourites>? favourites = null,
        Mock<IBanList>? banList = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            SongPlayerMocks.WithSuccessfulPlayback(songPlayer).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object,
            (banList ?? new Mock<IBanList>()).Object);
    }

    [Fact]
    public void Start_WithL_ListsFavourites()
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        favouritesMock.Setup(x => x.GetTopFavourites()).Returns(new List<FavouriteEntry>
        {
            new(@"C:\Music\song1.mp3", 5),
            new(@"C:\Music\song2.mp3", 3)
        });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":l" : ":q");

        var engine = CreateEngine(display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteYellowText(It.Is<string>(s => s.Contains("5x") && s.Contains("song1.mp3"))), Times.Once);
        displayMock.Verify(x => x.WriteYellowText(It.Is<string>(s => s.Contains("3x") && s.Contains("song2.mp3"))), Times.Once);
    }

    [Fact]
    public void Start_WithL_WhenNoFavourites_ShowsError()
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        favouritesMock.Setup(x => x.GetTopFavourites()).Returns(new List<FavouriteEntry>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":l" : ":q");

        var engine = CreateEngine(display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("No favourites yet"), Times.Once);
    }

    [Fact]
    public void Start_WithF_PlaysRandomFavourite()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        var favPath = Path.Combine("C:", "Music", "fav.mp3");
        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(favPath, 10));

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":f" : ":q");

        var engine = CreateEngine(songPlayer: songPlayerMock, display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songPlayerMock.Verify(x => x.PlaySong(favPath), Times.Once);
        displayMock.Verify(x => x.WriteYellowText("fav.mp3"), Times.Once);
        consoleMock.Verify(x => x.WriteText("Playing: "), Times.Once);
    }

    [Fact]
    public void Start_WithF_WhenNoFavourites_ShowsError()
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        favouritesMock.Setup(x => x.GetRandomFavourite()).Returns((FavouriteEntry?)null);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":f" : ":q");

        var engine = CreateEngine(display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("No favourites yet"), Times.Once);
    }

    [Fact]
    public void Start_WithF_ThenN_PlaysAnotherFavourite()
    {
        var songPlayerMock = new Mock<ISongPlayer>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        var favPath = Path.Combine("C:", "Music", "fav.mp3");
        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(favPath, 5));

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => ":f",
            1 => ":n",
            _ => ":q"
        });

        var engine = CreateEngine(songPlayer: songPlayerMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songPlayerMock.Verify(x => x.PlaySong(favPath), Times.Exactly(2));
        favouritesMock.Verify(x => x.RecordPlay(favPath), Times.Exactly(2));
    }

    [Fact]
    public void Start_WithF_ThenNewPattern_ExitsFavouritesMode()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        var favPath = Path.Combine("C:", "Music", "fav.mp3");
        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(favPath, 5));
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => ":f",
            1 => "rock",
            _ => ":q"
        });

        var engine = CreateEngine(songSourcesMock, songListMock, songPlayerMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "rock"), Times.Once);
    }

    [Fact]
    public void Start_SingleLetterL_IsUsedAsSearchPattern()
    {
        // Verify that "l" without colon is treated as a search pattern, not a command
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var consoleMock = new Mock<IConsoleEngine>();

        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "l" : ":q");

        var engine = CreateEngine(songSourcesMock, songListMock, console: consoleMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "l"), Times.Once);
    }

    private static Mock<IFavourites> FavouritesGrowingFrom(int startCount, int maxFavourites)
    {
        // Each RecordPlay adds a new song, so Count goes up by one.
        var favouritesMock = new Mock<IFavourites>();
        var count = startCount;
        favouritesMock.Setup(x => x.Count).Returns(() => count);
        favouritesMock.Setup(x => x.MaxFavourites).Returns(maxFavourites);
        favouritesMock.Setup(x => x.RecordPlay(It.IsAny<string>())).Callback(() => count++);
        return favouritesMock;
    }

    private static Mock<IConsoleEngine> ConsoleTyping(params string[] lines)
    {
        var consoleMock = new Mock<IConsoleEngine>();
        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount < lines.Length ? lines[callCount++] : ":q");
        return consoleMock;
    }

    private static Mock<ISongList> SongListWith(string fullPath)
    {
        var songMock = new Mock<ISong>();
        songMock.Setup(x => x.FullPath).Returns(fullPath);
        songMock.Setup(x => x.ShortenedPath).Returns(fullPath);

        var songListMock = new Mock<ISongList>();
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });
        return songListMock;
    }

    [Fact]
    public void Start_WhenNewSongTakesFavouritesOverLimit_AddsItAndWarns()
    {
        var displayMock = new Mock<IDisplay>();
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);
        var favouritesMock = FavouritesGrowingFrom(startCount: 100, maxFavourites: 100);

        var engine = CreateEngine(songList: SongListWith("new.mp3"), display: displayMock, console: ConsoleTyping("new"), favourites: favouritesMock);
        engine.Start();

        favouritesMock.Verify(x => x.RecordPlay("new.mp3"), Times.Once);
        displayMock.Verify(x => x.WriteError("Favourites is over its limit (101 of 100) - use :t to trim"), Times.Once);
    }

    [Fact]
    public void Start_WhenNewSongStaysWithinLimit_DoesNotWarn()
    {
        var displayMock = new Mock<IDisplay>();
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);
        var favouritesMock = FavouritesGrowingFrom(startCount: 99, maxFavourites: 100);

        var engine = CreateEngine(songList: SongListWith("new.mp3"), display: displayMock, console: ConsoleTyping("new"), favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError(It.Is<string>(s => s.StartsWith("Favourites is over its limit"))), Times.Never);
    }

    [Fact]
    public void Start_WhenReplayingFavouriteWhileOverLimit_DoesNotWarn()
    {
        var displayMock = new Mock<IDisplay>();
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(SongResponse.Yes);
        var favouritesMock = new Mock<IFavourites>();
        favouritesMock.Setup(x => x.Count).Returns(150);  // already over, and a replay doesn't grow it
        favouritesMock.Setup(x => x.MaxFavourites).Returns(100);

        var engine = CreateEngine(songList: SongListWith("old.mp3"), display: displayMock, console: ConsoleTyping("old"), favourites: favouritesMock);
        engine.Start();

        favouritesMock.Verify(x => x.RecordPlay("old.mp3"), Times.Once);
        displayMock.Verify(x => x.WriteError(It.Is<string>(s => s.StartsWith("Favourites is over its limit"))), Times.Never);
    }

    [Theory]
    [InlineData(":t")]
    [InlineData(":T")]
    public void Start_WithT_TrimsAndReportsWhatWasRemoved(string command)
    {
        var displayMock = new Mock<IDisplay>();
        var favouritesMock = new Mock<IFavourites>();
        favouritesMock.Setup(x => x.TrimLeastPlayed()).Returns(
        [
            new FavouriteEntry("a.mp3", 1),
            new FavouriteEntry("b.mp3", 1)
        ]);
        favouritesMock.Setup(x => x.Count).Returns(98);

        var engine = CreateEngine(display: displayMock, console: ConsoleTyping(command), favourites: favouritesMock);
        engine.Start();

        favouritesMock.Verify(x => x.TrimLeastPlayed(), Times.Once);
        displayMock.Verify(x => x.WriteYellowText("Trimmed 2 favourites with 1 play (98 left)"), Times.Once);
    }

    [Fact]
    public void Start_WithT_SingleEntryWithSeveralPlays_UsesSingularAndPluralCorrectly()
    {
        var displayMock = new Mock<IDisplay>();
        var favouritesMock = new Mock<IFavourites>();
        favouritesMock.Setup(x => x.TrimLeastPlayed()).Returns([new FavouriteEntry("a.mp3", 3)]);
        favouritesMock.Setup(x => x.Count).Returns(5);

        var engine = CreateEngine(display: displayMock, console: ConsoleTyping(":t"), favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteYellowText("Trimmed 1 favourite with 3 plays (5 left)"), Times.Once);
    }

    [Fact]
    public void Start_WithT_WhenNoFavourites_ShowsError()
    {
        var displayMock = new Mock<IDisplay>();
        var favouritesMock = new Mock<IFavourites>();
        favouritesMock.Setup(x => x.TrimLeastPlayed()).Returns([]);

        var engine = CreateEngine(display: displayMock, console: ConsoleTyping(":t"), favourites: favouritesMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("No favourites yet"), Times.Once);
    }

    [Fact]
    public void Start_WithQuestionMark_ShowsHelpAgain()
    {
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":?" : ":q");

        var engine = CreateEngine(console: consoleMock);
        engine.Start();

        // Once at start-up, once for :?
        consoleMock.Verify(x => x.WriteALine("Commands:"), Times.Exactly(2));
    }

    [Theory]
    [InlineData(":e")]
    [InlineData(":E")]
    public void Start_WithE_WhenNothingBanned_ShowsError(string command)
    {
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var banListMock = new Mock<IBanList>();
        banListMock.Setup(x => x.GetBannedSongs()).Returns([]);

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? command : ":q");

        var engine = CreateEngine(display: displayMock, console: consoleMock, banList: banListMock);
        engine.Start();

        displayMock.Verify(x => x.WriteError("No banned songs"), Times.Once);
    }
}
