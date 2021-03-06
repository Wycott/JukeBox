// TODO: Static to reduce noise
namespace JukeboxLibrary.Helpers;

public static class Display
{
    public static void FlowerBox()
    {
        var c = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;

        WriteText();

        Console.ForegroundColor = c;
    }

    public static void WriteYellowText(string data)
    {
        WriteColourText(data, ConsoleColor.DarkYellow);
    }

    public static bool? IsThisTheRightSong(string candidate)
    {
        Console.Write("Found: ");
        WriteYellowText(candidate);
        Console.Write("Play y/n? ");
        var cki = Console.ReadKey();
        Console.WriteLine();

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
        Console.WriteLine();
    }

    private static void WriteColourText(string data, ConsoleColor colour)
    {
        var c = Console.ForegroundColor;
        Console.ForegroundColor = colour;
        Console.WriteLine(data);
        Console.ForegroundColor = c;
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
        Console.WriteLine(s);
    }
}