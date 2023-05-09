using JukeboxLibrary.Interfaces;
using NAudio.Wave;

namespace JukeboxLibrary.MachineParts;

public class SongPlayer : ISongPlayer
{
    private Mp3FileReader? FileReader { get; set; }
    private WaveOutEvent? Player { get; set; }

    // TODO: Might be able to interrogate the various objects but surely this is easier
    private bool SongPlaying { get; set; }

    private IDisplay DisplayEngine { get; }

	public SongPlayer(IDisplay displayEngine)
    {
        DisplayEngine = displayEngine;
    }

    public void PlaySong(string filename)
    {
        try
        {
            CheckForPlayingSong();
            FileReader = new Mp3FileReader(filename);
            Player = new WaveOutEvent();
            Player.Init(FileReader);
            Player.Play();
            SongPlaying = true;
        }
        catch (InvalidOperationException)
        {
	        DisplayEngine.WriteError("Internal error - please pick another song");
        }
    }

    private void CheckForPlayingSong()
    {
        if (!SongPlaying || Player == null)
        {
            return;
        }

        Player.Stop();
        // TODO: Dispose too probably
        SongPlaying = false;
    }
}