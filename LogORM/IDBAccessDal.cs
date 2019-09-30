using Log2Net.Models;
using LogORM.Models;
using System.Collections.Generic;
using System.Data.Common;

namespace LogORM
{

    public interface ILogORMDal<T> where T : class
    {

        //添加一个实体
        ExeResEdm Add(AddDBPara<T> dBPara, DBOperUser dbLogMsg = null);

        //批量添加实体
        ExeResEdm Add(List<T> list, DBOperUser dbLogMsg = null);

        //根据字段更新
        ExeResEdm Update(Dictionary<string, object> whereParas, Dictionary<string, object> updateFDList, DBOperUser dbLogMsg = null);

        //根据字段更新实体
        ExeResEdm Update(T model, List<string> whereParas, DBOperUser dbLogMsg = null);

        //批量更新
        ExeResEdm Update(List<T> list, List<Dictionary<string, string>> updateFDList, DBOperUser dbLogMsg = null, string strComFields = "*");

        //批量进行添加/更新/删除
        ExeResEdm AddUpdateDelete( DBOperUser dbLogMsg = null, params AddUpdateDelEdm[] models);

        //根据id软删除
        ExeResEdm Delete(object id, DBOperUser dbLogMsg = null);

        //删除某个实体
        ExeResEdm Delete(T model, DBOperUser dbLogMsg = null);

        //根据字段删除
        ExeResEdm Delete(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null);


        //根据id软删除
        ExeResEdm SoftDelete(object id, DBOperUser dbLogMsg = null);

        //软删除某个实体
        ExeResEdm SoftDelete(T model, DBOperUser dbLogMsg = null);

        //根据条件软删除
        ExeResEdm SoftDelete(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null);

        //执行Sql语句
        ExeResEdm ExecuteNonQuery(string cmdText,DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //执行ExecuteScalar语句
        ExeResEdm ExecuteScalar(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //执行存储过程
        ExeResEdm ExecuteStoredProcedure(string storedProcedureName, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //执行事务
        ExeResEdm ExecuteTransaction(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null);

        //获取分页数据
        ExeResEdm GetAll(PageSerach<T> para, DBOperUser dbLogMsg = null);

        //获取DataSet数据
        ExeResEdm GetDataSet(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null);

        //获取DataSet数据
        ExeResEdm GetDataSet(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //获取一个数据表的表结构
        ExeResEdm SelectDBTableFormat( DBOperUser dbLogMsg = null ,string strField = "*");

        //获取查询的SQL语句
        CRUDSql GetSelectSql(T searchPara,  List<string> selectFields = null);

        //获取插入的SQL语句
        CRUDSql GetInsertSql<M>(M model, string tableName, bool bParameterizedQuery);

        //检查指定条件的数据是否存在
        ExeResEdm Exist(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null);

        //检查某个实体是否存在
        ExeResEdm Exist(T model, DBOperUser dbLogMsg = null);







    }
}
