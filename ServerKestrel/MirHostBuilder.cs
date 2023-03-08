using Microsoft.AspNetCore.Hosting;
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
            var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, "data source=user.db")
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                .Build(); //请务必定义成 Singleton 单例模式
            _builder.Services.AddSingleton(settings);
            _builder.Services.AddSingleton<GamePacketProcessor>();
            _builder.Services.AddSingleton<PacketDispatcher>();
            _builder.Services.AddSingleton<IFreeSql>(fsql);

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
