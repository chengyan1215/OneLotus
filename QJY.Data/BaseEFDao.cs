using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace QJY.Data
{
    /// <summary>
    /// 公共接口
    /// </summary>
    public class BaseEFDao<T> : IBaseDao<T> where T : class,new()   //限制T为class
    {


        private DbContext GetDbContext()
        {
            return new QJY_SaaSEntities();
        }

        public virtual IEnumerable<T> GetALLEntities()
        {
            using (DbContext Entities = GetDbContext())
            {
                //AsNoTracking不记录数据变化状况
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                return context.CreateObjectSet<T>().AsNoTracking().ToList();
            }

        }
        public virtual IEnumerable<T> GetEntities(Expression<Func<T, bool>> exp)
        {
            using (DbContext Entities = GetDbContext())
            {
                //AsNoTracking不记录数据变化状况
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                return context.CreateObjectSet<T>().AsNoTracking().Where(exp).ToList();
            }
            
        }


        public virtual IEnumerable<T> GetEntities(Expression<Func<T, bool>> exp, string ComId)
        {
            using (DbContext Entities = GetDbContext())
            {
                var param = Expression.Parameter(typeof(T), "x");
                var left = Expression.Property(param, "ComId");
                var right = Expression.Constant(ComId);
                var equal = Expression.Equal(left, right);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, param);
                //AsNoTracking不记录数据变化状况
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                return context.CreateObjectSet<T>().Where<T>(lambda).Where<T>(exp).ToList();
            }
        }



        /// <summary>
        /// 根据条件查找
        /// </summary>
        /// <param name="exp">lambda查询条件where</param>
        /// <returns></returns>
        public virtual T GetEntity(Expression<Func<T, bool>> exp)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                return context.CreateObjectSet<T>().Where(exp).SingleOrDefault();
            }
        }

        /// <summary>
        /// 获取所有Entity(立即执行请使用ToList()
        /// </summary>
        /// <param name="CommandText">Sql语句</param>
        /// <param name="objParams">可变参数</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetEntities(string CommandText)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                return context.ExecuteStoreQuery<T>("select * from " + typeof(T).Name + " where 1=1 and  " + CommandText).ToList();
            }
        }


        /// <summary>
        /// 插入Entity
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool Insert(T entity)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                var obj = context.CreateObjectSet<T>();
                obj.AddObject(entity);
                return Entities.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 同时插入多个实体。
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual bool Insert(IEnumerable<T> entities)
        {
            using (DbContext Entities = GetDbContext())
            {
                foreach (var entity in entities)
                {
                    ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;
                    var obj = context.CreateObjectSet<T>();
                    obj.AddObject(entity);
                }

                return Entities.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 更新Entity(注意这里使用的傻瓜式更新,可能性能略低)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool Update(T entity)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;

                var obj = context.CreateObjectSet<T>();
                obj.Attach(entity);
                context.ObjectStateManager.ChangeObjectState(entity, System.Data.Entity.EntityState.Modified);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 删除Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Delete(T entity)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;

                var obj = context.CreateObjectSet<T>();

                if (entity != null)
                {
                    obj.Attach(entity);
                    context.ObjectStateManager.ChangeObjectState(entity, System.Data.Entity.EntityState.Deleted);

                    obj.DeleteObject(entity);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
        /// <summary>
        /// 批量删除Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Delete(Func<T, bool> exp)
        {
            using (DbContext Entities = GetDbContext())
            {
                ObjectContext context = ((IObjectContextAdapter)Entities).ObjectContext;

                var q = context.CreateObjectSet<T>().Where(exp);
                foreach (var item in q)
                {
                    context.DeleteObject(item);
                }
                return context.SaveChanges() >= 0;
            }
        }



        /// <summary>
        /// 根据条件查找
        /// </summary>
        /// <param name="CommandText">Sql语句</param>
        /// <param name="objParams">可变参数</param>
        /// <returns></returns>
        public virtual DataTable GetDTByCommand(string CommandText)
        {

            using (DbContext Entities = GetDbContext())
            {
                string connectionString = Entities.Database.Connection.ConnectionString;
                return SqlQueryForDataTatable(connectionString, CommandText);
            }
        }



        /// <summary>
        /// 获取链接字符串
        /// </summary>
        /// <param name="CommandText">Sql语句</param>
        /// <param name="objParams">可变参数</param>
        /// <returns></returns>
        public string GetDBString()
        {

            using (DbContext Entities = GetDbContext())
            {
                string connectionString = Entities.Database.Connection.ConnectionString;
                return connectionString;
            }
        }
        /// <summary>
        /// EF SQL 语句返回 dataTable
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable SqlQueryForDataTatable(string strCon,
                 string sql)
        {


            SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            conn.ConnectionString = strCon;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            conn.Close();
            return table;
        }

        public DataSet SqlQueryForDataSet(string strCon, string sql)
        {
            SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            conn.ConnectionString = strCon;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            conn.Close();
            return ds;
        }

        //执行SQL语句
        public void ExsSql(string sql)
        {
            using (DbContext Entities = GetDbContext())
            {

                string connectionString = Entities.Database.Connection.ConnectionString;
                SqlConnection conn = new System.Data.SqlClient.SqlConnection();
                conn.ConnectionString = connectionString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public object ExsSclarSql(string sql)
        {
            using (DbContext Entities = GetDbContext())
            {

                string connectionString = Entities.Database.Connection.ConnectionString;
                SqlConnection conn = new System.Data.SqlClient.SqlConnection();
                conn.ConnectionString = connectionString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                var result = cmd.ExecuteScalar();
                conn.Close();

                return result;
            }
        }
        /// <summary>
        /// 数据分页
        /// </summary>
        /// <param name="viewName">表名</param>
        /// <param name="fieldName">字段</param>
        /// <param name="pageSize">默认20</param>
        /// <param name="pageNo">页数</param>
        /// <param name="orderString">排序</param>
        /// <param name="whereString">可选</param>
        /// <param name="recordTotal">总数</param>
        /// <returns></returns>
        public DataTable GetDataPager(string viewName, string fieldName, int pageSize, int pageNo, string orderString, string whereString, ref int recordTotal)
        {
            using (DbContext Entities = GetDbContext())
            {
                DataSet ds = new DataSet();
                string connectionString = Entities.Database.Connection.ConnectionString;

                SqlConnection conn = new System.Data.SqlClient.SqlConnection();
                conn.ConnectionString = connectionString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "usp_DataPager";
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter[] paras = new SqlParameter[7];
                    paras[0] = new SqlParameter("viewName", viewName);
                    paras[1] = new SqlParameter("fieldName", fieldName);
                    paras[2] = new SqlParameter("pageSize", pageSize);
                    paras[3] = new SqlParameter("pageNo", pageNo);
                    paras[4] = new SqlParameter("orderString", orderString);
                    if (whereString.Trim() == "")
                    {
                        whereString = " 1=1 ";
                    }
                    paras[5] = new SqlParameter("whereString", whereString);
                    paras[5].Size = Int32.MaxValue;
                    paras[6] = new SqlParameter("recordTotal", recordTotal);
                    paras[6].Direction = ParameterDirection.Output;
                    cmd.Parameters.AddRange(paras);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(ds);

                    recordTotal = Int32.Parse(paras[6].Value == null ? "0" : paras[6].Value.ToString());
                    return ds.Tables[0];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally { conn.Close(); }
            }

        }

    }
}