using RezaB.API.WebService.NLogExtentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadiusR.API.Netspeed
{
    public class ServiceSettings
    {
        WebServiceLogger Errorslogger = new WebServiceLogger("Errors");
        public string Password(string username)
        {
            if (username != RadiusR.DB.CustomerWebsiteSettings.WebsiteServicesUsername)
            {
                Errorslogger.LogInfo(username, "username is not found");
                // log wrong username
                return "";
            }
            var password = RadiusR.DB.CustomerWebsiteSettings.WebsiteServicesPassword;

            return password;
        }
        public TimeSpan Duration()
        {
            //add CacheDuration
            return Properties.Settings.Default.CacheDuration;
        }
    }
}