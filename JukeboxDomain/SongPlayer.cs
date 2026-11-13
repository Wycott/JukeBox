using JukeboxInterfaces;
using System.Runtime.Versioning;
using NAudio.Wave;

namespace JukeboxDomain;

[SupportedOSPlatform("windows")]
public class SongPlayer(IDisplay displayEngine) : ISongPlayer
{
    private AudioFileReader? FileReader { get; set; }
    private WaveOut? Player { get; set; }

    private IDisplay DisplayEngine { get; } = displayEngine;

    public event EventHandler? SongFinished;

    public bool PlaySong(string filename)
    {
        try
        {
            StopAndDisposeCurrent();
            FileReader = new AudioFileReader(filename);
            Player = new WaveOut();
            Player.PlaybackStopped += OnPlaybackStopped;
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

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        // Ignore the stop if it came from a player that has since been replaced.
        if (!ReferenceEquals(sender, Player))
        {
            return;
        }

        // The event arrives on the audio callback thread, where disposing the device
        // (which playing the next song does) can deadlock. Hand it off instead.
        Task.Run(() => SongFinished?.Invoke(this, EventArgs.Empty));
    }

    private void StopAndDisposeCurrent()
    {
        // Detach first so a failing Stop or Dispose can't leave stale objects behind
        // to be stopped again on the next call.
        var player = Player;
        var fileReader = FileReader;
        Player = null;
        FileReader = null;

        // Stopping on purpose is not "finished", so don't let Stop raise it.
        if (player != null)
        {
            player.PlaybackStopped -= OnPlaybackStopped;
        }

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
