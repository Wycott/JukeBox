using JukeboxDomain;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;
using JukeboxLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jukebox;

internal static class Program
{
    private static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var songSourcePaths = configuration.GetSection("SongSources").Get<List<string>>() ?? [];

        var favouritesPath = Path.Combine(AppContext.BaseDirectory, "favourites.json");

        using var serviceProvider = new ServiceCollection()
            .AddSingleton<IConsoleEngine, ConsoleEngine>()
            .AddSingleton<IDisplay, Display>()
            .AddSingleton<IFileSystemParser, FileSystemParser>()
            .AddSingleton<IFavourites>(new Favourites(favouritesPath))
            .AddSingleton<IJukeboxEngine, JukeboxEngine>()
            .AddSingleton<ISongSources>(sp => new SongSources(sp.GetRequiredService<IDisplay>(), songSourcePaths))
            .AddSingleton<ISongList, SongList>()
            .AddSingleton<ISongPlayer, SongPlayer>()
            .BuildServiceProvider();

        var engine = serviceProvider.GetRequiredService<IJukeboxEngine>();

        engine.Start();
    }
}