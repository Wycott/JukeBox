using JukeboxDomain;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test;

public class SongSourcesTest
{
    [Fact]
    public void Constructor_InitializesSources()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var sources = new List<string> { @"C:\Music\", @"D:\Songs\" };

        // Act
        var songSources = new SongSources(displayMock.Object, sources);

        // Assert
        Assert.NotNull(songSources.Sources);
        Assert.Equal(2, songSources.Sources.Count);
    }

    [Fact]
    public void Constructor_DoesNotPerformIO()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var sources = new List<string> { @"C:\Music\" };

        // Act
        _ = new SongSources(displayMock.Object, sources);

        // Assert - no display calls in constructor
        displayMock.Verify(x => x.WriteYellowText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DisplaySongCounts_ShowsCountForEachSource()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var sources = new List<string> { @"C:\Music\", @"D:\Songs\" };
        var songSources = new SongSources(displayMock.Object, sources);

        // Act
        songSources.DisplaySongCounts();

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.Is<string>(s => s.Contains("songs"))), Times.Exactly(sources.Count));
    }

    [Fact]
    public void DisplaySongCounts_CalledTwice_OnlyDisplaysOnce()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var sources = new List<string> { @"C:\Music\" };
        var songSources = new SongSources(displayMock.Object, sources);

        // Act
        songSources.DisplaySongCounts();
        songSources.DisplaySongCounts();

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.IsAny<string>()), Times.Once);
    }
}

public class SongSourcesEdgeCaseTest
{
    [Fact]
    public void Constructor_WithEmptySources_InitializesEmptyList()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();

        // Act
        var songSources = new SongSources(displayMock.Object, new List<string>());

        // Assert
        Assert.Empty(songSources.Sources);
    }

    [Fact]
    public void DisplaySongCounts_WithEmptySources_DoesNotCallDisplay()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var songSources = new SongSources(displayMock.Object, new List<string>());

        // Act
        songSources.DisplaySongCounts();

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DisplaySongCounts_WithNonExistentPath_ShowsZeroCount()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var songSources = new SongSources(displayMock.Object, new List<string> { @"Z:\NonExistent\" });

        // Act
        songSources.DisplaySongCounts();

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.Is<string>(s => s.Contains("0 songs"))), Times.Once);
    }
}
