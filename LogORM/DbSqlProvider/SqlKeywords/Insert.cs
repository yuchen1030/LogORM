using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
   public class Insert:WordsBase
    {
        public string TableName { get; private set; }

        public Insert(string tableName)
        {
            this.TableName = tableName;
        }

        public override string ToString(string[] columnNames)
        {
            return string.Format("Insert Into [{0}]({1}) ", TableName, base.ToString(columnNames));
        }

        public string ToString<T>()
        {
           string[] ColumnNames = ToColumns<T>();

            return string.Format("Insert Into [{0}]({1}) ", typeof(T).Name, base.ToString(ColumnNames));
        }
    }
}
