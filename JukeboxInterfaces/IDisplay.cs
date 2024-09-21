namespace JukeboxInterfaces;

public interface IDisplay
{
    void FlowerBox();

    void WriteYellowText(string data);

    bool? IsThisTheRightSong(string candidate);

    void WriteError(string errorMessage);
}