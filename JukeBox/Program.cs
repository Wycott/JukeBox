using JukeboxDomain;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using JukeboxLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace Jukebox;

internal static class Program
{
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
