using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using GushWeb.Models;

namespace GushWeb.Utility
{
    public class XmlSetting
    {
        readonly static string resPath = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", "TempTokenRes.xml");
        readonly static string expireDate = "expireDate";
        readonly static string isUsed = "isUsed";
        readonly static string nodeValue = "value";

        public static List<TempToken> GetNodes(string nodeName, DateTime date)
        {
            XElement xml = XElement.Load(resPath);
            var xnls = xml.Elements()
                .Where(d => d.Attribute(isUsed).Value.ToLower() == "false" && DateTime.Parse(d.Attribute(expireDate).Value) > date)
                .ToList().ConvertAll(d => new TempToken()
                {
                    Token = d.Attribute(nodeValue)?.Value.ToSalt(ConfigEntity.tknSalt),
                    IsUsed = false,
                    ExpireDate = DateTime.Parse(d.Attribute(expireDate).Value),
                });

            return xnls;
        }
    }
}