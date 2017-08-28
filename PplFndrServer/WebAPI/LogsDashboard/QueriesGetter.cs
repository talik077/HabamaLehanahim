using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    public class QueriesGetter
    {
        private static int QUERY_REPEAT_THRESHOLD = 5;

        // Returns a list of all queries that have been made more than
        // QUERY_REPEAT_THRESHOLD times.
        public static IEnumerable<object> GetRepeatedQueries()
        {
            return new LogDataContext().Requests
                .GroupBy(request => request.Request1)
                .Select(group => new
                {
                    Query = group.Key,
                    Count = group.Count()
                })
                .Where(group => group.Count >= 5)
                .OrderByDescending(group => group.Count);
        }

        // Returns a list of all queries with tags and how many times
        // each one has been made.
        public static IEnumerable<object> GetTagQueries()
        {
            return new LogDataContext().Requests
                .Where(request => request.Request1.Contains("#"))
                .GroupBy(request => request.Request1)
                .Select(group => new
                {
                    Query = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(group => group.Count); ;
        }
    }
}