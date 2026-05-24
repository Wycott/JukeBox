namespace JukeboxInterfaces;

public interface IFileSystemParser
{
    List<ISong> ParseFileSystem(ISongSources mediaDrives, string huntString);
}
