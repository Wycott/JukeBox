namespace JukeboxInterfaces;

public interface ISongSources
{
    IReadOnlyList<string> Sources { get; }
}