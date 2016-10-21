using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;

namespace WebUpdate
{
    /// <summary>
    /// WebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
       [WebMethod ]
        public string getver()
        {
            Db.path = AppDomain.CurrentDomain.BaseDirectory + "bin\\Update.ini";
            return Db.getver("version");
        }
       [WebMethod]
       public string[] GetZips()
       {
           string folder = HttpRuntime.AppDomainAppPath + "fileupdate";
           string[] zips = Directory.GetFileSystemEntries(folder);
           for (int i = 0; i < zips.Length; i++)
           {
               zips[i] = Path.GetFileName(zips[i]);
           }
           return zips;
       }
       /// <summary>
       /// 获取下载地址
       /// </summary>
       /// <returns>下载地址</returns>
       [WebMethod]
       public string GetUrl()
       {
           Db.path = AppDomain.CurrentDomain.BaseDirectory + "bin\\Update.ini";
           return Db.getver("url");
       }
    }
}
