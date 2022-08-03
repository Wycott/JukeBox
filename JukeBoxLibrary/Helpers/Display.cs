using static System.Console;
namespace JukeboxLibrary.Helpers;

public static class Display
{
    public static void FlowerBox()
    {
        var c = ForegroundColor;
        ForegroundColor = ConsoleColor.White;

        WriteText();

        ForegroundColor = c;
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
        var cki = ReadKey();
        WriteLine();

        return cki.Key switch
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
        var c = ForegroundColor;
        ForegroundColor = colour;
        WriteLine(data);
        ForegroundColor = c;
    }

    private static void WriteText()
    {
        var lines = new List<string> {
            "************************************",
            "*                                  *",
            "*   Robbie Dee's Console Jukebox   *",
            "*  © 2014-2022 Rogedo Consultants  *",
            "*                                  *",
            "************************************",
            ""
        };

        WriteLines(lines);
    }

    private static void WriteLines(IEnumerable<string> data)
    {
        foreach (var s in data)
        {
            Write(s);
        }
    }

    private static void Write(string s)
    {
        WriteLine(s);
    }
}