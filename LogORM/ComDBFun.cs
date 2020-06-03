﻿

using LogORM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using LogORM.ComUtil;
using Log2Net.Util;

namespace LogORM
{

    internal class DBBaseAttr
    {
        public DBBaseAttr()
        {
            DBStoreType = DBStoreType.NoSelect;
            ParaPreChar = "";
            LeftPre = "";
            RightSuf = "";
        }
        public DBStoreType DBStoreType { get; set; }
        public string ParaPreChar { get; set; }  //参数化查询时参数前的符号
        public string LeftPre { get; set; }  //参数名左边的符号
        public string RightSuf { get; set; }//参数名右边的符号



    }
    internal class ComDBFun
    {
        public ComDBFun(DBBaseAttr dBBaseAttr)
        {
            paraChar = dBBaseAttr.ParaPreChar;
            leftPre = dBBaseAttr.LeftPre;
            rightSuf = dBBaseAttr.RightSuf;
        }

        string paraChar = "@"; // define parameter sign
        string leftPre = "";
        string rightSuf = "";

        //获取数据库连接字符串
        internal static string GetConnectionString(string sqlStrKey)
        {
            //if (AppConfig.GetFinalConfig("ConnectStrInCode", false, false))
            //{
            //    return "";
            //}
            //else
            {
              //  string sqlStrKey = GetConnectionStringKey(dbType);
                string conStr = AppConfig.GetDBConnectString(sqlStrKey);
                return conStr;
            }

        }

        ////获取数据库连接字符串的key
        //public static string GetConnectionStringKey(DBType dbType)
        //{
        //    string sqlStrKey = "sqlStrTest";
        //    switch (dbType)
        //    {
        //        case DBType.LogTrace:
        //            sqlStrKey = AppConfig.GetFinalConfig("UserCfg_TraceDBConKey", "logTraceSqlStr", "");
        //            break;
        //        case DBType.LogMonitor:
        //            sqlStrKey = AppConfig.GetFinalConfig("UserCfg_MonitorDBConKey", "logMonitorSqlStr", "");
        //            break;
        //    }
        //    return sqlStrKey;
        //}


        //    static Dictionary<DBType, DBGeneral> DBGeneralDic = new Dictionary<DBType, DBGeneral>();

        //public static DBGeneral GetDBGeneralInfo(DBType dbType)
        //{
        //    if (DBGeneralDic.ContainsKey(dbType))
        //    {
        //        return DBGeneralDic[dbType];
        //    }

        //    DBStoreType curDBType = DBStoreType.SqlServer;
        //    if (dbType == DBType.LogTrace)
        //    {
        //        curDBType = AppConfig.GetFinalConfig("UserCfg_TraceDBTypeKey", DBStoreType.SqlServer,  DBStoreType.SqlServer);
        //    }
        //    else
        //    {
        //        curDBType = AppConfig.GetFinalConfig("UserCfg_MonitorDBTypeKey", DBStoreType.SqlServer,  DBStoreType.SqlServer);
        //    }

        //    DBGeneral dBGeneral = new DBGeneral() { DBStoreType = curDBType };

        //    if (curDBType == DBStoreType.SqlServer)
        //    {
        //        dBGeneral.SchemaName = "dbo";
        //    }
        //    else if (curDBType == DBStoreType.Oracle)
        //    {
        //        dBGeneral.SchemaName = "scott";
        //    }
        //    else if (curDBType == DBStoreType.MySql)
        //    {
        //        // dBGeneral.SchemaName = "";
        //    }
        //    DBGeneralDic.Add(dbType, dBGeneral);
        //    string msg = dbType.ToString() + "的数据库类型为【" + dBGeneral.DBStoreType.ToString() + "】";
        //   // LogApi.WriteMsgToDebugFile(new { 内容 = msg });
        //    return dBGeneral;
        //}

