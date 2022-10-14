using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Internal;

public static class KdbndpLoggerExtensions
{
    public static void MissingSchemaWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? schemaName)
    {
        var definition = KdbndpResources.LogMissingSchema(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, schemaName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void MissingTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? tableName)
    {
        var definition = KdbndpResources.LogMissingTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void ForeignKeyReferencesMissingPrincipalTableWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? foreignKeyName,
        string? tableName,
        string? principalTableName)
    {
        var definition = KdbndpResources.LogPrincipalTableNotInSelectionSet(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void ColumnFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string tableName,
        string columnName,
        string dataTypeName,
        bool nullable,
        bool identity,
        string? defaultValue,
        string? computedValue)
    {
        var definition = KdbndpResources.LogFoundColumn(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.LogDebug(
                    definition.EventId,
                    null,
                    definition.MessageFormat,
                    tableName,
                    columnName,
                    dataTypeName,
                    nullable,
                    identity,
                    defaultValue,
                    computedValue));
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void CollationFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string schema,
        string collationName,
        string lcCollate,
        string lcCtype,
        string? provider,
        bool deterministic)
    {
        var definition = KdbndpResources.LogFoundCollation(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, collationName, schema, lcCollate, lcCtype, provider, deterministic);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void UniqueConstraintFound(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? uniqueConstraintName,
        string tableName)
    {
        var definition = KdbndpResources.LogFoundUniqueConstraint(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, uniqueConstraintName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void EnumColumnSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string columnName)
    {
        var definition = KdbndpResources.LogEnumColumnSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, columnName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void ExpressionIndexSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string indexName,
        string tableName)
    {
        var definition = KdbndpResources.LogExpressionIndexSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void UnsupportedColumnIndexSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string indexName,
        string tableName)
    {
        var definition = KdbndpResources.LogUnsupportedColumnIndexSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }

    public static void UnsupportedColumnConstraintSkippedWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
        string? indexName,
        string tableName)
    {
        var definition = KdbndpResources.LogUnsupportedColumnConstraintSkipped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, indexName, tableName);
        }

        // No DiagnosticsSource events because these are purely design-time messages
    }
}