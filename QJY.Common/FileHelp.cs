using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Drawing;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using QJY.Data;

namespace QJY.Common
{
    public class FileHelp
    {



        /// <summary>
        /// 压缩需求
        /// </summary>
        /// <param name="strFileData"></param>
        /// <param name="UserInfo"></param>
        /// <returns></returns>
        public string CompressZip(string strFileData, JH_Auth_QY QYinfo)
        {
            Dictionary<String, String> DATA = new Dictionary<String, String>();
            DATA.Add("data", strFileData);
            HttpWebResponse ResponseData = CommonHelp.CreatePostHttpResponse(QYinfo.FileServerUrl.TrimEnd('/') + "/" +QYinfo.QYCode + "/document/zipfolder", DATA, 0, "", null);
            return CommonHelp.GetResponseString(ResponseData);
        }


        /// <summary>
        /// 注册新的文件接口
        /// 传入三个参数，qycode name 和description  第一个不能为空， 后两个可以为空，  第一个如果已经插入，就返回错误， 插入正确就返回true
        /// </summary>
        /// <param name="qycode"></param>
        /// <param name="strQYName"></param>
        /// <returns></returns>
        public void AddQycode(string qycode, string strQYName)
        {
            Dictionary<String, String> DATA = new Dictionary<String, String>();
            DATA.Add("qycode", qycode);
            DATA.Add("name", strQYName);
            string strFileAPIRegUrl = CommonHelp.GetConfig("FileAPIReg").ToString() + "addqycode";
            HttpWebResponse ResponseData = CommonHelp.CreatePostHttpResponse(strFileAPIRegUrl, DATA, 0, "", null);
            CommonHelp.GetResponseString(ResponseData);
        }
        //获取文件服务器
        public string GetFileServerUrl(string qycode)
        {
            string strFileAPIRegUrl = CommonHelp.GetConfig("FileAPIReg").ToString() + qycode + "/document/";
            return strFileAPIRegUrl;
        }
    }
}
