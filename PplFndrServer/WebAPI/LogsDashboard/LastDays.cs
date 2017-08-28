using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    public class LastDays
    {
        public static IEnumerable<object> GetUsageForLastDays(
            int numberOfDaysToShow,
            bool showUsersInsteadOfUsages)
        {
            if (numberOfDaysToShow < 1)
            {
                numberOfDaysToShow = 20;
            }
            var dataContext = new LogDataContext();
            var usageOnDays = new object[numberOfDaysToShow];
            for (var i = 0; i < numberOfDaysToShow; i++)
            {
                var usages = dataContext.GetUsageOnDaysBeforeToday(
                    i, showUsersInsteadOfUsages);
                usageOnDays[i] =  new { usages };
            }
            return usageOnDays.Reverse();
        }
    }

    static class UsageOnDaysExtension
    {
        public static int GetUsageOnDaysBeforeToday(
            this LogDataContext dataContext,
            int daysBeforeToday,
            bool showUsersInsteadOfUsages)
        {
            var targetDate = DateTime.Today.AddDays(daysBeforeToday * -1);
            return dataContext.Logs
                .Where(log => log.TimeCreated.Value.Date == targetDate)
                .GroupBy(log => showUsersInsteadOfUsages ? log.MisparIshi : log.ID)
                .Count();
        }
    }
}