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
    IBanList banList,
    int favouritesPageSize = 20)
    : IJukeboxEngine
{
    private IConsoleEngine ConsoleEngine { get; } = consoleEngine;
    private ISongSources SongSources { get; } = songSources;
    private ISongList SongList { get; } = songList;
    private ISongPlayer SongPlayer { get; } = songPlayer;
    private IDisplay DisplayEngine { get; } = displayEngine;
    private IFavourites Favourites { get; } = favourites;
    private IBanList BanList { get; } = banList;

    private string lastPattern = string.Empty;
    private string lastPlayedSong = string.Empty;
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
        lastPlayedSong = selectedSong;

        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType SelectVersion(out string selectedSong)
    {
        selectedSong = string.Empty;
        var userCancelled = false;

        foreach (var song in SongList.SongCollection)
        {
            var similarBanned = BanList.IsSimilarToBanned(song.FullPath);
            if (similarBanned != null)
            {
                var skippedName = Path.GetFileName(song.FullPath);
                var bannedName = Path.GetFileName(similarBanned);
                DisplayEngine.WriteYellowText($"  Skipped: {skippedName} (similar to banned: {bannedName})");
                continue;
            }

            var response = DisplayEngine.IsThisTheRightSong(song.ShortenedPath);

            if (response == SongResponse.Cancel)
            {
                userCancelled = true;
                break;
            }

            if (response == SongResponse.Ban)
            {
                BanList.Ban(song.FullPath);
                var fileName = Path.GetFileName(song.FullPath);
                DisplayEngine.WriteYellowText($"Banned: {fileName}");
                continue;
            }

            if (response != SongResponse.Yes)
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

        if (string.Equals(input, ":r", StringComparison.OrdinalIgnoreCase))
        {
            RemoveCurrentFromFavourites();
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        if (string.Equals(input, ":b", StringComparison.OrdinalIgnoreCase))
        {
            BanCurrentSong();
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        if (string.Equals(input, ":e", StringComparison.OrdinalIgnoreCase))
        {
            ListBannedSongs();
            selectedPattern = string.Empty;
            return JukeboxStateType.RequestSong;
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

        if (BanList.IsBanned(favourite.FullPath))
        {
            DisplayEngine.WriteError("No favourites yet");
            inFavouritesMode = false;
            selectedSong = string.Empty;
            return JukeboxStateType.RequestSong;
        }

        var similarBanned = BanList.IsSimilarToBanned(favourite.FullPath);
        if (similarBanned != null)
        {
            var skippedName = Path.GetFileName(favourite.FullPath);
            var bannedName = Path.GetFileName(similarBanned);
            DisplayEngine.WriteYellowText($"  Skipped: {skippedName} (similar to banned: {bannedName})");
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

        ConsoleEngine.WriteALine($"{favourites.Count} favourites:");
        ConsoleEngine.WriteALine();

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

    private void RemoveCurrentFromFavourites()
    {
        if (string.IsNullOrEmpty(lastPlayedSong))
        {
            DisplayEngine.WriteError("No song has been played yet");
            return;
        }

        Favourites.Remove(lastPlayedSong);
        var fileName = Path.GetFileName(lastPlayedSong);
        DisplayEngine.WriteYellowText($"Removed: {fileName}");
    }

    private void BanCurrentSong()
    {
        if (string.IsNullOrEmpty(lastPlayedSong))
        {
            DisplayEngine.WriteError("No song has been played yet");
            return;
        }

        BanList.Ban(lastPlayedSong);
        var fileName = Path.GetFileName(lastPlayedSong);
        DisplayEngine.WriteYellowText($"Banned: {fileName}");
    }

    private void ListBannedSongs()
    {
        var banned = BanList.GetBannedSongs();

        if (banned.Count == 0)
        {
            DisplayEngine.WriteError("No banned songs");
            return;
        }

        ConsoleEngine.WriteALine($"{banned.Count} banned:");
        ConsoleEngine.WriteALine();

        foreach (var song in banned)
        {
            var fileName = Path.GetFileName(song);
            DisplayEngine.WriteYellowText($"  {fileName}");
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
        ConsoleEngine.WriteALine("  yell sub        - might match 'Yellow Submarine' (words become wildcards)");
        ConsoleEngine.WriteALine("  *@doors         - might match anything by 'The Doors' (artist search)");
        ConsoleEngine.WriteALine();
        ConsoleEngine.WriteALine("Commands:");
        ConsoleEngine.WriteALine("  :n  - Next (repeat last search or next favourite)");
        ConsoleEngine.WriteALine("  :f  - Play a random favourite");
        ConsoleEngine.WriteALine("  :r  - Remove current song from favourites");
        ConsoleEngine.WriteALine("  :b  - Ban current song (exclude from future suggestions)");
        ConsoleEngine.WriteALine("  :e  - Show banned (excluded) songs");
        ConsoleEngine.WriteALine("  :l  - List favourites");
        ConsoleEngine.WriteALine("  :?  - Show this help");
        ConsoleEngine.WriteALine("  :q  - Quit");
        ConsoleEngine.WriteALine();
    }
}