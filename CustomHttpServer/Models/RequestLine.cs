namespace CustomHttpServer.Models;

public readonly struct RequestLine
{
    public string Method { get; }
    public string Path { get; }
    public string Version { get; }

    public RequestLine(string method, string path, string version)
    {
        Method = method;
        Path = path;
        Version = version;
    }
}