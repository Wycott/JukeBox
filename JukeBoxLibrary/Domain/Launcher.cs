//using System.Windows.Media;

using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace JukeBoxLibrary.Domain
{
    internal class Launcher : IController
    {
        private int _songCount;

        public string SongFile { get; set; }
        public string NextOne { get; set; }

        internal Launcher(string argument)
        {
            SongFile = argument;
        }

        public void Start()
        {
            PlaySong(SongFile);
        }

        public static void PlaySong(string path)
        {
            var reader = new Mp3FileReader(path);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
        }

        public void Next()
        {
            var s = new Shell(NextOne);
            s.Start();
        }

        private void DisplayOutput()
        {
            Console.Write("Now playing: ");
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Stripper(SongFile));
            Console.ForegroundColor = c;

            _songCount++;
            Console.WriteLine("Songs played so far: {0}", _songCount);

            Console.WriteLine();
            Console.Write("Enter song pattern: ");
            NextOne = Console.ReadLine();
        }

        private static string Stripper(string song)
        {
            return song.Substring(song.LastIndexOf('\\') + 1);
        }
    }
}
