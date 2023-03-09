using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using FreeSql.DataAnnotations;

namespace ServerKestrel.Mir2Amz.Models
{
    internal class Account : IAccount
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Index { get; set; }

        [Required] 
        public string AccountID { get; set; } = null!;

        [Required] 
        public string Password { get; set; } = null!;

        public string? UserName { get; set; }

        public string SecretQuestion { get; set; } = string.Empty;
        public string SecretAnswer { get; set; } = string.Empty;
        public string EMailAddress { get; set; } = string.Empty;

        public string CreationIP { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public bool Banned { get; set; }
        public bool RequirePasswordChange { get; set; }
        public string BanReason { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; } = null;
        public int WrongPasswordCount { get; set; }

        public string LastIP { get; set; } = string.Empty;
        public DateTime? LastDate = null;


        //Navigate
        [Navigate(nameof(Character.AccountIndex))]
        public List<Character> Characters { get; set; } = new();
    }
}