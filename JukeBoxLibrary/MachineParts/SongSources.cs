using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts
{
    public class SongSources : ISongSources
    {
        public List<string> Sources { get; set; }

        public SongSources()
        {
            Init();
        }

        private void Init()
        {
            // TODO: Should read from config or similar
            Sources = new List<string>()
            {
                @"e:\"
            };
        }
        
    }
}
