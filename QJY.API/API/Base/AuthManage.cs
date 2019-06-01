using FastReflectionLib;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QJY.Common;
using QJY.Data;
using Senparc.NeuChar.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace QJY.API
{
    public class AuthManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(AuthManage).GetMethod(msg.Action.ToUpper());
            AuthManage model = new AuthManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        #region 登录及密码
        public void MODIFYPWD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 == P2)
            {
                P1 = CommonHelp.GetMD5(P1);
                string userName = UserInfo.User.UserName;
                string uName = context.Request["username"] ?? "";
                if (!string.IsNullOrEmpty(uName))
                {
                    userName = uName;
                }
                new JH_Auth_UserB().UpdatePassWord(UserInfo.User.ComId.Value, userName, P1);
            }
            else
            {
                msg.ErrorMsg = "确认密码不一致";
            }
        }
        #endregion


        #region 部门管理


        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">部门名称</param>
        /// <param name="P2">部门描述</param>
        /// <param name="strUserName"></param>
        public void ADDBRANCH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Branch branch = new JH_Auth_Branch();
            branch = JsonConvert.DeserializeObject<JH_Auth_Branch>(P1);
            new JH_Auth_BranchB().AddBranch(UserInfo, branch, msg);
        }
        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETBRANCHBYCODE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int code = int.Parse(P1);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, code);
            msg.Result = branch;
            msg.Result1 = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, branch.DeptRoot);
        }
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void DELBRANCH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {


            int deptCode = int.Parse(P1);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetEntity(d => d.DeptCode == deptCode);
            if (branch != null)
            {

                if (new JH_Auth_UserB().GetEntities(d => d.BranchCode == deptCode && d.ComId == UserInfo.User.ComId).ToList().Count > 0)
                {
                    msg.ErrorMsg = "本部门中存在用户，请先删除用户";
                    return;
                }
                if (new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == branch.DeptCode).Count() > 0)
                {
                    msg.ErrorMsg = "本部门中存在子部门，请先删除子部门";
                    return;
                }
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp bm = new WXHelp(UserInfo.QYinfo);
                    bm.WX_DelBranch(branch.WXBMCode.ToString());
                }
                if (!new JH_Auth_BranchB().Delete(d => d.DeptCode == deptCode))
                {
                    msg.ErrorMsg = "删除部门失败";
                    return;
                }
            }


        }
        /// <summary>
        /// 获取部门列表，分配角色组织机构以及选择组织机构用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETALLBMUSERLISTNEW(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //判断是否强制加载所有部门数据
            string isSupAdmin = UserInfo.UserRoleCode.Contains("1218") ? "Y" : "N";//是不是超级管理员
            DataTable dtBMS = new DataTable();
            int deptRoot = 1;//默认从根目录加载
            string branchId = "";

            if (isSupAdmin == "N")
            {
                deptRoot = int.Parse(UserInfo.User.remark);
                branchId = UserInfo.UserBMQXCode;
                //不是超级管理员,从单位开始加载,权限部门设为空
            }
            //获取有权限的部门Id
            string strUserTree = "[" + new JH_Auth_BranchB().GetBranchTree(deptRoot, UserInfo.User.ComId.Value, P1, branchId).TrimEnd(',') + "]";
            msg.Result = strUserTree;
            msg.Result1 = UserInfo.User.BranchCode;
        }

        //新分级显示获取部门数据
        public void GETALLBMNEW(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtBMS = new DataTable();
            //获取有权限的部门Id
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            int rootBrancode = 1;


            if (!UserInfo.UserRoleCode.Contains("1218"))
            {
                rootBrancode = int.Parse(UserInfo.User.remark);
            }
            DataTable dt = new JH_Auth_BranchB().GetDTByCommand("SELECT * from JH_Auth_Branch  where DeptCode=" + rootBrancode + " and ComId=" + UserInfo.User.ComId.Value + " order by DeptShort DESC");
            dt.Columns.Add("ChildBranch", Type.GetType("System.Object"));

            dtBMS = new JH_Auth_BranchB().GetBranchList(rootBrancode, UserInfo.User.ComId.Value, branchId);
            dt.Rows[0]["ChildBranch"] = dtBMS;
            msg.Result = dt;
        }
        //机构管理添加人员加载的部门列表
        public void GETDLLBRANCHLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //获取有权限的部门Id
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            List<JH_Auth_Branch> branchList = new List<JH_Auth_Branch>();
            if (!string.IsNullOrEmpty(branchId))
            {
                int deptCode = int.Parse(branchId);
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, deptCode);
                string strSql = string.Format(" SELECT * from JH_Auth_Branch  where ComId={0} and (Remark1 like '%{1}%' OR DeptCode={2})", UserInfo.User.ComId, branch.DeptRoot + "-" + branch.DeptCode, branch.DeptCode); ;
                msg.Result = new JH_Auth_BranchB().GetDTByCommand(strSql);
            }
            else
            {
                branchList = new JH_Auth_BranchB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.DeptRoot != -1).ToList();

                msg.Result = branchList;
            }

        }
        #endregion

        #region 企业信息
        /// <summary>
        /// 添加或编辑企业信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void EDITCOMPANY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY company = new JH_Auth_QY();
            company = JsonConvert.DeserializeObject<JH_Auth_QY>(P1);
            if (company.ComId != 0)
            {
                if (!new JH_Auth_QYB().Update(company))
                {
                    msg.ErrorMsg = "修改企业信息失败";
                }
            }
            else
            {
                JH_Auth_QY company1 = new JH_Auth_QYB().GetEntity(d => d.QYName == company.QYName);
                if (company1 != null)
                {
                    msg.ErrorMsg = "企业名称已存在";
                    return;
                }
                company.CRUser = UserInfo.User.UserName;
                company.CRDate = DateTime.Now;
                if (!new JH_Auth_QYB().Insert(company))
                {
                    msg.ErrorMsg = "添加企业信息失败";
                }
            }
        }

        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETCOMPANYINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = UserInfo.QYinfo;
        }
        public void SAVECOMPANYQZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            UserInfo.QYinfo.DXQZ = P1;
            new JH_Auth_QYB().Update(UserInfo.QYinfo);
        }
        //更新下次不再提示
        public void UPDATECOMPANYNOALERT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_QY qymodel = UserInfo.QYinfo;
            qymodel.SystemGGId = "Y";
            new JH_Auth_QYB().Update(qymodel);
        }
        #endregion

        #region 组织部门、人员

        /// <summary>
        /// 添加人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void ADDUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            JH_Auth_User user = new JH_Auth_User();
            user = JsonConvert.DeserializeObject<JH_Auth_User>(P1);
            if (string.IsNullOrEmpty(user.UserName))
            {
                msg.ErrorMsg = "用户名必填";
                return;
            }
            if (string.IsNullOrEmpty(user.mobphone))
            {
                msg.ErrorMsg = "手机号必填";
                return;
            }
            //Regex regexPhone = new Regex("^0?1[3|4|5|8|7][0-9]\\d{8}$");
            //if (!regexPhone.IsMatch(user.mobphone))
            //{
            //    msg.ErrorMsg = "手机号填写不正确";
            //    return;
            //}
            Regex regexOrder = new Regex("^[0-9]*$");
            if (user.UserOrder != null && !regexOrder.IsMatch(user.UserOrder.ToString()))
            {
                msg.ErrorMsg = "序号必须是数字";
                return;
            }
            if (user.ID != 0)
            {
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_UpdateUser(user);
                }
                if (!new JH_Auth_UserB().Update(user))
                {
                    msg.ErrorMsg = "修改用户失败";
                }
            }
            else
            {
                JH_Auth_User user1 = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, user.UserName);
                if (user1 != null)
                {
                    msg.ErrorMsg = "用户已存在";
                    return;
                }
                List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.mobphone == user.mobphone && d.ComId == UserInfo.User.ComId).ToList();
                if (userList.Count > 0)
                {
                    msg.ErrorMsg = "此手机号的用户已存在";
                    return;
                }
                user.UserPass = CommonHelp.GetMD5(user.UserPass);
                user.ComId = UserInfo.User.ComId;
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_CreateUser(user);
                }
                user.CRDate = DateTime.Now;
                user.CRUser = UserInfo.User.UserName;
                user.logindate = DateTime.Now;
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(user.ComId.Value, user.BranchCode);

                user.remark = branch.Remark1.Split('-')[0];
                if (!new JH_Auth_UserB().Insert(user))
                {
                    msg.ErrorMsg = "添加用户失败";
                }

            }

            if (P2 != "")
            {
                new JH_Auth_UserRoleB().Delete(d => d.UserName == user.UserName);
                foreach (string code in P2.Split(','))
                {
                    //添加默认员工角色
                    JH_Auth_UserRole Model = new JH_Auth_UserRole();
                    Model.UserName = user.UserName;
                    Model.RoleCode = int.Parse(code);
                    Model.ComId = user.ComId;
                    new JH_Auth_UserRoleB().Insert(Model);
                }
            }

            msg.Result = user;


        }
        //批量设置部门
        public void PLSETBRANCH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string[] userNames = P1.Split(',');
            int branchCode = 0;
            int.TryParse(P2, out branchCode);
            List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.ComId == UserInfo.User.ComId && userNames.Contains(d.UserName)).ToList();
            foreach (JH_Auth_User user in userList)
            {
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(user.ComId.Value, branchCode);

                user.BranchCode = branchCode;
                user.remark = branch.Remark1.Split('-')[0];
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.WX_UpdateUser(user);
                }
                new JH_Auth_UserB().Update(user);
            }
        }
        /// <summary>
        /// 根据用户删除用户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void DELUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            if (UserInfo.QYinfo.IsUseWX == "Y")
            {
                WXHelp bm = new WXHelp(UserInfo.QYinfo);
                bm.WX_DelUser(P1);
            }
            if (!new JH_Auth_UserB().Delete(d => d.ComId == UserInfo.QYinfo.ComId && d.UserName == P1))
            {
                msg.ErrorMsg = "删除失败";
            }


        }
        /// <summary>
        /// 更改用户是否禁用的状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">用户名</param>
        /// <param name="P2">状态</param>
        /// <param name="strUserName"></param>
        public void UPDATEUSERISUSE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            JH_Auth_User UPUser = new JH_Auth_UserB().GetUserByUserName(UserInfo.QYinfo.ComId, P1);
            UPUser.IsUse = P2;

            if (UserInfo.QYinfo.IsUseWX == "Y")
            {
                WXHelp bm = new WXHelp(UserInfo.QYinfo);
                bm.WX_UpdateUser(UPUser);//为了更新微信用户状态
            }
            if (!new JH_Auth_UserB().Update(UPUser))
            {
                msg.ErrorMsg = "更新失败";
            }

        }

        public void SETISSHOWYD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_User user = UserInfo.User;
            user.IsShowYD = 1;
            new JH_Auth_UserB().Update(user);
        }

        /// <summary>
        /// 根据部门编号获取部门人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETUSERBYCODENEW(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            if (!int.TryParse(P1, out deptCode))
            {
                deptCode = 1;
            }
            DataTable dt = new JH_Auth_UserB().GetUserListbyBranch(deptCode, P2, UserInfo.QYinfo.ComId);
            dt.Columns.Add("ROLENAME");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["ROLENAME"] = new JH_Auth_UserRoleB().GetRoleNameByUserName(dt.Rows[i]["UserName"].ToString(), UserInfo.User.ComId.Value);
            }
            msg.Result = dt;
        }
        /// <summary>
        /// 根据部门编号获取部门人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETUSERBYCODENEW_PAGE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            int.TryParse(P1, out deptCode);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, deptCode);
            if (branch == null) { msg.ErrorMsg = "数据异常"; }
            string strQXWhere = "";
            if (deptCode != 1)
            {
                strQXWhere = string.Format("And  b.Remark1 like '%{0}%'", branch.DeptCode);

            }

            string tableName = " JH_Auth_User u  inner join JH_Auth_Branch b on u.branchCode=b.DeptCode";
            string tableColumn = " u.*,b.DeptName,b.DeptCode";
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

            dt.Columns.Add("ROLENAME");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["ROLENAME"] = new JH_Auth_UserRoleB().GetRoleNameByUserName(dt.Rows[i]["UserName"].ToString(), UserInfo.User.ComId.Value);
            }

            msg.Result = dt;

            msg.Result1 = Math.Ceiling(total * 1.0 / 8);
            msg.Result2 = total;
        }
        //导出员工
        public void EXPORTYG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            GETUSERBYCODENEW_PAGE(context, msg, P1, P2, UserInfo);

            DataTable dt = msg.Result;

            string sqlCol = "ID,UserOrder|序号,DeptName|部门,RoomCode|房间号,UserName|账号,UserRealName|姓名,Sex|性别,mobphone|手机,QQ|QQ,weixinCard|微信,mailbox|邮箱,telphone|座机,ROLENAME|职务,Usersign|职责,UserGW|岗位,IDCard|身份证,HomeAddress|家庭住址";
            DataTable dtResult = dt.DelTableCol(sqlCol);
            DataTable dtExtColumn = new JH_Auth_ExtendModeB().GetExtColumnAll(UserInfo.QYinfo.ComId, "YGGL");
            foreach (DataRow drExt in dtExtColumn.Rows)
            {
                dtResult.Columns.Add(drExt["TableFiledName"].ToString(), Type.GetType("System.String"));
            }

            if (dtExtColumn.Rows.Count > 0)
            {
                foreach (DataRow dr in dtResult.Rows)
                {
                    DataTable dtExtData = new JH_Auth_ExtendModeB().GetExtDataAll(UserInfo.QYinfo.ComId, "YGGL", dr["ID"].ToString());
                    foreach (DataRow drExtData in dtExtData.Rows)
                    {
                        dr[drExtData["TableFiledName"].ToString()] = drExtData["ExtendDataValue"].ToString();
                    }
                }
            }
            dtResult.Columns.Remove("ID");
            CommonHelp ch = new CommonHelp();
            msg.ErrorMsg = ch.ExportToExcel("员工", dtResult);
        }

        /// <summary>
        /// 根据部门编号获取可用人员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETYUSERBYCODE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int deptCode = 0;
            if (!int.TryParse(P1, out deptCode))
            {
                deptCode = 1;
            }
            DataTable dtUser = new JH_Auth_UserB().GetUserListbyBranchUse(deptCode, P2, UserInfo);
            msg.Result = dtUser;
        }
        //根据角色获取用户
        public void GETUSERBYROLECODE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = int.Parse(P1);
            DataTable dt = new JH_Auth_UserRoleB().GetUserDTByRoleCode(roleCode, UserInfo.User.ComId.Value);

            if (!UserInfo.UserRoleCode.Contains("1218"))
            {
                msg.Result = dt.FilterTable("remark=" + UserInfo.User.remark);//根据权限找同一个单位得
            }
            else
            {
                msg.Result = dt;
            }

        }
        /// <summary>
        /// 获取前端需要的人员选择列表
        /// </summary>
        public void GETUSERJS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtUsers = new JH_Auth_UserB().GetDTByCommand(" SELECT UserName,UserRealName,DeptCode,DeptName FROM JH_Auth_User INNER JOIN JH_Auth_Branch ON JH_Auth_User.ComId=JH_Auth_Branch.ComId AND JH_Auth_User.BranchCode=JH_Auth_Branch.DeptCode WHERE  JH_Auth_Branch.ComId='" + UserInfo.User.ComId + "'");
            //获取选择用户需要的HTML和转化用户名需要的json数据
            msg.Result = dtUsers;
        }

        public void SETBRANCHLEADER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("UPDATE JH_Auth_Branch set BranchLeader='{0}' where  DeptCode={1}", P1, P2);
            new JH_Auth_BranchB().ExsSql(strSql);
        }
        public void SETUSERLEADER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("UPDATE JH_Auth_User set UserLeader='{0}' where  username='{1}' and ComID={2}", P1, P2, UserInfo.User.ComId);
            new JH_Auth_BranchB().ExsSql(strSql);
        }
        #endregion




        #region 角色管理
        public void EDITROLE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Role role = new JH_Auth_Role();
            role = JsonConvert.DeserializeObject<JH_Auth_Role>(P1);

            if (role.RoleCode != 0)
            {
                if (role.RoleCode == 0 && P2 == "")
                {
                    msg.ErrorMsg = "管理员至少有一人！！！";
                    return;
                }
                if (!new JH_Auth_RoleB().Update(role))
                {
                    msg.ErrorMsg = "修改职务失败";
                }

            }
            else
            {
                if (string.IsNullOrEmpty(role.ComId.ToString()))
                {
                    JH_Auth_Role user1 = new JH_Auth_RoleB().GetEntity(d => d.RoleName == role.RoleName && d.ComId == UserInfo.User.ComId && d.IsUse == "Y");
                    if (user1 != null)
                    {
                        msg.ErrorMsg = "职务已存在";
                        return;
                    }
                    role.isSysRole = "N";
                    role.ComId = UserInfo.User.ComId;
                    if (!new JH_Auth_RoleB().Insert(role))
                    {
                        msg.ErrorMsg = "添加职务失败";
                    }
                }

            }
            if (msg.ErrorMsg == "")
            {
                new JH_Auth_UserRoleB().Delete(d => d.RoleCode == role.RoleCode);
            }
            if (P2 != "")
            {
                try
                {
                    foreach (string name in P2.Split(','))
                    {
                        string strSQL = string.Format("INSERT INTO  [JH_Auth_UserRole] ([UserName]  ,[RoleCode],ComID) VALUES  ('{0}'  ,'{1}','{2}')", name, role.RoleCode, UserInfo.User.ComId);
                        new JH_Auth_UserRoleB().ExsSql(strSQL);
                    }
                }
                catch (Exception ex)
                {
                    msg.ErrorMsg = ex.Message;
                }
            }
            msg.Result = role;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="strUserName"></param>
        public void GETROLE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = " where r.PRoleCode<>-1 and r.RoleCode!=0 and ( r.ComId=0 or  r.ComId=" + UserInfo.User.ComId + ")";
            if (P2 == "Y")
            {
                //去掉超级管理员
                strWhere = strWhere + " AND  r.RoleCode !='1218'";

            }
            if (P1 != "")
            {
                int Id = int.Parse(P1);
                msg.Result1 = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, Id);
            }
            DataTable dt = new JH_Auth_RoleB().GetDTByCommand(@" select r.RoleCode,r.RoleName,r.isSysRole,r.RoleQX,r.IsHasQX,COUNT(distinct u.UserName) userCount from JH_Auth_Role r left join JH_Auth_UserRole ur on r.RoleCode=ur.RoleCode and ur.ComId=" + UserInfo.User.ComId + @"     
                                                               left join JH_Auth_User u on ur.UserName=u.UserName  and u.ComId=" + UserInfo.User.ComId + strWhere + " group by r.RoleCode,r.RoleName,r.isSysRole,r.RoleQX,r.IsHasQX");

            msg.Result = dt;
            msg.Result2 = UserInfo.QYinfo.ComId.ToString();

        }

        public void DELROLE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == ID);
            if (role != null && role.ComId != 0 && role.RoleCode != 2)
            {
                new JH_Auth_RoleB().delRole(ID, UserInfo.User.ComId.Value);
            }
            else
            {
                msg.ErrorMsg = "此职务不能删除";
            }
        }
        public void GETROLEBYCODE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            int ComId = UserInfo.User.ComId.Value;
            if (Id == 0)
            {
                ComId = 0;
            }
            msg.Result = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == Id && d.ComId == ComId);
            int roleCode = int.Parse(P1);
            msg.Result1 = new JH_Auth_UserRoleB().GetDTByCommand("SELECT DISTINCT u.UserName,userrole.RoleCode from JH_Auth_User u inner join   JH_Auth_UserRole  userrole on u.username=userrole.username where userrole.RoleCode=" + roleCode);
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETROLEFUN(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            if (!string.IsNullOrEmpty(P1) && UserInfo.UserRoleCode != "" && UserInfo.User.isSupAdmin != "Y")
            {
                msg.Result = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, UserInfo.UserRoleCode, P1);
            }
            else
            {

                msg.Result = new JH_Auth_RoleB().GetModelFun(UserInfo.User.ComId.Value, "0", P1);
            }
        }
        //删除角色人员
        public void DELROLEUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = 0;
            int.TryParse(P1, out roleCode);
            new JH_Auth_UserRoleB().Delete(d => d.RoleCode == roleCode && d.UserName == P2 && d.ComId == UserInfo.User.ComId);
        }

        #endregion




        #region 消息中心
        public void GETXXZXIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" ComId={0} and UserTO='{1}' and isRead=0", UserInfo.User.ComId, UserInfo.User.UserName);
            string strSQL = string.Format(@"SELECT  MsgType,MsgContent,CRDate,UserFrom,isRead,ID,MsgLink from JH_Auth_User_Center Where  {0}  order by CRDate desc", strWhere);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSQL);
            msg.Result = dt;
            msg.Result1 = new JH_Auth_User_CenterB().ExsSclarSql("SELECT count(0) from  JH_Auth_User_Center Where  " + strWhere);
        }
        //抄送给我的消息
        public void GETXXZXABOUTLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("  isCS='Y'  And  ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" And MsgType='{0}'", P1);
            }
            string strSQL = string.Format(@"SELECT top 8 MsgType,MsgContent,CRDate,UserFrom,isRead,ID,MsgLink,MsgModeID from JH_Auth_User_Center Where {0}order by CRDate desc", strWhere);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSQL);
            msg.Result = dt;
            msg.Result1 = new JH_Auth_User_CenterB().ExsSclarSql("SELECT count(0) from  JH_Auth_User_Center Where  " + strWhere);
        }
        //抄送给我的消息
        public void GETXXZXABOUTTYPE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("  isCS='Y'  And  ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            string strSQL = string.Format(@"SELECT DISTINCT MsgType from JH_Auth_User_Center Where {0} ", strWhere);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSQL);
            msg.Result = dt;

        }
        /// <summary>
        /// 获取用户消息列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">消息类型</param>
        /// <param name="P2">消息内容模糊查询</param>
        /// <param name="strUserName"></param>
        public void GETXXZXIST_PAGE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format(" ComId={0}  And UserTO='{1}'", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" and isRead in ({0}) ", P1);
            }
            if (P2 != "")
            {
                strWhere += string.Format(" and MsgContent like  '%{0}%'", P2);

            }
            string msgType = context.Request["msgType"] ?? "";
            if (msgType != "")
            {
                strWhere += string.Format(" and MsgType ='{0}'", msgType);
            }
            string msgTypes = context.Request["msgTypes"] ?? "";
            if (msgTypes != "")
            {
                msgTypes = System.Web.HttpUtility.UrlDecode(msgTypes);
                strWhere += string.Format(" and MsgModeID in ('{0}')", msgTypes.Replace(",", "','"));
            }

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);
            int.TryParse(context.Request["p"] ?? "0", out page);
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_User_CenterB().GetDataPager("JH_Auth_User_Center", "MsgType,MsgModeID,MsgContent,CRDate,UserFrom,isRead,ID,wxLink,MsgLink ", pagecount, page, "isRead asc,CRDate desc", strWhere, ref total);
            msg.Result = dt;
            //  msg.Result1 = Math.Ceiling(total * 1.0 / pagecount);
            msg.Result1 = total;

        }

        //是否有未读消息
        public void HASREADMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string SQL = string.Format("select top 1 1 from JH_Auth_User_Center where ComId={0} and UserTO='{1}' and isRead=0 ", UserInfo.User.ComId, UserInfo.User.UserName);
            object ss2 = new JH_Auth_User_CenterB().ExsSclarSql(SQL);
            if (ss2 != null)
            {
                msg.Result = "1";  //1:标记有未读消息
            }
        }
        /// <summary>
        /// 更新读取状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPDTEREADSTATES(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strSql = "";
                string status = context.Request["s"] ?? "1";

                strSql = string.Format("UPDATE JH_Auth_User_Center set isRead='{0}' where ID in ({1}) ", status, P1);

                new JH_Auth_User_CenterB().ExsSql(strSql);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 根据消息类别设置消息状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPMSGSTATE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strSql = "";
                strSql = string.Format("UPDATE JH_Auth_User_Center set isRead='1' where MsgModelID = '{0}' AND UserTO='{1}' ", P1, UserInfo.User.UserName);
                new JH_Auth_User_CenterB().ExsSql(strSql);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        /// <summary>
        /// 获取消息中心类型
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETUSERCENTERTYPE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Empty;
            if (P1 != "")
            {
                string msgTypes = System.Web.HttpUtility.UrlDecode(P1);
                strWhere += string.Format(" and MsgModeID in ('{0}')", msgTypes.Replace(",", "','"));
            }
            string strSql = string.Format("SELECT  DISTINCT MsgType from JH_Auth_User_Center where ComId={0} and  userTo='{1}' " + strWhere, UserInfo.User.ComId, UserInfo.User.UserName);
            msg.Result = new JH_Auth_User_CenterB().GetDTByCommand(strSql);
        }
        //获取详细详情
        public void GETXXZXMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_User_Center userCenter = new JH_Auth_User_CenterB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = userCenter;
        }

        //删除消息中心消息
        public void DELXXZX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                if (P1 != "")
                {
                    string strSql = string.Format("Delete JH_Auth_User_Center where ID in ({0}) ", P1);
                    new JH_Auth_User_CenterB().ExsSql(strSql);
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        public void GETXXZXMODELINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format(@"SELECT DISTINCT MsgType,MsgModeID,SUM( case when isRead=0 then 1 else 0 END) num ,MAX(CRDate) CRDate 
                                      from JH_Auth_User_Center where ComId={0}  and UserTO='{1}' group by MsgType,MsgModeID order by CRDate DESC", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dt = new JH_Auth_User_CenterB().GetDTByCommand(strSql);
            dt.Columns.Add("NewXX", Type.GetType("System.Object"));
            foreach (DataRow row in dt.Rows)
            {
                string strSql2 = "SELECT top 1 * from JH_Auth_User_Center where ComId=" + UserInfo.User.ComId + "  and   UserTO='" + UserInfo.User.UserName + "' and MsgType='" + row["MsgType"] + "' ";
                if (row["num"].ToString() != "0")
                {
                    row["NewXX"] = new JH_Auth_User_CenterB().GetDTByCommand(strSql2 + " and isRead=0 order by CRDate DESC");
                }
                else
                {
                    row["NewXX"] = new JH_Auth_User_CenterB().GetDTByCommand(strSql2 + " and isRead=1 order by CRDate DESC");
                }
            }
            msg.Result = dt.OrderBy(" num desc");
        }
        #endregion

        public void GETUSERBYUSERNAME(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //如果获取当前用户信息，直接返回，否则按用户名查找
            if (P1 == UserInfo.User.UserName)
            {
                msg.Result = UserInfo;
                if (!string.IsNullOrEmpty(UserInfo.User.Files))
                {
                    int[] FilesIds = UserInfo.User.Files.SplitTOInt(',');
                    msg.Result1 = new FT_FileB().GetEntities(d => FilesIds.Contains(d.ID));
                    msg.Result2 = UserInfo.QYinfo.CRDate.Value.AddYears(1);
                }
            }
            else
            {
                UserInfo = new JH_Auth_UserB().GetUserInfo(UserInfo.User.ComId.Value, P1);
                msg.Result = UserInfo;
                if (!string.IsNullOrEmpty(UserInfo.User.Files))
                {
                    int[] FilesIds = UserInfo.User.Files.SplitTOInt(',');
                    msg.Result1 = new FT_FileB().GetEntities(d => FilesIds.Contains(d.ID));
                    msg.Result2 = UserInfo.QYinfo.CRDate.Value.AddYears(1);
                }
            }
        }









        #region 字典管理
        public void GETZIDIANLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var zdlist = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P1 && d.ComId == UserInfo.User.ComId && d.Remark == "0");
            if (P2 != "")//内容查询
            {
                zdlist = zdlist.Where(d => d.TypeName.Contains(P2));
            }
            msg.Result = zdlist;
        }
        public void GETZIDIANSLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new DataTable();
            if (P1 != "")
            {
                string[] strs = P1.Split(',');
                if (strs.Length > 1)
                {
                    dt.Columns.Add("Class", Type.GetType("System.String"));
                    dt.Columns.Add("Item", Type.GetType("System.Object"));

                    foreach (string str in strs)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Class"] = str;
                        dr["Item"] = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == str && d.ComId == UserInfo.User.ComId && d.Remark == "0");
                        dt.Rows.Add(dr);
                    }
                }
                else
                {
                    dt = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P1 && d.ComId == UserInfo.User.ComId && d.Remark == "0").ToDataTable();
                }
            }
            else if (P2 != "")
            {
                dt = new JH_Auth_ZiDianB().GetEntities(d => d.Class.ToString() == P2 && d.ComId == UserInfo.User.ComId && d.Remark == "0").ToDataTable();
            }
            msg.Result = dt;
        }
        //获取CRM分类设置列表
        public void GETCRMZIDIANLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int[] classid = new int[] { 10, 11, 12, 13, 14, 15, 16, 17 };
            List<JH_Auth_ZiDian> ZiDianList = new JH_Auth_ZiDianB().GetEntities(d => classid.Contains(d.Class.Value) && (d.ComId == 0 || d.ComId == UserInfo.User.ComId) && d.Remark == "0").ToList();
            DataTable dt = new DataTable();
            dt.Columns.Add("Class");
            dt.Columns.Add("ZiDianDataList", Type.GetType("System.Object"));
            for (int i = 10; i < 18; i++)
            {
                DataRow row = dt.NewRow();
                row["Class"] = i;
                row["ZiDianDataList"] = ZiDianList.Where(d => d.Class == i).ToList();
                dt.Rows.Add(row);
            }
            msg.Result = dt;
        }
        public void DELTYPEBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_ZiDian zidian = new JH_Auth_ZiDianB().GetEntity(d => d.ID == Id);
            if (zidian != null)
            {

                if (zidian.ComId == 0)
                {
                    msg.ErrorMsg = "此类型不能删除";
                }
                else if (zidian.Remark == "0")
                {
                    zidian.Remark = "1";
                    new JH_Auth_ZiDianB().Update(zidian);
                }
            }
        }
        public void SAVETYPEMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_ZiDian zidian = JsonConvert.DeserializeObject<JH_Auth_ZiDian>(P1);
            if (zidian.TypeName.Length > 18)
            {
                msg.ErrorMsg = "分类名称建议不超过10个字!";
                return;
            }
            List<JH_Auth_ZiDian> zidiannew = new JH_Auth_ZiDianB().GetEntities(d => d.TypeName == zidian.TypeName && d.Class == zidian.Class && d.ComId == UserInfo.QYinfo.ComId && d.Remark != "1" && d.ID != zidian.ID).ToList();

            if (zidiannew.Count > 0)
            {
                msg.ErrorMsg = "此分类已存在";
                return;
            }
            if (zidian.ID == 0)
            {
                zidian.ComId = UserInfo.User.ComId;
                zidian.Remark = "0";
                zidian.CRDate = DateTime.Now;
                zidian.CRUser = UserInfo.User.UserName;
                new JH_Auth_ZiDianB().Insert(zidian);

            }
            else
            {
                new JH_Auth_ZiDianB().Update(zidian);
            }
            msg.Result = zidian;
        }
        #endregion

        #region 微信使用
        /// <summary>
        /// 获取微信选人列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWXUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            List<WXUserBR> list = new List<WXUserBR>();
            //获取所有部门
            var listALL = new JH_Auth_BranchB().GetEntities(p => p.ComId == UserInfo.User.ComId).ToList();

            //第一级显示的部门
            var list0 = new List<JH_Auth_Branch>();

            //获取顶级部门信息，加载第二级部门信息列表
            var dcbm = new JH_Auth_BranchB().GetEntity(p => p.ComId == UserInfo.User.ComId && p.DeptRoot == -1);
            list0 = listALL.Where(p => p.DeptRoot == dcbm.DeptCode).OrderBy(p => p.DeptShort).ToList();
            //获取用户所在部门没有权限看到的部门Ids
            string branchId = new JH_Auth_BranchB().GetBranchQX(UserInfo);
            //如果有不能看到的部门Ids,排除用户不能看到的部门
            if (!string.IsNullOrEmpty(branchId))
            {
                int[] noqxBranch = branchId.SplitTOInt(',');
                list0 = list0.Where(d => !noqxBranch.Contains(d.DeptCode)).ToList();
            }

            //var list0 = listALL.Where(p => p.DeptCode == dept).OrderBy(p => p.DeptShort).ToList();
            //循环要显示的第一级部门信息加载部门以下的部门及部门人员信息
            foreach (var v in list0)
            {
                WXUserBR wx = new WXUserBR();
                wx.DeptCode = v.DeptCode;
                wx.DeptName = v.DeptName;
                var users = new JH_Auth_UserB().GetEntities(d => d.BranchCode == v.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.zhiwu });
                wx.DeptUser = users;
                wx.DeptUserNum = users.Count();
                GetNextWxUser(wx, listALL);
                list.Add(wx);
            }

            msg.Result = list;

            DataTable dtUser = new JH_Auth_UserB().GetUserListbyBranchUse(dcbm.DeptCode, "", UserInfo);

            msg.Result1 = dtUser.DelTableCol("ID,UserName,UserRealName,mobphone,telphone,zhiwu");
            msg.Result2 = UserInfo.QYinfo.QYCode;
            msg.Result3 = new JH_Auth_UserB().GetEntities(d => d.BranchCode == dcbm.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.zhiwu });
        }
        /// <summary>
        /// 获取部门下的列表
        /// </summary>
        /// <param name="wx"></param>
        /// <param name="listALL"></param>
        public void GetNextWxUser(WXUserBR wx, List<JH_Auth_Branch> listALL)
        {
            var list = (from p in listALL
                        where p.DeptRoot == wx.DeptCode
                        orderby p.DeptShort
                        select new WXUserBR { DeptCode = p.DeptCode, DeptName = p.DeptName }).ToList();

            wx.SubDept = list;
            foreach (var v in list)
            {
                var users = new JH_Auth_UserB().GetEntities(d => d.BranchCode == v.DeptCode && d.IsUse == "Y").ToList().Select(d => new { d.ID, d.UserName, d.UserRealName, d.telphone, d.mobphone, d.UserGW });
                v.DeptUser = users;
                v.DeptUserNum = users.Count();
                wx.DeptUserNum = wx.DeptUserNum + users.Count();
                GetNextWxUser(v, listALL);
            }
        }

        /// <summary>
        /// 初始化移动端
        /// </summary>
        public void WXINIT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dtUsers = new JH_Auth_UserB().GetDTByCommand(" SELECT UserName,UserRealName,mobphone FROM JH_Auth_User where ComId='" + UserInfo.User.ComId + "'");
            //获取选择用户需要的HTML和转化用户名需要的json数据
            msg.Result = dtUsers;
            JH_Auth_Common url = new JH_Auth_CommonB().GetEntity(p => p.ModelCode == P1 && p.MenuCode == P2);
            if (url != null)
            {
                msg.Result1 = url.Url1;
            }
            msg.Result2 = UserInfo.User.UserName + "," + UserInfo.User.UserRealName + "," + UserInfo.User.BranchCode + "," + UserInfo.BranchInfo.DeptName;
            msg.Result3 = UserInfo.QYinfo.FileServerUrl;
            msg.Result4 = UserInfo.QYinfo.QYCode;

        }


        public void GETUSERINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = UserInfo;
        }

        /// <summary>
        /// 搜索关键字对应的用户和部门
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSEARCHINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_BranchB().GetEntities(D => D.DeptName.Contains(P1));
            msg.Result1 = new JH_Auth_BranchB().GetEntities(D => D.DeptName.Contains(P1));

        }





        #endregion



        #region excel转换为table

        /// <summary>
        /// excel转换为table
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXCELTOTABLE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                DataTable dt = new DataTable();
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

                            List<JH_Auth_ExtendDataB.IMPORTYZ> yz = new List<JH_Auth_ExtendDataB.IMPORTYZ>();
                            yz = new JH_Auth_ExtendDataB().GetTable(P1, UserInfo.QYinfo.ComId);//获取字段
                            string str1 = string.Empty;//验证字段是否包含列名
                            //列名
                            for (int i = 0; i < cellCount; i++)
                            {
                                string strlm = headerRow.GetCell(i).ToString().Trim();
                                if (yz.Count > 0)
                                {
                                    #region 字段是否包含列名验证
                                    var l = yz.Where(p => p.Name == strlm);//验证字段是否包含列名
                                    if (l.Count() == 0)
                                    {
                                        if (string.IsNullOrEmpty(str1))
                                        {
                                            str1 = "文件中的【" + strlm + "】";
                                        }
                                        else
                                        {
                                            str1 = str1 + "、【" + strlm + "】";
                                        }

                                        strlm = strlm + "<span style='color:red;'>不会导入</span>";
                                    }
                                    #endregion
                                }
                                dt.Columns.Add(strlm);//添加列名
                            }

                            if (!string.IsNullOrEmpty(str1))
                            {
                                str1 = str1 + " 不属于当前导入的字段!<br>";
                            }

                            dt.Columns.Add("status", Type.GetType("System.String"));

                            string str2 = string.Empty;//验证必填字段是否存在

                            #region 必填字段在文件中存不存在验证
                            foreach (var v in yz.Where(p => p.IsNull == 1))
                            {
                                if (!dt.Columns.Contains(v.Name))
                                {
                                    if (string.IsNullOrEmpty(str2))
                                    {
                                        str2 = "当前导入的必填字段：【" + v.Name + "】";
                                    }
                                    else
                                    {
                                        str2 = str2 + "、【" + v.Name + "】";
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(str2))
                            {
                                str2 = str2 + " 在文件中不存在!<br>";
                            }
                            #endregion

                            string str3 = string.Empty;//验证必填字段是否有值
                            string str4 = string.Empty;//验证字段是否重复
                            string str5 = string.Empty;//验证字段是否存在

                            for (int i = (sheet.FirstRowNum + int.Parse(headrow) + 1); i <= sheet.LastRowNum; i++)
                            {
                                string str31 = string.Empty;
                                string str41 = string.Empty;
                                string str42 = string.Empty;
                                string str51 = string.Empty;

                                DataRow dr = dt.NewRow();
                                bool bl = false;
                                IRow row = sheet.GetRow(i);

                                dr["status"] = "0";

                                for (int j = row.FirstCellNum; j < cellCount; j++)
                                {
                                    string strsj = row.GetCell(j) != null ? row.GetCell(j).ToString().Trim() : "";
                                    if (strsj != "")
                                    {
                                        bl = true;
                                    }

                                    foreach (var v in yz.Where(p => p.Name == headerRow.GetCell(j).ToString().Trim()))
                                    {
                                        if (strsj == "")
                                        {
                                            #region 必填字段验证
                                            if (v.IsNull == 1)
                                            {
                                                //strsj = "<span style='color:red;'>必填</span>";

                                                if (string.IsNullOrEmpty(str31))
                                                {
                                                    str31 = "第" + (i + 1) + "行的必填字段：【" + v.Name + "】";
                                                }
                                                else
                                                {
                                                    str31 = str31 + "、【" + v.Name + "】";
                                                }
                                                dr["status"] = "2";
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region 长度验证
                                            if (v.Length != 0)
                                            {
                                                if (Encoding.Default.GetBytes(strsj).Length > v.Length)
                                                {
                                                    strsj = strsj + "<span style='color:red;'>长度不能超过" + v.Length + "</span>";
                                                    dr["status"] = "2";
                                                }
                                            }
                                            #endregion
                                            #region 重复验证
                                            if (!string.IsNullOrEmpty(v.IsRepeat))
                                            {
                                                #region 与现有数据比较是否重复
                                                string[] strRS = v.IsRepeat.Split('|');

                                                var cf = new JH_Auth_UserB().GetDTByCommand("select * from " + strRS[0] + " where " + strRS[1] + "= '" + strsj + "' and ComId='" + UserInfo.QYinfo.ComId + "'");
                                                if (cf.Rows.Count > 0)
                                                {
                                                    if (string.IsNullOrEmpty(str41))
                                                    {
                                                        str41 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str41 = str41 + "、【" + v.Name + "】:" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                                #endregion
                                                #region 与Excel中数据比较是否重复
                                                DataRow[] drs = dt.Select(headerRow.GetCell(j).ToString().Trim() + "='" + strsj + "'");
                                                if (drs.Length > 0)
                                                {
                                                    if (string.IsNullOrEmpty(str42))
                                                    {
                                                        str42 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str42 = str42 + "、【" + v.Name + "】" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                                #endregion
                                            }
                                            #endregion
                                            #region 存在验证
                                            if (!string.IsNullOrEmpty(v.IsExist))
                                            {
                                                string[] strES = v.IsExist.Split('|');

                                                var cz = new JH_Auth_UserB().GetDTByCommand("select * from " + strES[0] + " where " + strES[1] + "= '" + strsj + "' and ComId='" + UserInfo.QYinfo.ComId + "'");
                                                if (cz.Rows.Count == 0)
                                                {
                                                    if (string.IsNullOrEmpty(str51))
                                                    {
                                                        str51 = "第" + (i + 1) + "行的字段：【" + v.Name + "】" + strsj;
                                                    }
                                                    else
                                                    {
                                                        str51 = str51 + "、【" + v.Name + "】" + strsj;
                                                    }
                                                    dr["status"] = "2";
                                                }
                                            }
                                            #endregion
                                        }
                                    }

                                    dr[j] = strsj;
                                }
                                if (!string.IsNullOrEmpty(str31))
                                {
                                    str31 = str31 + " 不能为空！<br>";
                                    str3 = str3 + str31;
                                }
                                if (!string.IsNullOrEmpty(str41))
                                {
                                    str41 = str41 + " 已经存在！<br>";
                                    str4 = str4 + str41;
                                }
                                if (!string.IsNullOrEmpty(str42))
                                {
                                    str42 = str42 + " 在文件中已经存在！<br>";
                                    str4 = str4 + str42;
                                }
                                if (!string.IsNullOrEmpty(str51))
                                {
                                    str51 = str51 + " 不存在！<br>";
                                    str5 = str5 + str51;
                                }

                                if (bl)
                                {
                                    dt.Rows.Add(dr);
                                }
                            }
                            if (string.IsNullOrEmpty(str2) && string.IsNullOrEmpty(str3) && string.IsNullOrEmpty(str4) && string.IsNullOrEmpty(str5))
                            {
                                msg.Result = dt;
                            }

                            msg.Result1 = str1 + str2 + str3 + str4 + str5;
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
            catch (Exception ex)
            {
                //msg.ErrorMsg = ex.ToString();
                msg.ErrorMsg = "导入失败！";
            }
        }

        #endregion

        #region 导出模板excel

        /// <summary>
        /// 导出模板excel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXPORTTOEXCEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                List<JH_Auth_ExtendDataB.IMPORTYZ> yz = new List<JH_Auth_ExtendDataB.IMPORTYZ>();
                yz = new JH_Auth_ExtendDataB().GetTable(P1, UserInfo.QYinfo.ComId);//获取字段
                if (yz.Count > 0)
                {
                    HSSFWorkbook workbook = new HSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("Sheet1");

                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;
                    HeadercellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;
                    HeadercellStyle.FillPattern = FillPattern.SolidForeground;
                    HeadercellStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);


                    //用column name 作为列名
                    int icolIndex = 0;
                    IRow headerRow = sheet.CreateRow(0);
                    foreach (var l in yz)
                    {
                        ICell cell = headerRow.CreateCell(icolIndex);
                        cell.SetCellValue(l.Name);
                        cell.CellStyle = HeadercellStyle;
                        icolIndex++;
                    }

                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    cellStyle.SetFont(cellfont);

                    string strDataJson = new JH_Auth_ExtendDataB().GetExcelData(P1);
                    if (strDataJson != "")
                    {
                        string[] strs = strDataJson.Split(',');

                        //建立内容行

                        int iCellIndex = 0;

                        IRow DataRow = sheet.CreateRow(1);
                        for (int i = 0; i < strs.Length; i++)
                        {

                            ICell cell = DataRow.CreateCell(iCellIndex);
                            cell.SetCellValue(strs[i]);
                            cell.CellStyle = cellStyle;
                            iCellIndex++;
                        }
                    }

                    //自适应列宽度
                    for (int i = 0; i < icolIndex; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        workbook.Write(ms);

                        HttpContext curContext = HttpContext.Current;

                        string strName = string.Empty;


                        // 设置编码和附件格式
                        curContext.Response.ContentType = "application/vnd.ms-excel";
                        curContext.Response.ContentEncoding = Encoding.UTF8;
                        curContext.Response.Charset = "";
                        curContext.Response.AppendHeader("Content-Disposition",
                            "attachment;filename=" + HttpUtility.UrlEncode("CRM_" + strName + "_模板文件.xls", Encoding.UTF8));

                        curContext.Response.BinaryWrite(ms.GetBuffer());
                        curContext.Response.End();

                        workbook = null;
                        ms.Close();
                        ms.Dispose();
                    }
                }

            }
            catch
            {
                msg.ErrorMsg = "导入失败！";
            }
        }

        /// <summary>
        /// 下载模板excel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DOWNLOADEXCEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strName = string.Empty;
                if (P1 == "KHGL")
                {
                    strName = "CRM_客户_导入模板.xls";
                }
                else if (P1 == "KHLXR")
                {
                    strName = "CRM_客户联系人_导入模板.xls";
                }
                else if (P1 == "HTGL")
                {
                    strName = "CRM_合同_导入模板.xls";
                }
                HttpContext curContext = HttpContext.Current;
                string headrow = context.Request["headrow"] ?? "0";//头部开始行下标
                string path = curContext.Server.MapPath(@"/ViewV5/base/" + strName);
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                string suffix = path.Substring(path.LastIndexOf(".") + 1).ToLower();

                IWorkbook workbook = null;

                if (suffix == "xlsx") // 2007版本
                {
                    workbook = new XSSFWorkbook(file);
                }
                else if (suffix == "xls") // 2003版本
                {
                    workbook = new HSSFWorkbook(file);
                }
                ISheet sheet = workbook.GetSheetAt(0);

                IRow headerRow = sheet.GetRow(int.Parse(headrow));
                IRow oneRow = sheet.GetRow(int.Parse(headrow) + 1);

                int icolIndex = headerRow.Cells.Count;

                DataTable dtExtColumn = new JH_Auth_ExtendModeB().GetExtColumnAll(UserInfo.QYinfo.ComId, P1);
                foreach (DataRow drExt in dtExtColumn.Rows)
                {
                    ICell cell = headerRow.CreateCell(icolIndex);
                    cell.SetCellValue(drExt["TableFiledName"].ToString());
                    cell.CellStyle = headerRow.Cells[icolIndex - 1].CellStyle;

                    ICell onecell = oneRow.CreateCell(icolIndex);
                    onecell.SetCellValue("");
                    onecell.CellStyle = oneRow.Cells[icolIndex - 1].CellStyle;

                    icolIndex++;
                }

                //自适应列宽度
                for (int i = 0; i < icolIndex; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                if (P1 == "KHGL")
                {
                    //表头样式
                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);

                    //单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    headerfont.FontHeightInPoints = 10;
                    cellStyle.SetFont(cellfont);

                    for (int i = 10; i < 15; i++)
                    {
                        string strZTName = string.Empty;
                        if (i == 10) { strZTName = "客户类型"; }
                        if (i == 11) { strZTName = "跟进状态"; }
                        if (i == 12) { strZTName = "客户来源"; }
                        if (i == 13) { strZTName = "所属行业"; }
                        if (i == 14) { strZTName = "人员规模"; }
                        ISheet sheet1 = workbook.CreateSheet(strZTName);
                        IRow headerRow1 = sheet1.CreateRow(0);
                        ICell cell1 = headerRow1.CreateCell(0);
                        cell1.SetCellValue(strZTName);
                        cell1.CellStyle = HeadercellStyle;

                        int rowindex1 = 1;

                        foreach (var l in new JH_Auth_ZiDianB().GetEntities(p => p.ComId == UserInfo.QYinfo.ComId && p.Class == i))
                        {
                            IRow DataRow = sheet1.CreateRow(rowindex1);
                            ICell cell = DataRow.CreateCell(0);
                            cell.SetCellValue(l.TypeName);
                            cell.CellStyle = cellStyle;
                            rowindex1++;
                        }

                        sheet1.AutoSizeColumn(0);
                    }
                }
                if (P1 == "HTGL")
                {
                    //表头样式
                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    HeadercellStyle.BorderBottom = BorderStyle.Thin;
                    HeadercellStyle.BorderLeft = BorderStyle.Thin;
                    HeadercellStyle.BorderRight = BorderStyle.Thin;
                    HeadercellStyle.BorderTop = BorderStyle.Thin;
                    HeadercellStyle.Alignment = HorizontalAlignment.Center;

                    //字体
                    NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                    headerfont.Boldweight = (short)FontBoldWeight.Bold;
                    headerfont.FontHeightInPoints = 12;
                    HeadercellStyle.SetFont(headerfont);

                    //单元格样式
                    ICellStyle cellStyle = workbook.CreateCellStyle();

                    //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                    cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                    NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                    cellfont.Boldweight = (short)FontBoldWeight.Normal;
                    headerfont.FontHeightInPoints = 10;
                    cellStyle.SetFont(cellfont);

                    for (int i = 16; i < 18; i++)
                    {
                        string strZTName = string.Empty;
                        if (i == 16) { strZTName = "合同类型"; }
                        if (i == 17) { strZTName = "付款方式"; }
                        ISheet sheet1 = workbook.CreateSheet(strZTName);
                        IRow headerRow1 = sheet1.CreateRow(0);
                        ICell cell1 = headerRow1.CreateCell(0);
                        cell1.SetCellValue(strZTName);
                        cell1.CellStyle = HeadercellStyle;

                        int rowindex1 = 1;

                        foreach (var l in new JH_Auth_ZiDianB().GetEntities(p => p.ComId == UserInfo.QYinfo.ComId && p.Class == i))
                        {
                            IRow DataRow = sheet1.CreateRow(rowindex1);
                            ICell cell = DataRow.CreateCell(0);
                            cell.SetCellValue(l.TypeName);
                            cell.CellStyle = cellStyle;
                            rowindex1++;
                        }

                        sheet1.AutoSizeColumn(0);
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.Write(ms);

                    //HttpContext curContext = HttpContext.Current;

                    // 设置编码和附件格式
                    curContext.Response.ContentType = "application/vnd.ms-excel";
                    curContext.Response.ContentEncoding = Encoding.UTF8;
                    curContext.Response.Charset = "";
                    curContext.Response.AppendHeader("Content-Disposition",
                        "attachment;filename=" + HttpUtility.UrlEncode(strName, Encoding.UTF8));

                    curContext.Response.BinaryWrite(ms.GetBuffer());
                    curContext.Response.End();

                    workbook = null;
                    ms.Close();
                    ms.Dispose();
                }
            }
            catch
            {
                msg.ErrorMsg = "下载失败！";
            }
        }

        #endregion

        #region 菜单应用

        //获取应用菜单及接口
        public void GETFUNCTION(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            string strSql = string.Format("select JH_Auth_Model.* from  JH_Auth_Model WHERE JH_Auth_Model.ComId='0' and JH_Auth_Model.PModelCode<>'' ORDER by ModelType");
            DataTable dt = new JH_Auth_ModelB().GetDTByCommand(strSql);
            dt.Columns.Add("FunData", Type.GetType("System.Object"));
            DataTable dtRoleFun = new JH_Auth_RoleFunB().GetDTByCommand(@"SELECT DISTINCT fun.*,rolefun.ActionCode RoleFun,rolefun.FunCode 
                                                                            from JH_Auth_Function fun left join JH_Auth_RoleFun rolefun on fun.ID=rolefun.FunCode and rolefun.ComId=" + UserInfo.User.ComId + "and rolefun.RoleCode=" + P1 + " Where  fun.ComId=0 or fun.ComId=" + UserInfo.User.ComId + " ORDER BY fun.FunOrder ");
            int roleId = 0;
            int.TryParse(P1, out roleId);
            JH_Auth_Role roleModel = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == roleId && d.ComId == UserInfo.User.ComId);
            string isinit = "N";//是否需要默认加载权限
            if (roleModel.isSysRole == "Y")
            {
                DataRow[] roleFun = dtRoleFun.Select(" RoleFun is not null");
                isinit = roleFun.Count() > 0 ? "N" : "Y";//>0已分配过权限，==0未分配权限
            }

            foreach (DataRow row in dt.Rows)
            {
                int modelId = int.Parse(row["ID"].ToString());
                row["FunData"] = dtRoleFun.FilterTable(" ModelID =" + modelId);
            }
            msg.Result = dt;
            msg.Result1 = isinit;
        }
        //添加角色接口权限
        public void ADDROLEFUN(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int roleCode = int.Parse(P1);
            //删除之前设置的接口权限
            new JH_Auth_RoleFunB().Delete(d => d.ComId == UserInfo.User.ComId && d.RoleCode == roleCode);
            //添加要设置的接口权限
            List<JH_Auth_RoleFun> roleFunList = JsonConvert.DeserializeObject<List<JH_Auth_RoleFun>>(P2);
            foreach (JH_Auth_RoleFun fun in roleFunList)
            {
                fun.ComId = UserInfo.User.ComId;
                new JH_Auth_RoleFunB().Insert(fun);
            }
        }
        //获取应用二级菜单
        public void GETFUNCTIONDATE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int modelId = int.Parse(P1);
            msg.Result = new JH_Auth_FunctionB().GetEntities(d => d.ModelID == modelId);
        }
        public void DELFUNDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            new JH_Auth_FunctionB().Delete(d => d.ID == Id);
        }

        //获取二级菜单详细
        public void GETFUNCTIONMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            msg.Result = new JH_Auth_FunctionB().GetEntity(d => d.ID == Id);
        }  //初始化系统菜单数据

        #endregion

        #region 用户自定义分组
        //获取自定义列表
        public void GETUSERGROUP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_UserCustomDataB().GetEntities(d => d.UserName == UserInfo.User.UserName && d.ComId == UserInfo.User.ComId && d.DataType == P1);
            if (P1 == "DXGL")
            {
                string sql = string.Format("SELECT Distinct DataContent1 FROM JH_Auth_UserCustomData WHERE dataType='DXGL' AND UserName = '{0}' AND ComId={1} GROUP BY DataContent1 ORDER BY DataContent1 DESC", UserInfo.User.UserName, UserInfo.User.ComId);
                msg.Result1 = new JH_Auth_UserCustomDataB().GetDTByCommand(sql);
            }
        }
        //添加自定义组
        public void ADDUSERGROUP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_UserCustomData customData = new JH_Auth_UserCustomData();
            customData.ComId = UserInfo.User.ComId;
            customData.CRDate = DateTime.Now;
            customData.CRUser = UserInfo.User.UserName;
            customData.DataContent = P1;
            customData.DataContent1 = P2.Trim();
            customData.DataType = context.Request["DataType"];
            customData.UserName = UserInfo.User.UserName;
            new JH_Auth_UserCustomDataB().Insert(customData);
            msg.Result = customData;





        }
        //根据组id获取用户列表
        public void GETUSERLISTBYGROUP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            JH_Auth_UserCustomData customData = new JH_Auth_UserCustomDataB().GetEntity(d => d.ID == Id);
            string[] usernames = customData.DataContent1.Split(',');
            msg.Result = new JH_Auth_UserB().GetEntities(d => usernames.Contains(d.UserName) && d.ComId == UserInfo.User.ComId);

        }
        //删除用户自定义分组 ，短信模板
        public void DELUSERGROUP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            new JH_Auth_UserCustomDataB().Delete(d => d.ID == Id);
        }
        //删除通讯录分类分组
        public void DELUSERGROUPTXL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            DataTable dt = new JH_Auth_UserCustomDataB().GetDTByCommand("SELECT * from SZHL_TXL where TagName=" + Id + " and ComId=" + UserInfo.User.ComId);
            if (dt.Rows.Count == 0)
            {
                new JH_Auth_UserCustomDataB().Delete(d => d.ID == Id);
            }
            else
            {
                msg.ErrorMsg = "请先删除此分类下的人员信息";
            }
        }


        #endregion

        #region 系统日志
        public void GETXTRZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "1=1";
            string strContent = context.Request["Content"] ?? "";
            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                strWhere += string.Format(" and (LogContent like  '%{0}%' or Remark1 like  '%{0}%' or IP like  '%{0}%')", strContent);
            }
            if (P1 != "")
            {
                //strWhere += string.Format(" and LogType IN ('{0}')", P2.Replace(",", "','"));  //多个类型逗号隔开传过来
                switch (P1)
                {
                    case "1": strWhere += " and ComId !='0'"; break;
                    case "2": strWhere += " and ComId ='0'"; break;
                }
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "0", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_LogB().GetDataPager("JH_Auth_Log", "* ", pagecount, page, " CRDate desc ", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;

        }
        public void DELXTRZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = string.Format("( ComId={0} or ComId=0 )", UserInfo.User.ComId);
            if (P1 != "" && P2 != "")
            {
                switch (P1)
                {
                    case "1": strWhere += " and ID ='" + P2 + "'"; break;
                    case "2": strWhere += " and ID in(" + P2 + ")"; break;
                }
                try
                {
                    new JH_Auth_LogB().ExsSql(" delete JH_Auth_Log where " + strWhere);
                }
                catch (Exception ex)
                {
                    msg.ErrorMsg = ex.Message;
                }
            }
            else
            {
                msg.ErrorMsg = "删除失败";
            }
        }
        #endregion





        #region  获取评论
        public void GETCOMENT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new JH_Auth_TLB().GetTL(P1, P2);
        }
        #endregion

        #region  删除评论
        public void DELCOMENT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tl = new JH_Auth_TLB().GetEntities(" ID ='" + P1 + "'").FirstOrDefault();
            if (tl != null)
            {
                new JH_Auth_TLB().Delete(tl);
            }
        }
        #endregion

        #region 添加评论
        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="strParamData"></param>
        /// <param name="strUserName"></param>
        public void ADDCOMENT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strMsgType = context.Request["MsgType"] ?? "";
            string strMsgLYID = context.Request["MsgLYID"] ?? "";
            string strPoints = context.Request["Points"] ?? "0";
            string strfjID = context.Request["fjID"] ?? "";
            string strTLID = context.Request["TLID"] ?? "";


            if (!string.IsNullOrEmpty(P1) && APIHelp.TestWB(P1) != "0")
            {
                msg.ErrorMsg = "您得发言涉及违规内容,请完善后再发";
                return;
            }


            JH_Auth_TL Model = new JH_Auth_TL();
            Model.CRDate = DateTime.Now;
            Model.CRUser = UserInfo.User.UserName;
            Model.CRUserName = UserInfo.User.UserRealName;
            Model.MSGContent = P1;
            Model.ComId = UserInfo.User.ComId;
            Model.MSGTLYID = strMsgLYID;
            Model.MSGType = strMsgType;
            Model.MSGisHasFiles = strfjID;
            Model.Remark1 = P1;

            if (strTLID != "")
            {
                int TLID = Int32.Parse(strTLID);
                var tl = new JH_Auth_TLB().GetEntity(p => p.ID == TLID);
                if (tl != null)
                {
                    Model.TLID = TLID;
                    Model.ReUser = tl.CRUserName;
                }

            }


            int record = 0;
            int.TryParse(strPoints, out record);
            Model.Points = record;
            new JH_Auth_TLB().Insert(Model);
            if (Model.MSGType == "GZBG" || Model.MSGType == "RWGL" || Model.MSGType == "TSSQ")
            {
                int modelId = int.Parse(Model.MSGTLYID);
                string CRUser = "";
                string Content = UserInfo.User.UserRealName + "评论了您的";


                if (CRUser != UserInfo.User.UserName)
                {
                    SZHL_TXSX CSTX = new SZHL_TXSX();
                    CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CSTX.APIName = "XTGL";
                    CSTX.ComId = UserInfo.User.ComId;
                    CSTX.FunName = "SENDPLMSG";
                    CSTX.CRUserRealName = UserInfo.User.UserRealName;
                    CSTX.MsgID = modelId.ToString();

                    CSTX.TXContent = Content;
                    CSTX.ISCS = "N";
                    CSTX.TXUser = CRUser;
                    CSTX.TXMode = Model.MSGType;
                    CSTX.CRUser = UserInfo.User.UserName;

                    TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
                }
            }

            msg.Result = Model;
            if (Model.MSGisHasFiles == "")
                Model.MSGisHasFiles = "0";
            msg.Result1 = new FT_FileB().GetEntities(" ID in (" + Model.MSGisHasFiles + ")");
        }
        public void SENDPLMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
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
        /// 获取评论列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETPLLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " JH_Auth_TL.ComId=" + UserInfo.User.ComId;
            if (P1 != "")
            {
                strWhere += string.Format(" And  JH_Auth_TL.MSGContent like '%{0}%'", P1);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int total = 0;
            DataTable dt = new JH_Auth_TLB().GetDataPager(" JH_Auth_TL LEFT JOIN  SZHL_TSSQ ON  JH_Auth_TL.MSGTLYID=SZHL_TSSQ.ID ", " JH_Auth_TL.*,SZHL_TSSQ.HTNR ", pagecount, page, " JH_Auth_TL.CRDate desc", strWhere, ref total);
            msg.Result = dt;
            msg.Result1 = total;
        }

        #endregion


        public void UPLOADFILE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int fid = 3;
                if (!string.IsNullOrEmpty(P1))
                {
                    fid = Int32.Parse(P1);
                }

                List<FT_File> ls = new List<FT_File>();
                for (int i = 0; i < context.Request.Files.Count; i++)
                {
                    HttpPostedFile uploadFile = context.Request.Files[i];


                    string originalName = uploadFile.FileName;



                    string[] temp = uploadFile.FileName.Split('.');

                    //保存图片

                    string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                    string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);

                    //MP4上传阿里云
                    if (Path.GetExtension(originalName).ToLower() == ".mp4")
                    {
                        md5 = md5.Replace("\"", "").Split(',')[0];
                        AliyunHelp.UploadToOSS(md5, "mp4", uploadFile.InputStream);
                    }

                    FT_File newfile = new FT_File();
                    newfile.ComId = UserInfo.User.ComId;
                    newfile.Name = uploadFile.FileName.Substring(0, uploadFile.FileName.LastIndexOf('.'));
                    newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                    newfile.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                    newfile.FileSize = uploadFile.InputStream.Length.ToString();
                    newfile.FileVersin = 0;
                    newfile.CRDate = DateTime.Now;
                    newfile.CRUser = UserInfo.User.UserName;
                    newfile.UPDDate = DateTime.Now;
                    newfile.UPUser = UserInfo.User.UserName;
                    newfile.FileExtendName = temp[temp.Length - 1].ToLower();
                    newfile.FolderID = fid;
                    if (new List<string>() { "txt", "html", "doc", "mp4", "flv", "ogg", "jpg", "gif", "png", "bmp", "jpeg" }.Contains(newfile.FileExtendName.ToLower()))
                    {
                        newfile.ISYL = "Y";
                    }
                    if (new List<string>() { "pdf", "doc", "docx", "ppt", "pptx", "xls", "xlsx" }.Contains(newfile.FileExtendName.ToLower()))
                    {
                        newfile.ISYL = "Y";
                        //newfile.YLUrl = UserInfo.QYinfo.FileServerUrl + "/document/YL/" + newfile.zyid;
                        newfile.YLUrl = "https://www.txywpx.com/ViewV5/Base/doc.html?zyid=" + newfile.zyid;
                    }

                    if (P2 != "")
                    {
                        newfile.Name = P2.Substring(0, P2.LastIndexOf('.')); //文件名
                    }

                    new FT_FileB().Insert(newfile);

                    int filesize = 0;
                    int.TryParse(newfile.FileSize, out filesize);
                    new FT_FileB().AddSpace(UserInfo.User.ComId.Value, filesize);
                    //msg.Result = newfile;
                    ls.Add(newfile);
                }
                msg.Result = ls;
            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPLOADFILEV1(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int fid = 3;
                if (!string.IsNullOrEmpty(P1))
                {
                    fid = Int32.Parse(P1);
                }
                CommonHelp.WriteLOG("文件数量" + context.Request.Files.Count);

                List<FT_File> ls = new List<FT_File>();
                for (int i = 0; i < context.Request.Files.Count; i++)
                {
                    HttpPostedFile uploadFile = context.Request.Files[i];
                    string originalName = uploadFile.FileName;
                    string[] temp = uploadFile.FileName.Split('.');

                    //保存图片

                    string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                    string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);

                    string json = "[{filename:'" + uploadFile.FileName + "',md5:'" + md5.Split(',')[0] + "',zyid:'" + md5.Split(',')[1] + "',filesize:'" + uploadFile.InputStream.Length.ToString() + "'}]";
                    QYWDManage qywd = new QYWDManage();

                    //MP4上传阿里云
                    //if (Path.GetExtension(originalName).ToLower() == ".mp4")
                    //{
                    //    md5 = md5.Replace("\"", "").Split(',')[0];
                    //    AliyunHelp.UploadToOSS(md5, "mp4", uploadFile.InputStream);
                    //}

                    CommonHelp.WriteLOG("调用参数" + fid.ToString());
                    qywd.ADDFILE(context, msg, json, fid.ToString(), UserInfo);

                }
            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }

        /// <summary>
        /// 上传文件（文档中心）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void UPLOADFILES(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                HttpPostedFile uploadFile = context.Request.Files["upFile"];
                string originalName = uploadFile.FileName;
                string[] temp = uploadFile.FileName.Split('.');

                //保存图片
                string filename = System.Guid.NewGuid() + "." + temp[temp.Length - 1].ToLower();
                string md5 = new CommonHelp().SaveFile(UserInfo.QYinfo, filename, uploadFile);
                string json = "[{filename:'" + uploadFile.FileName + "',md5:" + md5 + ",filesize:'" + uploadFile.InputStream.Length.ToString() + "'}]";

                QYWDManage qywd = new QYWDManage();
                qywd.ADDFILE(context, msg, json, P1, UserInfo);

            }
            catch (Exception e)
            {
                msg.ErrorMsg = "上传图片";
            }
        }



        //设置手机APP首页显示应用
        public void SETAPPINDEX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string type = context.Request["type"] ?? "APPINDEX";//默认为APP首页显示菜单，传值为PC首页的快捷方式按钮
            //判断是否存在菜单的数据，存在只更新状态，不存在添加
            JH_Auth_UserCustomData customData = new JH_Auth_UserCustomDataB().GetEntity(d => d.UserName == UserInfo.User.UserName && d.DataType == type && d.ComId == UserInfo.User.ComId && d.DataContent == P1);
            string status = context.Request["Status"];
            string modelName = context.Request["name"] ?? "";
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
                customData.DataContent = P1;
                customData.Remark = modelName;
                customData.DataContent1 = "Y";
                customData.DataType = type;
                new JH_Auth_UserCustomDataB().Insert(customData);
            }
            msg.Result = customData;

        }
        #region 设置部门人员的查看权限
        public void SETBRANCHQX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int deptCode = int.Parse(P2);
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetEntity(d => d.ComId == UserInfo.User.ComId && d.DeptCode == deptCode);
            branch.TXLQX = P1;
            branch.IsHasQX = context.Request["qx"] ?? "N";
            new JH_Auth_BranchB().Update(branch);

        }
        #endregion
        #region 设置部门人员的查看角色
        public void SETROLEQX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            int roleCode = int.Parse(P2);
            JH_Auth_Role role = new JH_Auth_RoleB().GetEntity(d => d.RoleCode == roleCode);
            role.RoleQX = P1;
            role.IsHasQX = context.Request["qx"] ?? "N";
            new JH_Auth_RoleB().Update(role);
            msg.Result = role;
        }
        #endregion



        #region 获取已发送短信数及容量使用情况
        public void GETDXANDSPACE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            decimal DXCost = decimal.Parse(CommonHelp.GetConfig("DXCost"));
            //已发送短信总数量
            msg.Result = new SZHL_DXGLB().GetEntities(d => d.ComId == UserInfo.User.ComId.Value).Count();
            msg.Result1 = (int)(UserInfo.QYinfo.AccountMoney.Value / DXCost);
            msg.Result2 = UserInfo.QYinfo.QySpace / 10000000000;
            string strSql = string.Format("SELECT isnull(sum(CAST( FileSize  as DECIMAL(18,2))),0) from  FT_File where ComId=" + UserInfo.User.ComId);
            object obj = new FT_FileB().ExsSclarSql(strSql);
            decimal Size = 0;
            string fileSize = obj.ToString();
            if (fileSize.Length < 4)
            {
                Size = decimal.Parse(fileSize);
                msg.Result4 = "kb";
            }
            if (fileSize.Length >= 4 && fileSize.Length <= 8)
            {
                Size = Math.Round(decimal.Parse(fileSize) / 10000, 2);
                msg.Result4 = "M";
            }
            if (fileSize.Length > 8)
            {
                Size = Math.Round(decimal.Parse(fileSize) / 100000000, 2);
                msg.Result4 = "G";
            }
            msg.Result3 = Size;
        }
        #endregion

        #region  扩展字段
        public void GETEXTENDFIELD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = "";
            string pdid = context.Request["PDID"] ?? "0";
            DataTable dt = new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select * from JH_Auth_ExtendMode where ComId='{0}' and TableName='{1}' " + strWhere, UserInfo.User.ComId, P1));
            msg.Result = dt;
        }
        public void ADDEXTENDFIELD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_ExtendMode extMode = JsonConvert.DeserializeObject<JH_Auth_ExtendMode>(P1);
            if (extMode.ID == 0) //add
            {
                extMode.ComId = UserInfo.User.ComId;
                extMode.CRUser = UserInfo.User.UserName;
                extMode.CRDate = DateTime.Now;
                new JH_Auth_ExtendModeB().Insert(extMode);
            }
            else //edit
            {
                new JH_Auth_ExtendModeB().Update(extMode);
            }

            msg.Result = extMode;

        }
        public void DELEXTENDFIELD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            new JH_Auth_ExtendModeB().Delete(p => p.ID.ToString() == P1);
        }
        public void GETEXTDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 == "YGGL")
            {
                JH_Auth_User user = new JH_Auth_UserB().GetEntity(d => d.UserName == P2 && d.ComId == UserInfo.User.ComId);
                if (user != null)
                {
                    P2 = user.ID.ToString();
                }
                else
                {
                    P2 = "";
                }
            }
            string strWhere = "";
            string pdid = context.Request["PDID"] ?? "0";
            DataTable dt = new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select j.ComId, j.ID, j.TableName, j.TableFiledColumn, j.TableFiledName, j.TableFileType, j.DefaultOption, j.DefaultValue, j.IsRequire, d.ExtendModeID, d.ID AS ExtID, d.DataID, d.ExtendDataValue from [dbo].[JH_Auth_ExtendMode] j left join JH_Auth_ExtendData d on j.ComId=d.ComId and j.ID=d.ExtendModeID and d.DataID='{2}' where j.ComId='{0}' and j.TableName='{1}' " + strWhere, UserInfo.User.ComId, P1, P2));

            msg.Result = dt;
        }
        public void UPDATEEXTDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int dataid = Int32.Parse(P2);
            string ExtData = context.Request.Params["ExtData"];
            if (!string.IsNullOrEmpty(ExtData))
            {
                List<ExtDataModel> ext = JsonConvert.DeserializeObject<List<ExtDataModel>>(ExtData);
                if (ext.Count > 0)
                {
                    foreach (var v in ext)
                    {
                        var extModel = new JH_Auth_ExtendDataB().GetEntity(p => p.ID == v.ExtID);
                        if (extModel == null)
                        {
                            JH_Auth_ExtendData jext = new JH_Auth_ExtendData();
                            jext.ComId = v.ComId;
                            jext.DataID = jext.DataID == null ? dataid : jext.DataID;
                            jext.TableName = v.TableName;
                            jext.ExtendModeID = v.ID;
                            jext.ExtendDataValue = v.ExtendDataValue;
                            jext.CRUser = UserInfo.User.UserName;
                            jext.CRDate = DateTime.Now;
                            jext.CRUserName = UserInfo.User.UserRealName;
                            jext.BranchNo = UserInfo.BranchInfo.DeptCode;
                            jext.BranchName = UserInfo.BranchInfo.DeptName;
                            new JH_Auth_ExtendDataB().Insert(jext);
                        }
                        else
                        {
                            extModel.ExtendDataValue = v.ExtendDataValue;
                            new JH_Auth_ExtendDataB().Update(extModel);
                        }
                    }
                }

            }

        }


        #endregion

        #region 获取注册用户
        /// <summary>
        /// 获取注册用户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETQYUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWhere = " 1=1 ";
            if (P1 != "")
            {
                strWhere += string.Format(" And ( QYName like '%{0}%' )", P1);
            }
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int recordCount = 0;
            DataTable dt = new JH_Auth_QYB().GetDataPager(" JH_Auth_QY ", " * ", pagecount, page, " CRDate desc ", strWhere, ref recordCount);
            msg.Result = dt;
            msg.Result2 = UserInfo.User.ID;
            //msg.Result1 = Math.Ceiling(recordCount * 1.0 / 10);
            msg.Result1 = recordCount;
        }
        #endregion







        #region 应用管理v5
        //应用管理
        public void GETAPPLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string sql = string.Format(@"SELECT * FROM JH_Auth_Model WHERE (ComId=0 or ComId={0} ) AND ISNULL(PModelCode,'') != 'RLZY' AND ISNULL(PModelCode,'') !='CRM' ", UserInfo.User.ComId);
            DataTable dt = new JH_Auth_ModelB().GetDTByCommand(sql);

            msg.Result = dt;
        }
        #endregion

        #region 草稿管理
        //获取草稿
        public void GETDRAFT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string sql = string.Format(@"SELECT top 5 * FROM SZHL_DRAFT WHERE ComId={0} and CRUser='{2}' and FormCode='{1}'and DATAID IS NULL ", UserInfo.User.ComId, P1, UserInfo.User.UserName);
            if (P2 != "")
            {
                sql += " and FormID='" + P2 + "'";
            }
            sql += " order by CRTime desc ";
            DataTable dt = new SZHL_DRAFTB().GetDTByCommand(sql);

            msg.Result = dt;
        }
        public void GETDRAFTMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new SZHL_DRAFTB().GetEntity(p => p.ID == ID && p.ComId == UserInfo.User.ComId && p.CRUser == UserInfo.User.CRUser);
        }

        public void SAVEDRAFT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_DRAFT tt = JsonConvert.DeserializeObject<SZHL_DRAFT>(P1);
            tt.ID = 0;
            if (tt.ID == 0)
            {
                tt.ComId = UserInfo.User.ComId;
                tt.CRUser = UserInfo.User.UserName;
                tt.CRTime = DateTime.Now;
                new SZHL_DRAFTB().Insert(tt);
            }

            msg.Result = tt;
        }
        public void DELDRAFT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = Int32.Parse(P1);
            new SZHL_DRAFTB().Delete(p => p.ID == ID && p.ComId == UserInfo.User.ComId && p.CRUser == UserInfo.User.UserName);
        }
        #endregion
    }

    public class WXUserBR
    {
        public int DeptCode { get; set; }
        public string DeptName { get; set; }
        public dynamic DeptUser { get; set; }
        public int DeptUserNum { get; set; }
        public List<WXUserBR> SubDept { get; set; }
    }

    public class ExtDataModel
    {
        public int ComId { get; set; }
        public int ID { get; set; }
        public string TableName { get; set; }
        public string TableFiledColumn { get; set; }
        public string TableFiledName { get; set; }
        public string TableFileType { get; set; }
        public string DefaultOption { get; set; }
        public string DefaultValue { get; set; }
        public string IsRequire { get; set; }
        public int? ExtendModeID { get; set; }
        public int? ExtID { get; set; }
        public int? DataID { get; set; }
        public string ExtendDataValue { get; set; }
    }

}