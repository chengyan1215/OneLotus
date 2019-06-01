using FastReflectionLib;
using Newtonsoft.Json;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web;

namespace QJY.API
{
    public class TXSXManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(TXSXManage).GetMethod(msg.Action.ToUpper());
            TXSXManage model = new TXSXManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        #region 添加提醒事项
        /// <summary>
        /// 添加提醒事项
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void ADDTXSX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX txsx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);

            if (string.IsNullOrEmpty(txsx.Type))
            {
                msg.ErrorMsg = "提醒方式不能为空";
                return;
            }
            if (string.IsNullOrEmpty(txsx.TXUser) && txsx.Type != "3")
            {
                msg.ErrorMsg = "提醒人不能为空";
                return;
            }
            if (string.IsNullOrEmpty(txsx.TXContent))
            {
                msg.ErrorMsg = "提醒内容不能为空";
                return;
            }
            if (string.IsNullOrEmpty(txsx.TXType))
            {
                msg.ErrorMsg = "提醒模式不能为空";
                return;
            }
            else
            {
                if (txsx.TXType != "0")
                {

                    if (string.IsNullOrEmpty(txsx.Hour) || string.IsNullOrEmpty(txsx.Minute))
                    {
                        msg.ErrorMsg = "提醒时间不能为空";
                        return;
                    }
                    if (txsx.TXType == "1")
                    {
                        if (string.IsNullOrEmpty(txsx.Date))
                        {
                            msg.ErrorMsg = "仅一次模式时，提醒日期不能为空";
                            return;
                        }
                    }
                    if (txsx.TXType == "4")
                    {
                        if (string.IsNullOrEmpty(txsx.Days))
                        {
                            msg.ErrorMsg = "自定义模式时，提醒日期不能为空";
                            return;
                        }
                    }
                    if (txsx.TXType != "1")
                    {
                        if (string.IsNullOrEmpty(txsx.CFType))
                        {
                            msg.ErrorMsg = "重复模式时，重复方式不能为空";
                            return;
                        }
                        else
                        {
                            if (txsx.CFType == "2")
                            {
                                if (txsx.CFCount == null || txsx.CFCount == 0)
                                {
                                    msg.ErrorMsg = "重复次数不能为空或为0";
                                    return;
                                }
                            }
                            else if (txsx.CFType == "3")
                            {
                                if (txsx.CFJZDate == null)
                                {
                                    msg.ErrorMsg = "截止时间不能为空";
                                    return;
                                }
                            }
                        }
                    }
                }
            }


            if (txsx.ID == 0)
            {
                txsx.TXMode = "TXSX";
                txsx.WXLink = "";
                string strID = context.Request["ID"] ?? "";
                string strCode = context.Request["Code"] ?? "";
                string strType = context.Request["Type"] ?? "";
                txsx.WXLink = "A";
                if (!string.IsNullOrEmpty(strID) && !string.IsNullOrEmpty(strCode) && !string.IsNullOrEmpty(strType))
                {
                    txsx.TXMode = strCode;
                    txsx.WXLink = strType;
                    txsx.MsgID = strID;
                }
                txsx.ComId = UserInfo.QYinfo.ComId;
                txsx.Status = "0";
                txsx.CRDate = DateTime.Now;
                txsx.CRUser = UserInfo.User.UserName;
                txsx.CRUserRealName = UserInfo.User.UserRealName;
                txsx.ZXCount = 0;
                txsx.Remark = "Manual";
                new SZHL_TXSXB().Insert(txsx);
            }
            else
            {
                if (txsx.Status == "1")
                {
                    msg.ErrorMsg = "此提醒已结束不能修改";
                    return;
                }
                txsx.UpdateDate = DateTime.Now;
                txsx.UpdateUser = UserInfo.User.UserName;
                txsx.UpdateRealName = UserInfo.User.UserRealName;
                new SZHL_TXSXB().Update(txsx);
            }
            msg.Result = txsx;
        }
        #endregion

        #region 获取提醒事项列表
        /// <summary>
        /// 获取提醒事项列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXSXLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " Remark='Manual' and ComId=" + UserInfo.User.ComId;

            string type = context.Request["lb"] ?? "1";
            if (type == "1")
            {
                strWhere += string.Format(" And CRUser='{0}'", userName);
            }
            else if (type == "2")
            {
                strWhere += string.Format(" And ','+TXUser+',' like '%,{0},%'", userName);
            }

            if (P1 == "2")
            {
                strWhere += string.Format(" and Status='1' ");
            }
            else if (P1 == "1")
            {
                strWhere += string.Format(" and Status='0' ");
            }
            string strContent = context.Request["Content"] ?? "";
            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                //strWhere += string.Format(" And ( TXContent like '%{0}%' or TXUser  like '%{0}%' )", strContent);
                strWhere += string.Format(" And ( TXContent like '%{0}%')", strContent);
            }
            int DataID = -1;
            int.TryParse(context.Request["ID"] ?? "-1", out DataID);//记录Id
            if (DataID != -1)
            {
                strWhere += string.Format(" And ID = '{0}'", DataID);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;

            DataTable dt = new DataTable();

            if (P2 == "-1" || P2 == "")
            {
                strWhere += "  and type in ('0','1','2','3')";
            }
            else
            {
                strWhere += "  and type ='" + P2 + "'";
            }

            dt = new SZHL_TXSXB().GetDataPager("SZHL_TXSX", "*", pagecount, page, " CRDate desc", strWhere, ref total);
            dt.Columns.Add("TXMS", Type.GetType("System.Object"));
            foreach (DataRow dr in dt.Rows)
            {
                string html = "", html2 = "", html3 = "";
                switch (dr["TXType"].ToString())
                {
                    case "0": html2 = "立即发送"; break;
                    case "1": html2 = "仅一次," + dr["Date"] + " " + dr["Hour"] + ":" + dr["Minute"]; break;
                    case "2": html2 = "每个工作日的" + dr["Hour"] + ":" + dr["Minute"]; break;
                    case "3": html2 = "每天的" + dr["Hour"] + ":" + dr["Minute"]; break;
                    case "4": html2 = "每周" + dr["Days"] + "的" + dr["Hour"] + ":" + dr["Minute"]; break;
                    case "5": html2 = "每月" + dr["Days"] + "日的" + dr["Hour"] + ":" + dr["Minute"]; break;
                }
                if (!(dr["TXType"].ToString() == "0" || dr["TXType"].ToString() == "1"))
                {
                    switch (dr["CFType"].ToString())
                    {
                        case "1": html3 = ",无结束时间"; break;
                        case "2": html3 = "," + dr["CFCount"].ToString() + "次后结束,已经执行" + (string.IsNullOrWhiteSpace(dr["ZXCount"].ToString()) ? "0" : dr["ZXCount"].ToString()) + "次"; break;
                        case "3": html3 = ",结束时间：" + (string.IsNullOrWhiteSpace(dr["CFJZDate"].ToString()) ? "" : dr["CFJZDate"].ToString().Substring(0, 10)); break;
                    }
                }
                html = html2 + html3;
                dr["TXMS"] = html;
            }

            msg.Result = dt;
            msg.Result1 = total;

        }
        #endregion

        #region 查看提醒的详细
        /// <summary>
        /// 查看提醒的详细
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXSXMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FormID = Int32.Parse(P1);
            SZHL_TXSX Model = new SZHL_TXSXB().GetEntity(d => d.ID == FormID && d.ComId == UserInfo.QYinfo.ComId);
            msg.Result = Model;
        }
        #endregion

        #region 删除提醒事项
        /// <summary>
        /// 删除提醒事项
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELTXSX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FormID = Int32.Parse(P1);
            new SZHL_TXSXB().Delete(D => D.ID == FormID && D.ComId == UserInfo.QYinfo.ComId);
        }
        #endregion

        #region 查看当月的提醒
        /// <summary>
        /// 查看当月的提醒
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXSXDATA_BY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = " TXType='1' and Remark='Manual'  and comid=" + UserInfo.QYinfo.ComId + " and type in ('0','1','2','3') ";


            //string strQueryUser = context.Request["QUSER"] ?? "";
            //if (strQueryUser != "")
            //{
            //    if (strQueryUser.IndexOf(',') != -1)
            //    {
            //        strQueryUser = "0";
            //    }
            //    strWhere += string.Format(" and CRUser in('{0}') ", strQueryUser.ToFormatLike());
            //}
            //else
            //{
            //    strWhere += string.Format(" and CRUser='{0}'", UserInfo.User.UserName);
            //}
            strWhere += string.Format(" and ','+TXUser+','  like '%,{0},%' ", UserInfo.User.UserName);

            if (P1 != "")
            {
                strWhere += string.Format(" and Status='{0}' ", P1);
            }

            string start = context.Request["start"];
            string end = context.Request["end"];
            if (!string.IsNullOrEmpty(start))
            {
                strWhere += string.Format(" and [Date]>='{0}' ", start);
            }
            if (!string.IsNullOrEmpty(end))
            {
                strWhere += string.Format(" and [Date]<='{0}' ", end);
            }




            DataTable dtList = new SZHL_TXSXB().GetDTByCommand(" select ID,left([TXContent],10)+' '+[Hour]+':'+[Minute] as title,[Date]+' '+[Hour]+':'+[Minute]+':00' as start  FROM SZHL_TXSX where " + strWhere + " ORDER BY CRDate ");

            msg.Result = dtList;

        }

        /// <summary>
        /// 查看当月的提醒
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXSXDATA_MONTH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string start = context.Request["start"];
            string end = context.Request["end"];
            List<RLView> list = new List<RLView>();

            DateTime sd = DateTime.Parse(start + " 00:00:00");
            DateTime ed = DateTime.Parse(end + " 23:59:59");

            string strWhere = "Remark='Manual' and  comid=" + UserInfo.QYinfo.ComId + " and type in ('0','1','2','3') ";

            strWhere += string.Format(" and ','+TXUser+','  like '%,{0},%' ", UserInfo.User.UserName);

            var sxs = new SZHL_TXSXB().GetEntities(strWhere);
            foreach (var model in sxs)
            {
                DateTime cd = DateTime.Parse(model.CRDate.Value.ToString("yyyy-MM-dd") + " 00:00:00");
                switch (model.TXType)
                {
                    case "0":  //立即发送
                        {
                            #region 立即发送
                            DateTime dd = model.CRDate.Value;
                            if (dd >= sd && dd <= ed)
                            {
                                RLView rv = new RLView();
                                rv.id = model.ID;
                                if (!string.IsNullOrEmpty(model.TXContent))
                                {
                                    rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                    //rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                    rv.title = rv.title + " " + model.CRDate.Value.ToString("HH:mm");
                                }

                                //rv.start = model.Date + " " + model.Hour + ":" + model.Minute + ":00";
                                rv.start = model.CRDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                                rv.content = model.TXContent;
                                rv.txfs = txfs(model.Type);
                                rv.txms = txms(model);

                                list.Add(rv);
                            }
                            #endregion
                        }
                        break;
                    case "1":  //仅一次
                        {
                            #region 仅一次
                            DateTime dd = DateTime.Parse(model.Date + " " + model.Hour + ":" + model.Minute + ":00");

                            if (dd >= sd && dd <= ed)
                            {
                                RLView rv = new RLView();
                                rv.id = model.ID;
                                if (!string.IsNullOrEmpty(model.TXContent))
                                {
                                    rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                    rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                }

                                rv.start = model.Date + " " + model.Hour + ":" + model.Minute + ":00";

                                rv.content = model.TXContent;
                                rv.txfs = txfs(model.Type);
                                rv.txms = txms(model);

                                list.Add(rv);
                            }
                            #endregion
                        }
                        break;
                    case "2":  //每个工作日
                        {
                            #region 每个工作日（单一）
                            //DateTime jzdt = new DateTime();
                            //if (model.CFType == "2") { jzdt = WorkDate(cd, model.CFCount.Value-1); }

                            //for (DateTime dte = sd; dte <= ed; dte=dte.AddDays(1))
                            //{
                            //    if (dte >= cd)
                            //    {
                            //        bool bl = false;
                            //        if (model.CFType == "1")
                            //        {
                            //            bl = true;
                            //        }
                            //        else if (model.CFType == "2" && dte <= jzdt)
                            //        {
                            //            bl = true;
                            //        }
                            //        else if (model.CFType == "3" && dte <= model.CFJZDate.Value )
                            //        {
                            //            bl = true;
                            //        }
                            //        if (bl)
                            //        {
                            //            if (dte.DayOfWeek == DayOfWeek.Monday
                            //                || dte.DayOfWeek == DayOfWeek.Tuesday
                            //                || dte.DayOfWeek == DayOfWeek.Wednesday
                            //                || dte.DayOfWeek == DayOfWeek.Thursday
                            //                || dte.DayOfWeek == DayOfWeek.Friday
                            //                )
                            //            {
                            //                RLView rv = new RLView();
                            //                rv.id = model.ID;
                            //                if (!string.IsNullOrEmpty(model.TXContent))
                            //                {
                            //                    rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                            //                    rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                            //                }

                            //                rv.start = dte.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";

                            //                rv.content = model.TXContent;
                            //                rv.txfs = txfs(model.Type);
                            //                rv.txms = txms(model);

                            //                list.Add(rv);
                            //            }
                            //        }
                            //    }
                            //} 
                            #endregion

                            #region 每个工作日（连续）
                            DateTime sd1 = new DateTime();
                            DateTime ed1 = ed;
                            sd1 = sd > cd ? sd : cd;
                            if (model.CFType == "2")
                            {
                                DateTime jzdt = WorkDate(cd, model.CFCount.Value - 1);
                                ed1 = ed < jzdt ? ed : jzdt;
                            }
                            if (model.CFType == "3")
                            {
                                ed1 = ed < model.CFJZDate.Value ? ed : model.CFJZDate.Value;
                            }
                            while (sd1 <= ed1)
                            {
                                int n = -1;
                                switch (sd1.DayOfWeek)
                                {
                                    case DayOfWeek.Monday: n = 4; break;
                                    case DayOfWeek.Tuesday: n = 3; break;
                                    case DayOfWeek.Wednesday: n = 2; break;
                                    case DayOfWeek.Thursday: n = 1; break;
                                    case DayOfWeek.Friday: n = 0; break;
                                    case DayOfWeek.Saturday: sd1 = sd1.AddDays(2); break;
                                    case DayOfWeek.Sunday: sd1 = sd1.AddDays(1); break;
                                }
                                if (n != -1)
                                {
                                    DateTime ed2 = sd1.AddDays(n);
                                    ed2 = ed2 > ed1 ? ed1 : ed2;

                                    RLView rv = new RLView();
                                    rv.id = model.ID;
                                    if (!string.IsNullOrEmpty(model.TXContent))
                                    {
                                        rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                        rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                    }

                                    rv.start = sd1.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";
                                    rv.end = ed2.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";

                                    rv.content = model.TXContent;
                                    rv.txfs = txfs(model.Type);
                                    rv.txms = txms(model);

                                    list.Add(rv);

                                    sd1 = ed2.AddDays(3);
                                }
                            }
                            #endregion
                        }
                        break;
                    case "3":  //每天
                        {
                            #region 每天（单一）
                            //for (DateTime dte = sd; dte <= ed; dte = dte.AddDays(1))
                            //{
                            //    if (dte >= cd)
                            //    {
                            //        bool bl = false;
                            //        if (model.CFType == "1")
                            //        {
                            //            bl = true;
                            //        }
                            //        else if (model.CFType == "2" && dte <= cd.AddDays(model.CFCount.Value-1))
                            //        {
                            //            bl = true;
                            //        }
                            //        else if (model.CFType == "3" && dte <= model.CFJZDate.Value)
                            //        {
                            //            bl = true;
                            //        }
                            //        if (bl)
                            //        {
                            //            RLView rv = new RLView();
                            //            rv.id = model.ID;
                            //            if (!string.IsNullOrEmpty(model.TXContent))
                            //            {
                            //                rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                            //                rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                            //            }

                            //            rv.start = dte.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";

                            //            rv.content = model.TXContent;
                            //            rv.txfs = txfs(model.Type);
                            //            rv.txms = txms(model);

                            //            list.Add(rv);
                            //        }
                            //    }
                            //}
                            #endregion

                            #region 每天（连续）
                            DateTime sd1 = new DateTime();
                            DateTime ed1 = ed;

                            sd1 = sd > cd ? sd : cd;
                            if (model.CFType == "2")
                            {
                                ed1 = ed < cd.AddDays(model.CFCount.Value - 1) ? ed : cd.AddDays(model.CFCount.Value - 1);
                            }
                            if (model.CFType == "3")
                            {
                                ed1 = ed < model.CFJZDate.Value ? ed : model.CFJZDate.Value;
                            }
                            if (sd1 < ed1)
                            {
                                RLView rv = new RLView();
                                rv.id = model.ID;
                                if (!string.IsNullOrEmpty(model.TXContent))
                                {
                                    rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                    rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                }

                                rv.start = sd1.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";
                                rv.end = ed1.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";

                                rv.content = model.TXContent;
                                rv.txfs = txfs(model.Type);
                                rv.txms = txms(model);

                                list.Add(rv);
                            }

                            #endregion
                        }
                        break;
                    case "5":  //每月
                        {
                            #region 每月
                            DateTime jzdt = new DateTime();
                            if (model.CFType == "2") { jzdt = MonthDate(cd, "-" + model.Days + " " + model.Hour + ":" + model.Minute + ":00", model.CFCount.Value); }

                            for (DateTime dte = sd; dte <= ed; dte = dte.AddDays(1))
                            {
                                if (dte >= cd && dte.Day == Int32.Parse(model.Days))
                                {
                                    bool bl = false;
                                    if (model.CFType == "1")
                                    {
                                        bl = true;
                                    }
                                    else if (model.CFType == "2" && dte <= jzdt)
                                    {
                                        bl = true;
                                    }
                                    else if (model.CFType == "3" && dte <= model.CFJZDate.Value)
                                    {
                                        bl = true;
                                    }
                                    if (bl)
                                    {
                                        RLView rv = new RLView();
                                        rv.id = model.ID;
                                        if (!string.IsNullOrEmpty(model.TXContent))
                                        {
                                            rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                            rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                        }

                                        rv.start = dte.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";

                                        rv.content = model.TXContent;
                                        rv.txfs = txfs(model.Type);
                                        rv.txms = txms(model);

                                        list.Add(rv);
                                    }
                                }
                            }
                            #endregion
                        }
                        break;
                    case "4":  //自定义
                        {
                            #region 自定义
                            DateTime jzdt = new DateTime();
                            if (model.CFType == "2") { jzdt = CustomDate(cd, model.Days, model.CFCount.Value); }
                            for (DateTime dte = sd; dte <= ed; dte = dte.AddDays(1))
                            {
                                if (dte >= cd)
                                {
                                    bool bl = false;
                                    if (model.CFType == "1")
                                    {
                                        bl = true;
                                    }
                                    else if (model.CFType == "2" && dte <= jzdt)
                                    {
                                        bl = true;
                                    }
                                    else if (model.CFType == "3" && dte <= model.CFJZDate.Value)
                                    {
                                        bl = true;
                                    }
                                    if (bl)
                                    {
                                        string Days = model.Days;

                                        foreach (var d in Days.Split(','))
                                        {
                                            if (getWkDays(d) == dte.DayOfWeek)
                                            {
                                                RLView rv = new RLView();
                                                rv.id = model.ID;
                                                if (!string.IsNullOrEmpty(model.TXContent))
                                                {
                                                    rv.title = model.TXContent.Length > 10 ? model.TXContent.Substring(0, 10) : model.TXContent;
                                                    rv.title = rv.title + " " + model.Hour + ":" + model.Minute;
                                                }

                                                rv.start = dte.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00";
                                                rv.content = model.TXContent;
                                                rv.txfs = txfs(model.Type);
                                                rv.txms = txms(model);

                                                list.Add(rv);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        break;
                }
            }

            msg.Result = list;

        }

        #region 工作日后的时间
        /// <summary>
        /// 工作日后的时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public DateTime WorkDate(DateTime dt, int n)
        {
            DateTime temp = dt;
            while (n-- > 0)
            {
                temp = temp.AddDays(1);
                while (temp.DayOfWeek == System.DayOfWeek.Saturday || temp.DayOfWeek == System.DayOfWeek.Sunday)
                    temp = temp.AddDays(1);
            }
            return temp;
        }
        #endregion

        #region 自定义后的时间
        /// <summary>
        /// 自定义后的时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public DateTime CustomDate(DateTime dt, string strs, int n)
        {
            DateTime temp = dt;
            while (n-- > 0)
            {
                bool bl = true;
                foreach (var d in strs.Split(','))
                {
                    if (getWkDays(d) == temp.DayOfWeek)
                    {
                        bl = false;
                    }
                }
                if (bl) { n++; }
                if (n > 0)
                {
                    temp = temp.AddDays(1);
                }

            }
            return temp;
        }
        #endregion

        #region 月后的时间
        /// <summary>
        /// 月后的时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public DateTime MonthDate(DateTime dt, string strs, int n)
        {
            DateTime temp = dt;
            while (n-- > 0)
            {
                try
                {
                    temp = DateTime.Parse(temp.ToString("yyyy-MM") + strs);
                    if (temp < dt) { n++; }
                }
                catch
                {
                    n++;
                }
                if (n > 0)
                {
                    temp = temp.AddMonths(1);
                }
            }
            return temp;
        }
        #endregion

        #region 查询星期
        /// <summary>
        /// 查询星期
        /// </summary>
        /// <param name="wk"></param>
        /// <returns></returns>
        public DayOfWeek getWkDays(string wk)
        {
            DayOfWeek dw = new DayOfWeek();
            switch (wk)
            {
                case "周一":
                    dw = DayOfWeek.Monday;
                    break;
                case "周二":
                    dw = DayOfWeek.Tuesday;
                    break;
                case "周三":
                    dw = DayOfWeek.Wednesday;
                    break;
                case "周四":
                    dw = DayOfWeek.Thursday;
                    break;
                case "周五":
                    dw = DayOfWeek.Friday;
                    break;
                case "周六":
                    dw = DayOfWeek.Saturday;
                    break;
                case "周日":
                    dw = DayOfWeek.Sunday;
                    break;
            }

            return dw;
        }
        #endregion

        #region 日历对象
        /// <summary>
        /// 日历对象
        /// </summary>
        public class RLView
        {
            public int id { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string content { get; set; }
            public string txfs { get; set; }
            public string txms { get; set; }
        }
        #endregion

        #region 提醒方式
        /// <summary>
        /// 提醒方式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string txfs(string str)
        {
            string strs = string.Empty;
            switch (str)
            {
                case "0": strs = "短信和微信"; break;
                case "1": strs = "短信"; break;
                case "2": strs = "微信"; break;
            }

            return strs;
        }
        #endregion

        #region 提醒模式
        /// <summary>
        /// 提醒模式
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public string txms(SZHL_TXSX st)
        {
            string html1 = string.Empty;
            string html2 = string.Empty;
            string html3 = string.Empty;

            switch (st.TXType)
            {
                case "0": html2 = "立即发送"; break;
                case "1": html2 = "仅一次," + st.Date + " " + st.Hour + ":" + st.Minute; break;
                case "2": html2 = "每个工作日的" + st.Hour + ":" + st.Minute; break;
                case "3": html2 = "每天的" + st.Hour + ":" + st.Minute; break;
                case "4": html2 = "每周" + st.Days + "的" + st.Hour + ":" + st.Minute; break;
                case "5": html2 = "每月" + st.Days + "日的" + st.Hour + ":" + st.Minute; break;
            }
            if (!(st.TXType == "0" || st.TXType == "1"))
            {
                switch (st.CFType)
                {
                    case "1": html3 = ",无结束时间"; break;
                    case "2": html3 = "," + st.CFCount + "次后结束,已经执行" + (st.ZXCount != null ? st.ZXCount : 0) + "次"; break;
                    case "3": html3 = ",结束时间：" + (st.CFJZDate != null ? st.CFJZDate.ToString().Substring(0, 10) : ""); break;
                }
            }
            html1 = html2 + html3;
            return html1;
        }
        #endregion

        #endregion

        #region 首页获取日程提醒
        /// <summary>
        /// 首页获取日程提醒
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTXSX_INDEX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("select top 5 * from SZHL_TXSX where ','+TXUser+','  like '%," + UserInfo.User.UserName + ",%' and status=0 and comid=" + UserInfo.QYinfo.ComId);
            msg.Result = new SZHL_TXSXB().GetDTByCommand(strSql);
        }
        #endregion


    }


}