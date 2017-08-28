using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.StandardQueryFlow
{
    // Some LinqToSQL magic that lets us query the given fields for an entry
    // for each string in values.
    public static class WhereMatchesQuery
    {
        // The algorithm works as follows:
        // - Every value in standardInputTextValues needs to be a match. So
        //   we need the AND union for each one of standardInputTextValues
        //   being a match.
        // - Unless it is a special term like BIRTHDAY, we determine if it is
        //   a match by testing it against several different fields and see if
        //   it matches any of them. So this internal function is an OR union
        //   over different fields. It follows that our query is an AND union
        //   of OR unions.
        // The following fields are lists of functions to be applied to a
        // person to see a term amtches a person (we take the OR over these).
        // We split it up into different types of fields so we can decide dynamically
        // which ones to search and so we can search them in different ways.

        // TODO(Josh): Person.Location seems to be low-quality so research it more
        // and consider adding it to this list.
        private static Expression<Func<Person, string>>[] STRING_PARTIAL =
            new Expression<Func<Person, string>>[] {
                person => person.JobTitleSearchable,
                person => person.LongWorkTitleSearchable,
                person => person.AlternateNameSearchable,
                person => person.DepartmentSearchable,
                person => person.CompanySearchable,
                // TODO(josh) : Think of a way to move this to starts with/full match.
                // The problem: "בן חור" ,"בת שבע".
                // If you still don't understand, ask Ohav. He gets it.
                person => person.GivenName,
                person => person.Surname,
                // TODO(Ohav): See if this is worth having. It might be cool, but
                // it might hurt performance.
                person => person.WhatIDo
            };

        private static Expression<Func<Person, string>>[] STRING_FULL_MATCH =
            new Expression<Func<Person, string>>[] {
                person => person.Darga
            };

        private static Expression<Func<Person, string>>[] NUMBER_PARTIAL =
            new Expression<Func<Person, string>>[] {
                person => person.MisparIshi,
                person => person.Mobile,
                person => person.WorkPhone,
                person => person.OtherTelephone
            };

        private static MethodInfo STRING_EQUALS_METHOD =
            typeof(string).GetMethod("Equals", new[] { typeof(string) });

        private static Expression<Func<Person, string>> BIRTHDAY_STRING =
            person => person.BirthdayDisplayString;

        public static IQueryable<T> WhereMatches<T>(
            this IQueryable<T> source,
            List<string> standardInputTextValues,
            bool isOnlyNumbers)
        {
            var parameter = Expression.Parameter(typeof(T), "r");
            var body = isOnlyNumbers ?
                getExpressionForNumbers<T>(standardInputTextValues, parameter)
                : getExpressionForStrings<T>(standardInputTextValues, parameter);
            var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
            return source.Where(predicate);
        }

        private static Expression getExpressionForStrings<T>(
            List<string> standardInputTextValues,
            ParameterExpression parameter)
        {
            return standardInputTextValues
                .Select(value =>
                {
                    if (value.Equals(TermExtractions.BIRTHDAY))
                    {
                        var todayAsBirthday = BirthdayUtils.GetNowAsBirthdayString();
                        return BIRTHDAY_STRING.ToExpression<T>(
                            STRING_EQUALS_METHOD, todayAsBirthday, parameter);
                    }

                    // For the partial match strings, we search without quotation marks.
                    var valueForPartialStrings = value.Replace("\"", "");
                    var partialMatches = STRING_PARTIAL.Select(member =>
                        member.ToExpression<T>(
                            "Contains", valueForPartialStrings, parameter));

                    var fullMatches = STRING_FULL_MATCH.Select(
                        member => member.ToExpression<T>(
                            STRING_EQUALS_METHOD, value, parameter));

                    return partialMatches.Union(fullMatches)
                        .Aggregate(Expression.OrElse);
                }).Aggregate(Expression.AndAlso);
        }

        private static Expression getExpressionForNumbers<T>(
            List<string> standardInputTextValues,
            ParameterExpression parameter)
        {
            return standardInputTextValues
                .Select(value =>
                {
                    var valueToSearch = value.ReplaceAll("-", "");
                    return NUMBER_PARTIAL.Select(member =>
                        member.ToExpression<T>("Contains", valueToSearch, parameter))
                        .Aggregate(Expression.OrElse);
                }).Aggregate(Expression.AndAlso);
        }

        private static Expression ToExpression<T>(
            this Expression<Func<Person, string>> member,
            string functionName,
            string valueToSearchFor,
            ParameterExpression parameter)
        {
            return (Expression)Expression.Call(
                Expression.MakeMemberAccess(parameter, ((MemberExpression)member.Body).Member),
                functionName,
                Type.EmptyTypes,
                Expression.Constant(valueToSearchFor));
        }

        private static Expression ToExpression<T>(
            this Expression<Func<Person, string>> member,
            MethodInfo function,
            string valueToSearchFor,
            ParameterExpression parameter)
        {
            return (Expression)Expression.Call(
                Expression.MakeMemberAccess(parameter,
                    ((MemberExpression)member.Body).Member),
                function,
                Expression.Constant(valueToSearchFor));
        }
    }
}