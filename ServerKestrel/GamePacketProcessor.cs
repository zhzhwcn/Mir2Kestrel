using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommunityToolkit.HighPerformance;

namespace ServerKestrel
{
    internal class GamePacketProcessor
    {
        private readonly Dictionary<ClientPacketIds, Type> _clientPacketTypes = new();
        private Dictionary<ServerPacketIds, Type> _serverPacketTypes = new();
        private readonly object _packetTypeLoadLock = new();

        private readonly ILogger<GamePacketProcessor> _logger;

        public GamePacketProcessor(ILogger<GamePacketProcessor> logger)
        {
            _logger = logger;
        }

        public void LoadPacketTypes()
        {
            lock (_packetTypeLoadLock)
            {
                _clientPacketTypes.Clear();
                _serverPacketTypes.Clear();
                var packetType = typeof(Packet);
                var types = packetType.Assembly.GetExportedTypes();
                
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(packetType) && !type.IsAbstract)
                    {
                        if (string.IsNullOrEmpty(type.Namespace))
                        {
                            continue;
                        }

                        if (Activator.CreateInstance(type) is Packet p)
                        {
                            if (type.Namespace.Contains("Client") && Enum.IsDefined(typeof(ClientPacketIds), p.Index))
                            {
                                _clientPacketTypes[(ClientPacketIds) p.Index] = type;
                            }
                            else if (type.Namespace.Contains("Server") && Enum.IsDefined(typeof(ServerPacketIds), p.Index))
                            {
                                _serverPacketTypes[(ServerPacketIds) p.Index] = type;
                            }
                        }
                        
                    }
                }
            }
        }

        public IList<Packet> Parse(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
        {
            var memory = buffer.First;
            if (buffer.IsSingleSegment == false)
            {
                memory = buffer.ToArray().AsMemory();
            }

            var size = 0L;
            var packets = new List<Packet>();

            while (TryParse(memory, out var packet, out var readSize))
            {
                size += readSize;
                packets.Add(packet);
                memory = memory[(int)size..];
            }

            consumed = buffer.GetPosition(size);
            return packets;
        }

        private bool TryParse(ReadOnlyMemory<byte> memory, [MaybeNullWhen(false)] out Packet packet,
            out long size)
        {
            packet = default;
            size = 0;
            if (memory.IsEmpty)
            {
                return false;
            }

            var span = memory.Span;

            if (span.Length < 4)
            {
                return false;
            }

            using var stream = memory.AsStream();
            var reader = new BinaryReader(stream);
            var packetLength = reader.ReadInt16();
            var packetIndex = reader.ReadInt16();
            size = stream.Position;
            if (!Enum.IsDefined(typeof(ClientPacketIds), packetIndex))
            {
                _logger.LogWarning("Error PacketIndex:{}", packetIndex);
                return false;
            }

            var clientPacketId = (ClientPacketIds) packetIndex;
            if (!_clientPacketTypes.ContainsKey(clientPacketId))
            {
                _logger.LogWarning("Packet Type Not Found:{}", clientPacketId);
                return false;
            }

            packet = (Packet) Activator.CreateInstance(_clientPacketTypes[clientPacketId])!;
            packet.ReadPacket(reader);
            size = stream.Position;
            return true;
        }
    }
}
