using JukeboxLibrary;
using JukeboxLibrary.Helpers;
using JukeboxLibrary.Interfaces;
using JukeboxLibrary.MachineParts;
using Microsoft.Extensions.DependencyInjection;

namespace Jukebox;

internal static class Program
{
	// TODO : DI
	private static void Main()
	{
		var serviceProvider = new ServiceCollection()
			.AddSingleton<IConsoleEngine, ConsoleEngine>()
			.AddSingleton<IDisplay, Display>()
			.AddSingleton<IJukeboxEngine, JukeboxEngine>()
			.AddSingleton<ISongSources, SongSources>()
			.AddSingleton<ISongList, SongList>()
			.AddSingleton<ISongPlayer, SongPlayer>()
			.BuildServiceProvider();

		var engine = serviceProvider.GetService<IJukeboxEngine>();

		engine?.Start();

	}
}
