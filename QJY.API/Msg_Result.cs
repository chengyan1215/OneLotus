using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QJY.API
{
    /// <summary>
    /// 返回消息类
    /// </summary>
    public class Msg_Result
    {
        public string Action { get; set; }
        public string ErrorMsg { get; set; }
        public int DataLength { get; set; }
        public string ResultType { get; set; }
        public dynamic Result { get; set; }
        public dynamic Result1 { get; set; }
        public dynamic Result2 { get; set; }
        public dynamic Result3 { get; set; }
        public dynamic Result4 { get; set; }
        public dynamic Result5 { get; set; }
        public dynamic Result6 { get; set; }


    }
}
