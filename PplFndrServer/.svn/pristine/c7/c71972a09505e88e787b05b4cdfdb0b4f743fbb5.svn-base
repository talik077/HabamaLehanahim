using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.MetadataAccessors
{
    public class TagsToAddGetter
    {
        public static IEnumerable<object> GetTags()
        {
            var dataContext = new PersonDataContext();
            var isAdmin = CurrentMisparIshi.IsAdmin();
            var tagsList = dataContext.TagPrimes
                .Where(tag => tag.AllowNonAdminsToAdd || isAdmin)
                .Select(tag => new {
                    tag = tag.Tag,
                    type = tag.Type
                })
                .ToList();
            
            var groupedTags = tagsList.GroupBy(tag => tag.type);
            return groupedTags.Select(tagGroup => new
            {
                type = tagGroup.Key,
                tags = tagGroup.Select(singleTag => new {
                    tag = singleTag.tag
                }).OrderBy(tagWrapper => tagWrapper.tag)
            });
        }
    }
}