using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    // Generates a simple JSON for the number of people who have used the share feature.
    public class Shares
    {
        public static IEnumerable<object> GetShares()
        {
            return new LogDataContext().Logs
                .Where(log => log.Shared.HasValue && log.Shared.Value)
                .GroupBy(log => log.MisparIshi)
                .Select(group => new
                {
                    MisparIshi = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(group => group.Count);
        }
    }
}