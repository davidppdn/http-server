using System.Text;

namespace HttpServer.Utilities;

public static class ByteArrayUtilities
{

    private static readonly byte[] delimiter = "\r\n\r\n"u8.ToArray();
    private static readonly byte[] headerDelimiter = "\r\n"u8.ToArray();

    /// <summary>
    /// Returns the index of the first occurrence of the specified pattern in the byte array.
    /// </summary>
    /// <param name="buffer">The byte array to search in</param>
    /// <param name="pattern">The byte array to search for</param>
    /// <returns>index of first occurence of pattern</returns>
    public static int IndexOf(this List<byte> buffer, byte[] pattern, int startIndex = 0)
    {
        if (pattern.Length == 0) return 0;
        if (buffer.Count < pattern.Length) return -1;

        if (startIndex < 0 || startIndex > buffer.Count) return -1;

        for (int i = startIndex; i <= buffer.Count - pattern.Length; i++)
        {
            bool match = true;

            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Trims a byte array by removing space bytes from both sides. This is useful for trimming header keys and values, which may have extra spaces around them.
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> Trim(ReadOnlySpan<byte> span)
    {
        int start = 0;
        int end = span.Length - 1;

        while (start <= end && span[start] == (byte)' ')
            start++;

        while (end >= start && span[end] == (byte)' ')
            end--;

        return span.Slice(start, end - start + 1);
    }

    /// <summary>
    /// Extacts the header from a byte array containing the header section of an HTTP request. 
    /// 1. Split header by newline delimiter (\r\n)
    /// 2. Skip first section, which is the request line (e.g. GET /index.html HTTP/1.1)
    /// 3. Skip any malformed headers that don't contain a colon (e.g. "Host: example.com" is valid, but "InvalidHeader" is not)
    /// 4. Trim key and value, convert to ascii and append to dictionary.
    /// </summary>
    /// <param name="headerSection"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ExtractHeader(ReadOnlySpan<byte> headerSection)
    {
        var headers = new Dictionary<string, string>();
        var splitHeaderSections = Split(headerSection, headerDelimiter);

        for (int i = 1; i < splitHeaderSections.Count; i++)
        {
            var header = headerSection[splitHeaderSections[i]];
            var colonIndex = header.IndexOf((byte)':');
            if (colonIndex < 0)
                throw new Exception("Invalid HTTP request: malformed header");

            var trimmedKey = Trim(header[..colonIndex]);
            var trimmedSpan = Trim(header[(colonIndex + 1)..]);

            var key = Encoding.UTF8.GetString(trimmedKey);
            var value = Encoding.UTF8.GetString(trimmedSpan);

            headers[key] = value;
        }

        return headers;
    }

    public static List<Range> Split(ReadOnlySpan<byte> span, ReadOnlySpan<byte> delimiter)
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
}
