using Log2Net.Util;
using LogORM.ComUtil;
using LogORM.Models;

namespace LogORM.AdoNet.Oracle
{

    //oracle 11g 以前的版本的用户名和密码是不区分大小写的。oracle 11g 用户名和密码默认区分大小写。
    class OracleHelperFactory<T> where T : class
    {
        //string connstr = "";
        //public OracleHelperFactory(string connstr)
        //{
        //    this.connstr = connstr;
        //}
        static object lockObj = new object();
        static OracleHelperBase<T> OracleHelper = null;
        public static OracleHelperBase<T> GetInstance(string connstr)
        {
            if (OracleHelper == null)
            {
                lock (lockObj)
                {
                    if (OracleHelper == null)
                    {
#if NET
                        var orclHelperType = AppConfig.GetConfigValue("OracleDriverType");
                        if (orclHelperType ==  "1")   //微软驱动
                        {
                            OracleHelper = new OracleHelperMS<T>(connstr);
                        }
                        else
#endif
                        {
                            OracleHelper = new OracleHelper<T>(connstr);
                        }
                    }
                }
            }
            return OracleHelper;
        }
    }


    internal abstract class OracleHelperBase<T> : AdoNetBase<T>, IAdoNetBase<T> where T : class
    {

        readonly DBBaseAttr dbBaseAttr = new DBBaseAttr() { DBStoreType = DBStoreType.SqlServer, LeftPre = "", ParaPreChar = ":", RightSuf = "" };

        protected override DBBaseAttr DBBaseAttr { get { return dbBaseAttr; } }

        internal OracleHelperBase(string strConnStr) : base(strConnStr)
        {
            connstr = strConnStr;
        }

        protected override string GetColumnsNameSql(string strTbName, string strField = "*")
        {
            string strSqlTxt = "select  " + strField + " from " + strTbName.Trim().Trim(',') + " where rownum = 0";
            return strSqlTxt;
        }


    }


}
