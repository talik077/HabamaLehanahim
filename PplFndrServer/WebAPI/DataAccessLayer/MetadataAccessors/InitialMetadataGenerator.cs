using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Utils;

namespace WebAPI.MetadataAccessors
{
    // Responsible for creating the initial metadata object.
    public class InitialMetadataGenerator
    {
        public static IEnumerable<object> GetInitialMetadata()
        {
            return new object[] {
                new {
                    is_admin = CurrentMisparIshi.IsAdmin(),
                    tags_to_add_grouped = TagsToAddGetter.GetTags(),
                    me_as_person = MeGetter.GetMe(),
                    non_admins_can_add_tags = true
                }
            };
        }
    }
}