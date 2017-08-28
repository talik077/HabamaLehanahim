using WebAPI.DataAccessLayer;
using WebAPI.DataAccessLayer.StandardQueryFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using WebAPI.LogsDashboard;
using WebAPI.MetadataAccessors;
using WebAPI.DataAccessLayer.DataWriters;

namespace WebAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<object> Get([FromUri]Request request)
        {
            switch (request.Type)
            {
                case "query":
                    return new QueryHandler().GetPersons(
                        request.Data, request.ShowAll, request.ForceMe);
                case "initialmetadata":
                    return InitialMetadataGenerator.GetInitialMetadata();
                case "adminaddtag":
                    return AdminTagAdder.AddTag(
                        request.Data, request.TagForAnyone, request.TagType);
                case "addtag":
                    var new_tag =
                        TagAdder.AddTagForPerson(request.MisparIshi, request.Data);
                    return new object[] { new { new_tag } };
                case "getme":
                    return MeGetter.GetMe();
                case "dashboard":
                    return Dashboard.ProcessDashboardRequest(
                        request.DashboardRequestType,
                        request.DashboardNumDays);
            }
            return new object[] { };
        }

        // The following exist only for debugging purposes or old versions.
        // Feel free to ignore unless you know you're debugging with them.
        // GET api/values/name
        public IEnumerable<object> Get(string value, string showAll)
        {
            switch (showAll)
            {
                case "galwantsdata":
                    return LogDataForGal.GetLogDataForGal(value);
                case "0":
                    return new QueryHandler().GetPersons(
                        value, false, false);
                case "1":
                    return new QueryHandler().GetPersons(
                        value, true, false);
                case "reportoddity":
                    return BugReporter.ReportBug(value);
            }
            return new object[] { };
        }

        // POST api/values
        public void Post([FromBody]Request request)
        {
            switch (request.Type)
            {
                case PersonalFieldAdder.WHAT_I_DO:
                case PersonalFieldAdder.ADD_MOBILE:
                case PersonalFieldAdder.ADD_WORK_NUMBER:
                case PersonalFieldAdder.ADD_OTHER_NUMBER:
                case PersonalFieldAdder.SEX:
                    PersonalFieldAdder.AddPersonalField(
                        request.MisparIshi, request.Data, request.Type);
                    return;
                case "log":
                    Logger.Log(request.Logs);
                    return;
                case "deletetag":
                    TagAdder.DeleteTagForPerson(request.MisparIshi, request.Data);
                    return;
                case "reportoddity": // It's an oddity. We don't write bugs.
                    BugReporter.ReportBug(request.Data);
                    return;
                case "addadmin":
                    AdminAdder.AddAdmin(request.MisparIshi, request.IsSuperAdmin);
                    return;
                case "deleteadmin":
                    AdminAdder.DeleteAdmin(request.MisparIshi);
                    return;
                case "admindeletetag":
                    AdminTagDeleter.DeleteTag(request.Data);
                    return;
            }
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class Request
    {
        public string Type { get; set; }
        public string MisparIshi { get; set; }
        public string Data { get; set; }
        public LogData Logs { get; set; }
        public bool ShowAll { get; set; }
        public bool ForceMe { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool TagForAnyone { get; set; }
        public string TagType { get; set; }

        // Dashboard request fields.
        public string DashboardRequestType { get; set; }
        public int DashboardNumDays { get; set; }
        // End dashboard request fields.
    }
}