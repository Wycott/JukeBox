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

        var maxFavourites = configuration.GetValue("Favourites:MaxStored", 100);
        var favouritesPageSize = configuration.GetValue("Favourites:PageSize", 20);
        var favouritesPath = Path.Combine(AppContext.BaseDirectory, "favourites.json");

        using var serviceProvider = new ServiceCollection()
            .AddSingleton<IConsoleEngine, ConsoleEngine>()
            .AddSingleton<IDisplay, Display>()
            .AddSingleton<IFileSystemParser, FileSystemParser>()
            .AddSingleton<IFavourites>(new Favourites(favouritesPath, maxFavourites))
            .AddSingleton<IJukeboxEngine>(sp => new JukeboxEngine(
                sp.GetRequiredService<ISongSources>(),
                sp.GetRequiredService<ISongList>(),
                sp.GetRequiredService<ISongPlayer>(),
                sp.GetRequiredService<IDisplay>(),
                sp.GetRequiredService<IConsoleEngine>(),
                sp.GetRequiredService<IFavourites>(),
                favouritesPageSize))
            .AddSingleton<ISongSources>(sp => new SongSources(sp.GetRequiredService<IDisplay>(), songSourcePaths))
            .AddSingleton<ISongList, SongList>()
            .AddSingleton<ISongPlayer, SongPlayer>()
            .BuildServiceProvider();

        var engine = serviceProvider.GetRequiredService<IJukeboxEngine>();

        engine.Start();
    }
}