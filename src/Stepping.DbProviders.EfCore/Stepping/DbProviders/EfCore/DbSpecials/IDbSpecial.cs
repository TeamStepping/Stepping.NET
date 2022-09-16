using Stepping.Core;
using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore.DbSpecials;

public interface IDbSpecial
{
    string GetExistBarrierTableSql(SteppingOptions options);

    string GetCreateBarrierTableSql(SteppingOptions options);

    string GetInsertIgnoreSqlTemplate(string? tableName);

    string GetQueryPreparedSql(string? tableName);
}