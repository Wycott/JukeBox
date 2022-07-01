using JukeboxLibrary.Helpers;
using JukeboxLibrary.Types;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary;

public class JukeboxEngine : IJukeboxEngine
{
    public ISongSources SongSources { get; }
    public ISongList SongList { get; }
    public ISongPlayer SongPlayer { get; }

    private JukeboxStateType JukeboxState { get; set; } = JukeboxStateType.Unknown;

    public JukeboxEngine(ISongSources songSources, ISongList songList, ISongPlayer songPlayer)
    {
        SongSources = songSources;
        SongList = songList;
        SongPlayer = songPlayer;
    }

    public void Start()
    {
        JukeboxState = JukeboxStateType.ShowTitleBox;
        LetTheMusicPlay();
    }

    private void LetTheMusicPlay() // Ru
    {
        var selectedPattern = string.Empty;
        var selectedSong = string.Empty;

        while (true)
        {
            switch (JukeboxState)
            {
                case JukeboxStateType.Unknown:
                    break;
                case JukeboxStateType.ShowTitleBox:
                    Display.FlowerBox();
                    JukeboxState = JukeboxStateType.RequestSong;
                    break;
                case JukeboxStateType.RequestSong:
                    Console.Write("Enter pattern: ");
                    selectedPattern = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(selectedPattern))
                    {
                        JukeboxState = JukeboxStateType.FindSong;
                    }
                    break;
                case JukeboxStateType.FindSong:
                    if (selectedPattern != null)
                    {
                        SongList.Build(SongSources, selectedPattern);
                    }

                    if (SongList.SongCollection.Count > 0)
                    {
                        JukeboxState = JukeboxStateType.SelectVersion;
                    }
                    else
                    {
                        Display.WriteError("Not Found!");
                        JukeboxState = JukeboxStateType.RequestSong;
                    }
                    break;
                case JukeboxStateType.SelectVersion:
                    foreach (var song in SongList.SongCollection)
                    {
                        bool? isRightSong = Display.IsThisTheRightSong(song.ShortenedPath);

                        if (isRightSong == null)
                        {
                            JukeboxState = JukeboxStateType.RequestSong;
                            break;
                        }

                        if ((bool)isRightSong)
                        {
                            selectedSong = song.FullPath;
                            JukeboxState = JukeboxStateType.PlaySong;
                            break;
                        }
                    }
                    if (JukeboxState != JukeboxStateType.PlaySong)
                    {
                        Display.WriteError("Nothing selected!");
                        JukeboxState = JukeboxStateType.RequestSong;
                    }
                    break;
                case JukeboxStateType.PlaySong:
                    SongPlayer.PlaySong(selectedSong);
                    JukeboxState = JukeboxStateType.RequestSong;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}