using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GushWeb.Utility
{
    public class ConfigEntity
    {
        public readonly static string emailAccount = ConfigurationManager.AppSettings["emailAccount"];
        public readonly static string emailPwd = ConfigurationManager.AppSettings["emailPassword"];
        public readonly static string emailSmtp = ConfigurationManager.AppSettings["emailSmtp"];
        public readonly static string pwSalt = ConfigurationManager.AppSettings["passwordSalt"];
        public readonly static string tknSalt = ConfigurationManager.AppSettings["tokenSalt"];
        public const string NodeName = "tempTokens";
    }
}