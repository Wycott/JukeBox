using JukeboxInterfaces;
using Moq;
using System.Runtime.Versioning;

namespace JukeboxDomain.Test;

[SupportedOSPlatform("windows")]
public class SongPlayerTest
{
    [Fact]
    public void PlaySong_WithInvalidFile_ShowsError()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);

        // Act
        player.PlaySong("nonexistent_file.mp3");

        // Assert
        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void PlaySong_WithEmptyPath_ShowsError()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);

        // Act
        player.PlaySong("");

        // Assert
        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void PlaySong_WithMissingFile_ReturnsFalse()
    {
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);

        var result = player.PlaySong("nonexistent_file.mp3");

        Assert.False(result);
    }

    [Fact]
    public void PlaySong_WithUndecodableFile_ShowsErrorAndDoesNotThrow()
    {
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);
        var tempFile = Path.Combine(Path.GetTempPath(), $"not_audio_{Guid.NewGuid():N}.mp3");
        File.WriteAllText(tempFile, "this is not audio data");

        try
        {
            var result = player.PlaySong(tempFile);

            Assert.False(result);
            displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var player = new SongPlayer(displayMock.Object);

        // Act & Assert - should not throw
        player.Dispose();
        player.Dispose();
    }

    [Fact]
    public void Dispose_WhenNoSongPlayed_DoesNotThrow()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        var player = new SongPlayer(displayMock.Object);

        // Act & Assert
        var exception = Record.Exception(() => player.Dispose());

        Assert.Null(exception);
    }
}

[SupportedOSPlatform("windows")]
public class SongPlayerSequentialTest
{
    [Fact]
    public void PlaySong_CalledTwiceWithInvalidFiles_ShowsErrorTwice()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);

        // Act
        player.PlaySong("first_missing.mp3");
        player.PlaySong("second_missing.mp3");

        // Assert
        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void PlaySong_WithNullPath_ShowsError()
    {
        // Arrange
        var displayMock = new Mock<IDisplay>();
        using var player = new SongPlayer(displayMock.Object);

        // Act
        player.PlaySong(null!);

        // Assert
        displayMock.Verify(x => x.WriteError(It.IsAny<string>()), Times.Once);
    }
}
