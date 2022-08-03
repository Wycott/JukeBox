using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;

namespace JukeboxLibrary.Helpers;

public static class FileSystemParser
{
    public static List<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString)
    {
        var retVal = new List<ISong>();

        // TODO: Maybe do this elsewhere
        huntString = PreparePattern(huntString);

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
                            var shortPath = possibleSongFile.Replace(drive, "");
                            retVal.Add(new Song() { FullPath = possibleSongFile, ShortenedPath = shortPath });
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Standard...
                }
            }
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
}