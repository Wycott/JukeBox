namespace JukeboxInterfaces;

public interface ISongPlayer : IDisposable
{
    void PlaySong(string filename);
}