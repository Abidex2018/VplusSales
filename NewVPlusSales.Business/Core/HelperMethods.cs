using System;
using System.Configuration;
using System.Linq;
using NewVPlusSale.APIObjects.Common;
using PlugPortalManager.APIContract;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Business.Core
{
    internal  class HelperMethods
    {
        internal static bool IsUserValid(int adminUserId, string token, string[] roleNames, ref APIResponseMessage response)
        {
            try
            {
                var obj = new UserAuthObj { AdminUserId = adminUserId, SysPathCode = token };
                var user = PlugPortalManager.AdminPortalService.GetPortalUser(obj);
                if (user == null || user.Status.IsSuccessful == false || user.Users == null || !user.Users.Any())
                {
                    response.FriendlyMessage = "Invalid / Unauthorized User";
                    response.TechnicalMessage = "Invalid / Unauthorized User";
                    return false;
                }
                if (user.Users.Count != 1)
                {
                    response.FriendlyMessage = "Invalid / Unauthorized User";
                    response.TechnicalMessage = "Invalid / Unauthorized User";
                    return false;
                }

                if (PlugPortalManager.AdminPortalService.IsUserInRole(adminUserId, roleNames)) return true;
                response.FriendlyMessage = "Unauthorized Access";
                response.TechnicalMessage = "User does not belong to any role! ";
                return false;


            }
            catch (Exception ex)
            {
                response.FriendlyMessage = "Unable to authenticate Admin User";
                response.TechnicalMessage = "Error: " + ex.Message;
                return false;
            }
        }

        internal static bool IsUserValid(int adminUserId, string token, ref APIResponseMessage response)
        {
            try
            {
                var obj = new UserAuthObj { AdminUserId = adminUserId, SysPathCode = token };
                var user = PlugPortalManager.AdminPortalService.GetPortalUser(obj);
                if (user == null || user.Status.IsSuccessful == false || user.Users == null || !user.Users.Any())
                {
                    response.FriendlyMessage = "Invalid / Unauthorized User";
                    response.TechnicalMessage = "Invalid / Unauthorized User";
                    return false;
                }
                if (user.Users.Count != 1)
                {
                    response.FriendlyMessage = "Invalid / Unauthorized User";
                    response.TechnicalMessage = "Invalid / Unauthorized User";
                    return false;
                }

                var roleSearch = new RoleSearchObj
                {
                    UserId = adminUserId,
                    AdminUserId = adminUserId,
                    SysPathCode = token
                };
                var roles = PlugPortalManager.AdminPortalService.GetAllRoles(roleSearch);
                if (roles == null || roles.Status.IsSuccessful == false || !roles.Roles.Any())
                {
                    response.FriendlyMessage = "Unauthorized User";
                    response.TechnicalMessage = "User does not belong to any role! Main Error: " + roles.Status.Message.FriendlyMessage;
                    return false;
                }


                return true;
            }
            catch (Exception ex)
            {
                response.FriendlyMessage = "Unable to authenticate Admin User";
                response.TechnicalMessage = "Error: " + ex.Message;
                return false;
            }
        }

        #region Roles
        internal static string[] getAdminRoles()
        {
            return new[] { "PortalAdmin", "SiteAdmin" };
        }

        internal static string[] getStaffAdminRoles()
        {
            return new[] { "PortalAdmin", "AdminManager", "AdminOfficer" };
        }

        internal static string[] getPortalAdminRoles()
        {
            return new[] { "PortalAdmin" };
        }

        internal static string[] getAcctExecutiveRoles()
        {
            return new[] { "PortalAdmin", "AccountExecutive" };
        }

        internal static string[] getHODExecutiveRoles()
        {
            return new[] { "PortalAdmin", "HODExecutive" };
        }

        internal static string[] getAcctStaffRoles()
        {
            return new[] { "PortalAdmin", "AccountExecutive", "AccountOfficer" };
        }

        internal static string[] getExecutiveRoles()
        {
            return new[] { "PortalAdmin", "ExecutiveAdmin", "AccountExecutive", "HODExecutive" };
        }

        internal static string[] getMgtExecutiveRoles()
        {
            return new[] { "PortalAdmin", "ExecutiveAdmin", "AccountExecutive" };
        }

        internal static string[] getStaffRoles()
        {
            return new[] { "PortalAdmin", "Staff" };
        }

        internal static string[] getRequesterRoles()
        {
            return new[] { "PortalAdmin", "Staff", "AdminOfficer", "AccountOfficer", "AccountExecutive", "HODExecutive", "ExecutiveAdmin" };
        }
        #endregion

        internal static bool shouldClearCache(string itemName)
        {
            try
            {

                var status = ConfigurationManager.AppSettings.Get(itemName);
                return !string.IsNullOrEmpty(status) && DataCheck.IsNumeric(status) && int.Parse(status) == 1;
            }
            catch (Exception)
            {
                return true;
            }
        }

        internal static void clearCache(string cacheName)
        {
            try
            {
                if (CacheManager.GetCache(cacheName) != null)
                {
                    CacheManager.RemoveCache(cacheName);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}

