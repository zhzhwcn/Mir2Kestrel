using System.ComponentModel.DataAnnotations;
using System.Drawing;
using FreeSql.DataAnnotations;

namespace ServerKestrel.Mir2Amz.Models
{
    internal class Character
    {
        public int Index { get; set; }

        public int AccountIndex { get; set; }
        [Navigate(nameof(AccountIndex))]
        public Account? Account { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;
        public ushort Level { get; set; }
        public MirClass Class { get; set; }
        public MirGender Gender { get; set; }
        public byte Hair { get; set; }
        public int GuildIndex { get; set; } = -1;

        public string CreationIP { get; set; } = string.Empty;
        public DateTime? CreationDate { get; set; }

        public bool Banned { get; set; }
        public string BanReason { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }

        public bool ChatBanned { get; set; }
        public DateTime? ChatBanExpiryDate { get; set; }

        public string LastIP { get; set; } = string.Empty;
        public DateTime LastLogoutDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }

        //Location
        public string? CurrentMapIndex { get; set; }
        public Point CurrentLocation { get; set; }
        public MirDirection Direction { get; set; }
        public int BindMapIndex { get; set; }
        public Point BindLocation { get; set; }

        public int HP { get; set; }
        public int MP { get; set; }
        public long Experience { get; set; }

        public AttackMode AMode { get; set; }
        public PetMode PMode { get; set; }
        public bool AllowGroup { get; set; }
        public bool AllowTrade { get; set; }
        public bool AllowObserve { get; set; }

        public int PKPoints { get; set; }
    }
}