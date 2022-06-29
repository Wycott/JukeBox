using JukeboxLibrary;
using JukeboxLibrary.Domain;
using JukeboxLibrary.Helpers;
using JukeboxLibrary.Interfaces;

namespace Jukebox
{
    internal static class Program
    {
        private static void Main()
        {
            //Display.FlowerBox();
            //new Shell().Start();
            IJukeboxEngine engine = new JukeboxEngine();
            engine.Start();
        }
    }
}