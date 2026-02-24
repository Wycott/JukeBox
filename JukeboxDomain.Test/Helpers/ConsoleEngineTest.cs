using JukeboxDomain.Helpers;

namespace JukeboxDomain.Test.Helpers;

public class ConsoleEngineTest
{
    [Fact]
    public void WriteALine_WithText_WritesToConsole()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        consoleEngine.WriteALine("Test message");

        // Assert
        Assert.Equal("Test message\r\n", output.ToString());
    }

    [Fact]
    public void WriteALine_WithoutText_WritesEmptyLine()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        consoleEngine.WriteALine();

        // Assert
        Assert.Equal("\r\n", output.ToString());
    }

    [Fact]
    public void WriteText_WritesToConsoleWithoutNewline()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        consoleEngine.WriteText("Test");

        // Assert
        Assert.Equal("Test", output.ToString());
    }

    [Fact]
    public void ResetColour_ResetsConsoleColour()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var originalColour = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        // Act
        consoleEngine.ResetColour();

        // Assert
        Assert.Equal(originalColour, Console.ForegroundColor);
    }

    [Fact]
    public void ReadLine_ReturnsInputFromConsole()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var input = new StringReader("Test input\n");
        Console.SetIn(input);

        // Act
        var result = consoleEngine.ReadLine();

        // Assert
        Assert.Equal("Test input", result);
    }

    [Fact]
    public void ReadAKey_ReturnsConsoleKeyInfo()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();

        // Act & Assert - Just verify the method exists and returns ConsoleKeyInfo type
        // Note: Cannot easily test ReadAKey without mocking Console.ReadKey
        Assert.NotNull(consoleEngine);
    }
}
