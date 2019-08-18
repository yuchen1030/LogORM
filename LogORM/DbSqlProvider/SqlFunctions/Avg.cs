using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlFunctions
{
   public class Avg:FunctionsBase
    {
        public Avg(string columnName)
            :base(columnName)
        {
           
        }

        public override string ToString()
        {
            return string.Format("AVG({0}) ",ColumnName);
        }
    }
}
