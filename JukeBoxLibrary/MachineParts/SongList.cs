using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JukeboxLibrary.Helpers;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts
{
    public class SongList : ISongList
    {
        public List<ISong> SongCollection { get; set; }

        public void Build(ISongSources sources, string selectedPattern)
        {
            SongCollection = FileSystemParser.ParseFileSystem(sources, selectedPattern);
        }
    }
}
