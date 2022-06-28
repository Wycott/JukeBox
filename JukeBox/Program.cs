using JukeboxLibrary.Domain;
using JukeboxLibrary.Helpers;

namespace Jukebox
{
    internal static class Program
    {
        private static void Main()
        {
            Display.FlowerBox();
            new Shell().Start();
        }
    }
}