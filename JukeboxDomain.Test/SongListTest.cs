using JukeboxDomain;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test;

public class SongListTest
{
    [Fact]
    public void Build_DelegatesToFileSystemParser()
    {
        // Arrange
        var songList = new SongList();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string>());

        // Act
        songList.Build(songSourcesMock.Object, "test");

        // Assert
        Assert.NotNull(songList.SongCollection);
    }

    [Fact]
    public void Build_WithNoSources_ReturnsEmptyCollection()
    {
        // Arrange
        var songList = new SongList();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string>());

        // Act
        songList.Build(songSourcesMock.Object, "anything");

        // Assert
        Assert.Empty(songList.SongCollection);
    }

    [Fact]
    public void Build_WithInvalidDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        var songList = new SongList();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { @"Z:\NonExistent\" });

        // Act
        songList.Build(songSourcesMock.Object, "test");

        // Assert
        Assert.Empty(songList.SongCollection);
    }

    [Fact]
    public void SongCollection_DefaultsToEmptyList()
    {
        // Arrange & Act
        var songList = new SongList();

        // Assert
        Assert.NotNull(songList.SongCollection);
        Assert.Empty(songList.SongCollection);
    }
}
