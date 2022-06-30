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

    public static void WriteText(string data)
    {
        var c = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(data);
        Console.ForegroundColor = c;
    }

    public static bool IsThisTheRightSong(string candidate)
    {
        Console.Write("Found: ");
        WriteText(candidate);
        Console.Write("Play this y/n? ");
        var cki = Console.ReadKey();
        Console.WriteLine();
        return (cki.Key == ConsoleKey.Y);
    }

    public static void WriteError(string errorMessage)
    {
        var c = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.WriteLine();
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