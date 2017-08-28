using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.DataWriters
{
    // Every time an admin makes a change, we want to write it. Not because we're
    // creepy (not to say we're not) but just because we give the admins a lot
    // of power so we want to make sure it isn't abused.
    public class AdminChangeWriter
    {
        public static void WriteAdminChange(string description)
        {
            if (!CurrentMisparIshi.IsAdmin())
            {
                return;
            }
            var message = String.Format(
                "The admin {0} made the following change: {1}.",
                CurrentMisparIshi.GetCurrentMisparIshi(),
                description);
            var newAdminChange = new AdminChange();
            newAdminChange.Description = message;
            newAdminChange.TimeAdded = DateTime.Now;
            var dataContext = new LogDataContext();
            dataContext.AdminChanges.InsertOnSubmit(newAdminChange);
            dataContext.SubmitChanges();
        }
    }
}