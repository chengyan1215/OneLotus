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

namespace QJY.API
{
    public class TXLManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(TXLManage).GetMethod(msg.Action.ToUpper());
            TXLManage model = new TXLManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }
        /// <summary>
        /// 获取通讯录列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERTXLLIST_PAGE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" txl.CRUser='{0}'  and txl.ComId={1}", UserInfo.User.UserName, UserInfo.User.ComId);
            if (P1 != "")
            {
                strWhere += string.Format(" And txl.LXName like  '%{0}%'", P1);
            }
            if (P2 != "")
            {
                strWhere += string.Format(" And txl.TagName='{0}'", P2);
            }
            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int total = 0;
            DataTable dt = new SZHL_TXLB().GetDataPager("SZHL_TXL  txl inner join JH_Auth_UserCustomData data on txl.TagName=data.ID", "txl.*,data.DataContent", 8, page, " txl.CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(total * 1.0 / 8);
        }
        /// <summary>
        /// 删除记事本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELTXLBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int Id = int.Parse(P1);
                if (!new SZHL_TXLB().Delete(d => d.ID == Id))
                {

                    msg.ErrorMsg = "删除失败";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        /// <summary>
        /// 获取记事本信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXLMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            msg.Result = new SZHL_TXLB().GetEntity(d => d.ID == Id&&d.ComId==UserInfo.User.ComId);
        }
        /// <summary>
        /// 添加记事本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDTXL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXL TXL = JsonConvert.DeserializeObject<SZHL_TXL>(P1);
            if (TXL.LXName.Trim() == "")
            {
                msg.ErrorMsg = "联系人姓名不能为空";
                return;
            }

            if (TXL.ID == 0)
            {
                List<SZHL_TXL> txl1 = new SZHL_TXLB().GetEntities(d => d.LXHM == TXL.LXHM).ToList();
                if (txl1.Count()>0) {
                    msg.ErrorMsg = "此手机号联系人已存在";
                    return;
                }
                List<SZHL_TXL> txl2 = new SZHL_TXLB().GetEntities(d => d.LXMail == TXL.LXMail).ToList();
                if (txl2.Count() > 0)
                {
                    msg.ErrorMsg = "此邮箱手机号联系人已存在";
                    return;
                }
                TXL.CRDate = DateTime.Now;
                TXL.CRUser = UserInfo.User.UserName;
                TXL.UPDDate = DateTime.Now;
                TXL.UPUser = UserInfo.User.UserName;
                TXL.ComId = UserInfo.User.ComId;
                new SZHL_TXLB().Insert(TXL);
            }
            else
            {

                TXL.UPDDate = DateTime.Now;
                TXL.UPUser = UserInfo.User.UserName;
                new SZHL_TXLB().Update(TXL);
            }
            msg.Result = TXL;
        }
    }
}