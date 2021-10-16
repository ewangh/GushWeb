using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace GushWeb.Helpers
{
    public class INIhelp
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        //ini文件名称
        private const string inifilename = "GushWeb.ini";
        //获取ini文件路径
        private static string inifilepath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), inifilename);

        public static string GetValue(string section, string key)
        {
            StringBuilder s = new StringBuilder(102400);
            GetPrivateProfileString(section, key, string.Empty, s, 102400, inifilepath);
            return s.ToString().Trim();
        }


        public static void SetValue(string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, inifilepath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}