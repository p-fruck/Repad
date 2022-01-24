using System.Text.RegularExpressions;
using YamlDotNet.Serialization.NamingConventions;
using Yarp.ReverseProxy.Configuration;

namespace Pfruck.Repad
{
    public interface IProxyEntry
    {
        String Id { get; }
        String Name { get; }
        String Url { get; }
    }

    public class EntryContent
    {
        public String? Name { get; init; }

        public String? Url { get; init; }
    }

    public class ProxyEntry
    {
        private EntryContent _content;
        public ProxyEntry(String Id, EntryContent content)
        {
            this.Id = Id;
            _content = content;
        }

        public ClusterConfig ToClusterConfig() => new ClusterConfig()
        {
            ClusterId = Id,
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                { "destination1", new DestinationConfig() { Address = Url == null ? "" : Url } },
            }
        };

        public RouteConfig ToRouteConfig() => new RouteConfig()
        {
            RouteId = Id,
            ClusterId = Id,
            Match = new RouteMatch
            {
                // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                Hosts = new[] { Id + ".localhost" }
            },
        };

        public String Id { get; }

        public String? Name { get => _content.Name; }

        public String? Url { get => _content.Url; }
    }

    public class ProxyEntries
    {
        static List<ProxyEntry> _entries = new List<ProxyEntry>();

        public static void Init()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var configDir = new DirectoryInfo("./config");
            var validNames = new Regex(@"^[a-zA-Z0-9_-]+.y(a?)ml$");

            _entries = configDir.GetFiles("*.y*ml")
                .Where(file => validNames.IsMatch(file.Name))
                .Select(file =>
                {
                    var entry = deserializer.Deserialize<EntryContent>(file.OpenText());
                    return new ProxyEntry(file.Name.Split('.')[0], entry);
                })
                .ToList();
        }

        public static RouteConfig[] GetRoutes()
        {
            return _entries.Select(entry => entry.ToRouteConfig()).ToArray();
        }

        public static ClusterConfig[] GetClusters()
        {
            return _entries.Select(entry => entry.ToClusterConfig()).ToArray();
        }
    }
}
