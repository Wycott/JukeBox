using JukeboxDomain;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test;

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
