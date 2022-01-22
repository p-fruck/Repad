using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Pfruck.Repad
{
    /// <summary>
    /// Initialiaztion for ASP.NET using YARP reverse proxy
    /// </summary>
    public class Startup
    {
        private const string DEBUG_HEADER = "Debug";
        private const string DEBUG_METADATA_KEY = "debug";
        private const string DEBUG_VALUE = "true";

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
#if !NET6_0_OR_GREATER
            // Workaround the lack of distributed tracing support in SocketsHttpHandler before .NET 6.0
            services.AddSingleton<IForwarderHttpClientFactory, DiagnosticsHandlerFactory>();
#endif

            // Specify a custom proxy config provider, in this case defined in InMemoryConfigProvider.cs
            // Programatically creating route and cluster configs. This allows loading the data from an arbitrary source.
            services.AddReverseProxy()
                .LoadFromMemory(GetRoutes(), GetClusters());
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // We can customize the proxy pipeline and add/remove/replace steps
                endpoints.MapReverseProxy();
            });
        }

        private RouteConfig[] GetRoutes()
        {
            return new[]
            {
                new RouteConfig()
                {
                    RouteId = "gitlab",
                    ClusterId = "gitlab",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Hosts = new[]{ "gitlab.localhost" }
                    },
                }.WithTransformPathRemovePrefix(prefix: "/gitlab"),
                new RouteConfig()
                {
                    RouteId = "github",
                    ClusterId = "github",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Hosts = new[]{ "github.localhost" }
                    }
                }
            };
        }
        private ClusterConfig[] GetClusters()
        {
            var debugMetadata = new Dictionary<string, string>();
            debugMetadata.Add(DEBUG_METADATA_KEY, DEBUG_VALUE);

            return new[]
            {
                new ClusterConfig()
                {
                    ClusterId = "gitlab",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new DestinationConfig() { Address = "https://gitlab.com" } },
                    }
                },
                new ClusterConfig()
                {
                    ClusterId = "github",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new DestinationConfig() { Address = "https://github.com" } },
                    }
                },
            };
        }
    }
}
