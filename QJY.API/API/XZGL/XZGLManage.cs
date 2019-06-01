using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using FastReflectionLib;
using System.Data;
using QJY.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using System.Collections;
using QJY.Common;
using QJY.BusinessData;
using Senparc.NeuChar.Entities;

namespace QJY.API
{
    public class XZGLManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(XZGLManage).GetMethod(msg.Action.ToUpper());
            XZGLManage model = new XZGLManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }


        #region excel转换为table new

        /// <summary>
        /// excel转换为table
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXCELTOTABLENEW(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string str2 = "";
                DataTable dt = new DataTable();
                ArrayList al1 = new ArrayList();
                ArrayList al2 = new ArrayList();
                HttpPostedFile _upfile = context.Request.Files["upFile"];
                string headrow = context.Request["headrow"] ?? "0";//头部开始行下标
                if (_upfile == null)
                {
                    msg.ErrorMsg = "请选择要上传的文件 ";
                }
                else
                {
                    string fileName = _upfile.FileName;/*获取文件名： C:\Documents and Settings\Administrator\桌面\123.jpg*/
                    string suffix = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();/*获取后缀名并转为小写： jpg*/
                    int bytes = _upfile.ContentLength;//获取文件的字节大小   
                    if (suffix == "xls" || suffix == "xlsx")
                    {
                        IWorkbook workbook = null;

                        Stream stream = _upfile.InputStream;

                        if (suffix == "xlsx") // 2007版本
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                        else if (suffix == "xls") // 2003版本
                        {
                            workbook = new HSSFWorkbook(stream);
                        }

                        //获取excel的第一个sheet
                        ISheet sheet = workbook.GetSheetAt(0);

                        //获取sheet的第一行
                        IRow headerRow = sheet.GetRow(int.Parse(headrow));

                        IRow header0Row = sheet.GetRow(1);



                        //一行最后一个方格的编号 即总的列数
                        int cellCount = headerRow.LastCellNum;
                        //最后一列的标号  即总的行数
                        int rowCount = sheet.LastRowNum;
                        if (rowCount <= int.Parse(headrow))
                        {
                            msg.ErrorMsg = "文件中无数据! ";
                        }
                        else
                        {
                            CommonHelp ch = new CommonHelp();
                            string[] yz = { "姓名", "部门", "用户编码" };
                            //列名
                            for (int i = 0; i < cellCount; i++)
                            {
                                string strlm = string.Empty;
                                if (headerRow.GetCell(i).IsMergedCell)
                                {
                                    strlm = sheet.GetRow(int.Parse(headrow) - 1).GetCell(i).ToString().Trim();
                                }
                                else
                                {
                                    strlm = headerRow.GetCell(i).ToString().Trim();
                                }
                                if (string.IsNullOrWhiteSpace(strlm)) strlm = "第" + (i + 1) + "列";
                                dt.Columns.Add(strlm);//添加列名
                            }

                            #region 必填字段在文件中存不存在验证
                            foreach (var v in yz)
                            {
                                if (!dt.Columns.Contains(v))
                                {
                                    if (string.IsNullOrEmpty(str2))
                                    {
                                        str2 = "当前导入的必填字段：【" + v + "】";
                                    }
                                    else
                                    {
                                        str2 = str2 + "、【" + v + "】";
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(str2))
                            {
                                str2 = str2 + " 在文件中不存在!<br>";
                            }
                            #endregion

                            for (int i = (sheet.FirstRowNum + int.Parse(headrow) + 1); i <= sheet.LastRowNum; i++)
                            {
                                DataRow dr = dt.NewRow();
                                bool bl = false;
                                IRow row = sheet.GetRow(i);
                                for (int j = row.FirstCellNum; j < cellCount; j++)
                                {
                                    string strsj = exportsheet(row.GetCell(j)).Trim();
                                    if (strsj != "")
                                    {
                                        bl = true;
                                    }
                                    dr[j] = strsj;
                                }
                                if (bl)
                                {
                                    dt.Rows.Add(dr);
                                }
                            }
                            int ykindex = 0;
                            for (int n = 0; n < dt.Columns.Count; n++)
                            {
                                if (header0Row.GetCell(n).ToString().Contains("应扣"))
                                {
                                    ykindex = n;
                                    break;
                                }
                            }

                            for (int n = 0; n < dt.Columns.Count; n++)
                            {
                                var name = dt.Columns[n].ColumnName;
                                if (n > 2 && n < ykindex - 1)
                                {
                                    al1.Add(name);
                                }
                                //得跳过应发合计列
                                if (n > ykindex - 1 && n < dt.Columns.Count - 2)
                                {
                                    al2.Add(name);
                                }
                                //需要跳过应扣合计和实发合计
                            }

                            dt.Columns.Add("YF", Type.GetType("System.Object"));
                            dt.Columns.Add("YK", Type.GetType("System.Object"));

                            foreach (DataRow dr in dt.Rows)
                            {
                                JObject obj1 = new JObject();
                                JObject obj2 = new JObject();
                                foreach (var str in al1)
                                {
                                    obj1.Add(str.ToString(), dr[str.ToString()].ToString());
                                }
                                foreach (var str in al2)
                                {
                                    obj2.Add(str.ToString(), dr[str.ToString()].ToString());
                                }
                                dr["YF"] = obj1;
                                dr["YK"] = obj2;
                            }
                            foreach (var str in al1)
                            {
                                dt.Columns.Remove(str.ToString());
                            }
                            foreach (var str in al2)
                            {
                                dt.Columns.Remove(str.ToString());
                            }

                            msg.Result = dt;

                            string sql = "select top 1 * from szhl_xz_jl order by crdate desc";
                            msg.Result1 = new SZHL_XZ_JLB().GetDTByCommand(sql);

                            msg.ErrorMsg = str2;
                        }

                        sheet = null;
                        workbook = null;
                    }
                    else
                    {
                        msg.ErrorMsg = "请上传excel文件 ";
                    }
                }
            }
            catch (Exception)
            {
                msg.ErrorMsg = "导入失败！";
            }
        }

        private string exportsheet(ICell rowCell)
        {
            if (rowCell == null) return "";

            object shstring = "";
            switch (rowCell.CellType)
            {
                case CellType.Boolean:
                    shstring = Convert.ToString(rowCell.BooleanCellValue);
                    break;
                case CellType.Error:
                    shstring = ErrorEval.GetText(rowCell.ErrorCellValue);
                    break;
                case CellType.Formula:
                    switch (rowCell.CachedFormulaResultType)
                    {
                        case CellType.Boolean:
                            shstring = Convert.ToString(rowCell.BooleanCellValue);
                            break;
                        case CellType.Error:
                            shstring = ErrorEval.GetText(rowCell.ErrorCellValue);
                            break;
                        case CellType.Numeric:
                            shstring = Convert.ToString(rowCell.NumericCellValue);
                            break;
                        case CellType.String:
                            string strFORMULA = rowCell.StringCellValue;
                            if (strFORMULA != null && strFORMULA.Length > 0)
                            {
                                shstring = strFORMULA.ToString();
                            }
                            else
                            {
                                shstring = "";
                            }
                            break;
                        default:
                            shstring = "";
                            break;
                    }
                    break;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(rowCell))
                    {
                        shstring = DateTime.FromOADate(rowCell.NumericCellValue);
                    }
                    else
                    {
                        shstring = Convert.ToDouble(rowCell.NumericCellValue);
                    }
                    break;
                case CellType.String:
                    string str = rowCell.StringCellValue;
                    if (!string.IsNullOrEmpty(str))
                    {
                        shstring = Convert.ToString(str);
                    }
                    else
                    {
                        shstring = null;
                    }
                    break;
                default:
                    shstring = "";
                    break;
            }
            shstring = shstring == null ? "" : shstring;
            return shstring.ToString();
        }
        #endregion

        #region 薪资管理
        /// <summary>
        /// 发放工资条
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void FFXZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (string.IsNullOrWhiteSpace(P1))
            {
                msg.ErrorMsg = "发送失败";
                return;
            }

            string taitou = context.Request["taitou"] ?? "";
            string luokuan = context.Request["luokuan"] ?? "";
            string ffdx = context.Request["ffdx"] ?? "";
            string ffwx = context.Request["ffwx"] ?? "";
            string ym = context.Request["ym"] ?? "";

            SZHL_XZ_JL xzjj = new SZHL_XZ_JL();
            xzjj.YearMonth = ym;
            xzjj.title = P2;
            xzjj.rise = taitou;
            xzjj.Inscribe = luokuan;
            xzjj.ComId = UserInfo.User.ComId;
            xzjj.CRDate = DateTime.Now;
            xzjj.CRUser = UserInfo.User.UserName;
            xzjj.salaryData = P1;
            new SZHL_XZ_JLB().Insert(xzjj);

            //string shibaiuser = "";

            #region 工资单
            List<JObject> xxb = JsonConvert.DeserializeObject<List<JObject>>(P1);
            foreach (var a in xxb)
            {
                bool bl = false;
                string username = a["姓名"] != null ? a["姓名"].ToString().Trim() : "";
                string bmname = a["部门"] != null ? a["部门"].ToString().Trim() : "";
                string tel = a["用户编码"] != null ? a["用户编码"].ToString().Trim() : "";




                SZHL_XZ_GZD gzd = new SZHL_XZ_GZD();
                gzd.ComId = UserInfo.User.ComId;
                gzd.CRUser = UserInfo.User.UserName;
                gzd.CRDate = DateTime.Now;
                gzd.YearMonth = ym;
                gzd.Telephone = tel;
                gzd.BranchCode = xzjj.ID;//没地方存，只能存这个字段了
                gzd.salaryData = a.ToString();
                gzd.title = P2;
                gzd.rise = taitou;
                gzd.Inscribe = luokuan;
                gzd.IsRead = 0;
                gzd.UserRealName = username;
                gzd.BranchName = bmname;

                //List<JH_Auth_User> users = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && (d.mobphone == tel || d.UserName == tel || d.JobNum == tel || d.UserRealName == username)).ToList();
                List<JH_Auth_User> users = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.UserRealName == username).ToList();

                if (users.Count > 0)
                {
                    gzd.UserName = users[0].UserName;
                    bl = true;
                    new SZHL_XZ_GZDB().Delete(d => d.UserName == gzd.UserName && d.YearMonth == ym);//先删除该年月的发薪记录
                }


                new SZHL_XZ_GZDB().Insert(gzd);

                if (ffwx == "1" && tel != "" && bl)
                {
                    SZHL_TXSX CSTX = new SZHL_TXSX();
                    CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CSTX.APIName = "XZGL";
                    CSTX.ComId = UserInfo.User.ComId;
                    CSTX.FunName = "SENDXZMSG";
                    CSTX.CRUserRealName = UserInfo.User.UserRealName;

                    CSTX.MsgID = gzd.ID.ToString();
                    CSTX.TXContent = taitou;
                    CSTX.ISCS = "N";
                    CSTX.TXUser = users[0].UserName;
                    CSTX.TXMode = "XZFF";
                    CSTX.CRUser = UserInfo.User.UserName;
                    TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间
                }
                if (ffdx == "1" && tel != "")
                {
                    string hj = a["合计"] != null ? a["合计"].ToString().Trim() : "";
                    new SZHL_DXGLB().SendSMS(tel, username + "，" + taitou + "，" + "合计：" + hj + "元.点击" + UserInfo.QYinfo.WXUrl.TrimEnd('/') + "/View_Mobile/UI/UI_GZD_VIEW.html?ID=" + gzd.ID + " 查看详情", UserInfo.QYinfo.ComId);
                }
            }
            #endregion

            //msg.Result = shibaiuser.TrimEnd(',');
        }


        /// <summary>
        /// 删除工资单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELFFJL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 != "")//PIID
            {
                new SZHL_XZ_JLB().Delete(d => d.ID.ToString() == P1);
                new SZHL_XZ_GZDB().Delete(d => d.BranchCode.ToString() == P1);
            }
        }



        /// <summary>
        /// 按照年月统计
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void TJGZD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int strSYear = int.Parse(P1.Trim().Substring(0, 4));
            int strEYear = int.Parse(P2.Trim().Substring(0, 4));
            int strSMONTH = int.Parse(P1.Trim().Substring(5));
            int strEMONTH = int.Parse(P2.Trim().Substring(5));
            if (strEYear > strSYear)
            {
                msg.ErrorMsg = "开始年份不能大于结束年份";
            }

            List<string> ListNY = new List<string>();
            DateTime dtS = Convert.ToDateTime(strSYear.ToString() + "-" + strSMONTH.ToString() + "-01");
            DateTime dtE = Convert.ToDateTime(strEYear.ToString() + "-" + strEMONTH.ToString() + "-01");
            int diff = (dtE.Month - dtS.Month) + 1;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    ListNY.Add(dtS.AddMonths(i).ToString("Y"));
                }
            }

            msg.Result = new SZHL_XZ_GZDB().GetEntities(D => D.ComId == UserInfo.User.ComId && ListNY.Contains(D.YearMonth) && D.UserName == UserInfo.User.UserName);
        }





        /// <summary>
        /// 发送微信信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SENDXZMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX TX = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            Article ar0 = new Article();
            ar0.Title = TX.TXContent;
            ar0.Description = "";
            ar0.Url = TX.MsgID;

            List<Article> al = new List<Article>();
            al.Add(ar0);
            if (!string.IsNullOrEmpty(TX.TXUser))
            {
                try
                {
                    //发送PC消息
                    UserInfo = new JH_Auth_UserB().GetUserInfo(TX.ComId.Value, TX.CRUser);
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(al, TX.TXMode, "A", TX.TXUser);
                    new JH_Auth_User_CenterB().SendMsg(UserInfo, TX.TXMode, TX.TXContent, TX.MsgID, TX.TXUser);
                }
                catch (Exception)
                {
                }
                //发送微信消息

            }
        }
        /// <summary>
        /// 个人工资条列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETGZDLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" ComId={0} and UserName='{1}' ", UserInfo.User.ComId, UserInfo.User.UserName);

            int DataID = -1;
            int.TryParse(context.Request["ID"] ?? "-1", out DataID);//记录Id
            if (DataID != -1)
            {
                string strIsHasDataQX = new JH_Auth_QY_ModelB().ISHASDATAREADQX("XZFF", DataID, UserInfo);
                if (strIsHasDataQX == "Y")
                {
                    strWhere += string.Format(" And ID = '{0}'", DataID);
                }

            }

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);//页码
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int recordCount = 0;

            string strContent = context.Request["Content"] ?? "";
            if (strContent != "")
            {
                strWhere += string.Format(" And ( title like '%{0}%' )", strContent);
            }

            DataTable dt = new SZHL_XZ_GZDB().GetDataPager(" SZHL_XZ_GZD ", " * ", pagecount, page, " CRDate desc ", strWhere, ref recordCount);

            msg.Result = dt;
            msg.Result1 = recordCount;

        }
        /// <summary>
        /// 工资单信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETGZDMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            SZHL_XZ_GZD xzjl = new SZHL_XZ_GZDB().GetEntity(d => d.ID == id);
            msg.Result = xzjl;
        }
        /// <summary>
        /// 未读工资单数量
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void NOREADGZD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var xzjl = new SZHL_XZ_GZDB().GetEntities(d => d.UserName == UserInfo.User.UserName && d.IsRead != 1);
            msg.Result = xzjl.ToList().Count;
        }
        /// <summary>
        /// 已读工资单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void READGZD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            SZHL_XZ_GZD xzjl = new SZHL_XZ_GZDB().GetEntity(d => d.ID == id);
            if (xzjl != null)
            {
                xzjl.IsRead = 1;
            }
            new SZHL_XZ_GZDB().Update(xzjl);
        }
        /// <summary>
        /// 复制上月工资
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSYFFJL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new SZHL_XZ_GZDB().GetDTByCommand("select top 1 * from SZHL_XZ_JL where ComId='" + UserInfo.QYinfo.ComId + "' order by CRDate desc");

            msg.Result = dt;
        }
        /// <summary>
        /// 最近上传的4条工资单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETGZTLAST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new SZHL_XZ_GZDB().GetDTByCommand("select top 4 * from SZHL_XZ_JL where ComId='" + UserInfo.QYinfo.ComId + "' and CRUser='" + UserInfo.User.UserName + "' order by CRDate desc");

            msg.Result = dt;
        }
        /// <summary>
        /// 直接添加（用户列表）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new SZHL_XZ_GZDB().GetDTByCommand("select u.UserRealName '姓名',b.DeptName '部门',u.mobphone '用户编码' from dbo.JH_Auth_User u left join dbo.JH_Auth_Branch b on u.BranchCode=b.DeptCode  where u.ComId='" + UserInfo.QYinfo.ComId + "' and b.DeptRoot!=-1 order by b.DeptName");

            msg.Result = dt;
        }
        /// <summary>
        /// 发放记录列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFFJLLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);//页码
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int recordCount = 0;
            string strWhere = string.Format(" ComId={0} ", UserInfo.User.ComId, UserInfo.User.UserName);
            string strContent = context.Request["Content"] ?? "";
            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                strWhere += string.Format(" And ( title like '%{0}%' )", strContent);
            }

            DataTable dt = new SZHL_XZ_GZDB().GetDataPager(" SZHL_XZ_JL ", " * ", pagecount, page, " CRDate desc ", strWhere, ref recordCount);

            msg.Result = dt;
            msg.Result1 = recordCount;
        }
        /// <summary>
        /// 获取发放记录信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFFJLMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            SZHL_XZ_JL xzjl = new SZHL_XZ_JLB().GetEntity(d => d.ID == id);
            msg.Result = xzjl;
        }
        #endregion

        #region 薪资基本设置

        #region 基本设置列表
        /// <summary>
        /// 根据部门编号获取部门人员(基本设置列表)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETUSERJBSZLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            int.TryParse(P1, out deptCode);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, deptCode);
            if (branch == null) { msg.ErrorMsg = "数据异常"; }
            string strQXWhere = string.Format("And  ( u.branchCode={0} or b.Remark1 like '{1}%')", deptCode, (branch.Remark1 == "" ? "" : branch.Remark1 + "-") + branch.DeptCode);
            string branchqx = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            if (branch.DeptRoot == -1 && !string.IsNullOrEmpty(branchqx))
            {
                strQXWhere = " And (";
                int i = 0;
                foreach (int dept in branchqx.SplitTOInt(','))
                {
                    JH_Auth_Branch branchQX = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, dept);
                    strQXWhere += string.Format((i == 0 ? "" : "And") + "  ( u.branchCode!={0} And b.Remark1 NOT like '{1}%')", dept, (branchQX.Remark1 == "" ? "" : branchQX.Remark1 + "-") + branchQX.DeptCode);
                    i++;
                }
                strQXWhere += ")";
            }
            string tableName = " JH_Auth_User u  inner join JH_Auth_Branch b on u.branchCode=b.DeptCode left join SZHL_GZGL_JCSZ z on u.UserName=z.UserName and u.ComId=z.ComId ";
            string tableColumn = " z.*,u.ID as uid,u.UserName as un,u.UserRealName,u.mobphone,b.DeptName,b.DeptCode";
            string strWhere = string.Format("u.ComId={0}   {1}", UserInfo.User.ComId, strQXWhere);
            if (P2 != "")
            {
                strWhere += string.Format(" And (u.UserName like '%{0}%'  or u.UserRealName like '%{0}%'  or b.DeptName like '%{0}%' or u.mobphone like '%{0}%' ) ", P2);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "0", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;

            int total = 0;
            DataTable dt = new JH_Auth_UserB().GetDataPager(tableName, tableColumn, pagecount, page, " b.DeptShort,ISNULL(u.UserOrder, 1000000) asc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }
        #endregion

        #region 添加基础设置
        /// <summary>
        /// 添加基础设置
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void ADDJCSZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_GZGL_JCSZ GZBG = JsonConvert.DeserializeObject<SZHL_GZGL_JCSZ>(P1);
            if (GZBG == null)
            {
                msg.ErrorMsg = "添加失败";
                return;
            }
            if (string.IsNullOrWhiteSpace(GZBG.UserName))
            {
                msg.ErrorMsg = "用户名不能为空";
                return;
            }

            //if (P2 != "") // 处理微信上传的图片
            //{

            //    string fids = CommonHelp.ProcessWxIMG(P2, "GZBG", UserInfo);
            //    if (!string.IsNullOrEmpty(GZBG.Files))
            //    {
            //        GZBG.Files += "," + fids;
            //    }
            //    else
            //    {
            //        GZBG.Files = fids;
            //    }
            //}


            if (GZBG.ID == 0)
            {
                GZBG.CRDate = DateTime.Now;
                GZBG.CRUser = UserInfo.User.UserName;
                GZBG.ComId = UserInfo.User.ComId;
                new SZHL_GZGL_JCSZB().Insert(GZBG);
            }
            else
            {
                new SZHL_GZGL_JCSZB().Update(GZBG);
            }


            msg.Result = GZBG;
        }
        #endregion

        #region 获取基础设置
        /// <summary>
        /// 获取基础设置
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETJCSZMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);
            SZHL_GZGL_JCSZ sg = new SZHL_GZGL_JCSZB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = sg;
        }
        #endregion

        #endregion

        #region 福利

        #region 福利列表
        /// <summary>
        /// 福利列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFLLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " 1=1 and ComId=" + UserInfo.User.ComId;

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new DataTable();

            dt = new SZHL_GZGL_FLB().GetDataPager("SZHL_GZGL_FL", " * ", pagecount, page, " CRDate ", strWhere, ref total);

            if (dt.Rows.Count > 0)
            {
                dt.Columns.Add("FileList", Type.GetType("System.Object"));
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["Files"] != null && dr["Files"].ToString() != "")
                    {
                        dr["FileList"] = new FT_FileB().GetEntities(" ID in (" + dr["Files"].ToString() + ")");
                    }
                }
            }
            msg.Result = dt;
            msg.Result1 = total;

        }
        #endregion

        #region 获取福利信息
        /// <summary>
        /// 获取福利信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFLMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_GZGL_FL ccxj = new SZHL_GZGL_FLB().GetEntity(d => d.ID == Id);
            msg.Result = ccxj;
            if (ccxj != null)
            {
                if (!string.IsNullOrEmpty(ccxj.Files))
                {
                    msg.Result2 = new FT_FileB().GetEntities(" ID in (" + ccxj.Files + ")");
                }

                //new JH_Auth_User_CenterB().ReadMsg(UserInfo, ccxj.ID, "CCXJ");
            }

        }
        #endregion

        #region 添加福利
        /// <summary>
        /// 添加福利
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">客户信息</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDFL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            new SZHL_GZGL_FLB().Delete(d => d.ComId == UserInfo.User.ComId);

            if (!string.IsNullOrEmpty(P1))
            {
                List<SZHL_GZGL_FL> tdList = JsonConvert.DeserializeObject<List<SZHL_GZGL_FL>>(P1);
                tdList.ForEach(d => d.ComId = UserInfo.User.ComId);
                tdList.ForEach(d => d.CRDate = DateTime.Now);
                tdList.ForEach(d => d.CRUser = UserInfo.User.UserName);
                new SZHL_GZGL_FLB().Insert(tdList);
            }
        }
        #endregion 

        #endregion

        #region 五险一金

        #region 五险一金列表
        /// <summary>
        /// 五险一金列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXYJLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " 1=1 and ComId=" + UserInfo.User.ComId;

            DataTable dt = new DataTable();

            dt = new SZHL_GZGL_WXYJB().GetDTByCommand("select * from SZHL_GZGL_WXYJ where comid ='" + UserInfo.User.ComId + "'");
            if (dt.Rows.Count != 6)
            {
                dt = new SZHL_GZGL_WXYJB().GetDTByCommand(@"select * from ( select '养老保险' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "' union "
                + " select * from ( select '医疗保险' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "' union "
                + " select * from ( select '失业保险' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "' union "
                + " select * from ( select '工伤保险' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "' union "
                + " select * from ( select '生育保险' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "' union "
                + " select * from ( select '公积金' as name1 ) as a left join SZHL_GZGL_WXYJ as b on a.name1=b.Name and b.comid ='" + UserInfo.User.ComId + "'");
                foreach (DataRow dr in dt.Rows)
                {
                    dr["Name"] = dr["name1"];
                    if (dr["ID"] == null || dr["ID"].ToString() == "")
                    {
                        dr["ID"] = "0";
                    }
                }

                dt.Columns.Remove("name1");
            }

            msg.Result = dt;

        }
        #endregion

        #region 获取五险一金信息
        /// <summary>
        /// 获取五险一金信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXYJMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_GZGL_WXYJ ccxj = new SZHL_GZGL_WXYJB().GetEntity(d => d.ID == Id);
            msg.Result = ccxj;
            if (ccxj != null)
            {
                if (!string.IsNullOrEmpty(ccxj.Files))
                {
                    msg.Result2 = new FT_FileB().GetEntities(" ID in (" + ccxj.Files + ")");
                }

                //new JH_Auth_User_CenterB().ReadMsg(UserInfo, ccxj.ID, "CCXJ");
            }

        }
        #endregion

        #region 添加五险一金
        /// <summary>
        /// 添加五险一金
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">客户信息</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDWXYJ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (!string.IsNullOrEmpty(P1))
            {
                List<SZHL_GZGL_WXYJ> tdList = JsonConvert.DeserializeObject<List<SZHL_GZGL_WXYJ>>(P1);

                foreach (var l in tdList)
                {
                    var wxyj = new SZHL_GZGL_WXYJB().GetEntities(p => p.ComId == UserInfo.User.ComId && p.Name == l.Name).FirstOrDefault();
                    if (wxyj == null)
                    {
                        l.ComId = UserInfo.User.ComId;
                        l.CRDate = DateTime.Now;
                        l.CRUser = UserInfo.User.UserName;
                        new SZHL_GZGL_WXYJB().Insert(l);
                    }
                    else
                    {
                        wxyj.Base = l.Base;
                        wxyj.ComBL = l.ComBL;
                        wxyj.PerBL = l.PerBL;
                        new SZHL_GZGL_WXYJB().Update(wxyj);
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}