using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class Set : WordsBase
    {
        public Set() { }

        public string ToString(Dictionary<string, object> NameValues)
        {
            StringBuilder builder = new StringBuilder("Set ");
            int c = NameValues.Count;

            foreach (var item in NameValues)
            {
                builder.AppendFormat("[{0}]={1}{2}", item.Key,
                    IsDigital(item.Value) ? item.Value : string.Format("'{0}'", item.Value),
                   (--c) > 0 ? "," : "");
            }
            return builder.Append(" ").ToString();
        }

        public string ToString<T>(T value)
        {
            StringBuilder builder = new StringBuilder("Set ");
            var items = typeof(T).GetProperties();
            int c = items.Length;

            foreach (var item in items)
            {
                var rVal = item.GetValue(value, null);
                if (rVal == null) continue;

                builder.AppendFormat("[{0}]={1}{2}",item.Name,
                    IsDigital(rVal) ? rVal : string.Format("'{0}'", rVal),
                   (--c) > 0 ? "," : "");
            }
            return builder.Append(" ").ToString();
        }
    }
}
