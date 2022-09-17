﻿using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;

namespace JukeboxLibrary.Helpers;

public static class FileSystemParser
{
    public static List<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString)
    {
        var retVal = new List<ISong>();
        var artist = GetArtist(huntString);

        bool haveArtist = artist.Length > 0;

        // TODO: Maybe do this elsewhere
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
                                retVal.Add(new Song() { FullPath = possibleSongFile, ShortenedPath = shortPath });
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Standard...
                }
            }
        }

        if (haveArtist)
        {
            return retVal.OrderBy(g => Guid.NewGuid()).ToList();
        }

        return retVal;
    }

    private static bool ExtensionsOk(string candidate)
    {
        var extensions = new List<string> { ".mp3" };

        foreach (var s in extensions)
        {
            if (candidate.Contains(s))
            {
                return true;
            }
        }

        return false;
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
        else
        {
            var parts = initialPattern.Split(BandMarker);
            return parts[1];
        }
    }

    private static bool HaveASongByThisArtist(string songPattern, string artist)
    {
        var nonSongSection = songPattern.Substring(0, songPattern.LastIndexOf("\\", StringComparison.Ordinal));

        return nonSongSection.ToUpper().Contains(artist.ToUpper());
    }
}