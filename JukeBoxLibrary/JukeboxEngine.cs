using JukeboxInterfaces;
using JukeboxTypes;

namespace JukeboxLibrary;

public class JukeboxEngine(
    ISongSources songSources,
    ISongList songList,
    ISongPlayer songPlayer,
    IDisplay displayEngine,
    IConsoleEngine consoleEngine,
    IFavourites favourites,
    int favouritesPageSize = 20)
    : IJukeboxEngine
{
    private IConsoleEngine ConsoleEngine { get; } = consoleEngine;
    private ISongSources SongSources { get; } = songSources;
    private ISongList SongList { get; } = songList;
    private ISongPlayer SongPlayer { get; } = songPlayer;
    private IDisplay DisplayEngine { get; } = displayEngine;
    private IFavourites Favourites { get; } = favourites;

    private string lastPattern = string.Empty;
    private bool inFavouritesMode;

    public void Start(CancellationToken cancellationToken = default)
    {
        LetTheMusicPlay(cancellationToken);
    }

    private void LetTheMusicPlay(CancellationToken cancellationToken)
    {
        var selectedPattern = string.Empty;
        var selectedSong = string.Empty;

        var jukeboxState = JukeboxStateType.ShowTitleBox;

        while (jukeboxState != JukeboxStateType.Exit && !cancellationToken.IsCancellationRequested)
        {
            switch (jukeboxState)
            {
                case JukeboxStateType.ShowTitleBox:
                    jukeboxState = ShowTitleBox();
                    break;
                case JukeboxStateType.RequestSong:
                    jukeboxState = RequestSong(out selectedPattern, out selectedSong);
                    break;
                case JukeboxStateType.FindSong:
                    jukeboxState = FindSong(selectedPattern);
                    break;
                case JukeboxStateType.SelectVersion:
                    jukeboxState = SelectVersion(out selectedSong);
                    break;
                case JukeboxStateType.PlaySong:
                    jukeboxState = PlaySong(selectedSong);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private JukeboxStateType PlaySong(string selectedSong)
    {
        SongPlayer.PlaySong(selectedSong);
        Favourites.RecordPlay(selectedSong);

        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType SelectVersion(out string selectedSong)
    {
        selectedSong = string.Empty;
        var userCancelled = false;

        foreach (var song in SongList.SongCollection)
        {
            var isRightSong = DisplayEngine.IsThisTheRightSong(song.ShortenedPath);

            if (isRightSong == null)
            {
                userCancelled = true;
                break;
            }

            if (!(bool)isRightSong)
            {
                continue;
            }

            selectedSong = song.FullPath;
            return JukeboxStateType.PlaySong;
        }

        if (!userCancelled)
        {
            DisplayEngine.WriteError("Nothing selected!");
        }

        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType FindSong(string selectedPattern)
    {
        SongList.Build(SongSources, selectedPattern);

        if (SongList.SongCollection.Count > 0)
        {
            return JukeboxStateType.SelectVersion;
        }

        DisplayEngine.WriteError("Not Found!");
        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType RequestSong(out string selectedPattern, out string selectedSong)
    {
        selectedSong = string.Empty;

        ConsoleEngine.WriteText("Enter pattern or command ");
        var input = ConsoleEngine.ReadLine();

        if (string.Equals(input, ":q", StringComparison.OrdinalIgnoreCase))
        {
            selectedPattern = string.Empty;
            return JukeboxStateType.Exit;
        }

        if (string.Equals(input, ":?", StringComparison.Ordinal))
        {
            ShowHelp();
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        if (string.Equals(input, ":l", StringComparison.OrdinalIgnoreCase))
        {
            ListFavourites();
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        if (string.Equals(input, ":f", StringComparison.OrdinalIgnoreCase))
        {
            inFavouritesMode = true;
            selectedPattern = string.Empty;
            return PlayRandomFavourite(out selectedSong);
        }

        if (string.Equals(input, ":n", StringComparison.OrdinalIgnoreCase))
        {
            if (inFavouritesMode)
            {
                selectedPattern = string.Empty;
                return PlayRandomFavourite(out selectedSong);
            }

            if (lastPattern.Length > 0)
            {
                selectedPattern = lastPattern;
                return JukeboxStateType.FindSong;
            }

            DisplayEngine.WriteError("No previous pattern");
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        if (!string.IsNullOrWhiteSpace(input))
        {
            inFavouritesMode = false;
            lastPattern = input;
            selectedPattern = input;
            return JukeboxStateType.FindSong;
        }

        selectedPattern = string.Empty;
        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType PlayRandomFavourite(out string selectedSong)
    {
        var favourite = Favourites.GetRandomFavourite();

        if (favourite == null)
        {
            DisplayEngine.WriteError("No favourites yet");
            inFavouritesMode = false;
            selectedSong = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        var fileName = Path.GetFileName(favourite.FullPath);
        ConsoleEngine.WriteALine();
        ConsoleEngine.WriteText("Playing: ");
        DisplayEngine.WriteYellowText(fileName);
        selectedSong = favourite.FullPath;
        return JukeboxStateType.PlaySong;
    }

    private void ListFavourites()
    {
        var favourites = Favourites.GetTopFavourites();

        if (favourites.Count == 0)
        {
            DisplayEngine.WriteError("No favourites yet");
            return;
        }

        var displayed = 0;

        foreach (var entry in favourites)
        {
            var fileName = Path.GetFileName(entry.FullPath);
            DisplayEngine.WriteYellowText($"  {entry.PlayCount}x  {fileName}");
            displayed++;

            if (displayed % favouritesPageSize == 0 && displayed < favourites.Count)
            {
                ConsoleEngine.WriteText("-- More (any key) or q to stop --");
                var key = ConsoleEngine.ReadAKey();
                ConsoleEngine.WriteALine();

                if (key.Key == ConsoleKey.Q)
                {
                    break;
                }
            }
        }
    }

    private JukeboxStateType ShowTitleBox()
    {
        DisplayEngine.FlowerBox();
        SongSources.DisplaySongCounts();
        ShowHelp();

        return JukeboxStateType.RequestSong;
    }

    private void ShowHelp()
    {
        ConsoleEngine.WriteALine();
        ConsoleEngine.WriteALine("Patterns:");
        ConsoleEngine.WriteALine("  yellow sub      - might match 'Yellow Submarine' (words become wildcards)");
        ConsoleEngine.WriteALine("  *@doors         - might match anything by 'The Doors' (artist search)");
        ConsoleEngine.WriteALine();
        ConsoleEngine.WriteALine("Commands:");
        ConsoleEngine.WriteALine("  :n  - Next (repeat last search or next favourite)");
        ConsoleEngine.WriteALine("  :f  - Play a random favourite");
        ConsoleEngine.WriteALine("  :l  - List favourites");
        ConsoleEngine.WriteALine("  :?  - Show this help");
        ConsoleEngine.WriteALine("  :q  - Quit");
        ConsoleEngine.WriteALine();
    }
}