using JukeboxDomain.Helpers;
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
        return SongFiles.Find(source, "*").Count();
    }
}