using JukeboxInterfaces;

namespace JukeboxDomain.Helpers;

public class FileSystemParser(IBanList banList) : IFileSystemParser
{
    public IReadOnlyList<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString)
    {
        var retVal = new List<ISong>();
        var artist = GetArtist(huntString);

        var haveArtist = artist.Length > 0;

        huntString = haveArtist ? "*" : PreparePattern(huntString);

        foreach (var drive in mediaDrives.Sources)
        {
            foreach (var possibleSongFile in SongFiles.Find(drive, huntString))
            {
                if (banList.IsBanned(possibleSongFile))
                {
                    continue;
                }

                if (!haveArtist || HaveASongByThisArtist(possibleSongFile, artist))
                {
                    var shortPath = possibleSongFile.Replace(drive, "");
                    retVal.Add(new Song { FullPath = possibleSongFile, ShortenedPath = shortPath });
                }
            }
        }

        return haveArtist ? retVal.OrderBy(_ => Random.Shared.Next()).ToList() : retVal;
    }

    private static string PreparePattern(string initialPattern)
    {
        return "*" + initialPattern.Replace(' ', '*') + "*";
    }

    private static string GetArtist(string initialPattern)
    {
        const string BandMarker = "@";

        if (!initialPattern.Contains(BandMarker))
        {
            return string.Empty;
        }

        var parts = initialPattern.Split(BandMarker);

        return parts[1];
    }

    private static bool HaveASongByThisArtist(string songPattern, string artist)
    {
        var directory = Path.GetDirectoryName(songPattern) ?? string.Empty;

        return directory.Contains(artist, StringComparison.OrdinalIgnoreCase);
    }
}