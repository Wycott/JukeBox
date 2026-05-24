using JukeboxInterfaces;
using JukeboxTypes;

namespace JukeboxLibrary;

public class JukeboxEngine : IJukeboxEngine
{
    private IConsoleEngine ConsoleEngine { get; }
    public ISongSources SongSources { get; }
    public ISongList SongList { get; }
    public ISongPlayer SongPlayer { get; }
    public IDisplay DisplayEngine { get; }

    public JukeboxEngine(ISongSources songSources, ISongList songList, ISongPlayer songPlayer, IDisplay displayEngine, IConsoleEngine consoleEngine)
    {
        ConsoleEngine = consoleEngine;
        SongSources = songSources;
        SongList = songList;
        SongPlayer = songPlayer;
        DisplayEngine = displayEngine;
    }

    public void Start()
    {
        LetTheMusicPlay();
    }

    private void LetTheMusicPlay()
    {
        var selectedPattern = string.Empty;
        var selectedSong = string.Empty;

        var jukeboxState = JukeboxStateType.ShowTitleBox;

        while (jukeboxState != JukeboxStateType.Unknown)
        {
            switch (jukeboxState)
            {
                case JukeboxStateType.ShowTitleBox:
                    jukeboxState = ShowTitleBox();
                    break;
                case JukeboxStateType.RequestSong:
                    jukeboxState = RequestSong(out selectedPattern);
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

    private JukeboxStateType FindSong(string? selectedPattern)
    {
        JukeboxStateType jukeboxState;

        if (selectedPattern != null)
        {
            SongList.Build(SongSources, selectedPattern);
        }

        if (SongList.SongCollection.Count > 0)
        {
            jukeboxState = JukeboxStateType.SelectVersion;
        }
        else
        {
            DisplayEngine.WriteError("Not Found!");
            jukeboxState = JukeboxStateType.RequestSong;
        }

        return jukeboxState;
    }

    private JukeboxStateType RequestSong(out string? selectedPattern)
    {
        ConsoleEngine.WriteText("Enter pattern (q to quit): ");
        selectedPattern = ConsoleEngine.ReadLine();

        if (string.Equals(selectedPattern, "q", StringComparison.OrdinalIgnoreCase))
        {
            return JukeboxStateType.Unknown;
        }

        if (!string.IsNullOrWhiteSpace(selectedPattern))
        {
            return JukeboxStateType.FindSong;
        }

        return JukeboxStateType.RequestSong;
    }

    private JukeboxStateType ShowTitleBox()
    {
        DisplayEngine.FlowerBox();

        return JukeboxStateType.RequestSong;
    }
}