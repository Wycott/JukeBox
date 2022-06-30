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
            foreach (var possibleSongDirectory in Directory.GetDirectories(drive))
            {
                try
                {
                    foreach (var possibleSongFile in Directory.GetFiles(possibleSongDirectory, huntString, SearchOption.AllDirectories))
                    {
                        if (ExtensionsOk(possibleSongFile))
                        {

                            retVal.Add(new Song() { FullPath = possibleSongFile });
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
        var extensions = new List<string> { ".mp3", ".m4a" };

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