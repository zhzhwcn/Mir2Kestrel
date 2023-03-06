using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace ServerKestrel
{
    public class GameContext
    {
        private readonly ConnectionContext _connectionContext;

        public GameContext(ConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        internal Player? Player { get; set; }

        public async ValueTask SendPacket(Packet p)
        {
            await _connectionContext.SendPacket(p);
        }
    }
}
