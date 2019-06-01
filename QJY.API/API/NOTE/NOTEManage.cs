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
    public class NOTEManage : IWsService
    {

        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(NOTEManage).GetMethod(msg.Action.ToUpper());
            NOTEManage model = new NOTEManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        /// <summary>
        /// 获取记事本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">类型</param>
        /// <param name="P2">查询条件</param>
        /// <param name="strUserName"></param>
        public void GETNOTELIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int DataID = -1;
            int.TryParse(context.Request["ID"] ?? "-1", out DataID);//页码


            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);//页码
            page = page == 0 ? 1 : page;
            int recordCount = 0;
            string strWhere = string.Format(" note.ComId={0} And note.CRUser='{1}'", UserInfo.User.ComId, UserInfo.User.UserName);

            if (P1 != "")//分类
            {
                strWhere += string.Format("And  note.LeiBie={0}", P1);
            }
            if (P2 != "")//内容查询
            {
                strWhere += string.Format(" And note.NoteContent like '%{0}%'", P2);
            }
            if (DataID != -1)
            {

                strWhere += string.Format(" And note.ID = '{0}'", DataID);
            }
            DataTable dt = new SZHL_NOTEB().GetDataPager(" SZHL_NOTE note inner join JH_Auth_ZiDian zd on LeiBie= zd.ID and Class=8  ", " note.ID,note.NoteTitle,note.NoteContent,note.LeiBie,note.CRDate,note.CRUser,note.UPUser,note.UPDDate,zd.TypeName ", 8, page, "note.CRDate desc", strWhere, ref recordCount);

            msg.Result = dt;
            msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8);
        }

        /// <summary>
        /// 添加记事本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void ADDNOTE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_NOTE NOTE = JsonConvert.DeserializeObject<SZHL_NOTE>(P1);

            if (NOTE.NoteContent == null)
            {
                msg.ErrorMsg = "记事本内容不能为空";
                return;
            }


            if (NOTE.ID == 0)
            {
                NOTE.CRDate = DateTime.Now;
                NOTE.CRUser = UserInfo.User.UserName;
                NOTE.ComId = UserInfo.User.ComId;
                new SZHL_NOTEB().Insert(NOTE);

            }
            else
            {
                NOTE.UPDDate = DateTime.Now;
                NOTE.UPUser = UserInfo.User.UserName;
                new SZHL_NOTEB().Update(NOTE);
            }

            msg.Result = NOTE;
        }

        /// <summary>
        /// 删除记事本
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">日报ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELNOTEBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                if (new SZHL_NOTEB().Delete(d => d.ID.ToString() == P1))
                {
                    msg.ErrorMsg = "";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        public void GETNOTEMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);
            SZHL_NOTE sg = new SZHL_NOTEB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = sg;
        }

    }
}