        internal string GetSQLText(List<string> colNames, List<object> values)
        {
            if (colNames == null || colNames.Count <= 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder("(" + leftPre);
            sb.Append(string.Join(rightSuf + "," + leftPre, colNames));
            sb.Append(rightSuf + ")");

            //string[] colParamNames = GetColumnNames(colNames, paraChar);  //得到数组形式的@列名    
            ////sb.Append(" values([").Append(string.Join("],[", colParamNames)).Append("])");//values([@UserName],[@UserPWD])
            //sb.Append(" values(").Append(string.Join(",", colParamNames)).Append(")"); //values(@UserName,@UserPWD)
            //return sb.ToString();

            string valuesSql = "";
            if (values == null || values.Count <= 0)
            {
                string[] colParamNames = GetColumnNames(colNames, paraChar);  //得到数组形式的@列名  
                valuesSql = string.Join(",", colParamNames);
            }
            else
            {
                valuesSql = "'" + string.Join("','", values) + "'";
            }
            //sb.Append(" values([").Append(string.Join("],[", colParamNames)).Append("])");//values([@UserName],[@UserPWD])
            sb.Append(" values(").Append(valuesSql).Append(")"); //values(@UserName,@UserPWD)
            return sb.ToString();
        }

        internal string GetUpdateSQLText(List<string> colNames)
        {
            if (colNames == null || colNames.Count <= 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < colNames.Count; i++)
            {
                sb.Append(leftPre + colNames[i] + rightSuf + "=" + paraChar + RemoveSpecialChar(colNames[i]) + ",");
            }
            return sb.ToString().TrimEnd(',');
        }

        internal string GetWhereCondition(List<string> colNames, string and_or, params Dictionary<string, object>[] kvDic)
        {
            if (colNames == null || colNames.Count <= 0)
            {
                return "";
            }

            string result = "";
            for (int i = 0; i < colNames.Count; i++)
            {
                if (kvDic != null && kvDic.Length > 0 && kvDic[0].ContainsKey(colNames[i]))
                {
                    string[] values = kvDic[0][colNames[i]].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < values.Length; j++)
                    {
                        result += (leftPre + colNames[i] + rightSuf + "=" + paraChar + RemoveSpecialChar(colNames[i]) /*+ j*/);
                        if (j != values.Length - 1)
                        {
                            result += " " + and_or + " ";
                        }
                    }
                }
                else
                {
                    if (!colNames[i].Contains("="))
                    {
                        result += (leftPre + colNames[i] + rightSuf + "=" + paraChar + RemoveSpecialChar(colNames[i]));
                    }
                    else
                    {
                        result += colNames[i];
                    }
                }

                if (i != colNames.Count - 1)
                {
                    result += " " + and_or + " ";
                }
            }
            if (result != "")
            {
                result = " where " + result;
            }
            return result;
        }

        internal string[] GetColumnNames(List<string> colNames, string preFlag) ////得到数组形式的@列名
        {
            if (colNames == null || colNames.Count <= 0)
            {
                return new string[0];
            }
            string[] colnames = new string[colNames.Count];
            for (int i = 0; i < colNames.Count; i++)
            {
                colnames[i] = preFlag + RemoveSpecialChar(colNames[i]);
            }
            return colnames;
        }

        internal static string RemoveSpecialChar(string text)    //处理 "小时数/天数"之类的列名
        {
            text = text.Replace(' ', '_').Replace('/', '_').Replace('\\', '_').Replace('(', '_').Replace(')', '_').
                Replace('[', '_').Replace(']', '_').Trim();//最好用正则表达式匹配
            return text;
        }

        internal static string GetTableNameFromSelectSql(string selectSql)
        {
            if (!string.IsNullOrEmpty(selectSql) && (selectSql.Trim().StartsWith("select ", StringComparison.OrdinalIgnoreCase) || selectSql.Trim().StartsWith("with ", StringComparison.OrdinalIgnoreCase)))
            {
                string fromKey = " from ";
                int index = selectSql.IndexOf(fromKey, StringComparison.OrdinalIgnoreCase);
                var tb = selectSql.Substring(index + fromKey.Length).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return tb;
            }
            return "";
        }
    }


}


