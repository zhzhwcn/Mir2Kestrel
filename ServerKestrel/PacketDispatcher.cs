using System.Reflection;

namespace ServerKestrel
{
    public class PacketDispatcher
    {
        private Dictionary<ClientPacketIds, MethodInfo> _clientPacketHandlers = new();
    }
}
