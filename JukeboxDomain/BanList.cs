using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class BanList : IBanList
{
    private readonly string filePath;
    private readonly List<string> bannedSongs;

    public BanList(string filePath)
    {
        this.filePath = filePath;
        bannedSongs = JsonFile.Load<List<string>>(filePath);
    }

    public void Ban(string fullPath)
    {
        if (bannedSongs.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        bannedSongs.Add(fullPath);
        Save();
    }

    public bool IsBanned(string fullPath)
    {
        return bannedSongs.Contains(fullPath, StringComparer.OrdinalIgnoreCase);
    }

    public string? IsSimilarToBanned(string fullPath)
    {
        var candidateName = StripTrackNumber(Path.GetFileNameWithoutExtension(fullPath));

        foreach (var banned in bannedSongs)
        {
            if (string.Equals(banned, fullPath, StringComparison.OrdinalIgnoreCase))
                continue;

            var bannedName = StripTrackNumber(Path.GetFileNameWithoutExtension(banned));

            if (LevenshteinDistance.AreSimilar(candidateName, bannedName))
            {
                return banned;
            }
        }

        return null;
    }

    public IReadOnlyList<string> GetBannedSongs()
    {
        return bannedSongs.AsReadOnly();
    }

    private static string StripTrackNumber(string name)
    {
        // Remove leading track numbers like "12 " or "09 "
        var trimmed = name.TrimStart();
        var i = 0;
        while (i < trimmed.Length && char.IsDigit(trimmed[i]))
            i++;

        if (i > 0 && i < trimmed.Length && trimmed[i] == ' ')
            return trimmed[(i + 1)..];

        return trimmed;
    }

    private void Save()
    {
        JsonFile.Save(filePath, bannedSongs);
    }
}
