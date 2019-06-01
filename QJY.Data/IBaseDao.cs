using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;


namespace QJY.Data
{
    public interface IBaseDao<T> where T : class //限制class
    {
        #region 查询普通实现方案(基于Lambda表达式的Where查询)

        /// <summary>
        /// 获取所有Entity
        /// </summary>
        /// <param name="exp">Lambda条件的where</param>
        /// <returns></returns>
        IEnumerable<T> GetEntities(Expression<Func<T, bool>> exp);

        IEnumerable<T> GetEntities(Expression<Func<T, bool>> exp,string strComId);

        /// <summary>
        /// 根据条件查找
        /// </summary>
        /// <param name="exp">lambda查询条件where</param>
        /// <returns></returns>
        T GetEntity(Expression<Func<T, bool>> exp);

        #endregion 查询普通实现方案(基于Lambda表达式的Where查询)

        #region 查询Sql语句外接接口的查询实现

        /// <summary>
        /// 获取所有Entity(立即执行请使用ToList()
        /// </summary>
        /// <param name="CommandText">Sql语句</param>
        /// <param name="objParams">可变参数</param>
        /// <returns></returns>
        IEnumerable<T> GetEntities(string CommandText);



   
        #endregion 查询Sql语句外接接口的查询实现

        /// <summary>
        /// 插入Entity
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Insert(T entity);

        /// <summary>
        /// 更新Entity
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Update(T entity);

        /// <summary>
        /// 删除Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Delete(T entity);
    }
}