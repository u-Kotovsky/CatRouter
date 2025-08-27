using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CatRouter
{
    public class CatRouterConfig
    {
        public string redirectToIp = "127.0.0.1";
        public int redirectToPort = 2222;

        public string receiveOnIp = "127.0.0.1";
        public int receiveOnPort = 6454;

        public bool debug = false;

        private const string CONFIG_FILE = "config.yaml";
        public static CatRouterConfig Instance = null;
        public static bool IsLoaded = Instance != null;
        private static IDeserializer deserializer;
        private static ISerializer serializer;

        public static void LoadFromFile()
        {
            if (deserializer == null)
            {
                deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)  // see height_in_inches in sample yml 
                    .Build();
            }

            if (!File.Exists(CONFIG_FILE))
            {
                Instance = new CatRouterConfig();
                SaveToFile();
                Console.WriteLine("Config file wasn't found, it was created from template.");
            }

            try
            {
                Console.WriteLine($"Reading config file '{CONFIG_FILE}'");
                string yaml = File.ReadAllText(CONFIG_FILE, System.Text.Encoding.UTF8);
                Instance = deserializer.Deserialize<CatRouterConfig>(yaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Instance = new CatRouterConfig();
            }
            Console.WriteLine($"Config was loaded.");
        }

        public static void SaveToFile()
        {
            if (serializer == null)
            {
                serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
            }

            if (Instance == null)
            {
                Instance = new CatRouterConfig();
            }

            var yaml = serializer.Serialize(Instance);
            File.WriteAllText(CONFIG_FILE, yaml, System.Text.Encoding.UTF8);
            Console.WriteLine($"Config was saved to '{CONFIG_FILE}'.");
        }
    }
}
