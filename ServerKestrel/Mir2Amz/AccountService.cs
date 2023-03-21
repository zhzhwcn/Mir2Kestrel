using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Reflection;
using System.Security.Claims;
using System.Xml.Linq;
using ClientPackets;
using FreeSql.Internal;
using ServerKestrel.Mir2Amz.Models;

namespace ServerKestrel.Mir2Amz
{
    internal class AccountService
    {
        private readonly IFreeSql _sql;
        private readonly Settings _settings;
        private readonly MainProcess _mainProcess;
        private readonly ILogger<AccountService> _logger;

        private readonly Dictionary<int, GameContext> _accountGameContexts = new();

        public AccountService(IFreeSql sql, Settings settings, MainProcess mainProcess, ILogger<AccountService> logger)
        {
            _sql = sql;
            _settings = settings;
            _mainProcess = mainProcess;
            _logger = logger;
        }

        private static bool IsEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        [PacketHandle<NewAccount>]
        public async ValueTask CreateAccount(NewAccount p, GameContext context)
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

        [PacketHandle<ChangePassword>]
        public async ValueTask ChangePassword(ChangePassword p, GameContext context)
        {
            if (!_settings.AllowChangePassword)
            {
                await context.SendPacket(new ServerPackets.ChangePassword { Result = 0 });
                return;
            }
            var account = await _sql.Select<Account>().Where(a => a.AccountID == p.AccountID).ToOneAsync();

            if (account == null)
            {
                await context.SendPacket(new ServerPackets.ChangePassword { Result = 4 });
                return;
            }

            if (account.Banned)
            {
                if (account.ExpiryDate > _mainProcess.Now)
                {
                    await context.SendPacket(new ServerPackets.ChangePasswordBanned { Reason = account.BanReason, ExpiryDate = account.ExpiryDate.Value });
                    return;
                }
                account.Banned = false;
            }
            account.BanReason = string.Empty;
            account.ExpiryDate = DateTime.MinValue;

            if (!BCrypt.Net.BCrypt.Verify(p.CurrentPassword, account.Password))
            {
                await context.SendPacket(new ServerPackets.ChangePassword { Result = 5 });
                return;
            }

            account.Password = BCrypt.Net.BCrypt.HashPassword(p.NewPassword);
            account.RequirePasswordChange = false;
            await _sql.Update<Account>(account.Index).SetSource(account).ExecuteAffrowsAsync();
            await context.SendPacket(new ServerPackets.ChangePassword { Result = 6 });
        }

        [PacketHandle<Login>]
        public async ValueTask Login(Login p, GameContext context)
        {
            if (!_settings.AllowLogin)
            {
                await context.SendPacket(new ServerPackets.Login { Result = 0 });
                return;
            }

            var account = await _sql.Select<Account>().Where(a => a.AccountID == p.AccountID).ToOneAsync();

            if (account == null)
            {
                await context.SendPacket(new ServerPackets.Login { Result = 3 });
                return;
            }

            if (account.Banned)
            {
                if (account.ExpiryDate > _mainProcess.Now)
                {
                    await context.SendPacket(new ServerPackets.LoginBanned
                    {
                        Reason = account.BanReason,
                        ExpiryDate = account.ExpiryDate.Value
                    });
                    await _sql.Update<Account>(account.Index).SetSource(account).ExecuteAffrowsAsync();
                    return;
                }
                account.Banned = false;
            }
            account.BanReason = string.Empty;
            account.ExpiryDate = DateTime.MinValue;

            if (!BCrypt.Net.BCrypt.Verify(p.Password, account.Password))
            {
                if (account.WrongPasswordCount++ >= 5)
                {
                    account.Banned = true;
                    account.BanReason = "Too many Wrong Login Attempts.";
                    account.ExpiryDate = _mainProcess.Now.AddMinutes(2);

                    await context.SendPacket(new ServerPackets.LoginBanned
                    {
                        Reason = account.BanReason,
                        ExpiryDate = account.ExpiryDate.Value
                    });
                    await _sql.Update<Account>(account.Index).SetSource(account).ExecuteAffrowsAsync();
                    return;
                }

                await context.SendPacket(new ServerPackets.Login { Result = 4 });
                return;
            }
            account.WrongPasswordCount = 0;

            if (account.RequirePasswordChange)
            {
                await context.SendPacket(new ServerPackets.Login { Result = 5 });
                return;
            }

            if (_accountGameContexts.ContainsKey(account.Index))
            {
                if (!_accountGameContexts[account.Index].Disconnected)
                {
                    await _accountGameContexts[account.Index].Disconnect(1);
                }

            }

            context.Account = account;
            context.Stage = GameStage.Select;
            _accountGameContexts[account.Index] = context;

            account.LastDate = _mainProcess.Now;
            account.LastIP = context.ClientIpAddress?.ToString() ?? string.Empty;
            
            await _sql.Update<Account>(account.Index).SetSource(account).ExecuteAffrowsAsync();
            _logger.LogInformation(context.SessionId + ", " + context.ClientIpAddress + ", User logged in.");
            await context.SendPacket(new ServerPackets.LoginSuccess { Characters = await GetSelectInfo(account) });
        }

