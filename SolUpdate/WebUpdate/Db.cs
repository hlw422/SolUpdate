using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace WebUpdate
{
    public class Db
    {
        public static string path;　　 //INI文件名      
        public static string getver(string str)
        {
            string ver = IniReadValue("update", str );
            return ver;
        }
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //读取INI文件指定
        private static string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, path);
            return temp.ToString();
        }
    }
}