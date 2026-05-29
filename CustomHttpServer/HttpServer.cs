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

                    // try extract a request from the accumilated buffer
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
}
