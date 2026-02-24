using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using Moq;

namespace JukeboxDomain.Test.Helpers;

public class DisplayTest
{
    [Fact]
    public void WhenFlowerBoxIsCalled_ThenCharactersDisplayedShouldBeAsExpected()
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
}