using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeboxLibrary.Types
{
    public enum JukeboxStateType
    {
        Unknown,
        ShowTitleBox,
        RequestSong,
        FindSong,
        SelectVersion,
        //ShowStats,
        PlaySong,
        //StopSong
    }
}
