using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;


namespace QJY.API
{

    public class APIHelp
    {


        /// <summary>
        /// 添加部门和姓名列
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static DataTable GetUserNameAndDW(DataTable dt, int Comid, string strUserFiled = "CRUser")
        {

            if (strUserFiled != "")
            {
                dt.Columns.Add("UserRealName");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["UserRealName"] = new JH_Auth_UserB().GetUserRealName(Comid, dt.Rows[i][strUserFiled].ToString());


                }
            }
            if (!dt.Columns.Contains("DWName"))
            {
                dt.Columns.Add("DWName");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["DWName"] = new JH_Auth_UserB().GetUserDWName(Comid, dt.Rows[i][strUserFiled].ToString());

                }
            }

            return dt;
        }


        public static DataTable GetDWByUserName(DataTable dt, int Comid, string strUserFiled = "CRUser")
        {
            if (strUserFiled != "")
            {
                dt.Columns.Add("DWName");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strUserName = dt.Rows[i][strUserFiled].ToString();
                    dt.Rows[i]["DWName"] = new JH_Auth_UserB().GetUserDWName(Comid, strUserName);

                }
            }

            return dt;
        }


        /// <summary>
        /// 检测文本是否违规
        /// </summary>
        /// <param name="strWZ"></param>
        /// <returns></returns>
        public static string TestWB(string strWZ)
        {
          
            string strCode = "0";
        
            return strCode;
        }


    }
}
