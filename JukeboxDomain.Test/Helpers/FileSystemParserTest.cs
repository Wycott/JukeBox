using AiAnnotations;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test.Helpers;

[AiGenerated]
public class FileSystemParserTest : IDisposable
{
    private readonly string testRoot;

    public FileSystemParserTest()
    {
        testRoot = Path.Combine(Path.GetTempPath(), "JukeboxTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void ParseFileSystem_WithMatchingFiles_ReturnsSongs()
    {
        // Arrange
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "test_song.mp3"), "");
        File.WriteAllText(Path.Combine(artistDir, "other.txt"), "");

        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { testRoot + Path.DirectorySeparatorChar });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.Single(result);
        Assert.Contains("test_song.mp3", result[0].FullPath);
    }

    [Fact]
    public void ParseFileSystem_WithM4aFiles_ReturnsSongs()
    {
        // Arrange
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "track.m4a"), "");

        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { testRoot + Path.DirectorySeparatorChar });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "track");

        // Assert
        Assert.Single(result);
        Assert.Contains("track.m4a", result[0].FullPath);
    }

    [Fact]
    public void ParseFileSystem_WithArtistMarker_FiltersToArtistDirectory()
    {
        // Arrange
        var matchDir = Path.Combine(testRoot, "Bon Jovi");
        var otherDir = Path.Combine(testRoot, "Metallica");
        Directory.CreateDirectory(matchDir);
        Directory.CreateDirectory(otherDir);
        File.WriteAllText(Path.Combine(matchDir, "livin.mp3"), "");
        File.WriteAllText(Path.Combine(otherDir, "enter.mp3"), "");

        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { testRoot + Path.DirectorySeparatorChar });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "@Bon Jovi");

        // Assert
        Assert.Single(result);
        Assert.Contains("livin.mp3", result[0].FullPath);
    }

    [Fact]
    public void ParseFileSystem_WithNoMatchingFiles_ReturnsEmpty()
    {
        // Arrange
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "song.mp3"), "");

        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { testRoot + Path.DirectorySeparatorChar });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "zzzznotfound");

        // Assert
        Assert.Empty(result);
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

    [Fact]
    public void ParseFileSystem_WithEmptySources_ReturnsEmpty()
    {
        // Arrange
        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string>());

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "test");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseFileSystem_SetsCorrectShortenedPath()
    {
        // Arrange
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "song.mp3"), "");

        var parser = new FileSystemParser();
        var sourceWithSeparator = testRoot + Path.DirectorySeparatorChar;
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { sourceWithSeparator });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "song");

        // Assert
        Assert.Single(result);
        Assert.Equal("Artist" + Path.DirectorySeparatorChar + "song.mp3", result[0].ShortenedPath);
    }

    [Fact]
    public void ParseFileSystem_IgnoresNonMusicExtensions()
    {
        // Arrange
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "readme.txt"), "");
        File.WriteAllText(Path.Combine(artistDir, "cover.jpg"), "");
        File.WriteAllText(Path.Combine(artistDir, "song.wav"), "");

        var parser = new FileSystemParser();
        var songSourcesMock = new Mock<ISongSources>();
        songSourcesMock.Setup(x => x.Sources).Returns(new List<string> { testRoot + Path.DirectorySeparatorChar });

        // Act
        var result = parser.ParseFileSystem(songSourcesMock.Object, "*");

        // Assert
        Assert.Empty(result);
    }
}
