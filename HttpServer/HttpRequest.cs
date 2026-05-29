namespace HttpServer;

/// <summary>
/// This class represents a Http Request. Each http request consists of the following:
/// - A method (e.g. GET, POST, etc.)
/// - A path (e.g. /index.html)
/// - A version (e.g. HTTP/1.1)
/// - Headers (e.g. Host: localhost:8080, User-Agent: Mozilla/5.0, etc.)
/// </summary>
public class HttpRequest
{
    /// <summary>
    /// The HTTP method used by the client.
    /// This defines the type of operation the client wants to perform,
    /// such as retrieving data (GET), sending data (POST),
    /// replacing data (PUT), or deleting data (DELETE).
    /// </summary>
    public string Method { get; init; } = "";

    /// <summary>
    /// The requested URL path.
    /// This identifies the resource or endpoint the client is trying to access,
    /// such as "/", "/ping", "/users", or "/index.html".
    /// </summary>
    public string Path { get; init; } = "";

    /// <summary>
    /// The HTTP protocol version used by the client.
    /// This specifies which version of the HTTP protocol the request follows,
    /// such as HTTP/1.0 or HTTP/1.1.
    /// Different versions may support different features,
    /// including persistent connections and chunked transfer encoding.
    /// </summary>
    public string Version { get; init; } = "";

    /// <summary>
    /// A collection of HTTP headers included in the request.
    /// Headers provide additional metadata about the request,
    /// such as the target host, accepted content types,
    /// authentication information, content length, and connection behavior.
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = [];

    /// <summary>
    /// Body of the request
    /// </summary>
    public string Body { get; set; } = "";

    public HttpRequest(string method, string path, string version, Dictionary<string, string> headers, string body)
    {
        Method = method;
        Path = path;
        Version = version;
        Headers = headers;
        Body = body;
    }

    /// <summary>
    /// Parses a raw string into a HttpRequest object.
    /// 1. Split the raw string into lines using \r\n as the delimiter. This is because delimiter represents the end of a line in HTTP requests.
    /// 2. First three parts are the method, the path and the version. The remaining parts are the headers.
    /// 3. Each header is formatted as "Key: Value\r\n". So when we split by delimiter in step 1, each split after the first three will be a header.
    ///    Then we can split by : to get the key and value of the header. We also trim the key and value to remove any extra whitespace.
    ///    Then we add the key and value to the headers dictionary.
    /// 4. Return a new HttpRequest object with the parsed method, path, version and headers.
    /// </summary>
    /// <param name="raw"></param>
    /// <returns><see cref="HttpRequest"/></returns>
    public static HttpRequest Parse(string raw)
    {
        string[] parts = raw.Split("\r\n");

        if (parts.Length == 0)
            throw new Exception("Invalid HTTP request");

        string[] requestLine = parts[0].Split(' ');

        if (requestLine.Length < 3)
            throw new Exception("Invalid HTTP request line");

        var method = requestLine[0];
        var path = requestLine[1];
        var version = requestLine[2];

        var headers = new Dictionary<string, string>();

        for (int i = 1; i < parts.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(parts[i]))
                continue;

            int colonIndex = parts[i].IndexOf(':');
            if (colonIndex == -1) continue;

            string key = parts[i].Substring(0, colonIndex).Trim();
            string value = parts[i].Substring(colonIndex + 1).Trim();

            headers[key] = value;
        }

        return new HttpRequest(method, path, version, headers, "");
    }
}
