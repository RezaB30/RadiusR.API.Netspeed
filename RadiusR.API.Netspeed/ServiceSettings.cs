using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadiusR.API.Netspeed
{
    public static class ServiceSettings
    {
        public static string Password(string username)
        {
            return "123456";
        }
        public static TimeSpan Duration()
        {
            return new TimeSpan(0, 2, 0);
        }
    }
}