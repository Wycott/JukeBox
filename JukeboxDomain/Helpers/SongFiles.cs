namespace JukeboxDomain.Helpers;

public static class SongFiles
{
    private static readonly string[] AudioExtensions = [".mp3", ".m4a"];

    public static bool IsAudioFile(string path)
    {
        var extension = Path.GetExtension(path);

        return AudioExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    // Finds audio files matching searchPattern in the subdirectories of source.
    // A missing source, or a subdirectory that can't be read, is skipped rather than reported.
    public static IEnumerable<string> Find(string source, string searchPattern)
    {
        foreach (var directory in GetDirectoriesOrEmpty(source))
        {
            foreach (var file in GetFilesOrEmpty(directory, searchPattern))
            {
                if (IsAudioFile(file))
                {
                    yield return file;
                }
            }
        }
    }

    private static string[] GetDirectoriesOrEmpty(string path)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return [];
        }
    }

    private static string[] GetFilesOrEmpty(string directory, string searchPattern)
    {
        try
        {
            return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return [];
        }
    }
}
