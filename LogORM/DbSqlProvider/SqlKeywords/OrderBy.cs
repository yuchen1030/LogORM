using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class OrderBy : WordsBase
    {
        public SortType SortType { get; private set; }

        public string[] ColumnNames { get; private set; }

        public OrderBy(SortType sortType,params string[] columnNames)
        {
            this.SortType = sortType;
            this.ColumnNames = columnNames;
        }

        public override string ToString()
        {
            return string.Format("Order By {0} {1} ", "["+string.Join("],[", ColumnNames)+"]", SortType.ToString());
        }
    }

    public enum SortType:byte
    {
       Asc,
       Desc
    }
}
