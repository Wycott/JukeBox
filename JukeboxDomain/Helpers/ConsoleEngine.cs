using JukeboxInterfaces;

namespace JukeboxDomain.Helpers;

public class ConsoleEngine : IConsoleEngine
{
    public void WriteALine(string text)
    {
        Console.WriteLine(text);
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void WriteALine()
    {
        Console.WriteLine();
    }

    public void WriteText(string text)
    {
        Console.Write(text);
    }

    public ConsoleKeyInfo ReadAKey()
    {
        return Console.ReadKey();
    }

    public ConsoleColor TextColour
    {
        get => Console.ForegroundColor;

        set => Console.ForegroundColor = value;
    }

    public void ResetColour()
    {
        Console.ResetColor();
    }
}