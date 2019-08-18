using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.SqlExtensions
{
    using DbSqlProvider.SqlKeywords;

    public static class SetExtensions
    {
        public static Set Set(this Update update, Dictionary<string, object> nameValues)
        {
            Set set = new Set();
            set.SqlString = update.SqlString + set.ToString(nameValues);
            return set;
        }

        public static Set Set<T>(this Update update, T value)
        {
            Set set = new Set();
            set.SqlString = update.SqlString + set.ToString<T>(value);
            return set;
        }
    }
}
