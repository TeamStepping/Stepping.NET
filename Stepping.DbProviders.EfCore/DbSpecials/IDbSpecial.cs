using Stepping.Core;

namespace Stepping.DbProviders.EfCore.DbSpecials;

public interface IDbSpecial
{
    string GetCreateBarrierTableSql(SteppingOptions options);

    string GetInsertIgnoreSqlTemplate(string? tableName);

    string GetQueryPreparedSql(string? tableName);
}