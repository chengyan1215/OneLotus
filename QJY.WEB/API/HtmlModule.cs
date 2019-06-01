using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QJY.WEB
{
    public class HtmlModule : IHttpModule
    {
        

        public void Init(HttpApplication app)
        {
            app.BeginRequest += new EventHandler(app_BeginRequest);
        }

        void app_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (app.Request.FilePath.Contains("login.html"))
            {
                if (null != app.Request.Cookies["szhlcode"])
                {
                    string val = app.Request.Cookies["szhlcode"].Value;
                }
            }


        }


        public void Dispose() { }
    }
}