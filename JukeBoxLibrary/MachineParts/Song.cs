using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary.MachineParts
{
    public class Song : ISong
    {
        public string FullPath { get; set; }
        public bool EverPlayed { get; set; }
    }
}
