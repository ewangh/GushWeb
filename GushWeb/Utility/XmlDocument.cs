using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using GushWeb.Models;

namespace GushWeb.Utility
{
    public class XmlSetting
    {
        readonly static string resPath = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data","TempTokenRes.xml");
        readonly static string expireDate="expireDate";
        readonly static string isUsed = "isUsed";
        readonly static string nodeValue = "value";

        public static List<TempToken> GetNodes(string nodeName,DateTime date)
        {
            List<TempToken> nodes = new List<TempToken>();
            XmlDocument doc = new XmlDocument();
            doc.Load(resPath);
            XmlNode xn = doc.SelectSingleNode(nodeName);
            // 得到根节点的所有子节点
            XmlNodeList xnls = xn.ChildNodes;
            foreach (XmlNode xn1 in xnls)
            {
                XmlElement xe = (XmlElement)xn1;
                try
                {
                    bool enable = bool.Parse(xe.GetAttribute(isUsed));
                    if(enable)
                        continue;
                    var dt = DateTime.Parse(xe.GetAttribute(expireDate));
                    if (dt > date)
                    {
                        var tk = new TempToken()
                        {
                            Token = xe.GetAttribute(nodeValue),
                            IsUsed = false,
                            ExpireDate = dt,
                        };
                        nodes.Add(tk);
                    }
                }
                catch (Exception e)
                {
                    
                }
                
            }

            return nodes;
        }
    }
}