using CustomHttpServer.Models;
using CustomHttpServer.Utilities;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace CustomttpServer.Tests;

public static class HttpRequestTests
{
    public static void TestHttpRequest_NoBody()
    {
        var request = "GET /index.html HTTP/1.1\r\n\r\n";

        Assert(
            HttpRequestParser.TryParse(Encoding.UTF8.GetBytes(request), out var parsed, out _),
            "Parse failed");

        Assert(parsed.Method == "GET", "Method");
        Assert(parsed.Path == "/index.html", "Path");
        Assert(parsed.Version == "HTTP/1.1", "Version");
        Assert(parsed.Headers.Count == 0, "Headers not empty");
        Assert(parsed.Body.Length == 0, "Body not empty");

        Console.WriteLine("PASS");
    }

    public static void RunAll()
    {
        Console.WriteLine("Running HttpRequest Tests...");
        TestHttpRequest_NoBody();
    }

    static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception("FAIL: " + message);
    }
}