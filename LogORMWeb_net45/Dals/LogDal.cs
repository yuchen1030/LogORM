using Log2Net.Models;
using LogORM;
using LogORM.Models;

namespace LogORMWeb_net45.Dals
{




    public class Log_OperateTraceAdoDal : LogORMBaseDal<Log_OperateTrace>
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
                    Orderby = "Id",
                };
            }
        }

    }



}