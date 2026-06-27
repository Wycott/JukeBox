using System.Text.Json;
using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class BanList : IBanList
{
    private readonly string filePath;
    private List<string> bannedSongs;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public BanList(string filePath)
    {
        this.filePath = filePath;
        bannedSongs = Load();
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
        try
        {
            var json = JsonSerializer.Serialize(bannedSongs, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (IOException)
        {
            // Best effort — don't crash if we can't write
        }
    }

    private List<string> Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return [];
        }
    }
}
