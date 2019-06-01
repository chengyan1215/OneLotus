using SqlSugar;
using System.Collections.Generic;
using System.Data;

namespace QJY.Data
{
    public class DBFactory
    {
        private SqlSugarClient db;

        public DBFactory()
        {
        }

        public DBFactory(string ConnectionString)
        {
            var DbType = SqlSugar.DbType.SqlServer;
            db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,        //必填, 数据库连接字符串
                DbType = DbType,         //必填, 数据库类型
                IsAutoCloseConnection = true,               //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                InitKeyType = InitKeyType.SystemTable       //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
            });

        }

        public DBFactory(string DBType, string DBIP, string Port, string DBName, string Schema, string DBUser, string DBPwd)
        {
            string connectionString = "";
            var DbType = SqlSugar.DbType.SqlServer;
            if (DBType.ToLower() == "sqlserver")
            {
                if (string.IsNullOrEmpty(Port))
                {
                    Port = "1344";
                }
                DbType = SqlSugar.DbType.SqlServer;
                connectionString = string.Format("Data Source ={0},{4};Initial Catalog ={1};User Id ={2};Password={3};", DBIP, DBName, DBUser, DBPwd, Port);
            }
            else if (DBType.ToLower() == "mysql")
            {
                DbType = SqlSugar.DbType.MySql;
            }
            db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectionString,        //必填, 数据库连接字符串
                DbType = DbType,         //必填, 数据库类型
                IsAutoCloseConnection = true,               //默认false, 时候知道关闭数据库连接, 设置为true无需使用using或者Close操作
                InitKeyType = InitKeyType.SystemTable       //默认SystemTable, 字段信息读取, 如：该属性是不是主键，是不是标识列等等信息
            });

        }



        //测试连接
        public bool TestConn()
        {
            try
            {
                if (db.Ado.Connection.State != ConnectionState.Open)
                {
                    db.Ado.Connection.Open();
                }
                if (db.Ado.Connection.State == ConnectionState.Open)
                {
                    db.Ado.Connection.Close();
                    return true;
                }
            }
            catch { }

            return false;
        }

        public DataTable GetTables()
        {
            DataTable data = db.Ado.GetDataTable("select name ,type_desc from sys.objects where type_desc in ('USER_TABLE','VIEW') ORDER BY name");
            return data;
        }


        /// <summary>
        /// 获取表得字段信息
        /// </summary>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public DataTable GetFiledS(string strTableName)
        {
            DataTable data = db.Ado.GetDataTable(" sp_columns " + strTableName);
            return data;

        }

        public DataTable GetSQL(string strSQL)
        {
            DataTable data = db.Ado.GetDataTable(strSQL);
            return data;

        }

        /// <summary>
        /// 获取SQL模型数据
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="wd"></param>
        /// <param name="dl"></param>
        /// <param name="strTableFiled"></param>
        /// <param name="strDataCount"></param>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public DataTable GetYBData(BI_DB_Set DS, string wd, string dl, string strDataCount, string strWhere, string Order)
        {
            DataTable dt = new DataTable();
            string strSQL = "";

            string strCount = "";
            if (strDataCount != "0")
            {
                strCount = " TOP " + strDataCount;
            }


            string strOrder = "";
            if (Order != "")
            {
                strOrder = " ORDER BY " + Order;
            }

            if (wd == "")
            {
                strSQL = string.Format("select  {0}  {1} from ({2}) as DATESET  WHERE 1=1  {3}  {4}", strCount, dl, DS.DSQL, strWhere, strOrder);

            }
            else if (dl == "")
            {
                strSQL = string.Format("select  {0}  {1} from ({2}) as DATESET  WHERE 1=1  {3}  {4}", strCount, wd, DS.DSQL, strWhere, strOrder);

            }
            else
            {
                strSQL = string.Format("select  {0}  {1},{2} from ({3}) as DATESET  WHERE 1=1  {4}  group by {1} {5}", strCount, wd, dl, DS.DSQL, strWhere, strOrder);
            }
            dt = db.Ado.GetDataTable(strSQL);


            return dt;
        }



        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="strTable"></param>
        /// <returns></returns>
        public int InserData(Dictionary<string, object> DS, string strTable)
        {
            var t66 = db.Insertable(DS).AS(strTable).ExecuteReturnIdentity();
            return int.Parse(t66.ToString());

        }

        public int UpdateData(Dictionary<string, object> DS, string strTable)
        {
            var returndata = db.Updateable(DS).AS(strTable).ExecuteCommand();
            return int.Parse(returndata.ToString());

        }




        public bool CRTable(string strTablename, List<DbColumnInfo> ListCO)
        {
            try
            {
                db.DbMaintenance.CreateTable(strTablename, ListCO);

            }
            catch (System.Exception ex)
            {
                string msg = ex.Message.ToString();
                return false;
            }
            return true;
        }



    }
}

