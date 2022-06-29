using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts
{
    public class SongSources : ISongSources
    {
        public List<string> Sources { get; set; } = new();

        public SongSources()
        {
            Init();
        }

        private void Init()
        {
            // TODO: Should read from config or similar
            Sources = new List<string>()
            {
                @"e:\"
            };
        }
    }
}
