using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace GushWeb.Utility
{
    public class CookieDocument
    {
        public static void WriteCookie(string cn, string cv, DateTime Time)
        {
            HttpCookie cookie = new HttpCookie(cn)
            {
                Value = HttpUtility.UrlEncode(cv, Encoding.GetEncoding("GB2312")),
                Expires = Time
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string ReadCookie(string cn)
        {
            try
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[cn];
                return HttpUtility.UrlDecode(cookie.Value, Encoding.GetEncoding("GB2312"));
            }
            catch(Exception e)
            {
                return null;
            }
        }
        
    }
}