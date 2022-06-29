using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JukeboxLibrary.Helpers;
using JukeboxLibrary.Types;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary
{
    public class JukeboxEngine : IJukeboxEngine
    {
        public ISongSources SongSources { get; }
        public ISongList SongList { get; }
        public ISongPlayer SongPlayer { get; }

        private JukeboxStateType JukeboxState { get; set; } = JukeboxStateType.Unknown;

        public JukeboxEngine(ISongSources songSources, ISongList songList, ISongPlayer songPlayer)
        {
            SongSources = songSources;
            SongList = songList;
            SongPlayer = songPlayer;
        }

        public void Start()
        {
            JukeboxState = JukeboxStateType.ShowTitleBox;
            LetTheMusicPlay();
        }

        private void LetTheMusicPlay() // Ru
        {
            string selectedPattern = string.Empty;
            string selectedSong = string.Empty;
            while (true)
            {
                switch (JukeboxState)
                {
                    case JukeboxStateType.Unknown:
                        break;
                    case JukeboxStateType.ShowTitleBox:
                        Display.FlowerBox();
                        JukeboxState = JukeboxStateType.RequestSong;
                        break;
                    case JukeboxStateType.RequestSong:
                        Console.Write("Enter song pattern: ");
                        selectedPattern = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(selectedPattern))
                        {
                            JukeboxState = JukeboxStateType.FindSong;
                        }
                        break;
                    case JukeboxStateType.FindSong:
                        SongList.Build(SongSources, selectedPattern);
                        if (SongList.SongCollection.Count > 0)
                        {
                            JukeboxState = JukeboxStateType.SelectVersion;
                        }
                        else
                        {
                            Display.WriteError("Not Found!");
                            JukeboxState = JukeboxStateType.RequestSong;
                        }
                        break;
                    case JukeboxStateType.SelectVersion:
                        foreach (var song in SongList.SongCollection)
                        {
                            var isRightSong = Display.IsThisTheRightSong(song.FullPath);
                            if (isRightSong)
                            {
                                selectedSong = song.FullPath;
                                JukeboxState = JukeboxStateType.PlaySong;
                                break;
                            }
                        }
                        if (JukeboxState != JukeboxStateType.PlaySong)
                        {
                            Display.WriteError("Nothing selected!");
                            JukeboxState = JukeboxStateType.RequestSong;
                        }
                        break;
                    case JukeboxStateType.PlaySong:
                        SongPlayer.PlaySong(selectedSong);
                        JukeboxState = JukeboxStateType.RequestSong;
                        break;
                    //case JukeboxStateType.StopSong:
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //private static void WriteText(string data)
        //{
        //    var c = Console.ForegroundColor;
        //    Console.ForegroundColor = ConsoleColor.White;
        //    Console.WriteLine(data);
        //    Console.ForegroundColor = c;
        //}
    }
}
