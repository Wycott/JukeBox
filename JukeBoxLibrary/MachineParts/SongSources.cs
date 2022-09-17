
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts;

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
            @"E:\iTunes Music\",
            @"D:\iTunes Music\",
            @"D:\Program Files (x86)\Origin Games\The Sims 4\",
            @"C:\Users\rober\Music\"
        };
    }
}