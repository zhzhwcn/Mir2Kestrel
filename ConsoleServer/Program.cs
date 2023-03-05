using Microsoft.Extensions.Hosting;
using ServerKestrel;

namespace ConsoleServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var builder = Host.CreateDefaultBuilder();
            var builder = WebApplication.CreateBuilder(args);
            await builder.ConfigureMirHost()
                .Build()
                .UseMirServer()
                .RunAsync();
        }
    }
}