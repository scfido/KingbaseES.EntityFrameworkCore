using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;

/// <summary>
/// Represents options managed by the Kdbndp.
/// </summary>
public class KdbndpOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    private readonly List<UserRangeDefinition> _userRangeDefinitions;

    /// <summary>
    /// The name of the database for administrative operations.
    /// </summary>
    public virtual string? AdminDatabase { get; private set; }

    /// <summary>
    /// The backend version to target.
    /// </summary>
    public virtual Version? PostgresVersion { get; private set; }

    /// <summary>
    /// Whether to target Redshift.
    /// </summary>
    public virtual bool UseRedshift { get; private set; }

    /// <summary>
    /// The list of range mappings specified by the user.
    /// </summary>
    public virtual IReadOnlyList<UserRangeDefinition> UserRangeDefinitions => _userRangeDefinitions;

    /// <summary>
    /// The specified <see cref="ProvideClientCertificatesCallback"/>.
    /// </summary>
    public virtual ProvideClientCertificatesCallback? ProvideClientCertificatesCallback { get; private set; }

    /// <summary>
    /// The specified <see cref="RemoteCertificateValidationCallback"/>.
    /// </summary>
    public virtual RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; private set; }

    ///// <summary>
    ///// The specified <see cref="ProvidePasswordCallback"/>.
    ///// </summary>
    //public virtual ProvidePasswordCallback? ProvidePasswordCallback { get; private set; }

    /// <summary>
    /// True if reverse null ordering is enabled; otherwise, false.
    /// </summary>
    public virtual bool ReverseNullOrdering { get; private set; }

    /// <summary>
    /// Initializes an instance of <see cref="KdbndpOptionsExtension"/> with the default settings.
    /// </summary>
    public KdbndpOptionsExtension()
        => _userRangeDefinitions = new List<UserRangeDefinition>();

    // NB: When adding new options, make sure to update the copy ctor below.
    /// <summary>
    /// Initializes an instance of <see cref="KdbndpOptionsExtension"/> by copying the specified instance.
    /// </summary>
    /// <param name="copyFrom">The instance to copy.</param>
    public KdbndpOptionsExtension(KdbndpOptionsExtension copyFrom) : base(copyFrom)
    {
        AdminDatabase = copyFrom.AdminDatabase;
        PostgresVersion = copyFrom.PostgresVersion;
        UseRedshift = copyFrom.UseRedshift;
        _userRangeDefinitions = new List<UserRangeDefinition>(copyFrom._userRangeDefinitions);
        ProvideClientCertificatesCallback = copyFrom.ProvideClientCertificatesCallback;
        RemoteCertificateValidationCallback = copyFrom.RemoteCertificateValidationCallback;
        //ProvidePasswordCallback = copyFrom.ProvidePasswordCallback;
        ReverseNullOrdering = copyFrom.ReverseNullOrdering;
    }

    // The following is a hack to set the default minimum batch size to 2 in Kdbndp
    // See https://github.com/aspnet/EntityFrameworkCore/pull/10091
    public override int? MinBatchSize => base.MinBatchSize ?? 2;

    /// <summary>
    /// Returns a copy of the current instance configured with the specified range mapping.
    /// </summary>
    public virtual KdbndpOptionsExtension WithUserRangeDefinition<TSubtype>(
        string rangeName,
        string? schemaName = null,
        string? subtypeName = null)
        => WithUserRangeDefinition(rangeName, schemaName, typeof(TSubtype), subtypeName);

    /// <summary>
    /// Returns a copy of the current instance configured with the specified range mapping.
    /// </summary>
    public virtual KdbndpOptionsExtension WithUserRangeDefinition(
        string rangeName,
        string? schemaName,
        Type subtypeClrType,
        string? subtypeName)
    {
        Check.NotEmpty(rangeName, nameof(rangeName));
        Check.NotNull(subtypeClrType, nameof(subtypeClrType));

        var clone = (KdbndpOptionsExtension)Clone();

        clone._userRangeDefinitions.Add(new UserRangeDefinition(rangeName, schemaName, subtypeClrType, subtypeName));

        return clone;
    }

    /// <summary>
    /// Returns a copy of the current instance configured to use the specified administrative database.
    /// </summary>
    /// <param name="adminDatabase">The name of the database for administrative operations.</param>
    public virtual KdbndpOptionsExtension WithAdminDatabase(string? adminDatabase)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.AdminDatabase = adminDatabase;

        return clone;
    }

    /// <summary>
    /// Returns a copy of the current instance with the specified KingbaseES version.
    /// </summary>
    /// <param name="postgresVersion">The backend version to target.</param>
    /// <returns>
    /// A copy of the current instance with the specified KingbaseES version.
    /// </returns>
    public virtual KdbndpOptionsExtension WithPostgresVersion(Version? postgresVersion)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.PostgresVersion = postgresVersion;

        return clone;
    }

    /// <summary>
    /// Returns a copy of the current instance with the specified Redshift settings.
    /// </summary>
    /// <param name="useRedshift">Whether to target Redshift.</param>
    /// <returns>
    /// A copy of the current instance with the specified Redshift setting.
    /// </returns>
    public virtual KdbndpOptionsExtension WithRedshift(bool useRedshift)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.UseRedshift = useRedshift;

        return clone;
    }

    /// <summary>
    /// Returns a copy of the current instance configured with the specified value..
    /// </summary>
    /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
    internal virtual KdbndpOptionsExtension WithReverseNullOrdering(bool reverseNullOrdering)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.ReverseNullOrdering = reverseNullOrdering;

        return clone;
    }

    /// <inheritdoc />
    public override void Validate(IDbContextOptions options)
    {
        base.Validate(options);

        if (UseRedshift && PostgresVersion is not null)
        {
            throw new InvalidOperationException($"{nameof(UseRedshift)} and {nameof(PostgresVersion)} cannot both be set");
        }
    }

    #region Authentication

    /// <summary>
    /// Returns a copy of the current instance with the specified <see cref="ProvideClientCertificatesCallback"/>.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    public virtual KdbndpOptionsExtension WithProvideClientCertificatesCallback(ProvideClientCertificatesCallback? callback)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.ProvideClientCertificatesCallback = callback;

        return clone;
    }

    /// <summary>
    /// Returns a copy of the current instance with the specified <see cref="RemoteCertificateValidationCallback"/>.
    /// </summary>
    /// <param name="callback">The specified callback.</param>
    public virtual KdbndpOptionsExtension WithRemoteCertificateValidationCallback(RemoteCertificateValidationCallback? callback)
    {
        var clone = (KdbndpOptionsExtension)Clone();

        clone.RemoteCertificateValidationCallback = callback;

        return clone;
    }

    ///// <summary>
    ///// Returns a copy of the current instance with the specified <see cref="ProvidePasswordCallback"/>.
    ///// </summary>
    ///// <param name="callback">The specified callback.</param>
    //public virtual KdbndpOptionsExtension WithProvidePasswordCallback(ProvidePasswordCallback? callback)
    //{
    //    var clone = (KdbndpOptionsExtension)Clone();

    //    clone.ProvidePasswordCallback = callback;

    //    return clone;
    //}

    #endregion Authentication

    #region Infrastructure

    /// <inheritdoc />
    protected override RelationalOptionsExtension Clone() => new KdbndpOptionsExtension(this);

    /// <inheritdoc />
    public override void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkKdbndp();

    /// <inheritdoc />
    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        private int? _serviceProviderHash;
        private string? _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new KdbndpOptionsExtension Extension => (KdbndpOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => true;

        public override string LogFragment
        {
            get
            {
                if (_logFragment is not null)
                {
                    return _logFragment;
                }

                var builder = new StringBuilder(base.LogFragment);

                if (Extension.AdminDatabase is not null)
                {
                    builder.Append(nameof(Extension.AdminDatabase)).Append("=").Append(Extension.AdminDatabase).Append(' ');
                }

                if (Extension.PostgresVersion is not null)
                {
                    builder.Append(nameof(Extension.PostgresVersion)).Append("=").Append(Extension.PostgresVersion).Append(' ');
                }

                if (Extension.UseRedshift)
                {
                    builder.Append(nameof(Extension.UseRedshift)).Append(' ');
                }

                if (Extension.ProvideClientCertificatesCallback is not null)
                {
                    builder.Append(nameof(Extension.ProvideClientCertificatesCallback)).Append(" ");
                }

                if (Extension.RemoteCertificateValidationCallback is not null)
                {
                    builder.Append(nameof(Extension.RemoteCertificateValidationCallback)).Append(" ");
                }

                //if (Extension.ProvidePasswordCallback is not null)
                //{
                //    builder.Append(nameof(Extension.ProvidePasswordCallback)).Append(" ");
                //}

                if (Extension.ReverseNullOrdering)
                {
                    builder.Append(nameof(Extension.ReverseNullOrdering)).Append(" ");;
                }

                if (Extension.UserRangeDefinitions.Count > 0)
                {
                    builder.Append(nameof(Extension.UserRangeDefinitions)).Append("=[");
                    foreach (var item in Extension.UserRangeDefinitions)
                    {
                        builder.Append(item.SubtypeClrType).Append("=>");

                        if (item.SchemaName is not null)
                        {
                            builder.Append(item.SchemaName).Append(".");
                        }

                        builder.Append(item.RangeName);

                        if (item.SubtypeName is not null)
                        {
                            builder.Append("(").Append(item.SubtypeName).Append(")");
                        }

                        builder.Append(";");
                    }

                    builder.Length = builder.Length -1;
                    builder.Append("] ");
                }

                return _logFragment = builder.ToString();
            }
        }

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash is null)
            {
                var hashCode = new HashCode();

                foreach (var userRangeDefinition in Extension._userRangeDefinitions)
                {
                    hashCode.Add(userRangeDefinition);
                }

                hashCode.Add(Extension.AdminDatabase);
                hashCode.Add(Extension.PostgresVersion);
                hashCode.Add(Extension.UseRedshift);
                hashCode.Add(Extension.ProvideClientCertificatesCallback);
                hashCode.Add(Extension.RemoteCertificateValidationCallback);
                //hashCode.Add(Extension.ProvidePasswordCallback);
                hashCode.Add(Extension.ReverseNullOrdering);

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }

        /// <inheritdoc />
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.UseAdminDatabase)]
                = (Extension.AdminDatabase?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.SetPostgresVersion)]
                = (Extension.PostgresVersion?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.UseRedshift)]
                = Extension.UseRedshift.GetHashCode().ToString(CultureInfo.InvariantCulture);

            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.ReverseNullOrdering)]
                = Extension.ReverseNullOrdering.GetHashCode().ToString(CultureInfo.InvariantCulture);

            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.RemoteCertificateValidationCallback)]
                = (Extension.RemoteCertificateValidationCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.ProvideClientCertificatesCallback)]
                = (Extension.ProvideClientCertificatesCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            //debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.ProvidePasswordCallback)]
            //    = (Extension.ProvidePasswordCallback?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);

            foreach (var rangeDefinition in Extension._userRangeDefinitions)
            {
                debugInfo["Kdbndp.EntityFrameworkCore.KingbaseES:" + nameof(KdbndpDbContextOptionsBuilder.MapRange) + ":" + rangeDefinition.SubtypeClrType.Name]
                    = rangeDefinition.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    #endregion Infrastructure
}

/// <summary>
/// A definition for a user-defined KingbaseES range to be mapped.
/// </summary>
public record UserRangeDefinition
{
    /// <summary>
    /// The name of the KingbaseES range type to be mapped.
    /// </summary>
    public virtual string RangeName { get; }

    /// <summary>
    /// The KingbaseES schema in which the range is defined. If null, the default schema is used
    /// (which is public unless changed on the model).
    /// </summary>
    public virtual string? SchemaName { get; }

    /// <summary>
    /// The CLR type of the range's subtype (or element).
    /// The actual mapped type will be an <see cref="KdbndpRange{T}"/> over this type.
    /// </summary>
    public virtual Type SubtypeClrType { get; }

    /// <summary>
    /// Optionally, the name of the range's KingbaseES subtype (or element).
    /// This is usually not needed - the subtype will be inferred based on <see cref="SubtypeClrType"/>.
    /// </summary>
    public virtual string? SubtypeName { get; }

    public UserRangeDefinition(
        string rangeName,
        string? schemaName,
        Type subtypeClrType,
        string? subtypeName)
    {
        RangeName = Check.NotEmpty(rangeName, nameof(rangeName));
        SchemaName = schemaName;
        SubtypeClrType = Check.NotNull(subtypeClrType, nameof(subtypeClrType));
        SubtypeName = subtypeName;
    }
}
