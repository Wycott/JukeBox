using JukeboxInterfaces;
using Moq;

namespace JukeboxLibrary.Test;

internal static class SongPlayerMocks
{
    // Engine tests assume playback succeeds unless a test sets up PlaySong itself.
    // SetReturnsDefault only applies where no setup matches, so a test's own setups always win.
    public static Mock<ISongPlayer> WithSuccessfulPlayback(Mock<ISongPlayer>? songPlayer)
    {
        var mock = songPlayer ?? new Mock<ISongPlayer>();
        mock.SetReturnsDefault(true);

        return mock;
    }
}