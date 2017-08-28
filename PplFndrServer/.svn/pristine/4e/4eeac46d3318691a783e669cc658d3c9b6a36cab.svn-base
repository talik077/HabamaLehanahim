using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer
{
    public class Logger
    {
        public static void Log(LogData logData)
        {
            if (logData.SessionId == null || logData.LogType == null)
            {
                return;
            }
            
            var dataContext = new LogDataContext();

            if (CurrentMisparIshi.IsSuperAdmin())
            {
                return;
            }

            var log = getLog(dataContext, logData.SessionId, logData.Source);

            switch (logData.LogType.ToLower())
            {
                case "mail":
                    log.MailSent++;
                    break;
                case "request":
                    log.RequestsMade++;
                    if (logData.Query != null)
                    {
                        var request = new Request();
                        request.ID = logData.SessionId;
                        request.Request1 = logData.Query;
                        request.Time = DateTime.Now;
                        dataContext.Requests.InsertOnSubmit(request);
                    }
                    break;
                case "morepressed":
                    log.MorePressed++;
                    break;
                case "seeallpressed":
                    log.SeeAllPressed++;
                    break;
                case "shared":
                    log.Shared = true;
                    break;
            }
            dataContext.SubmitChanges();
        }

        private static Log getLog(
            LogDataContext dataContext,
            string id,
            string source)
        {
            var logFromDB =
                dataContext.Logs.Where(log => log.ID.Equals(id)).FirstOrDefault();
            if (logFromDB != null)
            {
                return logFromDB;
            }
            var misparIshi = CurrentMisparIshi.GetCurrentMisparIshi();

            var newLog = new Log();
            newLog.TimeCreated = DateTime.Now;
            newLog.ID = id;
            newLog.MailSent = 0;
            newLog.MorePressed = 0;
            newLog.RequestsMade = 0;
            newLog.SeeAllPressed = 0;
            newLog.Shared = false;
            newLog.MisparIshi = misparIshi;
            newLog.GivenName = getGivenNameForMisparIshi(misparIshi);
            newLog.Source = source;
            dataContext.Logs.InsertOnSubmit(newLog);
            return newLog;
        }

        private static string getGivenNameForMisparIshi(string misparIshi)
        {
            var dataContext = new PersonDataContext();
            return dataContext.Persons
                .Where(person => person.MisparIshi.Equals(misparIshi))
                .Select(person => String.Format("{0} {1}",
                    person.GivenName, person.Surname))
                .FirstOrDefault();
        }
    }

    public class LogData
    {
        public string SessionId { get; set; }
        public string LogType { get; set; }
        public string Query { get; set; }
        public string Source { get; set; }
    }
}