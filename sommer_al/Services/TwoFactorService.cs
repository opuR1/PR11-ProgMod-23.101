using System;
using System.Collections.Generic;
using System.Linq;
using sommer_al.Models;

namespace sommer_al.Services
{
    public class TwoFactorService
    {
        private static readonly Dictionary<string, TwoFactorData> _verificationCodes = new Dictionary<string, TwoFactorData>();
        private static readonly Random _random = new Random();

        private class TwoFactorData
        {
            public string Code { get; set; }
            public DateTime ExpirationTime { get; set; }
            public int UserId { get; set; }
        }

        public string GenerateVerificationCode() => _random.Next(1000, 10000).ToString("D4");

        public void SaveVerificationCode(string email, string code, int userId)
        {
            _verificationCodes[email] = new TwoFactorData
            {
                Code = code,
                ExpirationTime = DateTime.Now.AddMinutes(10),
                UserId = userId
            };
        }

        public bool VerifyCode(string email, string inputCode, out int userId)
        {
            userId = 0;
            if (!_verificationCodes.ContainsKey(email)) return false;

            var data = _verificationCodes[email];
            if (DateTime.Now > data.ExpirationTime)
            {
                _verificationCodes.Remove(email);
                return false;
            }

            if (data.Code != inputCode) return false;

            userId = data.UserId;
            _verificationCodes.Remove(email);
            return true;
        }

        public bool IsTwoFactorEnabled(int userId)
        {
            using (var db = new ProgramModPR5Entities3())
            {
                var user = db.Users.Find(userId);
                return user != null && user.IsTwoFactorEnabled == 1;
            }
        }

        public void EnableTwoFactorAuth(int userId)
        {
            UpdateStatus(userId, 1);
        }

        public void DisableTwoFactorAuth(int userId)
        {
            UpdateStatus(userId, 0);
        }

        private void UpdateStatus(int userId, int status)
        {
            using (var db = new ProgramModPR5Entities3())
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    user.IsTwoFactorEnabled = status;
                    db.SaveChanges();
                }
            }
        }
    }
}