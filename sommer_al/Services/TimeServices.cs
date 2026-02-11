using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sommer_al.Services
{
    internal class TimeServices
    {
        public static string GetGreeting()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            //var currentTime = new TimeSpan(09, 30, 0);

            if (currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(12, 0, 0))
                return "Доброе утро";
            else if (currentTime >= new TimeSpan(12, 1, 0) && currentTime <= new TimeSpan(17, 0, 0))
                return "Добрый день";
            else if (currentTime >= new TimeSpan(17, 1, 0) && currentTime <= new TimeSpan(19, 0, 0))
                return "Добрый вечер";
            else
                return "Добро пожаловать";
        }
        public static bool isWorkHours()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            //var currentTime = new TimeSpan(09, 30, 0);
            return currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(19, 0, 0);
        }
    }
}
