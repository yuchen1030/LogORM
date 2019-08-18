using Log2Net.Models;
using LogORM;
using LogORM.Models;
using System.Collections.Generic;

namespace LogORMWeb_DNC.Dals
{

    class Log_OperateTraceAdoDal : LogORMBaseDal<Log_OperateTrace>
    {

        protected override CurrentDalParas CurDalParas
        {
            get
            {
                return new CurrentDalParas()
                {
                    CurDatabaseType = DBStoreType.SqlServer,
                    DBConStringKey = "logTraceSqlStr",
                    TableName = "Log_OperateTrace",
                    PrimaryKey = "Id",
                    SkipCols = new string[] { "Id" },
                    DeleteKeys = new List<string>() { "Id" },
                    Orderby = "Id",
                };
            }
        }


    }



    class Log_SystemMonitorAdoDal : LogORMBaseDal<Log_SystemMonitor>
    {

        protected override CurrentDalParas CurDalParas
        {
            get
            {
                return new CurrentDalParas()
                {
                    CurDatabaseType = DBStoreType.SqlServer,
                    TableName = "Log_SystemMonitor",
                    PrimaryKey = "Id",
                    SkipCols = new string[] { "Id" },
                    DeleteKeys = new List<string>() { "Id" },
                    Orderby = "Id",
                };
            }
        }


    }


}
