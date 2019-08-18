using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.Models
{
    /// <summary>
    ///数据库类型
    /// </summary>
    public enum DBStoreType
    {
        NoSelect = 0,
        SqlServer = 0x01,//realize by zz on 20190407
        Oracle = 0x02, //realize by zz on 20190407
        MySql = 0x04,//realize by zz on 20190407
        Access = 0x08,//realize by xxx on yyyyMMdd
        PostgreSQL = 0x10,//realize by xxx on yyyyMMdd
        SQLite = 0x20,//realize by xxx on yyyyMMdd
        DB2 = 0x40,//realize by xxx on yyyyMMdd
        OleDb = 0x80,//realize by xxx on yyyyMMdd
    }


}
