using FastReflectionLib;
using Newtonsoft.Json;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web;
using System.Linq;

namespace QJY.API
{
    public class DataSetManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(DataSetManage).GetMethod(msg.Action.ToUpper());
            DataSetManage model = new DataSetManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        //获取数据集
        public void GETBIDBSETLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " 1=1 ";

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new BI_DB_SetB().GetDataPager(" BI_DB_Set T left join BI_DB_Source S on T.SID=S.ID ", " T.*,S.Name AS SJY,S.DBType ", pagecount, page, " T.CRDate desc ", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
            msg.Result2 = new BI_DB_SourceB().GetEntities(d => d.DBType == "SQLSERVER");

        }

        //添加修改数据集
        public void ADDBIDBSET(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tt = JsonConvert.DeserializeObject<BI_DB_Set>(P1);
            if (tt.ID == 0)
            {
                tt.CRUser = UserInfo.User.UserName;
                tt.CRDate = DateTime.Now;
                new BI_DB_SetB().Insert(tt);
            }
            else
            {
                tt.UPDate = DateTime.Now;
                new BI_DB_SetB().Update(tt);
            }


            msg.Result = tt;

        }

        //删除数据集
        public void DELBIDBSET(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                new BI_DB_SetB().Delete(d => d.ID == ID);
                new BI_DB_DimB().Delete(d => d.STID == ID);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        public void GETBIDBSET(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                msg.Result = new BI_DB_SetB().GetEntity(d => d.ID == ID);
                msg.Result1 = new BI_DB_DimB().GetEntities(d => d.STID == ID && d.Dimension == "1");
                msg.Result2 = new BI_DB_DimB().GetEntities(d => d.STID == ID && d.Dimension == "2");

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }

        public void UPBIDSET(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string WD = context.Request["WD"] ?? "";
                string DL = context.Request["DL"] ?? "";
                var tt = JsonConvert.DeserializeObject<BI_DB_Set>(P1);
                tt.UPDate = DateTime.Now;
                new BI_DB_SetB().Update(tt);

                List<BI_DB_Dim> ListWD = JsonConvert.DeserializeObject<List<BI_DB_Dim>>(WD);
                List<BI_DB_Dim> ListDL = JsonConvert.DeserializeObject<List<BI_DB_Dim>>(DL);
                new BI_DB_DimB().Delete(D => D.STID == tt.ID);

                ListWD.ForEach(D => D.CRDate = DateTime.Now);
                ListWD.ForEach(D => D.CRUser = UserInfo.User.UserName);

                ListDL.ForEach(D => D.CRDate = DateTime.Now);
                ListDL.ForEach(D => D.CRUser = UserInfo.User.UserName);

                new BI_DB_DimB().Insert(ListWD);
                new BI_DB_DimB().Insert(ListDL);


            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }


        /// <summary>
        /// 仪表盘页面使用数据集数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETYBDATASET(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                DataTable dt = new BI_DB_SetB().GetDTByCommand("select *  from BI_DB_Set  ORDER BY  ID DESC");
                dt.Columns.Add("wd", Type.GetType("System.Object"));
                dt.Columns.Add("dl", Type.GetType("System.Object"));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["wd"] = new BI_DB_SetB().GetDTByCommand("select * from BI_DB_Dim WHERE STID='" + dt.Rows[i]["ID"].ToString() + "' AND Dimension='1' ORDER BY  ColumnName");
                    dt.Rows[i]["dl"] = new BI_DB_SetB().GetDTByCommand("select * from BI_DB_Dim WHERE STID='" + dt.Rows[i]["ID"].ToString() + "' AND Dimension='2' ORDER BY  ColumnName");
                }
                msg.Result = dt;
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }



        public void JXSQL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int ID = int.Parse(P1);
                BI_DB_Set DS = new BI_DB_SetB().GetEntity(d => d.ID == ID);
                DBFactory db = new BI_DB_SourceB().GetDB(DS.SID.Value);
                DataTable dt = new DataTable();
                dt = db.GetSQL(CommonHelp.Filter(P2));
                List<BI_DB_Dim> ListDIM = new BI_DB_SetB().getCType(dt);
                ListDIM.ForEach(D => D.STID = ID);
                msg.Result = ListDIM.Where(D => D.Dimension == "1");
                msg.Result1 = ListDIM.Where(D => D.Dimension == "2");

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }
    }
}