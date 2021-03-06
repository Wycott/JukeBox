using JukeboxLibrary;
using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;

namespace Jukebox;

internal static class Program
{
    // TODO : DI
    private static void Main()
    {
        IJukeboxEngine engine = new JukeboxEngine(
            new SongSources(),
            new SongList(),
            new SongPlayer());

        engine.Start();
    }
}
