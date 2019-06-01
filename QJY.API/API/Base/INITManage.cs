using FastReflectionLib;
using Newtonsoft.Json;
using QJY.Common;
using QJY.Data;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace QJY.API
{
    public class INITManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(INITManage).GetMethod(msg.Action.ToUpper());
            INITManage model = new INITManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        #region 设置常用菜单显示
        //设置手机APP，PC首页菜单显示应用
        public void SETAPPINDEX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string type = context.Request["type"] ?? "APPINDEX";//默认为APP首页显示菜单，传值为PC首页的快捷方式按钮
            foreach (string str in P1.Split(','))
            {
                string[] content = str.Split(':');
                string modelCode = content[0];
                //判断是否存在菜单的数据，存在只更新状态，不存在添加
                JH_Auth_UserCustomData customData = new JH_Auth_UserCustomDataB().GetEntity(d => d.UserName == UserInfo.User.UserName && d.DataType == type && d.ComId == UserInfo.User.ComId && d.DataContent == modelCode);
                string status = content[1];
                if (customData != null)
                {
                    customData.DataContent1 = status;
                    new JH_Auth_UserCustomDataB().Update(customData);
                }
                else
                {
                    customData = new JH_Auth_UserCustomData();
                    customData.ComId = UserInfo.User.ComId;
                    customData.UserName = UserInfo.User.UserName;
                    customData.CRDate = DateTime.Now;
                    customData.CRUser = UserInfo.User.UserName;
                    customData.DataContent = modelCode;
                    customData.DataContent1 = status;
                    customData.DataType = type;
                    new JH_Auth_UserCustomDataB().Insert(customData);
                }
                if (type == "APPINDEX")
                {
                    msg.Result = customData;
                }
            }


        }
        #endregion
        #region 常用菜单设置
        /// <summary>
        /// 第五版的自定义显示菜单和左边菜单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETINDEXMENUNEW(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtModel = new JH_Auth_ModelB().GETMenuList(UserInfo, P1);
            dtModel.Columns.Add("ISSY", Type.GetType("System.Int32"));
            dtModel.Columns.Add("FunData", typeof(DataTable));
            if (dtModel != null && dtModel.Rows.Count > 0)
            {  //获取用户设置首页显示APP
                List<string> userCustom = new JH_Auth_UserCustomDataB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.UserName == UserInfo.User.UserName && d.DataType == P1 && d.DataContent1 == "Y").Select(d => d.DataContent).ToList();

                foreach (DataRow row in dtModel.Rows)
                {
                    if (userCustom.Count > 0)
                    {
                        row["ISSY"] = 0;
                        if (row["UserAPPID"].ToString() != "")
                        {
                            row["ISSY"] = 1;
                        }
                    }
                    else
                    {

                        row["ISSY"] = 1;
                    }
                    row["FunData"] = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, UserInfo.UserRoleCode, row["ID"].ToString());

                }
            }
            msg.Result = dtModel;
            msg.Result2 = UserInfo.User.isSupAdmin;

        }
        #endregion

        


        #region 应用管理
        public void GETYYDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "";
            if (P1 != "")
            {
                strWhere = " AND (ModelName like '%" + P1 + "%'OR ModelType like '%" + P1 + "%') ";
            }
            string strSQL = "SELECT * FROM JH_Auth_Model WHERE 1=1" + strWhere + " ORDER BY ModelType";
            DataTable dt = new JH_Auth_ModelB().GetDTByCommand(strSQL);
            msg.Result = dt;
        }


        public void DELYY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int intYYID = int.Parse(P1);
            new JH_Auth_ModelB().Delete(d => d.ID == intYYID);
            new JH_Auth_FunctionB().Delete(d => d.ModelID == intYYID);
        }

        public void GETYY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int intYYID = int.Parse(P1);
            JH_Auth_Model model = new JH_Auth_ModelB().GetEntity(d => d.ID == intYYID);
            msg.Result = model;
            msg.Result1 = new JH_Auth_FunctionB().GetEntities(d => d.ModelID == intYYID);
            msg.Result2 = new JH_Auth_CommonB().GetEntities(d => d.ModelCode == model.ModelCode);//移动应用菜单

        }




        public void ADDYY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model MODELO = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);

            JH_Auth_Model MODEL = new JH_Auth_Model();
            MODEL.ModelCode = MODELO.ModelCode;
            MODEL.PModelCode = MODELO.PModelCode;
            MODEL.ModelType = MODELO.ModelType;
            MODEL.ModelName = MODELO.ModelName;
            MODEL.ORDERID = MODELO.ORDERID;
            MODEL.WXUrl = MODELO.WXUrl;
            MODEL.ModelStatus = "0";
            MODEL.IsSQ = "1";
            MODEL.CRDate = DateTime.Now;
            MODEL.CRUser = UserInfo.User.UserName;
            MODEL.ComId = 0;
            MODEL.AppType = "1";
            MODEL.IsSys = 1;
            MODEL.IsKJFS = 0;
            new JH_Auth_ModelB().Insert(MODEL);

        }
        public void SAVEYY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model MODEL = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);
            new JH_Auth_ModelB().Update(MODEL);


            List<JH_Auth_Function> FUNLIST = JsonConvert.DeserializeObject<List<JH_Auth_Function>>(P2);
            FUNLIST.ForEach(d => d.CRDate = DateTime.Now);
            FUNLIST.ForEach(d => d.CRUser = UserInfo.User.UserName);
            FUNLIST.ForEach(d => d.ComId = 0);
            FUNLIST.ForEach(d => d.ModelID = MODEL.ID);
            FUNLIST.ForEach(d => d.ActionData = "[]");
            foreach (JH_Auth_Function item in FUNLIST)
            {
                if (item.ID == 0)
                {
                    //新增
                    new JH_Auth_FunctionB().Insert(item);
                }
                else
                {
                    new JH_Auth_FunctionB().Update(item);
                    //更新
                }
            }

            string delid = context.Request["DIDS"] ?? "";
            if (delid.Trim(',') != "")
            {
                new JH_Auth_FunctionB().ExsSclarSql("delete JH_Auth_Function where id in ('" + delid.ToFormatLike(',') + "')");
                //删除
            }
            string MENUData = context.Request["MENU"] ?? "";
            if (MENUData.Trim(',') != "")
            {
                new JH_Auth_CommonB().Delete(d => d.ModelCode == MODEL.ModelCode);
                List<JH_Auth_Common> MENUS = JsonConvert.DeserializeObject<List<JH_Auth_Common>>(MENUData);
                MENUS.ForEach(d => d.CRDate = DateTime.Now);
                MENUS.ForEach(d => d.CRUser = UserInfo.User.UserName);
                MENUS.ForEach(d => d.ModelCode = MODEL.ModelCode);
                new JH_Auth_CommonB().Insert(MENUS);

            }


        }

        #endregion









        #region EXCELTODatatable
        public void IMPORTUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            HttpPostedFile _upfile = context.Request.Files["upFile"];

            string headrow = context.Request["headrow"] ?? "0";//头部开始行下标
            if (_upfile == null)
            {
                msg.ErrorMsg = "请选择要上传的文件 ";
            }
            try
            {
                msg.Result = new CommonHelp().ExcelToTable(_upfile, int.Parse(headrow));
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        #endregion
        #region 导入用户
        public void SAVEIMPORTUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string branchMsg = "", branchErrorMsg = "", userMsg = "";
            int i = 0, j = 0;
            DataTable dt = new DataTable();
            dt = JsonConvert.DeserializeObject<DataTable>(P1);
            dt.Columns.Add("BranchCode");
            JH_Auth_Branch branchroot = new JH_Auth_BranchB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.DeptRoot == -1);


            foreach (DataRow row in dt.Rows)
            {
                int bRootid = branchroot.DeptCode;
                string branchName = row[4].ToString();
                if (branchName != "")
                {
                    string[] branchNames = branchName.Split('/');
                    string strBranch = branchNames[0];
                    JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strBranch && d.ComId == UserInfo.User.ComId);
                    if (branchModel == null)
                    {
                        branchModel = new JH_Auth_Branch();
                        branchModel.DeptName = branchNames[0];
                        branchModel.DeptDesc = branchNames[0];
                        branchModel.ComId = UserInfo.User.ComId;
                        branchModel.DeptRoot = bRootid;
                        branchModel.CRDate = DateTime.Now;
                        branchModel.CRUser = UserInfo.User.UserName;
                        new JH_Auth_BranchB().Insert(branchModel);
                        branchModel.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branchModel.DeptRoot) + branchModel.DeptCode;
                        new JH_Auth_BranchB().Update(branchModel);
                    }
                }
            }


            int rowIndex = 0;
            foreach (DataRow row in dt.Rows)
            {
                rowIndex++;
                string branchName = row[4].ToString();
                if (branchName != "")
                {
                    string[] branchNames = branchName.Split('/');
                    string strPBranch = branchNames[0];

                    JH_Auth_Branch PbranchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strPBranch && d.ComId == UserInfo.User.ComId);
                    int bRootid = PbranchModel.DeptCode;
                    for (int l = 1; l < branchNames.Length; l++)
                    {
                        string strBranch = branchNames[1];
                        JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptName == strBranch && d.DeptRoot == PbranchModel.DeptCode && d.ComId == UserInfo.User.ComId);
                        if (branchModel != null)
                        {
                            bRootid = branchModel.DeptCode;
                            if (l == branchNames.Length - 1)
                            {
                                row["BranchCode"] = branchModel.DeptCode;
                            }
                        }
                        else
                        {
                            branchModel = new JH_Auth_Branch();
                            branchModel.DeptName = strBranch;
                            branchModel.DeptDesc = strBranch;
                            branchModel.ComId = UserInfo.User.ComId;
                            branchModel.DeptRoot = bRootid;
                            branchModel.CRDate = DateTime.Now;
                            branchModel.CRUser = UserInfo.User.UserName;
                            new JH_Auth_BranchB().Insert(branchModel);
                            branchModel.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branchModel.DeptRoot) + branchModel.DeptCode;
                            new JH_Auth_BranchB().Update(branchModel);
                            try
                            {
                                bRootid = branchModel.DeptCode;
                                if (l == branchNames.Length - 1)
                                {
                                    row["BranchCode"] = branchModel.DeptCode;
                                }
                                i++;
                                branchMsg += "新增部门“" + strBranch + "”成功<br/>";
                            }
                            catch (Exception ex)
                            {

                                branchErrorMsg += "部门：" + strBranch + "失败 " + msg.ErrorMsg + "<br/>";
                            }
                        }
                    }
                    string userName = row[2].ToString();
                    JH_Auth_User userModel = new JH_Auth_UserB().GetEntity(d => d.UserName == userName && d.ComId == UserInfo.User.ComId);
                    if (userModel == null)
                    {
                        JH_Auth_User userNew = new JH_Auth_User();
                        if (row["BranchCode"].ToString() != "")
                        {
                            int tempcode = int.Parse(row["BranchCode"].ToString());
                            JH_Auth_Branch branchTemp = new JH_Auth_BranchB().GetEntity(d => d.DeptCode == tempcode && d.ComId == UserInfo.User.ComId);

                            userNew.BranchCode = branchTemp.DeptCode;
                            userNew.remark = branchTemp.Remark1.Split('-')[0];
                        }
                        else
                        {
                            userNew.BranchCode = bRootid;
                        }
                        userNew.ComId = UserInfo.User.ComId;
                        userNew.IsUse = "Y";
                        userNew.mailbox = row[3].ToString();
                        userNew.mobphone = row[2].ToString();
                        userNew.RoomCode = row[7].ToString();
                        userNew.Sex = row[1].ToString();
                        userNew.telphone = row[9].ToString();
                        DateTime result;
                        if (DateTime.TryParse(row[10].ToString(), out result))
                        {
                            userNew.Birthday = result;
                        }

                        userNew.UserGW = row[6].ToString();
                        userNew.UserName = row[2].ToString();
                        userNew.UserRealName = row[0].ToString();
                        userNew.zhiwu = row[5].ToString() == "" ? "员工" : row[5].ToString();
                        userNew.UserPass = CommonHelp.GetMD5(P2);
                        userNew.CRDate = DateTime.Now;
                        userNew.CRUser = UserInfo.User.UserName;

                        if (!string.IsNullOrEmpty(row[8].ToString()))
                        {
                            int orderNum = 0;
                            int.TryParse(row[8].ToString(), out orderNum);
                            userNew.UserOrder = orderNum;

                        }
                        try
                        {
                            msg.ErrorMsg = "";
                            if (string.IsNullOrEmpty(userNew.UserName))
                            {
                                msg.ErrorMsg = "用户名必填";
                            }
                            //Regex regexPhone = new Regex("^0?1[3|4|5|8|7][0-9]\\d{8}$");
                            //if (!regexPhone.IsMatch(userNew.UserName))
                            //{
                            //    msg.ErrorMsg = "用户名必须为手机号";
                            //}
                            if (string.IsNullOrEmpty(userNew.mobphone))
                            {
                                msg.ErrorMsg = "手机号必填";
                            }
                            //if (!regexPhone.IsMatch(userNew.mobphone))
                            //{
                            //    msg.ErrorMsg = "手机号填写不正确";
                            //}
                            Regex regexOrder = new Regex("^[0-9]*$");
                            if (userNew.UserOrder != null && !regexOrder.IsMatch(userNew.UserOrder.ToString()))
                            {
                                msg.ErrorMsg = "序号必须是数字";
                            }
                            if (msg.ErrorMsg != "")
                            {
                                userMsg += "第" + rowIndex + "行" + msg.ErrorMsg + "<br/>";
                            }
                            if (msg.ErrorMsg == "")
                            {
                                new JH_Auth_UserB().Insert(userNew);
                                JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleName == userNew.zhiwu && d.ComId == UserInfo.User.ComId);
                                if (role == null)
                                {
                                    role = new JH_Auth_Role();
                                    role.PRoleCode = 0;
                                    role.RoleName = userNew.zhiwu;
                                    role.RoleDec = userNew.zhiwu;
                                    role.IsUse = "Y";
                                    role.isSysRole = "N";
                                    role.leve = 0;
                                    role.ComId = UserInfo.User.ComId;
                                    role.DisplayOrder = 0;
                                    new JH_Auth_RoleB().Insert(role);
                                }
                                string strSql = string.Format("INSERT into JH_Auth_UserRole (UserName,RoleCode,ComId) Values('{0}',{1},{2})", userNew.UserName, role.RoleCode, UserInfo.User.ComId);
                                new JH_Auth_RoleB().ExsSql(strSql);
                                string isFS = context.Request["issend"] ?? "";
                                if (isFS.ToLower() == "true")
                                {
                                    string content = string.Format("尊敬的" + userNew.UserName + "用户您好：你已被添加到" + UserInfo.QYinfo.QYName + ",账号：" + userNew.mobphone + "，密码" + P2 + ",登录请访问" + UserInfo.QYinfo.WXUrl);
                                    new SZHL_DXGLB().SendSMS(userNew.mobphone, content, userNew.ComId.Value);
                                }
                                j++;
                            }
                        }
                        catch (Exception ex)
                        {
                            userMsg += "第" + rowIndex + "行" + msg.ErrorMsg + "<br/>";
                        }

                    }
                    else
                    {

                        userMsg += "第" + rowIndex + "行" + "用户“" + row[2].ToString() + "”已存在<br/>";
                    }
                }
                else
                {
                    branchErrorMsg += "第" + rowIndex + "行所在部门必填<br/>";
                }

            }
            msg.Result = branchErrorMsg + "<br/>" + userMsg;
            msg.Result1 = "新增部门" + i + "个,新增用户" + j + "个<br/>" + branchMsg + (branchMsg == "" ? "" : "<br/>");
        }



        #endregion

        #region 获取系统首页用户数量信息
        public void GETUSERCOUNT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT COUNT(0) TotalUser,isnull(sum(case when isgz=1 then 1 else 0 end ),0) gzCount,isnull(sum(case when isgz=4 then 1 else 0 end ),0) wgzCount,isnull(sum(case when IsUse!='Y' then 1 else 0 end ),0) wjhCount from JH_Auth_User where ComId={0}", UserInfo.User.ComId);
            msg.Result = new JH_Auth_UserB().GetDTByCommand(strSql);
            msg.Result1 = UserInfo.QYinfo.IsUseWX;
        }
        #endregion

        /// <summary>
        /// 同步通讯录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">初始化密码</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>


        #region 企业号相关
        /// <summary>
        /// 将系统的组织架构同步到微信中去
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void TBBRANCHUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            //判断是否启用微信后，启用部门需要同步添加微信部门
            if (UserInfo.QYinfo.IsUseWX == "Y")
            {

                #region 同步部门

                //系统部门
                List<JH_Auth_Branch> branchList = new JH_Auth_BranchB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.WXBMCode == null).ToList();

                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                //微信部门
                GetDepartmentListResult bmlist = wx.WX_GetBranchList("");
                foreach (JH_Auth_Branch branch in branchList)
                {
                    List<DepartmentList> departList = bmlist.department.Where(d => d.name == branch.DeptName).ToList();
                    WorkJsonResult result = null;
                    if (departList.Count() > 0)
                    {
                        branch.WXBMCode = int.Parse(departList[0].id.ToString());
                        result = wx.WX_UpdateBranch(branch);
                    }
                    else
                    {

                        int branchWxCode = int.Parse(wx.WX_CreateBranchTB(branch).ToString());
                        branch.WXBMCode = branchWxCode;
                    }
                    new JH_Auth_BranchB().Update(branch);
                }

                #endregion

                #region 同步人员
                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);
                List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.UserName != "administrator").ToList();
                foreach (JH_Auth_User user in userList)
                {
                    if (yg.userlist.Where(d => d.name == user.UserName || d.mobile == user.mobphone).Count() > 0)
                    {
                        wx.WX_UpdateUser(user);
                    }
                    else
                    {

                        wx.WX_CreateUser(user);
                    }
                }
                #endregion
            }
        }



        /// <summary>
        /// 从企业微信同步到系统里
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void TBTXL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {

                int bmcount = 0;
                int rycount = 0;
                if (P1 == "")
                {
                    msg.ErrorMsg = "请输入初始密码";
                    return;
                }
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                #region 更新部门
                GetDepartmentListResult bmlist = wx.WX_GetBranchList("");
                foreach (var wxbm in bmlist.department.OrderBy(d => d.parentid))
                {
                    var bm = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == wxbm.id);
                    if (bm == null)
                    {
                        #region 新增部门
                        JH_Auth_Branch jab = new JH_Auth_Branch();
                        jab.WXBMCode = int.Parse(wxbm.id.ToString());
                        jab.ComId = UserInfo.User.ComId;
                        jab.DeptName = wxbm.name;
                        jab.DeptDesc = wxbm.name;
                        jab.DeptShort = int.Parse(wxbm.order.ToString());

                        if (wxbm.parentid == 0)//如果是跟部门,设置其跟部门为-1
                        {
                            jab.DeptRoot = -1;
                        }
                        else
                        {
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == wxbm.parentid);
                            jab.DeptRoot = bm1.DeptCode;
                            jab.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, jab.DeptRoot);
                        }


                        new JH_Auth_BranchB().Insert(jab);
                        jab.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, jab.DeptRoot) + jab.DeptCode;
                        new JH_Auth_BranchB().Update(jab);


                        bmcount = bmcount + 1;
                        #endregion
                    }
                    else
                    {
                        //同步部门时放弃更新现有部门

                    }
                }
                #endregion

                #region 更新人员
                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);
                foreach (var u in yg.userlist)
                {
                    var user = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, u.userid);
                    if (user == null)
                    {
                        #region 新增人员
                        JH_Auth_User jau = new JH_Auth_User();
                        jau.ComId = UserInfo.User.ComId;
                        jau.UserName = u.userid;
                        jau.UserPass = CommonHelp.GetMD5(P1);
                        jau.UserRealName = u.name;
                        jau.Sex = u.gender == 1 ? "男" : "女";
                        if (u.department.Length > 0)
                        {
                            int id = int.Parse(u.department[0].ToString());
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == id);
                            jau.BranchCode = bm1.DeptCode;
                            jau.remark = bm1.Remark1.Split('-')[0];//用户得部门路径

                        }
                        jau.mailbox = u.email;
                        jau.mobphone = u.mobile;
                        jau.zhiwu = string.IsNullOrEmpty(u.position) ? "员工" : u.position;
                        jau.IsUse = "Y";

                        if (u.status == 1 || u.status == 4)
                        {
                            jau.isgz = u.status.ToString();
                        }
                        jau.txurl = u.avatar;

                        new JH_Auth_UserB().Insert(jau);

                        rycount = rycount + 1;
                        #endregion

                        //为所有人增加普通员工的权限
                        JH_Auth_Role rdefault = new JH_Auth_RoleB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.isSysRole == "Y" && p.RoleName == "员工");//找到默认角色
                        if (rdefault != null)
                        {
                            JH_Auth_UserRole jaurdefault = new JH_Auth_UserRole();
                            jaurdefault.ComId = UserInfo.User.ComId;
                            jaurdefault.RoleCode = rdefault.RoleCode;
                            jaurdefault.UserName = jau.UserName;
                            new JH_Auth_UserRoleB().Insert(jaurdefault);
                        }


                    }
                    else
                    {
                        //同步人员时放弃更新现有人员
                        #region 更新人员
                        user.UserRealName = u.name;
                        if (u.department.Length > 0)
                        {
                            int id = int.Parse(u.department[0].ToString());
                            var bm1 = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.WXBMCode == id);
                            user.BranchCode = bm1.DeptCode;
                        }
                        user.mailbox = u.email;
                        user.mobphone = u.mobile;
                        user.zhiwu = string.IsNullOrEmpty(u.position) ? "员工" : u.position;
                        user.Sex = u.gender == 1 ? "男" : "女";
                        if (u.status == 1 || u.status == 4)
                        {
                            user.IsUse = "Y";
                            user.isgz = u.status.ToString();
                        }
                        else if (u.status == 2)
                        {
                            user.IsUse = "N";
                        }
                        user.txurl = u.avatar;

                        new JH_Auth_UserB().Update(user);
                        #endregion
                    }

                    #region 更新角色(职务)
                    if (!string.IsNullOrEmpty(u.position))
                    {
                        var r = new JH_Auth_RoleB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.RoleName == u.position);

                        if (r == null)
                        {
                            JH_Auth_Role jar = new JH_Auth_Role();
                            jar.ComId = UserInfo.User.ComId;
                            jar.RoleName = u.position;
                            jar.RoleDec = u.position;
                            jar.PRoleCode = 0;
                            jar.isSysRole = "N";
                            jar.IsUse = "Y";
                            jar.leve = 0;
                            jar.DisplayOrder = 0;

                            new JH_Auth_RoleB().Insert(jar);

                            JH_Auth_UserRole jaur = new JH_Auth_UserRole();
                            jaur.ComId = UserInfo.User.ComId;
                            jaur.RoleCode = jar.RoleCode;
                            jaur.UserName = u.userid;
                            new JH_Auth_UserRoleB().Insert(jaur);


                        }
                        else
                        {

                        }
                    }
                    #endregion
                }
                #endregion



                msg.Result1 = bmcount;
                msg.Result2 = rycount;


            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.ToString();
            }
        }

        //同步关注状态
        public void TBGZSTATUS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {

                JH_Auth_Branch branchModel = new JH_Auth_BranchB().GetEntity(d => d.DeptRoot == -1 && d.ComId == UserInfo.User.ComId);

                #region 同步用户关注状态
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                GetDepartmentMemberInfoResult yg = wx.WX_GetDepartmentMemberInfo(branchModel.WXBMCode.Value);

                if (yg != null && yg.userlist != null)
                {
                    foreach (var u in yg.userlist)
                    {

                        JH_Auth_User user = new JH_Auth_UserB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.UserName == u.userid);

                        if (user != null && u != null && (u.status == 1 || u.status == 4))
                        {
                            user.isgz = u.status.ToString();
                            new JH_Auth_UserB().Update(user);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
            #endregion

        }

        public void YZCOMPANYQYH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY company = new JH_Auth_QY();
            company = JsonConvert.DeserializeObject<JH_Auth_QY>(P1);


            if (string.IsNullOrEmpty(company.corpSecret) || string.IsNullOrEmpty(company.corpId))
            {
                msg.ErrorMsg = "初始化企业号信息失败,corpId,corpSecret 不能为空";
                return;
            }
            if (!new JH_Auth_QYB().Update(company))
            {
                msg.ErrorMsg = "初始化企业号信息失败";
                return;
            }

        }




        /// <summary>
        /// 获取具有手机端的应用列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXAPP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_ModelB().GetEntities(d => !string.IsNullOrEmpty(d.WXUrl)).OrderBy(d => d.ORDERID);
        }


        /// <summary>
        /// 获取当前企业号拥有的IP,只返回和可信域名相同的应用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETQYAPP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = Int32.Parse(P1);
            var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
            msg.Result1 = model;//系统应用数据


            #region 获取应用默认菜单
            DataTable dt = new JH_Auth_CommonB().GetDTByCommand(" select * from JH_Auth_Common where ModelCode='" + model.ModelCode + "' and type='1' order by Sort");
            #endregion

            msg.Result2 = dt;

            //主页型应用的URL
            if (model.AppType == "2")
            {
                msg.Result3 = UserInfo.QYinfo.WXUrl.TrimEnd('/') + "/View_Mobile/UI/UI_COMMON.html?funcode=" + model.ModelCode + "&corpId=" + UserInfo.QYinfo.corpId; ;
            }
        }

        /// <summary>
        /// 保存应用Token和EncodingAESKey
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SAVEMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Model model = JsonConvert.DeserializeObject<JH_Auth_Model>(P1);
            if (model.ID != 0)
            {
                if (string.IsNullOrEmpty(model.AppID))
                {
                    msg.ErrorMsg = "至少选择一个企业号应用才能绑定";
                    return;
                }

                if (model.AppType == "1" && (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.EncodingAESKey)))
                {
                    msg.ErrorMsg = "Token、EncodingAESKey、企业号应用不能为空";
                }
                else
                {
                    new JH_Auth_ModelB().Update(model);
                }
            }
            else
            {
                msg.ErrorMsg = "绑定失败";
            }
        }

        /// <summary>
        /// 创建应用菜单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void CREATEMENU(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int id = Int32.Parse(P1);
                var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
                if (model != null)
                {
                    if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.EncodingAESKey) || string.IsNullOrEmpty(model.AppID))
                    {
                        msg.ErrorMsg = "Token、EncodingAESKey、企业号应用不能为空";
                    }
                    else
                    {
                        WXHelp WX = new WXHelp(UserInfo.QYinfo);
                        List<Senparc.Weixin.Work.Entities.Menu.BaseButton> lm = new List<Senparc.Weixin.Work.Entities.Menu.BaseButton>();
                        WorkJsonResult rel = WX.WX_WxCreateMenuNew(Int32.Parse(model.AppID), model.ModelCode, ref lm);
                        if (rel.errmsg != "ok")
                        {
                            msg.ErrorMsg = "创建菜单失败";
                        }
                    }
                }
                else
                {
                    msg.ErrorMsg = "当前应用不存在";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = "创建菜单失败";
            }
        }

        /// <summary>
        /// 解除应用绑定
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void FIREMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int id = Int32.Parse(P1);
                var model = new JH_Auth_ModelB().GetEntity(p => p.ID == id);
                if (model != null)
                {
                    //WXHelp WX = new WXHelp(UserInfo.QYinfo);
                    //WX.WX_DelMenu(Int32.Parse( model.AppID));

                    model.AppID = "";
                    model.Token = "";
                    model.EncodingAESKey = "";

                    new JH_Auth_ModelB().Update(model);
                }
                else
                {
                    msg.ErrorMsg = "当前应用不存在";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = "解除绑定失败";
            }
        }




        /// <summary>
        /// @用的查询用户数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERSBYKEY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {


            DataTable dt1 = new JH_Auth_UserB().GetDTByCommand("SELECT TOP 5 UserName,UserRealName,C.DeptName +'/'+ B.DeptName as PNAME  FROM JH_Auth_User a LEFT  JOIN  JH_Auth_Branch B on A.BranchCode=B.DeptCode  INNER   JOIN   JH_Auth_Branch C on b.DeptRoot=c.DeptCode WHERE   a.isTX='N' AND UserRealName LIKE '%" + P1 + "%'");
            DataTable dt2 = new JH_Auth_UserB().GetDTByCommand("SELECT  B.deptName +'/'+ A.deptName  AS PNAME,A.DeptCode FROM JH_Auth_Branch A INNER JOIN   JH_Auth_Branch B on A.DeptRoot=B.DeptCode  WHERE A.deptName LIKE '%" + P1 + "%' OR B.deptName LIKE '%" + P1 + "%'");
            msg.Result = dt1;
            msg.Result1 = dt2;


        }


        #endregion
    }
}