using JukeboxInterfaces;

namespace JukeboxDomain;

public class Song : ISong
{
    public string FullPath { get; set; } = string.Empty;

    public string ShortenedPath { get; set; } = string.Empty;

    public string FileName
    {
        get
        {
            var parts = FullPath.Split('\\');

            return parts[^1];
        }
    }
}