﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace ServerKestrel
{
    public class MirHostBuilder
    {
        private readonly WebApplicationBuilder _builder;

        public MirHostBuilder(WebApplicationBuilder builder)
        {
            _builder = builder;
        }

        internal MirHostBuilder ConfigureServices()
        {
            var settings = Settings.Load();
            _builder.Services.AddSingleton(settings);
            _builder.Services.AddSingleton<GamePacketProcessor>();
            _builder.Services.AddSingleton<PacketDispatcher>();

            PacketDispatcher.LoadPacketHandlers(_builder.Services);

            _builder.WebHost.UseUrls();
            _builder.WebHost.ConfigureKestrel(((context, options) =>
            {
                //options.ListenLocalhost(5000);
                options.Listen(settings.ListenIp, settings.ListenPort, listenOptions =>
                {
                    listenOptions.UseConnectionHandler<MirConnectionHandler>();
                });
            }));
            return this;
        }
    }
}
