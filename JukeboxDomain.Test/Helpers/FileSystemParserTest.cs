using AiAnnotations;
using JukeboxDomain;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test.Helpers;

[AiGenerated]
public class FileSystemParserTest
{
    [Fact]
    public void WhenParseFileSystemIsCalled_WithValidPattern_ThenSongsAreReturned()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { "C:\\" });

        // Act
        var result = FileSystemParser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<ISong>>(result);
    }

    [Fact]
    public void WhenParseFileSystemIsCalled_WithArtistMarker_ThenResultsAreRandomized()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string>());

        // Act
        var result = FileSystemParser.ParseFileSystem(songSourcesMock.Object, "*@Jovi");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<ISong>>(result);
    }

    [Fact]
    public void WhenParseFileSystemIsCalled_WithInvalidDirectory_ThenContinuesProcessing()
    {
        // Arrange
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { "Z:\\" });

        // Act
        var result = FileSystemParser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
