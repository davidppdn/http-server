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

        var headers = new Dictionary <string, string>();

        for (int i=1; i<lines.Count;i++)
        {
            var line = lines[i];
            var headerSpan = headerPart[line];
            var split = headerSpan.Split((byte)':');

            if (split.Count != 2)
                return false;

            var keySpan = headerSpan[split[0]];
            var valueSpan = headerSpan[split[1]];

            var key = keySpan.Trim();
            var value = valueSpan.Trim();

            headers.Add(
                Encoding.UTF8.GetString(key),
                Encoding.UTF8.GetString(value)
            );
        }

        if (!headers.TryGetValue("Content-Length", out var stringLength))
        {
            consumed = headerEnd + 4;
            request = new HttpRequest
            (
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[0]]),
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[1]]),
                Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[2]]),
                headers,
                string.Empty
            );
            return true;
        }

        var bodySize = int.Parse(stringLength);
        var totalSize = headerEnd + 4 + bodySize;
        var availableSize = buffer.Length;
        
        if (availableSize < totalSize)
            return false;

        var bodySpan = buffer[(headerEnd + 4)..totalSize];

        consumed = totalSize;
        request = new HttpRequest
        (
            Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[0]]),
            Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[1]]),
            Encoding.UTF8.GetString(requestLineSpan[requestLinePartsFiltered[2]]),
            headers,
            Encoding.UTF8.GetString(bodySpan)
        );

        return true;
    }
}
