using System;
using System.Linq.Expressions;

namespace LogORM.SqlExtensions
{
    using DbSqlProvider.SqlKeywords;

    public static class WhereExtensions
    {
        public static Where Where<T>(this From from, Expression<Func<T, bool>> expression)
        {
            Where where = new Where();
            where.SqlString = from.SqlString + where.ToString<T>(expression);
            return where;
        }

        public static Where Where<T>(this Set set, Expression<Func<T, bool>> expression)
        {
            Where where = new Where();
            where.SqlString = set.SqlString + where.ToString<T>(expression);
            return where;
        }
    }
}
