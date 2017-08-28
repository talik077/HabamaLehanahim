using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.DataWriters
{
    // Some fields such as phone number or sex can be edited by the user. This is
    // the class that handles such edit requests from the client.
    public class PersonalFieldAdder
    {
        public const string ADD_MOBILE = "personalfield-addmobile";
        public const string ADD_WORK_NUMBER = "personalfield-addworknumber";
        public const string ADD_OTHER_NUMBER = "personalfield-addothernumber";
        public const string WHAT_I_DO = "personalfield-whatido";
        public const string SEX = "personalfield-sex";

        public static void AddPersonalField(
            string misparIshi, 
            string value,
            string inputType)
        {
            var dataContext = new PersonDataContext();

            var personsFromDb = dataContext.Persons
                .Where(person => person.MisparIshi.Equals(misparIshi))
                .ToList();
            if (personsFromDb.Count() != 1)
            {
                return;
            }

            var personFromDb = personsFromDb.First();
            
            if (!CurrentMisparIshi.IsCurrentUserOrAdmin(
                personFromDb.MisparIshi))
            {
                return;
            }

            switch (inputType)
            {
                case PersonalFieldAdder.WHAT_I_DO:
                    personFromDb.WhatIDo = value;
                    break;
                case ADD_MOBILE:
                    var normailzedMobileNumber = value.ToValidMobileNumber();
                    if (normailzedMobileNumber == null)
                    {
                        return;
                    }
                    personFromDb.Mobile = normailzedMobileNumber;
                    personFromDb.MobilePhoneIsManuallyInput = true;
                    break;
                case ADD_WORK_NUMBER:
                    var normailzedWorkNumber = value.ToValidNonMobileNumber();
                    if (normailzedWorkNumber == null)
                    {
                        return;
                    }
                    personFromDb.WorkPhone = normailzedWorkNumber;
                    personFromDb.WorkPhoneIsManuallyInput = true;
                    break;
                case ADD_OTHER_NUMBER:
                    var normailzedOtherNumber = value.ToValidNonMobileNumber();
                    if (normailzedOtherNumber == null)
                    {
                        return;
                    }
                    personFromDb.OtherTelephone = normailzedOtherNumber;
                    personFromDb.OtherPhoneIsManuallyInput = true;
                    break;
                case SEX:
                    personFromDb.Sex = value;
                    personFromDb.SexIsManuallyInput = true;
                    break;
            }

            dataContext.SubmitChanges();

            if (CurrentMisparIshi.IsAdminButNotCurrentUser(misparIshi))
            {
                AdminChangeWriter.WriteAdminChange(
                    String.Format(
                    "Set field of type {0} for {1} to {2}",
                    inputType,
                    personFromDb.MisparIshi,
                    value));
            }
        }
    }

    static class MobilePhoneExtension
    {
        private static Regex isMobileNumberRegex = new Regex(@"^\d{10}$");
        private static Regex isNonMobileNumberRegex = new Regex(@"^\d+$");

        // Returns the given mobile number normalized to be in standard form.
        // Returns null if it is deemed invalid.
        public static string ToValidMobileNumber(this string mobileNumber)
        {
            var normalizedNumber = mobileNumber.normalize();
            return isMobileNumberRegex.IsMatch(normalizedNumber)
                || normalizedNumber.Length == 0 ?
                normalizedNumber : null;
        }

        // Returns the given mobile number normalized to be in standard form.
        // Returns null if it is deemed invalid.
        public static string ToValidNonMobileNumber(this string number)
        {
            var normalizedNumber = number.normalize();
            return isNonMobileNumberRegex.IsMatch(normalizedNumber)
                || normalizedNumber.Length == 0 ?
                normalizedNumber : null;
        }

        private static string normalize(this string phoneNumber)
        {
            return phoneNumber.ReplaceAll("-", "").ReplaceAll(" ", "");
        }
    }
}