namespace Pfruck.Repad
{
    /// <summary>
    /// Initialiaztion for ASP.NET using YARP reverse proxy
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
#if !NET6_0_OR_GREATER
            // Workaround the lack of distributed tracing support in SocketsHttpHandler before .NET 6.0
            services.AddSingleton<IForwarderHttpClientFactory, DiagnosticsHandlerFactory>();
#endif

            ProxyEntries.Init();
            // Specify a custom proxy config provider, in this case defined in InMemoryConfigProvider.cs
            // Programatically creating route and cluster configs. This allows loading the data from an arbitrary source.
            services.AddReverseProxy()
                .LoadFromMemory(ProxyEntries.GetRoutes(), ProxyEntries.GetClusters());
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
    }
}
