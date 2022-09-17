using JukeboxLibrary.Helpers;
using JukeboxLibrary.Types;
using JukeboxLibrary.Interfaces;

namespace JukeboxLibrary;

public class JukeboxEngine : IJukeboxEngine
{
    public ISongSources SongSources { get; }
    public ISongList SongList { get; }
    public ISongPlayer SongPlayer { get; }

    public JukeboxEngine(ISongSources songSources, ISongList songList, ISongPlayer songPlayer)
    {
        SongSources = songSources;
        SongList = songList;
        SongPlayer = songPlayer;
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
                    Display.FlowerBox();
                    jukeboxState = JukeboxStateType.RequestSong;
                    break;
                case JukeboxStateType.RequestSong:
                    Console.Write("Enter pattern: ");
                    selectedPattern = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(selectedPattern))
                    {
                        jukeboxState = JukeboxStateType.FindSong;
                    }
                    break;
                case JukeboxStateType.FindSong:
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
                        Display.WriteError("Not Found!");
                        jukeboxState = JukeboxStateType.RequestSong;
                    }
                    break;
                case JukeboxStateType.SelectVersion:
                    foreach (var song in SongList.SongCollection)
                    {
                        var isRightSong = Display.IsThisTheRightSong(song.ShortenedPath);

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
                    if (jukeboxState != JukeboxStateType.PlaySong)
                    {
                        Display.WriteError("Nothing selected!");
                        jukeboxState = JukeboxStateType.RequestSong;
                    }
                    break;
                case JukeboxStateType.PlaySong:
                    SongPlayer.PlaySong(selectedSong);
                    jukeboxState = JukeboxStateType.RequestSong;
                    break;
                case JukeboxStateType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}