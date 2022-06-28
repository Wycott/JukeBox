using JukeBoxLibrary.Domain;
using JukeBoxLibrary.Helpers;

namespace JukeBox
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