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
