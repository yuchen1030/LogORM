using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.SqlExtensions
{
    using DbSqlProvider.SqlKeywords;

    public static class FromExtensions
    {
        #region select  
        public static From From(this Select select, string tableName)
        {
            From from = new From(tableName);
            from.SqlString = select.SqlString + from.ToString();
            return from;
        }

        public static From From<T>(this Select select)
        {
            From from = new From(string.Empty);
            from.SqlString = select.SqlString + from.ToString<T>();
            return from;
        }

        #endregion

        #region delete 
        public static From From(this Delete delete, string tableName)
        {
            From from = new From(tableName);
            from.SqlString = delete.SqlString + from.ToString();
            return from;
        }

        public static From From<T>(this Delete delete)
        {
            From from = new From(string.Empty);
            from.SqlString = delete.SqlString + from.ToString<T>();
            return from;
        }
        #endregion
    }
}
