using JukeboxInterfaces;

namespace JukeboxDomain.Helpers;

public class FileSystemParser : IFileSystemParser
{
    public IReadOnlyList<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString)
    {
        var retVal = new List<ISong>();
        var artist = GetArtist(huntString);

        var haveArtist = artist.Length > 0;

        huntString = artist.Length > 0 ? "*" : PreparePattern(huntString);

        foreach (var drive in mediaDrives.Sources)
        {
            string[]? directories;
            try
            {
                directories = Directory.GetDirectories(drive);
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (IOException)
            {
                continue;
            }

            foreach (var possibleSongDirectory in directories)
            {
                try
                {
                    foreach (var possibleSongFile in Directory.GetFiles(possibleSongDirectory, huntString, SearchOption.AllDirectories))
                    {
                        if (ExtensionsOk(possibleSongFile))
                        {
                            if (!haveArtist || HaveASongByThisArtist(possibleSongFile, artist))
                            {
                                var shortPath = possibleSongFile.Replace(drive, "");
                                retVal.Add(new Song { FullPath = possibleSongFile, ShortenedPath = shortPath });
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Standard...
                }
                catch (IOException)
                {
                    // Network or I/O issue with subdirectory
                }
            }
        }

        return haveArtist ? retVal.OrderBy(_ => Random.Shared.Next()).ToList() : retVal;
    }

    private static readonly string[] ValidExtensions = [".mp3", ".m4a"];

    private static bool ExtensionsOk(string candidate)
    {
        var extension = Path.GetExtension(candidate);

        return ValidExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
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