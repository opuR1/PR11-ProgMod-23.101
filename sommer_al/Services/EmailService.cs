using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace sommer_al.Services
{
    /// <summary>
    /// Сервис для отправки email сообщений через SMTP
    /// </summary>
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }

        /// <summary>
        /// Асинхронная отправка email с кодом подтверждения
        /// </summary>
        public async Task SendVerificationCodeAsync(string userEmail, string verificationCode, bool isTwoFactor = false)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpUsername),
                        Subject = isTwoFactor ? "Код двухфакторной аутентификации" : "Восстановление пароля",
                        Body = $@"
                        <html>
                        <body>
                            <h3>{(isTwoFactor ? "Код двухфакторной аутентификации" : "Код для восстановления пароля")}</h3>
                            <p>Ваш код подтверждения: <strong>{verificationCode}</strong></p>
                            <p>Код действителен в течение 10 минут.</p>
                            <p><em>Если вы не запрашивали этот код, проигнорируйте это письмо.</em></p>
                        </body>
                        </html>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(userEmail);
                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки email: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}