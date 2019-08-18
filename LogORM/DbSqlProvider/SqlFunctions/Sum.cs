using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlFunctions
{
    public class Sum:FunctionsBase
    {
        public Sum(string columnName="*")
            :base(columnName)
        {
           
        }

        public override string ToString()
        {
            return string.Format("Sum({0}) ", string.IsNullOrEmpty(ColumnName) ? "*" : ColumnName);
        }
    }
}
