using System;
using System.Collections.Generic;
using System.Linq;
using sommer_al.Models;

namespace sommer_al.Services
{
    public class PasswordRecoveryService
    {
        private static readonly Dictionary<string, RecoveryData> _recoveryCodes = new Dictionary<string, RecoveryData>();
        private static readonly Random _random = new Random();

        private class RecoveryData
        {
            public string Code { get; set; }
            public DateTime ExpirationTime { get; set; }
            public string Email { get; set; }
        }

        public string GenerateRecoveryCode()
        {
            return _random.Next(1000, 10000).ToString("D4");
        }

        public bool StartPasswordRecovery(string loginOrEmail, out string userEmail)
        {
            userEmail = null;

            using (var db = ProgramModPR5Entities3.GetContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == loginOrEmail);

                if (user == null)
                    return false;

                userEmail = user.Email;

                var code = GenerateRecoveryCode();
                _recoveryCodes[userEmail] = new RecoveryData
                {
                    Code = code,
                    ExpirationTime = DateTime.Now.AddMinutes(10),
                    Email = userEmail
                };

                return true;
            }
        }

        public bool VerifyRecoveryCode(string email, string inputCode)
        {
            if (!_recoveryCodes.ContainsKey(email))
                return false;

            var data = _recoveryCodes[email];

            if (DateTime.Now > data.ExpirationTime)
            {
                _recoveryCodes.Remove(email);
                return false;
            }

            if (data.Code != inputCode)
                return false;

            _recoveryCodes.Remove(email);
            return true;
        }

        public bool ChangePassword(string email, string newPassword)
        {
            try
            {
                using (var db = ProgramModPR5Entities3.GetContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);

                    if (user == null)
                        return false;

                    user.Password = Hash.HashPassword(newPassword);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}