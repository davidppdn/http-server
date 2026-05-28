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

            Byte[] buffer = new Byte[1024];

            while (true)
            {
                Console.Write("Waiting for a connection... ");

                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();
                StringBuilder request = new StringBuilder();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    request.Append(Encoding.ASCII.GetString(buffer, 0, i));

                    int len = request.Length;

                    if (len >= 4 &&
                        request[len - 4] == '\r' &&
                        request[len - 3] == '\n' &&
                        request[len - 2] == '\r' &&
                        request[len - 1] == '\n')
                    {
                        Console.WriteLine("FULL HTTP REQUEST:");
                        Console.WriteLine(request.ToString());

                        string body = "Hello from custom C# server";

                        string response =
                            "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: text/plain\r\n" +
                            $"Content-Length: {Encoding.ASCII.GetByteCount(body)}\r\n" +
                            "\r\n" +
                            body;

                        byte[] msg = Encoding.ASCII.GetBytes(response);
                        stream.Write(msg, 0, msg.Length);

                        break;
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
}