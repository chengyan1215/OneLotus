using FastReflectionLib;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using System;
using System.Linq;
using System.Reflection;
using System.Web;

namespace QJY.API
{
    public class YBPManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(YBPManage).GetMethod(msg.Action.ToUpper());
            YBPManage model = new YBPManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }






        public void GETYBLISTDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new BI_DB_YBPB().GetALLEntities();

        }


        public void SAVEDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            BI_DB_YBP model = new BI_DB_YBP();
            model.Name = P1;
            model.YBType = P2;
            model.CRUser = UserInfo.User.UserName;
            model.CRDate = DateTime.Now;
            new BI_DB_YBPB().Insert(model);
            msg.Result = model;
        }

        public void UPYBDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            string strFormName = context.Request["FormName"] ?? "";
            string strFB = context.Request["ISFB"] ?? "N";



            int ID = Int32.Parse(P1);
            BI_DB_YBP model = new BI_DB_YBPB().GetEntities(d => d.ID == ID).FirstOrDefault();
            model.YBContent = P2;
            if (strFormName != "")
            {
                model.Name = strFormName;
            }
            if (strFB == "Y")
            {
                model.YBOption = P2;
            }
            new BI_DB_YBPB().Update(model);
            msg.Result = model;
        }

        public void GETYBBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            BI_DB_YBP model = new BI_DB_YBPB().GetEntities(d => d.ID == ID).FirstOrDefault();
            msg.Result = model;
        }

        public void DELYBDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new BI_DB_YBPB().Delete(D => D.ID == ID);
            new BI_DB_DimB().Delete(D => D.STID == ID);
        }


        /// <summary>
        /// 获取仪表盘数据接口
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETYBDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                JObject wigdata = JObject.Parse(P1);



                string datatype = (string)wigdata["datatype"];//数据来源类型0:SQL,1:API
                if (datatype == "0")//SQL取数据
                {
                    string strWigdetType = (string)wigdata["component"];
                    string strDateSetName = (string)wigdata["datasetname"];
                    string filtervalsql = (string)wigdata["filtervalsql"] ?? "";
                    string ordersql = (string)wigdata["order"] ?? "";


                    string strPageCount = context.Request["pagecount"] ?? "10";
                    string strquerydata = context.Request["querydata"] ?? "";//查询条件数据
                    string isglquery = (string)wigdata["isglquery"] == "True" ? "Y" : "N";//关联查询条件数据
                    string strWhere = "";
                    BI_DB_Set DS = new BI_DB_SetB().GetEntities(d => d.Name == strDateSetName).FirstOrDefault();
                    DBFactory db = new BI_DB_SourceB().GetDB(DS.SID.Value);



                    //默认过滤
                    if (filtervalsql != "")
                    {
                        strWhere = " AND " + filtervalsql;
                    }

                    ///有查询字段数据并且关联查询组件时生成查询条件
                    if (strquerydata != "" && isglquery == "Y")
                    {
                        JArray categories = JArray.Parse(strquerydata);
                        foreach (JObject item in categories)
                        {
                            string FiledName = (string)item["glfiled"];
                            string ColumnType = (string)item["ColumnType"];
                            string eltype = (string)item["component"];
                            if (eltype == "qjInput")
                            {
                                string strValue = (string)item["value"];
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    string strSQL = string.Format(" AND {0} LIKE ('%{1}%')", FiledName.Replace(',', '+'), strValue);
                                    strWhere = strWhere + strSQL;
                                }
                            }
                            if (eltype == "qjSeluser" || eltype == "qjSelbranch")
                            {
                                string strValue = (string)item["value"];
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    string strSQL = string.Format(" AND {0} IN ('{1}')", FiledName.Replace(',', '+'), strValue.ToFormatLike());
                                    strWhere = strWhere + strSQL;
                                }
                            }
                            if (eltype == "qjMonth" || eltype == "qjDate")
                            {
                                if (item["value"] != null && item["value"].ToString() != "")
                                {
                                    string strval = item["value"].ToString();
                                    string sDate = strval.Split(',')[0].ToString();
                                    string eDate = strval.Split(',')[1].ToString();
                                    string strSQL = string.Format(" AND {0} BETWEEN '{1} 00:00' AND '{2} 23:59' ", FiledName, sDate, eDate);
                                    strWhere = strWhere + strSQL;
                                }

                            }

                        }
                    }
                    //if (strWigdetType == "qjTable")
                    //{
                    //    string strTablefiled = "";
                    //    JArray categoriestab = (JArray)wigdata["tabfiledlist"];//查询字段
                    //    foreach (JObject item in categoriestab)
                    //    {
                    //        string FiledName = (string)item["colid"];
                    //        string FiledJSType = (string)item["caltype"] ?? "";

                    //        strTablefiled = strTablefiled + FiledName + ",";

                    //    }

                    //    msg.Result = db.GetYBData(DS, strWD, strDL, strTablefiled, strPageCount, strWhere);

                    //}
                    if (strWigdetType == "qjChartPie" || strWigdetType == "qjKBan" || strWigdetType == "qjTable" || strWigdetType == "qjChartBar")
                    {
                        JArray wdlist = (JArray)wigdata["wdlist"];
                        JArray dllist = (JArray)wigdata["dllist"];
                        string strWD = "";
                        foreach (JObject item in wdlist)
                        {
                            strWD = strWD + (string)item["colid"] + ",";
                        }
                        strWD = strWD.TrimEnd(',');

                        string strDL = "";
                        foreach (JObject item in dllist)
                        {
                            strDL = strDL + " " + (string)item["caltype"] + " (" + (string)item["colid"] + ") AS " + (string)item["colid"] + ",";
                        }
                        strDL = strDL.TrimEnd(',');




                        msg.Result = db.GetYBData(DS, strWD, strDL, strPageCount, strWhere, ordersql);

                    }
                }
                else//API取数据
                {
                    string strAPIUrl = (string)wigdata["apiurl"] + "&szhlcode=" + UserInfo.User.pccode;
                    string str = CommonHelp.GetAPIData(strAPIUrl);
                    msg.Result = str;
                }

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }




        /// <summary>
        /// 验证API数据接口
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void YZAPIDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strAPIUrl = P1 + "&szhlcode=" + UserInfo.User.pccode;
                msg.Result = CommonHelp.GetAPIData(strAPIUrl);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }

        }

    }
}