using CustomHttpServer.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CustomHttpServer.Utilities;

public static class HttpRequestParser
{
    public static bool TryParse(
        this ReadOnlySpan<byte> buffer,
        [NotNullWhen(true)] out HttpRequest? request,
        out int consumed)
    {
        request = null;
        consumed = 0;

        int headerEnd = buffer.IndexOf("\r\n\r\n"u8);
        if (headerEnd < 0)
            return false;

        // Contains GET /index.html HTTP/1.1\r\nHost: example.com\r\n\r\n
        var headerPart = buffer[..headerEnd];

        var lines = headerPart.Split("\r\n"u8);

        if (lines.Count == 0) return false;

        // request line: GET /index.html HTTP/1.1
        var requestLineSpan = headerPart[lines[0]];
        var requestLineParts = requestLineSpan.Split((byte)' ');
        // Filter out empty parts (e.g., due to multiple spaces)
        var requestLinePartsFiltered = requestLineParts.Where(part => part.Start.Value != part.End.Value).ToArray();

        if (requestLinePartsFiltered.Length != 3)
            return false;

        if (lines.Count == 1)
        {
            consumed = headerEnd + 4;
            request = new HttpRequest
            (
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[0]]),
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[1]]),
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[2]]),
                [],
                string.Empty
            );
            return true;
        }

        return false;
    }

    private static bool skipWs(ReadOnlySpan<byte> buffer, ref int index)
    {
        while (index < buffer.Length && IsWs(buffer[index]))
            index++;
        return index < buffer.Length;
    }

    private static bool IsWs(byte b) => b == ' ' || b == '\t';
}
