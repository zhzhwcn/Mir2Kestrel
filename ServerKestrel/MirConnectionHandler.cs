using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace ServerKestrel
{
    internal class MirConnectionHandler : ConnectionHandler
    {
        private readonly PacketDispatcher _packetDispatcher;
        private readonly GamePacketProcessor _packetProcessor;
        private readonly ILogger<MirConnectionHandler> _logger;

        public MirConnectionHandler(PacketDispatcher packetDispatcher, GamePacketProcessor packetProcessor, ILogger<MirConnectionHandler> logger)
        {
            _packetDispatcher = packetDispatcher;
            _packetProcessor = packetProcessor;
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            try
            {
                _logger.LogInformation("{} connected", connection.RemoteEndPoint);
                await HandleRequestsAsync(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handel Connection Error");
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        private async Task HandleRequestsAsync(ConnectionContext context)
        {
            var input = context.Transport.Input;
            var gameContext = new GameContext(context);
            await context.SendPacket(new ServerPackets.Connected());
            
            while (!context.ConnectionClosed.IsCancellationRequested)
            {
                var result = await input.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }
                _logger.LogInformation("Bytes Read:{}", result.Buffer.Length);
                var packets = _packetProcessor.Parse(result.Buffer, out var consumed);
                if (packets.Count > 0)
                {
                    foreach (var packet in packets)
                    {
                        _logger.LogInformation("Received Packet[{}]:{}", packet.Index, JsonSerializer.Serialize(packet));
                    }
                    input.AdvanceTo(consumed);
                }
                else
                {
                    input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
    }
}
