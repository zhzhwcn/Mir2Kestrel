using Microsoft.VisualBasic;
using ServerPackets;
using System.Drawing;

namespace ServerKestrel.Mir2Amz.Objects
{
    internal abstract class MapObject
    {
        protected MapObject(uint objectId)
        {
            ObjectId = objectId;
        }

        public uint ObjectId { get; }
        public abstract ObjectType Race { get; }

        public abstract string Name { get; set; }

        private Map? _currentMap;
        public Map? CurrentMap
        {
            set
            {
                _currentMap = value;
                CurrentMapIndex = _currentMap != null ? _currentMap.Info.Index : string.Empty;
            }
            get => _currentMap;
        }

        public abstract string? CurrentMapIndex { get; set; }
        public abstract Point CurrentLocation { get; set; }
        public abstract MirDirection Direction { get; set; }

        public abstract ushort Level { get; set; }

        public abstract int Health { get; }
        public abstract int MaxHealth { get; }
        public byte PercentHealth => (byte)(Health / (float)MaxHealth * 100);

        public byte Light { get; set; }
        public int AttackSpeed { get; set; }
        public Stats Stats { get; set; }

        private MapObject? _target;
        public virtual MapObject? Target
        {
            get => _target;
            set
            {
                if (_target != null && _target == value)
                {
                    return;
                }
                _target = value;
            }
        }

        private bool _inSafeZone;
        public bool InSafeZone {
            get => _inSafeZone;
            set
            {
                if (_inSafeZone == value) return;
                _inSafeZone = value;
                OnSafeZoneChanged();
            }
        }

        public virtual List<Poison> PoisonList { get; set; } = new List<Poison>();
        public PoisonType CurrentPoison = PoisonType.None;
        public List<DelayedAction> ActionList = new List<DelayedAction>();

        public virtual void OnSafeZoneChanged()
        {

        }
    }

    internal class Poison
    {
        private MapObject owner;
        public MapObject Owner
        {
            get 
            { 
                return owner switch
                {
                    //HeroObject hero => hero.Owner,
                    _ => owner
                };
            }
            set => owner = value;
        }
        public PoisonType PType;
        public int Value;
        public long Duration, Time, TickTime, TickSpeed;

        public Poison(MapObject owner)
        {
            this.owner = owner;
        }

        public Poison(BinaryReader reader, MapObject owner)
        {
            this.owner = owner;
            //Owner = null;
            PType = (PoisonType)reader.ReadByte();
            Value = reader.ReadInt32();
            Duration = reader.ReadInt64();
            Time = reader.ReadInt64();
            TickTime = reader.ReadInt64();
            TickSpeed = reader.ReadInt64();
        }
    }
}
