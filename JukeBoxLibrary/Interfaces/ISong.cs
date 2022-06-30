namespace JukeboxLibrary.Interfaces;

public interface ISong
{
    string FullPath { get; set; }
    string FileName { get; }
}