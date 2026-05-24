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

        // Act
        var songSources = new SongSources(displayMock.Object);

        // Assert
        Assert.NotNull(songSources.Sources);
        Assert.NotEmpty(songSources.Sources);
    }

    [Fact]
    public void Constructor_DisplaysSongCounts()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();

        // Act
        var songSources = new SongSources(displayMock.Object);

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Constructor_DisplaysCountForEachSource()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();

        // Act
        var songSources = new SongSources(displayMock.Object);

        // Assert
        displayMock.Verify(x => x.WriteYellowText(It.Is<string>(s => s.Contains("songs"))), Times.Exactly(songSources.Sources.Count));
    }
}
