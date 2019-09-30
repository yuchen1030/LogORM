
//#define MS_OracleClient  // 是采用微软oracle类库还是oracle自家的类库

using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

#if MS_OracleClient
using System.Data.OracleClient;
#else
using Oracle.ManagedDataAccess.Client;
#endif

namespace LogORM.AdoNet.Oracle
{
    /// <summary>
    /// 使用Oracle.ManagedDataAccess.Client实现的oracle 数据库访问类(无需安装客户端)，无32位/64位之分，但仅支持Oracle10g及以上
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OracleHelper<T> : OracleHelperBase<T> where T : class
    {
        internal OracleHelper(string strConnStr) : base(strConnStr)
        {
            connstr = strConnStr;
        }

        protected override ExeResEdm SqlCMD_DT(string cmdText, CommandType commandType, Func<DbDataAdapter, int> fun, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                parameters = ParameterPrepare(parameters);
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    cmd.CommandType = commandType;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange((parameters));
                    }
                    var da = new OracleDataAdapter(cmd);
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

        protected override ExeResEdm SqlCMD(string sql, Func<DbCommand, object> fun, params DbParameter[] pms)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                pms = ParameterPrepare(pms);
                using (OracleConnection con = new OracleConnection(connstr))
                {
                    using (OracleCommand cmd = new OracleCommand(sql, con))
                    {
                        con.Open();

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
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = GetColumnsNameSql(strTableName, strComFields);
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    adapter.UpdateCommand = new OracleCommandBuilder(adapter).GetUpdateCommand();
                    adapter.Update(dtInfos.GetChanges());
                    dtInfos.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "UpdateDtToDB方法";
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
            try
            {
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        OracleCommand cmd = conn.CreateCommand();
                        cmd.Transaction = tsOprate;
                        foreach (DataTable dtTemp in dsTables.Tables)
                        {
                            string strComFields = "*";
                            if (dicDtMainFields != null && dicDtMainFields.Count > 0 && dicDtMainFields.ContainsKey(dtTemp.TableName))
                            {
                                strComFields = dicDtMainFields[dtTemp.TableName];
                            }
                            cmd.CommandText = GetColumnsNameSql(dtTemp.TableName, strComFields);
                            OracleDataAdapter adapter = new OracleDataAdapter(cmd);                                                                    

                            var dtChanges = dtTemp.GetChanges();
                            adapter.FillSchema(dtChanges, SchemaType.Mapped);//new added
                            if (dtChanges != null)  //是添加或更新
                            {
                                adapter.UpdateCommand = new OracleCommandBuilder(adapter).GetUpdateCommand();
                                n += adapter.Update(dtChanges);
                                dtTemp.AcceptChanges();
                            }
                            else //是删除
                            {
                                adapter.DeleteCommand = new OracleCommandBuilder(adapter).GetDeleteCommand();
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
                        dBResEdm.Module = "UpdateDsToDB方法";
                        dBResEdm.ExBody = ex;
                        dBResEdm.ErrCode = 1;
                        return dBResEdm;
                    }

                }
            }
            catch (Exception ex)
            {
                dBResEdm.Module = "UpdateDsToDB方法";
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
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleTransaction oraOprate = conn.BeginTransaction();
                    try
                    {
                        OracleCommand cmd = conn.CreateCommand();
                        cmd.Transaction = oraOprate;
                        foreach (SqlContianer objOraSqlCon in ltSqls)
                        {
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            curSQL = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            int intRes = cmd.ExecuteNonQuery();
                            dBResEdm.ExeNum += intRes;
                            if (objOraSqlCon.intExpectNums >= 0)
                            {
                                if (intRes != objOraSqlCon.intExpectNums)
                                    throw new Exception("Update records not match the expect nums");
                            }
                            else if (objOraSqlCon.intExpectNums != Int16.MinValue)
                            {
                                if (intRes != 0 && intRes != objOraSqlCon.intExpectNums * -1)
                                    throw new Exception("Update records not match the expect nums");
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
            try
            {
                using (OracleConnection conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    OracleTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        OracleCommand cmd = conn.CreateCommand();
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
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            if (objOraSqlCon.ltOraParams != null && objOraSqlCon.ltOraParams.Count > 0)
                            {
                                cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            }
                            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                            adapter.Fill(dt);
                            ds.Tables.Add(dt);
                        }
                        tsOprate.Commit();
                    }
                    catch (Exception ex)
                    {
                        tsOprate.Rollback();
                        dBResEdm.Module = "GetDataSets方法";
                        dBResEdm.ExBody = ex;
                        dBResEdm.ErrCode = 1;
                        return dBResEdm;
                    }
                }

            }
            catch (Exception ex)
            {
                dBResEdm.Module = "GetDataSets方法";
                dBResEdm.ExBody = ex;
                dBResEdm.ErrCode = 1;
                return dBResEdm;
            }
            dBResEdm.ExeModel = ds;
            return dBResEdm;
        }

        protected override DbParameter GetOneDbParameter(string name, object value)
        {
            OracleParameter cur = new OracleParameter(name, value);
            return cur;
        }

        protected override DbParameter[] ParameterPrepare(DbParameter[] parameters)
        {
            return parameters;
            var paras = parameters.Select(a => new OracleParameter(a.ParameterName, a.Value)).ToArray();
            return paras;
        }


        protected override CRUDSql MakeConditionFieldForIn(List<string> ltDataVals)
        {
            return null;
        }

        public static string MakeConditionFieldForIn(List<string> ltDataVals, string strPrefix, ref List<OracleParameter> ltParams)
        {
            string strConditions = "";
            for (int i = 0; i < ltDataVals.Count(); i++)
            {
                string strFieldOcc = strPrefix + "_" + i;
                strConditions += "," + strFieldOcc;
                ltParams.Add(new OracleParameter(strFieldOcc, ltDataVals[i]));
            }
            strConditions = strConditions.Trim(',');
            return strConditions;
        }

    }

}
