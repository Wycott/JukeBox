namespace JukeboxInterfaces;

public interface IFileSystemParser
{
    IReadOnlyList<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString);
}
