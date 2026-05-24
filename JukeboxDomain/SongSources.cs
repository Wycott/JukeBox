using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongSources : ISongSources
{
    private readonly IDisplay _display;

    public List<string> Sources { get; set; } = new();

    public SongSources(IDisplay display)
    {
        _display = display;
        Init();
    }

    private void Init()
    {
        // TODO: Should read from config or similar
        Sources = new List<string>()
        {
            @"E:\iTunes Music\",
            //@"D:\iTunes Music\",
            //@"D:\Program Files (x86)\Origin Games\The Sims 4\",
            @"C:\Users\rober\Music\",
            @"C:\RobMusic\VlcConversions\"
        };

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