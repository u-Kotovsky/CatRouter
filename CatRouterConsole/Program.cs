using System.Net;
using CatRouter;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CatRouterConsole
{
    internal class Program
    {
        public static UdpSocket client;
        public static UdpSocket server;

        private const string CONFIG_FILE = "config.yaml";

        private static void OverwriteConfigFile()
        {
            var data = new CatRouterConfig();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(data);
            File.WriteAllText(CONFIG_FILE, yaml, System.Text.Encoding.UTF8);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting..");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();

            string yaml = "";
            CatRouterConfig config;

            if (!File.Exists(CONFIG_FILE))
            {
                OverwriteConfigFile();
                Console.WriteLine("Config file wasn't found, it was created from template.");
            }
            try
            {
                yaml = File.ReadAllText(CONFIG_FILE, System.Text.Encoding.UTF8);
                config = deserializer.Deserialize<CatRouterConfig>(yaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                config = new CatRouterConfig();
            }

            server = new UdpSocket();
            client = new UdpSocket();
            server.Server(config.receiveOnIp, config.receiveOnPort);
            client.Client(config.redirectToIp, config.redirectToPort);

            server.onMessageReceived += (from, buffer, offset, bytes) =>
            {
                client.Send(buffer, offset, bytes);
            };

            if (config.debug)
            {
                server.onMessageReceived += (from, buffer, offset, bytes) =>
                {
                    Console.WriteLine("RECV: {0}: {1}", from.ToString(), bytes);
                };
                client.onMessageSent += (bytes, text) =>
                {
                    Console.WriteLine("SEND: {0}, {1}", bytes, text);
                    Console.WriteLine($"[Redirect] packet {bytes} bytes");
                };
            }

            Console.WriteLine($"[Receive] Server is listening on {config.receiveOnIp}:{config.receiveOnPort}");
            Console.WriteLine($"[Redirect] Client is going to send to {config.redirectToIp}:{config.redirectToPort}");
            Console.WriteLine("CatRouter is ready to redirect packets");
            Console.WriteLine($"Enter 'stop' to stop the router.\n");

            string line = "";

            while (true)
            {
                line = Console.ReadLine();

                switch (line)
                {
                    case "stop" or "break" or "cancel" or "shutdown":
                        Console.WriteLine("Shutting down..");
                        server = null;
                        return;
                }
            }
        }
    }
}
