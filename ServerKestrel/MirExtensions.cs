using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServerKestrel
{
    public static class MirExtensions
    {
        public static WebApplicationBuilder ConfigureMirHost(this WebApplicationBuilder builder/*, Action<MirHostBuilder> configure*/)
        {
            var mirHostBuilder = new MirHostBuilder(builder);
            mirHostBuilder.ConfigureServices();
            return builder;
        }

        public static WebApplication UseMirServer(this WebApplication app)
        {
            var packetProcessor = app.Services.GetService<GamePacketProcessor>()!;
            packetProcessor.LoadPacketTypes();
            var packetDispatcher = app.Services.GetService<PacketDispatcher>()!;
            //packetDispatcher
            return app;
        }

        internal static ValueTask<FlushResult> SendPacket(this ConnectionContext context, Packet p)
        {
            var bytes = p.GetPacketBytes();
            return context.Transport.Output.WriteAsync(new ReadOnlyMemory<byte>(bytes.ToArray()));
        }
    }
}
