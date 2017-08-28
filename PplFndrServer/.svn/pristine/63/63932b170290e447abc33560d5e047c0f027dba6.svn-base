using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.DataAccessLayer.DataWriters;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    // Converts a PersonFromDbWrapper object to a C# object that will be sent as
    // as JSON to the client.
    public class PersonJsonConstructor
    {
        public object JsonFromClient { get; set; }

        // These fields are exposed to enable sorting by them.
        public bool IsMe { get; set; }
        public char MailFirstCharacter { get; set; }
        public string Name { get; set; }

        private PersonFromDbWrapper person;

        public PersonJsonConstructor(PersonFromDbWrapper person)
        {
            this.person = person;

            this.IsMe = this.getIsMe();
            this.Name = person.GivenName;
            // Sort by the first character of the mail so we have all the
            // S emails before the M emails.
            this.MailFirstCharacter = person.Mail == null ?
                ' ' : person.Mail.FirstOrDefault();

            this.JsonFromClient = toJsonObject(person);
        }

        private object toJsonObject(PersonFromDbWrapper person)
        {
            return new
            {
                mispar_ishi = person.MisparIshi,
                name = this.getDisplayName(),
                mail = person.Mail,
                picture = this.getPictureString(),
                // We show two rows for the person; the top is always
                // shown. The bottom is typically hidden behind an expander.
                top_row = this.createTopRowJson(),
                bottom_section = this.createBottomSectionJson(),
                tags = this.getTags(),
                is_me = this.IsMe,
                is_birthday_today = this.isBirthdayToday(),
            };
        }

        private IEnumerable<object> createTopRowJson()
        {
            var objects = new List<object>();
            objects.AddTopRowDisplayObject(
                "שם", this.getDisplayName(), true);
            objects.AddTopRowDisplayObject(
                "נייד", person.Mobile, true,
                PersonalFieldAdder.ADD_MOBILE);
            objects.AddTopRowDisplayObject(
                "תפקיד", person.LongWorkTitle, false);
            return objects;
        }

        private object createBottomSectionJson()
        {
            var whatIDoRow = new List<object>();
            whatIDoRow.AddBottomRowDisplayObject(
                "מה אני עושה בכמה מילים", person.WhatIDo,
                PersonalFieldAdder.WHAT_I_DO);
            
            var textRow = new List<object>();
            textRow.AddBottomRowDisplayObject(
                "מין", person.Sex,
                PersonalFieldAdder.SEX);
            textRow.AddBottomRowDisplayObject(
                "יום הולדת", this.getBirthdayDisplayString());
            textRow.AddBottomRowDisplayObject(
                "דרגה", person.Darga);
            textRow.AddBottomRowDisplayObject(
                "תיאור", person.JobTitle);

            var numbersRow = new List<object>();
            numbersRow.AddBottomRowDisplayObject(
                "מס' עבודה", person.WorkPhone,
                PersonalFieldAdder.ADD_WORK_NUMBER);
            numbersRow.AddBottomRowDisplayObject(
                "מס' עבודה 2", person.OtherTelephone,
                PersonalFieldAdder.ADD_OTHER_NUMBER);
            numbersRow.AddBottomRowDisplayObject(
                "פקס", person.Fax);
            numbersRow.AddBottomRowDisplayObject(
                "מייל", person.Mail);

            var rows = new List<List<object>>{ whatIDoRow, textRow, numbersRow }
                .Where(row => row.Count > 0);
            return new { rows };
        }

        private string getBirthdayDisplayString()
        {
            return person.BirthdayDisplayString;
        }

        private string getPictureString()
        {
            return person.Picture != null ?
                Convert.ToBase64String(person.Picture.ToArray()) : "";
        }

        private List<object> getTags()
        {
            var tags =  getFactors(person.Tags)
                .Where(factor => TagToPrimeDictionary.PRIME_TO_TAG.ContainsKey(factor))
                .Select(factor => {
                    return TagToPrimeDictionary.PRIME_TO_TAG[factor].ToJson();
                })
                .Where(tagJson => tagJson != null)
                .ToList();

            // The birthday tag gets special treatment. People don't actually
            // have this tag; we just pretend they do. So we add it on our own
            // dyanimcally.
            if (this.isBirthdayToday() && TagToPrimeDictionary.BIRTHDAY_TAG != null)
            {
                tags.Add(TagToPrimeDictionary.BIRTHDAY_TAG.ToJson());
            }
            return tags;
        }

        private bool isBirthdayToday()
        {
            var today = DateTime.Today;
            // TODO(Josh): Extract this because it is shared with the queryable extension class.
            var todayDispalayString = String.Format("{0}.{1}",
                today.Day.ToString("D2"),
                today.Month.ToString("D2"));
            var personBirthday = person.BirthdayDisplayString;
            return personBirthday != null
                && person.BirthdayDisplayString
                .Equals(BirthdayUtils.GetNowAsBirthdayString());
        }

        private IEnumerable<long> getFactors(long number)
        {
            var factors = new HashSet<long>();
            var i = 0;
            while (number > 1 && i < TagToPrimeDictionary.EXISTING_TAG_PRIMES.Count())
            {
                var currentPrime =
                    TagToPrimeDictionary.EXISTING_TAG_PRIMES.ElementAt(i);
                if (number % currentPrime == 0)
                {
                    number /= currentPrime;
                    factors.Add(currentPrime);
                    continue;
                }
                i++;
            }
            return factors.AsEnumerable();

        }

        private String getDisplayName() {
            return String.Format("{0} {1}", person.GivenName, person.Surname);
        }

        private bool getIsMe()
        {
            return CurrentMisparIshi.GetCurrentMisparIshi().Equals(person.MisparIshi);
        }
    }

    public static class JsonExtensions
    {
        public static void AddTopRowDisplayObject(
            this List<object> list,
            string name,
            string value,
            bool fixWidth = false,
            string editApiName = "")
        {
            list.addDisplayFieldObject(name, value, editApiName, fixWidth);
        }

        public static void AddBottomRowDisplayObject(
            this List<object> list,
            string name,
            string value,
            string editApiName = "")
        {
            if ((value == null || value.Length == 0) && editApiName.Length == 0)
            {
                return;
            }
            list.addDisplayFieldObject(name, value, editApiName);
        }
        
        private static void addDisplayFieldObject(
            this List<object> list,
            string name,
            string value,
            string editApiName = "",
            bool fixWidth = false)
        {
            // editApiName is used for fields that can be edited on the client
            // such as phone number. This field is an identifier that will be
            // sent back to the server.
            // fixWidth indicates whether we should give this field a fixed
            // width (determined on the client) or just take up as much as
            // it can.
            list.Add(new { name, value, fixWidth, editApiName });
        }    
    }
}