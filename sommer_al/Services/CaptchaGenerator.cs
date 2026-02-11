using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sommer_al.Services
{
    internal class CaptchaGenerator
    {
        private static readonly Random random = new Random();
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateCaptchaText(int lenght)
        {
            if (lenght <= 0)
            {
                throw new ArgumentException("Длина текста капчи должна быть больше нуля!");
            }

            StringBuilder captchaText = new StringBuilder(lenght);
            for (int i = 0; i < lenght; i++)
            {
                int index = random.Next(Characters.Length);
                captchaText.Append(Characters[index]);
            }

            return captchaText.ToString();
        }
    }
}
