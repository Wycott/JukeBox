namespace JukeboxInterfaces;

public interface ISongPlayer : IDisposable
{
    bool PlaySong(string filename);

    // Raised when a song plays through to the end. Not raised when a song is stopped
    // to play another one, or when the player is disposed. May be raised on a background thread.
    event EventHandler? SongFinished;
}