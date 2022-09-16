using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore.DbSpecials;

public class MySqlDbSpecial : IDbSpecial
{
    public static string DefaultBarrierTableName { get; set; } = "stepping_barrier";

    public static string BarrierTableAndValueSqlFormat { get; set; } =
        "{0}(trans_type, gid, branch_id, op, barrier_id, reason) values(@trans_type,@gid,@branch_id,@op,@barrier_id,@reason)";

    public static string QueryPreparedSqlFormat { get; set; } =
        "select reason from {0} where gid=@gid and branch_id=@branch_id and op=@op and barrier_id=@barrier_id";

    public string GetExistBarrierTableSql(SteppingOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var split = configuredTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var databaseName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : configuredTableName;

        return $@"
SELECT count(*) FROM information_schema.TABLES WHERE (TABLE_SCHEMA = '{databaseName}') AND (TABLE_NAME = '{tableName}');
";
    }

    public virtual string GetCreateBarrierTableSql(SteppingOptions options)
    {
        var configuredTableName = options.BarrierTableName ?? DefaultBarrierTableName;
        var split = configuredTableName.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);

        var databaseName = split.Length == 2 ? split[0] : null;
        var tableName = split.Length == 2 ? split[1] : configuredTableName;
        var tableFullName = databaseName is null ? tableName : $"{databaseName}.{tableName}";

        var sql = string.Empty;

        if (databaseName is not null)
        {
            sql += $@"
create database if not exists {databaseName}
/*!40100 DEFAULT CHARACTER SET utf8mb4 */
;
";
        }

        sql += $@"
create table if not exists {tableFullName}(
  id bigint(22) PRIMARY KEY AUTO_INCREMENT,
  trans_type varchar(45) default '',
  gid varchar(128) default '',
  branch_id varchar(128) default '',
  op varchar(45) default '',
  barrier_id varchar(45) default '',
  reason varchar(45) default '' comment 'the branch type who insert this record',
  create_time datetime DEFAULT now(),
  update_time datetime DEFAULT now(),
  key(create_time),
  key(update_time),
  UNIQUE key(gid, branch_id, op, barrier_id)
);
";
        return sql;
    }

    public virtual string GetInsertIgnoreSqlTemplate(string? tableName) =>
        $"insert ignore into {string.Format(BarrierTableAndValueSqlFormat, tableName ?? DefaultBarrierTableName)}";

    public virtual string GetQueryPreparedSql(string? tableName) =>
        string.Format(QueryPreparedSqlFormat, tableName ?? DefaultBarrierTableName);
}