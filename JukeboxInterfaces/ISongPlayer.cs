namespace JukeboxInterfaces;

public interface ISongPlayer : IDisposable
{
    bool PlaySong(string filename);
}