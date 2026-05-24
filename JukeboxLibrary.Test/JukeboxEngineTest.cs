using AiAnnotations;
using JukeboxInterfaces;
using Moq;

namespace JukeboxLibrary.Test;

[AiGenerated]
public class JukeboxEngineTest
{
    private static JukeboxEngine CreateEngine(
        Mock<ISongSources>? songSources = null,
        Mock<ISongList>? songList = null,
        Mock<ISongPlayer>? songPlayer = null,
        Mock<IDisplay>? display = null,
        Mock<IConsoleEngine>? console = null,
        Mock<IFavourites>? favourites = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            (songPlayer ?? new Mock<ISongPlayer>()).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object);
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
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(true);

        var engine = CreateEngine(songSourcesMock, songListMock, songPlayerMock, displayMock, consoleMock, favouritesMock);
        engine.Start();

        songListMock.Verify(x => x.Build(songSourcesMock.Object, "test"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test.mp3"), Times.Once);
        favouritesMock.Verify(x => x.RecordPlay("test.mp3"), Times.Once);
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
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns((bool?)null);

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
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(() => songCallCount++ == 0 ? false : true);

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
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(false);

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
        Mock<IFavourites>? favourites = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            (songPlayer ?? new Mock<ISongPlayer>()).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object);
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
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(true);

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
        Mock<IFavourites>? favourites = null)
    {
        return new JukeboxEngine(
            (songSources ?? new Mock<ISongSources>()).Object,
            (songList ?? new Mock<ISongList>()).Object,
            (songPlayer ?? new Mock<ISongPlayer>()).Object,
            (display ?? new Mock<IDisplay>()).Object,
            (console ?? new Mock<IConsoleEngine>()).Object,
            (favourites ?? new Mock<IFavourites>()).Object);
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

        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(@"C:\Music\fav.mp3", 10));

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? ":f" : ":q");

        var engine = CreateEngine(songPlayer: songPlayerMock, display: displayMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songPlayerMock.Verify(x => x.PlaySong(@"C:\Music\fav.mp3"), Times.Once);
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

        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(@"C:\Music\fav.mp3", 5));

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ switch
        {
            0 => ":f",
            1 => ":n",
            _ => ":q"
        });

        var engine = CreateEngine(songPlayer: songPlayerMock, console: consoleMock, favourites: favouritesMock);
        engine.Start();

        songPlayerMock.Verify(x => x.PlaySong(@"C:\Music\fav.mp3"), Times.Exactly(2));
        favouritesMock.Verify(x => x.RecordPlay(@"C:\Music\fav.mp3"), Times.Exactly(2));
    }

    [Fact]
    public void Start_WithF_ThenNewPattern_ExitsFavouritesMode()
    {
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var consoleMock = new Mock<IConsoleEngine>();
        var favouritesMock = new Mock<IFavourites>();

        favouritesMock.Setup(x => x.GetRandomFavourite())
            .Returns(new FavouriteEntry(@"C:\Music\fav.mp3", 5));
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
}
