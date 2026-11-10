namespace JukeboxDomain.Helpers;

public static class LevenshteinDistance
{
    public static int Calculate(string source, string? target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;

        var matrix = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= targetLength; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = char.ToLowerInvariant(source[i - 1]) == char.ToLowerInvariant(target[j - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[sourceLength, targetLength];
    }

    public static bool AreSimilar(string a, string b, double threshold = 0.3)
    {
        var distance = Calculate(a, b);
        var maxLength = Math.Max(a.Length, b.Length);

        if (maxLength == 0)
        {
            return true;
        }

        var similarity = 1.0 - (double)distance / maxLength;
        return similarity >= (1.0 - threshold);
    }
}
