using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.LogsDashboard
{
    public class AllTags
    {
        public static IEnumerable<object> GetTags()
        {
            return new PersonDataContext().TagPrimes
                .Select(tag => new
                {
                    type = tag.Type,
                    tag = tag.Tag,
                    can_non_admins_add = tag.AllowNonAdminsToAdd
                })
                .OrderBy(tag => tag.type)
                .ThenBy(tag => tag.tag);
        }
    }
}