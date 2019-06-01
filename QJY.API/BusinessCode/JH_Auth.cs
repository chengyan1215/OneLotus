using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace QJY.API
{


    #region 系统模块
    //用户表
    public class JH_Auth_UserB : BaseEFDao<JH_Auth_User>
    {



        public class UserInfo
        {
            public JH_Auth_User User;
            public JH_Auth_QY QYinfo;
            public JH_Auth_Branch BranchInfo;
            public string UserRoleCode;
            public string UserBMQXCode;
            public string UserDWName;

        }
        public UserInfo GetUserInfo(string strSZHLCode)
        {

            UserInfo UserInfo = new UserInfo();
            UserInfo.User = new JH_Auth_UserB().GetUserByPCCode(strSZHLCode);
            if (UserInfo.User != null)
            {
                UserInfo.UserRoleCode = new JH_Auth_UserRoleB().GetRoleCodeByUserName(UserInfo.User.UserName, UserInfo.User.ComId.Value);
                UserInfo.QYinfo = new JH_Auth_QYB().GetEntity(d => d.ComId == UserInfo.User.ComId.Value);
                UserInfo.BranchInfo = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, UserInfo.User.BranchCode);
                UserInfo.UserBMQXCode = GetQXBMByUserRole(UserInfo.QYinfo.ComId, UserInfo.UserRoleCode);
                if (UserInfo.UserBMQXCode == "")
                {
                    //如果没有设置部门权限,就默认管理二级权限
                    UserInfo.UserBMQXCode = new JH_Auth_BranchB().GetEntities(d => d.Remark1.Contains(UserInfo.User.remark)).Select(d => d.DeptCode).ToList().ListTOString(',');
                }
                UserInfo.UserDWName = new JH_Auth_UserB().GetUserDWName(UserInfo.User.ComId.Value, UserInfo.User.UserName);


            }

            return UserInfo;
        }

        public UserInfo GetUserInfoByWxopenid(string wxopenid)
        {

            UserInfo UserInfo = new UserInfo();
            UserInfo.User = new JH_Auth_UserB().GetUserByWxopenid(wxopenid);
            if (UserInfo.User != null)
            {
                UserInfo.UserRoleCode = new JH_Auth_UserRoleB().GetRoleCodeByUserName(UserInfo.User.UserName, UserInfo.User.ComId.Value);
                UserInfo.QYinfo = new JH_Auth_QYB().GetEntity(d => d.ComId == UserInfo.User.ComId.Value);
                UserInfo.BranchInfo = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, UserInfo.User.BranchCode);
                UserInfo.UserBMQXCode = GetQXBMByUserRole(UserInfo.QYinfo.ComId, UserInfo.UserRoleCode);
                if (UserInfo.UserBMQXCode == "")
                {
                    //如果没有设置部门权限,就默认管理二级权限
                    UserInfo.UserBMQXCode = new JH_Auth_BranchB().GetEntities(d => d.Remark1.Contains(UserInfo.User.remark)).Select(d => d.DeptCode).ToList().ListTOString(',');
                }
                UserInfo.UserDWName = new JH_Auth_UserB().GetUserDWName(UserInfo.User.ComId.Value, UserInfo.User.UserName);


            }

            return UserInfo;
        }


        public UserInfo GetUserInfo(int intComid, string strUserName)
        {
            UserInfo UserInfo = new UserInfo();
            JH_Auth_User User = new JH_Auth_UserB().GetUserByUserName(intComid, strUserName);
            UserInfo.User = User;
            UserInfo.UserRoleCode = new JH_Auth_UserRoleB().GetRoleCodeByUserName(UserInfo.User.UserName, UserInfo.User.ComId.Value);
            UserInfo.QYinfo = new JH_Auth_QYB().GetEntity(d => d.ComId == UserInfo.User.ComId.Value);
            UserInfo.BranchInfo = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.QYinfo.ComId, UserInfo.User.BranchCode);
            UserInfo.UserBMQXCode = GetQXBMByUserRole(UserInfo.QYinfo.ComId, UserInfo.UserRoleCode);
            if (UserInfo.UserBMQXCode == "")
            {
                UserInfo.UserBMQXCode = new JH_Auth_BranchB().GetEntities(d => d.Remark1.Contains(UserInfo.User.remark)).Select(d => d.DeptCode).ToList().ListTOString(',');
            }
            UserInfo.UserDWName = new JH_Auth_UserB().GetUserDWName(UserInfo.User.ComId.Value, UserInfo.User.UserName);


            return UserInfo;
        }

        public JH_Auth_User GetUserByUserName(int ComID, string UserName)
        {
            JH_Auth_User branchmodel = new JH_Auth_User();
            branchmodel = new JH_Auth_UserB().GetEntity(d => d.ComId == ComID && d.UserName == UserName);
            return branchmodel;
        }
        public string GetUserByUserRealName(int ComID, string RealName)
        {
            string strUserName = "";
            JH_Auth_User user = new JH_Auth_UserB().GetEntities(d => d.ComId == ComID && d.UserRealName == RealName).FirstOrDefault();
            if (user != null)
            {
                strUserName = user.UserName;
            }
            return strUserName;
        }
        public JH_Auth_User GetUserByPCCode(string PCCode)
        {
            JH_Auth_User branchmodel = new JH_Auth_User();
            branchmodel = new JH_Auth_UserB().GetEntity(d => d.pccode == PCCode);
            return branchmodel;
        }



        /// <summary>
        /// 根据微信openid获取用户信息
        /// </summary>
        /// <param name="PCCode"></param>
        /// <returns></returns>
        public JH_Auth_User GetUserByWxopenid(string WXopenid)
        {
            JH_Auth_User USER = new JH_Auth_User();
            USER = new JH_Auth_UserB().GetEntity(d => d.weixinCard == WXopenid);
            return USER;
        }

        /// <summary>
        /// 根据角色获取当前角色拥有的部门权限
        /// </summary>
        /// <param name="ComID"></param>
        /// <param name="UserRoleCode"></param>
        /// <returns></returns>
        public string GetQXBMByUserRole(int ComID, string UserRoleCode)
        {
            DataTable dt = new JH_Auth_RoleB().GetDTByCommand("SELECT RoleQX FROM JH_Auth_Role WHERE ROleCode in (" + UserRoleCode + ")");
            List<string> ListQXBM = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string strBMCodes = dt.Rows[i]["RoleQX"].ToString();
                foreach (string bmcode in strBMCodes.Split(','))
                {
                    if (!ListQXBM.Contains(bmcode))
                    {
                        ListQXBM.Add(bmcode);
                    }
                }
            }

            return ListQXBM.ListTOString(',');
        }

        public string GetUserRealName(int intComid, string strUserName)
        {
            JH_Auth_User User = new JH_Auth_UserB().GetUserByUserName(intComid, strUserName);
            if (User == null)
            {
                return strUserName;
            }
            else
            {
                return User.UserRealName;
            }
        }

        /// <summary>
        /// 获取用户单位名称
        /// </summary>
        /// <param name="ComId"></param>
        /// <param name="strUser"></param>
        /// <returns></returns>
        public string GetUserDWName(int ComId, string strUser)
        {

            try
            {
                string strSql = string.Format("SELECT DeptName FROM JH_Auth_User  INNER JOIN JH_Auth_Branch ON  JH_Auth_User.remark=JH_Auth_Branch.DeptCode WHERE JH_Auth_User.ComId={0} and UserName='{1}'", ComId, strUser);
                return new JH_Auth_UserB().ExsSclarSql(strSql).ToString();
            }
            catch (Exception)
            {
                return "无此单位";
            }


        }



        /// <summary>
        /// 获取数据查询条件
        /// </summary>
        /// <param name="ComId"></param>
        /// <param name="strDW"></param>
        /// <returns></returns>
        public string GetDWUserSWhere(JH_Auth_UserB.UserInfo UserInfo, string strUserFiled = "CRUser", string isAllData = "")
        {
            string strWhere = "";
            if (!UserInfo.UserRoleCode.Contains("1218") && isAllData == "")//如果不是超级管理员
            {
                string strUsers = "";
                DataTable dt = new JH_Auth_UserB().GetDTByCommand("SELECT UserName FROM  JH_Auth_User WHERE BranchCode IN ('" + UserInfo.UserBMQXCode.Trim(',').ToFormatLike() + "')");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    strUsers = strUsers + dt.Rows[i][0].ToString() + ",";
                }
                strUsers = strUsers.TrimEnd(',');
                strWhere = " AND " + strUserFiled + " IN ('" + strUsers.ToFormatLike() + "')";
            }
            return strWhere;
        }



        public void UpdateloginDate(int ComId, string strUser)
        {
            string strSql = string.Format("UPDATE JH_Auth_User SET logindate='{0}' WHERE ComId={1} and UserName = '{2}'", DateTime.Now.ToString("yyyy-MM-dd HH:ss"), ComId, strUser.ToFormatLike());
            new JH_Auth_UserB().ExsSql(strSql);
        }





        public void UpdatePassWord(int ComId, string strUser, string strNewPassWord)
        {
            string strSql = string.Format("UPDATE JH_Auth_User SET UserPass='{0}' WHERE ComId={1} and UserName in ('{2}')", strNewPassWord, ComId, strUser.ToFormatLike());
            new JH_Auth_UserB().ExsSql(strSql);
        }


        /// <summary>
        /// 根据部门获取用户列表
        /// </summary>
        /// <param name="branchCode">部门编号</param>
        /// <param name="strFilter">姓名，部门，手机号</param>
        /// <returns></returns>
        public DataTable GetUserListbyBranch(int branchCode, string strFilter, int ComId)
        {
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(ComId, branchCode);
            string strSQL = "select u.*,b.DeptName,b.DeptCode from  JH_Auth_User u  inner join JH_Auth_Branch b on u.branchCode=b.DeptCode where 1=1 ";
            strSQL += string.Format(" And  b.Remark1 like '%{0}%'", branch.Remark1);

            if (strFilter != "")
            {
                strSQL += string.Format(" And (u.UserName like '%{0}%'  or u.UserRealName like '%{0}%'  or b.DeptName like '%{0}%' or u.mobphone like '%{0}%')", strFilter);
            }
            DataTable dt = new JH_Auth_UserB().GetDTByCommand(strSQL + " ORDER by b.DeptCode,u.UserOrder asc");
            return dt;
        }




        /// <summary>
        /// 根据部门获取可用用户列表，包含下级部门
        /// </summary>
        /// <param name="branchCode">部门编号</param>
        /// <param name="strFilter">姓名，部门，手机号</param>
        /// <param name="comId">公司ID</param>
        /// <returns></returns>
        public DataTable GetUserListbyBranchUse(int branchCode, string strFilter, JH_Auth_UserB.UserInfo UserInfo)
        {
            JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(UserInfo.User.ComId.Value, branchCode);
            string strQXWhere = string.Format(" And  b.Remark1 like '%{0}%'", branch.Remark1);

            string strSQL = "select u.*,b.DeptName,b.DeptCode from  JH_Auth_User u  inner join JH_Auth_Branch b on u.branchCode=b.DeptCode where u.IsUse='Y' and u.ComId=" + UserInfo.User.ComId;
            strSQL += string.Format(" {0} ", strQXWhere);

            if (strFilter != "")
            {
                strSQL += string.Format(" And (u.UserName like '%{0}%'  or u.UserRealName like '%{0}%'  or b.DeptName like '%{0}%' or u.mobphone like '%{0}%')", strFilter);
            }
            DataTable dt = new JH_Auth_UserB().GetDTByCommand(strSQL + " order by b.DeptShort,ISNULL(u.UserOrder, 1000000) asc");
            return dt;
        }


        /// <summary>
        /// 找到用户的直属上级,先找用户表leader,再找部门leader
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public string GetUserLeader(int ComId, string strUserName)
        {
            string strLeader = "";
            UserInfo UserInfo = this.GetUserInfo(ComId, strUserName);
            if (!string.IsNullOrEmpty(UserInfo.User.UserLeader))
            {
                strLeader = UserInfo.User.UserLeader;
            }
            else
            {
                strLeader = UserInfo.BranchInfo.BranchLeader;
            }

            return strLeader;
        }


    }

    public class JH_Auth_User_CenterB : BaseEFDao<JH_Auth_User_Center>
    {
        /// <summary>
        /// 添加消息并发送微信消息
        /// </summary>
        /// <param name="UserInfo">用户信息</param>
        /// <param name="type">类型</param>
        /// <param name="title">标题</param>
        /// <param name="content">发送内容</param>
        /// <param name="Id">实体Id</param>
        /// <param name="JSR">接收人</param>
        public void SendMsg(JH_Auth_UserB.UserInfo UserInfo, string ModelCode, string content, string Id, string JSR, string type = "A", int PIID = 0, string IsCS = "N")
        {

            JH_Auth_Model Model = new JH_Auth_ModelB().GetModeByCode(ModelCode);
            JH_Auth_User_Center userCenter = new JH_Auth_User_Center();
            userCenter.ComId = UserInfo.QYinfo.ComId;
            userCenter.CRUser = UserInfo.User.UserName;
            userCenter.CRDate = DateTime.Now;
            userCenter.MsgContent = content;
            userCenter.MsgType = Model == null ? "" : Model.ModelName;
            userCenter.UserFrom = UserInfo.User.UserName;
            userCenter.isRead = 0;
            userCenter.DataId = Id;
            userCenter.MsgModeID = ModelCode;
            userCenter.MsgLink = GetMsgLink(ModelCode, type, Id.ToString(), PIID, UserInfo.User.ComId.Value);
            userCenter.wxLink = GetWXMsgLink(ModelCode, type, Id.ToString(), UserInfo.QYinfo);
            userCenter.isCS = IsCS;
            userCenter.Remark = PIID.ToString();
            string sendUser = "";
            List<string> jsrs = JSR.Split(',').Distinct().ToList();//去重接收人
            foreach (string people in jsrs)
            {
                if (!string.IsNullOrEmpty(people))
                {
                    userCenter.UserTO = people;
                    new JH_Auth_User_CenterB().Insert(userCenter);
                    sendUser += people + ",";
                }

            }

        }
        public string GetMsgLink(string modelCode, string type, string Id, int PIID, int ComId)
        {
            string url = "";
            JH_Auth_Common commonUrl = new JH_Auth_CommonB().GetEntity(d => d.ModelCode == modelCode && d.MenuCode == type);
            string flag = "?";
            if (commonUrl.Url2.IndexOf("?") > -1)
            {
                flag = "&";
            }
            if (commonUrl != null && !string.IsNullOrEmpty(commonUrl.Url2))
            {
                if (commonUrl.Url2.IndexOf("APP_ADD_WF") > -1)
                {
                    url = commonUrl.Url2 + flag + "FormCode=" + modelCode.ToUpper() + "&pageType=view&ID=" + Id;
                }
                else
                {
                    url = commonUrl.Url2 + flag + "ID=" + Id;
                }


            }
            return url;
        }
        public string GetWXMsgLink(string modelCode, string type, string Id, JH_Auth_QY Qyinfo)
        {
            //提醒事项的消息没有链接
            if (modelCode != "TXSX")
            {
                string url = "/View_Mobile/UI/UI_COMMON.html?funcode=" + modelCode + "_" + type + "_" + Id + "&corpId=" + Qyinfo.corpId;
                return url;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 阅读消息
        /// </summary>
        /// <param name="UserInfo"></param>
        /// <param name="DataID"></param>
        /// <param name="ModelCode"></param>
        public void ReadMsg(JH_Auth_UserB.UserInfo UserInfo, int DataID, string ModelCode)
        {
            Task<string> TaskCover = Task.Factory.StartNew<string>(() =>
            {
                string strSql = string.Format("UPDATE JH_Auth_User_Center SET isRead='1',ReadDate=GETDATE() WHERE DataId='{0}'AND ComId='{1}' AND UserTO='{2}' AND  MsgModeID='{3}'", DataID, UserInfo.User.ComId.Value, UserInfo.User.UserName, ModelCode);
                object obj = new JH_Auth_User_CenterB().ExsSclarSql(strSql);
                return "";
            });
        }
    }


    //部门表
    public class JH_Auth_BranchB : BaseEFDao<JH_Auth_Branch>
    {

        public void AddBranch(JH_Auth_UserB.UserInfo UserInfo, JH_Auth_Branch branch, Msg_Result msg)
        {

            if (branch.DeptCode == 0)//DeptCode==0为添加部门
            {
                //获取要添加的部门名称是否存在，存在提示用户，不存在添加
                JH_Auth_Branch branch1 = new JH_Auth_BranchB().GetEntity(d => d.DeptName == branch.DeptName && d.ComId == UserInfo.User.ComId && d.DeptRoot == branch.DeptRoot);
                if (branch1 != null)
                {
                    msg.ErrorMsg = "同级别内此部门已存在";
                    return;
                }

                branch.ComId = UserInfo.User.ComId;
                branch.CRUser = UserInfo.User.UserName;
                branch.CRDate = DateTime.Now;
                //添加部门，失败提示用户，成功赋值微信部门Code并更新
                if (!new JH_Auth_BranchB().Insert(branch))
                {
                    msg.ErrorMsg = "添加部门失败";
                    return;
                }
                //获取上级的Path，用于上级查找下级所有部门或用户
                branch.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branch.DeptRoot) + branch.DeptCode;
                new JH_Auth_BranchB().Update(branch);
                if (UserInfo.QYinfo.IsUseWX == "Y")
                {

                    WXHelp bm = new WXHelp(UserInfo.QYinfo);
                    long branid = bm.WX_CreateBranch(branch);
                    branch.WXBMCode = int.Parse(branid.ToString());
                    new JH_Auth_BranchB().Update(branch);



                }
                msg.Result = branch;
            }
            else//DeptCode不等于0时为修改部门
            {
                if (branch.DeptRoot != -1)
                {
                    branch.Remark1 = new JH_Auth_BranchB().GetBranchNo(UserInfo.User.ComId.Value, branch.DeptRoot) + branch.DeptCode;
                }
                if (UserInfo.QYinfo.IsUseWX == "Y" && branch.DeptRoot != -1)
                {
                    WXHelp bm = new WXHelp(UserInfo.QYinfo);
                    bm.WX_UpdateBranch(branch);
                }
                if (!new JH_Auth_BranchB().Update(branch))
                {
                    msg.ErrorMsg = "修改部门失败";
                    return;
                }
            }

        }

        public JH_Auth_Branch GetBMByDeptCode(int ComID, int DeptCode)
        {
            JH_Auth_Branch branchmodel = new JH_Auth_Branch();

            branchmodel = new JH_Auth_BranchB().GetEntity(d => d.ComId == ComID && d.DeptCode == DeptCode);

            return branchmodel;
        }


        public override bool Update(JH_Auth_Branch entity)
        {

            if (base.Update(entity))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Delete(JH_Auth_Branch entity)
        {

            return base.Delete(entity);
        }



        /// <summary>
        /// 根据部门代码删除部门及部门人员
        /// </summary>
        /// <param name="intBranchCode"></param>
        public void DelBranchByCode(int intBranchCode)
        {
            new JH_Auth_BranchB().Delete(d => d.DeptCode == intBranchCode);
            new JH_Auth_UserB().Delete(d => d.BranchCode == intBranchCode);
        }

        /// <summary>
        /// 获取机构数用在assginuser.ASPX中
        /// </summary>
        /// <param name="intRoleCode">角色代码</param>
        /// <param name="intBranchCode">机构代码</param>
        /// <returns></returns>
        public string GetBranchUserTree(string CheckNodes, int intBranchCode)
        {
            StringBuilder strTree = new StringBuilder();
            var q = new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == intBranchCode);
            foreach (var item in q)
            {
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',name:'{2}',{3}}},", item.DeptCode, item.DeptRoot, item.DeptName, item.DeptRoot == -1 || item.DeptRoot == 0 ? "open:true" : "open:false");
                strTree.Append(GetUserByBranch(CheckNodes, item.DeptCode));
                strTree.Append(GetBranchUserTree(CheckNodes, item.DeptCode));
            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }

        public string GetUserByBranch(string CheckNodes, int intBranchCode)
        {
            StringBuilder strTree = new StringBuilder();
            var q = new JH_Auth_UserB().GetEntities(d => d.BranchCode == intBranchCode);
            foreach (var item in q)
            {
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',name:'{2}',isUser:'{3}',{4}}},", item.UserName, intBranchCode, item.UserRealName, "Y", CheckNodes.SplitTOList(',').Contains(item.UserName) ? "checked:true" : "checked:false");
            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }




        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <param name="intDeptCode">机构代码</param>
        /// <returns></returns>
        public DataTable GetBranchList(int intDeptCode, int comId, string branchQX = "", int index = 0)
        {
            DataTable dtRoot = new DataTable();
            DataTable dt = new JH_Auth_BranchB().GetDTByCommand("SELECT * from JH_Auth_Branch  where DeptRoot=" + intDeptCode + " and ComId=" + comId + " order by DeptShort DESC");
            dt.Columns.Add("ChildBranch", Type.GetType("System.Object"));
            foreach (DataRow row in dt.Rows)
            {
                int deptCode = int.Parse(row["DeptCode"].ToString());
                index++;
                if (branchQX == "" || index == 1)
                {
                    row["ChildBranch"] = GetBranchList(deptCode, comId, branchQX, index);
                }
                else
                {
                    if (branchQX.SplitTOInt(',').Contains(deptCode))
                    {
                        row.Delete();
                    }
                    else
                    {
                        row["ChildBranch"] = GetBranchList(deptCode, comId, branchQX, index);
                    }
                }
            }
            dt.AcceptChanges();
            if (dtRoot.Rows.Count > 0)
            {
                dtRoot.Rows[0]["ChildBranch"] = dt;
                return dtRoot;
            }
            else
            {
                return dt;
            }
        }
        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <param name="intDeptCode">机构代码</param>
        /// <returns></returns>
        public string GetBranchTree(int intDeptCode, string checkValue)
        {
            string[] checkIds = checkValue.Split(',');
            StringBuilder strTree = new StringBuilder();
            var q = new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == intDeptCode);
            foreach (var item in q)
            {
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',attr:'{2}',name:'{3}',leader:'{4}',isuse:'{5}',order:'{6}',checked:{7}}},", item.DeptCode, item.DeptRoot, "Branch", item.DeptName, item.Remark1, item.Remark2, item.DeptShort, Array.IndexOf(checkIds, item.DeptCode.ToString()) > -1 ? "true" : "false");
                strTree.Append(GetBranchTree(item.DeptCode, checkValue));
            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }
        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <param name="intDeptCode">机构代码</param>
        /// <returns></returns>
        public string GetBranchTreeNew(int intDeptCode, int comId, int index = 0)
        {

            StringBuilder strTree = new StringBuilder();
            if (index == 0)
            {
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(comId, intDeptCode);
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',attr:'{2}',name:'{3}',leader:'{4}',isuse:'{5}',order:'{6}',{7}}},", branch.DeptCode, branch.DeptRoot, "Branch", branch.DeptName, branch.Remark1, branch.Remark2, branch.DeptShort, index == 0 ? "open:true" : "open:false");
                index++;
            }
            var q = new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == intDeptCode && d.ComId == comId);
            foreach (var item in q)
            {
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',attr:'{2}',name:'{3}',leader:'{4}',isuse:'{5}',order:'{6}',{7}}},", item.DeptCode, item.DeptRoot, "Branch", item.DeptName, item.Remark1, item.Remark2, item.DeptShort, index == 0 ? "open:true" : "open:false");
                index++;
                strTree.Append(GetBranchTreeNew(item.DeptCode, comId, index));
            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <param name="intDeptCode">机构代码</param>
        /// <returns></returns>
        public string GetBranchTree(int intDeptCode, int comId, string checkval, string branchQX = "", int index = 0)
        {
            StringBuilder strTree = new StringBuilder();
            if (index == 0)
            {
                JH_Auth_Branch branch = new JH_Auth_BranchB().GetBMByDeptCode(comId, intDeptCode);
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',attr:'{2}',name:'{3}',leader:'{4}',isuse:'{5}',order:'{6}',{7},checked:{8}}},", branch.DeptCode, branch.DeptRoot, "Branch", branch.DeptName, branch.Remark1, branch.Remark2, branch.DeptShort, index == 0 ? "open:true" : "open:false", Array.IndexOf(checkval.Split(','), branch.DeptCode.ToString()) > -1 ? "true" : "false");
                index++;
            }

            var q = new JH_Auth_BranchB().GetEntities(d => d.DeptRoot == intDeptCode && d.ComId == comId);
            foreach (var item in q)
            {
                //两种情况加载机构1:权限为空格代表拥有全部权限,2：权限部门包含当前部门
                if (branchQX == "" || branchQX.SplitTOInt(',').Contains(item.DeptCode))
                {
                    strTree.AppendFormat("{{id:'{0}',pId:'{1}',attr:'{2}',name:'{3}',leader:'{4}',isuse:'{5}',order:'{6}',{7},checked:{8}}},", item.DeptCode, item.DeptRoot, "Branch", item.DeptName, item.BranchLeader, item.Remark2, item.DeptShort, index == 0 ? "open:true" : "open:false", Array.IndexOf(checkval.Split(','), item.DeptCode.ToString()) > -1 ? "true" : "false");
                    strTree.Append(GetBranchTree(item.DeptCode, comId, checkval, branchQX, index));
                }
                index++;

            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }


        //获取JSON用户信息

        /// <summary>
        /// 获取部门级别编号
        /// </summary>
        /// <param name="DeptRoot"></param>
        /// <returns></returns>
        public string GetBranchNo(int ComID, int DeptRoot)
        {
            string BranchNo = "";
            var BranchUP = new JH_Auth_BranchB().GetBMByDeptCode(ComID, DeptRoot);
            //如果添加的上级部门存在，并且添加的同级部门中存在数据，获取同级部门的最后一个编号+1
            if (BranchUP != null)
            {
                BranchNo = BranchUP.Remark1 == "" ? "" : BranchUP.Remark1 + "-";
            }
            return BranchNo;
        }

        public class BranchUser
        {
            public int BranchID { get; set; }
            public string BranchName { get; set; }
            public string BranchFzr { get; set; }

            public List<BranchUser> SubBranch { get; set; }
            public List<JH_Auth_User> SubUsers { get; set; }

        }
        /// <summary>
        /// 获取当前部门不能查看的部门Ids
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="ComId"></param>
        /// <returns></returns>
        public string GetBranchQX(JH_Auth_UserB.UserInfo userInfo)
        {
            string strSql = string.Format("SELECT DeptCode from JH_Auth_Branch where ComId={0}  And ','+TXLQX+',' NOT LIKE '%,{1},%' and TXLQX!='' and TXLQX is NOT NULL and IsHasQX='Y'", userInfo.User.ComId, userInfo.User.BranchCode);
            DataTable dt = new JH_Auth_BranchB().GetDTByCommand(strSql);
            string qxbranch = "";
            foreach (DataRow row in dt.Rows)
            {
                qxbranch += row["DeptCode"] + ",";
            }
            qxbranch = qxbranch.Length > 0 ? qxbranch.Substring(0, qxbranch.Length - 1) : "";
            return qxbranch;
        }
    }





    //角色表
    public class JH_Auth_RoleB : BaseEFDao<JH_Auth_Role>
    {

        /// <summary>
        /// 获取角色树
        /// </summary>
        /// <param name="intRoleCode">角色代码</param>
        /// <returns></returns>
        public string GetRoleTree(int intRoleCode)
        {
            StringBuilder strTree = new StringBuilder();
            var q = new JH_Auth_RoleB().GetEntities(d => d.PRoleCode == intRoleCode);
            foreach (var item in q)
            {
                strTree.AppendFormat("{{id:'{0}',pId:'{1}',icon:'../../Image/admin/users.png',issys:'{2}',isuse:'{3}',name:'{4}',nodedec:'{5}'}},", item.RoleCode, item.PRoleCode, item.isSysRole, item.IsUse, item.RoleName, item.RoleDec);
                strTree.Append(GetRoleTree(item.RoleCode));
            }
            return strTree.Length > 0 ? strTree.ToString() : "";
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="intRoleCode">角色代码</param>
        /// <returns></returns>
        public string delRole(int intRoleCode, int ComId)
        {
            new JH_Auth_RoleB().Delete(d => intRoleCode == d.RoleCode && d.isSysRole != "Y" && d.ComId == ComId);
            new JH_Auth_UserRoleB().Delete(d => intRoleCode == d.RoleCode && d.ComId == ComId);
            new JH_Auth_RoleFunB().Delete(d => d.RoleCode == intRoleCode && d.ComId == ComId);
            return "Success";
        }

        public DataTable GetModelFun(int ComId, string RoleCode, string strModeID)
        {
            DataTable dt = new JH_Auth_UserRoleB().GetDTByCommand("SELECT  DISTINCT JH_Auth_Function.ID, JH_Auth_Function.ModelID,JH_Auth_Function.PageName,JH_Auth_Function.ExtData,JH_Auth_Function.PageUrl,JH_Auth_Function.FunOrder,JH_Auth_Function.PageCode,JH_Auth_Function.isiframe FROM JH_Auth_RoleFun INNER JOIN JH_Auth_Function ON JH_Auth_RoleFun.FunCode=JH_Auth_Function.ID WHERE RoleCode IN (" + RoleCode + ")  AND ModelID='" + strModeID + "' AND  JH_Auth_RoleFun.ComId=" + ComId + " and (JH_Auth_Function.ComId=" + ComId + " or JH_Auth_Function.ComId=0)  order by JH_Auth_Function.FunOrder");
            return dt;
        }


    }



    //用户角色表
    public class JH_Auth_UserRoleB : BaseEFDao<JH_Auth_UserRole>
    {


        /// <summary>
        /// 获取用户的角色代码
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <returns></returns>
        public string GetRoleCodeByUserName(string strUserName, int ComId)
        {
            string strRoleCode = "";
            var q = new JH_Auth_UserRoleB().GetEntities(d => d.UserName == strUserName && d.ComId == ComId);
            foreach (var item in q)
            {
                strRoleCode = strRoleCode + item.RoleCode.ToString() + ",";
            }
            return strRoleCode.TrimEnd(','); ;
        }

        /// <summary>
        /// 获取用户的角色代码
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <returns></returns>
        public string GetRoleNameByUserName(string strUserName, int ComId)
        {
            string strRoleName = "";
            string strRoleCode = GetRoleCodeByUserName(strUserName, ComId);
            DataTable dt = new JH_Auth_UserRoleB().GetDTByCommand("SELECT RoleName from JH_Auth_Role  where  RoleCode IN ('" + strRoleCode.ToFormatLike() + "') ");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                strRoleName = strRoleName + dt.Rows[i]["RoleName"].ToString() + ",";
            }
            return strRoleName.TrimEnd(',');
        }



        /// <summary>
        /// 根据角色获取相应用户
        /// </summary>
        /// <param name="intRoleCode"></param>
        /// <returns></returns>
        public string GetUserByRoleCode(int intRoleCode)
        {
            return new JH_Auth_UserRoleB().GetEntities(d => d.RoleCode == intRoleCode).Select(d => d.UserName).ToList().ListTOString(',');
        }

        public DataTable GetUserDTByRoleCode(int intRoleCode, int ComId)
        {
            DataTable dt = new JH_Auth_UserRoleB().GetDTByCommand(" SELECT JH_Auth_User.* FROM dbo.JH_Auth_UserRole ur inner join dbo.JH_Auth_User on ur.username=JH_Auth_User.username where  JH_Auth_User.IsUse='Y' And JH_Auth_User.ComId=" + ComId + " And  ur.rolecode= " + intRoleCode);
            return dt;
        }

    }






    public class JH_Auth_ZiDianB : BaseEFDao<JH_Auth_ZiDian>
    {









        private List<JH_Auth_ZiDian> GetZDList()
        {
            List<JH_Auth_ZiDian> ListData = CacheHelp.Get("zidian") as List<JH_Auth_ZiDian>;
            if (ListData != null)
            {
                return ListData;
            }
            else
            {
                ListData = base.GetALLEntities().ToList();
                CacheHelp.Set("zidian", ListData);
                return ListData;
            }
        }

        public override IEnumerable<JH_Auth_ZiDian> GetEntities(Expression<Func<JH_Auth_ZiDian, bool>> exp)
        {

            List<JH_Auth_ZiDian> ListData = this.GetZDList();
            return ListData.Where(exp.Compile());

        }

        /// <summary>
        /// 重写字典修改方法,先清除缓存再删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool Update(JH_Auth_ZiDian entity)
        {
            CacheHelp.Remove("zidian");
            return base.Update(entity);
        }

        public override bool Delete(JH_Auth_ZiDian entity)
        {
            CacheHelp.Remove("zidian");
            return base.Delete(entity);
        }

        public override bool Insert(JH_Auth_ZiDian entity)
        {
            CacheHelp.Remove("zidian");
            return base.Insert(entity);
        }
    }





    public class JH_Auth_LogB : BaseEFDao<JH_Auth_Log>
    {

        public void InsertLog(string Action, string LogContent, string ReMark, string strUser, string strUserName, int ComID, string strIP, string strNetCode = "")
        {
            Task<string> TaskCover = Task.Factory.StartNew<string>(() =>
            {
                this.Insert(new JH_Auth_Log()
                {
                    ComId = ComID.ToString(),
                    LogType = Action,
                    LogContent = LogContent,
                    Remark = ReMark,
                    IP = strIP,
                    Remark1 = strUserName,
                    CRUser = strUser,
                    CRDate = DateTime.Now,
                    netcode = strNetCode
                });
                return "";
            });
        }

    }


    public class JH_Auth_VersionB : BaseEFDao<JH_Auth_Version>
    {
        public DataTable GetLastVer(string strUserCode)
        {
            DataTable dt = new JH_Auth_UserRoleB().GetDTByCommand("SELECT TOP 1 *  from JH_Auth_Version WHERE ','+ReadUsers+','NOT like '%," + strUserCode + ",%'   ORDER by id DESC");
            return dt;
        }

        public void SetUserVerSion(string strUserCode, string strVerID)
        {

            JH_Auth_Version Model = this.GetEntity(d => d.ID.ToString() == strVerID);
            Model.ReadUsers = Model.ReadUsers + "," + strUserCode;
            this.Update(Model);
        }
    }


    public class JH_Auth_QYB : BaseEFDao<JH_Auth_QY>
    {


    }
    public class JH_Auth_UserCustomDataB : BaseEFDao<JH_Auth_UserCustomData>
    {

    }
    public class JH_Auth_RoleFunB : BaseEFDao<JH_Auth_RoleFun>
    { }

    public class JH_Auth_FunctionB : BaseEFDao<JH_Auth_Function>
    { }
    #endregion

    #region 流程处理模块

    public class Yan_WF_DaiLiB : BaseEFDao<Yan_WF_DaiLi>
    { }

    public class Yan_WF_TDB : BaseEFDao<Yan_WF_TD>
    {

    }
    public class Yan_WF_PDB : BaseEFDao<Yan_WF_PD>
    {
        /// <summary>
        ///获取流程Id
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public int GetProcessID(string processName)
        {
            return new Yan_WF_PDB().GetEntity(d => d.ProcessName == processName).ID;
        }
    }



    public class Yan_WF_PIB : BaseEFDao<Yan_WF_PI>
    {
        /// <summary>
        /// 添加流程()
        /// </summary>
        /// <param name="strAPPCode"></param>
        /// <param name="strSHR"></param>
        /// <returns>返回创建的第一个任务</returns>
        public Yan_WF_TI StartWF(Yan_WF_PD PD, string strModelCode, string userName, string strSHR, string strCSR, Yan_WF_PI PI, ref List<string> ListNextUser)
        {



            if (PD.ProcessType == "-1")
            {
                ENDWF(PI.ID);
            }//没有流程步骤的,直接结束流程


            //创建流程实例
            Yan_WF_TI TI = new Yan_WF_TI();
            if (PD.ProcessType == "1")//固定流程
            {
                //添加首任务
                TI.TDCODE = PI.PDID.ToString() + "-1";
                TI.PIID = PI.ID;
                TI.StartTime = DateTime.Now;
                TI.EndTime = DateTime.Now;
                TI.TaskUserID = userName;
                TI.TaskUserView = "发起表单";
                TI.TaskState = 1;//任务已结束
                TI.ComId = PD.ComId;
                TI.CRUser = userName;
                TI.CRDate = DateTime.Now;
                new Yan_WF_TIB().Insert(TI);
                //添加首任务
                ListNextUser = AddNextTask(TI);

            }
            return TI;

        }



        /// <summary>
        /// 结束当前任务
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="strManAgeUser"></param>
        /// <param name="strManAgeYJ"></param>
        private void ENDTASK(int TaskID, string strManAgeUser, string strManAgeYJ, int Status = 1)
        {
            Yan_WF_TI TI = new Yan_WF_TIB().GetEntity(d => d.ID == TaskID);
            TI.TaskUserID = strManAgeUser;
            TI.TaskUserView = strManAgeYJ;
            TI.EndTime = DateTime.Now;
            TI.TaskState = Status;
            new Yan_WF_TIB().Update(TI);
            new Yan_WF_PIB().ExsSclarSql("UPDATE Yan_WF_TI SET TaskState=" + Status + " WHERE PIID='" + TI.PIID + "' AND TDCODE='" + TI.TDCODE + "'");//将所有任务置为结束状态
        }

        /// <summary>
        /// 结束当前流程
        /// </summary>
        /// <param name="PID"></param>
        public void ENDWF(int PID)
        {
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == PID);
            PI.isComplete = "Y";
            PI.CompleteTime = DateTime.Now;
            new Yan_WF_PIB().Update(PI);
        }


        /// <summary>
        /// 添加下一任务节点
        /// </summary>
        /// <param name="PID"></param>
        private List<string> AddNextTask(Yan_WF_TI TI, string strZPUser = "")
        {
            List<string> ListNextUser = new List<string>();
            string strNextTcode = TI.TDCODE.Split('-')[0] + "-" + (int.Parse(TI.TDCODE.Split('-')[1]) + 1).ToString();//获取任务CODE编码,+1即为下个任务编码
            Yan_WF_TD TD = new Yan_WF_TD();
            TD = new Yan_WF_TDB().GetEntity(d => d.TDCODE == strNextTcode);
            if (TD != null)
            {
                Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == TI.PIID);
                if (TD.isSJ == "0")//选择角色时找寻角色人员
                {
                    DataTable dt = new JH_Auth_UserRoleB().GetUserDTByRoleCode(Int32.Parse(TD.AssignedRole), TI.ComId.Value);
                    foreach (DataRow dr in dt.Rows)
                    {
                        Yan_WF_TI Node = new Yan_WF_TI();
                        Node.TDCODE = strNextTcode;
                        Node.PIID = TI.PIID;
                        Node.StartTime = DateTime.Now;
                        Node.TaskUserID = dr["username"].ToString();
                        Node.TaskState = 0;//任务待结束
                        Node.TaskName = TD.TaskName;
                        Node.TaskRole = TD.TaskAssInfo;
                        Node.ComId = TI.ComId;
                        Node.CRDate = DateTime.Now;
                        new Yan_WF_TIB().Insert(Node);
                        ListNextUser.Add(dr["username"].ToString());
                    }
                }
                if (TD.isSJ == "1")//选择上级时找寻上级
                {
                    string Leader = new JH_Auth_UserB().GetUserLeader(PI.ComId.Value, TI.TaskUserID);
                    Yan_WF_TI Node = new Yan_WF_TI();
                    Node.TDCODE = strNextTcode;
                    Node.PIID = TI.PIID;
                    Node.StartTime = DateTime.Now;
                    Node.TaskUserID = Leader;
                    Node.TaskState = 0;//任务待结束
                    Node.TaskName = TD.TaskName;
                    Node.TaskRole = TD.TaskAssInfo;
                    Node.ComId = TI.ComId;
                    Node.CRDate = DateTime.Now;
                    new Yan_WF_TIB().Insert(Node);
                    ListNextUser.Add(Leader);

                }
                if (TD.isSJ == "2")//选择发起人时找寻发起人
                {


                    Yan_WF_TI Node = new Yan_WF_TI();
                    Node.TDCODE = strNextTcode;
                    Node.PIID = TI.PIID;
                    Node.StartTime = DateTime.Now;
                    Node.TaskUserID = PI.CRUser;
                    Node.TaskState = 0;//任务待结束
                    Node.TaskName = TD.TaskName;
                    Node.TaskRole = TD.TaskAssInfo;
                    Node.ComId = TI.ComId;
                    Node.CRDate = DateTime.Now;
                    new Yan_WF_TIB().Insert(Node);
                    ListNextUser.Add(PI.CRUser);
                }
                if (TD.isSJ == "3")//选择指定人员找指定人
                {
                    foreach (string user in TD.AssignedRole.TrimEnd(',').Split(','))
                    {

                        Yan_WF_TI Node = new Yan_WF_TI();
                        Node.TDCODE = strNextTcode;
                        Node.PIID = TI.PIID;
                        Node.StartTime = DateTime.Now;
                        Node.TaskUserID = user;
                        Node.TaskState = 0;//任务待结束
                        Node.TaskName = TD.TaskName;
                        Node.TaskRole = TD.TaskAssInfo;
                        Node.ComId = TI.ComId;
                        Node.CRDate = DateTime.Now;
                        new Yan_WF_TIB().Insert(Node);
                        ListNextUser.Add(user);
                    }
                }
                if (TD.isSJ == "4")//选择绑定字段的值
                {

                    string FiledValue = new JH_Auth_ExtendDataB().GetFiledValue(TD.AssignedRole, PI.PDID.Value, PI.ID);
                    string strGLUser = new JH_Auth_UserB().GetUserByUserRealName(PI.ComId.Value, FiledValue);
                    Yan_WF_TI Node = new Yan_WF_TI();
                    Node.TDCODE = strNextTcode;
                    Node.PIID = TI.PIID;
                    Node.StartTime = DateTime.Now;
                    Node.TaskUserID = strGLUser;
                    Node.TaskState = 0;//任务待结束
                    Node.TaskName = TD.TaskName;
                    Node.TaskRole = TD.TaskAssInfo;
                    Node.ComId = TI.ComId;
                    Node.CRDate = DateTime.Now;
                    new Yan_WF_TIB().Insert(Node);
                    ListNextUser.Add(strGLUser);
                }

            }


            return ListNextUser;

        }
        /// <summary>
        /// 退回当前流程
        /// </summary>
        /// <param name="PID"></param>
        private void REBACKWF(int PID)
        {
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == PID);
            PI.IsCanceled = "Y";
            PI.CanceledTime = DateTime.Now;
            new Yan_WF_PIB().Update(PI);

        }

        /// <summary>
        /// 获取需要处理的任务
        /// </summary>
        /// <param name="strUser">用户需要处理</param>
        /// <returns></returns>
        public List<Yan_WF_TI> GetDSH(JH_Auth_User User)
        {
            List<Yan_WF_TI> ListData = new List<Yan_WF_TI>();

            ListData = new Yan_WF_TIB().GetEntities("ComId ='" + User.ComId.Value + "' AND TaskUserID ='" + User.UserName + "'AND TaskState='0'").ToList();
            return ListData;
        }



        /// <summary>
        /// 判断当前用户当前流程是否可以审批
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public string isCanSP(string strUser, int PIID)
        {
            DataTable dt = new Yan_WF_TIB().GetDTByCommand("SELECT ID FROM  dbo.Yan_WF_TI  WHERE PIID='" + PIID + "' AND TaskState='0' AND TaskUserID='" + strUser + "' ");
            return dt.Rows.Count > 0 ? "Y" : "N";
        }


        /// <summary>
        /// 判断用户是否有编辑表单得权限
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public string isCanEdit(string strUser, int PIID)
        {
            DataTable dt = new Yan_WF_TIB().GetDTByCommand("SELECT Yan_WF_TD.isCanEdit FROM  dbo.Yan_WF_TI LEFT JOIN   Yan_WF_TD on  Yan_WF_TI.TDCODE=Yan_WF_TD.TDCODE  WHERE PIID='" + PIID + "' AND TaskState='0' AND TaskUserID='" + strUser + "'and isCanEdit='True' ");
            return dt.Rows.Count > 0 ? "Y" : "N";

        }


        /// <summary>
        /// 退回流程
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public bool REBACKLC(string strUser, int PIID, string strYJView)
        {
            try
            {
                Yan_WF_TI MODEL = new Yan_WF_TIB().GetEntities(" PIID='" + PIID + "' AND TaskState='0' AND TaskUserID='" + strUser + "' ").FirstOrDefault();
                if (MODEL != null)
                {
                    ENDTASK(MODEL.ID, strUser, strYJView, -1);//退回
                    REBACKWF(MODEL.PIID);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }


        }

        /// <summary>
        /// 处理流程
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public bool MANAGEWF(string strUser, int PIID, string strYJView, ref List<string> ListNextUser, string strShUser)
        {
            try
            {
                Yan_WF_TI MODEL = new Yan_WF_TIB().GetEntities(" PIID='" + PIID + "' AND TaskState='0' AND TaskUserID='" + strUser + "' ").FirstOrDefault();
                if (MODEL != null)
                {
                    ENDTASK(MODEL.ID, strUser, strYJView);
                    ListNextUser = AddNextTask(MODEL, strShUser);
                    //循环找下一个审核人是否包含本人，如果包含则审核通过
                    if (ListNextUser.Contains(strUser))
                    {
                        ListNextUser.Clear();
                        return MANAGEWF(strUser, PIID, strYJView, ref ListNextUser, strShUser);
                    }
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }


        }

        /// <summary>
        /// 已处理的任务
        /// </summary>
        /// <param name="strUser"></param>
        /// <returns></returns>
        public List<Yan_WF_TI> GetYSH(JH_Auth_User User)
        {
            List<Yan_WF_TI> ListData = new List<Yan_WF_TI>();
            ListData = new Yan_WF_TIB().GetEntities(" ComId ='" + User.ComId.Value + "' AND TaskUserID ='" + User.UserName + "' AND (TaskState=1 OR TaskState=-1) AND TaskUserView!='发起表单'").ToList();
            return ListData;
        }


        public int GETPDID(int PIID)
        {
            Yan_WF_PI pi = new Yan_WF_PIB().GetEntity(d => d.ID == PIID);
            return pi == null ? 0 : pi.PDID.Value;
        }


        /// <summary>
        /// 更具PID获取PD数据
        /// </summary>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public Yan_WF_PD GETPDMODELBYID(int PIID)
        {
            Yan_WF_PD MODEL = new Yan_WF_PD();
            Yan_WF_PI pi = new Yan_WF_PIB().GetEntity(d => d.ID == PIID);
            if (pi != null)
            {
                MODEL = new Yan_WF_PDB().GetEntity(d => d.ID == pi.PDID);
            }

            return MODEL;
        }



        /// <summary>
        /// 根据数据ID进行归档操作
        /// </summary>
        /// <param name="modelCode">modelCode</param>
        /// <param name="PIID">流程的PIID</param>
        /// <returns></returns>
        public void GDForm(string isGD, int ComID, string PIIDS)
        {
            string strSql = string.Format(" UPDATE Yan_WF_PI SET isGD='{0}',GDDate='{1}' where Comid='{2}' AND  ID in ({3})", isGD, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ComID.ToString(), PIIDS);
            object obj = new Yan_WF_PIB().ExsSclarSql(strSql);

        }


        /// <summary>
        /// 根据数据ID和modelCode更新PIID(新版废弃)
        /// </summary>
        /// <param name="modelCode">modelCode</param>
        /// <param name="PIID">流程的PIID</param>
        /// <returns></returns>
        public string UpdateDataIdByCode(string modelCode, int DATAID, int PIID)
        {
            JH_Auth_Model model = new JH_Auth_ModelB().GetEntity(d => d.ModelCode == modelCode);
            if (model != null)
            {
                string strSql = string.Format(" UPDATE {0} SET intProcessStanceid={1} where ID={2}", model.RelTable, PIID, DATAID);
                object obj = new Yan_WF_PIB().ExsSclarSql(strSql);
                return obj != null ? obj.ToString() : "";
            }
            return "";
        }



        /// <summary>
        /// 根据PIID判断当前流程的数据（）
        /// </summary>
        /// <param name="PIID"></param>
        /// <returns></returns>
        public string GetPDStatus(int PIID)
        {
            Yan_WF_PI Model = new Yan_WF_PIB().GetEntity(d => d.ID == PIID);
            if (Model != null)
            {
                if (Model.isComplete == "Y")
                {
                    return "已审批";
                }
                if (Model.IsCanceled == "Y")
                {
                    return "已退回";
                }

                return "正在审批";
            }
            else
            {
                return "";
            }
        }
        public int GetFormIDbyPID(string strModeCode, int PID)
        {
            int intFormID = 0;

            JH_Auth_Model QYModel = new JH_Auth_ModelB().GetEntity(d => d.ModelCode == strModeCode);
            string strSQL = string.Format("SELECT ID FROM " + QYModel.RelTable + " WHERE  intProcessStanceid='{0}'", PID);
            intFormID = int.Parse(new Yan_WF_PDB().ExsSclarSql(strSQL) == null ? "0" : new Yan_WF_PDB().ExsSclarSql(strSQL).ToString());

            return intFormID;
        }


        /// <summary>
        /// 判断当前用户当前流程是否可以撤回和更新,判断是不是只有一个处理过得节点，或者是自己创建的无流程的表单
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="PIID"></param>
        /// <returns>返回Y得时候可以撤回,返回不是Y,代表已经处理了不能撤回</returns>
        public string isCanCancel(string strUser, Yan_WF_PI PIMODEL)
        {
            string strReturn = "N";

            DataTable dt = new Yan_WF_TIB().GetDTByCommand("SELECT * FROM  dbo.Yan_WF_TI  WHERE PIID='" + PIMODEL.ID + "' AND EndTime IS not null");
            if (dt.Rows.Count == 1 && dt.Rows[0]["TaskUserID"].ToString() == strUser)
            {
                strReturn = "Y";
            }
            if (PIMODEL.CRUser == strUser && PIMODEL.PITYPE == "-1")
            {
                strReturn = "Y";
                //自己创建的无流程的表单
            }

            return strReturn;
        }

        /// <summary>
        /// 生成流水号
        /// </summary>
        /// <param name="PIMODEL"></param>
        /// <returns></returns>
        public string CreateWFNum(string PDID)
        {
            string strReturn = "";
            DataTable dt = new Yan_WF_TIB().GetDTByCommand(" select ISNULL(max(WFFormNum),0) as MAXFORMMUM from Yan_WF_PI WHERE PDID = '" + PDID + "'");
            strReturn = CommonHelp.GetWFNumber(dt.Rows[0][0].ToString(), PDID);
            return strReturn;
        }


    }

    public class Yan_WF_TIB : BaseEFDao<Yan_WF_TI> { }
    public class SZHL_DBGLB : BaseEFDao<SZHL_DBGL> { }


    #endregion

    #region 企业号相关
    public class JH_Auth_WXPJB : BaseEFDao<JH_Auth_WXPJ>
    {
    }
    public class JH_Auth_WXMSGB : BaseEFDao<JH_Auth_WXMSG>
    {
    }

    public class JH_Auth_QY_ModelB : BaseEFDao<JH_Auth_QY_Model>
    {


        public string isHasDataQX(string strModelCode, int DataID, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strISHasQX = "N";

            return strISHasQX;
        }

        public string ISHASDATAREADQX(string strModelCode, int DataID, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strISHasQX = "Y";

            return strISHasQX;
        }

    }
    public class JH_Auth_ModelB : BaseEFDao<JH_Auth_Model>
    {
        public JH_Auth_Model GetModeByCode(string strModeCode)
        {
            JH_Auth_Model QYModel = new JH_Auth_ModelB().GetEntity(d => d.ModelCode == strModeCode);
            return QYModel;
        }

        /// <summary>
        /// 获取首页显示菜单
        /// </summary>
        /// <param name="UserInfo">用户信息</param>
        /// <param name="modelType">APPINDEX APP首页  PCINDEX PC首页</param>
        /// <returns></returns>
        public DataTable GETMenuList(JH_Auth_UserB.UserInfo UserInfo, string modelType = "APPINDEX")
        {
            if (!string.IsNullOrEmpty(UserInfo.UserRoleCode))
            {
                string strSql = string.Format(@"SELECT DISTINCT  model.*,custom.ID UserAPPID,custom.DataContent1
                                             FROM JH_Auth_RoleFun rf INNER JOIN JH_Auth_Function fun on rf.FunCode=fun.ID and rf.ComId={0} and (fun.ComId={0} or fun.ComId=0)
                                            INNER join JH_Auth_Model model on fun.ModelID=model.ID AND (model.ComId={0} or model.ComId=0)
                                            LEFT join  JH_Auth_UserCustomData custom on model.ModelCode=custom.DataContent and custom.DataType='{1}' and custom.DataContent1='Y' and custom.UserName='{2}'and custom.ComId={0}
                                            where model.ModelStatus=0 and rf.RoleCode in ({3}) {4}", UserInfo.User.ComId, modelType, UserInfo.User.UserName, UserInfo.UserRoleCode, modelType == "APPINDEX" ? "and  WXUrl is not NULL  and  WXUrl!=''" : "");
                strSql = strSql + " ORDER by model.ORDERID ";
                return new JH_Auth_ModelB().GetDTByCommand(strSql);
            }
            return new DataTable();
        }



    }
    public class JH_Auth_QY_WXSCB : BaseEFDao<JH_Auth_QY_WXSC>
    {
    }
    public class JH_Auth_YYLogB : BaseEFDao<JH_Auth_YYLog>
    {
    }
    #endregion


    #region 扩展字段
    public class JH_Auth_ExtendModeB : BaseEFDao<JH_Auth_ExtendMode>
    {
        //获取扩展字段的值
        public DataTable GetExtData(int? ComId, string FormCode, string DATAID, string PDID = "")
        {
            string strWhere = string.Empty;
            if (PDID != "") { strWhere = " and j.PDID='" + PDID + "'"; }
            return new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select j.ComId, j.ID, j.TableName, j.TableFiledColumn, j.TableFiledName, j.TableFileType, j.DefaultOption, j.DefaultValue, j.IsRequire, d.ExtendModeID, d.ID AS ExtID, d.DataID, d.ExtendDataValue from [dbo].[JH_Auth_ExtendMode] j join JH_Auth_ExtendData d on j.ComId=d.ComId and j.ID=d.ExtendModeID where j.ComId='{0}' and j.TableName='{1}' and d.DataID='{2}' and d.ExtendDataValue<>'' and d.ExtendDataValue is not null  " + strWhere, ComId, FormCode, DATAID));
        }

        public DataTable GetExtDataAll(int? ComId, string FormCode, string DATAID, string PDID = "")
        {
            string strWhere = string.Empty;
            if (PDID != "") { strWhere = " and j.PDID='" + PDID + "'"; }
            return new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select j.ComId, j.ID, j.TableName, j.TableFiledColumn, j.TableFiledName, j.TableFileType, j.DefaultOption, j.DefaultValue, j.IsRequire, d.ExtendModeID, d.ID AS ExtID, d.DataID, d.ExtendDataValue from [dbo].[JH_Auth_ExtendMode] j join JH_Auth_ExtendData d on j.ComId=d.ComId and j.ID=d.ExtendModeID where j.ComId='{0}' and j.TableName='{1}' and d.DataID='{2}' " + strWhere, ComId, FormCode, DATAID));
        }

        public DataTable GetExtColumnAll(int? ComId, string FormCode, string PDID = "")
        {
            string strWhere = string.Empty;
            if (PDID != "") { strWhere = " and j.PDID='" + PDID + "'"; }
            return new JH_Auth_ExtendModeB().GetDTByCommand(string.Format("select j.ComId, j.ID, j.TableName, j.TableFiledColumn, j.TableFiledName, j.TableFileType, j.DefaultOption, j.DefaultValue, j.IsRequire from [dbo].[JH_Auth_ExtendMode] j where j.ComId='{0}' and j.TableName='{1}'" + strWhere, ComId, FormCode));
        }
    }
    public class JH_Auth_ExtendDataB : BaseEFDao<JH_Auth_ExtendData>
    {


        public string GetFiledValue(string FiledName, int pdid, int piid)
        {
            string value = "";
            JH_Auth_ExtendData model = new JH_Auth_ExtendDataB().GetEntities(d => d.PDID == pdid && d.DataID == piid && d.ExFiledColumn == FiledName).FirstOrDefault();
            if (model != null)
            {
                value = model.ExtendDataValue;
            }
            return value;
        }



        /// <summary>
        /// 获取导入excel的字段
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<IMPORTYZ> GetTable(string code, int comid)
        {
            string json = string.Empty;
            switch (code)
            {
                case "KHGL":

                    json = "[{\"Name\":\"客户名称\",\"Length\":\"50\",\"IsNull\":\"1\",\"IsRepeat\":\"SZHL_CRM_KHGL|KHName\"},{\"Name\":\"客户类型\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"电话\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"邮箱\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"传真\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"网址\",\"Length\":\"50\",\"IsNull\":\"0\"},"

                            + "{\"Name\":\"地址\",\"Length\":\"500\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"邮编\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"跟进状态\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"客户来源\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"所属行业\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"人员规模\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"负责人\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                            + "{\"Name\":\"备注\",\"Length\":\"0\",\"IsNull\":\"0\"}]";
                    break;
                case "KHLXR":

                    json = "[{\"Name\":\"姓名\",\"Length\":\"50\",\"IsNull\":\"1\"},{\"Name\":\"对应客户\",\"Length\":\"50\",\"IsNull\":\"0\",\"IsExist\":\"SZHL_CRM_KHGL|KHName\"},"
                        + "{\"Name\":\"手机\",\"Length\":\"11\",\"IsNull\":\"1\"},{\"Name\":\"邮箱\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"传真\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"网址\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"电话\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"分机\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"QQ\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"微信\",\"Length\":\"100\",\"IsNull\":\"0\"},"
                        //+ "{\"Name\":\"学历\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"公司\",\"Length\":\"100\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"部门\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"职位\",\"Length\":\"100\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"地址\",\"Length\":\"200\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"邮编\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"性别\",\"Length\":\"10\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"生日\",\"Length\":\"10\",\"IsNull\":\"0\"},{\"Name\":\"备注\",\"Length\":\"0\",\"IsNull\":\"0\"}]";
                    break;
                case "HTGL":
                    json = "[{\"Name\":\"合同标题\",\"Length\":\"2500\",\"IsNull\":\"1\"},{\"Name\":\"合同类型\",\"Length\":\"50\",\"IsNull\":\"1\"},"
                        + "{\"Name\":\"合同总金额\",\"Length\":\"100\",\"IsNull\":\"1\"},{\"Name\":\"签约日期\",\"Length\":\"10\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"对应客户\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"开始时间\",\"Length\":\"10\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"结束时间\",\"Length\":\"10\",\"IsNull\":\"0\"},{\"Name\":\"合同状态\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"关联产品\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"付款方式\",\"Length\":\"100\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"付款说明\",\"Length\":\"1500\",\"IsNull\":\"0\"},{\"Name\":\"有效期\",\"Length\":\"100\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"我方签约人\",\"Length\":\"50\",\"IsNull\":\"0\"},{\"Name\":\"客户方签约人\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"合同编号\",\"Length\":\"100\",\"IsNull\":\"0\"},{\"Name\":\"负责人\",\"Length\":\"50\",\"IsNull\":\"0\"},"
                        + "{\"Name\":\"备注\",\"Length\":\"0\",\"IsNull\":\"0\"}]";
                    break;
            }

            if (comid != 0)
            {
                json = json.Substring(0, json.Length - 1);

                DataTable dtExtColumn = new JH_Auth_ExtendModeB().GetExtColumnAll(comid, code);
                foreach (DataRow drExt in dtExtColumn.Rows)
                {
                    json = json + ",{\"Name\":\"" + drExt["TableFiledName"].ToString() + "\",\"Length\":\"0\",\"IsNull\":\"0\"}";
                }

                json = json + "]";
            }

            List<IMPORTYZ> cls = JsonConvert.DeserializeObject<List<IMPORTYZ>>(json);
            return cls;

        }



        public class IMPORTYZ
        {
            public string Name { get; set; }
            public int Length { get; set; }
            public int IsNull { get; set; }
            public string IsRepeat { get; set; }
            public string IsExist { get; set; }
        }


        /// <summary>
        /// 获取模板中的默认数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetExcelData(string str)
        {
            string json = string.Empty;
            switch (str)
            {
                case "KHGL":
                    json = "贸易公司, 普通客户,010-65881997,123@qq.com, 010-65881998, http://www.baidu.com/,"
                        + "东三环中路101号,100000,初访,广告,服务,小于10人,13312345678,主营外贸销售，代理国外一线品牌";
                    break;
                case "KHLXR":
                    json = "张三,贸易公司,13667894321,123@qq.com,010-65881997 ,http://www.baidu.com/,010-65881997,61601 ,1123213213,fassfd21421,"
                        + "客户部,经理,东三环中路101号,100000,男,1983-09-13,负责XX项目的实施";
                    break;
                case "HTGL":
                    json = "XX项目,项目合同,12300,2015-07-08,贸易公司,2015-07-08,2015-12-08,未开始,企业号项目, 银行转账,"
                        + "服务,6个月,张经理,王经理,AC2001243251002,13312345678,合同备注";
                    break;
            }

            return json;
        }
    }

    #endregion




    #region 数据BI模块

    public class BI_DB_DimB : BaseEFDao<BI_DB_Dim> { }
    public class BI_DB_SetB : BaseEFDao<BI_DB_Set>
    {


        /// <summary>
        /// 获取Data得经纬度
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<BI_DB_Dim> getCType(DataTable dt)
        {

            List<BI_DB_Dim> ListDIM = new List<BI_DB_Dim>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                BI_DB_Dim DIM = new BI_DB_Dim();
                DIM.Name = dt.Columns[i].ColumnName;
                DIM.ColumnName = dt.Columns[i].ColumnName;
                string strType = dt.Columns[i].DataType.Name.ToLower();
                if (strType.Contains("int"))
                {
                    DIM.ColumnType = "Num";
                    DIM.Dimension = "2";
                }
                else if (strType.Contains("decimal") || strType.Contains("float") || strType.Contains("Double"))
                {
                    DIM.ColumnType = "float";
                    DIM.Dimension = "2";

                }
                else if (strType.Contains("datetime"))
                {
                    DIM.ColumnType = "Date";
                    DIM.Dimension = "1";
                }
                else
                {
                    DIM.ColumnType = "Str";
                    DIM.Dimension = "1";

                }
                ListDIM.Add(DIM);
            }
            return ListDIM;
        }
    }
    public class BI_DB_SourceB : BaseEFDao<BI_DB_Source>
    {

        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <param name="intSID"></param>
        /// <returns></returns>
        public DBFactory GetDB(int intSID)
        {
            DBFactory db = new DBFactory();
            if (intSID == 0)//本地数据库
            {
                string strCon = new BI_DB_SetB().GetDBString();
                db = new DBFactory(strCon);
            }
            else
            {
                var tt = new BI_DB_SourceB().GetEntity(p => p.ID == intSID);
                if (tt != null)
                {
                    db = new DBFactory(tt.DBType, tt.DBIP, tt.Port, tt.DBName, tt.Schema, tt.DBUser, tt.DBPwd);
                }
            }
            return db;

        }
    }
    public class BI_DB_YBPB : BaseEFDao<BI_DB_YBP>
    {


    }

    public class BI_DB_TableB : BaseEFDao<BI_DB_Table> { }


    #endregion



    #region 文档管理模块
    public class FT_FolderB : BaseEFDao<FT_Folder>
    {


        public FoldFile GetWDTREE(int FolderID, ref List<FoldFileItem> ListID, int comId, string strUserName = "")
        {
            List<FT_Folder> ListAll = new FT_FolderB().GetEntities(d => d.ComId == comId).ToList();
            FT_Folder Folder = new FT_FolderB().GetEntity(d => d.ID == FolderID);
            FT_FolderB.FoldFile Model = new FT_FolderB.FoldFile();
            Model.Name = Folder.Name;
            Model.FolderID = Folder.ID;
            Model.CRUser = Folder.CRUser;
            Model.PFolderID = Folder.PFolderID.Value;
            ListID.Add(new FoldFileItem() { ID = Folder.ID, Type = "folder" });
            if (strUserName != "")
            {
                Model.SubFileS = new FT_FileB().GetEntities(d => d.FolderID == Folder.ID && d.CRUser == strUserName && d.ComId == comId).ToList();
            }
            else
            {
                Model.SubFileS = new FT_FileB().GetEntities(d => d.FolderID == Folder.ID && d.ComId == comId).ToList();
            }
            foreach (var item in Model.SubFileS)
            {
                ListID.Add(new FoldFileItem() { ID = item.ID, Type = "file" });

            }
            Model.SubFolder = new FT_FolderB().GETNEXTFLODER(Folder.ID, ListAll, ref ListID, comId, strUserName);
            return Model;
        }


        private List<FoldFile> GETNEXTFLODER(int FolderID, List<FT_Folder> ListAll, ref List<FoldFileItem> ListID, int comId, string strUserName = "")
        {
            List<FoldFile> ListData = new List<FoldFile>();
            var list = ListAll.Where(d => d.PFolderID == FolderID && d.ComId == comId);
            if (strUserName != "")
            {
                list = list.Where(d => d.CRUser == strUserName);
            }
            foreach (var item in list)
            {
                FoldFile FolderNew = new FoldFile();
                FolderNew.FolderID = item.ID;
                FolderNew.Name = item.Name;
                FolderNew.CRUser = item.CRUser;
                FolderNew.PFolderID = item.PFolderID.Value;
                if (strUserName != "")
                {
                    FolderNew.SubFileS = new FT_FileB().GetEntities(d => d.FolderID == item.ID && d.CRUser == strUserName && d.ComId == comId).ToList();
                }
                else
                {
                    FolderNew.SubFileS = new FT_FileB().GetEntities(d => d.FolderID == item.ID && d.ComId == comId).ToList();
                }
                foreach (var SubFile in FolderNew.SubFileS)
                {
                    ListID.Add(new FoldFileItem() { ID = SubFile.ID, Type = "file" });
                }
                FolderNew.SubFolder = GETNEXTFLODER(item.ID, ListAll, ref ListID, comId, strUserName);
                ListData.Add(FolderNew);
                ListID.Add(new FoldFileItem() { ID = item.ID, Type = "folder" });
            }
            return ListData;

        }



        /// <summary>
        /// 获取指定文件夹下得所有文件夹
        /// </summary>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public List<FT_Folder> GetChiFolder(int FolderID)
        {
            string strQuery = FolderID.ToString() + "-";
            return new FT_FolderB().GetEntities(d => d.Remark.Contains(strQuery)).ToList();
        }

        /// <summary>
        /// 复制树状结构
        /// </summary>
        /// <param name="FloderID"></param>
        /// <param name="PID"></param>
        public void CopyFloderTree(int FloderID, int PID, int comId)
        {
            List<FoldFileItem> ListID = new List<FoldFileItem>();
            FoldFile Model = new FT_FolderB().GetWDTREE(FloderID, ref ListID, comId);
            FT_Folder Folder = new FT_FolderB().GetEntity(d => d.ID == Model.FolderID && d.ComId == comId);
            Folder.PFolderID = PID;
            new FT_FolderB().Insert(Folder);

            //更新文件夹路径Code
            FT_Folder PFolder = new FT_FolderB().GetEntity(d => d.ID == PID && d.ComId == comId);
            Folder.Remark = PFolder.Remark + "-" + Folder.ID;
            new FT_FolderB().Update(Folder);

            foreach (FT_File file in Model.SubFileS)
            {
                file.FolderID = Folder.ID;
                new FT_FileB().Insert(file);
            }
            GreateWDTree(Model.SubFolder, Folder.ID, comId);
        }

        /// <summary>
        /// 根据父ID创建树装结构文档
        /// </summary>
        /// <param name="ListFoldFile"></param>
        private void GreateWDTree(List<FoldFile> ListFoldFile, int newfolderid, int comId)
        {

            foreach (FoldFile item in ListFoldFile)
            {

                FT_Folder PModel = new FT_FolderB().GetEntity(d => d.ID == item.FolderID && d.ComId == comId);
                PModel.PFolderID = newfolderid;
                new FT_FolderB().Insert(PModel);

                //更新文件夹路径Code
                FT_Folder PFolder = new FT_FolderB().GetEntity(d => d.ID == newfolderid && d.ComId == comId);
                PModel.Remark = PFolder.Remark + "-" + PModel.ID;
                new FT_FolderB().Update(PModel);

                foreach (FT_File file in item.SubFileS)
                {
                    file.FolderID = PModel.ID;
                    new FT_FileB().Insert(file);
                }

                GreateWDTree(item.SubFolder, PModel.ID, comId);



            }
        }





        /// <summary>
        /// 判断用户是否有当前文件件的管理权限
        /// </summary>
        /// <param name="FloderID"></param>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public string isHasManage(string FloderID, string strUserName)
        {
            string strReturn = "N";
            FT_Folder Model = new FT_FolderB().GetEntities("ID=" + FloderID).SingleOrDefault();
            if (!string.IsNullOrEmpty(Model.Remark))
            {
                string str = Model.Remark.ToFormatLike();
                DataTable dt = new FT_FolderB().GetDTByCommand("SELECT ID FROM FT_Folder WHERE ','+UploadaAuthUsers+','  like '%," + strUserName + ",%'  AND ID IN ('" + Model.Remark.ToFormatLike('-') + "')");
                if (dt.Rows.Count > 0)
                {
                    strReturn = "Y";
                }
            }
            return strReturn;
        }


        public void DelWDTree(int FolderID, int comId)
        {
            List<FoldFileItem> ListID = new List<FoldFileItem>();
            new FT_FolderB().GetWDTREE(FolderID, ref ListID, comId);
            foreach (FoldFileItem listitem in ListID)
            {
                if (listitem.Type == "file")
                {
                    new FT_FileB().Delete(d => d.ID == listitem.ID && d.ComId == comId);
                }
                else
                {
                    new FT_FolderB().Delete(d => d.ID == listitem.ID && d.ComId == comId);
                }
            }

        }



        public class FoldFile
        {
            public int FolderID { get; set; }
            public string Name { get; set; }
            public string CRUser { get; set; }
            public int PFolderID { get; set; }
            public string Remark { get; set; }

            public List<FoldFile> SubFolder { get; set; }
            public List<FT_File> SubFileS { get; set; }

        }
        public class FoldFileItem
        {
            public int ID { get; set; }
            public string Type { get; set; }

        }
    }
    public class FT_FileB : BaseEFDao<FT_File>
    {
        public void AddVersion(FT_File oldmodel, string strMD5, string strSIZE)
        {
            FT_File_Vesion Vseion = new FT_File_Vesion();
            Vseion.FileMD5 = oldmodel.FileMD5;
            Vseion.RFileID = oldmodel.ID;
            new FT_File_VesionB().Insert(Vseion);
            //添加新版本

            oldmodel.FileVersin = oldmodel.FileVersin + 1;
            oldmodel.FileMD5 = strMD5;
            oldmodel.FileSize = strSIZE;
            new FT_FileB().Update(oldmodel);
            //修改原版本

        }




        /// <summary>
        /// 判断同一目录下是否有相同文件(不判断应用文件夹)
        /// </summary>
        /// <param name="strMD5"></param>
        /// <param name="strFileName"></param>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public FT_File GetSameFile(string strFileName, int FolderID, int ComId)
        {
            int[] folders = { 1, 2, 3 };
            if (!folders.Contains(FolderID))
            {
                return new FT_FileB().GetEntities(d => d.ComId == ComId && (d.Name + "." + d.FileExtendName) == strFileName && d.FolderID == FolderID).FirstOrDefault();
            }
            return null;

        }

        /// <summary>
        /// 获取文件在服务器上的预览文件路径
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>


        /// <summary>
        /// 更新企业空间占用
        /// </summary>
        /// <param name="FileSize"></param>
        /// <returns></returns>
        public int AddSpace(int ComId, int FileSize)
        {
            JH_Auth_QY qymodel = new JH_Auth_QYB().GetEntity(d => d.ComId == ComId);
            if (qymodel != null)
            {
                qymodel.QyExpendSpace = qymodel.QyExpendSpace + FileSize;
            }
            new JH_Auth_QYB().Update(qymodel);
            return qymodel.QyExpendSpace.Value;
        }


        public string ProcessWxIMG(string mediaIds, string strCode, JH_Auth_UserB.UserInfo UserInfo, string strType = ".jpg")
        {
            try
            {
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                string ids = "";
                foreach (var mediaId in mediaIds.Split(','))
                {
                    string fileToUpload = wx.GetMediaFile(mediaId, strType);

                    string md5 = CommonHelp.PostFile(UserInfo.QYinfo, fileToUpload);
                    System.IO.FileInfo f = new FileInfo(fileToUpload);
                    FT_File newfile = new FT_File();
                    newfile.ComId = UserInfo.User.ComId;
                    newfile.Name = f.Name;
                    newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                    newfile.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                    newfile.FileSize = f.Length.ToString();
                    newfile.FileVersin = 0;
                    newfile.CRDate = DateTime.Now;
                    newfile.CRUser = UserInfo.User.UserName;
                    newfile.UPDDate = DateTime.Now;
                    newfile.UPUser = UserInfo.User.UserName;
                    newfile.FolderID = 3;
                    newfile.FileExtendName = f.Extension.Substring(1);
                    newfile.ISYL = "Y";
                    new FT_FileB().Insert(newfile);

                    if (ids == "")
                    {
                        ids = newfile.ID.ToString();
                    }
                    else
                    {
                        ids += "," + newfile.ID.ToString();
                    }
                }

                return ids;
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG(ex.ToString());
                return "";
            }
        }




        public void ADDFILE(Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                CommonHelp.WriteLOG("调用上传文件接口" + P1);

                JArray Files = (JArray)JsonConvert.DeserializeObject(P1);
                var date = DateTime.Now;
                List<FT_File> ListData = new List<FT_File>();

                List<FT_File> ListSameData = new List<FT_File>();//重名文件

                foreach (var item in Files)
                {
                    int index = item["filename"].ToString().LastIndexOf('.');
                    string filename = item["filename"].ToString().Substring(0, index);
                    string md5 = item["md5"].ToString();
                    string zyid = item["zyid"].ToString();

                    FT_File File = new FT_FileB().GetSameFile(item["filename"].ToString(), int.Parse(P2), UserInfo.User.ComId.Value);
                    if (File == null)//相同目录下没有重复文件
                    {
                        FT_File newfile = new FT_File();
                        newfile.Name = filename;
                        newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                        newfile.zyid = zyid;
                        newfile.FileSize = item["filesize"].ToString();
                        newfile.FileVersin = 0;
                        newfile.CRDate = date;
                        newfile.CRUser = UserInfo.User.UserName;
                        newfile.UPDDate = date;
                        newfile.ComId = UserInfo.User.ComId;
                        newfile.UPUser = UserInfo.User.UserName;
                        newfile.FolderID = int.Parse(P2);
                        newfile.FileExtendName = item["filename"].ToString().Substring(index + 1).ToLower();
                        if (new List<string>() { "txt", "html", "doc", "mp4", "flv", "ogg", "jpg", "gif", "png", "bmp", "jpeg" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            newfile.ISYL = "Y";
                        }
                        if (new List<string>() { "pdf", "doc", "docx", "ppt", "pptx", "xls", "xlsx" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            newfile.ISYL = "Y";
                            //newfile.YLUrl = UserInfo.QYinfo.FileServerUrl + "/document/YL/" + newfile.zyid;
                            newfile.YLUrl = "/ViewV5/Base/doc.html?zyid=" + newfile.zyid;

                        }
                        if (new List<string>() { "mp4" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            string FileUrl = UserInfo.QYinfo.FileServerUrl + "/" + UserInfo.QYinfo.QYCode + "/document/" + newfile.zyid;
                            AliyunHelp.CopyUrlToOSS(FileUrl, zyid, "mp4");

                        }

                        ListData.Add(newfile);
                    }
                    else
                    {
                        FT_File_Vesion Vseion = new FT_File_Vesion();
                        Vseion.RFileID = File.ID;
                        Vseion.FileSize = File.FileSize;
                        Vseion.CRDate = date;
                        Vseion.CRUser = UserInfo.User.UserName;
                        new FT_File_VesionB().Insert(Vseion);//加入新版本

                        File.FileVersin = File.FileVersin + 1;
                        File.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                        File.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                        File.FileSize = item["filesize"].ToString();
                        File.UPDDate = date;
                        File.UPUser = UserInfo.User.UserName;
                        new FT_FileB().Update(File);//修改新版本
                                                    //修改原版本
                        ListSameData.Add(File);
                    }
                }
                foreach (FT_File item in ListData)
                {
                    new FT_FileB().Insert(item);
                    int filesize = 0;
                    int.TryParse(item.FileSize, out filesize);
                    new FT_FileB().AddSpace(UserInfo.User.ComId.Value, filesize);
                }




                msg.Result = ListData;
                msg.Result1 = ListSameData;
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG("调用上传文件接口出错" + ex.Message.ToString());

            }



        }

    }



    public class FT_File_DownhistoryB : BaseEFDao<FT_File_Downhistory>
    {

    }


    public class FT_File_ShareB : BaseEFDao<FT_File_Share>
    {

    }


    public class FT_File_UserAuthB : BaseEFDao<FT_File_UserAuth>
    {

    }


    public class FT_File_UserTagB : BaseEFDao<FT_File_UserTag>
    {

    }

    public class FT_File_VesionB : BaseEFDao<FT_File_Vesion>
    {

    }

    #endregion


    #region 综合办公

    public class JH_Auth_TLB : BaseEFDao<JH_Auth_TL>
    {
        public DataTable GetTL(string strMsgType, string MSGTLYID)
        {
            DataTable dtList = new DataTable();
            dtList = new JH_Auth_TLB().GetDTByCommand("  SELECT *  FROM JH_Auth_TL WHERE MSGType='" + strMsgType + "' AND  MSGTLYID='" + MSGTLYID + "'");
            dtList.Columns.Add("FileList", Type.GetType("System.Object"));
            foreach (DataRow dr in dtList.Rows)
            {
                if (dr["MSGisHasFiles"] != null && dr["MSGisHasFiles"].ToString() != "")
                {
                    int[] fileIds = dr["MSGisHasFiles"].ToString().SplitTOInt(',');
                    dr["FileList"] = new FT_FileB().GetEntities(d => fileIds.Contains(d.ID));
                }
            }
            return dtList;
        }
    }

    public class SZHL_XXFBTypeB : BaseEFDao<SZHL_XXFBType>
    {
    }
    public class SZHL_XXFBB : BaseEFDao<SZHL_XXFB>
    {
    }
    public class SZHL_XXFB_ITEMB : BaseEFDao<SZHL_XXFB_ITEM>
    {
    }








    public class JH_Auth_CommonB : BaseEFDao<JH_Auth_Common>
    {
    }

    public class SZHL_TXSXB : BaseEFDao<SZHL_TXSX>
    {
    }
    public class SZHL_DXGLB : BaseEFDao<SZHL_DXGL>
    {

        public string SendSMS(string telephone, string content, int ComId = 0)
        {
            string err = "";
            try
            {
                string dxqz = "企捷科技";
                decimal amcountmoney = 0;
                var qy = new JH_Auth_QYB().GetEntity(d => d.ComId == ComId);
                if (qy != null)
                {
                    dxqz = qy.DXQZ;
                    amcountmoney = qy.AccountMoney.HasValue ? qy.AccountMoney.Value : 0;
                }

                string[] tels = telephone.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace('，', ',').Split(',');

                //判断金额是否够
                decimal DXCost = decimal.Parse(CommonHelp.GetConfig("DXCost"));
                decimal amount = tels.Length * DXCost;
                if (ComId != 0 && amcountmoney < amount) //短信余额不足
                {
                    err = "短信余额不足";
                }
                else
                {
                    string re = "";
                    foreach (string tel in tels)
                    {
                        re = CommonHelp.SendDX(tel, content + "【" + dxqz + "】", "");
                    }

                    err = "发送成功";

                    //扣款
                    if (ComId != 0 && qy != null)
                    {
                        qy.AccountMoney = qy.AccountMoney - amount;
                        new JH_Auth_QYB().Update(qy);


                    }

                }
            }
            catch { }
            return err;
        }

    }
    public class SZHL_TXLB : BaseEFDao<SZHL_TXL>
    {
    }

    public class SZHL_XXFB_SCKB : BaseEFDao<SZHL_XXFB_SCK>
    {
    }

    public class SZHL_NOTEB : BaseEFDao<SZHL_NOTE>
    {
    }




    public class SZHL_DRAFTB : BaseEFDao<SZHL_DRAFT> { }








    #endregion
}
