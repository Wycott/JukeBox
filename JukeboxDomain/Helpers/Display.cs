using JukeboxInterfaces;

namespace JukeboxDomain.Helpers;

public class Display : IDisplay
{
    private IConsoleEngine ConsoleEngine { get; }

    public Display(IConsoleEngine consoleEngine)
    {
        ConsoleEngine = consoleEngine;
    }

    private int _currentColour;

    // Yes, could have enumerated console colours but some of them aren't very bright
    private static readonly ConsoleColor[] BrightColours =
    {
        ConsoleColor.Blue,
        ConsoleColor.Green,
        ConsoleColor.Cyan,
        ConsoleColor.Red,
        ConsoleColor.Magenta,
        ConsoleColor.Yellow,
        ConsoleColor.White
    };

    public void FlowerBox()
    {
        ConsoleEngine.TextColour = ConsoleColor.White;

        WriteText();

        ConsoleEngine.ResetColour();
    }

    public void WriteYellowText(string data)
    {
        WriteColourText(data, ConsoleColor.DarkYellow);
    }

    public bool? IsThisTheRightSong(string candidate)
    {
        Write("Found: ");
        WriteYellowText(candidate);
        Write("Play y/n? ");
        var consoleInput = ConsoleEngine.ReadAKey();
        ConsoleEngine.WriteALine();

        return consoleInput.Key switch
        {
            ConsoleKey.Y => true,
            ConsoleKey.N => false,
            _ => null
        };
    }

    public void WriteError(string errorMessage)
    {
        WriteColourText(errorMessage, ConsoleColor.Red);
        ConsoleEngine.WriteALine();
    }

    private void WriteColourText(string data, ConsoleColor colour)
    {
        var currentConsoleColour = ConsoleEngine.TextColour;
        ConsoleEngine.TextColour = colour;
        ConsoleEngine.WriteALine(data);
        ConsoleEngine.TextColour = currentConsoleColour;
    }

    private void WriteText()
    {
        string[] lines = [
            "",
            " ************************************",
            " *                                  *",
            " *   Robbie Dee's Console Jukebox   *",
            " *  © 2014-2026 Rogedo Consultants  *",
            " *                                  *",
            " ************************************",
            ""
        ];

        WriteLines(lines, colouredText: true);
    }

    private void WriteLines(IEnumerable<string> data, bool colouredText = false)
    {
        foreach (var line in data)
        {
            Write(line, colouredText);
        }
    }

    private void Write(string line, bool colouredText = false)
    {
        if (!colouredText)
        {
            ConsoleEngine.WriteALine(line);

            return;
        }

        foreach (var c in line)
        {
            ConsoleEngine.TextColour = BrightColours[_currentColour];
            ConsoleEngine.WriteText(c.ToString());

            if (c != ' ')
            {
                _currentColour++;
            }

            if (_currentColour == BrightColours.Length)
            {
                _currentColour = 0;
            }
        }

        ConsoleEngine.WriteALine();
    }
}