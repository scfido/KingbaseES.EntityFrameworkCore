using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Internal;

/// <inheritdoc />
public class KdbndpSingletonOptions : IKdbndpSingletonOptions
{
    public static readonly Version DefaultPostgresVersion = new(12, 0);

    /// <inheritdoc />
    public virtual Version PostgresVersion { get; private set; } = null!;

    /// <inheritdoc />
    public virtual Version? PostgresVersionWithoutDefault { get; private set; }

    /// <inheritdoc />
    public virtual bool UseRedshift { get; private set; }

    /// <inheritdoc />
    public virtual bool ReverseNullOrderingEnabled { get; private set; }

    /// <inheritdoc />
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions { get; private set; }

    public KdbndpSingletonOptions()
        => UserRangeDefinitions = new UserRangeDefinition[0];

    /// <inheritdoc />
    public virtual void Initialize(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();

        PostgresVersionWithoutDefault = npgsqlOptions.PostgresVersion;
        PostgresVersion = npgsqlOptions.PostgresVersion ?? DefaultPostgresVersion;
        UseRedshift = npgsqlOptions.UseRedshift;
        ReverseNullOrderingEnabled = npgsqlOptions.ReverseNullOrdering;
        UserRangeDefinitions = npgsqlOptions.UserRangeDefinitions;
    }

    /// <inheritdoc />
    public virtual void Validate(IDbContextOptions options)
    {
        var npgsqlOptions = options.FindExtension<KdbndpOptionsExtension>() ?? new KdbndpOptionsExtension();

        if (PostgresVersionWithoutDefault != npgsqlOptions.PostgresVersion)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.SetPostgresVersion),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UseRedshift != npgsqlOptions.UseRedshift)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.UseRedshift),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (ReverseNullOrderingEnabled != npgsqlOptions.ReverseNullOrdering)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.ReverseNullOrdering),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }

        if (UserRangeDefinitions.Count != npgsqlOptions.UserRangeDefinitions.Count
            || UserRangeDefinitions.Zip(npgsqlOptions.UserRangeDefinitions).Any(t => t.First != t.Second))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(KdbndpDbContextOptionsBuilder.MapRange),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}