using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class GroupBy : WordsBase
    {
        public string[] ColumnNames { get; private set; }

        public GroupBy(params string[] columnNames)
        {
            this.ColumnNames = columnNames;
        }

        public override string ToString()
        {
            return string.Format("Group By [{0}] ", string.Join("],[", ColumnNames));
        }
    }
}
