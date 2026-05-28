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

                    while (true)
                    {
                        string current = requestBuffer.ToString();
                        int requestEnd = current.IndexOf("\r\n\r\n");

                        if (requestEnd == -1)
                            break;

                        string fullRequest = current.Substring(0, requestEnd + 4);
                        requestBuffer.Remove(0, requestEnd + 4);

                        var response = ProcessRequest(fullRequest);
                        byte[] msg = Encoding.ASCII.GetBytes(response);

                        stream.Write(msg, 0, msg.Length);
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

    private static string ProcessRequest(string request)
    {
        Console.WriteLine(request);

        return
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/plain\r\n" +
            "\r\n" +
            "Hello from server";
    }
}