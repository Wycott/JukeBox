using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeboxLibrary.Interfaces
{
    public interface ISong
    {
        string FullPath{ get; set; }
        bool EverPlayed { get; set; }
    }
}
