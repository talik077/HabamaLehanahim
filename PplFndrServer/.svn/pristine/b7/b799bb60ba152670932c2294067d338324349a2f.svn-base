using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.DataWriters
{
    public class AdminTagDeleter
    {
        public static void DeleteTag(string tagName)
        {
            lock (AdminTagAdder.LOCK)
            {
                if (!CurrentMisparIshi.IsAdmin())
                {
                    return;
                }
                if (!TagToPrimeDictionary.TAG_NAME_TO_TAG.ContainsKey(tagName))
                {
                    return;
                }
                var tagToRemoveId = TagToPrimeDictionary.TAG_NAME_TO_TAG[tagName].PrimeId;

                var dataContext = new PersonDataContext();
                var biggestTag = dataContext.TagPrimes
                    .OrderByDescending(tag => tag.PrimeId)
                    .FirstOrDefault();
                var biggestTagId = biggestTag.PrimeId;
                if (biggestTagId < 2)
                {
                    return;
                }

                // Ok so we've isolated the ID of the tag we want to remove R
                // and the ID biggest tag B. We now do the following:
                // (1) Change R's text to  " ". This is a bit of a hack but elsewhere
                // the code will ensure that " " is not searchable or rendered. Note
                // that we do this by deleting it and creating a new one.
                // (2) For every person with tag R, divide their tag value by R.ID.
                //     For every person with tag B, multiple their tag value by R.ID.
                // (3) Change R's text to B's text. Delete B.
                // (4) For every person with tag B, divide their tag value by B.ID.
                // (5) Reset the dictionaries.

                // Step (1).
                dataContext.makeTagToRemoveUnsearchable(tagToRemoveId);

                // Step (2)
                dataContext.RemoveTagIdFromAllPeople(tagToRemoveId);
                dataContext.Persons
                    .Where(person => person.Tags % biggestTagId == 0)
                    .ToList()
                    .ForEach(person => { person.Tags *= tagToRemoveId; });
                dataContext.SubmitChanges();

                // Step (3)
                dataContext.SwitchTagId(biggestTagId, tagToRemoveId);

                // Step (4)
                dataContext.RemoveTagIdFromAllPeople(tagToRemoveId);

                // Step (5)
                TagToPrimeDictionary.ResetTagToPrimeDictionaries();

                AdminChangeWriter.WriteAdminChange(
                    String.Format("Deleted tag {0}", tagName));
            }
        }
    }

    public static class AdminTagDeleterExtensions
    {
        public static TagPrime GetTagPrime(this PersonDataContext dataContext,
            int tagPrimeId)
        {
            return dataContext.TagPrimes
                .Where(tag => tag.PrimeId == tagPrimeId)
                .FirstOrDefault();
        }

        public static void makeTagToRemoveUnsearchable(
            this PersonDataContext dataContext,
            int tagToRemoveId)
        {
            var tagToRemove = dataContext.GetTagPrime(tagToRemoveId);
            var copyOfTagToRemove = new TagPrime();
            copyOfTagToRemove.PrimeId = tagToRemove.PrimeId;
            copyOfTagToRemove.Tag = TagToPrimeDictionary.INVALID_TAG_NAME;

            dataContext.TagPrimes.DeleteOnSubmit(tagToRemove);
            dataContext.SubmitChanges();
            dataContext.TagPrimes.InsertOnSubmit(copyOfTagToRemove);
            dataContext.SubmitChanges();
        }

        public static void RemoveTagIdFromAllPeople(
            this PersonDataContext dataContext,
            int tagPrimeId)
        {
            dataContext.Persons
                .Where(person => person.Tags % tagPrimeId == 0)
                .ToList()
                .ForEach(person => { person.Tags /= tagPrimeId; });
        }

        // Changes the ID of the tag with ID tagPrimeIdFrom to tagPrimeIdFrom.
        // (It actually does this by creating copies and deleting the old ones).
        // The previously existing tag with tagPrimeIdTo will be deleted.
        // If the from and to are the same, that tag is just deleted.
        public static void SwitchTagId(
            this PersonDataContext dataContext,
            int tagPrimeIdFrom,
            int tagPrimeIdTo)
        {
            // Make a copy of the tag with ID tagPrimeIdFrom but with
            // ID tagPrimeIdTo.
            var tagToCopy = dataContext.GetTagPrime(tagPrimeIdFrom);
            var copyOfTag = new TagPrime();
            copyOfTag.Tag = tagToCopy.Tag;
            copyOfTag.Type = tagToCopy.Type;
            copyOfTag.AllowNonAdminsToAdd = tagToCopy.AllowNonAdminsToAdd;
            copyOfTag.PrimeId = tagPrimeIdTo;

            var tagToDelete = dataContext.GetTagPrime(tagPrimeIdTo);
            dataContext.TagPrimes.DeleteOnSubmit(tagToDelete);
            dataContext.TagPrimes.DeleteOnSubmit(tagToCopy);
            dataContext.SubmitChanges();

            if (tagPrimeIdFrom == tagPrimeIdTo)
            {
                return;
            }
            dataContext.TagPrimes.InsertOnSubmit(copyOfTag);
            dataContext.SubmitChanges();
        }
    }
}