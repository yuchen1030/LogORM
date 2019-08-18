using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class Values:WordsBase
    {
        public Values( )
        {
            
        }

        public string ToString(IList<object[]> value)
        {
            StringBuilder builder = new StringBuilder("Values ");
            for (int i = 0; i < value.Count; ++i)
            {
                builder.AppendFormat("({0}){1}",
                    base.ParameterFormat(value[i]), i < value.Count - 1 ? "," : "");
            }
            return builder.Append(" ").ToString();
        }

        public string ToString<T>(params T[] value)
        {
            StringBuilder builder = new StringBuilder("Values ");
            var pros = typeof(T).GetProperties(BindingFlags.Public
                | BindingFlags.Instance);

            for (int i = 0; i < value.Length; ++i)
            {
                builder.AppendFormat("({0}){1}",
                    base.ParameterFormat<T>(pros,value[i]), i < value.Length - 1 ? "," : "");
            }
            return builder.Append(" ").ToString();
        }
    }
}
