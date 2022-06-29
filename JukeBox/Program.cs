using JukeboxLibrary;
using JukeboxLibrary.Domain;
using JukeboxLibrary.Helpers;
using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;

namespace Jukebox
{
    internal static class Program
    {
        private static void Main()
        {
            //Display.FlowerBox();
            //new Shell().Start();
            IJukeboxEngine engine = new JukeboxEngine(new SongSources(), new SongList(), new SongPlayer());
            engine.Start();
        }
    }
}