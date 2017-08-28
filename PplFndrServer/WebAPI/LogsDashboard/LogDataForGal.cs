using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.DataAccessLayer;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.LogsDashboard
{
    public class LogDataForGal
    {
        public static IEnumerable<object> GetLogDataForGal(string today)
        {
            if (!CurrentMisparIshi.IsAdmin())
            {
                return new object[] { new { message = "Only for admins" } };
            }
            return GetLogObjects(today == "today");
        }

        public static IEnumerable<object> GetLogObjects(bool todayOnly)
        {
            var dataContext = new LogDataContext();
            return dataContext.Logs
                .Where(log => log.MisparIshi != null
                    && (!todayOnly || (log.TimeCreated.HasValue
                    && log.TimeCreated.Value.Date == DateTime.Today)))
                .GroupBy(log => log.MisparIshi)
                .Select(log => new
                {
                    MisparIshi = log.Key,
                    Count = log.Count()
                })
                .Join(dataContext.PersonLogs,
                    logGroup => logGroup.MisparIshi,
                    person => person.MisparIshi,
                    (logGroup, person) => new
                    {
                        Name = person.GivenName + " " + person.Surname,
                        Count = logGroup.Count,
                        Tafkid = person.JobTitle,
                        Darga = person.Darga,
                    })
                .OrderByDescending(personLogs => personLogs.Count)
                .ToList();
        }
    }
}