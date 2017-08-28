using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.DataWriters
{
    public class AdminTagAdder
    {
        public static object LOCK = new object();

        public static IEnumerable<object> AddTag(
            string tagToAdd, bool isTagForAnyone, string type)
        {
            lock (LOCK)
            {
                if (!CurrentMisparIshi.IsAdmin())
                {
                    return createResponseObject(
                        "You're not an admin, what are you doing here??");
                }

                if (tagToAdd.Contains(" "))
                {
                    return createResponseObject(
                        "אסור להוסיף תגים עם רווחים");
                }

                if (tagToAdd.Length == 0)
                {
                    return createResponseObject("נו באמת...");
                }

                if (type.Length == 0)
                {
                    return createResponseObject("נא להוסיף סוג");
                }


                var nextPrime = TagToPrimeDictionary.GetNextPrime();
                if (nextPrime == -1)
                {
                    return createResponseObject(
                        "יש 10,000 תגים ויותר מזה לא נתמך");
                }


                var dataContext = new PersonDataContext();
                var alreadyExistingTags =
                    dataContext.TagPrimes.Where(tag => tag.Tag.Equals(tagToAdd));
                if (alreadyExistingTags.Count() > 0)
                {
                    return createResponseObject(
                        String.Format("התג {0} כבר קיים", tagToAdd));
                }

                var newTagPrime = new TagPrime();
                newTagPrime.PrimeId = nextPrime;
                newTagPrime.Tag = tagToAdd;
                newTagPrime.AllowNonAdminsToAdd = isTagForAnyone;
                newTagPrime.Type = type;

                dataContext.TagPrimes.InsertOnSubmit(newTagPrime);
                dataContext.SubmitChanges();

                TagToPrimeDictionary.ResetTagToPrimeDictionaries();

                AdminChangeWriter.WriteAdminChange(
                    String.Format("Added tag {0} of type {1}", tagToAdd, type));

                return createResponseObject(
                    String.Format("התג {0} מסוג {1} התווסף בהצלחה", tagToAdd, type));
            }
        }

        private static IEnumerable<object> createResponseObject(string response) {
            return new object[] { new  { response } };
        }
    }
}