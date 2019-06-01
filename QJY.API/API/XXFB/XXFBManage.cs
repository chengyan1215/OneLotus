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
using Senparc.NeuChar.Entities;
using QJY.Common;


namespace QJY.API
{
    public class XXFBManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(XXFBManage).GetMethod(msg.Action.ToUpper());
            XXFBManage model = new XXFBManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        #region 微信专用
        /// <summary>
        /// 获取信息分类,
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">0:获取初始类别,1:获取子类别,2:获取全部类别</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETXXFBTYPELISTWX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT  Id,PTypeID pId,TypeName name,TypePath from SZHL_XXFBType where comId='{0}' and PTypeID='{1}' and isDel=0", UserInfo.User.ComId, P1);
            DataTable dt = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
            msg.Result = dt;
        }

        public void GETXXFBTYPELISTALL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT  ID,PTypeID,TypeName,TypePath from SZHL_XXFBType where comId='{0}' and PTypeID='0' and isDel=0 ", UserInfo.User.ComId);
            DataTable dt = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
            dt.Columns.Add("SubItem", typeof(DataTable));

            foreach (DataRow dr in dt.Rows)
            {
                string topid = dr["ID"].ToString();
                strSql = string.Format("SELECT  SZHL_XXFBType.ID,PTypeID,TypeName,TypePath,COUNT(SZHL_XXFB.ID) as datacount from SZHL_XXFBType INNER JOIN SZHL_XXFB on SZHL_XXFBType.ID=SZHL_XXFB.XXFBType where SZHL_XXFBType.comId='{0}' and PTypeID='{1}' and isDel=0 AND  (','+SZHL_XXFB.JSUser+',' LIKE '%,{2},%' or  SZHL_XXFB.JSUser='') GROUP by SZHL_XXFBType.ID,PTypeID,TypeName,TypePath", UserInfo.User.ComId, topid, UserInfo.User.UserName);

                dr["SubItem"] = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
            }
            msg.Result = dt;
        }

        //获取接收的发布信息,已审核过的信息
        public void GETXXFBLISTWX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);  //页码
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数

            string strWhere = string.Format("SZHL_XXFB.ComId='{0}'  and IsSend='1' and MsgType=1 and (((SHstatus='2' and   (','+SZHL_XXFB.JSUser+',' LIKE '%,{1},%' or  SZHL_XXFB.JSUser='')) and FBTime<=getdate()) or (SHStatus=0 and SHUser='{1}')or (SHStatus=1 and SZHL_XXFB.CRUser='{1}')) ", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" And (SZHL_XXFB.XXTitle like '%{0}%' )", P1);
            }
            if (P2 != "")
            {
                strWhere += string.Format(" and ( xxtype.Id={0} or TypePath like '{1}%') ", P2.Split('-').LastOrDefault(), P2);
            }
            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB inner join SZHL_XXFBType xxtype on XXFBType=xxtype.ID", " SZHL_XXFB.*,xxtype.TypeName", pagecount, page, "SHstatus asc,IsSend asc, FBTime desc", strWhere, ref recordCount);
            if (dt.Rows.Count > 0)
            {
                dt.Columns.Add("Item", Type.GetType("System.Object"));
                foreach (DataRow dr in dt.Rows)
                {
                    int xid = Int32.Parse(dr["ID"].ToString());
                    var list = new SZHL_XXFB_ITEMB().GetEntities(p => p.XXFBId == xid);
                    dr["Item"] = list;
                }
            }
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8);

        }

        //获取需要审核的信息
        public void GETXXFBLISTWXSH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);
            //
            string strWhere = string.Format("SZHL_XXFB.ComId='{0}'  and IsSend=1 and MsgType=1 and FBTime<=getdate() ", UserInfo.User.ComId);

            if (P1 == "1") //显示待发布列表
            {
                strWhere += string.Format(" and SZHL_XXFB.CRUser='{0}' ", UserInfo.User.UserName);
            }
            else //默认显示待审核列表
            {
                strWhere += string.Format(" and SZHL_XXFB.SHUser='{0}' ", UserInfo.User.UserName);
            }

            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB inner join SZHL_XXFBType xxtype on XXFBType=xxtype.ID", " SZHL_XXFB.*,xxtype.TypeName", 8, page, " FBTime desc", strWhere, ref recordCount);
            if (dt.Rows.Count > 0)
            {
                dt.Columns.Add("Item", Type.GetType("System.Object"));
                foreach (DataRow dr in dt.Rows)
                {
                    int xid = Int32.Parse(dr["ID"].ToString());
                    var list = new SZHL_XXFB_ITEMB().GetEntities(p => p.XXFBId == xid);
                    dr["Item"] = list;
                }
            }
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8);
        }
        //获取信息详情 PC微信共同使用
        public void GETXXFBMODELWX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_XXFB_ITEM model = new SZHL_XXFB_ITEMB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            if (model != null)
            {
                SZHL_XXFB xmodel = new SZHL_XXFBB().GetEntity(p => p.ID == model.XXFBId);
                xmodel.Remark = new SZHL_XXFBTypeB().GetEntity(p => p.ID == xmodel.XXFBType).TypeName;
                msg.Result = model;
                msg.Result1 = new SZHL_XXFB_ITEMB().GetDTByCommand("  SELECT ID,MSGTLYID,MSGType,MSGContent,CRDate,CRUser,CRUserName  FROM JH_Auth_TL WHERE MSGType='XXFB' AND  MSGTLYID='" + P1 + "'");
                if (!string.IsNullOrEmpty(model.Files))
                {
                    msg.Result2 = new FT_FileB().GetEntities(" ID in (" + model.Files + ")");
                }

                msg.Result3 = new JH_Auth_User_CenterB().GetEntities(d => d.DataId == P1 && d.MsgModeID == "xxfb" && d.isRead == 0).Select(d => d.UserTO);
                msg.Result4 = xmodel;
            }
            new JH_Auth_User_CenterB().ReadMsg(UserInfo, Id, "XXFB");

        }

        #endregion

        /// <summary>
        /// 信息发布ztree获取列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETXXFBTYPELIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("SELECT  Id,PTypeID pId,TypeName name,TypePath from SZHL_XXFBType where comId='{0}' and isDel!=1", UserInfo.User.ComId);
            DataTable dt = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
            msg.Result = dt;
        }
        /// <summary>
        /// 信息发布类别获取
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETXXFBTYPELISTPAGE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            int recordCount = 0;
            string strWhere = string.Format(" comId='{0}' and isDel!=1 and PTypeID!=0 ", UserInfo.User.ComId);
            string strContent = context.Request["Content"] ?? "";
            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                strWhere += string.Format(" And TypeName like '%{0}%' ", strContent);
            }

            DataTable dt = new SZHL_XXFBTypeB().GetDataPager("SZHL_XXFBType", " ID,TypeName,TypeDec,TypeManager,ISzjfb,CheckUser,SeeUser,IsCheck,IsSee,PTypeID ", pagecount, page, " CRDate desc", strWhere, ref recordCount);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ISzjfb"].ToString().ToLower() == "true")
                    {
                        dr["ISzjfb"] = "是";
                    }
                    else
                    {
                        dr["ISzjfb"] = "否";
                    }
                    if (dr["PTypeID"] != null && dr["PTypeID"].ToString() == "0")
                    {
                        dr["ISzjfb"] = "";
                        dr["CheckUser"] = "";
                        dr["SeeUser"] = "";
                        dr["IsCheck"] = "";
                        dr["IsSee"] = "";
                    }
                    else
                    {
                        if (dr["IsCheck"] != null && dr["IsCheck"].ToString().ToLower() == "false")
                        {
                            dr["ISzjfb"] = "";
                            dr["CheckUser"] = "";
                        }
                        if (dr["IsSee"] != null && dr["IsSee"].ToString().ToLower() == "false")
                        {
                            dr["SeeUser"] = "";
                        }

                        if (dr["IsCheck"].ToString().ToLower() == "true")
                        {
                            dr["IsCheck"] = "是";
                        }
                        else
                        {
                            dr["IsCheck"] = "否";
                        }
                        if (dr["IsSee"].ToString().ToLower() == "true")
                        {
                            dr["IsSee"] = "是";
                        }
                        else
                        {
                            dr["IsSee"] = "否";
                        }
                    }
                }
            }


            msg.Result = dt;
            msg.Result1 = recordCount;
        }
        //获取管理员所管理的类型列表
        public void GETXXFBCHILDRENMANAGE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<SZHL_XXFBType> allTypeList = new SZHL_XXFBTypeB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.IsDel == 0).ToList();
            if (allTypeList.Count > 0)
            {
                string pIds = allTypeList.Select(d => d.PTypeID.Value).Distinct().ToList().ListTOString(',');
                string strSql = string.Format("SELECT * from SZHL_XXFBType where ','+TypeManager+',' like '%,{0},%'and Id not in ({1}) and ComId={2} and isDel=0 order by PTypeID", UserInfo.User.UserName, pIds, UserInfo.User.ComId);
                DataTable dtType = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
                foreach (DataRow row in dtType.Rows)
                {
                    string parentTypeName = allTypeList.Where(d => row["TypePath"].ToString().Split('-').Contains(d.ID.ToString())).OrderBy(d => d.ID).Select(d => d.TypeName).ToList<string>().ListTOString('-');
                    row["TypeName"] = parentTypeName + "-" + row["TypeName"];
                }
                msg.Result = dtType;
            }

        }
        //获取所有子项列表
        public void GETXXFBCHILDREN(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<SZHL_XXFBType> allTypeList = new SZHL_XXFBTypeB().GetEntities(d => d.ComId == UserInfo.User.ComId).ToList();
            if (allTypeList.Count > 0)
            {
                string pIds = allTypeList.Select(d => d.PTypeID.Value).Distinct().ToList().ListTOString(',');
                string strSql = string.Format("SELECT * from SZHL_XXFBType where Id not in ({0}) and isDel=0  and ComId={1} order by PTypeID", pIds, UserInfo.User.ComId);
                DataTable dtType = new SZHL_XXFBTypeB().GetDTByCommand(strSql);

                foreach (DataRow row in dtType.Rows)
                {
                    string parentTypeName = allTypeList.Where(d => row["TypePath"].ToString().Split('-').Contains(d.ID.ToString())).OrderBy(d => d.ID).Select(d => d.TypeName).ToList<string>().ListTOString('-');
                    row["TypeName"] = parentTypeName + (parentTypeName != "" ? "-" : "") + row["TypeName"];
                }
                msg.Result = dtType;
            }
        }
        //获取所有子项列表
        public void GETXXFBUSERCHILDREN(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format(@"SELECT  DISTINCT type.* from SZHL_XXFBType type  inner join SZHL_XXFB  xxfb on type.ID=xxfb.XXFBType 
                                            where xxfb.ComId={0} and ','+xxfb.JSUser+',' LIKE '%,{1},%'", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dt = new SZHL_XXFBB().GetDTByCommand(strSql);
            if (dt.Rows.Count > 0)
            {
                string pIds = "";
                string Ids = "";
                foreach (DataRow row in dt.Rows)
                {
                    pIds += row["PTypeID"] + ",";
                    Ids += row["Id"] + ",";
                }
                pIds = pIds.Substring(0, pIds.Length - 1);
                Ids = Ids.Substring(0, Ids.Length - 1);
                string strSql1 = string.Format("SELECT * from SZHL_XXFBType where Id in ({0}) and isDel=0  and ComId={1}  order by PTypeID", pIds, UserInfo.User.ComId, Ids);
                DataTable dtParentType = new SZHL_XXFBTypeB().GetDTByCommand(strSql1);
                foreach (DataRow row in dt.Rows)
                {
                    string parentTypeName = "";
                    if (!string.IsNullOrEmpty(row["TypePath"].ToString()))
                    {
                        DataTable parentRow = dtParentType.Where("ID in (" + row["TypePath"].ToString().Replace('-', ',') + ")").OrderBy(" ID asc ");
                        foreach (DataRow prow in parentRow.Rows)
                        {
                            parentTypeName = parentTypeName + (parentTypeName != "" ? "-" : "") + prow["TypeName"];
                        }
                        row["TypeName"] = parentTypeName + (parentTypeName != "" ? "-" : "") + row["TypeName"];
                    }

                }
                msg.Result = dt;
            }
        }
        public void GETXXFBMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_XXFB model = new SZHL_XXFBB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            model.Remark = new SZHL_XXFBTypeB().GetEntity(p => p.ID == model.XXFBType).TypeName;
            msg.Result = model;
            msg.Result1 = new SZHL_XXFBB().GetDTByCommand("  SELECT ID,MSGTLYID,MSGType,MSGContent,CRDate,CRUser,CRUserName  FROM JH_Auth_TL WHERE MSGType='XXFB' AND  MSGTLYID='" + P1 + "'");
            if (!string.IsNullOrEmpty(model.Files))
            {
                msg.Result2 = new FT_FileB().GetEntities(" ID in (" + model.Files + ")");
            }
            msg.Result3 = new JH_Auth_User_CenterB().GetEntities(d => d.DataId == P1 && d.MsgModeID == "xxfb" && d.isRead == 0).Select(d => d.UserTO);
            msg.Result4 = new SZHL_XXFB_ITEMB().GetEntities(d => d.XXFBId == model.ID);

        }
        public void ADDXXFBTYPE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_XXFBType type = JsonConvert.DeserializeObject<SZHL_XXFBType>(P1);
            if (type != null)
            {
                if (type.IsCheck == "True" && type.CheckUser == "")
                {
                    msg.Result = "请选择审核人";
                    return;
                }
                if (type.ID == 0)
                {
                    DataTable dt = new SZHL_XXFBTypeB().GetDTByCommand("select ID,TypeName,PTypeID from SZHL_XXFBType where ComId=" + UserInfo.User.ComId + " and IsDel=0 ");


                    if (type.PTypeID == null || type.PTypeID <= 0)
                    {
                        DataTable dtpid = dt.FilterTable(" PTypeID =0 ");
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            type.PTypeID = (int)(dt.Rows[0]["ID"] ?? 0);
                            type.TypePath = dt.Rows[0]["ID"].ToString();
                        }
                    }
                    DataTable dtcunzai = dt.FilterTable("PTypeID ='" + type.PTypeID + "' and TypeName='" + type.TypeName + "'");

                    if (dtcunzai != null && dtcunzai.Rows.Count > 0)
                    {
                        msg.ErrorMsg = "分类已存在";
                    }
                    else
                    {
                        type.CRDate = DateTime.Now;
                        type.CRUser = UserInfo.User.UserName;
                        type.ComId = UserInfo.User.ComId;
                        type.IsDel = 0;
                        new SZHL_XXFBTypeB().Insert(type);
                    }
                }
                else
                {
                    new SZHL_XXFBTypeB().Update(type);
                }
            }
        }
        public void GETXXFBTYPE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            msg.Result = new SZHL_XXFBTypeB().GetEntity(d => d.ID == Id);
        }
        public void DELXXFBTYPE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            SZHL_XXFBType type = new SZHL_XXFBTypeB().GetEntity(d => d.ID == ID && d.ComId == UserInfo.User.ComId);
            string typepath = type.TypePath == "" ? type.ID + "-" : type.TypePath + "-" + type.ID;
            if (new SZHL_XXFBTypeB().GetEntities(d => d.ComId == UserInfo.User.ComId).ToList().Where(d => (d.TypePath + "-").IndexOf(typepath) > -1 && d.IsDel == 0).Count() > 0)
            {
                msg.ErrorMsg = "请先删除子分类";
            }
            else
            {
                type.IsDel = 1;
                new SZHL_XXFBTypeB().Update(type);
            }
        }
        //获取接收人列表
        public void GETXXFBUSERLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            SZHL_XXFBType xxfbType = new SZHL_XXFBTypeB().GetEntity(d => d.ID == id && d.ComId == UserInfo.User.ComId);
            msg.Result = new JH_Auth_UserB().GetEntities(d => xxfbType.SeeUser.Contains(d.UserName) && d.ComId == UserInfo.User.ComId);
        }

        public void ADDXXFB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_XXFB xxmodel = JsonConvert.DeserializeObject<SZHL_XXFB>(P1);
            string JsUser = xxmodel.JSUser;
            if (xxmodel.ID == 0)//企业信息添加
            {
                List<SZHL_XXFB_ITEM> xxfbList = JsonConvert.DeserializeObject<List<SZHL_XXFB_ITEM>>(P2); //企业信息发布多图文信息列表
                //企业信息基础赋值
                xxmodel.CRDate = DateTime.Now;
                xxmodel.CRUser = UserInfo.User.UserName;
                xxmodel.CRUserName = UserInfo.User.UserRealName;
                xxmodel.ComId = UserInfo.User.ComId;
                if (xxmodel.FBTime == null || xxmodel.FBTime < DateTime.Now)
                {
                    xxmodel.FBTime = DateTime.Now;
                }
                //如果是草稿不添加发送日期
                if (xxmodel.IsSend == "0")
                {
                    xxmodel.FBTime = null;
                }
                xxmodel.Remark = P2;
                xxmodel.XXTitle = xxfbList[0].XXTitle;
                //判断神皖是否需要审核
                // xxmodel.SHStatus = xxmodel.IsSH.ToLower() == "true" ? 0 : 1; //是否需要审核 
                //Saas判断是否需要审核
                SZHL_XXFBType type = new SZHL_XXFBTypeB().GetEntity(d => d.ID == xxmodel.XXFBType);
                xxmodel.SHStatus = type.IsCheck.ToLower() == "true" ? 0 : 2; //是否需要审核 
                //添加企业信息
                new SZHL_XXFBB().Insert(xxmodel);
                //循环多图文信息列表添加表，并判断是否发送消息
                foreach (SZHL_XXFB_ITEM xxfb in xxfbList)
                {
                    if (!string.IsNullOrEmpty(xxfb.XXTitle) || !string.IsNullOrEmpty(xxfb.XXContent))
                    {
                        xxfb.XXFBId = xxmodel.ID;
                        xxfb.ComId = UserInfo.User.ComId;
                        xxfb.FBTime = xxmodel.FBTime;
                        new SZHL_XXFB_ITEMB().Insert(xxfb);

                    }
                }
                //判断发布信息操作的微信消息 0为草稿 1为发布
                if (xxmodel.IsSend == "1")
                {
                    SZHL_TXSX tx = new SZHL_TXSX();
                    tx.ComId = UserInfo.User.ComId;
                    tx.APIName = "XXFB";
                    tx.TXMode = "XXFB";
                    tx.MsgID = xxmodel.ID.ToString();
                    tx.CRUser = UserInfo.User.UserName;
                    if (xxmodel.SHStatus == 2)  //无须审核
                    {
                        tx.FunName = "SENDWXMSG";
                        tx.Date = xxmodel.FBTime.Value.ToString("yyyy-MM-dd HH:mm:ss");

                        TXSX.TXSXAPI.AddALERT(tx); //时间为发送时间
                    }
                    else //需要审核
                    {
                        tx.FunName = "SENDWXMSG_CHECK";
                        tx.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        TXSX.TXSXAPI.AddALERT(tx);

                    }
                }
            }
            else
            {
                List<SZHL_XXFB_ITEM> xxfbList = JsonConvert.DeserializeObject<List<SZHL_XXFB_ITEM>>(P2);
                xxmodel.Remark = P2;
                xxmodel.FBTime = xxmodel.FBTime < DateTime.Now ? DateTime.Now : xxmodel.FBTime;
                xxmodel.XXTitle = xxfbList[0].XXTitle;
                if (xxmodel.FBTime == null || xxmodel.FBTime < DateTime.Now)
                {
                    xxmodel.FBTime = DateTime.Now;
                }
                //如果是草稿不添加发送日期
                if (xxmodel.IsSend == "0")
                {
                    xxmodel.FBTime = null;
                }
                new SZHL_XXFBB().Update(xxmodel);//更新企业信息
                new SZHL_XXFB_ITEMB().Delete(d => d.XXFBId == xxmodel.ID); //删除企业信息的多图文

                //循环多图文信息列表添加表，并判断是否发送消息
                foreach (SZHL_XXFB_ITEM xxfb in xxfbList)
                {
                    if (!string.IsNullOrEmpty(xxfb.XXTitle) || !string.IsNullOrEmpty(xxfb.XXContent))
                    {
                        xxfb.XXFBId = xxmodel.ID;
                        xxfb.ComId = UserInfo.User.ComId;
                        xxfb.FBTime = xxmodel.FBTime;
                        new SZHL_XXFB_ITEMB().Insert(xxfb);

                    }
                }
                //判断发布信息操作的微信消息 0为草稿，1为发布
                if (xxmodel.IsSend == "1")
                {
                    SZHL_TXSX tx = new SZHL_TXSX();
                    tx.ComId = UserInfo.User.ComId;
                    tx.APIName = "XXFB";
                    tx.TXMode = "XXFB";
                    tx.MsgID = xxmodel.ID.ToString();
                    tx.CRUser = UserInfo.User.UserName;
                    if (xxmodel.SHStatus == 2)  //无须审核
                    {
                        tx.FunName = "SENDWXMSG";
                        tx.Date = xxmodel.FBTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        TXSX.TXSXAPI.AddALERT(tx); //时间为发送时间
                    }
                    else //需要审核
                    {
                        tx.FunName = "SENDWXMSG_CHECK";
                        tx.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        TXSX.TXSXAPI.AddALERT(tx);

                    }

                }
            }

        }
        //修改信息发布
        public void XXFBMODIFY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_XXFB xxmodel = JsonConvert.DeserializeObject<SZHL_XXFB>(P1);
            string JsUser = xxmodel.JSUser;
            List<SZHL_XXFB_ITEM> xxfbList = JsonConvert.DeserializeObject<List<SZHL_XXFB_ITEM>>(P2);
            xxmodel.Remark = P2;
            xxmodel.FBTime = xxmodel.FBTime < DateTime.Now ? DateTime.Now : xxmodel.FBTime;
            xxmodel.XXTitle = xxfbList[0].XXTitle;
            if (xxmodel.FBTime == null || xxmodel.FBTime < DateTime.Now)
            {
                xxmodel.FBTime = DateTime.Now;
            }
            //如果是草稿不添加发送日期
            if (xxmodel.IsSend == "0")
            {
                xxmodel.FBTime = null;
            }
            new SZHL_XXFBB().Update(xxmodel);//更新企业信息
            //new SZHL_XXFB_ITEMB().Delete(d => d.XXFBId == xxmodel.ID); //删除企业信息的多图文

            //循环多图文信息列表添加表，并判断是否发送消息
            foreach (SZHL_XXFB_ITEM xxfb in xxfbList)
            {
                if (!string.IsNullOrEmpty(xxfb.XXTitle) || !string.IsNullOrEmpty(xxfb.XXContent))
                {
                    xxfb.XXFBId = xxmodel.ID;
                    xxfb.ComId = UserInfo.User.ComId;
                    xxfb.FBTime = xxmodel.FBTime;
                    int fbId = xxfb.ID;
                    if (xxfb.ID > 0)
                    {
                        new SZHL_XXFB_ITEMB().Update(xxfb);
                    }
                    else
                    {
                        new SZHL_XXFB_ITEMB().Insert(xxfb);
                    }

                }
            }
        }

        //获取企业信息的多图文列表
        public void GETXXFBITEMLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int fbId = int.Parse(P1);
            msg.Result = new SZHL_XXFB_ITEMB().GetEntities(d => d.XXFBId == fbId);
            msg.Result1 = new SZHL_XXFBB().GetEntity(d => d.ID == fbId && d.MsgType == 1 && d.IsSend == "1");
            new JH_Auth_User_CenterB().ReadMsg(UserInfo, fbId, "XXFB");
        }
        //获取接收的发布信息
        public void GETXXFBLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            string strWhere = string.Format("SZHL_XXFB.ComId='{0}'   and IsSend='1' and MsgType=1 ", UserInfo.User.ComId);
            if (P1 != "")
            {
                strWhere += string.Format(" And (SZHL_XXFB.XXTitle like '%{0}%' )", P1);
            }
            if (P2 != "")
            {
                strWhere += string.Format(" and ( xxtype.Id={0} or TypePath like '{1}%') ", P2.Split('-').LastOrDefault(), P2);
            }
            string type = context.Request["type"] ?? "";
            if (type == "1") //我接收的
            {
                strWhere += string.Format(" and SHStatus=2  and (','+SZHL_XXFB.JSUser+',' LIKE '%,{0},%' or  SZHL_XXFB.JSUser='') and FBTime<getdate()", UserInfo.User.UserName, DateTime.Now);
            }
            else
            {//我创建的
                strWhere += string.Format("  and  SZHL_XXFB.CRUser='{0}' ", UserInfo.User.UserName);
            }
            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB inner join SZHL_XXFBType xxtype on XXFBType=xxtype.ID", " SZHL_XXFB.*,xxtype.TypeName", pagecount, page, " FBTime desc", strWhere, ref recordCount);

            List<SZHL_XXFB_ITEM> ListXCX = new List<SZHL_XXFB_ITEM>();
            dt.Columns.Add("Item", Type.GetType("System.Object"));
            foreach (DataRow dr in dt.Rows)
            {
                int xid = Int32.Parse(dr["ID"].ToString());
                var list = new SZHL_XXFB_ITEMB().GetEntities(p => p.XXFBId == xid);
                ListXCX.AddRange(list);
                dr["Item"] = list;
            }
            msg.Result = dt;
            //  msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8);
            msg.Result1 = recordCount;
            msg.Result2 = ListXCX;
        }
        //获取自己创建的发布信息及草稿
        public void GETXXFBLIST_USER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);
            string strWhere = string.Format("SZHL_XXFB.ComId='{0}' and  SZHL_XXFB.CRUser='{1}' and SZHL_XXFB.MsgType=1 ", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" And (SZHL_XXFB.XXTitle like '%{0}%')", P1);
            }
            if (P2 != "")
            {
                if (!P2.Contains('-'))
                {
                    strWhere += string.Format(" and (  xxtype.Id={0} ) ", P2);
                }
                else
                {
                    strWhere += string.Format(" and ( xxtype.Id={0} or TypePath+'-' like '{1}-%') ", P2.Split('-').LastOrDefault(), P2);

                }
            }
            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB inner join SZHL_XXFBType xxtype on XXFBType=xxtype.ID", " SZHL_XXFB.*,xxtype.TypeName", 8, page, "IsSend asc, FBTime desc", strWhere, ref recordCount);
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8); ;
        }
        //获取需要登录人审核的数据
        public void GETXXFBLIST_DSH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);
            string strWhere = string.Format("SZHL_XXFB.ComId='{0}' and  SZHL_XXFB.SHUser='{1}' and SHStatus=0 and IsSend='1'and SZHL_XXFB.MsgType=1 ", UserInfo.User.ComId, UserInfo.User.UserName);
            if (P1 != "")
            {
                strWhere += string.Format(" And (SZHL_XXFB.XXTitle like '%{0}%')", P1);
            }
            if (P2 != "")
            {
                if (!P2.Contains('-'))
                {
                    strWhere += string.Format(" and (  xxtype.Id={0} ) ", P2);
                }
                else
                {
                    strWhere += string.Format(" and ( xxtype.Id={0} or TypePath+'-' like '{1}-%') ", P2.Split('-').LastOrDefault(), P2);

                }
            }
            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB inner join SZHL_XXFBType xxtype on XXFBType=xxtype.ID ", " SZHL_XXFB.*,xxtype.TypeName", 8, page, "IsSend asc, FBTime desc", strWhere, ref recordCount);
            msg.Result = dt;
            msg.Result1 = Math.Ceiling(recordCount * 1.0 / 8); ;
        }
        /// <summary>
        /// 信息发布审核
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SHXXFB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("UPDATE SZHL_XXFB set SHStatus={0} ,SHYJ='{1}',SHDate=getdate() where ID in ({2}) and ComId={3}", P2, context.Request["jy"] ?? "", P1, UserInfo.User.ComId);
            new SZHL_XXFBB().ExsSql(strSql);
            int[] Ids = P1.SplitTOInt(',');
            foreach (int ID in Ids)
            {
                SZHL_XXFB model = new SZHL_XXFBB().GetEntity(d => d.ID == ID);
                SZHL_XXFBType type = new SZHL_XXFBTypeB().GetEntity(d => d.ID == model.XXFBType);
                //判断审核状态为审核通过，给接收人发送消息
                if (model.SHStatus == 1)
                {
                    if (type.ISzjfb.ToLower() == "true")  //判断审核通过直接发布
                    {
                        SUREXXFB(context, msg, ID.ToString(), P2, UserInfo);
                    }
                    else
                    {
                        //给接收人发送消息
                        SZHL_TXSX tx = new SZHL_TXSX();
                        tx.ComId = UserInfo.User.ComId;
                        tx.APIName = "XXFB";
                        tx.TXMode = "XXFB";
                        tx.MsgID = model.ID.ToString();
                        tx.CRUser = UserInfo.User.UserName;
                        tx.FunName = "SENDWXMSG_CHECK";
                        tx.Date = model.FBTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        TXSX.TXSXAPI.AddALERT(tx); //时间为发送时间 
                    }
                }
                else
                    if (model.SHStatus == -1)//退回发送PC消息及微信消息
                {
                    string strMsg = UserInfo.User.UserRealName + "退回了您发布的企业信息";
                    new JH_Auth_User_CenterB().SendMsg(UserInfo, "XXFB", strMsg, model.ID.ToString(), model.CRUser);
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendWXRText(strMsg, "XXFB", model.CRUser);
                }
            }
        }
        public void SUREXXFB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //更新审核状态
            int Id = int.Parse(P1);
            SZHL_XXFB model = new SZHL_XXFBB().GetEntity(d => d.ID == Id);
            if (model.SHStatus != 2)
            {

                model.SHStatus = 2;
                new SZHL_XXFBB().Update(model);
                //给接收人发送消息
                SZHL_TXSX tx = new SZHL_TXSX();
                tx.ComId = UserInfo.User.ComId;
                tx.APIName = "XXFB";
                tx.TXMode = "XXFB";
                tx.MsgID = model.ID.ToString();
                tx.CRUser = UserInfo.User.UserName;
                tx.FunName = "SENDWXMSG";
                tx.Date = model.FBTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                TXSX.TXSXAPI.AddALERT(tx); //时间为发送时间 
            }
            else
            {
                msg.ErrorMsg = "此新闻公告已确认发布,请更新列表";
            }
        }
        public void GETSHXXFBLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int.TryParse(context.Request["p"] ?? "1", out page);
            string strWhere = string.Format(" fgtype.IsCheck='True'  and  ','+fgtype.CheckUser+',' LIKE '%,{0},%'  and fb.ComId={1} and fb.MsgType=1", UserInfo.User.UserName, UserInfo.User.ComId);
            int recordCount = 0;
            DataTable dt = new SZHL_XXFBB().GetDataPager("SZHL_XXFB fb inner join SZHL_XXFBType fgtype on fb.TypeID=fgtype.ID", " fb.*,fgtype.TypeName,fgtype.CheckUser", 8, page, " FBTime desc", strWhere, ref recordCount);
            msg.Result = dt;
            msg.Result1 = recordCount;
        }
        public void UPXXFBREADUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_XXFB_ITEM model = new SZHL_XXFB_ITEMB().GetEntity(d => d.ID == Id);
            if (model != null)
            {
                if (model.ReadUser == null || !model.ReadUser.Split(',').Contains(UserInfo.User.UserName))
                {
                    model.ReadUser += (model.ReadUser == "" || model.ReadUser == null ? "" : ",") + UserInfo.User.UserName;
                    new SZHL_XXFB_ITEMB().Update(model);
                    string strSql = string.Format("UPDATE JH_Auth_User_Center set isRead=1 where ComId={0} and MsgModeId='xxfb' and DataId={1} and UserTO='{2}'", UserInfo.User.ComId, Id, UserInfo.User.UserName);
                    new JH_Auth_User_CenterB().ExsSql(strSql);
                }
            }
        }
        public void DELDRAFTS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            SZHL_XXFB xxfb = new SZHL_XXFBB().GetEntity(d => d.ID == ID && d.IsSend == "0" && d.ComId == UserInfo.User.ComId);
            if (xxfb != null)
            {
                new SZHL_XXFBB().Delete(xxfb);
                new SZHL_XXFB_ITEMB().Delete(d => d.XXFBId == xxfb.ID);
            }

        }
        public void GETDRAFTLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            DataTable dt = new SZHL_XXFBB().GetDTByCommand("SELECT * from SZHL_XXFB where CRUser='" + UserInfo.User.UserName + "' and IsSend=0 and MsgType=1 and ComId=" + UserInfo.User.ComId + "");
            msg.Result = dt;
        }
        //保存为自己的素材
        public void SETSCUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            SZHL_XXFB xxfb = new SZHL_XXFBB().GetEntity(d => d.ID == ID);
            if (xxfb.SCUser == null || !xxfb.SCUser.Split(',').Contains(UserInfo.User.UserName))
            {
                if (string.IsNullOrEmpty(xxfb.SCUser))
                {
                    xxfb.SCUser = UserInfo.User.UserName;
                }
                else
                {
                    xxfb.SCUser = "," + UserInfo.User.UserName;
                }
                new SZHL_XXFBB().Update(xxfb);
                //保存素材
                SZHL_XXFB_SCK model = JsonConvert.DeserializeObject<SZHL_XXFB_SCK>(P2);
                model.ComId = UserInfo.User.ComId;
                model.CRUser = UserInfo.User.UserName;
                model.CRDate = DateTime.Now;
                new SZHL_XXFB_SCKB().Insert(model);
            }
        }
        //撤回信息发布
        public void DELETEXXFB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = int.Parse(P1);
            string strSql = string.Format("update SZHL_XXFB set IsSend='0' where ID={0} and ComId={1}", id, UserInfo.User.ComId);
            new SZHL_XXFBB().ExsSql(strSql);
            new SZHL_XXFB_ITEMB().Delete(d => d.XXFBId == id && d.ComId == UserInfo.User.ComId);
        }
        //获取自己的素材信息列表
        public void GETMATTERDRAFTLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("select * from SZHL_XXFB_SCK where CRUser='{0}' and ComId={1}  and Type=0", UserInfo.User.UserName, UserInfo.User.ComId);//and Type={1} 
            msg.Result = new SZHL_XXFB_SCKB().GetDTByCommand(strSql);
        }


        public void SENDWXMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);

            int msgid = Int32.Parse(tx.MsgID);

            var model = new SZHL_XXFBB().GetEntity(p => p.ID == msgid);
            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, model.CRUser);
            if (model != null)
            {
                var item = new SZHL_XXFB_ITEMB().GetEntities(d => d.XXFBId == model.ID);
                if (item.Count() > 0)
                {
                    List<Article> Msgs = new List<Article>();
                    foreach (var v in item)
                    {

                        new JH_Auth_User_CenterB().SendMsg(UserInfo, "XXFB", model.CRUserName + "发布了一个企业信息", v.ID.ToString(), model.JSUser);

                        Article ar = new Article();
                        ar.Title = v.XXTitle;
                        ar.Description = v.XXTitle;
                        ar.PicUrl = v.ImageIds.Split(',')[0];
                        ar.Url = v.ID.ToString();
                        Msgs.Add(ar);
                    }

                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(Msgs, "XXFB", "A", model.JSUser);
                }
            }

        }
        //发送审核消息
        public void SENDWXMSG_CHECK(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            int ID = Int32.Parse(tx.MsgID);
            var model = new SZHL_XXFBB().GetEntity(p => p.ID == ID);
            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, model.CRUser);
            if (model != null)
            {
                if (model.SHStatus == 0 || model.SHStatus == 1) //如果已处理，则不再发消息
                {
                    string wxflag = "B";//审核页面
                    string JSUser = "";
                    Article ar0 = new Article();
                    if (model.SHStatus == 0)
                    {
                        JSUser = model.SHUser;
                        ar0.Title = model.CRUserName + "发布了一个企业信息，请您审核";
                    }
                    else if (model.SHStatus == 1)
                    {
                        ar0.Title = new JH_Auth_UserB().GetUserRealName(UserInfo.QYinfo.ComId, model.SHUser) + "审核了一个企业信息，请您确认发布";
                        JSUser = model.CRUser;
                        wxflag = "C"; //发布确认页面
                    }
                    ar0.Description = "";
                    ar0.Url = model.XXFBType.ToString();

                    List<Article> al = new List<Article>();
                    al.Add(ar0);

                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    new JH_Auth_User_CenterB().SendMsg(UserInfo, "XXFB", ar0.Title, model.ID.ToString(), JSUser, wxflag);
                    wx.SendTH(al, "XXFB", wxflag, JSUser);
                }
            }

        }



        public void SENDWXMSG_SRTX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                CommonHelp.WriteLOG("调用生日提醒接口"+ P1);
                var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
                UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, tx.CRUser);
                WXHelp wx = new WXHelp(UserInfo.QYinfo);

                var qdata = new JH_Auth_UserB().GetEntities(d => d.Birthday != null);
                foreach (var item in qdata)
                {
                    if (item.Birthday.Value.ToString("MM-dd") == DateTime.Now.ToString("MM-dd"))
                    {
                        Article ar0 = new Article();
                        ar0.Title = "生日提醒";
                        ar0.Description = "";
                        ar0.Url = UserInfo.QYinfo.WXUrl + "/View_Mobile/UI/RLZY/sr.html?user=" + item.UserRealName;
                        ar0.PicUrl = UserInfo.QYinfo.WXUrl + "/View_Mobile/UI/RLZY/images/sr.jpg";
                        List<Article> al = new List<Article>();
                        al.Add(ar0);
                        wx.SendTPMSG("XXFB", al, item.UserName);
                    }


                }
            }
            catch (Exception ex)
            {

                CommonHelp.WriteLOG("调用生日提醒接口" + ex.ToString());
            }



        }


        //删除素材
        public void DELMATTER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            SZHL_XXFB xxfb = new SZHL_XXFBB().GetEntity(d => d.ID == Id);
            xxfb.SCUser = ("," + xxfb.SCUser + ",").Replace(UserInfo.User.UserName + ",", "").Trim(',');
            new SZHL_XXFBB().Update(xxfb);
        }

        #region 素材库
        public void ADDSCCONTENT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            List<SZHL_XXFB_SCK> modelList = JsonConvert.DeserializeObject<List<SZHL_XXFB_SCK>>(P1);
            foreach (SZHL_XXFB_SCK model in modelList)
            {
                if (model.Id == 0) //ID==0为添加
                {
                    model.ComId = UserInfo.User.ComId;
                    model.CRUser = UserInfo.User.UserName;
                    model.CRDate = DateTime.Now;
                    new SZHL_XXFB_SCKB().Insert(model);
                }
                else //否则为编辑
                {
                    new SZHL_XXFB_SCKB().Update(model);
                }
            }
            msg.Result = modelList;
        }
        //获取素材列表
        public void GETSCLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("select * from SZHL_XXFB_SCK where CRUser='{0}' and ComId={1} ", UserInfo.User.UserName, UserInfo.User.ComId);//and Type={1} 
            msg.Result = new SZHL_XXFB_SCKB().GetDTByCommand(strSql + " and Type=0");
            string strSql2 = string.Format("SELECT sck.*,files.name,files.FileExtendName,files.FileMD5 from  SZHL_XXFB_SCK sck inner join FT_File files on sck.Files=files.Id   where sck.CRUser='{0}' and sck.ComId={1} ", UserInfo.User.UserName, UserInfo.User.ComId);//and Type={1}
            msg.Result1 = new SZHL_XXFB_SCKB().GetDTByCommand(strSql2 + " and Type=1");
            msg.Result2 = new SZHL_XXFB_SCKB().GetDTByCommand(strSql2 + " and Type=2");
        }
        public void DELETESCMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = int.Parse(P1);
            new SZHL_XXFB_SCKB().Delete(d => d.Id == Id && d.ComId == UserInfo.User.ComId && d.CRUser == UserInfo.User.UserName);
        }
        #endregion

        public void GETMOBILEINDEX(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format(@"SELECT  top 3 item.* from  SZHL_XXFB_ITEM item inner join SZHL_XXFB  xxfb on item.XXFBId=xxfb.ID  
                                            where item.ComId={0}  and xxfb.IsSend='1'  
                                            and SHStatus=2  and (','+xxfb.JSUser+',' LIKE '%,{1},%' or  xxfb.JSUser='')  order by xxfb.FBTime DESC", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dt = new SZHL_XXFBB().GetDTByCommand(strSql);
            msg.Result = dt;
        }
        public void GETXXFBTYPETOP(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //获取信息发布分类下的第一条公告
            string strSql = string.Format(@"SELECT * from (
                                            SELECT  xxtype.ID typeId,xxtype.TypeName,xxfb.XXTitle,xxfb.CRDate,xxfb.ID,  ROW_NUMBER() OVER (PARTITION BY xxtype.TypeName ORDER BY xxfb.CRDate DESC) NewIndex
                                            FROM SZHL_XXFBType xxtype LEFT JOIN SZHL_XXFB xxfb on xxtype.ID=xxfb.XXFBType AND ','+xxfb.JSUser+',' like '%,{0},%' 
		                                     where xxtype.ComId={1} 
		                                     ) newTab where newTab.NewIndex=1 ORDER by newTab.CRDate DESC", UserInfo.User.UserName, UserInfo.User.ComId);
            DataTable dt = new SZHL_XXFBB().GetDTByCommand(strSql);
            dt.Columns.Add("msgcount");
            string strSql1 = string.Format(@"SELECT xxfbitem.XXTitle,xxfb.ID,xxfb.XXFBType from SZHL_XXFB xxfb INNER join SZHL_XXFB_ITEM  xxfbitem on xxfb.ID=xxfbitem.XXFBId 
                                            inner join JH_Auth_User_Center center  on center.MsgModeID='XXFB' and center.DataId=xxfbitem.ID and center.isRead=0
                                             where xxfb.ComId={0} and center.UserTO='{1}' ", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dtXXFB = new SZHL_XXFBB().GetDTByCommand(strSql1);
            string strSql2 = string.Format("SELECT * from SZHL_XXFB where ComId={0} and   IsSend=1 and ((CRUser='{1}' and SHStatus=1) or (SHUser='{1}' and SHStatus=0))", UserInfo.User.ComId, UserInfo.User.UserName);
            DataTable dtSHXXFB = new SZHL_XXFBB().GetDTByCommand(strSql2);
            foreach (DataRow row in dt.Rows)
            {
                if (!string.IsNullOrEmpty(row["ID"].ToString()))
                {
                    row["msgcount"] = dtXXFB.Select(" XXFBType=" + row["typeId"]).Count() + dtSHXXFB.Select("XXFBType=" + row["typeId"]).Count();
                }
            }
            msg.Result = dt;
        }




        public void GETXXFBBYUSER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strSql = string.Format("select * from SZHL_XXFBType xxfb where (','+xxfb.TypeManager+',' LIKE '%,{0},%')  and ComId={1} and PTypeID!='0' and isDel=0 and  IsCheck='false' ", UserInfo.User.UserName, UserInfo.User.ComId);//and Type={1} 
            msg.Result = new SZHL_XXFBTypeB().GetDTByCommand(strSql);
        }
        public void ADDXXFBM(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            SZHL_XXFB xxmodel = JsonConvert.DeserializeObject<SZHL_XXFB>(P1);
            string JsUser = xxmodel.JSUser;

            List<SZHL_XXFB_ITEM> xxfbList = new List<SZHL_XXFB_ITEM>(); //企业信息发布多图文信息列表
            xxmodel.CRDate = DateTime.Now;
            xxmodel.CRUser = UserInfo.User.UserName;
            xxmodel.CRUserName = UserInfo.User.UserRealName;
            xxmodel.ComId = UserInfo.User.ComId;
            if (xxmodel.FBTime == null || xxmodel.FBTime < DateTime.Now)
            {
                xxmodel.FBTime = DateTime.Now;
            }

            xxmodel.Remark = P2;
            xxmodel.XXTitle = xxfbList[0].XXTitle;
            //判断神皖是否需要审核
            // xxmodel.SHStatus = xxmodel.IsSH.ToLower() == "true" ? 0 : 1; //是否需要审核 
            //Saas判断是否需要审核
            xxmodel.SHStatus = 2; //是否需要审核 
                                  //添加企业信息
            new SZHL_XXFBB().Insert(xxmodel);
            //循环多图文信息列表添加表，并判断是否发送消息
            foreach (SZHL_XXFB_ITEM xxfb in xxfbList)
            {
                if (!string.IsNullOrEmpty(xxfb.XXTitle) || !string.IsNullOrEmpty(xxfb.XXContent))
                {
                    xxfb.XXFBId = xxmodel.ID;
                    xxfb.ComId = UserInfo.User.ComId;
                    xxfb.FBTime = xxmodel.FBTime;
                    new SZHL_XXFB_ITEMB().Insert(xxfb);

                }
            }
            //判断发布信息操作的微信消息 0为草稿 1为发布
            if (xxmodel.IsSend == "1")
            {
                SZHL_TXSX tx = new SZHL_TXSX();
                tx.ComId = UserInfo.User.ComId;
                tx.APIName = "XXFB";
                tx.TXMode = "XXFB";
                tx.MsgID = xxmodel.ID.ToString();
                tx.CRUser = UserInfo.User.UserName;
                if (xxmodel.SHStatus == 2)  //无须审核
                {
                    tx.FunName = "SENDWXMSG";
                    tx.Date = xxmodel.FBTime.Value.ToString("yyyy-MM-dd HH:mm:ss");

                    TXSX.TXSXAPI.AddALERT(tx); //时间为发送时间
                }
                else //需要审核
                {
                    tx.FunName = "SENDWXMSG_CHECK";
                    tx.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    TXSX.TXSXAPI.AddALERT(tx);

                }
            }
        }



    }

}