using Log2Net.Models;
using LogORM.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace LogORM.AdoNet
{
    /// <summary>
    /// sql server 数据库访问类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MySqlHelper<T> : AdoNetBase<T>, IAdoNetBase<T> where T : class
    {
        readonly DBBaseAttr dbBaseAttr = new DBBaseAttr() { DBStoreType = DBStoreType.MySql, LeftPre = "", ParaPreChar = "?", RightSuf = "" };

        protected override DBBaseAttr DBBaseAttr { get { return dbBaseAttr; } }

        internal MySqlHelper(string strConnStr) : base(strConnStr)
        {
            connstr = strConnStr;
        }

        protected override ExeResEdm SqlCMD_DT(string cmdText, CommandType commandType, Func<DbDataAdapter, int> fun, params DbParameter[] parameters)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                parameters = ParameterPrepare(parameters);
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = cmdText;
                    cmd.CommandType = commandType;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange((parameters));
                    }
                    var da = new MySqlDataAdapter(cmd);
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


        //select SQL_CALC_FOUND_ROWS * from log_systemmonitor where OnlineCnt >1 order by time DESC limit 0,5;
        //SELECT FOUND_ROWS();
        //SELECT SQL_CALC_FOUND_ROWS @rowno:=@rowno+1 as rowno,r.* from log_systemmonitor r,(select @rowno:= 0) t where OnlineCnt >1 order by time DESC;
        //SELECT FOUND_ROWS();
        protected override ExeResEdm GetDataByPage(string tableName, string strWhere, string orderby, int pageIndex, int pageSize, out int totalCnt )
        {
            totalCnt = 0;
            StringBuilder strSql = new StringBuilder();
            string columns = "";//为空，则获取全部列
            if (string.IsNullOrEmpty(orderby) || string.IsNullOrEmpty(orderby.Trim()))
            {
                return null;
            }

            if (string.IsNullOrEmpty(columns))
            {
                columns = "*";
            }
            columns = "SQL_CALC_FOUND_ROWS " + columns;

            strSql.Append("SELECT " + columns + " FROM " + tableName);
            if (!string.IsNullOrEmpty(strWhere) && !string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(" order by " + orderby + " limit " + DBBaseAttr.ParaPreChar + "startIx," + DBBaseAttr.ParaPreChar + "endIx;");
            strSql.Append("SELECT FOUND_ROWS();");

            Dictionary<string, object> pageDic = new Dictionary<string, object>() {
                { "startIx",pageSize *(pageIndex-1 )  },
                {"endIx", pageSize }
            };

            DbParameter[] pms = GetDbParametersFromDic(pageDic);
            try
            {
                string text = strSql.ToString();
                ExeResEdm ds = GetDataSet(strSql.ToString(),null, pms);
                if (ds != null && ds.ErrCode == 0 && (ds.ExeModel as DataSet).Tables.Count > 1)
                {
                    totalCnt = Convert.ToInt32((ds.ExeModel as DataSet).Tables[1].Rows[0][0].ToString());
                    return new ExeResEdm() { ExeModel = (ds.ExeModel as DataSet).Tables[0] };
                }
                else
                {
                    return ds;
                }
            }
            catch (Exception ex)
            {
                return new ExeResEdm() { ErrCode = 1, ExBody = ex, Module = "GetDataByPage-MySql" };
            }
        }

        protected override ExeResEdm SqlCMD(string sql, Func<DbCommand, object> fun, params DbParameter[] pms)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            try
            {
                pms = ParameterPrepare(pms);
                using (MySqlConnection con = new MySqlConnection(connstr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
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
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = GetColumnsNameSql(strTableName, strComFields);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.UpdateCommand = new MySqlCommandBuilder(adapter).GetUpdateCommand();
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

        protected override ExeResEdm UpdateDsToDB(DataSet dsTables, Dictionary<string, string> dicDtFields = null)
        {
            ExeResEdm dBResEdm = new ExeResEdm();
            int n = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        MySqlCommand cmd = conn.CreateCommand();

                        foreach (DataTable dtTemp in dsTables.Tables)
                        {
                            string strComFields = "*";
                            if (dicDtFields != null && dicDtFields.Count > 0 && dicDtFields.ContainsKey(dtTemp.TableName))
                            {
                                strComFields = dicDtFields[dtTemp.TableName];
                            }
                            cmd.CommandText = GetColumnsNameSql(dtTemp.TableName, strComFields);
                            cmd.Transaction = tsOprate;
                            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                            var dtChanges = dtTemp.GetChanges();
                            adapter.FillSchema(dtChanges, SchemaType.Mapped);//new added
                            if (dtChanges != null)  //是添加或更新
                            {
                                adapter.UpdateCommand = new MySqlCommandBuilder(adapter).GetUpdateCommand();
                                n += adapter.Update(dtChanges);
                                dtTemp.AcceptChanges();
                            }
                            else //是删除
                            {
                                adapter.DeleteCommand = new MySqlCommandBuilder(adapter).GetDeleteCommand();
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
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlTransaction oraOprate = conn.BeginTransaction();
                    try
                    {
                        MySqlCommand cmd = conn.CreateCommand();
                        cmd.Transaction = oraOprate;
                        foreach (SqlContianer objOraSqlCon in ltSqls)
                        {
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            curSQL = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            int intRes = cmd.ExecuteNonQuery();
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
                using (MySqlConnection conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    MySqlTransaction tsOprate = conn.BeginTransaction();
                    try
                    {
                        MySqlCommand cmd = conn.CreateCommand();
                        cmd.Transaction = tsOprate;
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
                            cmd.CommandText = objOraSqlCon.strSqlTxt;
                            cmd.Parameters.Clear();
                            if (objOraSqlCon.ltOraParams != null && objOraSqlCon.ltOraParams.Count > 0)
                            {
                                cmd.Parameters.AddRange(objOraSqlCon.ltOraParams.ToArray());
                            }
                            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
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
            MySqlParameter cur = new MySqlParameter(name, value);
            return cur;
        }

        protected override DbParameter[] ParameterPrepare(DbParameter[] parameters)
        {
            return parameters;
            var paras = parameters.Select(a => new MySqlParameter(a.ParameterName, a.Value)).ToArray();
            //  var paras2 = parameters.Select(a => new MySqlParameter(a.ParameterName,a.DbType,a.Size,a.Direction,a.IsNullable,a.Precision, a.Scale,a.SourceColumn,a.SourceVersion, a.Value)).ToArray();
            //  string parameterName, MySqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value
            return paras;
        }


        protected override SelectSql MakeConditionFieldForIn(List<string> ltDataVals)
        {
            return null;
        }

    }


}




