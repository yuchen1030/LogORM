#if NET

using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;


namespace LogORM.AdoNet.Oracle
{
    /// <summary>
    /// 使用System.Data.OracleClient实现的oracle 数据库访问类(需安装客户端)，有32位/64位之分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OracleHelperMS<T> : OracleHelperBase<T> where T : class
    {
        internal OracleHelperMS(string strConnStr) : base(strConnStr)
        {
            connstr = strConnStr;
        }

        protected override ExeResEdm SqlCMD_DT(string cmdText, CommandType commandType, Func<DbDataAdapter, int> fun, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                parameters = ParameterPrepare(parameters);
                using (System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    conn.Open();
                    System.Data.OracleClient.OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    cmd.CommandType = commandType;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange((parameters));
                    }
                    var da = new System.Data.OracleClient.OracleDataAdapter(cmd);
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
                using (System.Data.OracleClient.OracleConnection con = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    using (System.Data.OracleClient.OracleCommand cmd = new System.Data.OracleClient.OracleCommand(sql, con))
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
                using (System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    conn.Open();
                    System.Data.OracleClient.OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = GetColumnsNameSql(strTableName, strComFields);
                    System.Data.OracleClient.OracleDataAdapter adapter = new System.Data.OracleClient.OracleDataAdapter(cmd);
                    adapter.UpdateCommand = new System.Data.OracleClient.OracleCommandBuilder(adapter).GetUpdateCommand();
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
                using (System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    conn.Open();
                    System.Data.OracleClient.OracleTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        System.Data.OracleClient.OracleCommand cmd = conn.CreateCommand();
                        cmd.Transaction = tsOprate;
                        foreach (DataTable dtTemp in dsTables.Tables)
                        {
                            strTableName = dtTemp.TableName;
                            string strComFields = "*";
                            if (dicDtMainFields != null && dicDtMainFields.Count > 0 && dicDtMainFields.ContainsKey(dtTemp.TableName))
                            {
                                strComFields =  ! string.IsNullOrEmpty(dicDtMainFields[dtTemp.TableName]) ?    dicDtMainFields[dtTemp.TableName]: strComFields;
                            }
                            cmd.CommandText = GetColumnsNameSql(dtTemp.TableName, strComFields);
                            System.Data.OracleClient.OracleDataAdapter adapter = new System.Data.OracleClient.OracleDataAdapter(cmd);

                            var dtChanges = dtTemp.GetChanges();
                            adapter.FillSchema(dtChanges, SchemaType.Mapped);//new added
                            if (dtChanges != null)  //是添加或更新
                            {
                                adapter.UpdateCommand = new System.Data.OracleClient.OracleCommandBuilder(adapter).GetUpdateCommand();
                                n += adapter.Update(dtChanges);
                                dtTemp.AcceptChanges();
                            }
                            else //是删除
                            {
                                adapter.DeleteCommand = new System.Data.OracleClient.OracleCommandBuilder(adapter).GetDeleteCommand();
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
                using (System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    conn.Open();
                    System.Data.OracleClient.OracleTransaction oraOprate = conn.BeginTransaction();
                    try
                    {
                        System.Data.OracleClient.OracleCommand cmd = conn.CreateCommand();
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
                                    throw new Exception("Update records["+ intRes + "] not match the expect nums["+ objOraSqlCon.intExpectNums + "]");
                            }
                            else if (objOraSqlCon.intExpectNums < 0)
                            {
                                if (intRes != 0 && intRes != objOraSqlCon.intExpectNums * -1)
                                    throw new Exception("Update records["+ intRes + "] not match the expect nums["+ objOraSqlCon.intExpectNums + "]");
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
                using (System.Data.OracleClient.OracleConnection conn = new System.Data.OracleClient.OracleConnection(connstr))
                {
                    conn.Open();
                    System.Data.OracleClient.OracleTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        System.Data.OracleClient.OracleCommand cmd = conn.CreateCommand();
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

                            System.Data.OracleClient.OracleDataAdapter adapter = new System.Data.OracleClient.OracleDataAdapter(cmd);
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
            System.Data.OracleClient.OracleParameter cur = new System.Data.OracleClient.OracleParameter(name, value);
            return cur;
        }
        
        protected override DbParameter[] ParameterPrepare(DbParameter[] parameters)
        {
            return parameters;
            var paras = parameters.Select(a => new System.Data.OracleClient.OracleParameter(a.ParameterName, a.Value.ToString())).ToArray();
            return paras;
        }

        protected override CRUDSql MakeConditionFieldForIn(List<string> ltDataVals)
        {
            return null;
        }



    }
}

#endif