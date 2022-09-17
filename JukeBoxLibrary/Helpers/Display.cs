using static System.Console;
namespace JukeboxLibrary.Helpers;

public static class Display
{
    private static int currentColour;

    // Yes, could have enumerated console colours but some of them aren't very bright
    private static readonly List<ConsoleColor> BrightColours = new()
    {
        ConsoleColor.Blue,
        ConsoleColor.Green,
        ConsoleColor.Cyan,
        ConsoleColor.Red,
        ConsoleColor.Magenta,
        ConsoleColor.Yellow,
        ConsoleColor.White
    };

    public static void FlowerBox()
    {
        ForegroundColor = ConsoleColor.White;

        WriteText();

        Console.ResetColor();
    }

    public static void WriteYellowText(string data)
    {
        WriteColourText(data, ConsoleColor.DarkYellow);
    }

    public static bool? IsThisTheRightSong(string candidate)
    {
        Write("Found: ");
        WriteYellowText(candidate);
        Write("Play y/n? ");
        var consoleInput = ReadKey();
        WriteLine();

        return consoleInput.Key switch
        {
            ConsoleKey.Y => true,
            ConsoleKey.N => false,
            _ => null
        };
    }

    public static void WriteError(string errorMessage)
    {
        WriteColourText(errorMessage, ConsoleColor.Red);
        WriteLine();
    }

    private static void WriteColourText(string data, ConsoleColor colour)
    {
        var currentConsoleColour = ForegroundColor;
        ForegroundColor = colour;
        WriteLine(data);
        ForegroundColor = currentConsoleColour;
    }

    private static void WriteText()
    {
        var lines = new List<string> {
            "",
            " ************************************",
            " *                                  *",
            " *   Robbie Dee's Console Jukebox   *",
            " *  © 2014-2022 Rogedo Consultants  *",
            " *                                  *",
            " ************************************",
            ""
        };

        WriteLines(lines, colouredText: true);
    }

    private static void WriteLines(IEnumerable<string> data, bool colouredText = false)
    {
        foreach (var line in data)
        {
            Write(line, colouredText);
        }
    }

    private static void Write(string line, bool colouredText = false)
    {
        if (!colouredText)
        {
            WriteLine(line);

            return;
        }

        foreach (var c in line)
        {
            var printString = c.ToString();
            ForegroundColor = BrightColours[currentColour];
            Console.Write(c.ToString());

            if (printString != " ")
                currentColour++;

            if (currentColour == BrightColours.ToList().Count) currentColour = 0;
        }

        WriteLine();
    }
}