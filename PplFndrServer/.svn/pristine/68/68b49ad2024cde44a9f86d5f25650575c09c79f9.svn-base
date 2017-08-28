using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    public class BugReports
    {
        public static IEnumerable<object> GetAllBugReports()
        {
            var dataContext = new LogDataContext();
            return dataContext
                .BugReports
                .OrderByDescending(bugReport => bugReport.TimeReported)
                .ToList()
                .Select(bugReport => new {
                    reporter = bugReport.ReporterMisparIshi,
                    report = bugReport.Report,
                    whenCreated = bugReport.TimeReported.ToString("dd.MM @ HH:mm"),
                });
        }
    }
}