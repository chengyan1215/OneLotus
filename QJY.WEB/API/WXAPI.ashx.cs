using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Web.SessionState;
using System.IO;
using System.Xml;
using QJY.API;
using QJY.Data;
using QJY.Common;
using Newtonsoft.Json.Linq;

namespace QJY.WEB
{
    /// <summary>
    /// WXAPI 的摘要说明
    /// </summary>
    public class WXAPI : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS,DELETE"); //支持的http 动作
            context.Response.AddHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type,authorization");
            context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            context.Response.AddHeader("pragma", "no-cache");
            context.Response.AddHeader("cache-control", "");
            context.Response.CacheControl = "no-cache";
            string strAction = context.Request["Action"] ?? "";
            string UserName = context.Request["UserName"] ?? "";
            string strIP = CommonHelp.getIP(context);

            Msg_Result Model = new Msg_Result() { Action = strAction.ToUpper(), ErrorMsg = "" };

            if (!string.IsNullOrEmpty(strAction))
            {

                #region 企业号应用callback
                if (strAction == "XXJS")
                {
                    String strCorpID = context.Request["corpid"] ?? "";
                    string strCode = context.Request["Code"] ?? "";
                    try
                    {
                        JH_Auth_QY jaq = new JH_Auth_QYB().GetALLEntities().FirstOrDefault();
                        JH_Auth_Model jam = new JH_Auth_ModelB().GetEntity(p => p.ModelCode == strCode);
                        //if (jaq != null && jam != null && !string.IsNullOrEmpty(jam.TJId))
                        if (jaq != null && jam != null)
                        {
                            #region POST
                            if (HttpContext.Current.Request.HttpMethod.ToUpper() == "POST")
                            {
                                string signature = HttpContext.Current.Request.QueryString["msg_signature"];//企业号的 msg_signature
                                string timestamp = HttpContext.Current.Request.QueryString["timestamp"];
                                string nonce = HttpContext.Current.Request.QueryString["nonce"];

                                // 获得客户端RAW HttpRequest  
                                StreamReader srResult = new StreamReader(context.Request.InputStream);
                                string str = srResult.ReadToEnd();
                                XmlDocument XmlDocument = new XmlDocument();
                                XmlDocument.LoadXml(HttpContext.Current.Server.UrlDecode(str));
                                string ToUserName = string.Empty;
                                string strde = string.Empty;
                                string msgtype = string.Empty;//微信响应类型
                                foreach (XmlNode xn in XmlDocument.ChildNodes[0].ChildNodes)
                                {
                                    if (xn.Name == "ToUserName")
                                    {
                                        ToUserName = xn.InnerText;
                                    }
                                }
                                var pj = new JH_Auth_WXPJB().GetEntity(p => p.TJID == jam.TJId);
                                //Tencent.WXBizMsgCrypt wxcpt = new Tencent.WXBizMsgCrypt(pj.Token, pj.EncodingAESKey, ToUserName);
                                //int n = wxcpt.DecryptMsg(signature, timestamp, nonce, str, ref strde);
                                XmlDocument XmlDocument1 = new XmlDocument();
                                XmlDocument1.LoadXml(HttpContext.Current.Server.UrlDecode(strde));
                                foreach (XmlNode xn1 in XmlDocument1.ChildNodes[0].ChildNodes)
                                {
                                    if (xn1.Name == "MsgType")
                                    {
                                        msgtype = xn1.InnerText;
                                    }
                                    //CommonHelp.WriteLOG(XmlDocument1.OuterXml);

                                }
                                if (msgtype == "event")//处理事件
                                {
                                    //需要处理进入应用的菜单更改事件
                                    string strEvent = XmlDocument1.ChildNodes[0]["Event"].InnerText;
                                    string strUserName = XmlDocument1.ChildNodes[0]["FromUserName"].InnerText;
                                    string strAgentID = XmlDocument1.ChildNodes[0]["AgentID"].InnerText;
                                    string strEventKey = XmlDocument1.ChildNodes[0]["EventKey"].InnerText;

                                    if (strEvent.ToLower() == "enter_agent" || strEvent.ToLower() == "view")
                                    {
                                        //进入应用和点击菜单
                                        //JH_Auth_User jau = new JH_Auth_UserB().GetEntity(p => p.ComId == jaq.ComId && p.UserName == strUserName);
                                        //JH_Auth_QY_Model jhqm = new JH_Auth_QY_ModelB().GetEntity(p => p.ComId == jaq.ComId && p.AgentId == strAgentID);
                                        //if (jau != null && jhqm != null)
                                        //{
                                        //    JH_Auth_YYLog jay = new JH_Auth_YYLog();
                                        //    jay.ComId = jaq.ComId;
                                        //    jay.AgentID = strAgentID;
                                        //    jay.CorpID = strCorpID;
                                        //    jay.CRDate = DateTime.Now;
                                        //    jay.CRUser = strUserName;
                                        //    jay.Event = strEvent;
                                        //    jay.EventKey = strEventKey;
                                        //    jay.ModelCode = strCode;
                                        //    jay.ModelID = jhqm.ModelID;
                                        //    jay.QYName = jaq.QYName;
                                        //    jay.TJID = jam.TJId;
                                        //    jay.Type = msgtype;
                                        //    jay.UserName = strUserName;
                                        //    jay.UserRealName = jau.UserRealName;

                                        //    new JH_Auth_YYLogB().Insert(jay);

                                        //    if (strEvent.ToLower() == "enter_agent")
                                        //    {
                                        //        var jays = new JH_Auth_YYLogB().GetEntities(p => p.ComId == jaq.ComId && p.Event == "enter_agent" && p.AgentID == strAgentID && p.CRUser == strUserName);
                                        //        if (jays.Count() <= 1)
                                        //        {
                                        //        }
                                        //    }
                                        //}

                                    }

                                }
                                if (new List<string> { "text", "image", "voice", "video", "shortvideo", "link" }.Contains(msgtype))//处理消息事件
                                {

                                    if (XmlDocument1.ChildNodes.Count > 0)
                                    {
                                        JH_Auth_WXMSG wxmsgModel = new JH_Auth_WXMSG();
                                        wxmsgModel.AgentID = int.Parse(XmlDocument1.ChildNodes[0]["AgentID"].InnerText);
                                        wxmsgModel.ComId = jaq.ComId;
                                        wxmsgModel.ToUserName = XmlDocument1.ChildNodes[0]["ToUserName"].InnerText;
                                        wxmsgModel.FromUserName = XmlDocument1.ChildNodes[0]["FromUserName"].InnerText;
                                        wxmsgModel.CRDate = DateTime.Now;
                                        wxmsgModel.CRUser = XmlDocument1.ChildNodes[0]["FromUserName"].InnerText;
                                        wxmsgModel.MsgId = XmlDocument1.ChildNodes[0]["MsgId"].InnerText;
                                        wxmsgModel.MsgType = msgtype;
                                        wxmsgModel.ModeCode = strCode;
                                        wxmsgModel.Tags = "微信收藏";

                                        switch (msgtype)
                                        {
                                            case "text":
                                                wxmsgModel.MsgContent = XmlDocument1.ChildNodes[0]["Content"].InnerText;
                                                break;
                                            case "image":
                                                wxmsgModel.PicUrl = XmlDocument1.ChildNodes[0]["PicUrl"].InnerText;
                                                wxmsgModel.MediaId = XmlDocument1.ChildNodes[0]["MediaId"].InnerText;
                                                break;
                                            case "voice":
                                                wxmsgModel.MediaId = XmlDocument1.ChildNodes[0]["MediaId"].InnerText;
                                                wxmsgModel.Format = XmlDocument1.ChildNodes[0]["Format"].InnerText;
                                                break;
                                            case "video":
                                                wxmsgModel.MediaId = XmlDocument1.ChildNodes[0]["MediaId"].InnerText;
                                                wxmsgModel.ThumbMediaId = XmlDocument1.ChildNodes[0]["ThumbMediaId"].InnerText;
                                                break;
                                            case "shortvideo":
                                                wxmsgModel.MediaId = XmlDocument1.ChildNodes[0]["MediaId"].InnerText;
                                                wxmsgModel.ThumbMediaId = XmlDocument1.ChildNodes[0]["ThumbMediaId"].InnerText;
                                                break;
                                            case "link":
                                                wxmsgModel.Description = XmlDocument1.ChildNodes[0]["Description"].InnerText;
                                                wxmsgModel.Title = XmlDocument1.ChildNodes[0]["Title"].InnerText;
                                                wxmsgModel.URL = XmlDocument1.ChildNodes[0]["Url"].InnerText;
                                                wxmsgModel.PicUrl = XmlDocument1.ChildNodes[0]["PicUrl"].InnerText;
                                                break;
                                        }
                                        if (new List<string>() { "link", "text" }.Contains(msgtype))
                                        {
                                            if (msgtype == "link")
                                            {
                                                var jaw = new JH_Auth_WXMSGB().GetEntity(p => p.ComId == jaq.ComId && p.MsgId == wxmsgModel.MsgId);
                                                if (jaw == null)
                                                {
                                                    string strMedType = ".jpg";
                                                    JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB.UserInfo();
                                                    UserInfo = new JH_Auth_UserB().GetUserInfo(jaq.ComId, wxmsgModel.FromUserName);
                                                    //  string fileID = CommonHelp.ProcessWxIMGUrl(wxmsgModel.PicUrl, UserInfo, strMedType);

                                                    //wxmsgModel.FileId = fileID;
                                                    //new JH_Auth_WXMSGB().Insert(wxmsgModel);

                                                    //if (strCode == "TSSQ")
                                                    //{
                                                    //    SZHL_TXSX tx1 = new SZHL_TXSX();
                                                    //    tx1.ComId = jaq.ComId;
                                                    //    tx1.APIName = "TSSQ";
                                                    //    tx1.MsgID = wxmsgModel.ID.ToString();
                                                    //    tx1.FunName = "SENDWXMSG";
                                                    //    tx1.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    //    tx1.CRUser = wxmsgModel.CRUser;
                                                    //    tx1.CRDate = DateTime.Now;
                                                    //    TXSX.TXSXAPI.AddALERT(tx1); //时间为发送时间
                                                    //}
                                                }
                                            }
                                            else
                                            {
                                                new JH_Auth_WXMSGB().Insert(wxmsgModel);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(wxmsgModel.MediaId))
                                        {
                                            var jaw = new JH_Auth_WXMSGB().GetEntity(p => p.ComId == jaq.ComId && p.MediaId == wxmsgModel.MediaId);
                                            if (jaw == null)
                                            {
                                                string strMedType = ".jpg";
                                                if (strCode == "QYWD" || strCode == "CRM")//判断模块
                                                {
                                                    if (msgtype == "shortvideo" || msgtype == "video")//视频,小视频
                                                    {
                                                        strMedType = ".mp4";
                                                    }
                                                    if (new List<string>() { "image", "shortvideo", "video", "voice" }.Contains(msgtype))//下载到本地服务器
                                                    {
                                                        JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB.UserInfo();
                                                        UserInfo = new JH_Auth_UserB().GetUserInfo(jaq.ComId, wxmsgModel.FromUserName);
                                                        //  string fileID = CommonHelp.ProcessWxIMG(wxmsgModel.MediaId, strCode, UserInfo, strMedType);
                                                        //  wxmsgModel.FileId = fileID;
                                                        // new JH_Auth_WXMSGB().Insert(wxmsgModel);
                                                    }

                                                }


                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region GET
                            if (HttpContext.Current.Request.HttpMethod.ToUpper() == "GET")
                            {
                                Auth(jam.Token, jam.EncodingAESKey, jaq.corpId);
                            }
                            #endregion

                        }
                    }
                    catch (Exception ex)
                    {
                        Model.ErrorMsg = ex.ToString();
                        CommonHelp.WriteLOG(ex.ToString());
                    }

                }
                #endregion

              

                #region 获取唯一code
                if (strAction.ToUpper() == "GetUserCodeByCode".ToUpper())
                {
                    #region 获取Code
                    Model.ErrorMsg = "获取Code错误，请重试";

                    string strCode = context.Request["code"] ?? "";
                    string strCorpID = context.Request["corpid"] ?? "";
                    string strModelCode = context.Request["funcode"] ?? "";

                    if (!string.IsNullOrEmpty(strCode))
                    {

                        var qy = new JH_Auth_QYB().GetEntity(p => p.corpId == strCorpID);
                        if (qy != null)
                        {
                            try
                            {

                                //通过微信接口获取用户名
                                WXHelp wx = new WXHelp(qy);
                                string username = wx.GetUserDataByCode(strCode, strModelCode);
                                CommonHelp.WriteLOG(username);
                                if (!string.IsNullOrEmpty(username))
                                {
                                    var jau = new JH_Auth_UserB().GetUserByUserName(qy.ComId, username);
                                    CommonHelp.WriteLOG(JsonConvert.SerializeObject(jau));

                                    if (jau != null)
                                    {
                                        //如果PCCode为空或者超过60分钟没操作,统统重新生成PCCode,并更新最新操作时间
                                        if (jau.logindate == null)
                                        {
                                            jau.logindate = DateTime.Now;
                                        }
                                        TimeSpan ts = new TimeSpan(jau.logindate.Value.Ticks).Subtract(new TimeSpan(DateTime.Now.Ticks)).Duration();
                                        if (string.IsNullOrEmpty(jau.pccode) || ts.TotalMinutes > 60)
                                        {
                                            string strGuid = CommonHelp.CreatePCCode(jau);
                                            jau.pccode = strGuid;
                                            jau.logindate = DateTime.Now;
                                            new JH_Auth_UserB().Update(jau);
                                        }
                                        Model.ErrorMsg = "";
                                        Model.Result = jau.pccode;
                                        Model.Result1 = jau.UserName;
                                        Model.Result2 = ts.TotalMinutes;
                                        Model.Result3 = qy.FileServerUrl;
                                    }

                                }
                                else
                                {
                                    Model.ErrorMsg = "当前用户不存在";
                                }
                            }
                            catch (Exception ex)
                            {
                                Model.ErrorMsg = ex.ToString();
                            }
                        }
                        else
                        {
                            Model.ErrorMsg = "当前企业号未在电脑端注册";
                        }

                    }
                    else
                    {
                        Model.ErrorMsg = "Code为空";
                    }
                    #endregion
                }
                #endregion
                #region 是否存在
                if (strAction.ToUpper() == "isexist".ToUpper())
                {
                    if (context.Request["szhlcode"] != null)
                    {
                        //通过Cookies获取Code
                        //string szhlcode = "5ab470be-4988-4bb3-9658-050481b98fca"; 
                        string szhlcode = context.Request["szhlcode"].ToString();
                        //通过Code获取用户名，然后执行接口方法
                        var jau = new JH_Auth_UserB().GetUserByPCCode(szhlcode);
                        if (jau == null)
                        {
                            Model.Result = "NOCODE";
                        }
                    }
                }
                #endregion
                #region 发送提醒
                if (strAction.ToUpper() == "AUTOALERT")
                {
                    TXSX.TXSXAPI.AUTOALERT();
                }
                //阿里云转码通知
                if (strAction.ToUpper() == "ZMNOTICE")
                {
                    #region 转码通知

                   
                    Stream stream = context.Request.InputStream;
                    StreamReader reader = new StreamReader(stream);
                    string text = reader.ReadToEnd();
                    reader.Close();

                    if (!string.IsNullOrEmpty(text))
                    {
                        JObject jo = JObject.Parse(text);
                        JObject message = JObject.Parse(jo["Message"].ToString());

                        string RunId = message["RunId"].ToString();
                        string State = message["State"].ToString();
                        if(State.ToUpper()== "SUCCESS")
                        {
                            JObject MediaWorkflowExecution = JObject.Parse(message["MediaWorkflowExecution"].ToString());
                            string InputFileobject = MediaWorkflowExecution["Input"]["InputFile"]["Object"].ToString();

                            if (MediaWorkflowExecution["State"].ToString().ToUpper() == "COMPLETED")
                            {
                                JArray ActivityList = JArray.Parse(MediaWorkflowExecution["ActivityList"].ToString());
                                foreach(var al in ActivityList)
                                {
                                    string alType = al["Type"].ToString();
                                    CommonHelp.WriteLOG("alType:" + alType);
                               
                                    if (alType.ToUpper() == "TRANSCODE")
                                    {
                                        string alname = al["Name"].ToString();
                                        string md5 = InputFileobject.Substring(0, InputFileobject.LastIndexOf("."));

                                        var files = new FT_FileB().GetEntities(p => p.FileMD5 == md5);
                                        foreach(var v in files)
                                        {
                                            v.YLUrl = string.Format("http://chengyanout.oss-cn-beijing.aliyuncs.com/{0}/{1}/{2}", alname, RunId, InputFileobject);
                                            new FT_FileB().Update(v);
                                        }                                                     

                                    }
                                }

                            }

                        }

                        //转码成功则删除原始文件
                        //OssClient client = new OssClient("",);

                        context.Response.Write("HTTP/1.1 204 No Content");
                        //}
                    }
                    context.Response.Write("HTTP/1.1 500 No Content");

                    #endregion
                }
                if (strAction.ToUpper() == "WXAPPSIGNATURE")//上传签名
                {
                    var sign = QJY.API.BusinessCode.Signature.GetUploadSignature();
                    Model.Result = sign;

                }
                if (strAction.ToUpper() == "CHECKBINDYH")//判断是否绑定账号
                {
                    //string code = context.Request["code"] ?? "";
                    //if (string.IsNullOrEmpty(code))
                    //{
                    //    Model.ErrorMsg = "请先获取微信code";
                    //}
                    //else
                    //{
                    //    string openid = WXApp.OnLogin(code);
                    //    if (openid == "")
                    //    {
                    //        Model.ErrorMsg = "获取openid失败,请重试";
                    //    }
                    //    else
                    //    {
                    //        Model.Result = openid;
                    //        //判断是否绑定
                    //        var user = new JH_Auth_UserB().GetEntity(p => p.weixinCard == openid);
                    //        if (user != null)
                    //        {
                    //            Model.Result1 = "Y";
                    //            Model.Result2 = user;
                    //        }
                    //    }

                    //}
                    

                }
                if (strAction.ToUpper() == "BINDYH")//绑定用户
                {

                    string password = context.Request["password"] ?? "";
                    string username = context.Request["UserName"] ?? "";
                    string wxopenid = context.Request["wxopenid"] ?? "";
                    string nickname = context.Request["nickname"] ?? "";
                    string txurl = context.Request["txurl"] ?? "";


                    JH_Auth_QY qyModel = new JH_Auth_QYB().GetALLEntities().First();
                    password = CommonHelp.GetMD5(password);
                    JH_Auth_User userInfo = new JH_Auth_User();

                    List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => (d.UserName == username || d.mobphone == username) && d.UserPass == password).ToList();
                    if (userList.Count() == 0)
                    {
                        Model.ErrorMsg = "用户名或密码不正确";
                    }
                    else
                    {
                        userInfo = userList[0];
                        if (userInfo.IsUse != "Y")
                        {
                            Model.ErrorMsg = "用户被禁用,请联系管理员";
                        }
                        if (Model.ErrorMsg == "")
                        {
                            userInfo.weixinCard = wxopenid;
                            userInfo.NickName = nickname;
                            userInfo.txurl = txurl;
                            new JH_Auth_UserB().Update(userInfo);
                            Model.Result = userInfo.pccode;
                            Model.Result1 = userInfo.UserName;
                            Model.Result2 = qyModel.FileServerUrl;
                            Model.Result4 = userInfo;
                        }

                    }
                }

                if (strAction.ToUpper() == "LOGIN")
                {
                    string password = context.Request["password"] ?? "";
                    string username = context.Request["UserName"] ?? "";
                    string chkcode = context.Request["chkcode"] ?? "";
                    Model.ErrorMsg = "";

                    if (chkcode.ToUpper() != "APP")
                    {
                        if (context.Session["chkcode"] != null)
                        {

                            if (!chkcode.ToUpper().Equals(context.Session["chkcode"].ToString()))
                            {
                                Model.ErrorMsg = "验证码不正确";
                            }
                        }
                        else
                        {
                            Model.ErrorMsg = "验证码已过期";
                        }
                    }



                    JH_Auth_QY qyModel = new JH_Auth_QYB().GetALLEntities().First();
                    password = CommonHelp.GetMD5(password);
                    JH_Auth_User userInfo = new JH_Auth_User();

                    List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => (d.UserName == username || d.mobphone == username) && d.UserPass == password).ToList();
                    if (userList.Count() == 0)
                    {
                        Model.ErrorMsg = "用户名或密码不正确";
                    }
                    else
                    {
                        userInfo = userList[0];
                        if (userInfo.IsUse != "Y")
                        {
                            Model.ErrorMsg = "用户被禁用,请联系管理员";
                        }
                        if (Model.ErrorMsg == "")
                        {
                            if (string.IsNullOrEmpty(userInfo.pccode))
                            {
                                userInfo.pccode = CommonHelp.CreatePCCode(userInfo);
                            }
                            userInfo.logindate = DateTime.Now;
                            new JH_Auth_UserB().Update(userInfo);
                            CacheHelp.Remove(userInfo.pccode);//登陆时清理缓存

                            Model.Result = userInfo.pccode;
                            Model.Result1 = userInfo.UserName;
                            Model.Result2 = qyModel.FileServerUrl;
                            Model.Result4 = userInfo;

                        }

                    }

                }

                #endregion
            }
            else
            {
                #region 获取SuiteTicket
                if (HttpContext.Current.Request.HttpMethod.ToUpper() == "POST")
                {

                    string signature = HttpContext.Current.Request.QueryString["msg_signature"];//企业号的 msg_signature
                    string timestamp = HttpContext.Current.Request.QueryString["timestamp"];
                    string nonce = HttpContext.Current.Request.QueryString["nonce"];

                    // 获得客户端RAW HttpRequest  
                    StreamReader srResult = new StreamReader(context.Request.InputStream);
                    string str = srResult.ReadToEnd();

                    XmlDocument XmlDocument = new XmlDocument();
                    XmlDocument.LoadXml(HttpContext.Current.Server.UrlDecode(str));

                    string ToUserName = string.Empty;
                    string Encrypt = string.Empty;

                    string strde = string.Empty;
                    string strinfotype = string.Empty;


                    foreach (XmlNode xn in XmlDocument.ChildNodes[0].ChildNodes)
                    {
                        if (xn.Name == "ToUserName")
                        {
                            ToUserName = xn.InnerText;
                        }
                        if (xn.Name == "Encrypt")
                        {
                            Encrypt = xn.InnerText;
                        }
                    }

                    var pj = new JH_Auth_WXPJB().GetEntity(p => p.TJID == ToUserName);

                  
                    int n = new WXHelp().DecryptMsg(pj.Token, pj.EncodingAESKey, ToUserName, signature, timestamp, nonce, str, ref strde);
                    string strtct = string.Empty;
                    string strSuiteId = string.Empty;
                    string strtAuthCorpId = string.Empty;

                    XmlDocument XmlDocument1 = new XmlDocument();
                    XmlDocument1.LoadXml(HttpContext.Current.Server.UrlDecode(strde));

                    foreach (XmlNode xn1 in XmlDocument1.ChildNodes[0].ChildNodes)
                    {
                        if (xn1.Name == "SuiteId")
                        {
                            strSuiteId = xn1.InnerText;
                        }
                        if (xn1.Name == "SuiteTicket")
                        {
                            strtct = xn1.InnerText;
                        }
                        if (xn1.Name == "InfoType")
                        {
                            strinfotype = xn1.InnerText;
                        }
                        if (xn1.Name == "AuthCorpId")
                        {
                            strtAuthCorpId = xn1.InnerText;
                        }
                    }
                    if (strinfotype == "suite_ticket")
                    {
                        pj.Ticket = strtct;

                        new JH_Auth_WXPJB().Update(pj);
                    }


                    HttpContext.Current.Response.Write("success");
                    HttpContext.Current.Response.End();
                }

                #endregion
            }

            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string Result = JsonConvert.SerializeObject(Model, Newtonsoft.Json.Formatting.Indented, timeConverter).Replace("null", "\"\"");
            context.Response.Write(Result);
        }

        /// <summary>
        /// 成为开发者的第一步，验证并相应服务器的数据
        /// </summary>
        private void Auth(string token, string encodingAESKey, string corpId)
        {

            string echoString = HttpContext.Current.Request.QueryString["echoStr"];
            string signature = HttpContext.Current.Request.QueryString["msg_signature"];//企业号的 msg_signature
            string timestamp = HttpContext.Current.Request.QueryString["timestamp"];
            string nonce = HttpContext.Current.Request.QueryString["nonce"];

            string decryptEchoString = "";
            if (CheckSignature(token, signature, timestamp, nonce, corpId, encodingAESKey, echoString, ref decryptEchoString))
            {
                if (!string.IsNullOrEmpty(decryptEchoString))
                {
                    Int64 v = Convert.ToInt64(decryptEchoString);
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.Write(v);
                    HttpContext.Current.Response.End();
                }
            }
        }

        #region 验证企业号签名
        /// <summary>
        /// 验证企业号签名
        /// </summary>
        /// <param name="token">企业号配置的Token</param>
        /// <param name="signature">签名内容</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">nonce参数</param>
        /// <param name="corpId">企业号ID标识</param>
        /// <param name="encodingAESKey">加密键</param>
        /// <param name="echostr">内容字符串</param>
        /// <param name="retEchostr">返回的字符串</param>
        /// <returns></returns>
        public bool CheckSignature(string token, string signature, string timestamp, string nonce, string corpId, string encodingAESKey, string echostr, ref string retEchostr)
        {
        
            int result = new WXHelp().CheckSignature(token, encodingAESKey, corpId, signature, timestamp, nonce, echostr, ref retEchostr);
            if (result != 0)
            {
                CommonHelp.WriteLOG("ERR: VerifyURL fail, ret: " + result);
                return false;
            }

            return true;

            //ret==0表示验证成功，retEchostr参数表示明文，用户需要将retEchostr作为get请求的返回参数，返回给企业号。
            // HttpUtils.SetResponse(retEchostr);
        }

        #endregion
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }



    }

}