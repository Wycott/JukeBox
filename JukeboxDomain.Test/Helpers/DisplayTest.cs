using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test.Helpers;

public class DisplayTest
{
    [Fact]
    public void FlowerBox_WritesExpectedCharacterCount()
    {
        // Arrange
        const int ExpectedCharactersWritten = 222;
        var consoleEngineMock = new Mock<IConsoleEngine>();
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        displayEngine.FlowerBox();

        // Assert
        consoleEngineMock.Verify(x => x.WriteText(It.IsAny<string>()), Times.Exactly(ExpectedCharactersWritten));
    }

    [Fact]
    public void FlowerBox_SetsWhiteColourAndResets()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        displayEngine.FlowerBox();

        // Assert
        consoleEngineMock.VerifySet(x => x.TextColour = ConsoleColor.White, Times.AtLeastOnce());
        consoleEngineMock.Verify(x => x.ResetColour(), Times.Once);
    }

    [Fact]
    public void WriteYellowText_WritesInDarkYellow()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        displayEngine.WriteYellowText("Hello");

        // Assert
        consoleEngineMock.VerifySet(x => x.TextColour = ConsoleColor.DarkYellow, Times.Once());
        consoleEngineMock.Verify(x => x.WriteALine("Hello"), Times.Once);
    }

    [Fact]
    public void WriteError_WritesInRed()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        displayEngine.WriteError("Something went wrong");

        // Assert
        consoleEngineMock.VerifySet(x => x.TextColour = ConsoleColor.Red, Times.Once());
        consoleEngineMock.Verify(x => x.WriteALine("Something went wrong"), Times.Once);
    }

    [Fact]
    public void IsThisTheRightSong_WhenYPressed_ReturnsTrue()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        consoleEngineMock.Setup(x => x.ReadAKey())
            .Returns(new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        var result = displayEngine.IsThisTheRightSong("test song");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsThisTheRightSong_WhenNPressed_ReturnsFalse()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        consoleEngineMock.Setup(x => x.ReadAKey())
            .Returns(new ConsoleKeyInfo('n', ConsoleKey.N, false, false, false));
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        var result = displayEngine.IsThisTheRightSong("test song");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsThisTheRightSong_WhenOtherKeyPressed_ReturnsNull()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        consoleEngineMock.Setup(x => x.ReadAKey())
            .Returns(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        var result = displayEngine.IsThisTheRightSong("test song");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsThisTheRightSong_DisplaysCandidateName()
    {
        // Arrange
        var consoleEngineMock = new Mock<IConsoleEngine>();
        consoleEngineMock.Setup(x => x.ReadAKey())
            .Returns(new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));
        var displayEngine = new Display(consoleEngineMock.Object);

        // Act
        displayEngine.IsThisTheRightSong("My Song.mp3");

        // Assert
        consoleEngineMock.Verify(x => x.WriteALine("My Song.mp3"), Times.Once);
    }
}