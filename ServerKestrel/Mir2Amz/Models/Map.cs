using System.Drawing;

namespace ServerKestrel.Mir2Amz.Models
{
    public class Map
    {
        public Map(string index, string extraInfo)
        {
            Index = index;
            ExtraInfo = extraInfo;
        }

        public string Index { get; set; }
        public string ExtraInfo { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ushort MiniMap { get; set; }
        public ushort BigMap { get; set; }
        public ushort Music { get; set; }
        public LightSetting Light { get; set; }
        public byte MapDarkLight { get; set; } = 0;
        public byte MineIndex { get; set; } = 0;

        public bool NoTeleport { get; set; }
        public bool NoReconnect { get; set; }
        public bool NoRandom { get; set; }
        public bool NoEscape { get; set; }
        public bool NoRecall { get; set; }
        public bool NoDrug { get; set; }
        public bool NoPosition { get; set; }
        public bool NoFight { get; set; }
        public bool NoThrowItem { get; set; }
        public bool NoDropPlayer { get; set; }
        public bool NoDropMonster { get; set; }
        public bool NoNames { get; set; }
        public bool NoMount { get; set; }
        public bool NeedBridle { get; set; }
        public bool Fight { get; set; }
        public bool NeedHole { get; set; }
        public bool Fire { get; set; }
        public bool Lightning { get; set; }
        public bool NoTownTeleport { get; set; }
        public bool NoReincarnation { get; set; }
        public string NoReconnectMap { get; set; } = string.Empty;
        public int FireDamage { get; set; }
        public int LightningDamage { get; set; }

        public List<Movement> Movements = new();
        public List<SafeZone> SafeZones = new();
    }

    public class Movement
    {
        public string ToMapIndex { get; }
        public Point Source { get; set; }
        public Point Destination { get; set; }
        public bool NeedHole { get; set; }
        public bool NeedMove { get; set; }
        public bool ShowOnBigMap { get; set; }
        public int ConquestIndex { get; set; }
        public int Icon { get; set; }

        public Movement(string toMapIndex)
        {
            ToMapIndex = toMapIndex;
        }
    }

    public class SafeZone
    {
        public Point Location { get; set; }
        public ushort Size { get; set; }
        public bool StartPoint { get; set; }
    }
}
