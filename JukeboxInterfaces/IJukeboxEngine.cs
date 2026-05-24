namespace JukeboxInterfaces;

public interface IJukeboxEngine
{
    void Start(CancellationToken cancellationToken = default);
}