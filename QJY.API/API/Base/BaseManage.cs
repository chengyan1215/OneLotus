using QJY.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using FastReflectionLib;
using System.Data;
using QJY.Data;
using Newtonsoft.Json;
using Senparc.Weixin.Work.Entities;

namespace QJY.API
{
    public class BaseManage
    {

        /// <summary>
        /// 删除业务数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">模块数据</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public virtual void DELMODEL(HttpContext context, JH_Auth_UserB.UserInfo UserInfo, Msg_Result msg, string strModelCode, string strDataID)
        {
            try
            {

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        public virtual void GETMODEL(HttpContext context, JH_Auth_UserB.UserInfo UserInfo, Msg_Result msg, string strModelCode, string strDataID)
        {
            try
            {

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


    }
}