using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using WebAPI.Models;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    // The main entry point for handling a query.
    public class QueryHandler
    {
        // (1) Validate the query and return an empty list if the query is invalid.
        // (2) Convert the input query to a SQL query.
        // (3) Post-process each SQL output to convert it to the JSON the client
        //     expects.
        // (4) Return a list containing a Metadata object and the list of JSON
        //     objects for each person.
        public IEnumerable<object> GetPersons(
            string inputMaybeInEnglish,
            bool shouldShowAll,
            bool forceMe = false)
        {
            var input = EnglishToHebrew.maybeConvertToHebrew(inputMaybeInEnglish);

            var timer = new Timer(input, shouldShowAll);
            var dbRequest = new DbRequest(input, shouldShowAll, forceMe);
            // If we could not crate a valid DB request, get out of here.
            if (!dbRequest.IsValid) {
                return new object[] { };
            }

            // Generate the list of people and the metadata.
            var allMatchingPersons = createMatchingPersonsList(dbRequest);
            var metadataObject =
                createMetadataObject(input, allMatchingPersons, dbRequest);

            // Push them both into a nice list which we will return.
            var returnObjects = new List<object> { metadataObject };
            returnObjects.AddRange(allMatchingPersons.Take(dbRequest.NumberToShow));

            timer.Stop();

            return returnObjects;
        }

        private List<object> createMatchingPersonsList(DbRequest dbRequest)
        {
            // Post process the selected entities; adjusting and renaming some fields.
            return DbReader.GetPersonsFromDb(dbRequest)
                .Select(person =>  new PersonJsonConstructor(person))
                .OrderByDescending(person => person.IsMe)
                .ThenByDescending(person => person.MailFirstCharacter)
                .ThenBy(person => person.Name)
                .Select(person => person.JsonFromClient)
                .ToList();
        }

        private object createMetadataObject(string input,
            IEnumerable<object> persons,
            DbRequest dbRequest)
        { 
            // We requested more than we will show as a means to know if
            // the list was cut off.
            var listWasCutOff = persons.Count() > dbRequest.NumberToShow;
            
            return new {
                query = input,
                shouldShowSeeMore = listWasCutOff
            };
        }
    }
}