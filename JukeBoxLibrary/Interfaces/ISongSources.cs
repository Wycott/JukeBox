using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeboxLibrary.Interfaces
{
    public interface ISongSources
    {
        List<string> Sources { get; set; }
    }
}
