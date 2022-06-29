using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts
{
    public class Song : ISong
    {
        public string FullPath { get; set; } = string.Empty;
    }
}
