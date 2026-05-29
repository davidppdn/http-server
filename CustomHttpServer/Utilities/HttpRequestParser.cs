using CustomHttpServer.Models;
using System.Diagnostics.CodeAnalysis;

namespace CustomHttpServer.Utilities;

public static class HttpRequestParser
{
    public static bool TryParse(
        this ReadOnlySpan<byte> buffer,
        [NotNullWhen(true)] out HttpRequest request, 
        out int consumed)
    {
        request = null;
        consumed = 0;
    }
}
