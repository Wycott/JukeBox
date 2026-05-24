using AiAnnotations;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test.Helpers;

[AiGenerated]
public class FileSystemParserTest
{
    [Fact]
    public void ParseFileSystem_WithValidPattern_ReturnsResults()
    {
        // Arrange
        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { "C:\\" });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<ISong>>(result);
    }

    [Fact]
    public void ParseFileSystem_WithArtistMarker_ReturnsResults()
    {
        // Arrange
        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string>());

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "*@Jovi");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<ISong>>(result);
    }

    [Fact]
    public void ParseFileSystem_WithInvalidDirectory_ReturnsEmpty()
    {
        // Arrange
        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { "Z:\\" });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
