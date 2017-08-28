using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.DataAccessLayer.DataWriters;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer
{

    // Responsible for adding/removing given tags from given persons.
    public class TagAdder
    {
       
        public static object AddTagForPerson(string misparIshi, string tag)
        {
            return new PersonAndTag(misparIshi, tag).Add();
        }

        public static bool DeleteTagForPerson(string misparIshi, string tag)
        {
            return new PersonAndTag(misparIshi, tag).Delete();
        }
    }

    class PersonAndTag
    {
        private static long HEAD_OF_COMMUNITY_TAG_ID =
            TagToPrimeDictionary.TAG_NAME_TO_TAG.ContainsKey("ראש_קהילה") ?
            TagToPrimeDictionary.TAG_NAME_TO_TAG["ראש_קהילה"].PrimeId : -1;

        private PersonDataContext dataContext;
        private Person personFromDb;
        private long tagPrime;
        private bool isValid;
        private bool isCommunityHeadTag;
        // If this is an admin making a change not for themselves.
        private bool isAdminChange;

        public PersonAndTag(string misparIshi, string tag)
        {
            if (!CurrentMisparIshi.IsCurrentUserOrAdmin(misparIshi)
                || !TagToPrimeDictionary.TAG_TO_PRIME.ContainsKey(tag))
            {
                this.isValid = false;
                return;
            }
            this.isAdminChange =
                CurrentMisparIshi.IsAdminButNotCurrentUser(misparIshi);

            this.tagPrime = TagToPrimeDictionary.TAG_TO_PRIME[tag];

            this.dataContext = new PersonDataContext();

            var personsFromDb = dataContext.Persons
                .Where(person => person.MisparIshi.Equals(misparIshi))
                .ToList();
            if (personsFromDb.Count() != 1)
            {
                this.isValid = false;
                return;
            }
            
            this.personFromDb = personsFromDb.First();
            
            this.isCommunityHeadTag = this.tagPrime == HEAD_OF_COMMUNITY_TAG_ID;

            this.isValid = true;
        }

        public object Add()
        {
            if (!this.isValid || tagAlreadyExists())
            {
                return false;
            }
            if (this.isCommunityHeadTag)
            {
                AdminAdder.AddAdmin(personFromDb.MisparIshi,
                    false, false /* forceNewAdminLevel */);
            }
            if (this.isAdminChange)
            {
                AdminChangeWriter.WriteAdminChange(
                    String.Format(
                    "Added tag {0} for {1}",
                    TagToPrimeDictionary.PRIME_TO_TAG[tagPrime].Tag,
                    personFromDb.MisparIshi));
            }
            long newTagValue = this.personFromDb.Tags * tagPrime;
            if((newTagValue > long.MaxValue || newTagValue < this.personFromDb.Tags) && (newTagValue % tagPrime != 0))
            {
                // Here we check if Tags * tagPrime went over the long.MaxValue. 
                // Note - The value loops around to negative and continutes rising.
                // So, newTagValue will probably never actually be over MaxValue, so we check if the tag exists
                // in the new tag variable. A complete fix will require changing the way we store tags.
                return false;
            }
            this.personFromDb.Tags = newTagValue;
            this.dataContext.SubmitChanges();
            return TagToPrimeDictionary.PRIME_TO_TAG[tagPrime].ToJson();
        }

        public bool Delete()
        {
            if (!this.isValid || !tagAlreadyExists())
            {
                return false;
            }
            if (this.isCommunityHeadTag)
            {
                AdminAdder.DeleteAdmin(personFromDb.MisparIshi);
            }
            if (this.isAdminChange)
            {
                AdminChangeWriter.WriteAdminChange(
                    String.Format(
                    "Deleted tag {0} for {1}",
                    TagToPrimeDictionary.PRIME_TO_TAG[tagPrime].Tag,
                    personFromDb.MisparIshi));
            }
            this.personFromDb.Tags /= tagPrime;
            this.dataContext.SubmitChanges();
            return true;
        }

        private bool tagAlreadyExists()
        {
            return this.personFromDb.Tags % tagPrime == 0;
        }
    }
}