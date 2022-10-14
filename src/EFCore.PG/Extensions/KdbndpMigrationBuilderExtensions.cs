using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class KdbndpMigrationBuilderExtensions
{
    /// <summary>
    /// Returns true if the active provider in a migration is the Kdbndp provider.
    /// </summary>
    /// The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
    /// <see cref="Migration.Down(MigrationBuilder)" />.
    /// <returns>True if Kdbndp is being used; false otherwise.</returns>
    public static bool IsKdbndp(this MigrationBuilder builder)
        => builder.ActiveProvider == typeof(KdbndpMigrationBuilderExtensions).GetTypeInfo().Assembly.GetName().Name;

    public static MigrationBuilder EnsurePostgresExtension(
        this MigrationBuilder builder,
        string name,
        string? schema = null,
        string? version = null)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NullButNotEmpty(version, nameof(schema));

        var op = new AlterDatabaseOperation();
        op.GetOrAddPostgresExtension(schema, name, version);
        builder.Operations.Add(op);

        return builder;
    }

    [Obsolete("Use EnsurePostgresExtension instead")]
    public static MigrationBuilder CreatePostgresExtension(
        this MigrationBuilder builder,
        string name,
        string? schema = null,
        string? version = null)
        => EnsurePostgresExtension(builder, name, schema, version);

    [Obsolete("This no longer does anything and should be removed.")]
    public static MigrationBuilder DropPostgresExtension(
        this MigrationBuilder builder,
        string name)
        => builder;
}