using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace ServerKestrel
{
    internal class GameContext
    {
        private readonly ConnectionContext _connectionContext;

        public GameContext(ConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        public Player? Player { get; set; }

        public Task SendPacket()
        {
            throw new NotImplementedException();
        }
    }
}
