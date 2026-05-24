using AiAnnotations;
using JukeboxDomain.Helpers;

namespace JukeboxDomain.Test.Helpers;

[AiGenerated]
public class ConsoleEngineTest
{
    [Fact]
    public void WriteALine_WithText_WritesToConsole()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            consoleEngine.WriteALine("Test message");

            // Assert
            Assert.Equal("Test message\r\n", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void WriteALine_WithoutText_WritesEmptyLine()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            consoleEngine.WriteALine();

            // Assert
            Assert.Equal("\r\n", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void WriteText_WritesToConsoleWithoutNewline()
    {
        // Arrange
        var consoleEngine = new ConsoleEngine();
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act
            consoleEngine.WriteText("Test");

            // Assert
            Assert.Equal("Test", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
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
        var originalIn = Console.In;
        var input = new StringReader("Test input\n");
        Console.SetIn(input);

        try
        {
            // Act
            var result = consoleEngine.ReadLine();

            // Assert
            Assert.Equal("Test input", result);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }
}
