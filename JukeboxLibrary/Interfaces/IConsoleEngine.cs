namespace JukeboxLibrary.Interfaces;

public interface IConsoleEngine
{
    void WriteALine(string text);
    void WriteALine();
    void WriteText(string text);
    ConsoleKeyInfo ReadAKey();
    ConsoleColor TextColour { get; set; }
}