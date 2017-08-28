using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    public class InputCleaner
    {

        private static int MININUM_WORD_LENGTH_REQUIRED = 2;

        // Trims the input from extraneous spaces and verifies validity.
        // Returns null if the input is invalid.
        // Also handles special terms that should be extracted together.
        // TODO(Josh): Clarify the division of responsibilities between
        // this class and DbRequest.cs.
        public static IEnumerable<string> ToCleanInputWords(string input)
        {
            // Trim and eliminate all occurences of consecutive spaces.
            input = input.Trim().ReplaceAll("  ", " ");

            var specialTermsFromInput = new List<string>();
            TermExtractions.TERMS.Keys.ToList().ForEach(termToRemove =>
            {
                if (!input.Contains(termToRemove))
                {
                    return;
                }
                input = input.Without(termToRemove);
                specialTermsFromInput.Add(TermExtractions.TERMS[termToRemove]);
            });

            // Remove all words that appear multiple times.
            var separatedWords = input.Split(' ')
                .Union(specialTermsFromInput)
                .Where(word => word.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .MergeNumberWords();

            // Make sure there is at least one word with
            // MININUM_WORD_LENGTH_REQUIRED characters.
            var hasOneWordWithEnoughCharacters = false;
            separatedWords.ForEach(word =>
            {
                hasOneWordWithEnoughCharacters |=
                    word.Length >= MININUM_WORD_LENGTH_REQUIRED;
            });

            return hasOneWordWithEnoughCharacters ? separatedWords : new List<string>();
        }
    }

    public static class InputCleanerExtensions
    {
        private static Regex isNumberRegex = new Regex(@"^\d+$");

        // For every word in the list at index i such that the word at
        // index i + 1 is an integer and the word at index i is not
        // (e.g., "בח"א" folled by "8"), merge the strings at indices
        // i and i + 1 into one string.
        public static List<string> MergeNumberWords(
            this List<string> strings)
        {
            List<string> unmodifiedWords = new List<string>();
            List<string> mergedWords = new List<string>();
            for (var i = 0; i < strings.Count; i++)
            {
                // If we're always on the last word, add it as unmodified because
                // There is no next word to check.
                if (i == strings.Count - 1
                    // If this is a number or the next one is not a number, we
                    // haven't met the conditions.
                    || isNumberRegex.IsMatch(strings.ElementAt(i))
                    || !isNumberRegex.IsMatch(strings.ElementAt(i + 1))
                    )
                {
                    unmodifiedWords.Add(strings.ElementAt(i));
                    continue;
                }
                
                // If here, string i is not a number and string i + 1 is
                // so instead we add it as a merged word.
                mergedWords.Add(
                    String.Format("{0} {1}",
                    strings.ElementAt(i),
                    strings.ElementAt(i + 1)));
               
                // i + 1 is the number we just added so skip it.
                i++;

            }
            return mergedWords.Union(unmodifiedWords).ToList();
        }
    }
}