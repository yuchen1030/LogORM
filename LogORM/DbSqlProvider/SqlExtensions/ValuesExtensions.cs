using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.SqlExtensions
{
    using DbSqlProvider.SqlKeywords;

    public static class ValuesExtensions
    {
        public static Values Values(this Insert insert, IList<object[]> values)
        {
            Values val = new Values();
            val.SqlString = insert.SqlString + val.ToString(values);

            return val;
        }

        public static Values Values<T>(this Insert insert,T[] values)
        {
            Values val = new Values();
            val.SqlString = insert.SqlString + val.ToString(values);

            return val;
        }
    }
}
