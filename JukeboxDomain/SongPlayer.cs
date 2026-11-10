using JukeboxInterfaces;
using NAudio.Wave;

namespace JukeboxDomain;

public class SongPlayer(IDisplay displayEngine) : ISongPlayer, IDisposable
{
    private AudioFileReader? FileReader { get; set; }
    private WaveOutEvent? Player { get; set; }

    private IDisplay DisplayEngine { get; } = displayEngine;

    public bool PlaySong(string filename)
    {
        try
        {
            StopAndDisposeCurrent();
            FileReader = new AudioFileReader(filename);
            Player = new WaveOutEvent();
            Player.Init(FileReader);
            Player.Play();

            return true;
        }
        catch (Exception)
        {
            // Playback is a boundary: unreadable files, unsupported formats, missing drives
            // and audio device errors should never take the jukebox down.
            StopAndDisposeCurrent();
            DisplayEngine.WriteError("Internal error - please pick another song");

            return false;
        }
    }

    private void StopAndDisposeCurrent()
    {
        if (Player != null)
        {
            Player.Stop();
            Player.Dispose();
            Player = null;
        }

        if (FileReader != null)
        {
            FileReader.Dispose();
            FileReader = null;
        }
    }

    public void Dispose()
    {
        StopAndDisposeCurrent();
        GC.SuppressFinalize(this);
    }
}
