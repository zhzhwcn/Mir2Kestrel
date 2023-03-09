using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using ServerKestrel.Mir2Amz;
using ServerPackets;

namespace ServerKestrel
{
    internal class GameContext
    {
        private readonly ConnectionContext _connectionContext;

        public GameContext(ConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        internal IAccount? Account { get; set; }
        internal IPlayer? Player { get; set; }
        internal GameStage Stage { get; set; } = GameStage.None;

        public IPAddress? ClientIpAddress
        {
            get
            {
                if (_connectionContext.RemoteEndPoint is IPEndPoint ip)
                {
                    return ip.Address;
                }
                return null;
            }
        }

        public string SessionId => _connectionContext.ConnectionId;

        public bool Disconnected => _connectionContext.ConnectionClosed.IsCancellationRequested;

        public async ValueTask SendPacket(Packet p)
        {
            await _connectionContext.SendPacket(p);
        }

        public async ValueTask Disconnect(byte reason)
        {
            await _connectionContext.SendPacket(new Disconnect(){Reason = reason});
            _connectionContext.Abort();
        }
    }
}
