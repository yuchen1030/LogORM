using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace LogORM.AdoNet
{
    /// <summary>
    /// sql server 数据库访问类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SqlServerHelper<T> : AdoNetBase<T>, IAdoNetBase<T> where T : class
    {
        readonly DBBaseAttr dbBaseAttr = new DBBaseAttr() { DBStoreType = DBStoreType.SqlServer, LeftPre = "[", ParaPreChar = "@", RightSuf = "]" };

        protected override DBBaseAttr DBBaseAttr { get { return dbBaseAttr; } }

        public SqlServerHelper(string strConnStr) : base(strConnStr)
        {
            connstr = strConnStr;
        }

        protected override ExeResEdm SqlCMD_DT(string cmdText, CommandType commandType, Func<DbDataAdapter, int> fun, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                parameters = ParameterPrepare(parameters);
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    cmd.CommandType = commandType;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange((parameters));
                    }
                    var da = new SqlDataAdapter(cmd);
                    var res = fun(da);
                    dBResEdm.ExeNum = res;
                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "SqlCMD_DT 方法";
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            return dBResEdm;
        }

        protected override ExeResEdm SqlCMD(string sql, CommandType commandType, Func<DbCommand, object> fun, params DbParameter[] pms)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                pms = ParameterPrepare(pms);
                using (SqlConnection con = new SqlConnection(connstr))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        con.Open();
                        cmd.CommandType = commandType;
                        if (pms != null && pms.Length > 0)
                        {
                            cmd.Parameters.AddRange((pms));
                        }
                        var res = fun(cmd);
                        dBResEdm.ExeModel = res;
                        return dBResEdm;
                    }
                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "SqlCMD方法";
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;

            }
        }



        protected override ExeResEdm UpdateDtToDB(DataTable dtInfos, string strComFields = "*")
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            string strTableName = dtInfos.TableName;
            try
            {
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = GetColumnsNameSql(strTableName, strComFields);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.UpdateCommand = new SqlCommandBuilder(adapter).GetUpdateCommand();
                    adapter.Update(dtInfos.GetChanges());
                    dtInfos.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "UpdateDtToDB方法" + AddTableNameOrSqlText(strTableName);
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            return dBResEdm;
        }


        protected override ExeResEdm UpdateDsToDB(DataSet dsTables, Dictionary<string, string> dicDtMainFields = null)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            int n = 0;
            string strTableName = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();

                        foreach (DataTable dtTemp in dsTables.Tables)
                        {
                            string strComFields = "*";
                            if (dicDtMainFields != null && dicDtMainFields.Count > 0 && dicDtMainFields.ContainsKey(dtTemp.TableName))
                            {
                                strComFields = dicDtMainFields[dtTemp.TableName];
                            }
                            cmd.CommandText = GetColumnsNameSql(dtTemp.TableName, strComFields);
                            cmd.Transaction = tsOprate;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                            var dtChanges = dtTemp.GetChanges();
                            if (dtChanges != null)  //是添加或更新
                            {
                                adapter.UpdateCommand = new SqlCommandBuilder(adapter).GetUpdateCommand();
                                n += adapter.Update(dtChanges);
                                dtTemp.AcceptChanges();
                            }
                            else //是删除
                            {
                                adapter.DeleteCommand = new SqlCommandBuilder(adapter).GetDeleteCommand();
                                for (int i = dtTemp.Rows.Count - 1; i >= 0; i--)
                                {
                                    dtTemp.Rows[i].Delete();
                                }
                                n += adapter.Update(dtTemp);
                            }
                        }
                        dsTables.AcceptChanges();
                        tsOprate.Commit();
                    }
                    catch (Exception ex)
                    {
                        tsOprate.Rollback();
                        dBResEdm.Module = "UpdateDsToDB方法" + AddTableNameOrSqlText(strTableName);
                        dBResEdm.ExBody = ex;
                        dBResEdm.ErrCode = 1;
                        return dBResEdm;
                    }

                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "UpdateDsToDB方法" + AddTableNameOrSqlText(strTableName);
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            dBResEdm.ExeNum = n;
            return dBResEdm;
        }


        protected override ExeResEdm ExecuteNonQueryFromSqlContianer(List<SqlContianer> ltSqls)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            string curSQL = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlTransaction oraOprate = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.Transaction = oraOprate;
                        foreach (SqlContianer objOraSqlCon in ltSqls)
                        {
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            curSQL = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            int intRes = cmd.ExecuteNonQuery();
                            dBResEdm.ExeNum += intRes;
                            if (objOraSqlCon.intExpectNums > 0)
                            {
                                if (intRes != objOraSqlCon.intExpectNums)
                                    throw new Exception("Update records[" + intRes + "] not match the expect nums[" + objOraSqlCon.intExpectNums + "]");
                            }
                            else if (objOraSqlCon.intExpectNums < 0)
                            {
                                if (intRes != 0 && intRes != objOraSqlCon.intExpectNums * -1)
                                    throw new Exception("Update records[" + intRes + "] not match the expect nums[" + objOraSqlCon.intExpectNums + "]");
                            }

                        }
                        oraOprate.Commit();
                    }
                    catch (Exception ex)
                    {
                        oraOprate.Rollback();
                        dBResEdm.Module = "ExecuteNonQueryFromSqlContianer方法";
                        dBResEdm.ExBody = ex;
                        dBResEdm.ErrCode = 1;
                        return dBResEdm;
                    }
                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "ExecuteNonQueryFromSqlContianer方法";
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            return dBResEdm;
        }

        protected override ExeResEdm GetDataSets(List<SqlContianer> ltSqls)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            DataSet ds = new DataSet();
            string curTableName = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    conn.Open();
                    SqlTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.Transaction = tsOprate;
                        List<string> tbNames = new List<string>();
                        foreach (var objOraSqlCon in ltSqls)
                        {
                            DataTable dt = new DataTable();
                            if (!string.IsNullOrEmpty(objOraSqlCon.tableName))
                            {
                                dt.TableName = objOraSqlCon.tableName;
                            }
                            else
                            {
                                string tb = GetTableNameFromSelectSql(objOraSqlCon.strSqlTxt);
                                if (!string.IsNullOrEmpty(tb))
                                {
                                    dt.TableName = tb;
                                }
                            }
                            if (tbNames.Contains(dt.TableName))
                            {
                                dt.TableName = dt.TableName + "_" + (tbNames.Count() + 1);
                            }
                            tbNames.Add(dt.TableName);
                            curTableName = dt.TableName;
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            if (objOraSqlCon.ltOraParams != null && objOraSqlCon.ltOraParams.Count > 0)
                            {
                                cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            }
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt);
                            ds.Tables.Add(dt);
                        }
                        tsOprate.Commit();
                    }
                    catch (Exception ex)
                    {
                        tsOprate.Rollback();
                        dBResEdm.Module = "GetDataSets方法" + AddTableNameOrSqlText(curTableName);
                        dBResEdm.ExBody = ex;
                        dBResEdm.ErrCode = 1;
                        return dBResEdm;
                    }
                }

            }
            catch (Exception ex)
            {
                dBResEdm.Module = "GetDataSets方法" + AddTableNameOrSqlText(curTableName);
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            dBResEdm.ExeModel = ds;
            return dBResEdm;
        }

        protected override DbParameter GetOneDbParameter(string name, object value)
        {
            SqlParameter cur = new SqlParameter(name, value);
            return cur;
        }

        protected override DbParameter[] ParameterPrepare(DbParameter[] parameters)
        {
            return parameters;
            var paras = parameters.Select(a => new SqlParameter(a.ParameterName, a.Value)).ToArray();
            return paras;
        }


        protected override CRUDSql MakeConditionFieldForIn(List<string> ltDataVals)
        {
            return null;
        }


    }


}
