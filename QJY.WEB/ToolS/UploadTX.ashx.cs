using System;
using System.Collections;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using System.Web.SessionState;
using QJY.Data;
using QJY.API;
using QJY.Common;

namespace QJY.WEB
{
    /// <summary>
    /// UploadFiles 的摘要说明
    /// </summary>

    public class UploadTX : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string strAction = context.Request.Form["Action"] ?? "";

            if (strAction == "TX")
            {
                string strUserName = context.Request.Form["UserName"] ?? "";
                UpTX(context, strUserName);
            }
            if (strAction == "ComIco")
            {
                UpTX(context, "SZHL");
            }

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        public void UpTX(HttpContext context, string strUserName)
        {
            try
            {
                Result result = new Result();
                result.avatarUrls = new ArrayList();
                result.success = false;
                result.msg = "Failure!";
                string tx_path = "\\Upload\\TX";
                tx_path = context.Server.MapPath(tx_path);//获取文件上传路径
                TXFileHelper.CreateDir(tx_path);
                //取服务器时间+8位随机码作为部分文件名，确保文件名无重复。
                string fileName = DateTime.Now.ToString("yyyyMMddhhmmssff") + CreateRandomCode(8);
                //定义一个变量用以储存当前头像的序号
                int avatarNumber = 1;
                string fileid = "";
                //遍历所有文件域
                foreach (string fieldName in context.Request.Files.AllKeys)
                {
                    HttpPostedFile file = context.Request.Files[fieldName];
                    //处理原始图片（默认的 file 域的名称是__source，可在插件配置参数中自定义。参数名：src_field_name）
                    //如果在插件中定义可以上传原始图片的话，可在此处理，否则可以忽略。
                    if (fieldName == "__source")
                    {
                        //文件名，如果是本地或网络图片为原始文件名（不含扩展名）、如果是摄像头拍照则为 *FromWebcam
                        fileName = file.FileName;
                        //当前头像基于原图的初始化参数（即只有上传原图时才会发送该数据），用于修改头像时保证界面的视图跟保存头像时一致，提升用户体验度。
                        //修改头像时设置默认加载的原图url为当前原图url+该参数即可，可直接附加到原图url中储存，不影响图片呈现。
                        string initParams = context.Request.Form["__initParams"];
                        result.sourceUrl = string.Format("upload/csharp_source_{0}.jpg", fileName);
                        // file.SaveAs(string.Format("{0}\\{1}", tx_path, strUserName + ".jpg"));
                        SaveTXFile(context, file, fieldName + ".jpg", ref fileid);
                        result.sourceUrl += initParams;
                        /*
                            可在此将 result.sourceUrl 储存到数据库，如果有需要的话。
                        */
                    }
                    //处理头像图片(默认的 file 域的名称：__avatar1,2,3...，可在插件配置参数中自定义，参数名：avatar_field_names)
                    else if (fieldName.StartsWith("__avatar1"))
                    {
                        string virtualPath = string.Format("upload/csharp_avatar{0}_{1}.jpg", avatarNumber, fileName);
                        result.avatarUrls.Add(virtualPath);
                        // file.SaveAs(string.Format("{0}\\{1}", tx_path, strUserName + ".jpg"));
                        SaveTXFile(context, file, fileName + ".jpg", ref fileid);
                        /*
                            可在此将 virtualPath 储存到数据库，如果有需要的话。
                        */
                        avatarNumber++;
                    }
                    else if (fieldName.IndexOf("__avatar") == -1 && fieldName != "__source")
                    {
                        tx_path = TXFileHelper.GetUploadPath() + "\\Log";
                        //  file.SaveAs(string.Format("{0}\\{1}", tx_path, strUserName + ".png"));
                        SaveTXFile(context, file, fileName + ".png", ref fileid);
                    }

                }
                result.success = true;
                result.msg = fileid;
                //返回图片的保存结果（返回内容为json字符串，可自行构造，该处使用Newtonsoft.Json构造）
                context.Response.Write(JsonConvert.SerializeObject(result));

            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message.ToString() + ex.StackTrace);
            }

        }
        public void SaveTXFile(HttpContext cxt, HttpPostedFile file, string fileName, ref string fileid)
        {
            if (cxt.Request.Cookies["szhlcode"] != null && cxt.Request.Cookies["szhlcode"].ToString() != "")
            {
                JH_Auth_UserB.UserInfo usermodel = new JH_Auth_UserB().GetUserInfo(cxt.Request.Cookies["szhlcode"].Value.ToString());
                string md5 = new CommonHelp().SaveFile(usermodel.QYinfo, fileName, file);
                FT_File newfile = new FT_File();
                newfile.ComId = usermodel.User.ComId;
                newfile.Name = fileName;
                newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                newfile.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                newfile.FileSize = "0";
                newfile.FileVersin = 0;
                newfile.CRDate = DateTime.Now;
                newfile.CRUser = usermodel.User.UserName;
                newfile.UPDDate = DateTime.Now;
                newfile.UPUser = usermodel.User.UserName;
                newfile.FileExtendName = "jpg";
                newfile.FolderID = 3;
                newfile.ISYL = "Y";
                new FT_FileB().Insert(newfile);
                usermodel.User.UserLogoId = newfile.ID;
                fileid = newfile.ID.ToString();
                new JH_Auth_UserB().Update(usermodel.User);
            }
        }
    
        private string GetFileType(string extname)
        {
            switch (extname.ToLower())
            {
                case ".gif":
                case ".jpg":
                case ".bmp":
                case ".jpeg":
                case ".tiff":
                case ".png":
                    return "pic";
                default:
                    return extname.ToLower().TrimStart('.');
            }
        }


        /// <summary>
        /// 生成指定长度的随机码。
        /// </summary>
        private string CreateRandomCode(int length)
        {
            string[] codes = new string[36] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            StringBuilder randomCode = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                randomCode.Append(codes[rand.Next(codes.Length)]);
            }
            return randomCode.ToString();
        }
        /// <summary>
        /// 表示图片的上传结果。
        /// </summary>
        private struct Result
        {
            /// <summary>
            /// 表示图片是否已上传成功。
            /// </summary>
            public bool success;
            /// <summary>
            /// 自定义的附加消息。
            /// </summary>
            public string msg;
            /// <summary>
            /// 表示原始图片的保存地址。
            /// </summary>
            public string sourceUrl;
            /// <summary>
            /// 表示所有头像图片的保存地址，该变量为一个数组。
            /// </summary>
            public ArrayList avatarUrls;
        }
    }

    /// <summary>
    ///FileHelper 的摘要说明
    /// </summary>
    public class TXFileHelper
    {
        public TXFileHelper()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }
        public static string GetUploadPath()
        {
            string path = HttpContext.Current.Server.MapPath("~/");
            string uploadDir = path + "images";
            CreateDir(uploadDir);
            return uploadDir;
        }


        private static string GetDirName()
        {
            return CommonHelp.GetConfig("uploaddir");
        }
        public static void CreateDir(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
    }

}