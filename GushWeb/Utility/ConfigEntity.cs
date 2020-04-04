using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GushWeb.Utility
{
    public class ConfigEntity
    {
        private static readonly string _redisService = "127.0.0.1";
        private static readonly int _redisPort = 6379;

        public readonly static string emailAccount = ConfigurationManager.AppSettings["emailAccount"];
        public readonly static string emailPwd = ConfigurationManager.AppSettings["emailPassword"];
        public readonly static string emailSmtp = ConfigurationManager.AppSettings["emailSmtp"];
        public readonly static string pwSalt = ConfigurationManager.AppSettings["passwordSalt"];
        public readonly static string tknSalt = ConfigurationManager.AppSettings["tokenSalt"];
        public readonly static string redisService = ConfigurationManager.AppSettings["redisService"]??_redisService;
        public readonly static int redisPort = IntParse(ConfigurationManager.AppSettings["redisPort"],_redisPort);
        public const string NodeName = "tempTokens";

        private static int IntParse(string v1, int v2)
        {
            Int32.TryParse(v1, out v2);
            return v2;
        }

        private static bool BoolParse(string v1, bool v2)
        {
            bool.TryParse(v1, out v2);
            return v2;
        }

        private static T EnumParse<T>(string v1, T v2) where T : struct, Enum
        {
            Enum.TryParse(v1, false, out v2);
            return v2;
        }
    }
}