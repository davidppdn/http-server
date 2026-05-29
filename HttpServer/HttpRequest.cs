using HttpServer.Utilities;
using System.Text;

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
    private static readonly byte[] delimiter = "\r\n\r\n"u8.ToArray();
    private static readonly byte[] headerDelimiter = "\r\n"u8.ToArray();

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
    public static HttpRequest Parse(ReadOnlySpan<byte> request)
    {
        if (request.Length == 0) throw new ArgumentOutOfRangeException(nameof(request), "Request cannot be empty");
        
        var requestSections = ByteArrayUtilities.Split(request,delimiter);

        // Assume no body until Content-Length header is found.
        if (requestSections.Count < 1)
            throw new ArgumentOutOfRangeException(nameof(request), "Failed to split the request");

        var headerSection = request[requestSections[0]];
        var splitHeader = ByteArrayUtilities.Split(headerSection, headerDelimiter);

        // Assume no headers because no headers is a valid http request, but the first line is required.
        if (splitHeader.Count < 1)
            throw new FormatException("Invalid HTTP request: request line is missing or malformed.");

        var requestLineBytes = headerSection[splitHeader[0]];
        var splitRequestLine = ByteArrayUtilities.Split(requestLineBytes, " "u8);

        if (splitRequestLine.Count < 3)
            throw new FormatException("Invalid HTTP request: request line must contain method, path, and version.");

        var methodBytes = requestLineBytes[splitRequestLine[0]];
        var method = Encoding.UTF8.GetString(methodBytes);

        var pathBytes = requestLineBytes[splitRequestLine[1]];
        var path = Encoding.UTF8.GetString(pathBytes);

        var versionBytes = requestLineBytes[splitRequestLine[2]];
        var version = Encoding.UTF8.GetString(versionBytes);

        var headers = new Dictionary<string, string>();

        for (int i = 1; i < splitHeader.Count; i++)
        {
            var header = headerSection[splitHeader[i]];
            var colonIndex = header.IndexOf((byte)':');
            if (colonIndex < 0)
                throw new Exception("Invalid HTTP request: malformed header");

            var trimmedKey = ByteArrayUtilities.Trim(header[..colonIndex]);
            var trimmedSpan = ByteArrayUtilities.Trim(header[(colonIndex + 1)..]);

            var key = Encoding.UTF8.GetString(trimmedKey);
            var value = Encoding.UTF8.GetString(trimmedSpan);

            headers[key] = value;
        }

        if (requestSections.Count < 2)
            return new HttpRequest(method, path, version, headers, "");

        if (!headers.TryGetValue("Content-Length", out var contentLengthValue) || !int.TryParse(contentLengthValue, out var contentLength))
            throw new FormatException("Invalid HTTP request: missing or invalid Content-Length header.");

        var bodyBytes = request[requestSections[1]];
        
        if (bodyBytes.Length != contentLength)
            throw new ArgumentException("Invalid HTTP request: body length and Content-Length value do not match.");

        var body = Encoding.UTF8.GetString(bodyBytes);

        return new HttpRequest(method, path, version, headers, body);        
    }
}
