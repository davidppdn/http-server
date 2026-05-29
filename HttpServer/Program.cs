using HttpServer;
using HttpServer.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static readonly Int32 _port = 13000;
    private static readonly IPAddress _address = IPAddress.Parse("127.0.0.1");
    private static readonly byte[] delimiter = "\r\n\r\n"u8.ToArray();
    private static readonly byte[] headerDelimiter = "\r\n"u8.ToArray();

    static void Main(string[] args)
    {
        //HttpServer.Tests.HttpRequestTests.RunAll();

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

                byte[] readBuffer = new byte[8192];
                byte[] accumilatedBuffer = new byte[65536];
                int length = 0;

                int bytesRead;

                // this loop ends when the stream is closed => client disconnects
                while ((bytesRead = stream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                {
                    Array.Copy(readBuffer, 0, accumilatedBuffer, length, bytesRead);
                    length += bytesRead;

                    while (true)
                    {
                        int headerEnd = accumilatedBuffer.IndexOf(delimiter);

                        // Case 1: We haven't received the full header yet, wait for more data
                        if (headerEnd == -1)
                            break;

                        ReadOnlySpan<byte> headerSection = accumilatedBuffer.AsSpan(0, headerEnd);
                        var headers = ByteArrayUtilities.ExtractHeader(headerSection);
                        int bodyStart = headerEnd + 4;

                        // Case 2: We have the full header but no Content-Length, so we assume no body and process the request
                        if (!headers.TryGetValue("Content-Length", out var cl))
                        {
                            var requestBytes = accumilatedBuffer.AsSpan(0, bodyStart);
                            var request = HttpRequest.Parse(requestBytes);
                            
                            var response = ProcessRequest(request);
                            stream.Write(Encoding.UTF8.GetBytes(response));
                            
                            var remaining = length - bodyStart;
                            Array.Copy(accumilatedBuffer, bodyStart, accumilatedBuffer, 0, remaining);
                            length = remaining;

                            continue;
                        }

                        int contentLength = int.Parse(cl);
                        int available = length - bodyStart;

                        // Case 3: We have the full header but not the full body yet, wait for more data
                        if (available < contentLength)
                            break;

                        // Case 4: We have the full header and the full body, process the request and remove it from the buffer
                        var fullRequestLength = bodyStart + contentLength;
                        var fullRequest = accumilatedBuffer.AsSpan(0, fullRequestLength);
                        var httpRequest = HttpRequest.Parse(fullRequest);
                        
                        var httpResponse = ProcessRequest(httpRequest);
                        stream.Write(Encoding.UTF8.GetBytes(httpResponse));

                        var remainingBufferLength = length - fullRequestLength;
                        Array.Copy(accumilatedBuffer, fullRequestLength, accumilatedBuffer, 0, remainingBufferLength);
                        length = remainingBufferLength;
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