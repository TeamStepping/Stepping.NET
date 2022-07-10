using Stepping.Core;
using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore.DbSpecials;

public class SqLiteDbSpecial : IDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "stepping_barrier";

    public static string BarrierTableAndValueSqlFormat { get; set; } =
        "{0} (trans_type, gid, branch_id, op, barrier_id, reason) values (@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";

    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";

    public string Name { get; } = "sqlite";

    public virtual string GetCreateBarrierTableSql(SteppingOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var tableName = configuredTableName;

        var sql = string.Empty;

        sql += $@"
CREATE TABLE IF NOT EXISTS {tableName} (
  [id] INTEGER PRIMARY KEY AUTOINCREMENT,
  [trans_type] varchar(45) NOT NULL DEFAULT (''),
  [gid] varchar(128) NOT NULL DEFAULT (''),
  [branch_id] varchar(128) NOT NULL DEFAULT (''),
  [op] varchar(45) NOT NULL DEFAULT (''),
  [barrier_id] varchar(45) NOT NULL DEFAULT (''),
  [reason] varchar(45) NOT NULL DEFAULT (''),
  [create_time] datetime NOT NULL DEFAULT (datetime(CURRENT_TIMESTAMP)),
  [update_time] datetime NOT NULL DEFAULT (datetime(CURRENT_TIMESTAMP))
);

CREATE UNIQUE INDEX IF NOT EXISTS [ix_uniq_barrier] ON {tableName}
([gid] ASC, [branch_id] ASC, [op] ASC, [barrier_id] ASC);
";
        return sql;
    }

    public virtual string GetInsertIgnoreSqlTemplate(string? tableName) =>
        $"insert or ignore into {string.Format(BarrierTableAndValueSqlFormat, tableName ?? DefaultBarrierTableName)}";

    public virtual string GetQueryPreparedSql(string? tableName) =>
        string.Format(QueryPreparedSqlFormat, tableName ?? DefaultBarrierTableName);
}