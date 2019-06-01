using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QJY.Common
{
    public static class MyExtensions
    {
        public static int[] SplitTOInt(this string strs, char ch)
        {
            string[] arrstr = strs.Split(ch);
            int[] arrint = new int[arrstr.Length];
            for (int i = 0; i < arrstr.Length; i++)
            {
                arrint[i] = int.Parse(arrstr[i].ToString());
            }
            return arrint;
        }

        public static List<string> SplitTOList(this string strs, char ch)
        {
            List<string> Lstr = new List<string>();
            string[] arrstr = strs.Split(ch);
            int[] arrint = new int[arrstr.Length];
            for (int i = 0; i < arrstr.Length; i++)
            {
                Lstr.Add(arrstr[i].ToString());
            }
            return Lstr;
        }

        public static Dictionary<string, string> SplitTODictionary(this string strs, char ch, string strKey)
        {
            Dictionary<string, string> Lstr = new Dictionary<string, string>();
            string[] arrstr = strs.Split(ch);
            int[] arrint = new int[arrstr.Length];
            for (int i = 0; i < arrstr.Length; i++)
            {
                Lstr.Add(arrstr[i].ToString(), strKey);
            }
            return Lstr;
        }

        /// <summary>
        /// 将List转化为String
        /// </summary>
        /// <param name="Lists"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string ListTOString(this List<string> Lists, char ch)
        {
            string strReturn = "";
            foreach (var item in Lists)
            {
                strReturn = strReturn + item.ToString() + ch;
            }
            return strReturn.TrimEnd(ch);
        }

        /// <summary>
        /// 将List(int)转化为String
        /// </summary>
        /// <param name="Lists"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string ListTOString(this List<int> Lists, char ch)
        {
            string strReturn = "";
            foreach (var item in Lists)
            {
                strReturn = strReturn + item.ToString() + ch;
            }
            return strReturn.TrimEnd(ch);
        }
        /// <summary>
        /// 获取需要IN的格式
        /// </summary>
        /// <param name="strKys"></param>
        /// <returns></returns>
        public static string ToFormatLike(this string strKys)
        {
            StringBuilder sbKeys = new StringBuilder();
            foreach (var item in strKys.Split(','))
            {
                sbKeys.AppendFormat("'" + item.ToString() + "',");
            }
            return sbKeys.Length > 0 ? sbKeys.ToString().TrimEnd(',').Trim('\'') : "";
        }



        /// <summary>
        /// 获取需要IN的格式
        /// </summary>
        /// <param name="strKys"></param>
        /// <returns></returns>
        public static string ToFormatLike(this string strKys, char ch)
        {
            StringBuilder sbKeys = new StringBuilder();
            foreach (var item in strKys.Split(ch))
            {
                sbKeys.AppendFormat("'" + item.ToString() + "',");
            }
            return sbKeys.Length > 0 ? sbKeys.ToString().TrimEnd(',').Trim('\'') : "";
        }




        /// <summary>
        /// 取前N个字符,后面的用省略号代替
        /// </summary>
        /// <param name="strKys"></param>
        /// <returns></returns>
        public static string ToMangneStr(this string strKys, int intLenght)
        {

            return strKys.Length > intLenght ? strKys.Substring(0, intLenght) + "…………" : strKys;
        }



        public static DataTable ToDataTable<T>(this IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public static DataTable OrderBy(this DataTable dt, string orderBy)
        {
            dt.DefaultView.Sort = orderBy;
            return dt.DefaultView.ToTable();
        }
        public static DataTable Where(this DataTable dt, string where)
        {
            DataTable resultDt = dt.Clone();
            DataRow[] resultRows = dt.Select(where);
            foreach (DataRow dr in resultRows) resultDt.Rows.Add(dr.ItemArray);
            return resultDt;
        }


        /// <summary>
        /// Datatable转换为Json
        /// </summary>
        /// <param name="table">Datatable对象</param>
        /// <returns>Json字符串</returns>
        public static string ToJson(this DataTable dt)
        {
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            DataRowCollection drc = dt.Rows;
            for (int i = 0; i < drc.Count; i++)
            {
                jsonString.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string strKey = dt.Columns[j].ColumnName;
                    string strValue = drc[i][j].ToString();
                    Type type = dt.Columns[j].DataType;
                    jsonString.Append("\"" + strKey + "\":");
                    strValue = StringFormat(strValue, type);
                    if (j < dt.Columns.Count - 1)
                    {
                        jsonString.Append(strValue + ",");
                    }
                    else
                    {
                        jsonString.Append(strValue);
                    }
                }
                jsonString.Append("},");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            if (jsonString.Length != 0)
            {
                jsonString.Append("]");
            }
            return jsonString.ToString();
        }



        public static int ToInt32(this Object obj)
        {
            return Convert.ToInt32(obj);
        }


        public static DateTime ToDateTime(this Object obj)
        {
            return Convert.ToDateTime(obj);
        }

        public static T GetProperty<T>(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return (T)property.GetValue(obj, null);
            }
            throw new ArgumentNullException(propertyName);
        }



        /// <summary>
        /// DataTable转成Json
        /// </summary>
        /// <param name="jsonName"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(this DataTable dt, string jsonName)
        {
            StringBuilder Json = new StringBuilder();
            if (string.IsNullOrEmpty(jsonName))
                jsonName = dt.TableName;
            Json.Append("{\"" + jsonName + "\":[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Json.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Type type = dt.Rows[i][j].GetType();
                        Json.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + StringFormat(dt.Rows[i][j].ToString(), type));
                        if (j < dt.Columns.Count - 1)
                        {
                            Json.Append(",");
                        }
                    }
                    Json.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        Json.Append(",");
                    }
                }
            }
            Json.Append("]}");
            return Json.ToString();
        }

        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string String2Json(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化字符型、日期型、布尔型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
                str = String2Json(str);
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(Int32))
            {
                if (str.Trim() == "")
                {
                    str = "\"" + str + "\"";
                }
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            return str;
        }



        /// <summary>
        /// 过滤特殊字符
        /// 如果字符串为空，直接返回。
        /// </summary>
        /// <param name="str">需要过滤的字符串</param>
        /// <returns>过滤好的字符串</returns>
        public static string FilterSpecial(this string str)
        {
            if (str == "")
            {
                return str;
            }
            else
            {
                str = str.Replace("'", "");
                str = str.Replace("<", "");
                str = str.Replace(">", "");
                str = str.Replace("%", "");
                str = str.ToLower().Replace("'delete", "");
                str = str.ToLower().Replace("'truncate", "");
                str = str.Replace("''", "");
                str = str.Replace("\"\"", "");
                str = str.Replace(",", "");
                str = str.Replace(".", "");
                str = str.Replace(">=", "");
                str = str.Replace("=<", "");
                str = str.Replace(";", "");
                str = str.Replace("||", "");
                str = str.Replace("[", "");
                str = str.Replace("]", "");
                str = str.Replace("&", "");
                str = str.Replace("#", "");
                str = str.Replace("/", "");
                str = str.Replace("|", "");
                str = str.Replace("?", "");
                str = str.Replace(">?", "");
                str = str.Replace("?<", "");
                return str;
            }
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sCol">保存的列,其他删除</param>
        /// <returns></returns>
        public static DataTable DelTableCol(this DataTable dt, string sCol)
        {

            if (sCol == "")
                return dt;
            string[] sp = sCol.Split(',');
            string sqldd = "";
            for (int n = 0; n < sp.Length; n++)
            {
                sqldd += sp[n].Split('|')[0] + ',';
            }
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (!sqldd.Split(',').Contains(dt.Columns[i].ColumnName))
                {
                    dt.Columns.RemoveAt(i);
                    i = i - 1;
                }
                else
                {
                    for (int j = 0; j < sp.Length; j++)
                    {
                        string[] sg = sp[j].Split('|');
                        if (sg.Length > 1 && sg[0] == dt.Columns[i].ColumnName)
                        {
                            dt.Columns[i].ColumnName = sg[1];
                        }
                    }
                }
            }
            return dt;
        }

        /// <summary>   
        /// 根据条件过滤表   
        /// </summary>   
        /// <param name="dt">未过滤之前的表</param>   
        /// <param name="filter">过滤条件</param>   
        /// <returns>返回过滤后的表</returns>   
        public static DataTable FilterTable(this DataTable dt, string filter, string isSJ = "N")
        {

            DataTable newTable = dt.Clone();
            DataRow[] drs = dt.Select(filter);
            foreach (DataRow dr in drs)
            {
                newTable.Rows.Add(dr.ItemArray);
            }

            return newTable;
        }


        /// <summary>
        /// 随机排序
        /// </summary>
        /// <param name="newTable"></param>
        /// <returns></returns>
        public static DataTable SJTable(this DataTable newTable)
        {

            Random ran = new Random();
            newTable.Columns.Add("sort", typeof(int));
            for (int i = 0; i < newTable.Rows.Count; i++)
            {
                newTable.Rows[i]["sort"] = ran.Next(0, 100);
            }
            DataView dv = newTable.AsDataView();
            dv.Sort = "sort asc";
            newTable = dv.ToTable();
            return newTable;
        }


        /// <summary>
        /// dataTable分页
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static DataTable SplitDataTable(this DataTable dt, int PageIndex, int PageSize)
        {
            if (PageIndex == 0)
                return dt;
            DataTable newdt = dt.Clone();
            //newdt.Clear();
            int rowbegin = (PageIndex - 1) * PageSize;
            int rowend = PageIndex * PageSize;

            if (rowbegin >= dt.Rows.Count)
                return newdt;

            if (rowend > dt.Rows.Count)
                rowend = dt.Rows.Count;
            for (int i = rowbegin; i <= rowend - 1; i++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[i];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }
                newdt.Rows.Add(newdr);
            }

            return newdt;
        }
    }

}