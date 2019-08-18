namespace LogORM.DbSqlProvider
{
    using DbSqlProvider.SqlKeywords;
    using LogORM.Models;

    public interface ISqlProvider
    {
        DBStoreType ProviderType { get; set; }

        Select Select(params string[] columns);

        Select Select<T>() where T : class;

        Insert Insert(string tableName, string[] columnNames);

        Insert Insert<T>() where T : class;

        Update Update(string tableName);

        Update Update<T>() where T : class;

        Delete Delete();
    }
}
