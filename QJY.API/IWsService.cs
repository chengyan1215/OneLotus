using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace QJY.API
{
  public  interface IWsService
    {
      void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo);
    }
}
