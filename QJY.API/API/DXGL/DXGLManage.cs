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
using QJY.Common;

namespace QJY.API
{
    public class DXGLManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(DXGLManage).GetMethod(msg.Action.ToUpper());
            DXGLManage model = new DXGLManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }
        /// <summary>
        /// 获取短信列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">短信内容</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETDXGLLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " ComId=" + UserInfo.User.ComId + " And CRUser='"+userName+"'";
            if (P1 != "")
            {
                strWhere += string.Format(" And  dxContent like '%{0}%'", P1);
            }
            string status = context.Request["status"] ?? "";
            if (status != "")
            {
                strWhere += string.Format(" And  SendTime{0}getdate()", status=="0"?"<":">");
            }           
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new SZHL_DXGLB().GetDataPager(" SZHL_DXGL ", "ID,dxContent,dxnums,SendTime,CRUser,CRDate,case when SendTime<=getdate() then '已发送' else '待发送' end as status", pagecount, page, " CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        //获取短信内容
        public void GETDXGLMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_DXGL sd = new SZHL_DXGLB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = sd;
        }

        /// <summary>
        /// 添加短信内容并发送短信
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDDXGL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                SZHL_DXGL dxgl = JsonConvert.DeserializeObject<SZHL_DXGL>(P1);
                if (dxgl.dxContent.Trim() != "")
                {
                    dxgl.CRUser = UserInfo.User.UserName;
                    dxgl.CRDate = DateTime.Now;
                    dxgl.SendTime = dxgl.SendTime == null ? DateTime.Now : dxgl.SendTime;
                    dxgl.ComId = UserInfo.User.ComId.Value;
                    //发送短信
                    new SZHL_DXGLB().Insert(dxgl);

                    //消息提醒
                    SZHL_TXSX TX = new SZHL_TXSX();
                    TX.Date = dxgl.SendTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    TX.APIName = "DXGL";
                    TX.ComId = UserInfo.User.ComId;
                    TX.FunName = "DXGL_CHECK";
                    TX.MsgID = dxgl.ID.ToString();
                    TX.TXContent = dxgl.dxContent;
                    TX.TXUser = dxgl.dxnums;
                    TX.TXMode = "DXGL";
                    TX.CRUser = UserInfo.User.UserName;
                    TXSX.TXSXAPI.AddALERT(TX); //时间为发送时间
                }
                else
                {
                    msg.ErrorMsg = "请输入短信内容";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        /// <summary>
        /// 删除短信信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">短信信息Id</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELDXGL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                var dx = new SZHL_DXGLB().GetEntity(d => d.ID == ID);
                if (dx.SendTime > DateTime.Now)
                {
                    new SZHL_TXSXB().Delete(p => p.MsgID == P1 && p.TXMode == "DXGL");
                }

                new SZHL_DXGLB().Delete(d => d.ID == ID);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        public void DXGL_CHECK(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX TX = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            if (!string.IsNullOrEmpty(TX.TXUser))
            {
                // UserInfo = new JH_Auth_UserB().GetUserInfo(TX.ComId.Value,TX.CRUser);
                //发送微信消息
                new SZHL_DXGLB().SendSMS(TX.TXUser, TX.TXContent, TX.ComId.Value);

            }


        }
        public void GETDXQTY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            decimal DXCost = decimal.Parse(CommonHelp.GetConfig("DXCost"));
            if (UserInfo.QYinfo.ComId != 0)
            {
                int qty = (int)((UserInfo.QYinfo.AccountMoney.HasValue ? UserInfo.QYinfo.AccountMoney.Value : 0) / DXCost);
                msg.Result = qty.ToString();
            }
            else
            {
                msg.Result = "10000";
            }

        }

    }
}