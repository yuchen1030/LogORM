using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class Select:WordsBase
    {
        public string[] ColumnNames { get; private set; }

        public Select() { }

        public Select(string[] columnNames)
        {
            this.ColumnNames = columnNames;
        }
 
        public override string ToString()
        {
            return string.Format("Select {0} ", base.ToString(ColumnNames));
        }

        public virtual string ToString<T>()
        {
            ColumnNames = ToColumns<T>();

            return string.Format("Select {0} ", base.ToString(ColumnNames));
        }
    }
}
