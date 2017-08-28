using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.Utils
{
    public class CurrentMisparIshi
    {
        private static HashSet<string> ADMINS;
        private static HashSet<string> SUPER_ADMINS;

        static CurrentMisparIshi()
        {
            SetAdmins();
        }

        public static string GetCurrentMisparIshi()
        {
            var userNameSplit = System.Web.HttpContext.Current.Request
                .LogonUserIdentity.Name.Split('\\');
            if (userNameSplit.Length == 0 || userNameSplit[1].Length < 2)
            {
                return "";
            }
            return userNameSplit[1].Substring(1);
        }

        public static bool IsAdmin()
        {
            return SUPER_ADMINS.Contains(GetCurrentMisparIshi());
        }

        public static bool IsSuperAdmin()
        {
            return ADMINS.Contains(GetCurrentMisparIshi());
        }

        public static bool IsCurrentUser(string misparIshi)
        {
            return GetCurrentMisparIshi().Equals(misparIshi);
        }

        public static bool IsCurrentUserOrAdmin(string misparIshi)
        {
            return IsAdmin() || IsCurrentUser(misparIshi);
        }

        public static bool IsAdminButNotCurrentUser(string misparIshi)
        {
            return IsAdmin() && !IsCurrentUser(misparIshi);
        }

        public static void SetAdmins()
        {
            var admins = new AdminDataContext().Admins.ToList();
            ADMINS =
                new HashSet<string>(admins.Select(admin => admin.MisparIshi));
            SUPER_ADMINS =
                new HashSet<string>(admins
                    .Where(admin => admin.IsSuperAdmin)
                    .Select(admin => admin.MisparIshi));

        }
    }
}