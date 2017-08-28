using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    // Wraps the data needed to make a request to a DB
    public class DbRequest
    {
        private static Regex isNumberRegex = new Regex(@"^\d+-?\d+$");

        // On a query like "עומר", we won't show all 600+ of them, so
        // we initally show some fixed amount with the option for the
        // user to see all of them.
        private static int NUMBER_TO_SHOW_IN_FIRST_TRY = 15;

        // The standard in put values like "עומר" "חדשנות"
        public List<string> StandardInputTextValues { get; set; }
        // The product of all the prime numbers that correspond to the
        // tags that were searched for.
        public long Tags { get; set; }
                
        // Whether the query should force returning the current user instead
        // of anything else.
        public bool ForceMe { get; set; }
        
        // Queries with only numbers get treated differently because
        // we can optimize by only searching in the numbers fields.
        public bool IsOnlyNumbers { get; set; }

        public int NumberToShow { get; set; }
        // NumberToTake is the amount we request from the DB. We set this to be
        // 1 greater than NumberToShow so that we can know if the list we sent
        // is cut off. (Note that if NumberToShow is infinity, this is irrelevant.
        public int NumberToTake { get; set; }

        // This is like pretty important.
        public bool IsValid { get; set; }

        public DbRequest(string input, bool shouldShowAll, bool forceMe)
        {
            var substrings = InputCleaner.ToCleanInputWords(input);

            this.IsValid = substrings.Count() > 0;
            this.NumberToShow = shouldShowAll ?
                int.MaxValue : NUMBER_TO_SHOW_IN_FIRST_TRY;
            this.NumberToTake = shouldShowAll ?
                int.MaxValue : NUMBER_TO_SHOW_IN_FIRST_TRY + 1;

            this.ForceMe = forceMe;
            this.IsOnlyNumbers =
                substrings.All(substring => isNumberRegex.IsMatch(substring));

            this.StandardInputTextValues = new List<string>();
            this.Tags = 1; // Default value.
            substrings.ToList().ForEach(substring => {
                var tagPrime = getTagPrimeIfExists(substring);
                if (tagPrime != null)
                {
                    // The birthday tag gets special treatment. People don't actually
                    // have this tag; we just pretend they do. So we enable searching
                    // for it by replacing it with the birthday search term.
                    if (tagPrime == TagToPrimeDictionary.BIRTHDAY_TAG)
                    {
                        this.StandardInputTextValues.Add(TermExtractions.BIRTHDAY);
                    }
                    else
                    {
                        this.Tags *= tagPrime.PrimeId;
                    }
                }
                else
                {
                    this.StandardInputTextValues.Add(substring);
                }
            });
        }

        // Returns a tag ID if the string starts with # and we have a tag with this name.
        // Returns -1 otherwise.
        private TagPrime getTagPrimeIfExists(string inputWord)
        {
            if (!inputWord.StartsWith("#"))
            {
                return null;
            }
            var tag = inputWord.Substring(1);
            if (!TagToPrimeDictionary.TAG_NAME_TO_TAG.ContainsKey(tag))
            {
                return null;
            }
            return TagToPrimeDictionary.TAG_NAME_TO_TAG[tag];
        }
    }
}