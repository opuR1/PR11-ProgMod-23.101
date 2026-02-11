using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sommer_al.Services
{
    /// <summary>
    /// Сервис для управления двухфакторной аутентификацией
    /// </summary>
    public class TwoFactorService
    {
        private static readonly Dictionary<string, TwoFactorData> _verificationCodes = new Dictionary<string, TwoFactorData>();
        private static readonly Random _random = new Random();

        // Список пользователей, у которых включена 2FA
        private static readonly HashSet<int> _usersWith2FAEnabled = new HashSet<int>();

        private class TwoFactorData
        {
            public string Code { get; set; }
            public DateTime ExpirationTime { get; set; }
            public int UserId { get; set; }
        }

        /// <summary>
        /// Генерация 4-значного кода подтверждения
        /// </summary>
        public string GenerateVerificationCode()
        {
            return _random.Next(1000, 10000).ToString("D4");
        }

        /// <summary>
        /// Сохранение кода подтверждения для пользователя
        /// </summary>
        public void SaveVerificationCode(string email, string code, int userId)
        {
            _verificationCodes[email] = new TwoFactorData
            {
                Code = code,
                ExpirationTime = DateTime.Now.AddMinutes(10),
                UserId = userId
            };
        }

        /// <summary>
        /// Проверка кода подтверждения
        /// </summary>
        public bool VerifyCode(string email, string inputCode, out int userId)
        {
            userId = 0;

            if (!_verificationCodes.ContainsKey(email))
                return false;

            var data = _verificationCodes[email];

            if (DateTime.Now > data.ExpirationTime)
            {
                _verificationCodes.Remove(email);
                return false;
            }

            if (data.Code != inputCode)
                return false;

            userId = data.UserId;
            _verificationCodes.Remove(email);
            return true;
        }

        /// <summary>
        /// Включение двухфакторной аутентификации для пользователя
        /// </summary>
        public void EnableTwoFactorAuth(int userId)
        {
            _usersWith2FAEnabled.Add(userId);
        }

        /// <summary>
        /// Отключение двухфакторной аутентификации для пользователя
        /// </summary>
        public void DisableTwoFactorAuth(int userId)
        {
            _usersWith2FAEnabled.Remove(userId);
        }

        /// <summary>
        /// Проверка, включена ли двухфакторная аутентификация для пользователя
        /// </summary>
        public bool IsTwoFactorEnabled(int userId)
        {
            return _usersWith2FAEnabled.Contains(userId);
        }
    }
}