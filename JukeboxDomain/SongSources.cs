using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongSources : ISongSources
{
    private readonly IDisplay _display;

    public List<string> Sources { get; set; } = new();

    public SongSources(IDisplay display, List<string> sources)
    {
        _display = display;
        Sources = sources;
        DisplaySongCounts();
    }

    private void DisplaySongCounts()
    {
        foreach (var source in Sources)
        {
            var count = CountSongsInSource(source);
            _display.WriteYellowText($"{source}: {count} songs");
        }
    }

    private int CountSongsInSource(string source)
    {
        var count = 0;
        var extensions = new[] { ".mp3", ".m4a" };

        try
        {
            var directories = Directory.GetDirectories(source);
            foreach (var directory in directories)
            {
                try
                {
                    var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                    count += files.Count(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
        }

        return count;
    }
}