namespace JukeboxInterfaces;

public interface IConsoleEngine
{
    void WriteALine();
    void ResetColour();
    void WriteText(string text);
    void WriteALine(string text);
    string? ReadLine();
    ConsoleKeyInfo ReadAKey();
    ConsoleColor TextColour { get; set; }
}