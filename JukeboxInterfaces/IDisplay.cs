using JukeboxTypes;

namespace JukeboxInterfaces;

public interface IDisplay
{
    void FlowerBox();

    void WriteYellowText(string data);

    SongResponse IsThisTheRightSong(string candidate);

    void WriteError(string errorMessage);
}