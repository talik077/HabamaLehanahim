using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    public class DbReader
    {
        // Retrieves the matching people form the Database and wraps each one in a
        // PersonFromDbWrapper object.
        public static IEnumerable<PersonFromDbWrapper> GetPersonsFromDb(
            DbRequest dbRequest)
        {
            var dataContext = new PersonDataContext();
            dataContext.Log = new DebugWriter(); // Toggle comment to log SQL.

            return search(dbRequest, dataContext);
        }

        private static IEnumerable<PersonFromDbWrapper> search(
            DbRequest dbRequest,
            PersonDataContext dataContext)
        {
            return getQueryable(dbRequest, dataContext)
                .Take(dbRequest.NumberToTake)
                .Select(person => new PersonFromDbWrapper(
                        person.MisparIshi,
                        person.GivenName,
                        person.Surname,
                        person.Mail,
                        person.Mobile,
                        person.JobTitle,
                        person.WorkPhone,
                        person.OtherTelephone,
                        person.Fax,
                        person.HomeTelephone,
                        person.LongWorkTitle,
                        person.Picture,
                        person.BirthdayDisplayString,
                        person.Darga,
                        person.Sex,
                        person.Tags,
                        person.WhatIDo))
                    .ToList();
        }

        // Returns the queryable object to the DB.
        private static IQueryable<Person> getQueryable(
            DbRequest dbRequest,
            PersonDataContext dataContext)
        {
            var query = dataContext.Persons.AsQueryable();

            if (dbRequest.ForceMe
                && CurrentMisparIshi.GetCurrentMisparIshi().Length > 0)
            {
                query = query.Where(person => 
                    person.MisparIshi == CurrentMisparIshi.GetCurrentMisparIshi());
                return query;
            }
            
            if (dbRequest.Tags > 1)
            {
                query = query.Where(person =>
                    person.Tags % dbRequest.Tags == 0);
            }
            
            if (dbRequest.StandardInputTextValues.Count != 0)
            {
                // Note that WhereMatches is defined in WhereMatchesQuery.cs.
                query = query.WhereMatches(
                    dbRequest.StandardInputTextValues, dbRequest.IsOnlyNumbers);
            }
            return query;
        }
    }

    // Wraps a subset of fields from the DB person object. We use this object
    // so that we don't pull all of the fields all the time.
    public class PersonFromDbWrapper
    {
        public string MisparIshi { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Mobile { get; set; }
        public string JobTitle { get; set; }
        public string WorkPhone { get; set; }
        public string OtherTelephone { get; set; }
        public string Fax { get; set; }
        public string HomePhone { get; set; }
        public string LongWorkTitle { get; set; }
        public Binary Picture { get; set; }
        public string BirthdayDisplayString { get; set; }
        public string Darga { get; set; }
        public string Sex { get; set; }
        public long Tags { get; set; }
        public string WhatIDo { get; set; }

        public PersonFromDbWrapper(
            string MisparIshi,
            string GivenName,
            string Surname,
            string Mail,
            string Mobile,
            string JobTitle,
            string WorkPhone,
            string OtherTelephone,
            string Fax,
            string HomePhone,
            string LongWorkTitle,
            Binary Picture,
            string BirthdayDisplayString,
            string Darga,
            string Sex,
            long Tags,
            string WhatIDo)
        {
            this.MisparIshi = MisparIshi;
            this.GivenName = GivenName;
            this.Surname = Surname;
            this.Mail = Mail;
            this.Mobile = Mobile;
            this.JobTitle = JobTitle;
            this.WorkPhone = WorkPhone;
            this.OtherTelephone = OtherTelephone;
            this.Fax = Fax;
            this.HomePhone = HomePhone;
            this.LongWorkTitle = LongWorkTitle;
            this.Picture = Picture;
            this.BirthdayDisplayString = BirthdayDisplayString;
            this.Darga = Darga;
            this.Sex = Sex;
            this.Tags = Tags;
            this.WhatIDo = WhatIDo;
        }
    }
    
    class DebugWriter : System.IO.TextWriter
    {
        public override void Write(string value)
        {
            Debug.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Debug.Write(new String(buffer, index, count));
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }
}