        private async Task<List<SelectInfo>> GetSelectInfo(IAccount account)
        {
            var list = await _sql.Select<Character>().Where(c => c.AccountIndex == account.Index && !c.Deleted)
                .Limit(Globals.MaxCharacterCount)
                .ToListAsync(c => new SelectInfo()
                {
                    Index = c.Index,
                    Name = c.Name,
                    Level = c.Level,
                    Class = c.Class,
                    Gender = c.Gender,
                    LastAccess = c.LastLogoutDate
                });

            return list;
        }

        [PacketHandle]
        public async Task NewCharacter(NewCharacter p, GameContext context)
        {
            if (context.Stage != GameStage.Select)
            {
                return;
            }

            if (context.Account == null)
            {
                return;
            }

            if (!_settings.AllowNewCharacter)
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 0 });
                return;
            }

            // if (!CharacterReg.IsMatch(p.Name))
            // {
            //     await context.SendPacket(new ServerPackets.NewCharacter { Result = 1 });
            //     return;
            // }

            if (_settings.DisabledCharNames.Contains(p.Name.ToUpper()))
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 1 });
                return;
            }

            if (!Enum.IsDefined(p.Gender))
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 2 });
                return;
            }

            if (!Enum.IsDefined(p.Class))
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 3 });
                return;
            }

            if (p.Class == MirClass.Assassin && !_settings.AllowCreateAssassin ||
                p.Class == MirClass.Archer && !_settings.AllowCreateArcher)
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 3 });
                return;
            }

            var count = await _sql.Select<Character>().Where(c => c.AccountIndex == context.Account.Index && !c.Deleted).CountAsync();
            if (++count >= Globals.MaxCharacterCount)
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 4 });
                return;
            }

            if (await _sql.Select<Character>().Where(c => c.Name == p.Name).AnyAsync())
            {
                await context.SendPacket(new ServerPackets.NewCharacter { Result = 5 });
                return;
            }

            var c = new Character()
            {
                AccountIndex = context.Account.Index,
                Name = p.Name,
                Class = p.Class,
                Gender = p.Gender,
                CreationIP = context.ClientIpAddress?.ToString() ?? "Unknown",
                CreationDate = _mainProcess.Now,
            };

            await _sql.Insert(c).ExecuteAffrowsAsync();

            var info = new SelectInfo()
            {
                Index = c.Index,
                Name = c.Name,
                Level = c.Level,
                Class = c.Class,
                Gender = c.Gender,
                LastAccess = c.LastLogoutDate
            };

            await context.SendPacket(new ServerPackets.NewCharacterSuccess { CharInfo = info });
        }

        [PacketHandle]
        public async Task DeleteCharacter(DeleteCharacter p, GameContext context)
        {
            if (context.Stage != GameStage.Select || context.Account == null)
            {
                return;
            }
            
            if (!_settings.AllowDeleteCharacter)
            {
                await context.SendPacket(new ServerPackets.DeleteCharacter { Result = 0 });
                return;
            }

            var temp = await _sql.Select<Character>().Where(c => c.Index == p.Index && c.AccountIndex == context.Account.Index).FirstAsync();

            if (temp == null)
            {
                await context.SendPacket(new ServerPackets.DeleteCharacter { Result = 1 });
                return;
            }

            temp.Deleted = true;
            temp.DeleteDate = _mainProcess.Now;
            //TODO:Envir.RemoveRank(temp);
            await context.SendPacket(new ServerPackets.DeleteCharacterSuccess { CharacterIndex = temp.Index });
        }


    }
}
