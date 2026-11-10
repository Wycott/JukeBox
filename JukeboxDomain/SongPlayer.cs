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
            try
            {
                StopAndDisposeCurrent();
            }
            catch (Exception)
            {
                // The device is already broken; there is nothing more to clean up.
            }

            DisplayEngine.WriteError("Internal error - please pick another song");

            return false;
        }
    }

    private void StopAndDisposeCurrent()
    {
        // Detach first so a failing Stop or Dispose can't leave stale objects behind
        // to be stopped again on the next call.
        var player = Player;
        var fileReader = FileReader;
        Player = null;
        FileReader = null;

        try
        {
            player?.Stop();
        }
        finally
        {
            try
            {
                player?.Dispose();
            }
            finally
            {
                fileReader?.Dispose();
            }
        }
    }

    public void Dispose()
    {
        StopAndDisposeCurrent();
        GC.SuppressFinalize(this);
    }
}
