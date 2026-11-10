using AiAnnotations;
using JukeboxDomain.Helpers;

namespace JukeboxDomain.Test.Helpers;

[AiGenerated]
public class SongFilesTest : IDisposable
{
    private readonly string testRoot;

    public SongFilesTest()
    {
        testRoot = Path.Combine(Path.GetTempPath(), "JukeboxTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("song.mp3", true)]
    [InlineData("song.MP3", true)]
    [InlineData("song.m4a", true)]
    [InlineData("song.wav", false)]
    [InlineData("notes.txt", false)]
    [InlineData("mp3", false)]
    public void IsAudioFile_ChecksExtension(string path, bool expected)
    {
        Assert.Equal(expected, SongFiles.IsAudioFile(path));
    }

    [Fact]
    public void Find_ReturnsAudioFilesInNestedSubdirectories()
    {
        var albumDir = Path.Combine(testRoot, "Artist", "Album");
        Directory.CreateDirectory(albumDir);
        File.WriteAllText(Path.Combine(albumDir, "one.mp3"), "");
        File.WriteAllText(Path.Combine(albumDir, "two.m4a"), "");
        File.WriteAllText(Path.Combine(albumDir, "cover.jpg"), "");

        var result = SongFiles.Find(testRoot, "*").Select(Path.GetFileName).Order().ToList();

        Assert.Equal(["one.mp3", "two.m4a"], result);
    }

    [Fact]
    public void Find_AppliesSearchPattern()
    {
        var artistDir = Path.Combine(testRoot, "Artist");
        Directory.CreateDirectory(artistDir);
        File.WriteAllText(Path.Combine(artistDir, "yellow submarine.mp3"), "");
        File.WriteAllText(Path.Combine(artistDir, "help.mp3"), "");

        var result = SongFiles.Find(testRoot, "*yell*").ToList();

        Assert.Single(result);
        Assert.EndsWith("yellow submarine.mp3", result[0]);
    }

    [Fact]
    public void Find_WithMissingSource_ReturnsEmpty()
    {
        var result = SongFiles.Find(Path.Combine(testRoot, "does-not-exist"), "*");

        Assert.Empty(result);
    }
}
