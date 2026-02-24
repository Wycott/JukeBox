using JukeboxInterfaces;
using Moq;

namespace JukeboxLibrary.Test;

public class JukeboxEngineTest
{
    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();

        // Act
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Assert
        Assert.Equal(songSourcesMock.Object, engine.SongSources);
        Assert.Equal(songListMock.Object, engine.SongList);
        Assert.Equal(songPlayerMock.Object, engine.SongPlayer);
        Assert.Equal(displayMock.Object, engine.DisplayEngine);
    }

    [Fact]
    public void Start_ShowsFlowerBox()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        var songListMock = new Mock<ISongList>();
        var songPlayerMock = new Mock<ISongPlayer>();
        var displayMock = new Mock<IDisplay>();
        var consoleMock = new Mock<IConsoleEngine>();
        
        var cts = new CancellationTokenSource();
        consoleMock.Setup(x => x.WriteText(It.IsAny<string>()));
        consoleMock.Setup(x => x.ReadLine())
            .Callback(() => cts.Cancel())
            .Throws(new OperationCanceledException());
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try
        {
            engine.Start();
        }
        catch (OperationCanceledException)
        {
        }

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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => 
        {
            if (callCount++ == 0) return "test";
            throw new OperationCanceledException();
        });
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(true);
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try { engine.Start(); } catch (OperationCanceledException) { }

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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => 
        {
            if (callCount++ < 2) return "";
            throw new OperationCanceledException();
        });
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try { engine.Start(); } catch (OperationCanceledException) { }

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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => 
        {
            if (callCount++ == 0) return "test";
            throw new OperationCanceledException();
        });
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try { engine.Start(); } catch (OperationCanceledException) { }

        // Assert
        displayMock.Verify(x => x.WriteError("Not Found!"), Times.Once);
    }

    [Fact]
    public void Start_UserSkipsSong_ShowsNothingSelectedError()
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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => 
        {
            if (callCount++ == 0) return "test";
            throw new OperationCanceledException();
        });
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns((bool?)null);
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try { engine.Start(); } catch (OperationCanceledException) { }

        // Assert
        displayMock.Verify(x => x.WriteError("Nothing selected!"), Times.Once);
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
        consoleMock.Setup(x => x.ReadLine()).Returns(() => 
        {
            if (callCount++ == 0) return "test";
            throw new OperationCanceledException();
        });
        
        var songCallCount = 0;
        displayMock.Setup(x => x.IsThisTheRightSong(It.IsAny<string>())).Returns(() => songCallCount++ == 0 ? false : true);
        
        var engine = new JukeboxEngine(songSourcesMock.Object, songListMock.Object, 
            songPlayerMock.Object, displayMock.Object, consoleMock.Object);

        // Act
        try { engine.Start(); } catch (OperationCanceledException) { }

        // Assert
        displayMock.Verify(x => x.IsThisTheRightSong("test1"), Times.Once);
        displayMock.Verify(x => x.IsThisTheRightSong("test2"), Times.Once);
        songPlayerMock.Verify(x => x.PlaySong("test2.mp3"), Times.Once);
    }
}
