namespace Pfruck.Repad
{
    /// <summary>
    /// Class that contains the entrypoint for the Reverse Proxy sample app.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entrypoint of the application.
        /// </summary>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
