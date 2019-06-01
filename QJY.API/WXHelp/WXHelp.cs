using QJY.Common;
using QJY.Data;
using Senparc.CO2NET.HttpUtility;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work.AdvancedAPIs.MailList.Member;
using Senparc.Weixin.Work.AdvancedAPIs.OaDataOpen;
using Senparc.Weixin.Work.AdvancedAPIs.OAuth2;
using Senparc.Weixin.Work.CommonAPIs;
using Senparc.Weixin.Work.Entities;
using Senparc.Weixin.Work.Tencent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace QJY.API
{
    public class WXHelp
    {


        public JH_Auth_QY Qyinfo = null;

        public WXHelp(JH_Auth_QY QY)
        {
            //获取企业信息
            Qyinfo = QY;
        }
        public WXHelp()
        {

        }

        public string GetToken(string appID = "")
        {

            if (Qyinfo.IsUseWX == "Y")
            {
                if (appID == "")
                {
                    AccessTokenResult Token = CommonApi.GetToken(Qyinfo.corpId.Trim(), Qyinfo.corpSecret.Trim());
                    return Token.access_token;
                }
                else
                {
                    JH_Auth_Model Model = new JH_Auth_ModelB().GetEntities(d => d.AppID == appID).FirstOrDefault();
                    string strcorpSecret = Qyinfo.corpSecret.Trim();
                    if (Model != null && !string.IsNullOrEmpty(Model.Remark1))
                    {
                        strcorpSecret = Model.Remark1.Trim();
                    }
                    AccessTokenResult Token = CommonApi.GetToken(Qyinfo.corpId.Trim(), strcorpSecret);
                    return Token.access_token;
                }
            }
            else
            {
                return "";
            }
        }

        public JsApiTicketResult GetTicket()
        {
            if (Qyinfo.IsUseWX == "Y")
            {
                JsApiTicketResult js = CommonApi.GetTicket(Qyinfo.corpId.Trim(), Qyinfo.corpSecret.Trim());
                return js;
            }
            return null;
        }

        public JsonGroupTicket GetGroup_Ticket()
        {
            string access_token = GetToken();
            var url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/ticket/get?access_token={0}&type=contact",
               access_token);


            JsonGroupTicket js = Get.GetJson<JsonGroupTicket>(url);
            return js;
        }

        #region 消息相关
        public void SendTH(List<Article> MODEL, string ModelCode, string type, string strUserS = "@all")
        {
            try
            {
                var app = new JH_Auth_ModelB().GetEntity(p => p.ModelCode == ModelCode);

                if (strUserS == "")
                {
                    return;
                }
                thModel th = new thModel();
                th.MODEL = MODEL;
                th.authAppID = app.AppID;
                th.UserS = string.IsNullOrEmpty(strUserS) ? "@all" : strUserS;
                if (Qyinfo.IsUseWX == "Y")
                {
                    th.MODEL.ForEach(d => d.Url = Qyinfo.WXUrl.TrimEnd('/') + "/View_Mobile/UI/UI_COMMON.html?funcode=" + ModelCode + "_" + type + (d.Url == "" ? "" : "_" + d.Url) + "&corpid=" + Qyinfo.corpId.Trim());
                    th.MODEL.ForEach(d => d.PicUrl = (string.IsNullOrEmpty(d.PicUrl) ? "" : Qyinfo.FileServerUrl.Trim() + Qyinfo.QYCode + "/document/image/" + new FT_FileB().ExsSclarSql("select zyid from FT_File where ID='" + d.PicUrl + "'").ToString()));

                    //if (app.AppType == "1")
                    //{
                    MassApi.SendNews(GetToken(app.AppID.ToString()), app.AppID, th.MODEL, th.UserS.Replace(',', '|'), "", "");
                    //}
                    //else
                    //{
                    //    MassApi.SendText(GetToken(app.AppID.ToString()), th.UserS.Replace(',', '|'), "", "", app.AppID, th.MODEL[0].Title);
                    //}
                }
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG(ex.ToString());
            }
        }

        public void SendTPMSG(string ModelCode, List<Article> MODEL, string strUserS = "@all")
        {
            try
            {
                var app = new JH_Auth_ModelB().GetEntity(p => p.ModelCode == ModelCode);

                if (strUserS == "")
                {
                    return;
                }
                if (Qyinfo.IsUseWX == "Y")
                {

                    MassApi.SendNews(GetToken(app.AppID.ToString()), app.AppID, MODEL, strUserS, "", "");
                }
            }
            catch { }
        }

        /// <summary>
        /// 文字消息
        /// </summary>
        /// <param name="MsgText"></param>
        /// <param name="strAPPID"></param>
        /// <param name="strUserS"></param>
        public void SendWXRText(string MsgText, string ModelCode, string strUserS = "@all")
        {
            try
            {
                var app = new JH_Auth_ModelB().GetEntity(p => p.ModelCode == ModelCode);

                if (strUserS == "")
                {
                    return;
                }
                if (Qyinfo.IsUseWX == "Y")
                {

                    MassApi.SendText(GetToken(app.AppID.ToString()), strUserS, "", "", app.AppID, MsgText);
                }
            }
            catch { }
        }
        /// <summary>
        /// 图片消息
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="strAPPID"></param>
        /// <param name="strUserS"></param>
        public void SendImage(string filePath, string strAPPID, string strUserS = "@all")
        {
            try
            {
                if (strUserS == "")
                {
                    return;
                }
                if (Qyinfo.IsUseWX == "Y")
                {
                    Senparc.Weixin.Work.AdvancedAPIs.Media.UploadTemporaryResultJson md = MediaApi.Upload(GetToken(), Senparc.Weixin.Work.UploadMediaFileType.image, filePath);
                    if (md.media_id != "")
                    {
                        MassApi.SendImage(GetToken(), strUserS, "", "", strAPPID, md.media_id);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 组织机构相关

        public string GetUserDataByCode(string strCode, string strModelCode)
        {
            string UserCode = "";
            strModelCode = strModelCode.Split('_')[0];
            try
            {
                if (Qyinfo.IsUseWX == "Y")
                {
                    JH_Auth_Model Model = new JH_Auth_ModelB().GetEntities(d => d.ModelCode == strModelCode).FirstOrDefault();
                    if (Model != null)
                    {
                        GetUserInfoResult OBJ = OAuth2Api.GetUserId(GetToken(Model.AppID), strCode);
                        UserCode = OBJ.UserId;

                    }
                }
            }
            catch (Exception EX)
            {
                new JH_Auth_LogB().Insert(new JH_Auth_Log() { CRDate = DateTime.Now, LogContent = strModelCode + "获取用户代码" + strCode + "|GetUserDataByCode" + EX.Message.ToString() });

            }

            return UserCode;
        }
        public long WX_CreateBranch(JH_Auth_Branch Model)
        {

            int pid = 0;
            var bm = new JH_Auth_BranchB().GetEntity(p => p.DeptCode == Model.DeptRoot && p.ComId == Model.ComId);
            if (bm != null)
            {
                pid = Int32.Parse(bm.WXBMCode.ToString());
            }
            return MailListApi.CreateDepartment(GetToken(), Model.DeptName, pid, Model.DeptShort, Model.WXBMCode).id;

        }
        //同步部门使用
        public long WX_CreateBranchTB(JH_Auth_Branch Model)
        {

            int pid = 0;
            var bm = new JH_Auth_BranchB().GetEntity(p => p.DeptCode == Model.DeptRoot && p.ComId == Model.ComId);
            if (bm != null)
            {
                pid = Int32.Parse(bm.WXBMCode.ToString());
            }
            return MailListApi.CreateDepartment(GetToken(), Model.DeptName, pid, Model.DeptShort).id;

        }
        public WorkJsonResult WX_UpdateBranch(JH_Auth_Branch Model)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                int pid = 0;
                var bm = new JH_Auth_BranchB().GetEntity(p => p.DeptCode == Model.DeptRoot && p.ComId == Model.ComId);
                if (bm != null)
                {
                    pid = Int32.Parse(bm.WXBMCode.ToString());
                }
                Ret = MailListApi.UpdateDepartment(GetToken(), long.Parse(Model.WXBMCode.ToString()), Model.DeptName, pid, Model.DeptShort);
            }
            return Ret;
        }

        public WorkJsonResult WX_DelBranch(string strDeptCode)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.DeleteDepartment(GetToken(), long.Parse(strDeptCode.ToString()));
            }
            return Ret;
        }
        public GetDepartmentListResult WX_GetBranchList(string strDeptCode)
        {
            GetDepartmentListResult Ret = new GetDepartmentListResult();
            int? id = null;
            if (!string.IsNullOrEmpty(strDeptCode))
            {
                id = Int32.Parse(strDeptCode);
            }
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.GetDepartmentList(GetToken(), id);
            }
            return Ret;
        }



        public WorkJsonResult WX_CreateUser(JH_Auth_User Model)
        {
            try
            {
                WorkJsonResult Ret = new WorkJsonResult();
                if (Qyinfo.IsUseWX == "Y")
                {
                    long[] Branch = { new JH_Auth_BranchB().GetEntity(d => d.DeptCode == Model.BranchCode).WXBMCode.Value };

                    MemberCreateRequest User = new MemberCreateRequest();
                    User.userid = Model.UserName;
                    User.name = Model.UserRealName;
                    User.mobile = Model.mobphone;
                    User.department = Branch;
                    User.gender = Model.Sex == "男" ? "1" : "2";
                    User.enable = Model.IsUse == "Y" ? 1 : 0;
                    Ret = MailListApi.CreateMember(GetToken(), User);
                }
                return Ret;
            }
            catch (Exception ex)
            {
                WorkJsonResult Ret = new WorkJsonResult();
                new QJY.API.JH_Auth_LogB().Insert(new QJY.Data.JH_Auth_Log() { CRDate = DateTime.Now, LogContent = Model.UserName + "新增错误：" + ex.ToString() });
                return Ret;
            }
        }
        /// <summary>
        /// 更新用户包括状态
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public WorkJsonResult WX_UpdateUser(JH_Auth_User Model)
        {
            try
            {
                WorkJsonResult Ret = new WorkJsonResult();
                if (Qyinfo.IsUseWX == "Y")
                {

                    long[] Branch = { new JH_Auth_BranchB().GetEntity(d => d.DeptCode == Model.BranchCode).WXBMCode.Value };
                    MemberUpdateRequest User = new MemberUpdateRequest();
                    User.userid = Model.UserName;
                    User.name = Model.UserRealName;
                    User.mobile = Model.mobphone;
                    User.department = Branch;
                    User.gender = Model.Sex == "男" ? "1" : "2";
                    User.enable = Model.IsUse == "Y" ? 1 : 0;
                    Ret = MailListApi.UpdateMember(GetToken(), User);
                }
                return Ret;
            }
            catch (Exception ex)
            {
                WorkJsonResult Ret = new WorkJsonResult();
                new QJY.API.JH_Auth_LogB().Insert(new QJY.Data.JH_Auth_Log() { CRDate = DateTime.Now, LogContent = Model.UserName + "更新错误：" + ex.ToString() });
                return Ret;
            }
        }

        public WorkJsonResult WX_DelUser(string strUserName)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.DeleteMember(GetToken(), strUserName);
            }
            return Ret;
        }

        public WorkJsonResult WX_GetDepartmentList()
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.GetDepartmentList(GetToken());
            }
            return Ret;
        }
        public WorkJsonResult WX_GetDepartmentMember(int depid)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.GetDepartmentMember(GetToken(), depid, 1);
            }
            return Ret;
        }
        public GetDepartmentMemberInfoResult WX_GetDepartmentMemberInfo(int depid)
        {
            GetDepartmentMemberInfoResult Ret = new GetDepartmentMemberInfoResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = MailListApi.GetDepartmentMemberInfo(GetToken(), depid, 1);
            }
            return Ret;
        }
        #endregion







        #region 菜单相关
        public WorkJsonResult WX_WxCreateMenuNew(int agentId, string ModelCode, ref List<Senparc.Weixin.Work.Entities.Menu.BaseButton> lm)
        {
            string strMenuURL = Qyinfo.WXUrl.TrimEnd('/') + "/View_Mobile/UI/UI_COMMON.html";
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {

                var list = new JH_Auth_CommonB().GetEntities(p => p.ModelCode == ModelCode && p.Type == "1").OrderBy(p => p.Sort);

                foreach (var l in list)
                {
                    string url = string.Empty;
                    string key = string.Empty;
                    url = strMenuURL + "?funcode=" + l.ModelCode + "_" + l.MenuCode + "&corpId=" + Qyinfo.corpId;
                    key = l.ModelCode;
                    lm.Add(GetButton(l.Type, l.MenuName, url, key));

                }
                if (lm.Count > 0)
                {
                    Senparc.Weixin.Work.Entities.Menu.ButtonGroup buttonData = new Senparc.Weixin.Work.Entities.Menu.ButtonGroup();
                    buttonData.button = lm;
                    Ret = WX_CreateMenu(agentId, buttonData);
                }
            }
            return Ret;
        }
        public WorkJsonResult WX_CreateMenu(int agentId, Senparc.Weixin.Work.Entities.Menu.ButtonGroup buttonData)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = CommonApi.CreateMenu(GetToken(agentId.ToString()), agentId, buttonData);
            }
            return Ret;
        }
        public GetMenuResult WX_GetMenu(int agentId)
        {
            GetMenuResult Ret = new GetMenuResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = CommonApi.GetMenu(GetToken(agentId.ToString()), agentId);
            }
            return Ret;
        }
        public WorkJsonResult WX_DelMenu(int agentId)
        {
            WorkJsonResult Ret = new WorkJsonResult();
            if (Qyinfo.IsUseWX == "Y")
            {
                Ret = CommonApi.DeleteMenu(GetToken(agentId.ToString()), agentId);
            }
            return Ret;
        }
        #endregion



        public string GetMediaFile(string mediaId, string strType = ".jpg")
        {
            string path = HttpContext.Current.Server.MapPath("\\temp\\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string mdfile = path + Guid.NewGuid().ToString() + strType;
            FileStream fs = new FileStream(mdfile, FileMode.Create);
            MediaApi.Get(GetToken(), mediaId, fs);
            fs.Close();
            return mdfile;
        }
        public Senparc.Weixin.Work.Entities.Menu.BaseButton GetButton(string type, string menuname, string url, string key)
        {
            Senparc.Weixin.Work.Entities.Menu.BaseButton bb = new Senparc.Weixin.Work.Entities.Menu.BaseButton();
            switch (type)
            {
                case "1": //跳转URL
                    Senparc.Weixin.Work.Entities.Menu.SingleViewButton svb = new Senparc.Weixin.Work.Entities.Menu.SingleViewButton();
                    svb.name = menuname;
                    svb.type = "view";
                    svb.url = url;

                    bb = svb;
                    break;
                case "2": //点击推事件
                    Senparc.Weixin.Work.Entities.Menu.SingleClickButton scb = new Senparc.Weixin.Work.Entities.Menu.SingleClickButton();
                    scb.name = menuname;
                    scb.type = "click";
                    scb.key = key;

                    bb = scb;
                    break;
                case "3"://扫码推事件
                    Senparc.Weixin.Work.Entities.Menu.SingleScancodePushButton spb = new Senparc.Weixin.Work.Entities.Menu.SingleScancodePushButton();
                    spb.name = menuname;
                    spb.type = "scancode_push";
                    spb.key = key;

                    bb = spb;
                    break;
                case "4"://扫码推事件且弹出“消息接收中”提示框
                    Senparc.Weixin.Work.Entities.Menu.SingleScancodeWaitmsgButton swb = new Senparc.Weixin.Work.Entities.Menu.SingleScancodeWaitmsgButton();
                    swb.name = menuname;
                    swb.type = "scancode_waitmsg";
                    swb.key = key;

                    bb = swb;
                    break;
                case "5"://弹出系统拍照发图
                    Senparc.Weixin.Work.Entities.Menu.SinglePicSysphotoButton ssb = new Senparc.Weixin.Work.Entities.Menu.SinglePicSysphotoButton();
                    ssb.name = menuname;
                    ssb.type = "pic_sysphoto";
                    ssb.key = key;

                    bb = ssb;
                    break;
                case "6"://弹出拍照或者相册发图
                    Senparc.Weixin.Work.Entities.Menu.SinglePicPhotoOrAlbumButton sab = new Senparc.Weixin.Work.Entities.Menu.SinglePicPhotoOrAlbumButton();
                    sab.name = menuname;
                    sab.type = "pic_photo_or_album";
                    sab.key = key;

                    bb = sab;
                    break;
                case "7"://弹出微信相册发图器
                    Senparc.Weixin.Work.Entities.Menu.SinglePicWeixinButton sxb = new Senparc.Weixin.Work.Entities.Menu.SinglePicWeixinButton();
                    sxb.name = menuname;
                    sxb.type = "pic_weixin";
                    sxb.key = key;

                    bb = sxb;
                    break;
                case "8"://弹出地理位置选择器
                    Senparc.Weixin.Work.Entities.Menu.SingleLocationSelectButton slb = new Senparc.Weixin.Work.Entities.Menu.SingleLocationSelectButton();
                    slb.name = menuname;
                    slb.type = "location_select";
                    slb.key = key;

                    bb = slb;
                    break;
            }
            return bb;
        }




        /// <summary>
        /// 获取微信审批数据
        /// </summary>
        /// <param name="strSDate"></param>
        /// <param name="strEDate"></param>
        /// <returns></returns>
        public GetApprovalDataJsonResult GetWXSHData(string strSDate, string strEDate, string strLastNum = "")
        {
            AccessTokenResult Token = CommonApi.GetToken(Qyinfo.corpId.Trim(), CommonHelp.GetConfig("WXLCDATA"));
            string strReturn = "";
            string access_token = Token.access_token;

            GetApprovalDataJsonResult obj = OaDataOpenApi.GetApprovalData(access_token, DateTime.Parse(strSDate), DateTime.Parse(strEDate), 0);

            return obj;
        }





        /// <summary>
        /// 修改用户性别
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="UserRealName"></param>
        /// <param name="gender">1为男，2为女</param>
        /// <returns></returns>
        public string UpUserXB(string UserName, string UserRealName, string gender = "1")
        {
            string responeJsonStr = "{";
            responeJsonStr += "\"userid\": \"" + UserName + "\",";
            responeJsonStr += "\"name\": \"" + UserRealName + "\",";
            responeJsonStr += "\"gender\": \"" + gender + "\"";
            responeJsonStr += "}";
            string accessToken = GetToken();
            string postUrl = string.Format("https://qyapi.weixin.qq.com/cgi-bin/user/update?access_token={0}", accessToken);
            return CommonHelp.PostWebRequest(postUrl, responeJsonStr, Encoding.UTF8);
        }



        public int DecryptMsg(string Token, string EncodingAESKey, string ToUserName, string signature, string timestamp, string nonce, string str, ref string strde)
        {
            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(Token, EncodingAESKey, ToUserName);
            int n = wxcpt.DecryptMsg(signature, timestamp, nonce, str, ref strde);
            return n;
        }


        public int CheckSignature(string token, string encodingAESKey, string corpId, string signature, string timestamp, string nonce, string echostr, ref string retEchostr)
        {
            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(token, encodingAESKey, corpId);
            int result = wxcpt.VerifyURL(signature, timestamp, nonce, echostr, ref retEchostr);
            return result;
        }


    }

    public class thModel
    {
        public List<Article> MODEL { get; set; }
        public string authAppID { get; set; }
        public int ID { get; set; }
        public string UserS { get; set; }
    }

    public class JsonGroupTicket
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }
        public string group_id { get; set; }
        public string ticket { get; set; }
        public string expires_in { get; set; }

    }







    #region 微信扫码登录所需的类
    public class GetProviderToken
    {
        /// <summary>
        /// 服务提供商的accesstoken
        /// </summary>
        public string provider_access_token { get; set; }

        /// <summary>
        /// access_token超时时间
        /// </summary>
        public int expires_in { get; set; }
    }
    #endregion
}