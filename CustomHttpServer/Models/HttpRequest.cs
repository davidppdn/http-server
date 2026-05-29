namespace CustomHttpServer.Models;

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
}