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
        private JukeboxStateType JukeboxState { get; set; } = JukeboxStateType.Unknown;

        public void Start()
        {
            JukeboxState = JukeboxStateType.ShowTitleBox;
            LetTheMusicPlay();
        }

        private void LetTheMusicPlay() // Ru
        {
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
                        Console.WriteLine("I'm here!");
                        return; // for now...
                        break;
                    case JukeboxStateType.ShowStats:
                        break;
                    case JukeboxStateType.PlayingSong:
                        break;
                    case JukeboxStateType.StopSong:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
