using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.DataAccessLayer;
using WebAPI.Utils;

namespace WebAPI.LogsDashboard
{
    public class Dashboard
    {
        public static IEnumerable<object> ProcessDashboardRequest(
            string dashboardRequestType,
            int numDays)
        {
            if (dashboardRequestType == null)
            {
                return createEmptyResponse("Why are you sending me a null request??");
            }
            if (!CurrentMisparIshi.IsSuperAdmin())
            {
                return createEmptyResponse("Go away you non-super-admin, you.");
            }
            switch (dashboardRequestType)
            {
                case "total_users":
                    return LogDataForGal.GetLogDataForGal("1");
                case "today_users":
                    return LogDataForGal.GetLogDataForGal("today");
                case "last_days_usages":
                    return LastDays.GetUsageForLastDays(
                        numDays, false /* showUsersInsteadOfUsages */);
                case "last_days_users":
                    return LastDays.GetUsageForLastDays(
                        numDays, true /* showUsersInsteadOfUsages */);
                case "tag_searches":
                    return QueriesGetter.GetTagQueries();
                case "common_searches":
                    return QueriesGetter.GetRepeatedQueries();
                case "bug_reports":
                    return BugReports.GetAllBugReports();
                case "shares":
                    return Shares.GetShares();
                case "admin_changes":
                    return AdminChanges.GetAdminChanges();
                case "all_tags":
                    return AllTags.GetTags();
            }
            return createEmptyResponse(String.Format(
                "{0} is not supported.", dashboardRequestType));
        }

        private static IEnumerable<object>
            createEmptyResponse(string errorMessage)
        {
            return new object[] { new { errorMessage } };
        }
    }

    public class DashboardRequest
    {
        public string DashboardType { set; get; }
    }
}