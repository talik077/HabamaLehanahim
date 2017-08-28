using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    // Generates a simple JSON for the all the admin changes that have been made.
    public class AdminChanges
    {
        public static IEnumerable<object> GetAdminChanges()
        {
            return new LogDataContext().AdminChanges
                .ToList()
                .Select(adminChange => new
                {
                    message = adminChange.Description
                                + " This happened at "
                                + adminChange.TimeAdded.ToString("dd.MM, HH:mm")
                });
        }
    }
}