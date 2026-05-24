namespace JukeboxInterfaces;

public interface ISong
{
    string FullPath { get; }
    string ShortenedPath { get; }
    string FileName { get; }
}