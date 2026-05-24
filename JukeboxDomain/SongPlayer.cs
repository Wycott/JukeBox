using JukeboxInterfaces;
using NAudio.Wave;

namespace JukeboxDomain;

public class SongPlayer(IDisplay displayEngine) : ISongPlayer, IDisposable
{
    private Mp3FileReader? FileReader { get; set; }
    private WaveOutEvent? Player { get; set; }

    private IDisplay DisplayEngine { get; } = displayEngine;

    public void PlaySong(string filename)
    {
        try
        {
            StopAndDisposeCurrent();
            FileReader = new Mp3FileReader(filename);
            Player = new WaveOutEvent();
            Player.Init(FileReader);
            Player.Play();
        }
        catch (Exception ex) when (ex is InvalidOperationException
                                       or FileNotFoundException
                                       or ArgumentException)
        {
            DisplayEngine.WriteError("Internal error - please pick another song");
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