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

/* 
This program implements a simple TCP server that listens on the loopback IP address (127.0.0.1) and a specified port. 
When a client connects, 
- it accepts the TCP connection, 
- reads a byte stream from the NetworkStream,  
- converts the received data to uppercase, 
- sends it back to the client. 

The server runs in an infinite loop and continues accepting new connections until it is stopped.

There are several limitations to this implementation:

- It handles one client connection at a time, meaning additional clients must wait until the current connection is fully processed.
- It assumes the data is encoded in ASCII, which may not support all character sets.
- It treats each read operation as if it corresponds to a single complete message, 
  but TCP is a stream-based protocol where data can be split across multiple reads or combined into a single read.
 
 */