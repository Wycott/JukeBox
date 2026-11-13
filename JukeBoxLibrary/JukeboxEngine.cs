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
    private bool stopped;

    // Songs finishing are reported on a background thread, so anything that plays songs
    // or changes engine state holds this lock.
    private readonly Lock stateLock = new();

    private const string Prompt = "Enter pattern or command ";

    public void Start(CancellationToken cancellationToken = default)
    {
        SongPlayer.SongFinished += OnSongFinished;

        try
        {
            LetTheMusicPlay(cancellationToken);
        }
        finally
        {
            SongPlayer.SongFinished -= OnSongFinished;

            lock (stateLock)
            {
                stopped = true;
            }
        }
    }

    private void OnSongFinished(object? sender, EventArgs e)
    {
        lock (stateLock)
        {
            if (stopped || !inFavouritesMode)
            {
                return;
            }

            if (PlayNextFavouriteAutomatically())
            {
                // The song info was printed over the waiting prompt, so show it again.
                ConsoleEngine.WriteText(Prompt);
            }
        }
    }

    private bool PlayNextFavouriteAutomatically()
    {
        // Picks that are skipped (similar to banned) or fail to play shouldn't end the run,
        // but give up after one full pass so a list of unplayable songs can't loop forever.
        var attempts = Favourites.Count;

        for (var i = 0; i < attempts && inFavouritesMode; i++)
        {
            if (PlayRandomFavourite(out var selectedSong) != JukeboxStateType.PlaySong)
            {
                continue;
            }

            if (PlaySongCore(selectedSong))
            {
                return true;
            }
        }

        return false;
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
                    lock (stateLock)
                    {
                        jukeboxState = PlaySong(selectedSong);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private JukeboxStateType PlaySong(string selectedSong)
    {
        PlaySongCore(selectedSong);

        return JukeboxStateType.RequestSong;
    }

    private bool PlaySongCore(string selectedSong)
    {
        // The selected song becomes "current" even when it fails, so :r and :b act on it
        // rather than on whatever played before.
        lastPlayedSong = selectedSong;

        if (!SongPlayer.PlaySong(selectedSong))
        {
            RemoveMissingFavourite(selectedSong);
            return false;
        }

        var countBefore = Favourites.Count;

        Favourites.RecordPlay(selectedSong);
        WarnIfNewSongTookFavouritesOverLimit(countBefore);

        return true;
    }

    private void WarnIfNewSongTookFavouritesOverLimit(int countBefore)
    {
        // Only a new song grows the list; replaying a favourite doesn't need a warning.
        var addedNewSong = Favourites.Count > countBefore;

        if (addedNewSong && Favourites.Count > Favourites.MaxFavourites)
        {
            DisplayEngine.WriteError(
                $"Favourites is over its limit ({Favourites.Count} of {Favourites.MaxFavourites}) - use :t to trim");
        }
    }

    private void RemoveMissingFavourite(string selectedSong)
    {
        // A favourite whose file no longer exists would otherwise keep being picked by :f and :n.
        // Files that exist but fail (codec, device) are kept, since the failure may be temporary.
        if (!inFavouritesMode || File.Exists(selectedSong))
        {
            return;
        }

        Favourites.Remove(selectedSong);
        
        var fileName = Path.GetFileName(selectedSong);
        
        DisplayEngine.WriteYellowText($"Removed missing file from favourites: {fileName}");
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
        selectedPattern = string.Empty;
        selectedSong = string.Empty;

        ConsoleEngine.WriteText(Prompt);

        var input = ConsoleEngine.ReadLine();

        lock (stateLock)
        {
            return HandleInput(input, out selectedPattern, out selectedSong);
        }
    }

    private JukeboxStateType HandleInput(string? input, out string selectedPattern, out string selectedSong)
    {
        selectedPattern = string.Empty;
        selectedSong = string.Empty;

        switch (input?.ToLowerInvariant())
        {
            case ":q":
                return JukeboxStateType.Exit;
            case ":?":
                ShowHelp();
                return JukeboxStateType.RequestSong;
            case ":l":
                ListFavourites();
                return JukeboxStateType.RequestSong;
            case ":f":
                inFavouritesMode = true;
                return PlayRandomFavourite(out selectedSong);
            case ":r":
                RemoveCurrentFromFavourites();
                return JukeboxStateType.RequestSong;
            case ":t":
                TrimLeastPlayedFavourites();
                return JukeboxStateType.RequestSong;
            case ":b":
                BanCurrentSong();
                return JukeboxStateType.RequestSong;
            case ":e":
                ListBannedSongs();
                return JukeboxStateType.RequestSong;
            case ":n":
                return PlayNext(out selectedPattern, out selectedSong);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return JukeboxStateType.RequestSong;
        }

        inFavouritesMode = false;
        lastPattern = input;
        selectedPattern = input;

        return JukeboxStateType.FindSong;
    }

    private JukeboxStateType PlayNext(out string selectedPattern, out string selectedSong)
    {
        selectedPattern = string.Empty;
        selectedSong = string.Empty;

        if (inFavouritesMode)
        {
            return PlayRandomFavourite(out selectedSong);
        }

        if (lastPattern.Length > 0)
        {
            selectedPattern = lastPattern;
            return JukeboxStateType.FindSong;
        }

        DisplayEngine.WriteError("No previous pattern");

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
        ConsoleEngine.WriteText($"Playing {favourite.Rank} of {Favourites.Count}: ");
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
            DisplayEngine.WriteYellowText($"  {entry.Rank,3}. {entry.PlayCount}x  {fileName}");
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

    private void TrimLeastPlayedFavourites()
    {
        var removed = Favourites.TrimLeastPlayed();

        if (removed.Count == 0)
        {
            DisplayEngine.WriteError("No favourites yet");

            return;
        }

        var playCount = removed[0].PlayCount;
        var songs = removed.Count == 1 ? "favourite" : "favourites";
        var plays = playCount == 1 ? "play" : "plays";

        DisplayEngine.WriteYellowText(
            $"Trimmed {removed.Count} {songs} with {playCount} {plays} ({Favourites.Count} left)");
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
        ConsoleEngine.WriteALine("  :t  - Trim favourites (removes those with the fewest plays)");
        ConsoleEngine.WriteALine("  :b  - Ban current song (exclude from future suggestions)");
        ConsoleEngine.WriteALine("  :e  - Show banned (excluded) songs");
        ConsoleEngine.WriteALine("  :l  - List favourites");
        ConsoleEngine.WriteALine("  :?  - Show this help");
        ConsoleEngine.WriteALine("  :q  - Quit");
        ConsoleEngine.WriteALine();
    }
}