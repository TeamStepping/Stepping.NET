using Stepping.Core;

namespace Stepping.DbProviders.EfCore.DbSpecials;

public class SqlServerDbSpecial : IDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "stepping.Barrier";

    public static string BarrierTableAndValueSqlFormat { get; set; } =
        "{0} (trans_type, gid, branch_id, op, barrier_id, reason) values (@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";

    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";

    public virtual string GetCreateBarrierTableSql(SteppingOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var split = configuredTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var schemaName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : configuredTableName;
        var tableFullName = schemaName is null ? tableName : $"{schemaName}.{tableName}";

        var sql = string.Empty;

        if (schemaName is not null)
        {
            sql += $@"
IF NOT EXISTS (SELECT
    *
  FROM sys.schemas
  WHERE name = '{schemaName}')
BEGIN
  EXEC ('CREATE SCHEMA [{schemaName}]');
END;
";
        }

        sql += $@"
IF OBJECT_ID(N'{tableFullName}', N'U') IS NULL
BEGIN
  CREATE TABLE {tableFullName} (
    [id] bigint NOT NULL IDENTITY (1, 1) PRIMARY KEY,
    [trans_type] varchar(45) NOT NULL DEFAULT (''),
    [gid] varchar(128) NOT NULL DEFAULT (''),
    [branch_id] varchar(128) NOT NULL DEFAULT (''),
    [op] varchar(45) NOT NULL DEFAULT (''),
    [barrier_id] varchar(45) NOT NULL DEFAULT (''),
    [reason] varchar(45) NOT NULL DEFAULT (''),
    [create_time] datetime NOT NULL DEFAULT (GETDATE()),
    [update_time] datetime NOT NULL DEFAULT (GETDATE())
  );

  CREATE UNIQUE INDEX [ix_uniq_barrier] ON {tableFullName}
  ([gid] ASC, [branch_id] ASC, [op] ASC, [barrier_id] ASC)
  WITH (IGNORE_DUP_KEY = ON);
END;
";
        return sql;
    }

    public virtual string GetInsertIgnoreSqlTemplate(string? tableName) =>
        $"insert into {string.Format(BarrierTableAndValueSqlFormat, tableName ?? DefaultBarrierTableName)}";

    public virtual string GetQueryPreparedSql(string? tableName) =>
        string.Format(QueryPreparedSqlFormat, tableName ?? DefaultBarrierTableName);
}