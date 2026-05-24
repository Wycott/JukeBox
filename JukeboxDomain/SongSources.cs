using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongSources(IDisplay display, List<string> sources) : ISongSources
{
    private bool songCountsDisplayed;

    public IReadOnlyList<string> Sources { get; } = sources;

    public void DisplaySongCounts()
    {
        if (songCountsDisplayed)
        {
            return;
        }

        songCountsDisplayed = true;

        foreach (var source in Sources)
        {
            var count = CountSongsInSource(source);
            display.WriteYellowText($"{source}: {count} songs");
        }
    }

    private static int CountSongsInSource(string source)
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
                catch (IOException)
                {
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }

        return count;
    }
}