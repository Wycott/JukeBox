namespace JukeboxLibrary.Interfaces;

public interface ISong
{
    string FullPath { get; set; }
    string ShortenedPath { get; set; }
    string FileName { get; }
}