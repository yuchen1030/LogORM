using LogORM.DbSqlProvider;
using LogORM.AdoNet;
using LogORM.AdoNet.Oracle;
using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LogORM
{

    //logorm.MODELS独立出来，引用少
    //CurrentDalParas 中增加 public bool IsDesc { get; set; }
    //DBOperUser dbLogMsg  变为 DBOperUser dbOperUser
    //是否SKIP GetDataSet方法 类的日志；    SkipQueryTypeLog

    public abstract class LogORMBaseDal<T> : ILogORMDal<T> where T : class
    {
        public LogORMBaseDal()
        {
            tableName = CurDalParas.TableName;
            primaryKey = CurDalParas.PrimaryKey;
            skipCols = CurDalParas.SkipCols;
            updateKeys = CurDalParas.UpdateKeys;
            deleteKeys = CurDalParas.DeleteKeys;
            softDelFalg = CurDalParas.SoftDelFalg;
            orderby = CurDalParas.Orderby;
            conStr = ComDBFun.GetConnectionString(CurDalParas.DBConStringKey);
            GetBaseDBByDBType();
            CurSqlProvider = SqlProvider.CreateProvider(CurDalParas.CurDatabaseType);
        }

        protected abstract CurrentDalParas CurDalParas { get; }//抽象属性，要求子类必须实现
        public ISqlProvider CurSqlProvider = null;
        string tableName = "";
        string primaryKey = "";
        string[] skipCols = new string[] { "" };
        List<string> updateKeys = new List<string>() { "" };
        List<string> deleteKeys = new List<string>() { "" };
        Dictionary<string, object> softDelFalg = new Dictionary<string, object>() { };
        string orderby = "";//分页的排序字段，包括字段和顺序，例如 a1 asc, a2 desc
        string conStr = "";
        IAdoNetBase<T> baseDB = null;

        public struct CurrentDalParas
        {
            public DBStoreType CurDatabaseType { get; set; }
            public string TableName { get; set; }
            public string DBConStringKey { get; set; }
            public string PrimaryKey { get; set; }
            public string[] SkipCols { get; set; } //自增字段时
            public List<string> UpdateKeys { get; set; }
            public List<string> DeleteKeys { get; set; }
            public Dictionary<string, object> SoftDelFalg { get; set; }
            public string Orderby { get; set; }//分页的排序字段，包括字段和顺序，例如 a1 asc, a2 desc
            public IAdoNetBase<T> AdoNetBase { get; set; }
        }

        void GetBaseDBByDBType()
        {
            bool bOK = false;
            try
            {
                DBStoreType dataBaseType = CurDalParas.CurDatabaseType;
                switch (dataBaseType)
                {
                    case DBStoreType.SqlServer:
                        baseDB = new SqlServerHelper<T>(conStr);
                        break;

                    case DBStoreType.Oracle:
                        // baseDB = new OracleHelperFactory<T>(conStr).GetInstance();
                        baseDB = OracleHelperFactory<T>.GetInstance(conStr);
                        break;
                    case DBStoreType.MySql:
                        baseDB = new MySqlHelper<T>(conStr);
                        break;
                }
                bOK = true;
            }
            catch
            {
            }
            if (!bOK || baseDB == null)
            {
                string msg = "您配置" + typeof(T).Name + "的数据库类型为【" + CurDalParas.CurDatabaseType.ToString() + "】，但代码中尚未实现。";
                //  LogApi.WriteMsgToDebugFile(new { 内容 = msg });
                throw new Exception(msg);
            }

        }


        //添加一个实体
        public ExeResEdm Add(AddDBPara<T> dBPara, DBOperUser dbLogMsg = null)
        {
            return baseDB.Add(tableName, dBPara.Model, dbLogMsg, skipCols);
        }

        //批量添加实体
        public ExeResEdm Add(List<T> list, DBOperUser dbLogMsg = null)
        {
            return baseDB.Add(tableName, list, dbLogMsg);
        }

        //根据字段更新
        public ExeResEdm Update(Dictionary<string, object> whereParas, Dictionary<string, object> updateFDList, DBOperUser dbLogMsg = null)
        {
            return baseDB.Update(tableName, whereParas, updateFDList);
        }

        //根据字段更新实体
        public ExeResEdm Update(T model, List<string> whereParas, DBOperUser dbLogMsg = null)
        {
            return baseDB.Update(tableName, model, updateKeys, dbLogMsg, skipCols);
        }

        //批量更新
        public ExeResEdm Update(List<T> list, List<Dictionary<string, string>> updateFDList, DBOperUser dbLogMsg = null, string strComFields = "*")
        {
            return baseDB.Update(tableName, list, updateFDList, dbLogMsg, strComFields);
        }

        //批量进行添加/更新/删除
        public ExeResEdm AddUpdateDelete(DBOperUser dbLogMsg = null, params AddUpdateDelEdm[] models)
        {
            if (models != null && models.Length > 0)
            {
                models = models.Select(a => { a.TableName = !string.IsNullOrEmpty(a.TableName) ? a.TableName : tableName; return a; }).ToArray();
            }
            return baseDB.AddUpdateDelete(dbLogMsg, models);
        }

        //根据id删除
        public ExeResEdm Delete(object id, DBOperUser dbLogMsg = null)
        {
            if (deleteKeys != null && deleteKeys.Count == 1)
            {
                Dictionary<string, object> whereParas = new Dictionary<string, object>() { { deleteKeys[0], id } };
                return Delete(whereParas, dbLogMsg);
            }
            else
            {
                return new ExeResEdm() { ErrCode = 1, ErrMsg = "必须是唯一主键才能根据ID删除", Module = tableName };
            }
        }

        //删除某个实体
        public ExeResEdm Delete(T model, DBOperUser dbLogMsg = null)
        {
            return baseDB.Delete(tableName, model, deleteKeys, dbLogMsg);
        }

        //根据字段删除
        public ExeResEdm Delete(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null)
        {
            return baseDB.Delete(tableName, whereParas, dbLogMsg);
        }

        //根据id软删除
        public ExeResEdm SoftDelete(object id, DBOperUser dbLogMsg = null)
        {
            if (deleteKeys != null || deleteKeys.Count == 1)
            {
                Dictionary<string, object> whereParas = new Dictionary<string, object>() { { deleteKeys[0], id } };
                return SoftDelete(whereParas, dbLogMsg);
            }
            else
            {
                return new ExeResEdm() { ErrCode = 1, ErrMsg = "必须是唯一主键才能根据ID删除", Module = tableName };
            }
        }

        //软删除某个实体
        public ExeResEdm SoftDelete(T model, DBOperUser dbLogMsg = null)
        {
            return baseDB.SoftDelete(tableName, model, deleteKeys, softDelFalg, dbLogMsg);
        }

        //根据条件软删除
        public ExeResEdm SoftDelete(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null)
        {
            return baseDB.SoftDelete(tableName, whereParas, softDelFalg, dbLogMsg);
        }

        //执行Sql语句
        public ExeResEdm ExecuteNonQuery(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            return baseDB.ExecuteNonQuery(cmdText, dbLogMsg, parameters);
        }

        //执行ExecuteScalar语句
        public ExeResEdm ExecuteScalar(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            return baseDB.ExecuteScalar(cmdText, dbLogMsg, parameters);
        }

        //执行存储过程
        public ExeResEdm ExecuteStoredProcedure(string storedProcedureName, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            return baseDB.ExecuteStoredProcedure(storedProcedureName, dbLogMsg, parameters);
        }

        //执行事务
        public ExeResEdm ExecuteTransaction(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null)
        {
            return baseDB.ExecuteTransaction(ltSqls, dbLogMsg);
        }

        //获取分页数据
        public ExeResEdm GetAll(PageSerach<T> para, DBOperUser dbLogMsg = null)
        {
            var data = baseDB.GetListByPage(tableName, para, dbLogMsg);
            return data;
        }

        //获取DataSet数据
        public ExeResEdm GetDataSet(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null)
        {
            ltSqls = ltSqls ?? new List<SqlContianer>();
            ltSqls = ltSqls.Select(a => { var tb = ComDBFun.GetTableNameFromSelectSql(a.strSqlTxt); if (string.IsNullOrEmpty(tb)) a.strSqlTxt = "select * from " + tableName + " where " + a.strSqlTxt; return a; }).ToList();
            return baseDB.GetDataSet(ltSqls, dbLogMsg);
        }

        //获取DataSet数据
        public ExeResEdm GetDataSet(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            string sql = "";
            cmdText = cmdText ?? "";
            if (cmdText.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                sql = cmdText;
            }
            else
            {
                sql += " where " + cmdText;
            }
            return baseDB.GetDataSet(cmdText, dbLogMsg, parameters);
        }

        //获取一个数据表的表结构
        public ExeResEdm SelectDBTableFormat(DBOperUser dbLogMsg = null, string strField = "*")
        {
            return baseDB.SelectDBTableFormat(tableName, dbLogMsg, strField);
        }

        //获取SQL语句
        public CRUDSql GetSelectSql(T searchPara, List<string> selectFields = null)
        {
            return baseDB.GetSelectSql(searchPara, tableName, orderby, selectFields);
        }

        //获取插入的SQL语句
        public CRUDSql GetInsertSql<M>(M model, string tableName, bool bParameterizedQuery)
        {
            return baseDB.GetInsertSql(model, tableName, bParameterizedQuery);
        }

        //检查指定条件的数据是否存在
        public ExeResEdm Exist(Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null)
        {
            return baseDB.Exist(tableName, whereParas, dbLogMsg);
        }

        //检查某个实体是否存在
        public ExeResEdm Exist(T model, DBOperUser dbLogMsg = null)
        {
            return baseDB.Exist(tableName, model, updateKeys, dbLogMsg);
        }



    }



}
