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
        private int _currentRecoveryRoleId;

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

        public bool StartPasswordRecovery(string loginOrEmail, bool isEmployee, out string userEmail, out string generatedCode)
        {
            userEmail = null;
            generatedCode = null;

            using (var db = ProgramModPR5Entities3.GetContext())
            {
                int targetRoleId = isEmployee ? 1 : 2;

                _currentRecoveryRoleId = targetRoleId;

                var user = db.Users.FirstOrDefault(u => u.Email == loginOrEmail && u.RoleID == targetRoleId);

                if (user == null) return false;

                userEmail = user.Email;
                generatedCode = GenerateRecoveryCode();

                _recoveryCodes[userEmail] = new RecoveryData
                {
                    Code = generatedCode,
                    ExpirationTime = DateTime.Now.AddMinutes(10),
                    Email = userEmail
                };

                return true;
            }
        }
        public string UpdateRecoveryCode(string email)
        {
            string newCode = GenerateRecoveryCode();
            if (_recoveryCodes.ContainsKey(email))
            {
                _recoveryCodes[email].Code = newCode;
                _recoveryCodes[email].ExpirationTime = DateTime.Now.AddMinutes(10);
            }
            return newCode;
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
                using (var db = new ProgramModPR5Entities3())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email && u.RoleID == _currentRecoveryRoleId);

                    if (user == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка: Пользователь с email {email} не найден.");
                        return false;
                    }

                    string hashedPassword = Hash.HashPassword(newPassword);
                    user.Password = hashedPassword;

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    int rowsAffected = db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Пароль успешно обновлен. Строк затронуто: {rowsAffected}");
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА БД: " + ex.Message);
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine("INNER EXCEPTION: " + ex.InnerException.Message);

                return false;
            }
        }
    }
}