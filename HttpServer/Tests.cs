using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Tests
{
    public static class HttpRequestTests
    {
        private record TestCase(string Name, string Raw);

        public static void RunAll()
        {
            var tests = new List<TestCase>
            {
                new("Basic GET",
@"GET / HTTP/1.1
Host: localhost
User-Agent: TestClient
Accept: */*

"),

                new("Minimal GET",
@"GET /index.html HTTP/1.1
Host: example.com
"),

                new("POST with body",
@"POST /submit HTTP/1.1
Host: localhost
Content-Length: 11

hello world"),

                new("JSON POST",
@"POST /api HTTP/1.1
Host: localhost
Content-Type: application/json
Content-Length: 13

{""a"":1}"),

                new("Headers with spaces",
@"GET /test HTTP/1.1
Host:   localhost:8080
X-Test-Header:    value with spaces
"),

                new("Colon in header value",
@"GET / HTTP/1.1
Host: localhost
Time: 12:30:45 GMT
"),

                new("No Content-Length but body exists",
@"POST /weird HTTP/1.1
Host: localhost

hello"),

                new("Malformed request line",
@"GET /onlymethod"),

                new("Extra CRLF",
@"GET / HTTP/1.1
Host: localhost


")
            };

            int passed = 0;

            foreach (var test in tests)
            {
                try
                {
                    var normalized = test.Raw
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n")
                        .Replace("\n", "\r\n");

                    var bytes = Encoding.UTF8.GetBytes(normalized);

                    Console.WriteLine($"[TEST] {test.Name}");
                    Console.WriteLine($"Raw request:\n{test.Raw}\n");
                    Console.WriteLine($"UTF8: \n{BitConverter.ToString(bytes)}\n");

                    var request = HttpRequest.Parse(bytes);

                    Console.WriteLine($"[PASS] {test.Name}");
                    Console.WriteLine($"       {request.Method} {request.Path} {request.Version}");
                    Console.WriteLine($"       Headers: {request.Headers.Count}");
                    Console.WriteLine($"       Body: '{request.Body}'");
                    Console.WriteLine();

                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FAIL] {test.Name}");
                    Console.WriteLine($"       {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"RESULT: {passed}/{tests.Count} passed");
        }
    }
}