
using QJY.API;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.UI;

namespace QJY.WEB
{
    public partial class DownFile : Page
    {

        public string type
        {
            get { return Request["type"] ?? "file"; }
        }

        public string Name
        {
            get { return Request["Name"] ?? "测试文件"; }
        }
        public string MD5
        {
            get { return Request["MD5"] ?? ""; }
        }
        public string FileId
        {
            get { return Request["fileId"] ?? ""; }
        }
        public string szhlcode
        {
            get { return Request["szhlcode"] ?? ""; }
        }
        public string userName
        {
            get { return Request["user"] ?? ""; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                JH_Auth_QY QYMODEL = new JH_Auth_QYB().GetALLEntities().FirstOrDefault();

                if (szhlcode != "")
                {
                    if (!string.IsNullOrEmpty(FileId))//如果FileID不为空
                    {
                        string filename = "";
                        int fileId = int.Parse(FileId.Split(',')[0]);
                        FT_File file = new FT_FileB().GetEntity(d => d.ID == fileId);
                        if (type == "file") //默认下载文件
                        {
                            List<string> extends = new List<string>() { "jpg", "png", "gif", "jpeg" };
                            if (extends.Contains(file.FileExtendName.ToLower()))
                            {
                                string width = Request["width"] ?? "";
                                string height = Request["height"] ?? "";
                                Response.AddHeader("Content-Disposition", "attachment;filename=" + file.Name);
                                Response.ContentType = "application/octet-stream";
                                filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/" + file.zyid;
                                if (width + height != "")
                                {
                                    filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/image/" + file.zyid + (width + height != "" ? ("/" + width + "/" + height) : "");
                                }
                                Response.Redirect(filename);

                            }
                            else
                            {
                                Response.AddHeader("Content-Disposition", "attachment;filename=" + Name);
                                Response.ContentType = "application/octet-stream";
                                filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/" + file.zyid;
                                Response.Redirect(filename);
                            }
                        }
                        else //返回代表类型的图片
                        {
                            Response.AddHeader("Content-Disposition", "attachment;filename=" + Name);
                            Response.ContentType = "application/octet-stream";
                            filename = "/ViewV5/images/qywd/" + file.FileExtendName + ".png";
                            Response.Redirect(filename);
                        }
                    }
                    else
                    {
                        if (type == "folder" && MD5 != "")//下载压缩文件
                        {

                            Response.AddHeader("Content-Disposition", "attachment;filename=" + Name);
                            Response.ContentType = "application/octet-stream";
                            string filename = QYMODEL.FileServerUrl + "/zipfile/" + MD5;
                            Response.Redirect(filename);

                        }
                        if (type == "video" && MD5 != "")
                        {
                            string url = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/" + MD5;
                            Byte[] bytes = new WebClient().DownloadData(url);
                            Response.OutputStream.Write(bytes, 0, bytes.Length);

                        }
                        if (type == "TX" && !string.IsNullOrEmpty(userName))//获取用户头像
                        {
                            var userinfo = new JH_Auth_UserB().GetEntity(p => p.UserName == userName); //抓取当前用户信息
                            //JH_Auth_User userinfo = UserInfo.User;
                            if (userinfo != null)
                            {
                                string filename = "";
                                if (userinfo.UserLogoId != null)
                                {
                                    FT_File file = new FT_FileB().GetEntity(d => d.ID == userinfo.UserLogoId);
                                    List<string> extends = new List<string>() { "jpg", "png", "gif", "jpeg" };
                                    if (!extends.Contains(file.FileExtendName.ToLower()))//文件不是图片的不返回地址，此方法只用于图片查看
                                    {
                                        return;
                                    }
                                    filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/" + file.zyid;
                                    Response.AddHeader("Content-Disposition", "attachment;filename=" + file.Name);
                                    Response.ContentType = "application/octet-stream";
                                }
                                else if (!string.IsNullOrEmpty(userinfo.txurl))
                                {
                                    Response.AddHeader("Content-Disposition", "attachment;filename=" + Name);
                                    Response.ContentType = "application/octet-stream";
                                    filename = userinfo.txurl;

                                }
                                else
                                {

                                    Response.AddHeader("Content-Disposition", "attachment;filename=" + Name);
                                    Response.ContentType = "application/octet-stream";
                                    filename = "/ViewV5/images/tx.png";
                                }
                                Response.Redirect(filename);
                                return;
                            }
                        }
                    }



                }




            }
            catch (Exception ex) { }
            // Response.ContentType = "application/x-zip-compressed";
        }
    }
}