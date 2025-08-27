using CatRouter;

namespace CatRouterConsole
{
    internal class Program
    {
        public static UdpSocket client;
        public static UdpSocket server;

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting Cat's UDP traffic redirect..");
                CatRouterConfig.LoadFromFile();
                if (CatRouterConfig.Instance.debug)
                    Console.WriteLine("Running in debug mode. It will display packets that are sent to this router and it's size.");

                server = new UdpSocket();
                client = new UdpSocket();

                server.Server(CatRouterConfig.Instance.receiveOnIp, CatRouterConfig.Instance.receiveOnPort);
                client.Client(CatRouterConfig.Instance.redirectToIp, CatRouterConfig.Instance.redirectToPort);

                server.onMessageReceived += (from, buffer, offset, bytes) =>
                {
                    client.Send(buffer, offset, bytes);
                };

                if (CatRouterConfig.Instance.debug)
                {
                    server.onMessageReceived += (from, buffer, offset, bytes) =>
                    {
                        Console.WriteLine("RECV: '{0}': '{1}' bytes; isConnected: '{2}', isBlocking: '{3}'", from.ToString(), bytes, server.socket.Connected, server.socket.Blocking);
                    };
                    client.onMessageSent += (bytes, text) =>
                    {
                        Console.WriteLine("SEND: '{0}' bytes, '{1}' text", bytes, text);
                    };
                }

                Console.WriteLine($"'{CatRouterConfig.Instance.receiveOnIp}:{CatRouterConfig.Instance.receiveOnPort}' -> " +
                    $"'{CatRouterConfig.Instance.redirectToIp}:{CatRouterConfig.Instance.redirectToPort}'");
                Console.WriteLine("\nCatRouter is now ONLINE\nEnter 'stop' to stop the router.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            string line = "";

            while (true)
            {
                line = Console.ReadLine();

                switch (line)
                {
                    case "stop" or "break" or "cancel" or "shutdown":
                        Console.WriteLine("Shutting down..");
                        try
                        {
                            client.socket.Close();
                            client.socket.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        try
                        {
                            server.socket.Close();
                            server.socket.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        server = null;
                        client = null;
                        Console.WriteLine("\nCatRouter is now OFFLINE\n");
                        return;
                }
            }
        }
    }
}
