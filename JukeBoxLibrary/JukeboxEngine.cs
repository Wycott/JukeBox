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

    private void LetTheMusicPlay() // Ru
    {
        var selectedPattern = string.Empty;
        var selectedSong = string.Empty;

        var jukeboxState = JukeboxStateType.ShowTitleBox;

        while (true)
        {
            switch (jukeboxState)
            {
                case JukeboxStateType.ShowTitleBox:
                    jukeboxState = ShowTitleBox();
                    break;
                case JukeboxStateType.RequestSong:
                    jukeboxState = RequestSong(jukeboxState, out selectedPattern);
                    break;
                case JukeboxStateType.FindSong:
                    jukeboxState = FindSong(selectedPattern);
                    break;
                case JukeboxStateType.SelectVersion:
                    jukeboxState = SelectVersion(jukeboxState, out selectedSong);
                    break;
                case JukeboxStateType.PlaySong:
                    jukeboxState = PlaySong(selectedSong);
                    break;
                case JukeboxStateType.Unknown:
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

    private JukeboxStateType SelectVersion(JukeboxStateType jukeboxState, out string selectedSong)
    {
        selectedSong = string.Empty;

        foreach (var song in SongList.SongCollection)
        {
            var isRightSong = DisplayEngine.IsThisTheRightSong(song.ShortenedPath);

            if (isRightSong == null)
            {
                jukeboxState = JukeboxStateType.RequestSong;
                break;
            }

            if (!(bool)isRightSong)
            {
                continue;
            }

            selectedSong = song.FullPath;
            jukeboxState = JukeboxStateType.PlaySong;
            break;
        }

        if (jukeboxState == JukeboxStateType.PlaySong)
        {
            return jukeboxState;
        }

        DisplayEngine.WriteError("Nothing selected!");
        jukeboxState = JukeboxStateType.RequestSong;

        return jukeboxState;
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

    private JukeboxStateType RequestSong(JukeboxStateType jukeboxState, out string? selectedPattern)
    {
        ConsoleEngine.WriteText("Enter pattern: ");
        selectedPattern = ConsoleEngine.ReadLine();

        if (!string.IsNullOrWhiteSpace(selectedPattern))
        {
            jukeboxState = JukeboxStateType.FindSong;
        }

        return jukeboxState;
    }

    private JukeboxStateType ShowTitleBox()
    {
        DisplayEngine.FlowerBox();

        return JukeboxStateType.RequestSong;
    }
}