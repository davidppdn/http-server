using System.Net;
using System.Net.Sockets;

class Program
{
    private static readonly Int32 _port = 13000;
    private static readonly IPAddress _address = IPAddress.Parse("127.0.0.1");

    static void Main(string[] args)
    {
        TcpListener? server = null;
        try
        {
            // Create a TCP/IP socket, and start listening for incoming connections.
            server = new(_address, _port);
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String? data = null;

            while (true)
            {
                Console.Write("Waiting for a connection... ");

                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                data = null;

                NetworkStream stream = client.GetStream();

                int i;

                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", data);
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