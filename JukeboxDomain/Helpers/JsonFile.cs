using System.Text.Json;

namespace JukeboxDomain.Helpers;

// Best-effort JSON persistence for small lists: a missing, unreadable or corrupt file loads
// as empty, and a failed write is ignored rather than taking the jukebox down.
internal static class JsonFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static T Load<T>(string path) where T : new()
    {
        try
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return new T();
        }
    }

    public static void Save<T>(string path, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, Options);
            File.WriteAllText(path, json);
        }
        catch (IOException)
        {
            // Best effort — don't crash if we can't write
        }
    }
}
