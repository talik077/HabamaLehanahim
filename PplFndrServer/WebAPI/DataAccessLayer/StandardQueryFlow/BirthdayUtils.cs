using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    public class BirthdayUtils
    {
        public static string GetNowAsBirthdayString()
        {
            var now = DateTime.Now;
            return String.Format("{0}.{1}",
                now.Day.ToString("D2"),
                now.Month.ToString("D2")); 
        }
    }
}