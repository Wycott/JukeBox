using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongList : ISongList
{
    public List<ISong> SongCollection { get; set; } = new();

    public void Build(ISongSources sources, string selectedPattern)
    {
        SongCollection = FileSystemParser.ParseFileSystem(sources, selectedPattern);
    }
}