﻿using JukeboxLibrary.Interfaces;
using NAudio.Wave;

namespace JukeboxLibrary.MachineParts
{
    public class SongPlayer : ISongPlayer
    {
        private Mp3FileReader? FileReader { get; set; }
        private WaveOutEvent? Player { get; set; }

        // TODO: Might be able to interrogate the various objects but surely this is easier
        private bool SongPlaying { get; set; }

        public void PlaySong(string filename)
        {
            CheckForPlayingSong();
            FileReader = new Mp3FileReader(filename);
            Player = new WaveOutEvent();
            Player.Init(FileReader);
            Player.Play();
            SongPlaying = true;
        }

        private void CheckForPlayingSong()
        {
            if (SongPlaying && Player != null)
            {
                Player.Stop();
                SongPlaying = false;
            }
        }
    }
}