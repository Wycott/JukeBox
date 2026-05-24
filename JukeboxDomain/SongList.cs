using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongList(IFileSystemParser fileSystemParser) : ISongList
{
    public IReadOnlyList<ISong> SongCollection { get; private set; } = [];

    public void Build(ISongSources sources, string selectedPattern)
    {
        SongCollection = fileSystemParser.ParseFileSystem(sources, selectedPattern);
    }
}