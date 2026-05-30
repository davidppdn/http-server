namespace CustomHttpServer.Utilities;

public static class ByteUtilities
{
    public static List<Range> Split(this ReadOnlySpan<byte> span, ReadOnlySpan<byte> delimiter)
    {
        var result = new List<Range>();

        int start = 0;

        while (true)
        {
            int index = span[start..].IndexOf(delimiter);

            if (index < 0)
                break;

            int match = start + index;

            result.Add(new Range(start, match));

            start = match + delimiter.Length;
        }

        if (start < span.Length)
            result.Add(new Range(start, span.Length));

        return result;
    }

    public static List<Range> Split(this ReadOnlySpan<byte> span, byte delimiter)
    {
        return Split(span, new ReadOnlySpan<byte>(new[] { delimiter }));
    }
}
