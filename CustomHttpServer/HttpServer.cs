using CustomHttpServer.Models;
using CustomHttpServer.Utilities;
using System.Net;
using System.Net.Sockets;

namespace CustomHttpServer;

public class HttpServer
{
    private Int32 _port;
    private IPAddress _host;
    private TcpListener? server;

    public HttpServer(string host, int port)
    {
        _host = IPAddress.Parse(host);
        _port = port;
    }

    public HttpServer(string connectionString)
    {
        var parts = connectionString.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Connection string must be in the format 'host:port'");
        _host = IPAddress.Parse(parts[0]);
        _port = int.Parse(parts[1]);
    }

    public void Start()
    {
        try
        {
            server = new(_host, _port);
            server.Start();

            while (true)
            {
                Console.Write("Waiting for a connection... ");
                
                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();

                byte[] incomingBuffer = new byte[8192];
                byte[] accumilatedBuffer = new byte[65536];
                int accumilatedBufferLength = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(incomingBuffer, 0, incomingBuffer.Length)) != 0)
                {
                    Array.Copy(incomingBuffer, 0, accumilatedBuffer, accumilatedBufferLength, bytesRead);
                    accumilatedBufferLength += bytesRead;

                    while (HttpRequestParser.TryParse(
                        accumilatedBuffer.AsSpan(0, accumilatedBufferLength),
                        out var request,
                        out var consumed))
                    {
                        ProcessRequest(request);

                        var remaining = accumilatedBufferLength - consumed;

                        if (remaining > 0)
                        {
                            Buffer.BlockCopy(
                                accumilatedBuffer,
                                consumed,
                                accumilatedBuffer,
                                0,
                                remaining);
                        }

                        accumilatedBufferLength = remaining;
                    }

                }

            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server?.Stop();
        }
    }

    public void Stop()
    {
        server?.Stop();
    }

    // For simplicity, this method just returns a fixed response for the "/ping" path and a 404 for everything else.
    private static string ProcessRequest(HttpRequest request)
    {
        Console.WriteLine($"Method: {request.Method}");
        Console.WriteLine($"Path: {request.Path}");

        if (request.Path == "/ping")
        {
            return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\npong";
        }

        return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\nnot found";
    }
}
