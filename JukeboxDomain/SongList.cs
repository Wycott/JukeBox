using JukeboxInterfaces;

namespace JukeboxDomain;

public class SongList : ISongList
{
    private readonly IFileSystemParser _fileSystemParser;

    public IReadOnlyList<ISong> SongCollection { get; private set; } = [];

    public SongList(IFileSystemParser fileSystemParser)
    {
        _fileSystemParser = fileSystemParser;
    }

    public void Build(ISongSources sources, string selectedPattern)
    {
        SongCollection = _fileSystemParser.ParseFileSystem(sources, selectedPattern);
    }
}