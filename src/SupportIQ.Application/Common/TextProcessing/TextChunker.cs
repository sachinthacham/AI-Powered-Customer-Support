namespace SupportIQ.Application.Common.TextProcessing;

/// <summary>
/// Splits plain text into overlapping, word-boundary-safe chunks for embedding. A pure,
/// dependency-free algorithm deliberately kept in Application (no I/O, easy to unit test)
/// rather than folded into the Infrastructure ingestion pipeline.
/// </summary>
public static class TextChunker
{
    public static IReadOnlyList<string> Chunk(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return Array.Empty<string>();

        var chunks = new List<string>();
        var current = new List<string>();
        var currentLength = 0;

        foreach (var word in words)
        {
            if (currentLength + word.Length + 1 > chunkSize && current.Count > 0)
            {
                chunks.Add(string.Join(' ', current));
                current = TakeTrailingOverlap(current, overlap, out currentLength);
            }

            current.Add(word);
            currentLength += word.Length + 1;
        }

        if (current.Count > 0)
            chunks.Add(string.Join(' ', current));

        return chunks;
    }

    private static List<string> TakeTrailingOverlap(List<string> words, int overlapChars, out int length)
    {
        var overlapWords = new List<string>();
        length = 0;

        for (var i = words.Count - 1; i >= 0 && length < overlapChars; i--)
        {
            length += words[i].Length + 1;
            overlapWords.Insert(0, words[i]);
        }

        return overlapWords;
    }
}
