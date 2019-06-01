using Aspose.Words;
using FastReflectionLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using Senparc.NeuChar.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace QJY.API
{
    public class FORMBIManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(FORMBIManage).GetMethod(msg.Action.ToUpper());
            FORMBIManage model = new FORMBIManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }






        #region 流程设置相关

        /// <summary>
        /// 获取流程列表 P1==""流程设置列表，P1!="" 自定义流程添加选择列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWFPDLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (P1 != "")
            {
                //授权管理用户，角色能看到的表单数据
                int lcs = int.Parse(P1);
                string strSql = string.Format(@"SELECT * from Yan_WF_PD  where   lcstatus='{0}' and ComId={1} and  IsSuspended= 'Y'  and ','+ManageUser+',' like '%,{2},%'", lcs, UserInfo.User.ComId, UserInfo.User.UserName);

                string strRoleSQL = "";
                foreach (var item in UserInfo.UserRoleCode.Split(','))
                {
                    strRoleSQL = strRoleSQL + string.Format(@"SELECT * from Yan_WF_PD  where   lcstatus='{0}' and ComId={1} and  IsSuspended= 'Y'  and ','+ManageRole+',' like '%,{2},%'", lcs, UserInfo.User.ComId, item);
                    strRoleSQL = strRoleSQL + "  UNION  ";
                }
                if (strRoleSQL.Length > 5)
                {
                    strRoleSQL = strRoleSQL.TrimEnd();
                    strRoleSQL = strRoleSQL.Substring(0, strRoleSQL.Length - 5);
                    strSql = strSql + " UNION " + strRoleSQL;
                }
                DataTable dtData = new Yan_WF_PDB().GetDTByCommand(strSql);
                msg.Result = dtData;

            }
            else
            {
                //授权管理用户能看到的表单模板
                string strWhere = " ( wfpd.CRUser='" + UserInfo.User.UserName + "' OR wfpd.SQUser='" + UserInfo.User.UserName + "') and wfpd.ComId=" + UserInfo.User.ComId;
                string strContent = context.Request["Content"] ?? "";
                strContent = strContent.TrimEnd();
                if (strContent != "")
                {
                    strWhere += string.Format(" And ( wfpd.ProcessName like '%{0}%' )", strContent);
                }
                string strLB = context.Request["LB"] ?? "";
                strLB = strLB.TrimEnd();
                if (strLB != "")
                {
                    strWhere += string.Format(" And ( wfpd.ProcessClass like '%{0}%' )", strLB);
                }
                string strSql = string.Format(@"SELECT DISTINCT wfpd.ProcessClass, wfpd.ProcessName,wfpd.ManageUser,wfpd.ID,count(wfpi.ID) formCount,wfpd.lcstatus,wfpd.IsSuspended from Yan_WF_PD wfpd LEFT join Yan_WF_PI wfpi on wfpd.ID=wfpi.PDID where   wfpd.isTemp='1' and  {0} group by  wfpd.ProcessClass, wfpd.ProcessName,wfpd.ID,wfpd.lcstatus,wfpd.IsSuspended,wfpd.ManageUser", strWhere, UserInfo.User.UserName);
                msg.Result = new Yan_WF_PDB().GetDTByCommand(strSql);
            }
        }

        /// <summary>
        /// 获取流程的具体步骤
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETTDLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            msg.Result = new Yan_WF_TDB().GetEntities(d => d.ProcessDefinitionID == Id).OrderBy(d => d.Taskorder);
        }
        /// <summary>
        /// 流程审批添加
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDPROCESS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            Yan_WF_PD lcsp = JsonConvert.DeserializeObject<Yan_WF_PD>(P1);
            if (lcsp.ProcessName.Trim() == "")
            {
                msg.ErrorMsg = "流程名称不能为空";
                return;
            }
            if (lcsp.ID == 0)//如果Id为0，为添加操作
            {
                if (new Yan_WF_PDB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.ProcessName == lcsp.ProcessName).Count() > 0)
                {
                    msg.ErrorMsg = "已存在此流程";
                    return;
                }

                lcsp.lcstatus = 1;
                lcsp.ComId = UserInfo.User.ComId;
                if (lcsp.ManageRole != null)
                {
                    lcsp.ManageRole.Trim(',');
                }
                lcsp.CRDate = DateTime.Now;
                lcsp.CRUser = UserInfo.User.UserName;
                lcsp.CRUserName = UserInfo.User.UserRealName;

                new Yan_WF_PDB().Insert(lcsp); //添加流程表数据
            }
            else
            {
                //修改流程表数据
                new Yan_WF_PDB().Update(lcsp);
            }
            //如果流程类型为固定流程并且固定流程内容不为空，添加固定流程数据
            if (lcsp.ProcessType == "1" && !string.IsNullOrEmpty(P2))
            {
                List<Yan_WF_TD> tdList = JsonConvert.DeserializeObject<List<Yan_WF_TD>>(P2);
                tdList.ForEach(d => d.ProcessDefinitionID = lcsp.ID);
                tdList.ForEach(d => d.ComId = UserInfo.User.ComId);
                tdList.ForEach(d => d.CRDate = DateTime.Now);
                tdList.ForEach(d => d.CRUser = UserInfo.User.UserName);
                tdList.ForEach(d => d.TDCODE = d.ProcessDefinitionID + "-" + d.Taskorder);
                tdList.ForEach(d => d.AssignedRole = d.AssignedRole.Trim(','));
                new Yan_WF_TDB().Delete(d => d.ProcessDefinitionID == tdList[0].ProcessDefinitionID);

                List<string> ExtendModes = new List<string>();

                ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == lcsp.ID).Select(D => D.TableFiledColumn).ToList();
                tdList[0].WritableFields = ExtendModes.ListTOString(',');
                new Yan_WF_TDB().Insert(tdList);

            }
            msg.Result = lcsp;
        }
        /// <summary>
        /// 获取流程信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETPROCESSBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            if (PD != null)
            {
                msg.Result = PD;
                if (!string.IsNullOrEmpty(PD.Files))
                {
                    msg.Result1 = new FT_FileB().GetDTByCommand("SELECT * FROM FT_File WHERE ID IN (" + PD.Files + ") ");
                }
            }

        }
        /// <summary>
        /// 删除流程信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELPROCESSBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {

                if (new Yan_WF_PDB().Delete(d => d.ID.ToString() == P1))
                {
                    msg.ErrorMsg = "";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        /// <summary>
        /// 禁用或启用流程审批类别
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void MODIFYLCSTATE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strSQL = string.Format("Update Yan_WF_PD set IsSuspended='{0}' where Id={1}", P1, P2);
                new Yan_WF_PDB().ExsSql(strSQL);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }





        /// <summary>
        /// 获取流程类别数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETLCBDLB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format(" SELECT DISTINCT ProcessClass FROM  Yan_WF_PD WHERE  ( CRUser='" + UserInfo.User.UserName + "' OR  SQUser='" + UserInfo.User.UserName + "') and ComId={0} and ProcessClass!='' and ProcessClass is not null  ", UserInfo.User.ComId);
            msg.Result = new Yan_WF_PDB().GetDTByCommand(strSql);
            msg.Result1 = new JH_Auth_RoleB().GetALLEntities();

        }


        #endregion




        #region 流程管理相关


        //待审核统计
        public void GETMODELDSHQTY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new Yan_WF_PIB().GetDSH(UserInfo.User).Count;

        }



        /// <summary>
        /// 获取流程数据添加页面
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWFDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int PDID = int.Parse(P1);
                if (PDID > 0)
                {
                    Yan_WF_PD pdmodel = new Yan_WF_PDB().GetEntity(d => d.ID == PDID);
                    if (pdmodel != null)
                    {
                        var dtList = new Yan_WF_TDB().GetEntities(d => d.ProcessDefinitionID == pdmodel.ID).OrderBy(d => d.Taskorder);
                        msg.Result = dtList;
                        msg.Result1 = pdmodel;
                        if (!string.IsNullOrEmpty(pdmodel.Files))
                        {
                            msg.Result2 = new FT_FileB().GetDTByCommand("SELECT * FROM FT_File WHERE ID IN (" + pdmodel.Files + ") ");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }



        public void GETMANGWFDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int PIID = int.Parse(P1);
                if (PIID > 0)
                {
                    Yan_WF_PI PIMODEL = new Yan_WF_PIB().GetEntity(d => d.ID == PIID);

                    if (PIMODEL == null)
                    {
                        msg.ErrorMsg = "流程数据已清除";
                        return;
                    }
                    else
                    {
                        DataTable dtList = new Yan_WF_TDB().GetEntities(d => d.ProcessDefinitionID == PIMODEL.PDID.Value).OrderBy(d => d.Taskorder).ToDataTable();
                        dtList.Columns.Add("userrealname");
                        dtList.Columns.Add("EndTime");
                        dtList.Columns.Add("TaskUserView");
                        dtList.Columns.Add("state");

                        foreach (DataRow dr in dtList.Rows)
                        {
                            string tdCode = dr["TDCODE"].ToString();
                            Yan_WF_TI tiModel = new Yan_WF_TIB().GetEntity(d => d.PIID == PIID && d.TDCODE == tdCode && d.EndTime != null);//
                            if (tiModel != null)
                            {
                                dr["userrealname"] = new JH_Auth_UserB().GetUserRealName(UserInfo.QYinfo.ComId, tiModel.TaskUserID);
                                dr["EndTime"] = tiModel.EndTime;
                                dr["TaskUserView"] = tiModel.TaskUserView;
                                dr["state"] = tiModel.TaskState;
                            }
                        }
                        msg.Result = dtList;

                        Yan_WF_PD pdmodel = new Yan_WF_PDB().GetEntity(d => d.ID == PIMODEL.PDID);
                        msg.Result1 = pdmodel;
                        msg.Result2 = "{ \"ISCANSP\":\"" + new Yan_WF_PIB().isCanSP(UserInfo.User.UserName, int.Parse(P1)) + "\",\"ISCANCEL\":\"" + new Yan_WF_PIB().isCanCancel(UserInfo.User.UserName, PIMODEL) + "\"}";
                        msg.Result3 = PIMODEL;//可修改字段
                        if (!string.IsNullOrEmpty(pdmodel.Files))
                        {
                            msg.Result4 = new FT_FileB().GetDTByCommand("SELECT * FROM FT_File WHERE ID IN (" + pdmodel.Files + ") ");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        public void CANCELWF(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strMode = context.Request["ModelCode"].ToString();



            int PIID = 0;
            if (!int.TryParse(P1, out PIID))
            {
                msg.ErrorMsg = "数据错误";
                return;
            }
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == PIID);
            string strISCanCel = new Yan_WF_PIB().isCanCancel(UserInfo.User.UserName, PI);
            if (strISCanCel == "N")
            {
                msg.ErrorMsg = "该表单已处理完毕,您无法再进行撤回操作";
                return;
            }
            //删除流程相关数据
            new Yan_WF_PIB().Delete(d => d.ID == PIID);
            new Yan_WF_TIB().Delete(d => d.PIID == PIID);

            //删除表单数据
        }

        /// <summary>
        /// 对流程待处理人员发送提醒（催办）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SENDLCCB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            var CBResult = new Yan_WF_TIB().GetEntities(d => d.PIID == Id && d.TaskState == 0);
            foreach (Yan_WF_TI item in CBResult)
            {
                SZHL_TXSX MODEL = new SZHL_TXSXB().GetEntity(d => d.TXUser == item.TaskUserID && d.intProcessStanceid == item.PIID);
                if (MODEL != null)
                {
                    MODEL.Status = "0";
                    new SZHL_TXSXB().Update(MODEL);
                }
            }
        }

        public void MANAGEWF(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strShUser = context.Request["SHUser"] ?? "";
                string strCSUser = context.Request["csr"] ?? "";
                string modelcode = context.Request["formcode"] ?? "";
                string strContent = context.Request["content"] ?? "";

                int PID = int.Parse(P1);




                Yan_WF_PIB PIB = new Yan_WF_PIB();
                if (PIB.isCanSP(UserInfo.User.UserName, PID) == "Y")//先判断用户能不能处理此流程
                {

                    List<string> ListNextUser = new List<string>();
                    PIB.MANAGEWF(UserInfo.User.UserName, PID, P2, ref ListNextUser, strShUser);//处理任务
                    Yan_WF_PI PI = PIB.GetEntity(d => d.ID == PID);
                    //更新抄送人
                    PI.ChaoSongUser = strCSUser;
                    PIB.Update(PI);
                    //更新抄送人

                    Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == PI.PDID.Value);

                    string content = PI.CRUserName + "发起了" + PD.ProcessName + "表单,等待您审阅";
                    string strTXUser = ListNextUser.ListTOString(',');
                    string funName = "LCSP_CHECK";
                    //添加消息提醒
                    string strIsComplete = ListNextUser.Count() == 0 ? "Y" : "N";//结束流程,找不到人了
                    if (strIsComplete == "Y")//找不到下家就结束流程,并且给流程发起人发送消息
                    {
                        PIB.ENDWF(PID);
                        msg.Result = "Y";//已结束
                        content = UserInfo.User.UserRealName + "审批完成了您发起的" + PD.ProcessName + "表单";
                        strTXUser = PI.CRUser;
                        funName = "LCSP_CHECK";
                        //发送消息给抄送人 
                        if (!string.IsNullOrEmpty(PI.ChaoSongUser))
                        {
                            SZHL_TXSX CSTX = new SZHL_TXSX();
                            CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            CSTX.APIName = "FORMBI";
                            CSTX.ComId = UserInfo.User.ComId;
                            CSTX.FunName = "LCSP_CHECK";
                            CSTX.intProcessStanceid = PID;
                            CSTX.CRUserRealName = UserInfo.User.UserRealName;
                            CSTX.MsgID = PID.ToString();
                            CSTX.TXContent = new JH_Auth_UserB().GetEntity(p => p.ComId == PI.ComId && p.UserName == PI.CRUser).UserRealName + "抄送一个" + PD.ProcessName + "，请您查阅接收";
                            CSTX.ISCS = "Y";
                            CSTX.TXUser = PI.ChaoSongUser;
                            CSTX.TXMode = modelcode;
                            CSTX.CRUser = UserInfo.User.UserName;
                            TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间
                        }
                    }
                    SZHL_TXSX TX = new SZHL_TXSX();
                    TX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    TX.APIName = "FORMBI";
                    TX.ComId = UserInfo.User.ComId;
                    TX.FunName = funName;
                    TX.intProcessStanceid = PID;
                    TX.CRUser = PI.CRUser;
                    TX.CRUserRealName = UserInfo.User.UserRealName;
                    TX.MsgID = PID.ToString();
                    TX.TXContent = content;
                    TX.TXUser = strTXUser;
                    TX.TXMode = modelcode;
                    TXSX.TXSXAPI.AddALERT(TX); //时间为发送时间
                }
                else
                {
                    msg.ErrorMsg = "该流程已被处理,您已无法处理此流程";
                }

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }




        /// <summary>
        /// 退回当前流程
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void REBACKWF(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int PID = int.Parse(P1);
                Yan_WF_PIB PIB = new Yan_WF_PIB();
                if (PIB.isCanSP(UserInfo.User.UserName, PID) == "Y")//先判断用户能不能处理此流程
                {
                    new Yan_WF_PIB().REBACKLC(UserInfo.User.UserName, PID, P2);//结束任务
                    string ModeCode = context.Request["formcode"] ?? "LCSP";

                    if (!string.IsNullOrEmpty(ModeCode))
                    {
                        Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == PID);


                        //消息提醒
                        SZHL_TXSX TX = new SZHL_TXSX();
                        TX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        TX.APIName = "FORMBI";
                        TX.ComId = UserInfo.User.ComId;
                        TX.FunName = "LCSP_CHECK";
                        TX.intProcessStanceid = PID;
                        TX.CRUserRealName = UserInfo.User.UserRealName;
                        TX.MsgID = PID.ToString();
                        TX.TXContent = UserInfo.User.UserRealName + "退回了" + new Yan_WF_PDB().GetEntity(d => d.ID == PI.PDID.Value).ProcessName + "，请您查阅";
                        TX.TXUser = PI.CRUser;
                        TX.TXMode = ModeCode;
                        TX.CRUser = UserInfo.User.UserName;
                        TXSX.TXSXAPI.AddALERT(TX); //时间为发送时间
                    }
                }
                else
                {
                    msg.ErrorMsg = "该流程已被处理,您已无法处理此流程";
                }





            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }




        /// <summary>
        /// 开始流程
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">启动流程的应用Code</param>
        /// <param name="P2">审核人信息</param>
        /// <param name="UserInfo"></param>
        public void STARTWF(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                string strModelCode = P1;
                string strCSR = context.Request["csr"] ?? "";
                string strZSR = context.Request["zsr"] ?? "";
                string strContent = context.Request["content"] ?? "";



                int PDID = 0;
                int.TryParse(context.Request["PDID"] ?? "0", out PDID);
                Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == PDID && d.ComId == UserInfo.User.ComId);

                if (PD == null)
                {
                    //没有流程,直接返回
                    return;
                }
                Yan_WF_PIB PIB = new Yan_WF_PIB();
                List<string> ListNextUser = new List<string>();//获取下一任务的处理人
                Yan_WF_PI PI = new Yan_WF_PI();
                PI.CRUserName = UserInfo.User.UserRealName;
                PI.BranchName = UserInfo.BranchInfo.DeptName;
                PI.BranchNO = UserInfo.BranchInfo.DeptCode;
                PI.Content = strContent;
                PI.PDID = PD.ID;
                PI.WFFormNum = new Yan_WF_PIB().CreateWFNum(PI.PDID.ToString());
                PI.ComId = PD.ComId;
                PI.StartTime = DateTime.Now;
                PI.CRUser = UserInfo.User.UserName;
                PI.CRDate = DateTime.Now;
                PI.PITYPE = PD.ProcessType;
                PI.ChaoSongUser = strCSR;
                PI.isGD = "N";
                new Yan_WF_PIB().Insert(PI);



                Yan_WF_TI TI = PIB.StartWF(PD, strModelCode, UserInfo.User.UserName, strZSR, strCSR, PI, ref ListNextUser);

                //返回新增的任务
                msg.Result = PI;
                msg.Result1 = TI;


                //发送消息给审核人员
                string jsr = ListNextUser.ListTOString(',');
                if (!string.IsNullOrEmpty(jsr))
                {
                    SZHL_TXSX TX = new SZHL_TXSX();
                    TX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    TX.APIName = "FORMBI";
                    TX.ComId = UserInfo.User.ComId;
                    TX.FunName = "LCSP_CHECK";
                    TX.intProcessStanceid = TI.PIID;
                    TX.CRUserRealName = UserInfo.User.UserRealName;
                    TX.MsgID = TI.PIID.ToString();
                    TX.TXContent = UserInfo.User.UserRealName + "发起了一个" + PD.ProcessName + "，请您查阅审核";
                    TX.TXUser = jsr;
                    TX.TXMode = strModelCode;
                    TX.CRUser = UserInfo.User.UserName;
                    TXSX.TXSXAPI.AddALERT(TX); //时间为发送时间
                }
                //发送消息
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        public void SAVEPIDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int pId = int.Parse(P1);
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == pId);
            //更新抄送人
            PI.Content = P2;
            new Yan_WF_PIB().Update(PI);

        }


        /// <summary>
        /// 获取新增单据得流水号(可能存在跳号)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void CRWFNUM(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string pdId = P1;
            msg.Result = new Yan_WF_PIB().CreateWFNum(pdId);
        }


        #endregion


        #region 流程中的消息处理方法
        public void LCSP_CHECK(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_TXSX TX = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);

            //todo 需要根据PID找到对应的数据ID
            Article ar0 = new Article();
            ar0.Title = TX.TXContent;
            ar0.Description = "";
            ar0.Url = TX.MsgID;
            List<Article> al = new List<Article>();
            al.Add(ar0);
            JH_Auth_UserB.UserInfo UserTXInfo = new JH_Auth_UserB().GetUserInfo(TX.ComId.Value, TX.CRUser);
            if (!string.IsNullOrEmpty(TX.TXUser))
            {
                try
                {
                    //发送PC消息
                    new JH_Auth_User_CenterB().SendMsg(UserTXInfo, TX.TXMode, TX.TXContent, TX.MsgID, TX.TXUser, "A", TX.intProcessStanceid, TX.ISCS);
                }
                catch (Exception)
                {
                }

                //发送微信消息
                WXHelp wx = new WXHelp(UserTXInfo.QYinfo);
                wx.SendTH(al, TX.TXMode, "A", TX.TXUser);
            }

        }

        #endregion


        #region 表单数据相关

        /// <summary>
        /// 流程审批列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETLCSPLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            string strWhere = " 1=1 and pi.ComId=" + UserInfo.User.ComId;

            string leibie = context.Request["lb"] ?? "";
            if (leibie != "")
            {
                strWhere += string.Format(" And pi.PDID='{0}' ", leibie);
                int pdid = int.Parse(leibie);
                msg.Result2 = new Yan_WF_PDB().GetEntity(d => d.ID == pdid);
            }

            int DataID = -1;
            int.TryParse(context.Request["ID"] ?? "-1", out DataID);//记录Id
            if (DataID != -1)
            {
                strWhere += string.Format(" And pi.ID = '{0}'", DataID);
            }

            if (P1 != "")
            {
                int page = 0;
                int pagecount = 8;
                int.TryParse(context.Request["p"] ?? "1", out page);
                int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
                page = page == 0 ? 1 : page;
                int total = 0;

                DataTable dt = new DataTable();

                switch (P1)
                {
                    case "1": //创建
                        {
                            strWhere += " And pi.CRUser ='" + userName + "'";
                        }

                        break;
                    case "2": //待审核
                        {
                            List<string> intProD = new Yan_WF_PIB().GetDSH(UserInfo.User).Select(d => d.PIID.ToString()).ToList();
                            if (intProD.Count > 0)
                            {
                                strWhere += " And pi.ID in (" + (intProD.ListTOString(',') == "" ? "0" : intProD.ListTOString(',')) + ")";
                            }
                            else
                            {
                                strWhere += " and 1=0 ";
                            }
                        }
                        break;
                    case "3": //已审核
                        {
                            var intProD = new Yan_WF_PIB().GetYSH(UserInfo.User).Select(d => d.PIID.ToString()).ToList();
                            if (intProD.Count > 0)
                            {
                                strWhere += " And pi.ID  in (" + (intProD.ListTOString(',') == "" ? "0" : intProD.ListTOString(',')) + ")";
                            }
                            else
                            {
                                strWhere += " and 1=0 ";
                            }
                        }
                        break;

                    case "4": //抄送我的
                        {
                            strWhere += " AND pi.isComplete='Y'    AND   ',' + pi.ChaoSongUser  + ',' like '%," + userName + ",%'";

                        }
                        break;
                }
                dt = new Yan_WF_PIB().GetDataPager("Yan_WF_PI pi inner join Yan_WF_PD pd on pd.ID=pi.PDID ", "pi.*,pd.ProcessClass, pd.ProcessType,pd.ProcessName,'LCSP' as ModelCode", pagecount, page, " pi.CRDate desc", strWhere, ref total);
                dt.Columns.Add("StatusName");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int pid = int.Parse(dt.Rows[i]["ID"].ToString());
                    string strStatusName = "正在审批";
                    if (dt.Rows[i]["isComplete"].ToString() == "Y")
                    {
                        strStatusName = "已审批";
                    }
                    if (dt.Rows[i]["IsCanceled"].ToString() == "Y")
                    {
                        strStatusName = "已退回";
                    }
                    if (dt.Rows[i]["ProcessType"].ToString() == "-1")
                    {
                        strStatusName = "无流程数据";
                    }
                    dt.Rows[i]["StatusName"] = strStatusName;
                }
                msg.Result = dt;
                msg.Result1 = total;
            }
        }


        #endregion

        #region 表单处理

        /// <summary>
        /// 更新表单模板
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETPDTEMP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);


            string strFormop = context.Request["formop"] ?? "";
            string strfmdata = context.Request["fmdata"] ?? "";

            Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            PD.Tempcontent = P2.Replace("null", "''");
            PD.Poption = strFormop;
            PD.fmdata = strfmdata;
            JArray filds = JArray.Parse(P2);
            //保存模板得时候顺便保存扩展字段
            List<JH_Auth_ExtendMode> ListNew = new List<JH_Auth_ExtendMode>();
            foreach (JObject item in filds)
            {
                List<string> ListNofiled = new List<string>() { "qjLine" };
                string strComponentName = (string)item["component"];
                if (!ListNofiled.Contains(strComponentName))
                {
                    //有几个组件是不需要存到字段里得
                    JH_Auth_ExtendMode Model = new JH_Auth_ExtendMode();
                    Model.ComId = UserInfo.User.ComId;
                    Model.CRDate = DateTime.Now;
                    Model.CRUser = UserInfo.User.UserName;
                    Model.PDID = PD.ID;
                    Model.TableFiledColumn = ((string)item["wigdetcode"]).Trim();
                    Model.TableFiledName = ((string)item["title"]).Trim();
                    string strFileType = "Str";
                    if ((string)item["eltype"] == "number")
                    {
                        strFileType = "Num";

                    }
                    if ((string)item["eltype"] == "date" || (string)item["eltype"] == "datetime")
                    {
                        strFileType = "Date";
                    }
                    if ((string)item["eltype"] == "qjTable")
                    {
                        strFileType = "Table";
                    }
                    Model.TableFileType = strFileType;
                    Model.TableName = "LCSP";
                    ListNew.Add(Model);
                }

            }
            new JH_Auth_ExtendModeB().Delete(d => d.PDID == PD.ID);
            new JH_Auth_ExtendModeB().Insert(ListNew);

            new Yan_WF_PDB().Update(PD);
        }






        public void SAVEEXDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int piId = 0;
            int.TryParse(P1, out piId);
            int PDID = 0;
            int.TryParse(P2, out PDID);
            Yan_WF_PI lcsp = new Yan_WF_PIB().GetEntity(d => d.ID == piId);
            Yan_WF_PD pd = new Yan_WF_PDB().GetEntity(d => d.ID == PDID);



            JArray datas = (JArray)JsonConvert.DeserializeObject(lcsp.Content);
            var dt = new Dictionary<string, object>();
            dt.Add("CRUser", UserInfo.User.UserName);
            dt.Add("CRDate", DateTime.Now);
            dt.Add("ComID", UserInfo.User.ComId.Value);
            dt.Add("intProcessStanceid", piId);
            try
            {
                List<JH_Auth_ExtendData> ListNew = new List<JH_Auth_ExtendData>();
                foreach (JObject item in datas)
                {
                    JH_Auth_ExtendData Model = new JH_Auth_ExtendData();
                    Model.ComId = UserInfo.User.ComId;
                    Model.BranchNo = UserInfo.User.BranchCode;
                    Model.DataID = piId;
                    Model.PDID = PDID;
                    Model.TableName = "LCSP";
                    Model.ExFiledColumn = (string)item["wigdetcode"];
                    Model.ExFiledName = (string)item["title"];
                    string strValue = "";
                    strValue = (string)item["value"];
                    Model.ExtendDataValue = strValue.Trim(',');
                    Model.CRUser = UserInfo.User.UserName;
                    Model.CRDate = DateTime.Now;
                    Model.CRUserName = UserInfo.User.UserRealName;
                    Model.BranchNo = UserInfo.BranchInfo.DeptCode;
                    Model.BranchName = UserInfo.BranchInfo.DeptName;
                    ListNew.Add(Model);

                    string strglfiled = (string)item["glfiled"] ?? "";
                    if (strglfiled != "")
                    {
                        dt.Add(strglfiled, strValue);
                    }
                }
                new JH_Auth_ExtendDataB().Delete(d => d.DataID == piId && d.PDID == PDID);
                new JH_Auth_ExtendDataB().Insert(ListNew);

                if (!string.IsNullOrEmpty(pd.RelatedTable))
                {
                    int intcount = new Yan_WF_PDB().GetDTByCommand("SELECT * FROM " + pd.RelatedTable + " WHERE intProcessStanceid='" + piId.ToString() + "'").Rows.Count;
                    DBFactory db = new BI_DB_SourceB().GetDB(0);
                    if (intcount > 0)
                    {
                        //更新
                    }
                    else
                    {
                        //新增
                        db.InserData(dt, pd.RelatedTable);
                    }
                }

            }
            catch (Exception ex)
            {

            }



        }
        public void GETSQLDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new DataTable();
            string SQL = CommonHelp.Filter(P1);
            dt = new Yan_WF_PDB().GetDTByCommand(SQL);
            msg.Result = dt;
        }

        /// <summary>
        /// 获取表单内的可管理字段
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFORMFILED(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int pdid = 0;
            int.TryParse(P1, out pdid);
            int tdid = 0;
            int.TryParse(P2, out tdid);
            List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
            ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == pdid).ToList();
            msg.Result = ExtendModes;

            if (tdid != 0)
            {
                Yan_WF_TD TD = new Yan_WF_TDB().GetEntity(d => d.ID == tdid && d.ComId == UserInfo.User.ComId);
                msg.Result1 = TD.WritableFields;
            }


        }

        /// <summary>
        /// 更新表单可管理字段
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETPDFILED(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);
            Yan_WF_TD TD = new Yan_WF_TDB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            TD.WritableFields = P2;
            new Yan_WF_TDB().Update(TD);
        }




        /// <summary>
        /// 归档表单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GDFORM(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            P2 = (P2 == "" ? "N" : P2);
            if (!string.IsNullOrEmpty(P1))
            {
                new Yan_WF_PIB().GDForm(P2, UserInfo.User.ComId.Value, P1);
            }
        }

        /// <summary>
        /// 彻底删除已归档的数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELFORM(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (!string.IsNullOrEmpty(P1))
            {
                int[] IDS = P1.SplitTOInt(',');
                new Yan_WF_PIB().Delete(d => d.ComId == UserInfo.User.ComId && IDS.Contains(d.ID));
                new Yan_WF_TIB().Delete(d => d.ComId == UserInfo.User.ComId && IDS.Contains(d.PIID));
                new JH_Auth_ExtendDataB().Delete(d => d.ComId == UserInfo.User.ComId && IDS.Contains(d.DataID.Value));
                //是不是还需要删除关联表得数据
            }
        }

        /// <summary>
        /// 按照WORD导出模板导出数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void EXPORTWORD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int pdid = 0;
            int.TryParse(P1, out pdid);

            int piid = 0;
            int.TryParse(P2, out piid);
            Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == pdid && d.ComId == UserInfo.User.ComId);
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == piid && d.ComId == UserInfo.User.ComId);

            if (PD.ExportFile != null)
            {
                int fileID = int.Parse(PD.ExportFile);
                FT_File File = new FT_FileB().GetEntities(d => d.ID == fileID).FirstOrDefault();
                Dictionary<string, string> dictSource = new Dictionary<string, string>();

                List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
                ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == pdid).ToList();
                foreach (JH_Auth_ExtendMode item in ExtendModes)
                {
                    string strValue = new JH_Auth_ExtendDataB().GetFiledValue(item.TableFiledColumn, pdid, piid);
                    dictSource.Add("qj_" + item.TableFiledColumn, strValue);
                }

                dictSource.Add("qj_CRUser", PI.CRUserName);
                dictSource.Add("qj_BranchName", PI.BranchName);
                dictSource.Add("qj_CRDate", PI.CRDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                dictSource.Add("qj_PINUM", PI.ID.ToString());


                List<Yan_WF_TI> tiModels = new Yan_WF_TIB().GetEntities(d => d.PIID == piid).ToList();
                for (int i = 0; i < tiModels.Count; i++)
                {
                    dictSource.Add("qj_Task" + i + ".TaskUser", new JH_Auth_UserB().GetUserRealName(UserInfo.User.ComId.Value, tiModels[i].TaskUserID));
                    dictSource.Add("qj_Task" + i + ".TaskUserView", tiModels[i].TaskUserView);
                    if (tiModels[i].EndTime != null)
                    {
                        dictSource.Add("qj_Task" + i + ".EndTime", tiModels[i].EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
                string strFileUrl = UserInfo.QYinfo.FileServerUrl + "/" + UserInfo.QYinfo.QYCode + "/document/" + File.zyid;
                Aspose.Words.Document doc = new Aspose.Words.Document(strFileUrl);

                //使用文本方式替换
                foreach (string name in dictSource.Keys)
                {
                    doc.Range.Replace(name, dictSource[name], false, true);
                }

                #region 使用书签替换模式

                //Aspose.Words.Bookmark bookmark = doc.Range.Bookmarks["ACCUSER_SEX"];
                //if (bookmark != null)
                //{
                //    bookmark.Text = "男";
                //}
                //bookmark = doc.Range.Bookmarks["ACCUSER_TEL"];
                //if (bookmark != null)
                //{
                //    bookmark.Text = "1862029207*";
                //}

                #endregion
                string Filepath = HttpContext.Current.Request.MapPath("/") + "/Export/";
                string strFileName = PD.ProcessName + DateTime.Now.ToString("yyMMddHHss") + ".doc";

                doc.Save(Filepath + strFileName, Aspose.Words.Saving.DocSaveOptions.CreateSaveOptions(SaveFormat.Doc));
                context.Response.ContentType = "application/x-zip-compressed";
                context.Response.AddHeader("Content-type", "text/html;charset=UTF-8");
                context.Response.AddHeader("Content-Disposition", "attachment;filename=" + strFileName);
                //string filename = Server.MapPath("/" + attch.FileUrl);
                context.Response.TransmitFile(Filepath + strFileName);
                context.Response.End();

            }
        }

        #endregion

        #region 表单统计
        public void GETBDTJDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int pdid = 0;
            int.TryParse(P1, out pdid);


            string strSDate = context.Request["sdate"] ?? DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd");
            string strEDate = context.Request["edate"] ?? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");


            List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
            ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == pdid).ToList();
            string strWhere = "";
            if (P2 != "")
            {
                JArray datas = (JArray)JsonConvert.DeserializeObject(P2);
                foreach (JObject item in datas)
                {
                    string filed = (string)item["filed"];
                    if (ExtendModes.Select(D => D.TableFiledColumn).ToList().Contains(filed))
                    {
                        string qtype = (string)item["qtype"];
                        string qvalue = (string)item["qvalue"];
                        strWhere = CommonHelp.CreateqQsql(filed, qtype, qvalue);
                    }

                }
            }
            string strISGD = context.Request["isGD"] ?? "";
            if (strISGD != "")
            {
                strWhere = strWhere + " AND ISGD='" + strISGD.FilterSpecial() + "'";
            }


            string pdfields = context.Request["pdfields"] ?? "";
            if (pdfields != "")
            {
                ExtendModes = ExtendModes.Where(d => d.TableFiledColumn == pdfields).ToList();
            }
            if (ExtendModes.Count > 0)
            {
                string strSQL = " SELECT NEWID() AS ID, * FROM (  SELECT   Yan_WF_PI.ISGD,Yan_WF_PI.BranchNo,Yan_WF_PI.BranchName,JH_Auth_ExtendData.DataID, Yan_WF_PI.CRUser,Yan_WF_PI.CRUserName,  Yan_WF_PI.CRDate, JH_Auth_ExtendMode.TableFiledColumn,  ExtendDataValue  from JH_Auth_ExtendMode INNER JOIN JH_Auth_ExtendData ON JH_Auth_ExtendMode.PDID=JH_Auth_ExtendData.PDID and JH_Auth_ExtendMode.TableFiledColumn=JH_Auth_ExtendData.ExFiledColumn and JH_Auth_ExtendMode.PDID='" + pdid + "' INNER JOIN Yan_WF_PI  ON JH_Auth_ExtendData.DataID=Yan_WF_PI.ID  ) AS P PIVOT ( MAX(ExtendDataValue) FOR  p.TableFiledColumn  IN (" + ExtendModes.Select(D => D.TableFiledColumn).ToList().ListTOString(',') + ") ) AS T WHERE CRDATE BETWEEN '" + strSDate + " 01:01:01'  AND '" + strEDate + " 23:59:59' " + strWhere;
                DataTable dt = new Yan_WF_PDB().GetDTByCommand(strSQL);
                msg.Result = dt;
                msg.Result1 = ExtendModes;
            }

        }

        //导出表单数据EXCEL
        public void EXPORTBDTJDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int pdid = 0;
            int.TryParse(P1, out pdid);
            Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == pdid && d.ComId == UserInfo.User.ComId);

            GETBDTJDATA(context, msg, P1, P2, UserInfo);
            DataTable dt = msg.Result;

            string sqlCol = "CRUserName|发起人,CRDate|发起时间,BranchName|所在部门,";
            List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
            ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == pdid).ToList();
            foreach (JH_Auth_ExtendMode item in ExtendModes)
            {
                sqlCol = sqlCol + item.TableFiledColumn + "|" + item.TableFiledName + ",";
            }

            DataTable dtResult = dt.DelTableCol(sqlCol.TrimEnd(','));
            CommonHelp ch = new CommonHelp();
            msg.ErrorMsg = ch.ExportToExcel(PD.ProcessName, dtResult);
        }


        /// <summary>
        /// 表单监控数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETBDJKDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {



            string strSDate = context.Request["sdate"] ?? DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd");
            string strEDate = context.Request["edate"] ?? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            string strWhere = "";
            if (P1 != "")
            {
                int pdid = 0;
                int.TryParse(P1, out pdid);
                strWhere = "AND  Yan_WF_PI.PDID='" + pdid + "'";
            }
            if (P2 == "1")
            {
                strWhere = strWhere + "AND  Yan_WF_PI.CRUser='" + UserInfo.User.UserName + "'";
            }
            string strSQL = " SELECT Yan_WF_PI.*,Yan_WF_PD.ProcessName,Yan_WF_PD.ProcessClass,Yan_WF_PD.ProcessType FROM  Yan_WF_PI  INNER JOIN Yan_WF_PD ON Yan_WF_PI.PDID =Yan_WF_PD.ID  WHERE 1=1  " + strWhere + " AND Yan_WF_PI.CRDATE BETWEEN '" + strSDate + " 01:01:01'  AND '" + strEDate + " 23:59:59' ";
            DataTable dt = new Yan_WF_PIB().GetDTByCommand(strSQL);
            dt.Columns.Add("StatusName");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int pid = int.Parse(dt.Rows[i]["ID"].ToString());
                string strStatusName = "正在审批";
                if (dt.Rows[i]["isComplete"].ToString() == "Y")
                {
                    strStatusName = "已审批";
                }
                if (dt.Rows[i]["IsCanceled"].ToString() == "Y")
                {
                    strStatusName = "已退回";
                }
                if (dt.Rows[i]["ProcessType"].ToString() == "-1")
                {
                    strStatusName = "无流程数据";
                }
                dt.Rows[i]["StatusName"] = strStatusName;
            }
            msg.Result = dt;

        }

        #endregion


        #region 表单设计
        public void GETFORMFILEDS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {


            var PDS = new Yan_WF_PDB().GetEntities(D => D.ComId == UserInfo.User.ComId);

            JArray arrs = new JArray();

            foreach (var item in PDS)
            {
                List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
                ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == item.ID).ToList();
                JObject obj = JObject.FromObject(new
                {
                    value = item.ID,
                    label = item.ProcessName,
                    children =
                        from p in ExtendModes
                        select new
                        {
                            value = p.TableFiledColumn,
                            label = p.TableFiledName,

                        }
                });
                arrs.Add(obj);
            }
            msg.Result = arrs;

        }

        public void GETFIELDDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            DBFactory db = new BI_DB_SourceB().GetDB(0);

            //var dt = new Dictionary<string, object>();
            //dt.Add("ID", "6988");
            //dt.Add("Remark1", "123");
            //dt.Add("Remark2", "asdasd");
            //db.UpdateData(dt, "JH_Auth_ZiDian");
            DataTable dt = db.GetSQL(CommonHelp.Filter("SELECT TOP 1 * FROM " + P1));
            List<BI_DB_Dim> ListDIM = new BI_DB_SetB().getCType(dt);
            msg.Result = ListDIM;

        }



        #endregion
    }
}


