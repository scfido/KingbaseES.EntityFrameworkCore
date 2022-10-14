using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Kdbndp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Migrations.Operations;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal;

public class KdbndpDatabaseCreator : RelationalDatabaseCreator
{
    private readonly IKdbndpRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

    public KdbndpDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        IKdbndpRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder)
        : base(dependencies)
    {
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
    }

    public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

    public override void Create()
    {
        using (var masterConnection = _connection.CreateMasterConnection())
        {
            try
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);
            }
            catch (KingbaseException e) when (
                e.SqlState == "23505" && e.ConstraintName == "pg_database_datname_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            ClearPool();
        }

        Exists();
    }

    public override async Task CreateAsync(CancellationToken cancellationToken = default)
    {
        using (var masterConnection = _connection.CreateMasterConnection())
        {
            try
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (KingbaseException e) when (
                e.SqlState == "23505" && e.ConstraintName == "pg_database_datname_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

            ClearPool();
        }

        await ExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    public override bool HasTables()
        => Dependencies.ExecutionStrategy
            .Execute(
                _connection,
                connection => (bool)CreateHasTablesCommand()
                    .ExecuteScalar(
                        new RelationalCommandParameterObject(
                            connection,
                            null,
                            null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.CommandLogger))!);

    public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => Dependencies.ExecutionStrategy.ExecuteAsync(
            _connection,
            async (connection, ct) => (bool)(await CreateHasTablesCommand()
                .ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger),
                    cancellationToken: ct).ConfigureAwait(false))!, cancellationToken);

    private IRelationalCommand CreateHasTablesCommand()
        => _rawSqlCommandBuilder
            .Build(@"
SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
FROM pg_class AS cls
JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
WHERE
        cls.relkind IN ('r', 'v', 'm', 'f', 'p') AND
        ns.nspname NOT IN ('pg_catalog', 'information_schema') AND
        -- Exclude tables which are members of PG extensions
        NOT EXISTS (
            SELECT 1 FROM pg_depend WHERE
                classid=(
                    SELECT cls.oid
                    FROM pg_class AS cls
                             JOIN pg_namespace AS ns ON ns.oid = cls.relnamespace
                    WHERE relname='pg_class' AND ns.nspname='pg_catalog'
                ) AND
                objid=cls.oid AND
                deptype IN ('e', 'x')
        )");

    private IReadOnlyList<MigrationCommand> CreateCreateOperations()
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;

        return Dependencies.MigrationsSqlGenerator.Generate(
            new[]
            {
                new KdbndpCreateDatabaseOperation
                {
                    Name = _connection.DbConnection.Database,
                    Template = designTimeModel.GetDatabaseTemplate(),
                    Collation = designTimeModel.GetCollation(),
                    Tablespace = designTimeModel.GetTablespace()
                }
            });
    }

    public override bool Exists()
        => Exists(async: false).GetAwaiter().GetResult();

    public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => Exists(async: true, cancellationToken);

    private async Task<bool> Exists(bool async, CancellationToken cancellationToken = default)
    {
        // When checking whether a database exists, pooling must be off, otherwise we may
        // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
        // If Pooling is off, but Multiplexing is on - KdbndpConnectionStringBuilder.Validate will throw,
        // so we turn off Multiplexing as well.
        var unpooledCsb = new KdbndpConnectionStringBuilder(_connection.ConnectionString)
        {
            Pooling = false,
            //Multiplexing = false
        };

        using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        var unpooledRelationalConnection = _connection.CloneWith(unpooledCsb.ToString());
        try
        {
            if (async)
            {
                await unpooledRelationalConnection.OpenAsync(errorsExpected: true, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                unpooledRelationalConnection.Open(errorsExpected: true);
            }

            return true;
        }
        catch (KingbaseException e)
        {
            if (IsDoesNotExist(e))
            {
                return false;
            }

            throw;
        }
        catch (KdbndpException e) when (
            e.InnerException is IOException &&
            e.InnerException.InnerException is SocketException socketException &&
            socketException.SocketErrorCode == SocketError.ConnectionReset
        )
        {
            // Pretty awful hack around #104
            return false;
        }
        finally
        {
            if (async)
            {
                await unpooledRelationalConnection.CloseAsync().ConfigureAwait(false);
                await unpooledRelationalConnection.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                unpooledRelationalConnection.Close();
                unpooledRelationalConnection.Dispose();
            }
        }
    }

    // Login failed is thrown when database does not exist (See Issue #776)
    private static bool IsDoesNotExist(KingbaseException exception) => exception.SqlState == "3D000";

    public override void Delete()
    {
        ClearAllPools();

        using (var masterConnection = _connection.CreateMasterConnection())
        {
            Dependencies.MigrationCommandExecutor
                .ExecuteNonQuery(CreateDropCommands(), masterConnection);
        }
    }

    public override async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        ClearAllPools();

        using (var masterConnection = _connection.CreateMasterConnection())
        {
            await Dependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public override void CreateTables()
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;
        var operations = Dependencies.ModelDiffer.GetDifferences(null, designTimeModel.GetRelationalModel());
        var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, designTimeModel);

        // If a KingbaseES extension, enum or range was added, we want Kdbndp to reload all types at the ADO.NET level.
        var reloadTypes =
            operations.OfType<AlterDatabaseOperation>()
                .Any(o =>
                    o.GetPostgresExtensions().Any() ||
                    o.GetPostgresEnums().Any() ||
                    o.GetPostgresRanges().Any());

        try
        {
            Dependencies.MigrationCommandExecutor.ExecuteNonQuery(commands, _connection);
        }
        catch (KingbaseException e) when (
            e.SqlState == "23505" && e.ConstraintName == "pg_type_typname_nsp_index"
        )
        {
            // This occurs when two connections are trying to create the same database concurrently
            // (happens in the tests). Simply ignore the error.
        }

        if (reloadTypes)
        {
            _connection.Open();
            try
            {
                ((KdbndpConnection)_connection.DbConnection).ReloadTypes();
            }
            catch
            {
                _connection.Close();
            }
        }
    }

    public override async Task CreateTablesAsync(CancellationToken cancellationToken = default)
    {
        var designTimeModel = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;
        var operations = Dependencies.ModelDiffer.GetDifferences(null, designTimeModel.GetRelationalModel());
        var commands = Dependencies.MigrationsSqlGenerator.Generate(operations, designTimeModel);

        // If a KingbaseES extension, enum or range was added, we want Kdbndp to reload all types at the ADO.NET level.
        var reloadTypes =
            operations.OfType<AlterDatabaseOperation>()
                .Any(o =>
                    o.GetPostgresExtensions().Any() ||
                    o.GetPostgresEnums().Any() ||
                    o.GetPostgresRanges().Any());

        try
        {
            await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(commands, _connection, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (KingbaseException e) when (
            e.SqlState == "23505" && e.ConstraintName == "pg_type_typname_nsp_index"
        )
        {
            // This occurs when two connections are trying to create the same database concurrently
            // (happens in the tests). Simply ignore the error.
        }

        if (reloadTypes)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // TODO: Not async
                ((KdbndpConnection)_connection.DbConnection).ReloadTypes();
            }
            catch
            {
                _connection.Close();
            }
        }
    }

    private IReadOnlyList<MigrationCommand> CreateDropCommands()
    {
        var operations = new MigrationOperation[]
        {
            // TODO Check DbConnection.Database always gives us what we want
            // Issue #775
            new KdbndpDropDatabaseOperation { Name = _connection.DbConnection.Database }
        };

        return Dependencies.MigrationsSqlGenerator.Generate(operations);
    }

    // Clear connection pools in case there are active connections that are pooled
    private static void ClearAllPools() => KdbndpConnection.ClearAllPools();

    // Clear connection pool for the database connection since after the 'create database' call, a previously
    // invalid connection may now be valid.
    private void ClearPool() => KdbndpConnection.ClearPool((KdbndpConnection)_connection.DbConnection);
}
