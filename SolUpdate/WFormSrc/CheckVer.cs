using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFormSrc
{
    class CheckVer
    {
        public string GetServiceVer()
        {

            WebReference.WebService updateService = new WebReference.WebService();
            string ver = updateService.getver();
           return ver;
        }
    }
}
