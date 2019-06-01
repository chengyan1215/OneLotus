using FastReflectionLib;
using Newtonsoft.Json;
using QJY.BusinessData;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using Senparc.NeuChar.Entities;

namespace QJY.API
{
    public class TSSQManage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(TSSQManage).GetMethod(msg.Action.ToUpper());
            TSSQManage model = new TSSQManage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }

        #region 获取话题列表
        /// <summary>
        /// 获取话题列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETHTLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {


            string userName = UserInfo.User.UserName;

            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);//页码
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int recordCount = 0;
            string strWhere = string.Format(" sq.ComId={0} ", UserInfo.User.ComId);
            string strContent = context.Request["Content"] ?? "";

            strContent = strContent.TrimEnd();
            if (strContent != "")
            {
                strWhere += string.Format(" And ( sq.HTNR like '%{0}%' )", strContent);
            }

            string leibie = context.Request["lb"] ?? "";
            if (leibie != "")
            {
                if (leibie == "0")//精华
                {
                    strWhere += string.Format(" And sq.ISJH='Y' ");
                }
                else
                {
                    strWhere += string.Format(" And sq.LeiBie='{0}' ", leibie);
                }

            }

            string biaoqian = context.Request["biaoqian"] ?? "";//标签
            if (biaoqian != "" && biaoqian != "全部")
            {
                strWhere += string.Format(" And sq.biaoqian='{0}' ", biaoqian);
            }

            int DataID = -1;
            int.TryParse(context.Request["ID"] ?? "-1", out DataID);//记录Id
            if (DataID != -1)
            {
                strWhere += string.Format(" And sq.ID = '{0}'", DataID);
            }
            if (P1 != "")
            {
                switch (P1)
                {
                    case "0":
                        {
                            //设置usercenter已读
                            new JH_Auth_User_CenterB().ReadMsg(UserInfo, DataID, "TSSQ");
                        }
                        break;
                    case "1": //我的
                        {
                            strWhere += " And sq.CRUser='" + userName + "'";
                        }
                        break;
                    case "2": //我评论过的
                        {
                            //DataTable dtPL = new JH_Auth_TLB().GetDTByCommand(string.Format("SELECT tl.MSGTLYID  FROM JH_Auth_TL tl WHERE tl.MSGType='TSSQ' AND tl.MsgISShow <> 'Y' AND tl.CRUser='{0}'", UserInfo.User.UserName));
                            string htIDs = new JH_Auth_TLB().GetEntities(d => d.MSGType == "TSSQ" && d.CRUser == UserInfo.User.UserName && (d.MsgISShow != "Y" && d.MsgISShow != "N")).Select(d => d.MSGTLYID).ToList().ListTOString(',');
                            if (!string.IsNullOrWhiteSpace(htIDs))
                                strWhere += "And sq.ID IN (" + htIDs + ") ";
                            else
                                strWhere += "And sq.ID ='' ";
                        }
                        break;
                    case "3"://全部可看的
                        {
                            strWhere += " And (ISNULL(sq.CYR,'') = '' OR ','+sq.CYR+','  like '%," + userName + ",%' )";
                        }
                        break;
                }
                DataTable dt = new SZHL_TSSQB().GetDataPager(@" SZHL_TSSQ sq LEFT JOIN JH_Auth_ZiDian zd ON sq.LeiBie=zd.ID and Class=19 left join JH_Auth_User u on sq.CRUser=u.UserName", " sq.*,zd.TypeName,u.UserRealName,u.zhiwu ", pagecount, page, " sq.Status DESC,sq.CRDate DESC ", strWhere, ref recordCount);
                dt = APIHelp.GetDWByUserName(dt, UserInfo.User.ComId.Value);
                #region 附件评论
                string Ids = "";
                string fileIDs = "";
                foreach (DataRow row in dt.Rows)
                {
                    Ids += row["ID"].ToString() + ",";
                    if (!string.IsNullOrEmpty(row["Files"].ToString()))
                    {
                        fileIDs += row["Files"].ToString() + ",";
                    }
                }
                Ids = Ids.TrimEnd(',');
                fileIDs = fileIDs.TrimEnd(',');
                if (Ids != "")
                {
                    List<FT_File> FileList = new List<FT_File>();
                    DataTable dtPL = new JH_Auth_TLB().GetDTByCommand(string.Format("SELECT tl.*  FROM JH_Auth_TL tl WHERE tl.MSGType='TSSQ' AND (ISNULL(tl.MsgISShow,'') <> 'Y' and ISNULL(tl.MsgISShow,'') <> 'N') AND  tl.MSGTLYID in ({0}) ORDER BY CRDate ASC", Ids));
                    DataTable zandt = new JH_Auth_TLB().GetDTByCommand(string.Format("SELECT tl.*  FROM JH_Auth_TL tl WHERE tl.MSGType='TSSQ' AND MsgISShow='Y' AND  tl.MSGTLYID in ({0})", Ids));

                    dtPL = APIHelp.GetDWByUserName(dtPL, UserInfo.User.ComId.Value);
                    zandt = APIHelp.GetDWByUserName(zandt, UserInfo.User.ComId.Value);



                    if (!string.IsNullOrEmpty(fileIDs))
                    {
                        int[] fileId = fileIDs.SplitTOInt(',');
                        FileList = new FT_FileB().GetEntities(d => fileId.Contains(d.ID)).ToList();
                    }


                    dt.Columns.Add("PLList", Type.GetType("System.Object"));
                    dt.Columns.Add("FileList", Type.GetType("System.Object"));
                    dt.Columns.Add("ZanList", Type.GetType("System.Object"));
                    dt.Columns.Add("IsZan", Type.GetType("System.Object"));
                    foreach (DataRow row in dt.Rows)
                    {
                        DataTable dtPLs = dtPL.FilterTable("MSGTLYID='" + row["ID"] + "'");
                        dtPLs.Columns.Add("FileList", Type.GetType("System.Object"));
                        foreach (DataRow dr in dtPLs.Rows)
                        {
                            if (dr["MSGisHasFiles"] != null && dr["MSGisHasFiles"].ToString() != "")
                            {
                                int[] fileIds = dr["MSGisHasFiles"].ToString().SplitTOInt(',');
                                dr["FileList"] = new FT_FileB().GetEntities(d => fileIds.Contains(d.ID));
                            }
                        }
                        row["PLList"] = dtPLs;
                        row["ZanList"] = zandt.FilterTable("MSGTLYID='" + row["ID"] + "'");
                        row["IsZan"] = zandt.FilterTable(" MSGTLYID='" + row["ID"] + "' and CRUser='" + userName + "'").Rows.Count > 0 ? "Y" : "N";
                        if (FileList.Count > 0)
                        {

                            string[] fileIds = row["Files"].ToString().Split(',');
                            row["FileList"] = FileList.Where(d => fileIds.Contains(d.ID.ToString()));
                        }
                    }

                    msg.Result3 = dtPL;



                }
                #endregion

                msg.Result = dt;
                msg.Result1 = recordCount;


                new SZHL_TSSQB().ExsSclarSql("UPDATE  SZHL_TSSQ SET RedUsers= RedUsers+'," + UserInfo.User.UserName + "' WHERE  ','+RedUsers+',' NOT like '%," + UserInfo.User.UserName + ",%'");
            }
        }
        #endregion

        #region 添加话题
        /// <summary>
        /// 添加话题
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDHT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            if (UserInfo.User.isJY == "Y" && UserInfo.User.JYDate > DateTime.Now)
            {
                msg.ErrorMsg = "您已被禁言!无法发表交流";
                return;
            }



            SZHL_TSSQ tssq = JsonConvert.DeserializeObject<SZHL_TSSQ>(P1);
            if (!string.IsNullOrEmpty(tssq.HTNR) && APIHelp.TestWB(tssq.HTNR) != "0")
            {
                msg.ErrorMsg = "您得发言涉及违规内容,请完善后再发";
                return;
            }


            if (!string.IsNullOrEmpty(tssq.URL) && !tssq.URL.Contains("http://"))
            {
                tssq.URL = "http://" + tssq.URL;
            }

            if (P2 != "") // 处理微信上传的图片
            {
                string fids = new FT_FileB().ProcessWxIMG(P2, "TSSQ", UserInfo);

                if (!string.IsNullOrEmpty(tssq.Files))
                {
                    tssq.Files += "," + fids;
                }
                else
                {
                    tssq.Files = fids;
                }
            }
            if (tssq.ID == 0)
            {
                tssq.CRDate = DateTime.Now;
                tssq.CRUser = UserInfo.User.UserName;
                tssq.ComId = UserInfo.User.ComId;
                tssq.Status = 0;
                tssq.RedUsers = UserInfo.User.UserName;
                new SZHL_TSSQB().Insert(tssq);

                SZHL_TXSX CSTX = new SZHL_TXSX();
                CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                CSTX.APIName = "TSSQ";
                CSTX.ComId = UserInfo.User.ComId;
                CSTX.FunName = "SENDHTMSG";
                CSTX.CRUserRealName = UserInfo.User.UserRealName;
                CSTX.MsgID = tssq.ID.ToString();
                CSTX.ISCS = "N";
                CSTX.TXUser = tssq.CRUser;
                CSTX.TXMode = "TSSQ";
                CSTX.CRUser = UserInfo.User.UserName;

                TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
            }
            else
            {
                new SZHL_TSSQB().Update(tssq);
            }
            msg.Result = tssq;
        }

        public void SENDHTMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            int msgid = Int32.Parse(tx.MsgID);

            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, tx.CRUser);

            var model = new SZHL_TSSQB().GetEntity(p => p.ID == msgid && p.ComId == UserInfo.User.ComId);
            if (model != null)
            {
                #region old
                //Article ar0 = new Article();

                //ar0.Title = UserInfo.User.UserRealName + "发表了新话题";
                //ar0.Description = CommonHelp.RemoveHtml(model.HTNR);
                //ar0.Url = model.ID.ToString();
                //if (!string.IsNullOrEmpty(model.Files))
                //{
                //    ar0.PicUrl = model.Files.Split(',')[0];
                //}
                //List<Article> al = new List<Article>();
                //al.Add(ar0);

                //string jsr = string.Empty;
                //if (!string.IsNullOrEmpty(model.CYR))
                //{
                //    jsr = model.CYR;
                //}
                //else
                //{
                //    jsr = new JH_Auth_UserB().GetEntities(p => p.ComId == UserInfo.QYinfo.ComId).Select(d => d.UserName).ToList().ListTOString(',');
                //}
                ////发送消息
                //string content = ar0.Description;
                //new JH_Auth_User_CenterB().SendMsg(UserInfo, "TSSQ", content, model.ID.ToString(), jsr, "A");
                //if (!string.IsNullOrEmpty(jsr))
                //{
                //    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                //    wx.SendTH(al, "TSSQ", "A", jsr);
                //}
                #endregion

                //小程序发送通知

                var ht = new CommonHelp().StripHT(model.HTNR);

                string tzr = model.TZR.ToString();


                foreach (var bm in model.TZBM.Split(','))
                {
                    var bmr = new JH_Auth_UserB().GetUserListbyBranch(Int32.Parse(bm), "", UserInfo.User.ComId.Value);
                    foreach (DataRow dr in bmr.Rows)
                    {
                        if (!tzr.Contains(dr["UserName"].ToString()))
                        {
                            tzr += "," + dr["UserName"].ToString();
                        }
                    }
                }

                CommonHelp.WriteLOG(tzr);

                foreach (var ksuser in tzr.Split(','))
                {
                    if (ksuser != "")
                    {
                        CommonHelp.WriteLOG(ksuser);

                        var usr = new JH_Auth_UserB().GetEntity(p => p.UserName == ksuser);
                        if (usr != null && !string.IsNullOrEmpty(usr.weixinCard))
                        {
                          

                        }

                    }
                }
            }
        }
        #endregion

        #region 获取话题信息
        /// <summary>
        /// 获取话题BYID
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETHTMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);
            SZHL_TSSQ sg = new SZHL_TSSQB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = sg;
            if (sg != null)
            {
                if (!string.IsNullOrWhiteSpace(sg.Files))
                {
                    msg.Result1 = new FT_FileB().GetEntities(" ID in (" + sg.Files + ")");
                }
                if (sg.LeiBie != null && !string.IsNullOrWhiteSpace(sg.LeiBie.ToString()))
                {
                    var SS = new JH_Auth_ZiDianB().GetEntity(p => p.ID == sg.LeiBie);
                    if (SS != null)
                    {
                        msg.Result2 = SS.TypeName;
                    }
                }
                //msg.Result3 = new JH_Auth_TLB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.MSGType == "TSSQ" && d.MSGTLYID == P1 && (d.MsgISShow != "Y" && d.MsgISShow != "N")).ToList();

                DataTable dtPL = new SZHL_TSSQB().GetDTByCommand("  SELECT *  FROM JH_Auth_TL WHERE MSGType='TSSQ' AND  MSGTLYID='" + P1 + "' and MsgISShow!='Y' and MsgISShow!='N')");
                dtPL.Columns.Add("FileList", Type.GetType("System.Object"));
                foreach (DataRow dr in dtPL.Rows)
                {
                    if (dr["MSGisHasFiles"] != null && dr["MSGisHasFiles"].ToString() != "")
                    {
                        int[] fileIds = dr["MSGisHasFiles"].ToString().SplitTOInt(',');
                        dr["FileList"] = new FT_FileB().GetEntities(d => fileIds.Contains(d.ID));
                    }
                }
                msg.Result3 = dtPL;
            }
        }
        #endregion

        #region 删除话题
        /// <summary>
        /// 删除话题
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELHTBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int SQID = int.Parse(P1);
                SZHL_TSSQ SQ = new SZHL_TSSQ();
                SQ = new SZHL_TSSQB().GetEntity(d => d.ID == SQID);
                if (SQ.CRUser == UserInfo.User.UserName && DateTime.Now.AddMonths(-1) < SQ.CRDate)
                {
                    if (new SZHL_TSSQB().Delete(d => d.ID == SQID))
                    {
                        if (new JH_Auth_TLB().Delete(d => d.ComId == UserInfo.User.ComId && d.MSGType == "TSSQ" && d.MSGTLYID == P1))
                        {
                            msg.ErrorMsg = "";
                        }
                    }
                }
                else
                {
                    msg.ErrorMsg = "超出时限,您已无法删除该信息";
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }
        #endregion

        #region 话题点赞
        /// <summary>
        /// 话题点赞
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ZANHTBYID(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            if (int.TryParse(P1, out id))
            {
                List<JH_Auth_TL> tls = new JH_Auth_TLB().GetEntities(d => d.ComId == UserInfo.User.ComId && d.MSGType == "TSSQ" && d.CRUser == UserInfo.User.UserName && (d.MsgISShow == "Y" || d.MsgISShow == "N") && d.MSGTLYID == P1).ToList();
                if (tls.Count > 0)
                {

                    //if (tls[0].CRUser != UserInfo.User.UserName)
                    //{
                    //    SZHL_TXSX CSTX = new SZHL_TXSX();
                    //    CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //    CSTX.APIName = "TSSQ";
                    //    CSTX.ComId = UserInfo.User.ComId;
                    //    CSTX.FunName = "SENDQXZANMSG";
                    //    CSTX.CRUserRealName = UserInfo.User.UserRealName;
                    //    CSTX.MsgID = tls[0].MSGTLYID;

                    //    CSTX.TXContent = "";
                    //    CSTX.ISCS = "N";
                    //    CSTX.TXUser = tls[0].CRUser;
                    //    CSTX.TXMode = tls[0].MSGType;
                    //    CSTX.CRUser = UserInfo.User.UserName;

                    //    TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
                    //}

                    //new JH_Auth_TLB().Delete(tls[0]);
                    if (tls[0].MsgISShow == "N")
                    {
                        tls[0].MsgISShow = "Y";
                        new JH_Auth_TLB().Update(tls[0]);
                        msg.Result = tls[0];
                    }
                    else if (tls[0].MsgISShow == "Y")
                    {
                        tls[0].MsgISShow = "N";
                        new JH_Auth_TLB().Update(tls[0]);
                        msg.ErrorMsg = "del";
                    }


                }
                else
                {
                    JH_Auth_TL Model = new JH_Auth_TL();
                    Model.CRDate = DateTime.Now;
                    Model.CRUser = UserInfo.User.UserName;
                    Model.CRUserName = UserInfo.User.UserRealName;
                    Model.ComId = UserInfo.User.ComId;
                    Model.MSGTLYID = P1;
                    Model.MSGType = "TSSQ";
                    Model.MsgISShow = "Y";
                    new JH_Auth_TLB().Insert(Model);
                    SZHL_TSSQ tssq = new SZHL_TSSQB().GetEntity(d => d.ID == id);
                    if (tssq.CRUser != UserInfo.User.UserName)
                    {
                        SZHL_TXSX CSTX = new SZHL_TXSX();
                        CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        CSTX.APIName = "TSSQ";
                        CSTX.ComId = UserInfo.User.ComId;
                        CSTX.FunName = "SENDZANMSG";
                        CSTX.CRUserRealName = UserInfo.User.UserRealName;
                        CSTX.MsgID = P1;

                        CSTX.TXContent = "";
                        CSTX.ISCS = "N";
                        CSTX.TXUser = tssq.CRUser;
                        CSTX.TXMode = Model.MSGType;
                        CSTX.CRUser = UserInfo.User.UserName;

                        TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
                    }

                    msg.Result = Model;
                }
            }
        }
        public void SENDZANMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            int msgid = Int32.Parse(tx.MsgID);

            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, tx.CRUser);

            var model = new SZHL_TSSQB().GetEntity(p => p.ID == msgid && p.ComId == UserInfo.User.ComId);
            if (model != null)
            {
                Article ar0 = new Article();
                ar0.Title = "您收到一个赞";
                ar0.Description = "话题:" + CommonHelp.RemoveHtml(model.HTNR) + "\r\n来自:" + UserInfo.User.UserRealName;
                ar0.Url = model.ID.ToString();
                List<Article> al = new List<Article>();
                al.Add(ar0);

                string jsr = string.Empty;
                jsr = model.CRUser;

                if (!string.IsNullOrEmpty(jsr))
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(al, "TSSQ", "A", jsr);
                }
            }
        }
        public void SENDQXZANMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            int msgid = Int32.Parse(tx.MsgID);

            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, tx.CRUser);

            var model = new SZHL_TSSQB().GetEntity(p => p.ID == msgid && p.ComId == UserInfo.User.ComId);
            if (model != null)
            {
                Article ar0 = new Article();
                ar0.Title = "您被取消一个赞";
                ar0.Description = "话题:" + CommonHelp.RemoveHtml(model.HTNR) + "\r\n来自:" + UserInfo.User.UserRealName;
                ar0.Url = model.ID.ToString();
                List<Article> al = new List<Article>();
                al.Add(ar0);

                string jsr = string.Empty;
                jsr = model.CRUser;
                //发送消息
                //string content = ar0.Description;
                //new JH_Auth_User_CenterB().SendMsg(UserInfo, "CRM", content, model.ID.ToString(), jsr, "A");
                if (!string.IsNullOrEmpty(jsr))
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(al, "TSSQ", "A", jsr);
                }
            }
        }

        #endregion

        #region 分享链接的信息
        /// <summary>
        /// 分享链接的信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SENDWXMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var tx = JsonConvert.DeserializeObject<SZHL_TXSX>(P1);
            int msgid = Int32.Parse(tx.MsgID);
            UserInfo = new JH_Auth_UserB().GetUserInfo(tx.ComId.Value, tx.CRUser);
            var model = new JH_Auth_WXMSGB().GetEntity(p => p.ID == msgid && p.ComId == UserInfo.User.ComId);
            if (model != null)
            {
                Article ar0 = new Article();
                ar0.Title = model.Title;
                ar0.Description = "您收到了" + tx.CRUserRealName + "创建的话题信息，请查阅";
                ar0.Url = model.ID.ToString();
                ar0.PicUrl = model.FileId;

                List<Article> al = new List<Article>();
                al.Add(ar0);

                string jsr = string.Empty;
                jsr = model.CRUser;
                //发送消息
                //string content = ar0.Description;
                //new JH_Auth_User_CenterB().SendMsg(UserInfo, "CRM", content, model.ID.ToString(), jsr, "A");
                if (!string.IsNullOrEmpty(jsr))
                {
                    WXHelp wx = new WXHelp(UserInfo.QYinfo);
                    wx.SendTH(al, "TSSQ", "D", jsr);
                }
            }
        }
        #endregion

        #region 获取分享链接的信息
        /// <summary>
        /// 获取分享链接的信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFXLJMODEL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int Id = 0;
            int.TryParse(P1, out Id);
            JH_Auth_WXMSG sg = new JH_Auth_WXMSGB().GetEntity(d => d.ID == Id && d.ComId == UserInfo.User.ComId);
            msg.Result = sg;
            if (sg != null)
            {
                if (sg.FileId != null && sg.FileId != "")
                {
                    msg.Result1 = new FT_FileB().GetEntities(" ID in (" + sg.FileId + ")");
                }
            }
        }
        #endregion



        #region 评论回复
        /// <summary>
        /// 评论回复
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="strParamData"></param>
        /// <param name="strUserName"></param>
        public void ADDCOMENT(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            string strMsgLYID = context.Request["MsgLYID"] ?? "";
            string TLID = context.Request["TLID"] ?? "";
            string strfjID = context.Request["fjID"] ?? "";
            JH_Auth_TL Model = new JH_Auth_TL();
            Model.CRDate = DateTime.Now;
            Model.CRUser = UserInfo.User.UserName;
            Model.CRUserName = UserInfo.User.UserRealName;
            Model.MSGContent = P1;
            Model.ComId = UserInfo.User.ComId;
            Model.MSGTLYID = strMsgLYID;
            Model.MSGisHasFiles = strfjID;
            Model.MSGType = "TSSQ";
            if (TLID != "")
            {
                Model.TLID = int.Parse(TLID);
                Model.ReUser = new JH_Auth_TLB().GetEntity(d => d.ID == Model.TLID).CRUser;
            }
            new JH_Auth_TLB().Insert(Model);
            string Content = UserInfo.User.UserRealName + "回复了您的评论";
            if (Model.ReUser != UserInfo.User.UserName)
            {
                SZHL_TXSX CSTX = new SZHL_TXSX();
                CSTX.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                CSTX.APIName = "TSSQ";
                CSTX.ComId = UserInfo.User.ComId;
                CSTX.FunName = "SENDPLMSG";
                CSTX.CRUserRealName = UserInfo.User.UserRealName;
                CSTX.MsgID = Model.MSGTLYID;
                CSTX.TXContent = Content;
                CSTX.ISCS = "N";
                CSTX.TXUser = Model.ReUser;
                CSTX.TXMode = Model.MSGType;
                CSTX.CRUser = UserInfo.User.UserName;
                TXSX.TXSXAPI.AddALERT(CSTX); //时间为发送时间 
            }

            msg.Result = Model;
            if (Model.MSGisHasFiles != "")
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
                var usr = new JH_Auth_UserB().GetEntity(p => p.UserName == TX.TXUser);
                if (usr != null && !string.IsNullOrEmpty(usr.weixinCard))
                {
                 

                }



            }
        }
        #endregion



        #region 补充接口

        /// <summary>
        /// 设置精华发文
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETJH(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                int Id = 0;
                int.TryParse(P1, out Id);

                int STATUS = 0;
                int.TryParse(P2, out STATUS);
                SZHL_TSSQ MODEL = new SZHL_TSSQB().GetEntity(D => D.ID == Id);
                MODEL.Status = STATUS;
                new SZHL_TSSQB().Update(MODEL);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 设置禁止被@
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETJZTZ(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {

                JH_Auth_User MODEL = new JH_Auth_UserB().GetEntity(D => D.UserName == P1);
                MODEL.isTX = P2;
                new JH_Auth_UserB().Update(MODEL);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 设置禁止发言
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETJY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                string JYDate = context.Request["JYDate"] ?? "";

                JH_Auth_User MODEL = new JH_Auth_UserB().GetEntity(D => D.UserName == P1);
                MODEL.isJY = P2;
                if (P2 == "Y")
                {
                    MODEL.JYDate = DateTime.Parse(JYDate);
                }
                else
                {
                    MODEL.JYDate = null;

                }

                new JH_Auth_UserB().Update(MODEL);
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// 获取可查看的社区类别
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSQLB(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                string strBranCode = UserInfo.User.BranchCode.ToString();
                var MODEL = new JH_Auth_ZiDianB().GetEntities(D => D.Class == 19);
                // msg.Result = MODEL.Where(D => D.Remark3.Contains(strBranCode));
                msg.Result = MODEL;

            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.Message;
            }
        }


        public void GETZXJLINDEXDATA(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                msg.Result = new SZHL_TSSQB().GetDTByCommand("SELECT TOP 12  A.CRUser,UserRealName,COUNT(A.id) AS zs FROM (SELECT CRUser,'TL' AS TL, ID FROM JH_Auth_TL UNION ALL  SELECT CRUser,'JL' AS TL, ID FROM SZHL_TSSQ   ) A  LEFT JOIN JH_Auth_User on A.CRUser=JH_Auth_User.UserName   GROUP BY A.CRUser,UserRealName ORDER BY COUNT(A.id) DESC");

                msg.Result1 = new SZHL_TSSQB().GetDTByCommand("SELECT  TOP 8 * FROM ( SELECT SZHL_TSSQ.ID,SZHL_TSSQ.HTNR,COUNT(JH_Auth_TL.ID)  ZS FROM SZHL_TSSQ INNER JOIN  JH_Auth_TL ON SZHL_TSSQ.ID=JH_Auth_TL.MSGTLYID   AND  JH_Auth_TL.MsgISShow!='N' GROUP BY SZHL_TSSQ.ID,SZHL_TSSQ.HTNR ) A ORDER BY ZS DESC");


            }
            catch (Exception ex)
            {

            }
        }




        public void GETZXJLYHLIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string userName = UserInfo.User.UserName;
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request["p"] ?? "1", out page);//页码
            int.TryParse(context.Request["pagecount"] ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;
            int recordCount = 0;
            string strWhere = string.Format("ComId={0} ", UserInfo.User.ComId);
            string strContent = context.Request["Content"] ?? "";
            strWhere = strWhere + string.Format(" AND  UserRealName like '%{0}%' ", strContent); ;
            DataTable dt = new JH_Auth_UserB().GetDataPager(@" JH_Auth_User ", " *", pagecount, page, " JH_Auth_User.UserOrder ", strWhere, ref recordCount);
            msg.Result = dt;
            msg.Result1 = recordCount;


        }




        public void GETWDSL(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            try
            {
                msg.Result = new SZHL_TSSQB().GetDTByCommand("SELECT COUNT(*) AS SL FROM SZHL_TSSQ WHERE  ','+RedUsers+',' NOT like '%," + UserInfo.User.UserName + ",%'");
            }
            catch (Exception ex)
            {

            }
        }
    }


    #endregion


}