using JukeboxInterfaces;

namespace JukeboxDomain;

public class Song : ISong
{
    public string FullPath { get; init; } = string.Empty;

    public string ShortenedPath { get; init; } = string.Empty;

    public string FileName => Path.GetFileName(FullPath);
}