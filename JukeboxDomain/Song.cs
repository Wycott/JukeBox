using JukeboxInterfaces;

namespace JukeboxDomain;

public class Song : ISong
{
    public string FullPath { get; set; } = string.Empty;

    public string ShortenedPath { get; set; } = string.Empty;

    public string FileName => Path.GetFileName(FullPath);
}