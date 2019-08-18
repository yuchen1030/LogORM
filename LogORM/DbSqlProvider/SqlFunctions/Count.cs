using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlFunctions
{
    public class Count:FunctionsBase
    {
        public Count(string columnName="*")
            : base(columnName)
        {

        }

        public override string ToString()
        {
            return string.Format("Count({0}) ",
                string.IsNullOrEmpty(ColumnName) ? "*" : ColumnName);
        }
    }
}
