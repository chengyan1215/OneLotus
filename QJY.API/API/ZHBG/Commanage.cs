using FastReflectionLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;


namespace QJY.API
{
    public class Commanage : IWsService
    {
        public void ProcessRequest(HttpContext context, ref Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            MethodInfo methodInfo = typeof(Commanage).GetMethod(msg.Action.ToUpper());
            Commanage model = new Commanage();
            methodInfo.FastInvoke(model, new object[] { context, msg, P1, P2, UserInfo });
        }


        #region 官网登录和注册




        public void CHECKREGISTERPHONE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            var qy2 = new JH_Auth_QYB().GetEntities(p => p.Mobile == P1.Trim());
            if (qy2.Count() > 0)
            {
                msg.ErrorMsg = "此手机已注册企业，请更换手机号继续注册";
            }
        }



        public void REGISTERYSOLD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            JH_Auth_QY QY = new JH_Auth_QYB().GetEntity(d => d.ComId == 10334);
            QY.CRDate = DateTime.Now.AddYears(-2);
            QY.QYProfile = QY.FileServerUrl;
            QY.FileServerUrl = "";
            QY.IsUseWX = "N";
            new JH_Auth_QYB().Update(QY);
            msg.Result = QY;

        }

        public void REGISTERYS(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            string strXM = P2;
            string strPhone = P1;
            JH_Auth_User user1 = new JH_Auth_UserB().GetUserByUserName(10334, P1);
            if (user1 != null)
            {
                msg.ErrorMsg = "用户已存在";
                return;
            }
            JH_Auth_User user = new JH_Auth_User();
            user.UserName = strPhone;
            user.mobphone = strPhone;
            user.UserRealName = P2;
            user.UserPass = CommonHelp.GetMD5("abc123");
            user.ComId = 10334;
            user.BranchCode = 1728;
            user.CRDate = DateTime.Now;
            user.CRUser = "administrator";
            user.logindate = DateTime.Now;
            user.IsUse = "Y";
            if (!new JH_Auth_UserB().Insert(user))
            {
                msg.ErrorMsg = "添加用户失败";
            }
            else
            {

                JH_Auth_QY QY = new JH_Auth_QYB().GetEntity(d => d.ComId == 10334);
                WXHelp wx = new WXHelp(QY);
                wx.WX_CreateUser(user);

                //添加默认员工角色
                JH_Auth_UserRole Model = new JH_Auth_UserRole();
                Model.UserName = user.UserName;
                Model.RoleCode = 1219;
                Model.ComId = user.ComId;
                new JH_Auth_UserRoleB().Insert(Model);

            }
        }
        /// <summary>
        /// 发送手机验证码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SENDCHKMSG(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //if (!string.IsNullOrEmpty(P1))
            //{
            //    string code = CommonHelp.numcode(4);
            //    try
            //    {
            //        string type = context.Request["type"] ?? "";
            //        string content = "";
            //        switch (type)
            //        {
            //            case "changeadmin":
            //                content = "您更换超级管理员的验证码为：" + code + "，如非本人操作，请忽略本短信";
            //                break;
            //            default:
            //                content = "注册验证码：" + code + "，如非本人操作，请忽略本短信";
            //                break;

            //        }

            //        CommonHelp.SendSMS(P1, content, 0);
            //        msg.Result = CommonHelp.GetMD5(code);
            //    }
            //    catch
            //    {
            //        msg.ErrorMsg = "发送验证码失败";
            //    }
            //}
        }
        /// <summary>
        /// 验证企业名称
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void YZQYMC(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            if (!string.IsNullOrEmpty(P1))
            {
                var qy = new JH_Auth_QYB().GetEntity(p => p.QYName == P1);
                if (qy != null)
                {
                    msg.ErrorMsg = "企业名称已存在";
                }
            }
        }


        #endregion


        #region 评论


        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETQYINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string qycode = context.Request["qycode"];
            //string qycode = "2072782222";
            JH_Auth_QY Qyinfo = new JH_Auth_QYB().GetEntity(d => d.QYCode == qycode);
            if (Qyinfo == null)
            {
                msg.ErrorMsg = "没有找到该企业";
            }
            else
            {
                msg.Result = Qyinfo;


                //WXHelp wx = new WXHelp(Qyinfo);
                //var list = wx.GetAppList().agentlist.Select(p => new { id = p.agentid, info = wx.GetAPPinfo(Int32.Parse(p.agentid)) });
                //wx.WX_WxCreateMenuNew(138, "GZBG");
                //msg.Result1 = list;
            }
        }
        #endregion



        #region 找回密码
        public void CHECKPHONE(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {

            //string ComId = context.Request["comId"] ?? "";
            //int id = 0;
            //int.TryParse(ComId, out id);
            //if (id > 0)
            //{
            //    List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.mobphone == P2 && d.ComId == id).ToList();
            //    JH_Auth_User user = new JH_Auth_UserB().GetEntity(d => d.mobphone == P2 && d.ComId == id);
            //    if (userList.Count != 1)
            //    {
            //        msg.ErrorMsg = "此手机号无效";
            //    }
            //}
            //else
            //{
            //    List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => d.mobphone == P2).ToList();
            //    if (userList.Count == 0)
            //    {
            //        msg.ErrorMsg = "此手机号无效";
            //    }
            //    else if (userList.Count > 1)
            //    {
            //        msg.ErrorMsg = "-1";
            //    }
            //}

        }
        //找回密码验证二级域名
        public void FINDYZQYYM(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //if (!string.IsNullOrEmpty(P1))
            //{
            //    if (P1.ToLower() == "www" || P1.ToLower() == "saas") //www及saas不让用户注册
            //    {
            //        msg.ErrorMsg = "二级域名无效";
            //        return;
            //    }
            //    var qy = new JH_Auth_QYB().GetEntity(p => p.QYCode == P1);
            //    if (qy == null)
            //    {
            //        msg.ErrorMsg = "二级域名不存在";
            //    }
            //    else
            //    {
            //        msg.Result = qy.ComId;
            //    }
            //}
        }
        public void FINDPASSWORD(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            //string userpass = context.Request["pass"];
            //if (P1 == "1")
            //{
            //    string ComId = context.Request["ComId"];
            //    int qyId = 0;
            //    int.TryParse(ComId, out qyId);
            //    JH_Auth_QY qymodel = new JH_Auth_QYB().GetEntity(d => d.ComId == qyId);
            //    if (qymodel != null)
            //    {
            //        JH_Auth_User userInfo = new JH_Auth_UserB().GetEntity(d => d.mobphone == P2 && d.ComId == qymodel.ComId);
            //        userInfo.UserPass = CommonHelp.GetMD5(userpass);
            //        new JH_Auth_UserB().Update(userInfo);
            //        msg.Result = qymodel.QYCode;
            //    }
            //}
            //else
            //{
            //    JH_Auth_User userInfo = new JH_Auth_UserB().GetEntity(d => d.mobphone == P2);
            //    userInfo.UserPass = CommonHelp.GetMD5(userpass);
            //    new JH_Auth_UserB().Update(userInfo);
            //}
        }
        #endregion


        #region 分享查看文档操作
        /// <summary>
        /// 获取文档资源
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWDZY(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            FT_File ff = new FT_FileB().GetEntities(p => p.zyid == P1).FirstOrDefault();
            JH_Auth_QY QY = new JH_Auth_QYB().GetALLEntities().FirstOrDefault();

            if (ff != null)
            {
                HttpWebResponse ResponseData = CommonHelp.CreateHttpResponse(QY.FileServerUrl + "/document/officecov/" + ff.zyid, null, 0, "", null, "GET");
                string Returndata = CommonHelp.GetResponseString(ResponseData);
                msg.Result = Returndata.ToString().Split(',')[1];
                msg.Result1 = ff.Name;
                msg.Result2 = QY.FileServerUrl;

            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }
        public void GETSHAREINFO(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = 0;
            int.TryParse(P1, out ID);
            if (ID > 0)
            {
                FT_File_Share Model = new FT_File_ShareB().GetEntity(d => d.ID == ID);


                if (Model.SharePasd == P2 || Model.ShareType == "0")//公开链接或者输入提取码正确
                {
                    string strSql = string.Format(@"SELECT qy.QYName,qy.LogoID,share.CRUserName,share.RefType,share.ShareDueDate,share.CRDate,CASE WHEN share.RefType='file' then f.Name+'.'+f.FileExtendName WHEN share.RefType='wj'  THEN folder.Name END Name 
                                            ,CASE WHEN share.RefType='file' then f.ID WHEN share.RefType='wj'  THEN folder.ID END  ID ,f.FileExtendName,f.FileSize,share.ComId,f.ISYL,f.FileMD5,qy.FileServerUrl,f.YLUrl
                                            from FT_File_Share share INNER join JH_Auth_QY  qy on share.ComId=qy.ComId
                                            LEFT join FT_File f on share.RefID=f.ID and share.ComId=f.ComId and share.RefType='file'
                                            LEFT join  FT_Folder folder on share.RefID=folder.ID and share.RefType='wj' where share.ID={0} and share.IsDel!='Y'", ID);
                    DataTable dt = new FT_File_ShareB().GetDTByCommand(strSql);
                    if (dt.Rows.Count > 0)
                    {
                        DateTime dueDate = DateTime.Now;
                        if (DateTime.TryParse(dt.Rows[0]["ShareDueDate"].ToString(), out dueDate) && dueDate > DateTime.Now)
                        {
                            msg.Result = dt;
                        }
                        else
                        {
                            msg.ErrorMsg = "分享已过期";
                        }
                    }
                    else
                    {
                        msg.ErrorMsg = "分享已取消";
                    }
                }
                else
                {
                    msg.Result = 1;
                    msg.ErrorMsg = "提取码有误，请重新输入";
                }

            }
        }
        //分享页面根据文件夹Id获取文件列表
        public void GETFILELIST(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int FolderID = int.Parse(P1);//
            int ComId = int.Parse(P2);
            msg.Result = new FT_FolderB().GetEntities(d => d.ComId == ComId && d.PFolderID == FolderID);
            msg.Result1 = new FT_FileB().GetEntities(d => d.ComId == ComId && d.FolderID == FolderID);
            return;
        }


        /// <summary>
        /// 判断是否公开链接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ISPUBLIC(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);//
            FT_File_Share Model = new FT_File_ShareB().GetEntity(d => d.ID == ID && d.IsDel != "Y");
            msg.Result = Model.ShareType;
        }
        /// <summary>
        /// 向服务器发送压缩目录命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">目录ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COMPRESSFOLDER(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strCode = P1;
            int ComId = int.Parse(P2);
            FT_FolderB.FoldFile Mode = new FT_FolderB.FoldFile();
            Mode.FolderID = -1;
            Mode.Name = "压缩文件";
            Mode.SubFileS = new List<FT_File>();
            Mode.SubFolder = new List<FT_FolderB.FoldFile>();
            foreach (string item in P1.SplitTOList(','))
            {
                int FileID = int.Parse(item.Split('|')[0].ToString());
                string strType = item.Split('|')[1].ToString();
                if (item.Split('|')[1].ToString() == "file")
                {
                    FT_File file = new FT_FileB().GetEntity(d => d.ID == FileID);
                    file.YLUrl = "";
                    Mode.SubFileS.Add(file);
                }
                else
                {
                    List<FT_FolderB.FoldFileItem> ListID = new List<FT_FolderB.FoldFileItem>();
                    FT_FolderB.FoldFile obj = new FT_FolderB().GetWDTREE(FileID, ref ListID, ComId);
                    Mode.SubFolder.Add(obj);
                }
            }
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string Result = JsonConvert.SerializeObject(Mode, Newtonsoft.Json.Formatting.Indented, timeConverter).Replace("null", "\"\"");
            JH_Auth_QY qymodel = new JH_Auth_QYB().GetEntity(d => d.ComId == ComId);
            //压缩文件
            Dictionary<String, String> DATA = new Dictionary<String, String>();
            DATA.Add("data", Result);
            HttpWebResponse ResponseData = CommonHelp.CreatePostHttpResponse(qymodel.FileServerUrl.TrimEnd('/') + "/" + qymodel.QYCode + "/document/zipfolder", DATA, 0, "", null);
            string strData = CommonHelp.GetResponseString(ResponseData);
            msg.Result = strData;
        }

        /// <summary>
        /// 获取页面html(excel)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETHTML(HttpContext context, Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            string strWDYM = CommonHelp.GetConfig("WDYM");

            FT_File ff = new FT_FileB().GetEntities(p => p.YLCode == P1).FirstOrDefault();
            if (ff != null)
            {
                //定义局部变量
                HttpWebRequest httpWebRequest = null;
                HttpWebResponse httpWebRespones = null;
                Stream stream = null;
                string htmlString = string.Empty;
                string url = strWDYM + ff.YLPath;

                //请求页面
                try
                {
                    httpWebRequest = WebRequest.Create(url + ".html") as HttpWebRequest;
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "建立页面请求时发生错误！";
                }
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
                //获取服务器的返回信息
                try
                {
                    httpWebRespones = (HttpWebResponse)httpWebRequest.GetResponse();
                    stream = httpWebRespones.GetResponseStream();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "接受服务器返回页面时发生错误！";
                }

                StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                //读取返回页面
                try
                {
                    htmlString = streamReader.ReadToEnd();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "读取页面数据时发生错误！";
                }
                //释放资源返回结果
                streamReader.Close();
                stream.Close();

                msg.Result = htmlString;
                msg.Result1 = url;

            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }
        #endregion




        public string strBLUrl(string strVid)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            string ptime = t.ToString();

            string vid = strVid;
            string s = "format=json&ptime=" + ptime + "&vid=" + strVid + "Ur4a8Jn7XU";
            string strQM = s.Sha1Sign().ToUpper();

            Dictionary<String, String> DATA = new Dictionary<String, String>();
            DATA.Add("vid", vid);
            DATA.Add("ptime", ptime);
            DATA.Add("sign", strQM);
            DATA.Add("format", "json");
            //   DATA.Add("userid", "33074c0252");

            string strFileAPIRegUrl = "http://api.polyv.net/v2/video/33074c0252/get-video-msg?format=json&vid=" + vid + "&ptime=" + ptime + "&sign=" + strQM;
            string st = CommonHelp.HttpGet(strFileAPIRegUrl);

            JObject wigdata = JObject.Parse(st);

            JArray categories = JArray.Parse(wigdata["data"].ToString());
            string strMP4 = (string)categories[0]["mp4_1"];
            return strMP4;
        }









    }
}