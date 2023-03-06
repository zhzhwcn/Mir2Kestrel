namespace ServerKestrel.Mir2Amz
{
    public class BaseGameService
    {
        [PacketHandle<ClientPackets.ClientVersion>]
        public async ValueTask CheckClientVersion(ClientPackets.ClientVersion packet, GameContext context)
        {
            await context.SendPacket(new ServerPackets.ClientVersion() {Result = 1});
        }

        [PacketHandle<ClientPackets.KeepAlive>]
        public async ValueTask KeepAlive(ClientPackets.KeepAlive packet, GameContext context)
        {
            await context.SendPacket(new ServerPackets.KeepAlive() {Time = packet.Time});
        }
    }
}
