using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test;

public class SongListTest
{
    [Fact]
    public void Build_DelegatesToFileSystemParser()
    {
        // Arrange
        var parserMock = new Mock<IFileSystemParser>();
        var songSourcesMock = new Mock<ISongSources>();
        var songs = new List<ISong> { new Mock<ISong>().Object };
        parserMock.Setup(x => x.ParseFileSystem(songSourcesMock.Object, "test")).Returns(songs);
        var songList = new SongList(parserMock.Object);

        // Act
        songList.Build(songSourcesMock.Object, "test");

        // Assert
        parserMock.Verify(x => x.ParseFileSystem(songSourcesMock.Object, "test"), Times.Once);
        Assert.Single(songList.SongCollection);
    }

    [Fact]
    public void Build_WithNoResults_ReturnsEmptyCollection()
    {
        // Arrange
        var parserMock = new Mock<IFileSystemParser>();
        var songSourcesMock = new Mock<ISongSources>();
        parserMock.Setup(x => x.ParseFileSystem(It.IsAny<ISongSources>(), It.IsAny<string>()))
            .Returns(new List<ISong>());
        var songList = new SongList(parserMock.Object);

        // Act
        songList.Build(songSourcesMock.Object, "anything");

        // Assert
        Assert.Empty(songList.SongCollection);
    }

    [Fact]
    public void BuildShuffled_ContainsEverySongFromTheParser()
    {
        var parserMock = new Mock<IFileSystemParser>();
        var songSourcesMock = new Mock<ISongSources>();
        var songs = Enumerable.Range(1, 50)
            .Select(i => (ISong)new Song { FullPath = $"song{i}.mp3", ShortenedPath = $"song{i}" })
            .ToList();
        parserMock.Setup(x => x.ParseFileSystem(songSourcesMock.Object, "*")).Returns(songs);
        var songList = new SongList(parserMock.Object);

        songList.BuildShuffled(songSourcesMock.Object);

        Assert.Equal(
            songs.Select(s => s.FullPath).Order(),
            songList.SongCollection.Select(s => s.FullPath).Order());
    }

    [Fact]
    public void SongCollection_DefaultsToEmptyList()
    {
        // Arrange & Act
        var parserMock = new Mock<IFileSystemParser>();
        var songList = new SongList(parserMock.Object);

        // Assert
        Assert.NotNull(songList.SongCollection);
        Assert.Empty(songList.SongCollection);
    }
}
