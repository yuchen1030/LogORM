using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class In : WordsBase
    {
        public string[] ColumnNames { get; private set; }
        private string _in = string.Empty;

        public In(params string[] columnNames)
        {
            this.ColumnNames = columnNames;

            _in = string.Format("[{0}] In(", ColumnNames.Length > 1?
                string.Format("({0}) ", string.Join("],[", ColumnNames)) : ColumnNames[0]);
        }

        public string ToString<T>(T value)
        {
            StringBuilder builder = new StringBuilder(_in);
            var items = typeof(T).GetProperties();
            int c = items.Length;

            foreach (var item in items)
            {
                var rVal = item.GetValue(value, null);
                if (rVal == null) continue;

                builder.AppendFormat("{0}{1}",
                    IsDigital(rVal) ? rVal : string.Format("'{0}'", rVal),
                   (--c) > 0 ? "," : "");
            }
            return builder.Append(") ").ToString();
        }

        public string ToString(object[] values)
        {
            StringBuilder builder = new StringBuilder(_in);
            int c = values.Length;

            foreach (var value in values)
            {
                builder.AppendFormat("{0}{1}",
                    IsDigital(value) ? value : string.Format("'{0}'", value),
                   (--c) > 0 ? "," : "");
            }
            return builder.Append(") ").ToString();
        }
    }
}
