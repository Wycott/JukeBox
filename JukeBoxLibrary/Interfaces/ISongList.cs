using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeboxLibrary.Interfaces
{
    public interface ISongList
    {
        List<ISong> SongCollection { get; set; }
        void Build(ISongSources sources, string selectedPattern);
    }
}
