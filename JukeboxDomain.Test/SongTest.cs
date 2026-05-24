using JukeboxDomain;

namespace JukeboxDomain.Test;

public class SongTest
{
    [Fact]
    public void FileName_ReturnsFileNameFromFullPath()
    {
        // Arrange
        var song = new Song { FullPath = @"C:\Music\Artist\Album\Track.mp3" };

        // Act & Assert
        Assert.Equal("Track.mp3", song.FileName);
    }

    [Fact]
    public void FileName_WithForwardSlashes_ReturnsFileName()
    {
        // Arrange
        var song = new Song { FullPath = "C:/Music/Artist/Track.mp3" };

        // Act & Assert
        Assert.Equal("Track.mp3", song.FileName);
    }

    [Fact]
    public void FileName_WhenFullPathIsEmpty_ReturnsEmpty()
    {
        // Arrange
        var song = new Song();

        // Act & Assert
        Assert.Equal(string.Empty, song.FileName);
    }

    [Fact]
    public void FileName_WithNoDirectory_ReturnsFullPath()
    {
        // Arrange
        var song = new Song { FullPath = "Track.mp3" };

        // Act & Assert
        Assert.Equal("Track.mp3", song.FileName);
    }

    [Fact]
    public void ShortenedPath_DefaultsToEmpty()
    {
        // Arrange & Act
        var song = new Song();

        // Assert
        Assert.Equal(string.Empty, song.ShortenedPath);
    }

    [Fact]
    public void Properties_CanBeSetViaInit()
    {
        // Arrange & Act
        var song = new Song
        {
            FullPath = @"C:\Music\song.mp3",
            ShortenedPath = @"Music\song.mp3"
        };

        // Assert
        Assert.Equal(@"C:\Music\song.mp3", song.FullPath);
        Assert.Equal(@"Music\song.mp3", song.ShortenedPath);
    }
}
