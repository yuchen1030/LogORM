using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogORM.DbSqlProvider
{
    using DbSqlProvider.SqlKeywords;
    using LogORM.Models;
  

    public class SqlProvider : ISqlProvider
    {
       public DBStoreType ProviderType { get;  set; }

        public SqlProvider(DBStoreType providerType=DBStoreType.SqlServer)
        {
            this.ProviderType = providerType;
        }

        public static ISqlProvider CreateProvider(DBStoreType providerType = DBStoreType.SqlServer)
        {
            return new SqlProvider(providerType);
        }

        public Select Select(params string[] columns)
        {
            Select _select = new Select(columns);
            _select.SqlString = _select.ToString();
            return _select;
        }

        public Select Select<T>() where T : class
        {
            Select _select = new Select();
            _select.SqlString = _select.ToString<T>();
            return _select;
        }

        public Insert Insert<T>() where T : class
        {
            Insert  insert = new Insert(string.Empty);
            insert.SqlString = insert.ToString<T>();

            return insert;
        }

        public Insert Insert(string tableName,string[] columnNames)
        {
            Insert insert = new Insert(tableName);
            insert.SqlString = insert.ToString(columnNames);

            return insert;
        }

        public Update Update(string tableName)
        {
            Update up = new Update();
            up.SqlString = up.ToString(tableName);
            return up;
        }

        public Update Update<T>() where T:class
        {
            Update up = new Update();
            up.SqlString = up.ToString<T>();
            return up;
        }

        public Delete Delete()
        {
            Delete delete = new Delete();
            delete.SqlString = delete.ToString();

            return delete;
        }
    }
}
