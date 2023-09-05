using System.Drawing;
using ServerKestrel.Mir2Amz.Models;

namespace ServerKestrel.Mir2Amz.Objects
{
    internal sealed class PlayerObject : HumanObject
    {
        private readonly Character _character;

        public PlayerObject(uint objectId, Character character) : base(objectId)
        {
            _character = character;
        }

        public override ObjectType Race => ObjectType.Player;

        public override string Name
        {
            get => _character.Name;
            set { /*Check if Name exists.*/ }
        }
        public override string? CurrentMapIndex
        {
            get => _character.CurrentMapIndex;
            set => _character.CurrentMapIndex = value;
        }
        public override Point CurrentLocation
        {
            get => _character.CurrentLocation;
            set => _character.CurrentLocation = value;
        }
        public override MirDirection Direction
        {
            get => _character.Direction;
            set => _character.Direction = value;
        }
        public override ushort Level
        {
            get => _character.Level;
            set => _character.Level = value;
        }
        public override int Health => HP;

        public override int MaxHealth => Stats[Stat.HP];

        public int HP
        {
            get => _character.HP;
            set => _character.HP = value;
        }
        public int MP
        {
            get => _character.MP;
            set => _character.MP = value;
        }
        public AttackMode AMode
        {
            get => _character.AMode;
            set => _character.AMode = value;
        }
        public PetMode PMode
        {
            get => _character.PMode;
            set => _character.PMode = value;
        }
        public long Experience
        {
            set => _character.Experience = value;
            get => _character.Experience;
        }

        public long MaxExperience { get; set; }
        public byte Hair
        {
            get => _character.Hair;
            set => _character.Hair = value;
        }
        public MirClass Class => _character.Class;

        public MirGender Gender => _character.Gender;
    }
}
