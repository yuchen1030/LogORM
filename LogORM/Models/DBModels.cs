using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace LogORM.Models
{
    public class DBBasePara<T> where T : class
    {
        // public string DBType { get; set; }
        // public string TableName { get; set; }
    }


    //public class SelectSql
    //{
    //    public string Sql { get; set; }
    //    public DbParameter[] PMS { get; internal set; }
    //}


    public class CRUDSql
    {
        public string Sql { get; set; }
        public DbParameter[] PMS { get; set; }
    }


    public class SqlContianer
    {
        public string tableName = "";

        public string strSqlTxt = "";

        public List<DbParameter> ltOraParams = new List<DbParameter>();

        public int intExpectNums = 1;//若为负数，则表示可取正或可为0，为Int16.MinValue表示不检测数量
    }


    public class AddDBPara<T> : DBBasePara<T> where T : class
    {
        //public AddDBPara()
        //{
        //    SkipCols = new string[0];
        //}


        public T Model { get; set; }
        //  public string[] SkipCols { get; set; }

    }


    public class AddUpdateDelEdm
    {
        public AddUpdateDelEdm()
        {
            TableName = "";
            Datas = new List<object>();
        }
        public string TableName { get; set; }
        public string MainFields { get; set; } //当是更新时，需要该字段：主键字段 + 需要更新的字段。
        public List<object> Datas { get; set; }
        public List<Dictionary<string, string>> UpdateFD { get; set; }
    }


    [Serializable]
    public class SearchParam
    {
        public SearchParam()
        {
            PageIndex = 1;
            PageSize = 10;
            Orderby = "FBillNo";
        }
        public string TableName { get; set; }
        public string Orderby { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string StrWhere { get; set; }

        public int TotalCount { get; set; }
    }


    public class PageSerach<T> : BaseSerach<T>
    {
        public PageSerach()
        {
            PageIndex = 1;
            PageSize = 10;
        }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public class BaseSerach<T>
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        //lambda难以表达时直接使用的where语句。
        public string StrWhere { get; set; }
        //public Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; set; }//排序条件
        //public Func<IQueryable<T>, IQueryable<T>> OrderBy { get; set; }//排序条件
        public Expression<Func<IQueryable<T>, IQueryable<T>>> OrderBy { get; set; }

        //需要包含的导航属性
        public List<string> IncludeProps { get; set; }

    }


    //执行结果Model
    public class ExeResEdm
    {
        public int ErrCode { get; set; }  //错误码，0为成功，1为失败，>1为具体的错误代码
        public string Module { get; set; } //模块
        public Exception ExBody { get; set; } //异常Exception
        public object ExeModel { get; set; }  //执行结果
        public int ExeNum { get; set; } //影响的行数

        public string ErrMsg//错误信息
        {
            get
            {
                var detailMsg = "";
                if (ExBody != null)
                {
                    if (ExBody.InnerException != null)
                    {
                        detailMsg = ExBody.Message + ",InnerException=" + ExBody.InnerException.Message;
                    }
                    else
                    {
                        detailMsg = ExBody.Message;
                    }
                }
                var res = string.Join(" : ", new List<string>() { _errMsg, detailMsg }.Where(a => !string.IsNullOrEmpty(a)));
                return res;
            }
            set { _errMsg = value; }
        }
        string _errMsg = "";


    }



    /// <summary>
    /// 业务系统中数据库操作日志所用的实体
    /// </summary>
    public class DBOperUser
    {
        public string UserId { get { return _UserId; } set { _UserId = value; } }
        public string UserName { get { return _UserName; } set { _UserName = value; } }
        string _UserId = "系统";
        string _UserName = "系统";
    }


}
