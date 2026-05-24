using AiAnnotations;
using JukeboxInterfaces;
using Moq;

namespace JukeboxLibrary.Test;

[AiGenerated]
public class JukeboxEngineTest
{
    [Fact]
    public void Start_ShowsFlowerBox()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        consoleMock.Setup(x => x.ReadLine()).Returns("q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.FlowerBox(), Times.Once);
    }

    [Fact]
    public void Start_WithValidPattern_FindsAndPlaysSong()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.FullPath).Returns("test.mp3");
        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : "q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(true);

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        songListMock.Verify(x => x.Build(songSourcesMock.Object, "test"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test.mp3"), Times.Once);
    }

    [Fact]
    public void Start_WithEmptyPattern_StaysInRequestState()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ < 2 ? "" : "q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithNoSongsFound_ShowsError()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : "q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.WriteError("Not Found!"), Times.Once);
    }

    [Fact]
    public void Start_UserSkipsSong_ReturnsToPromptWithoutError()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : "q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns((bool?)null);

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert - user cancelled, so no error message should be shown
        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_UserRejectsSong_ContinuesToNextSong()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : "q");

        var songCallCount = 0;
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(() => songCallCount++ == 0 ? false : true);

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.IsThisTheRightSong("test1"), Times.Once);
        displayMock.Verify(x => x.IsThisTheRightSong("test2"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test2.mp3"), Times.Once);
    }

    [Fact]
    public void Start_WithCancellationToken_ExitsGracefully()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        using var cts = new CancellationTokenSource();
        consoleMock.Setup(x => x.ReadLine()).Callback(() => cts.Cancel()).Returns("");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start(cts.Token);

        // Assert - should exit without throwing
        displayMock.Verify(x => x.FlowerBox(), Times.Once);
    }
}

public class JukeboxEngineAdditionalTest
{
    [Fact]
    public void Start_ShowTitleBox_CallsDisplaySongCounts()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        consoleMock.Setup(x => x.ReadLine()).Returns("q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        songSourcesMock.Verify(x => x.DisplaySongCounts(), Times.Once);
    }

    [Fact]
    public void Start_WithUppercaseQ_ExitsGracefully()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        consoleMock.Setup(x => x.ReadLine()).Returns("Q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.FlowerBox(), Times.Once);
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithNullReadLine_StaysInRequestState()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? null : "q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_UserRejectsAllSongs_ShowsNothingSelectedError()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        var songMock = new Mock<ISong>();

        songMock.Setup(x => x.ShortenedPath).Returns("test");
        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong> { songMock.Object });

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "test" : "q");
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(false);

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.WriteError("Nothing selected!"), Times.Once);
    }

    [Fact]
    public void Start_WhitespaceOnlyPattern_DoesNotSearch()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "   " : "q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }
}

public class JukeboxEngineNextPatternTest
{
    [Fact]
    public void Start_WithN_RepeatsLastPattern()
    {
        // Arrange
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
        consoleMock.Setup(x => x.ReadLine()).Returns(() =>
        {
            return callCount++ switch
            {
                0 => "love",   // first search
                1 => "n",      // repeat last pattern
                _ => "q"
            };
        });

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert - Build should be called twice with "love"
        songListMock.Verify(x => x.Build(songSourcesMock.Object, "love"), Times.Exactly(2));
    }

    [Fact]
    public void Start_WithN_WhenNoLastPattern_ShowsError()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() => callCount++ == 0 ? "n" : "q");

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert
        displayMock.Verify(x => x.WriteError("No previous pattern"), Times.Once);
        songListMock.Verify(x => x.Build(It.IsAny<ISongSources>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Start_WithUppercaseN_RepeatsLastPattern()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        songListMock.Setup(x => x.SongCollection).Returns(new List<ISong>());

        var callCount = 0;
        consoleMock.Setup(x => x.ReadLine()).Returns(() =>
        {
            return callCount++ switch
            {
                0 => "@stones",
                1 => "N",       // uppercase N
                _ => "q"
            };
        });

        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object,
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        engine.Start();

        // Assert - Build called twice with the same pattern
        songListMock.Verify(x => x.Build(songSourcesMock.Object, "@stones"), Times.Exactly(2));
    }
}
