using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer
{
    public class BugReporter
    {
        public static IEnumerable<object> ReportBug(string report)
        {
            var bugReport = new BugReport();
            bugReport.Report = report;
            bugReport.ReporterMisparIshi =
                CurrentMisparIshi.GetCurrentMisparIshi();
            bugReport.TimeReported = DateTime.Now;
            
            var dataContext = new LogDataContext();
            dataContext.BugReports.InsertOnSubmit(bugReport);
            dataContext.SubmitChanges();

            return new object[] { new { Success = "Yes" } };

            // TODO(Josh): Figure out why this isn't working and fix it.
            /*try
            {
                Microsoft.Office.Interop.Outlook.Application app =
                    new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem mailItem =
                    app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                mailItem.Subject = "מערכת אנשים - Bug report";
                mailItem.To = "s6073570@iaf.idf.il";
                mailItem.Body = report;
                mailItem.Send();
                return new object[] { new { Success = "Yes" } };
            }
            catch (Exception exception)
            {
                return new object[] { new { exception } };
            }*/
        }
    }
}