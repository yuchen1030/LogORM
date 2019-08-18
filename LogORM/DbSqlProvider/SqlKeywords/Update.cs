using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class Update : WordsBase
    {
        public Update() { }

        public string ToString(string tableName)
        {
            return string.Format("Update [{0}] ", tableName);
        }

        public string ToString<T>()
        {
            return string.Format("Update [{0}] ", typeof(T).Name);
        }
    }
}
