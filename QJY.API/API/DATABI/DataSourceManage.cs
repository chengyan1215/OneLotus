using FastReflectionLib;
using Newtonsoft.Json;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web;

namespace QJY.API
{
    public class DataSourceManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(DataSourceManage).GetMethod(msg.Action.ToUpper());
            DataSourceManage model = new DataSourceManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        //测试数据源连接
        public void TESTBIDBSOURCE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Source>(P1);
            var db = new DBFactory(tt.DBType, tt.DBIP, tt.Port, tt.DBName, tt.Schema, tt.DBUser, tt.DBPwd);
            if (db.TestConn())
            {
                msg.Result = "1"; //1：代表连接成功
            }
            else
            {
                msg.ErrorMsg = "连接失败";
            }

        }

        //获取数据源
        public void GETBIDBSOURCELIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = "1=1";
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new BI_DB_SourceB().GetDataPager(" BI_DB_Source ", "*", pagecount, page, " CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        //添加修改数据源
        public void ADDBIDBSOURCE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Source>(P1);
            if (tt.ID == 0)
            {
                tt.CRUser = UserInfo.User.UserName;
                tt.CRDate = DateTime.Now;
                new BI_DB_SourceB().Insert(tt);
            }
            else
            {
                new BI_DB_SourceB().Update(tt);
            }
            msg.Result = tt;

        }

        //删除数据源
        public void DELBIDBSOURCE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                new BI_DB_SourceB().Delete(d => d.ID == ID);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        //获取数据集表名和视图名
        public void GETBIDBSOURCEVIEWLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(ID);

            msg.Result = db.GetTables();

        }
        public void GETBIFILEDSLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(ID);
            string strTableName = P2;
            msg.Result = db.GetFiledS(strTableName);



        }

        /// <summary>
        /// 生成数据集
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>

        public void ADDBISETLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            DBFactory db = new BI_DB_SourceB().GetDB(ID);
            string strTableName = P2;
            string strDataSetName = context.Request["DsetName"] ?? "1";





            BI_DB_Set DS = new BI_DB_Set();
            DS.Name = strDataSetName;
            DS.SID = ID;
            DS.SName = strTableName;
            DS.CRDate = DateTime.Now;
            DS.CRUser = UserInfo.User.UserName;
            DS.Type = "SQL";
            DS.DSQL = "SELECT  * FROM " + strTableName;
            new BI_DB_SetB().Insert(DS);




            DataTable dt = db.GetSQL(CommonHelp.Filter("SELECT TOP 1 * FROM " + strTableName));
            List<BI_DB_Dim> ListDIM = new BI_DB_SetB().getCType(dt);
            ListDIM.ForEach(D => D.STID = DS.ID);
            ListDIM.ForEach(D => D.CRDate = DateTime.Now);
            ListDIM.ForEach(D => D.CRUser = UserInfo.User.UserName);

            new BI_DB_DimB().Insert(ListDIM);




        }




    }
}