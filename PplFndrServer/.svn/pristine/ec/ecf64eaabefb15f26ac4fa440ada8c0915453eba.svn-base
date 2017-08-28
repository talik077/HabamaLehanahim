using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.DataAccessLayer.DataWriters
{
    public class AdminAdder
    {
        public static void AddAdmin(string misparIshi,
            bool isSuperAdmin,
            // Whether we should set isSuperAdmin if this admin already exists or
            // just leave the current value.
            bool forceNewAdminLevel = true)
        {
            if (!CurrentMisparIshi.IsSuperAdmin())
            {
                return;
            }
            var dataContext = new AdminDataContext();
            var existingAdmin = dataContext.Admins
                .Where(admin => admin.MisparIshi == misparIshi)
                .FirstOrDefault();
            if (existingAdmin != null)
            {
                if (forceNewAdminLevel)
                {
                    existingAdmin.IsSuperAdmin = isSuperAdmin;
                }
            }
            else
            {
                var newAdmin = new Admin();
                newAdmin.MisparIshi = misparIshi;
                newAdmin.IsSuperAdmin = isSuperAdmin;
                dataContext.Admins.InsertOnSubmit(newAdmin);
            }
            dataContext.SubmitChanges();
            AdminChangeWriter.WriteAdminChange(
                String.Format("Added admin {0}", misparIshi));
            CurrentMisparIshi.SetAdmins();
        }
        
        public static void DeleteAdmin(string misparIshi)
        {
            if (!CurrentMisparIshi.IsSuperAdmin())
            {
                return;
            }
            var dataContext = new AdminDataContext();
            var existingAdmin = dataContext.Admins
                .Where(admin => admin.MisparIshi == misparIshi)
                .FirstOrDefault();
            if (existingAdmin == null || existingAdmin.IsSuperAdmin)
            {
                return;
            }
            dataContext.Admins.DeleteOnSubmit(existingAdmin);
            dataContext.SubmitChanges();
            AdminChangeWriter.WriteAdminChange(
                String.Format("Deleted admin {0}", misparIshi));
            CurrentMisparIshi.SetAdmins();
        }
    }
}