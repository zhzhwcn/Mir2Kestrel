using System.ComponentModel.DataAnnotations;
using ServerKestrel.Mir2Amz.Models;

namespace ServerKestrel.Mir2Amz
{
    public class AccountService
    {
        private readonly IFreeSql _sql;
        private readonly Settings _settings;

        public AccountService(IFreeSql sql, Settings settings)
        {
            _sql = sql;
            _settings = settings;
        }

        private static bool IsEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        [PacketHandle<ClientPackets.NewAccount>]
        public async ValueTask CreateAccount(ClientPackets.NewAccount p, GameContext context)
        {
            if (!_settings.AllowNewAccount)
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 0 });
                return;
            }

            // if (!AccountIDReg.IsMatch(p.AccountID))
            // {
            //     await context.SendPacket(new ServerPackets.NewAccount { Result = 1 });
            //     return;
            // }
            //
            // if (!PasswordReg.IsMatch(p.Password))
            // {
            //     await context.SendPacket(new ServerPackets.NewAccount { Result = 2 });
            //     return;
            // }

            if (!string.IsNullOrWhiteSpace(p.EMailAddress) && !IsEmail(p.EMailAddress) ||
                p.EMailAddress.Length > 50)
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 3 });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.UserName) && p.UserName.Length > 20)
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 4 });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretQuestion) && p.SecretQuestion.Length > 30)
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 5 });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretAnswer) && p.SecretAnswer.Length > 30)
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 6 });
                return;
            }

            if (await _sql.Select<Account>().AnyAsync(a => a.AccountID == p.AccountID))
            {
                await context.SendPacket(new ServerPackets.NewAccount { Result = 7 });
                return;
            }
            
            var account = new Account()
            {
                AccountID = p.AccountID,
                Password = BCrypt.Net.BCrypt.HashPassword(p.Password),
                UserName = p.UserName,
                SecretQuestion =  p.SecretQuestion,
                SecretAnswer = p.SecretAnswer,
                CreationIP = context.ClientIpAddress?.ToString() ?? "Unknown",
                CreationDate = DateTime.Now,
                Banned = false,
                BanReason = string.Empty,
                RequirePasswordChange = false,
                ExpiryDate = null,
                WrongPasswordCount = 0,
                LastIP = string.Empty,
                LastDate = null
            };
            var result = await _sql.Insert(account).ExecuteAffrowsAsync();

            await context.SendPacket(new ServerPackets.NewAccount { Result = 8 });
        }
    }
}
