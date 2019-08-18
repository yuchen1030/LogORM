using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlFunctions
{
    public class Max : FunctionsBase
    {
        public Max(string columnName)
            : base(columnName)
        {

        }

        public override string ToString()
        {
            return string.Format("Max({0}) ",ColumnName);
        }
    }
}
