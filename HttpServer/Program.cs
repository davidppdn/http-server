using HttpServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static readonly Int32 _port = 13000;
    private static readonly IPAddress _address = IPAddress.Parse("127.0.0.1");

    static void Main(string[] args)
    {
        TcpListener? server = null;
        try
        {
            server = new(_address, _port);
            server.Start();

            while (true)
            {
                Console.Write("Waiting for a connection... ");

                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();
                
                StringBuilder requestBuffer = new StringBuilder();
                Byte[] buffer = new Byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestBuffer.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    var remainingBodyLength = 0;

                    while (true)
                    {
                        string current = requestBuffer.ToString();
                        int requestEnd = current.IndexOf("\r\n\r\n");

                        if (requestEnd == -1)
                            break;

                        string headerPart = current.Substring(0, requestEnd + 4);
                        var request = HttpRequest.Parse(headerPart);

                        int bodyStart = requestEnd + 4;

                        if (!request.Headers.TryGetValue("Content-Length", out var cl))
                        {
                            requestBuffer.Remove(0, bodyStart);
                            ProcessRequest(request);
                            continue;
                        }

                        int contentLength = int.Parse(cl);

                        int available = current.Length - bodyStart;

                        if (available < contentLength)
                            break;

                        string body = current.Substring(bodyStart, contentLength);

                        request.Body = body;

                        requestBuffer.Remove(0, bodyStart + contentLength);

                        ProcessRequest(request);
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

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }


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