using Microsoft.EntityFrameworkCore.Query;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.Internal;

/// <summary>
/// The default factory for Kdbndp-specific query SQL generators.
/// </summary>
public class KdbndpQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly IKdbndpSingletonOptions _npgsqlSingletonOptions;

    public KdbndpQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        IKdbndpSingletonOptions npgsqlSingletonOptions)
    {
        _dependencies = dependencies;
        _npgsqlSingletonOptions = npgsqlSingletonOptions;
    }

    public virtual QuerySqlGenerator Create()
        => new KdbndpQuerySqlGenerator(
            _dependencies,
            _npgsqlSingletonOptions.ReverseNullOrderingEnabled,
            _npgsqlSingletonOptions.PostgresVersion);
}