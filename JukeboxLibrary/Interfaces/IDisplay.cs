using JukeboxLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeboxLibrary.Interfaces
{
	public interface IDisplay
	{
		void FlowerBox();

		void WriteYellowText(string data);

		bool? IsThisTheRightSong(string candidate);

		void WriteError(string errorMessage);
	}
}
