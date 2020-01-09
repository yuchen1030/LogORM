using Log2Net.Models;
using LogORM.ComUtil;
using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.AdoNet
{

    public interface IAdoNetBase<T> where T : class
    {

        //添加一个实体
        ExeResEdm Add(string tableName, T model, DBOperUser dbLogMsg = null, params string[] skipCols);

        //批量添加实体
        ExeResEdm Add(string tableName, List<T> list, DBOperUser dbLogMsg = null, string strComFields = "*");

        //根据字段更新
        ExeResEdm Update(string tableName, Dictionary<string, object> whereParas, Dictionary<string, object> updateFDList, DBOperUser dbLogMsg = null);

        //根据字段更新实体
        ExeResEdm Update(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null, params string[] skipCols);

        //批量更新
        ExeResEdm Update(string tableName, List<T> list, List<Dictionary<string, string>> updateFDList, DBOperUser dbLogMsg = null, string strComFields = "*");

        //批量进行添加/更新/删除
        ExeResEdm AddUpdateDelete(DBOperUser dbLogMsg = null, params AddUpdateDelEdm[] models);

        //删除某个实体
        ExeResEdm Delete(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null);

        //根据字段删除
        ExeResEdm Delete(string tableName, Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null);

        //软删除某个实体
        ExeResEdm SoftDelete(string tableName, T model, List<string> whereParas, Dictionary<string, object> softDelFalg, DBOperUser dbLogMsg = null);

        //根据条件软删除
        ExeResEdm SoftDelete(string tableName, Dictionary<string, object> whereParas, Dictionary<string, object> softDelFalg, DBOperUser dbLogMsg = null);

        //执行Sql语句
        ExeResEdm ExecuteNonQuery(string cmdText, DBOperUser DBOperUser, params DbParameter[] parameters);

        //执行ExecuteScalar语句
        ExeResEdm ExecuteScalar(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //执行存储过程
        ExeResEdm ExecuteStoredProcedure(string storedProcedureName, bool bOutputDT = true, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //执行事务
        ExeResEdm ExecuteTransaction(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null);

        //获取分页数据
        ExeResEdm GetListByPage(string tableName, PageSerach<T> para, DBOperUser dbLogMsg = null);

        //获取DataSet数据
        ExeResEdm GetDataSet(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null);

        //获取DataSet数据
        ExeResEdm GetDataSet(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters);

        //获取一个数据表的表结构
        ExeResEdm SelectDBTableFormat(string tableName, DBOperUser dbLogMsg = null, string strField = "*");

        //获取查询的SQL语句
        CRUDSql GetSelectSql(T searchPara, string tableName, string orderBy, List<string> selectFields = null);

        //获取插入的SQL语句
        CRUDSql GetInsertSql<M>(M model, string tableName, bool bParameterizedQuery);

        //检查指定条件的数据是否存在
        ExeResEdm Exist(string tableName, Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null);

        //检查某个实体是否存在
        ExeResEdm Exist(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null);

    }



    internal abstract class AdoNetBase<T> : IAdoNetBase<T> where T : class
    {
        #region 属性字段   
        protected abstract DBBaseAttr DBBaseAttr { get; }
        protected string connstr { get; set; }
        #endregion 属性字段

        //构造函数   
        internal AdoNetBase(string strConnStr)
        {
            connstr = strConnStr;
        }

        #region 接口的实现 

        public ExeResEdm Add(string tableName, T model, DBOperUser dbLogMsg = null, params string[] skipCols)
        {
            Dictionary<string, object> dic = DtModelConvert<T>.GetPropertity(model);
            object[] values = dic.Values.ToArray();
            // string idVal = dic.Values.ToArray()[0].ToString();
            //SqlParameter[] pms = GetOleDbParameters(dic.Keys.ToList(), dic.Values.ToList());//参数过多，不会影响程序执行的正确性。
            for (int i = 0; i < skipCols.Length; i++)//自动增长的列要忽略
            {
                dic.Remove(skipCols[i]);
            }

            for (int i = dic.Values.Count - 1; i >= 0; i--)//值为空的不参与
            {
                if (dic.Values.ToList()[i] == null)
                {
                    dic.Remove(dic.Keys.ToList()[i]);
                }
            }
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);
            string textParas = ComDBFun.GetSQLText(dic.Keys.ToList(), null);
            string sql = "insert into " + tableName + textParas;

            DbParameter[] pms = GetDbParametersFromDic(dic);

            LogTraceEdm logMsg = null;
            if (dbLogMsg != null)
            {
                logMsg = new LogTraceEdm() { LogType = LogType.添加, UserId = dbLogMsg.UserId, UserName = dbLogMsg.UserName, TabOrModu = tableName, };
            }

            var n = ExecuteNonQuery(sql, logMsg, pms);
            return n;

        }

        public ExeResEdm Add(string tableName, List<T> list, DBOperUser dbLogMsg = null, string strComFields = "*")
        {
            var dt = GetDataTable(tableName, null, list, null);
            var res = UpdateDtToDB(dt, strComFields);
            WriteLogMsg(dbLogMsg, LogType.批量插入, "参数为：" + DtModelConvert<List<T>>.SerializeToString(list) + "，受影响的行数为" + res.ExeNum, tableName);
            return res;
        }

        public ExeResEdm Update(string tableName, Dictionary<string, object> whereParas, Dictionary<string, object> updateFDList, DBOperUser dbLogMsg = null)
        {
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);
            updateFDList = updateFDList ?? new Dictionary<string, object>();
            var paras = whereParas.Union(updateFDList).ToDictionary(k => k.Key, v => v.Value);
            DbParameter[] pms = GetDbParametersFromDic(paras);
            string textParas = ComDBFun.GetUpdateSQLText(updateFDList.Keys.ToList());
            string whereSql = ComDBFun.GetWhereCondition(whereParas.Keys.ToList(), "and");
            textParas += whereSql;   // " where " + dic.Keys.ToArray()[skipIndex] + "=@" + dic.Keys.ToArray()[skipIndex];
            string sql = "update " + tableName + " set " + textParas;
            LogTraceEdm logMsg = null;
            if (dbLogMsg != null)
            {
                logMsg = new LogTraceEdm() { LogType = LogType.修改, UserId = dbLogMsg.UserId, UserName = dbLogMsg.UserName, TabOrModu = tableName, };
            }
            var n = ExecuteNonQuery(sql, logMsg, pms);
            return n;
        }

        public ExeResEdm Update(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null, params string[] skipCols)
        {
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);

            Dictionary<string, object> dic = DtModelConvert<T>.GetPropertity(model);
            // object[] values = dic.Values.ToArray();
            string idVal = dic.Values.ToArray()[0].ToString();
            for (int i = 0; i < skipCols.Length; i++)//自动增长的列要忽略
            {
                dic.Remove(skipCols[i]);
            }

            for (int i = 0; i < whereParas.Count; i++)
            {
                try
                {
                    var curKey = dic.Where(a => a.Key.Equals(whereParas[i], StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Key;
                    dic.Remove(curKey);
                }
                catch
                {

                }
            }

            for (int i = dic.Values.Count - 1; i >= 0; i--)//比较值为空的不参与比较
            {
                if (dic.Values.ToList()[i] == null)
                {
                    dic.Remove(dic.Keys.ToList()[i]);
                }
            }
            return Update(tableName, dic, null, dbLogMsg);


        }

        public ExeResEdm Update(string tableName, List<T> list, List<Dictionary<string, string>> updateFDList, DBOperUser dbLogMsg = null, string strComFields = "*")
        {
            var dt = GetDataTableComplex(tableName, null, list, updateFDList);
            var res = UpdateDtToDB(dt, strComFields);
            WriteLogMsg(dbLogMsg, LogType.批量修改, "参数为：" + DtModelConvert<object>.SerializeToString(new { Data = list, UpdateFDs = updateFDList }) + "，受影响的行数为" + res.ExeNum, tableName);
            return res;
        }

        public ExeResEdm AddUpdateDelete(DBOperUser dbLogMsg = null, params AddUpdateDelEdm[] datas)
        {
            DataSet ds = new DataSet();
            if (datas == null || datas.Length <= 0)
            {
                return new ExeResEdm() { ErrCode = 1, Module = "AddUpdateDelete方法", ErrMsg = "没有有效的参数", };
            }
            datas = datas.Where(a => a.Datas != null && a.Datas.Count > 0).ToArray();
            if (datas.Length <= 0)
            {
                return new ExeResEdm() { ErrCode = 1, Module = "AddUpdateDelete方法", ErrMsg = "没有有效的参数", };
            }
            var models = datas.ToList();
            //若TableName相同，则合并
            models = models.Select(a => { a.TableName = a.TableName.Trim().ToLower().Trim(); return a; }).ToList();
            var groups = models.GroupBy(a => a.TableName).ToList();
            List<AddUpdateDelEdm> realModels = new List<AddUpdateDelEdm>();
            Dictionary<string, string> dtPKs = new Dictionary<string, string>();
            models.Clear();
            foreach (var item in groups)
            {
                AddUpdateDelEdm cur = new AddUpdateDelEdm() { TableName = item.Key, };
                cur.Datas = new List<object>();
                cur.UpdateFD = new List<Dictionary<string, string>>();
                var itemList = item.ToList();
                bool[] bAdd = new bool[itemList.Count];
                for (int i = 0; i < itemList.Count; i++)
                {
                    var m = itemList[i];
                    cur.MainFields = !string.IsNullOrEmpty(m.MainFields) ? m.MainFields : cur.MainFields;
                    if (m.UpdateFD != null && m.UpdateFD.Count == m.Datas.Count)
                    {
                        cur.Datas.AddRange(m.Datas);
                        cur.UpdateFD.AddRange(m.UpdateFD);
                        bAdd[i] = false;
                    }
                    else
                    {
                        foreach (var d in m.Datas)
                        {
                            cur.Datas.Add(d);
                            if (m.UpdateFD == null || m.UpdateFD.Count <= 0)
                            {
                                //  cur.UpdateFD.Add(new Dictionary<string, string>() { });
                                bAdd[i] = true;
                            }
                            else if (m.UpdateFD.Count == 1)
                            {
                                cur.UpdateFD.Add(m.UpdateFD[0]);
                                bAdd[i] = false;
                            }
                        }
                    }
                    dtPKs.Add(cur.TableName, cur.MainFields);
                }
                if (bAdd.Distinct().Count() != 1)
                {
                    ExeResEdm exeResEdm = new ExeResEdm()
                    {
                        ErrCode = 1,
                        ExBody = new Exception("同一张表[" + item.Key + "]，不能同时有添加和更新操作"),
                        Module = "AddUpdate 方法",
                    };
                    return exeResEdm;
                }
                models.Add(cur);
            }

            //获取所有的datatable模板         
            List<SqlContianer> ltSqls = models.Select(a => new SqlContianer() { tableName = a.TableName, strSqlTxt = GetColumnsNameSql(a.TableName) }).ToList();
            var dsTemplate = GetDataSets(ltSqls).ExeModel as DataSet;
            if (dsTemplate == null || dsTemplate.Tables.Count <= 0)
            {
                return new ExeResEdm() { ErrCode = 1, ErrMsg = "未获取到数据", Module = "AddUpdateDelete方法" };
            }
            foreach (var item in models)
            {
                DataTable dt = new DataTable();
                DataTable dtTemplate = dsTemplate.Tables[item.TableName];
                if (item.UpdateFD == null || item.UpdateFD.Count == 0)
                {
                    dt = GetDataTable(item.TableName, dtTemplate, item.Datas);
                }
                else if (item.UpdateFD.Count == 1)
                {
                    dt = GetDataTable(item.TableName, dtTemplate, item.Datas, item.UpdateFD[0]);
                }
                else
                {
                    dt = GetDataTableComplex(item.TableName, dtTemplate, item.Datas, item.UpdateFD);
                }
                ds.Tables.Add(dt);
            }

            var res = UpdateDsToDB(ds, dtPKs);
            WriteLogMsg(dbLogMsg, LogType.批量增删改, "参数为：" + DtModelConvert<List<AddUpdateDelEdm>>.SerializeToString(models) + "，受影响的行数为" + res.ExeNum, "AddUpdateDelete方法");
            return res;
        }

        public ExeResEdm Delete(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null)
        {
            whereParas = whereParas.Where(a => !string.IsNullOrEmpty(a)).Select(a => a.ToLower()).Distinct().ToList();
            Dictionary<string, object> whereDic = DtModelConvert<T>.GetPropertity(model);
            whereDic = whereDic.Where(a => whereParas.Contains(a.Key.ToLower())).ToDictionary(k => k.Key, v => v.Value);
            return Delete(tableName, whereDic, dbLogMsg);
        }

        public ExeResEdm Delete(string tableName, Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null)
        {
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);
            string whereSql = ComDBFun.GetWhereCondition(whereParas.Keys.ToList(), "and", whereParas);
            string sql = "delete " + tableName + whereSql;
            DbParameter[] pms = GetDbParametersFromDic(whereParas);
            LogTraceEdm logMsg = null;
            if (dbLogMsg != null)
            {
                logMsg = new LogTraceEdm() { LogType = LogType.硬删除, UserId = dbLogMsg.UserId, UserName = dbLogMsg.UserName, TabOrModu = tableName, };
            }
            var n = ExecuteNonQuery(sql, logMsg, pms);
            return n;
        }

        public ExeResEdm SoftDelete(string tableName, T model, List<string> whereParas, Dictionary<string, object> softDelFalg, DBOperUser dbLogMsg = null)
        {
            whereParas = whereParas.Where(a => !string.IsNullOrEmpty(a)).Select(a => a.ToLower()).Distinct().ToList();
            Dictionary<string, object> whereDic = DtModelConvert<T>.GetPropertity(model);
            whereDic = whereDic.Where(a => whereParas.Contains(a.Key.ToLower())).ToDictionary(k => k.Key, v => v.Value);
            return Update(tableName, whereDic, softDelFalg);
        }

        public ExeResEdm SoftDelete(string tableName, Dictionary<string, object> whereParas, Dictionary<string, object> softDelFalg, DBOperUser dbLogMsg = null)
        {
            if (softDelFalg == null || softDelFalg.Count <= 0)
            {
                return new ExeResEdm() { ErrCode = 1, Module = "SoftDelete方法", ExBody = new Exception("未定义" + tableName + "表数据软删除的SoftDelFalg参数") };
            }
            return Update(tableName, whereParas, softDelFalg);
        }

        public ExeResEdm ExecuteNonQuery(string cmdText, DBOperUser dbOperUser = null, params DbParameter[] parameters)
        {
            LogTraceEdm logMsg = null;
            if (dbOperUser != null)
            {
                logMsg = new LogTraceEdm() { LogType = LogType.ExecuteNonQuery, UserId = dbOperUser.UserId, UserName = dbOperUser.UserName, TabOrModu = "ExecuteNonQuery方法", };
            }
            var n = ExecuteNonQuery(cmdText, logMsg, parameters);
            return n;
        }

        public ExeResEdm ExecuteScalar(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = SqlCMD(cmdText, cmd => cmd.ExecuteScalar(), parameters);
            dBResEdm.ExeNum = 1;
            WriteLogMsg(dbLogMsg, LogType.ExecuteScalar, "根据" + GetRealSql(cmdText, parameters) + "执行，结果为" + dBResEdm.ExeModel, "ExecuteScalar方法");
            return dBResEdm;
        }

        public ExeResEdm ExecuteStoredProcedure(string storedProcedureName, bool bOutputDT = true, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            #region 存储过程例子，建议将输出参数以select形式输出
            //CREATE PROCEDURE[dbo].[getInsertLog] @userid nvarchar(100), 	@bok INT OUTPUT
            //AS
            //BEGIN
            //SET NOCOUNT ON;
            //INSERT INTO Log_OperateTrace
            //        ([LogTime], [UserID], [UserName] , [LogType] , [SystemID]  , [ServerHost]
            //        , [ServerIP], [ClientHost], [ClientIP] , [TabOrModu], [Detail], [Remark])
            //VALUES
            //    (CONVERT(varchar, GETDATE(),120), @userid, @userid  ,3  ,10 ,'ServerHost'
            //    ,'ServerIP'   ,'ClientHost'   ,'ClientIP' ,'TabOrModu','<Detail' ,'<Remark')
            //set @bok = 1;
            //SELECT @bok;
            //SELECT[ID] FROM Log_OperateTrace WHERE[ID] = SCOPE_IDENTITY();
            //SELECT * FROM Log_OperateTrace
            //END
            #endregion 存储过程例子，建议将输出参数以select形式输出
            DataSet ds = new DataSet();
            parameters = ParameterPrepare(parameters);
            ExeResEdm res = new ExeResEdm();
            if (bOutputDT)  //update insert 操作的存储过程，也可以使用 SqlCMD_DT 方法
            {
                res = SqlCMD_DT(storedProcedureName, CommandType.StoredProcedure, adt => adt.Fill(ds), parameters);
                res.ExeModel = ds;
            }
            else
            {
                res = SqlCMD(storedProcedureName, CommandType.StoredProcedure, cmd => cmd.ExecuteNonQuery(), parameters);
            }
            List<string> resultList = new List<string>();
            if (ds != null && ds.Tables.Count > 0)
            {
                int rowCnt = 0;
                foreach (DataTable item in ds.Tables)
                {
                    if (item != null)
                    {
                        rowCnt += item.Rows.Count;
                    }
                }
                resultList.Add("得到" + ds.Tables.Count + "张表" + rowCnt + "条数据");
            }
            else
            {
                resultList.Add("影响了" + res.ExeModel + "条记录");
            }
            if (parameters != null && parameters.Length > 0)
            {
                var outParas = parameters.Where(a => a.Direction != ParameterDirection.Input).ToList();
                if (outParas.Count > 0)
                {
                    resultList.Add("返回值为[" + string.Join(" and  ", outParas.Select(a => a.ParameterName + " = " + a.Value)) + "]");
                }
            }
            try
            {
                string detalMsg = ("执行存储过程" + storedProcedureName + "，" + string.Join("，", resultList)).Trim('，');
                WriteLogMsg(dbLogMsg, LogType.存储过程, detalMsg, "ExecuteStoredProcedure方法");
            }
            catch
            {

            }
            return res;
        }

        public ExeResEdm ExecuteTransaction(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null)
        {
            var res = ExecuteNonQueryFromSqlContianer(ltSqls);
            WriteLogMsg(dbLogMsg, LogType.事务, "根据" + GetRealSql(ltSqls) + "执行事务，受影响行数为" + res.ExeNum, "ExecuteTransaction方法");
            return res;

        }

        public ExeResEdm GetListByPage(string tableName, PageSerach<T> para, DBOperUser dbLogMsg = null)
        {
            var orderByStr = LambdaToSqlHelper<T>.GetSqlFromLambda(para.OrderBy).OrderbySql;
            string whereSql = !string.IsNullOrEmpty(para.StrWhere) ? para.StrWhere : LambdaToSqlHelper<T>.GetWhereFromLambda(para.Filter, DBStoreType.NoSelect);
            SearchParam searchParam = new SearchParam() { Orderby = orderByStr, PageIndex = para.PageIndex, PageSize = para.PageSize, TableName = tableName, StrWhere = whereSql, };
            ExeResEdm res = GetDTByPage(searchParam);
            int curNum = 0;
            if (res.ErrCode == 0)
            {
                List<T> list = DtModelConvert<T>.DatatableToList((res.ExeModel as DataTable));
                res.ExeModel = list.AsQueryable();
                curNum = list.Count();
                res.ExeNum = searchParam.TotalCount;
            }
            WriteLogMsg(dbLogMsg, LogType.查询, "根据[" + DtModelConvert<T>.SerializeToString(searchParam) + "]获取了分页数据，返回了"
                + curNum + "/" + searchParam.TotalCount + "条记录", tableName);
            return res;

        }

        public ExeResEdm GetDataSet(List<SqlContianer> ltSqls, DBOperUser dbLogMsg = null)
        {
            ExeResEdm exeRes = new ExeResEdm();
            int n = 0;
            try
            {
                exeRes = GetDataSets(ltSqls);
                n = (exeRes.ExeModel as DataSet).Tables.Count;
            }
            catch (Exception ex)
            {
                exeRes.ExBody = ex;
                exeRes.ErrCode = 1;
                exeRes.Module = "GetDataSet方法";
            }
            WriteLogMsg(dbLogMsg, LogType.查询, "根据" + GetRealSql(ltSqls) + "获取了" + n + "张表", "GetDataSet方法");
            return exeRes;
        }

        public ExeResEdm GetDataSet(string cmdText, DBOperUser dbLogMsg = null, params DbParameter[] parameters)
        {
            DataSet ds = new DataSet();
            parameters = ParameterPrepare(parameters);
            var res = SqlCMD_DT(cmdText, CommandType.Text, adt => adt.Fill(ds), parameters);
            res.ExeModel = ds.Copy();
            res.ExeNum = ds.Tables.Count;
            WriteLogMsg(dbLogMsg, LogType.查询, "根据" + GetRealSql(cmdText, parameters) + "获取了" + ds.Tables.Count + "张表", "GetDataSet方法");
            return res;
        }

        public ExeResEdm SelectDBTableFormat(string tableName, DBOperUser dbLogMsg = null, string strField = "*")
        {
            string strSqlTxt = GetColumnsNameSql(tableName, strField);
            ExeResEdm dtFb = GetDataTable(strSqlTxt);
            WriteLogMsg(dbLogMsg, LogType.查询, "获取了表结构", tableName);
            return dtFb;
        }


        public CRUDSql GetSelectSql(T searchPara, string tableName, string orderBy, List<string> selectFields = null)
        {
            //  ComDBFun ComDBFun = new ComDBFun(bOrcl);
            Dictionary<string, object> dic = DtModelConvert<T>.GetPropertity(searchPara);
            List<string> whereParas = dic.Keys.ToList();
            object[] values = dic.Values.ToArray();
            for (int i = dic.Values.Count - 1; i >= 0; i--)//比较值为空的不参与比较
            {
                if (dic.Values.ToList()[i] == null || string.IsNullOrEmpty(dic.Values.ToList()[i].ToString()))
                {
                    whereParas.Remove(dic.Keys.ToList()[i]);
                    dic.Remove(dic.Keys.ToList()[i]);
                }
            }
            string whereSql = new ComDBFun(DBBaseAttr).GetWhereCondition(whereParas, "and");
            string fds = (selectFields == null || selectFields.Count <= 0) ? "*" : string.Join(",", selectFields);
            orderBy = string.IsNullOrEmpty(orderBy) ? "" : "order by " + orderBy;
            string sql = string.Format("select {0} from {1} {2} {3}", fds, tableName, whereSql, orderBy);
            CRUDSql res = new CRUDSql() { Sql = sql };
            res.PMS = GetDbParametersFromDic(dic);
            return res;
        }


        public CRUDSql GetInsertSql<M>(M model, string tableName, bool bParameterizedQuery)
        {
            Dictionary<string, object> dic = DtModelConvert<T>.GetPropertity(model);
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);

            string textParas = ComDBFun.GetSQLText(dic.Keys.ToList(), (bParameterizedQuery ? null : dic.Values.ToList()));
            string sql = "insert into " + tableName + textParas;
            CRUDSql insertSql = new CRUDSql() { Sql = sql };
            if (bParameterizedQuery)
            {
                insertSql.PMS = GetDbParametersFromDic(dic);
            }
            return insertSql;
        }


        public ExeResEdm Exist(string tableName, Dictionary<string, object> whereParas, DBOperUser dbLogMsg = null)
        {
            ComDBFun ComDBFun = new ComDBFun(DBBaseAttr);
            string whereSql = ComDBFun.GetWhereCondition(whereParas.Keys.ToList(), "and");
            DbParameter[] pms = GetDbParametersFromDic(whereParas);
            string sql = "select count(0) from " + tableName + whereSql;
            var res = ExecuteScalar(sql, dbLogMsg, pms);
            if (res.ErrCode == 0)
            {
                try
                {
                    int n = (int)res.ExeModel;
                    res.ExeNum = n;
                    res.ExeModel = n > 0;
                    return res;
                }
                catch (Exception ex)
                {
                    res.ErrCode = 1; res.ExBody = ex;
                    res.Module = "Exist方法";
                }
            }
            return res;

        }

        public ExeResEdm Exist(string tableName, T model, List<string> whereParas, DBOperUser dbLogMsg = null)
        {
            whereParas = whereParas.Where(a => !string.IsNullOrEmpty(a)).Select(a => a.ToLower()).Distinct().ToList();
            Dictionary<string, object> whereDic = DtModelConvert<T>.GetPropertity(model);
            whereDic = whereDic.Where(a => whereParas.Contains(a.Key.ToLower())).ToDictionary(k => k.Key, v => v.Value);
            return Exist(tableName, whereDic, dbLogMsg);
        }


        #endregion 接口的实现 


        #region 可继承的方法，子类方法 

        protected ExeResEdm ExecuteNonQuery(string cmdText, LogTraceEdm logMsg, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = SqlCMD(cmdText, cmd => cmd.ExecuteNonQuery(), parameters);
            if (dBResEdm.ErrCode == 0)
            {
                dBResEdm.ExeNum = Convert.ToInt32(dBResEdm.ExeModel);
            }
            WriteLogMsg(logMsg, dBResEdm.ExeNum, GetRealSql(cmdText, parameters));
            return dBResEdm;
        }

        protected ExeResEdm SqlCMD(string sql, Func<DbCommand, object> fun, params DbParameter[] pms)
        {
            return SqlCMD(sql, CommandType.Text, fun, pms);
        }
        //SQL Server 和 oracle 可以使用此方法，MySQL不行
        protected virtual ExeResEdm GetDataByPage(string tableName, string strWhere, string orderby, int pageIndex, int pageSize, out int totalCnt)
        {
            totalCnt = 0;
            StringBuilder strSql = new StringBuilder();
            string columns = "";//为空，则获取全部列
            if (string.IsNullOrEmpty(orderby) || string.IsNullOrEmpty(orderby.Trim()))
            {
                return new ExeResEdm() { ErrCode = 1, ErrMsg = "orderby 参数不能为空", Module = "GetDataByPage" };
            }

            if (string.IsNullOrEmpty(columns))
            {
                columns = "*";
            }
            int startIndex = (pageIndex - 1) * pageSize + 1;
            int endIndex = pageSize * pageIndex;

            strSql.Append("SELECT * FROM ( ");
            strSql.Append(" SELECT ROW_NUMBER() OVER (");

            strSql.Append("order by T." + orderby);
            strSql.Append(")AS RowIx, T.* ,COUNT(*) OVER() AS dbtotal from " + tableName + " T ");
            if (!string.IsNullOrEmpty(strWhere) && !string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(" ) TT");

            strSql.AppendFormat(" WHERE TT.RowIx between " + DBBaseAttr.ParaPreChar + "startIx and " + DBBaseAttr.ParaPreChar + "endIx "); //ComDBFun.paraChar

            Dictionary<string, object> pageDic = new Dictionary<string, object>() {
                { "startIx",pageSize *(pageIndex-1 ) +1 },
                {"endIx", pageSize * pageIndex}
            };

            DbParameter[] pms = GetDbParametersFromDic(pageDic);
            try
            {
                string text = strSql.ToString();

                ExeResEdm res = GetDataTable(strSql.ToString(), pms);
                if (res.ErrCode == 0 && res.ExeNum > 0)
                {
                    totalCnt = Convert.ToInt32((res.ExeModel as DataTable).Rows[0]["dbtotal"].ToString());
                }
                return res;
            }

            catch (Exception ex)
            {
                return new ExeResEdm() { ErrCode = 1, ExBody = ex, Module = "GetDataByPage" };
            }

        }

        //SQL Server 和 MySql 可以使用此方法，oracle 不行
        protected virtual string GetColumnsNameSql(string strTbName, string strField = "*")
        {
            string strSqlTxt = "select top 0 " + strField.Trim().Trim(',') + " from " + strTbName;
            return strSqlTxt;
        }

        protected string GetTableNameFromSelectSql(string selectSql)
        {
            return ComDBFun.GetTableNameFromSelectSql(selectSql);
        }

        protected string AddTableNameOrSqlText(string tableOrSql)
        {
            if (string.IsNullOrEmpty(tableOrSql))
            {
                return ":" + tableOrSql;
            }
            return "";
        }

        protected DbParameter[] GetDbParametersFromDic(Dictionary<string, object> dic)
        {
            List<DbParameter> list = new List<DbParameter>();

            if (dic == null || dic.Count <= 0)
            {
                return list.ToArray();
            }
            List<string> colNames = dic.Keys.ToList(); List<object> colValues = dic.Values.ToList();
            for (int i = 0; i < dic.Count; i++)
            {
                DbParameter cur = GetOneDbParameter(DBBaseAttr.ParaPreChar + ComDBFun.RemoveSpecialChar(colNames[i]), GetValue(colValues[i]));
                list.Add(cur);
            }
            return list.ToArray();

        }


        #endregion 可继承的方法，子类方法 


        #region 私有方法   
        protected object GetValue(object value)
        {
            if (value.GetType().Name == "".GetType().Name)
            {
                return value.ToString().Trim();
            }
            if (value.GetType().BaseType.Name == "Enum")
            {
                var enumVal = Enum.Parse(value.GetType(), value.ToString());
                return (int)enumVal;
            }
            return value;
        }

        string GetRealSql(string cmdText, params DbParameter[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var dic = parameters.ToDictionary(k => k.ParameterName, v => v.Value);
                foreach (var item in dic)
                {
                    var val = item.Value != null ? item.Value.ToString().Trim() : "null";
                    cmdText = cmdText.Replace(item.Key, val);
                }
            }
            return "[" + cmdText + "]语句";
        }


        string GetRealSql(List<SqlContianer> ltSqls)
        {
            List<string> res = new List<string>();
            foreach (var item in ltSqls)
            {
                res.Add(GetRealSql(item.strSqlTxt, item.ltOraParams.ToArray()));
            }
            return string.Join(";", res);
        }


        ExeResEdm GetDTByPage(SearchParam searchParam)
        {
            string strWhere = searchParam.StrWhere;
            string orderby = searchParam.Orderby;

            int pageSize = searchParam.PageSize;
            int pageIndex = searchParam.PageIndex;
            int totalCnt = 0;
            if (string.IsNullOrEmpty(orderby) || string.IsNullOrEmpty(orderby.Trim()))
            {
                return null;
            }

            ExeResEdm dtRes = GetDataByPage(searchParam.TableName, strWhere, orderby, pageIndex, pageSize, out totalCnt);
            searchParam.TotalCount = totalCnt;
            return dtRes;
        }

        ExeResEdm GetDataTable(string cmdText, params DbParameter[] parameters)
        {
            DataTable dt = new DataTable();
            parameters = ParameterPrepare(parameters);
            var res = SqlCMD_DT(cmdText, CommandType.Text, adt => adt.Fill(dt), parameters);
            res.ExeModel = dt;
            return res;
        }

        //批量添加/更新/删除数据时所使用的的 DataTable，更新使用的是相同值
        DataTable GetDataTable<V>(string tableName, DataTable dtTemplate, List<V> paras, Dictionary<string, string> updateDic = null)
        {
            if (updateDic == null || updateDic.Count <= 0)
            {
                return GetDataTableComplex<V>(tableName, dtTemplate, paras, null);
            }
            return GetDataTableComplex<V>(tableName, dtTemplate, paras, new List<Dictionary<string, string>> { updateDic });
        }

        //批量添加/更新/删除数据时所使用的的 DataTable，更新使用的是不同值值
        DataTable GetDataTableComplex<V>(string tableName, DataTable dt, List<V> paras, List<Dictionary<string, string>> updateDicList = null)
        {
            //DtModelConvertHelper<T>.ModelListToDT(paras);  //不行
            if (dt == null || dt.Columns.Count <= 0)
            {
                dt = (SelectDBTableFormat(tableName).ExeModel) as DataTable;
            }
            else
            {
                dt = dt.Clone();
            }

            dt.TableName = tableName;
            foreach (var item in paras)
            {
                DataRow dr = dt.NewRow();
                dr = DtModelConvert<V>.ObjConvertToDr(item, dr);
                dt.Rows.Add(dr);
            }

            List<string> errList = new List<string>();
            if (updateDicList != null && updateDicList.Count > 0)//是更新
            {
                dt.AcceptChanges();

                //检查和更新列名
                //List<string> dtCols = new List<string>();
                //foreach (DataColumn item in dt.Columns)
                //{
                //    dtCols.Add(item.ColumnName);
                //}

                if (errList.Count > 0)
                {
                    throw new Exception("字段[" + string.Join(" , ", errList) + "]不是" + tableName + "表的字段，无法更新");
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow item = dt.Rows[i];
                    var updateDic = updateDicList.Count == 1 ? updateDicList[0] : updateDicList[i];
                    if (updateDic == null || updateDic.Count <= 0)
                    {
                        continue;
                    }
                    foreach (var col in updateDic)
                    {
                        item[col.Key] = col.Value;
                    }
                }
            }
            return dt;
        }


        void WriteLogMsg(DBOperUser logUser, LogType logType, string detail, string module)
        {
            if (logUser != null && !string.IsNullOrEmpty(detail))
            {
                LogTraceEdm logTraceEdm = new LogTraceEdm() { Detail = detail, TabOrModu = module, LogType = logType };
                if (logUser != null)
                {
                    logTraceEdm.UserId = logUser.UserId;
                    logTraceEdm.UserName = logUser.UserName;
                }
                WriteLogMsg(logTraceEdm, null, null);
            }
        }

        void WriteLogMsg(LogTraceEdm logMsg, object res, string keyID, params object[] values)
        {
            var skip = Log2Net.Util.AppConfig.GetConfigValue("SkipQueryTypeLog");
            if (logMsg.LogType == LogType.查询 && skip == "1")
            {
                return;
            }
            if (logMsg != null && (!string.IsNullOrEmpty(logMsg.Detail) || !string.IsNullOrEmpty(logMsg.UserId + logMsg.UserName)))
            {
                try
                {
                    if (string.IsNullOrEmpty(logMsg.Detail))
                    {
                        string valStr = "";
                        if (values != null && values.Length > 0)
                        {
                            valStr = "(" + string.Join(",", values) + ")";
                        }
                        string str = "SQL/条件";
                        if (keyID.Contains("="))
                        {
                            //  str = "语句";
                        }
                        string action = logMsg.LogType.ToString();
                        string msg = action + "了" + str + "为【" + keyID + "】的" + logMsg.TabOrModu + "记录" + valStr + "，结果为" + res + "。";
                        logMsg.Detail = msg;
                    }
                    int maxLength = 2000;
                    logMsg.Detail = logMsg.Detail.Substring(0, Math.Min(maxLength, logMsg.Detail.Length));//当字段过长时，截取前一部分，防止插入失败
                    try { var logRes = Task.Factory.StartNew(() => Log2Net.LogApi.WriteLog(LogLevel.DBRec, logMsg)); } catch { }
                }
                catch (Exception ex)
                {
                    var exModel = new
                    {
                        ErrMsg = "写数据失败",
                        DBData = logMsg,
                        Exception = ex.Message,
                        InnerException = ex.InnerException == null ? "" : ex.InnerException.Message
                    };
                    Log2Net.LogApi.WriteMsgToInfoFile(exModel);
                }
            }
        }




        #endregion 私有方法


        #region 抽象方法   
        protected abstract ExeResEdm SqlCMD_DT(string cmdText, CommandType commandType, Func<DbDataAdapter, int> fun, params DbParameter[] parameters);

        protected abstract ExeResEdm SqlCMD(string sql, CommandType commandType, Func<DbCommand, object> fun, params DbParameter[] pms);

        protected abstract ExeResEdm UpdateDtToDB(DataTable dtInfos, string strComFields = "*");

        protected abstract ExeResEdm UpdateDsToDB(DataSet dsTables, Dictionary<string, string> dicDtMainFields = null);

        protected abstract ExeResEdm GetDataSets(List<SqlContianer> ltSqls);

        protected abstract ExeResEdm ExecuteNonQueryFromSqlContianer(List<SqlContianer> ltSqls);

        protected abstract DbParameter GetOneDbParameter(string name, object value);

        protected abstract DbParameter[] ParameterPrepare(DbParameter[] parameters);

        protected abstract CRUDSql MakeConditionFieldForIn(List<string> ltDataVals);


        #endregion 抽象方法
    }



}
