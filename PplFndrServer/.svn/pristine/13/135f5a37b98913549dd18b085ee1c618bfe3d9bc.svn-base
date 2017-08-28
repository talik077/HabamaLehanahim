using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.MetadataAccessors
{
    public class MeGetter
    {

        public static IEnumerable<object> GetMe()
        {
            return new PersonDataContext().Persons
                .Where(person =>
                    person.MisparIshi == CurrentMisparIshi.GetCurrentMisparIshi())
                .Select(person => new {
                    name = person.GivenName,
                    // TODO(Josh): Extract this is as it is common functionality.
                    full_name = String.Format("{0} {1}", person.GivenName, person.Surname),
                    mispar_ishi = person.MisparIshi,
                    picture = person.Picture,
                    activity_level = person.GetUserTagLevel(),
                    department = person.Department})
                .ToList();
        }



        private static long getTagPrime(string tagName)
        {
            return new PersonDataContext().TagPrimes.Where(
                tagPrime => tagPrime.Tag.Equals(tagName))
            .Select(tagPrime => tagPrime.PrimeId)
            .FirstOrDefault();
        }
    }

    static class MeGetterExtensions
    {
        private static long superUserTag = getTagPrime("משתמש_על");
        private static long heavyUserTag = getTagPrime("משתמש_הרבה");
        private static long lightUserTag = getTagPrime("משתמש_קל");

        // Returns a one-based integer indicating how active the user is
        // with 1 being the most active.
        public static int GetUserTagLevel(this Person person)
        {
            if (person.hasTag(superUserTag))
            {
                return 1;
            }
            if (person.hasTag(heavyUserTag))
            {
                return 2;
            }
            if (person.hasTag(lightUserTag))
            {
                return 3;
            }
            return -1;
        }

        private static bool hasTag(this Person person, long tag)
        {
            return person.Tags % tag == 0;
        }


        private static long getTagPrime(string tagName)
        {
            return new PersonDataContext().TagPrimes
            .Where(tagPrime => tagPrime.Tag.Equals(tagName))
            .Select(tagPrime => tagPrime.PrimeId)
            .FirstOrDefault();
        }
    }
}