﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;
using Kdbndp.NameTranslation;
using KdbndpTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Kdbndp specific extension methods for <see cref="ModelBuilder"/>.
/// </summary>
public static class KdbndpModelBuilderExtensions
{
    #region HiLo

    /// <summary>
    /// Configures the model to use a sequence-based hi-lo pattern to generate values for properties
    /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseHiLo(this ModelBuilder modelBuilder, string? name = null, string? schema = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var model = modelBuilder.Model;

        name ??= KdbndpModelExtensions.DefaultHiLoSequenceName;

        if (model.FindSequence(name, schema) is null)
        {
            modelBuilder.HasSequence(name, schema).IncrementsBy(10);
        }

        model.SetValueGenerationStrategy(KdbndpValueGenerationStrategy.SequenceHiLo);
        model.SetHiLoSequenceName(name);
        model.SetHiLoSequenceSchema(schema);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the database sequence used for the hi-lo pattern to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES.
    /// </summary>
    /// <param name="modelBuilder"> The model builder. </param>
    /// <param name="name"> The name of the sequence. </param>
    /// <param name="schema">The schema of the sequence. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    /// <returns> A builder to further configure the sequence. </returns>
    public static IConventionSequenceBuilder? HasHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!modelBuilder.CanSetHiLoSequence(name, schema))
        {
            return null;
        }

        modelBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
        modelBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

        return name is null ? null : modelBuilder.HasSequence(name, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
    /// </summary>
    /// <param name="modelBuilder"> The model builder. </param>
    /// <param name="name"> The name of the sequence. </param>
    /// <param name="schema">The schema of the sequence. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    /// <returns> <c>true</c> if the given name and schema can be set for the hi-lo sequence. </returns>
    public static bool CanSetHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        return modelBuilder.CanSetAnnotation(KdbndpAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
            && modelBuilder.CanSetAnnotation(KdbndpAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
    }

    #endregion HiLo

    #region Serial

    /// <summary>
    /// <para>
    /// Configures the model to use the KingbaseES SERIAL feature to generate values for properties
    /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES.
    /// </para>
    /// <para>
    /// This option should be considered deprecated starting with KingbaseES 10, consider using <see cref="UseIdentityColumns"/> instead.
    /// </para>
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseSerialColumns(
        this ModelBuilder modelBuilder)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));

        var property = modelBuilder.Model;

        property.SetValueGenerationStrategy(KdbndpValueGenerationStrategy.SerialColumn);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);

        return modelBuilder;
    }

    #endregion Serial

    #region Identity

    /// <summary>
    /// <para>
    /// Configures the model to use the KingbaseES IDENTITY feature to generate values for properties
    /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES. Values for these
    /// columns will always be generated as identity, and the application will not be able to override
    /// this behavior by providing a value.
    /// </para>
    /// <para>Available only starting KingbaseES 10.</para>
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityAlwaysColumns(this ModelBuilder modelBuilder)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));

        var property = modelBuilder.Model;

        property.SetValueGenerationStrategy(KdbndpValueGenerationStrategy.IdentityAlwaysColumn);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);

        return modelBuilder;
    }

    /// <summary>
    /// <para>
    /// Configures the model to use the KingbaseES IDENTITY feature to generate values for properties
    /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES. Values for these
    /// columns will be generated as identity by default, but the application will be able to override
    /// this behavior by providing a value.
    /// </para>
    /// <para>
    /// This is the default behavior when targeting KingbaseES. Available only starting KingbaseES 10.
    /// </para>
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityByDefaultColumns(this ModelBuilder modelBuilder)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));

        var property = modelBuilder.Model;

        property.SetValueGenerationStrategy(KdbndpValueGenerationStrategy.IdentityByDefaultColumn);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);

        return modelBuilder;
    }

    /// <summary>
    /// <para>
    /// Configures the model to use the KingbaseES IDENTITY feature to generate values for properties
    /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting KingbaseES. Values for these
    /// columns will be generated as identity by default, but the application will be able to override
    /// this behavior by providing a value.
    /// </para>
    /// <para>
    /// This is the default behavior when targeting KingbaseES. Available only starting KingbaseES 10.
    /// </para>
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityColumns(this ModelBuilder modelBuilder)
        => modelBuilder.UseIdentityByDefaultColumns();

    /// <summary>
    /// Configures the value generation strategy for the key property, when targeting KingbaseES.
    /// </summary>
    /// <param name="modelBuilder">The builder for the property being configured.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    /// The same builder instance if the configuration was applied, <c>null</c> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        KdbndpValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetAnnotation(
                KdbndpAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            if (valueGenerationStrategy != KdbndpValueGenerationStrategy.SequenceHiLo)
            {
                modelBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
            }

            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default value generation strategy.
    /// </summary>
    /// <param name="modelBuilder"> The model builder. </param>
    /// <param name="valueGenerationStrategy"> The value generation strategy. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    /// <returns> <c>true</c> if the given value can be set as the default value generation strategy. </returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        KdbndpValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));

        return modelBuilder.CanSetAnnotation(
            KdbndpAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);
    }

    #endregion Identity

    #region Extensions

    /// <summary>
    /// Registers a KingbaseES extension in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder in which to define the extension.</param>
    /// <param name="schema">The schema in which to create the extension.</param>
    /// <param name="name">The name of the extension to create.</param>
    /// <param name="version">The version of the extension.</param>
    /// <returns>
    /// The updated <see cref="ModelBuilder"/>.
    /// </returns>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/external-extensions.html
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/></exception>
    public static ModelBuilder HasPostgresExtension(
        this ModelBuilder modelBuilder,
        string? schema,
        string name,
        string? version = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NullButNotEmpty(schema, nameof(schema));
        Check.NotEmpty(name, nameof(name));

        modelBuilder.Model.GetOrAddPostgresExtension(schema, name, version);

        return modelBuilder;
    }

    /// <summary>
    /// Registers a KingbaseES extension in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder in which to define the extension.</param>
    /// <param name="name">The name of the extension to create.</param>
    /// <returns>
    /// The updated <see cref="ModelBuilder"/>.
    /// </returns>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/external-extensions.html
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/></exception>
    public static ModelBuilder HasPostgresExtension(
        this ModelBuilder modelBuilder,
        string name)
        => modelBuilder.HasPostgresExtension(null, name, null);

    #endregion

    #region Enums

    /// <summary>
    /// Registers a user-defined enum type in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
    /// <param name="schema">The schema in which to create the enum type.</param>
    /// <param name="name">The name of the enum type to create.</param>
    /// <param name="labels">The enum label values.</param>
    /// <returns>
    /// The updated <see cref="ModelBuilder"/>.
    /// </returns>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
    /// </remarks>
    /// <exception cref="ArgumentNullException">builder</exception>
    public static ModelBuilder HasPostgresEnum(
        this ModelBuilder modelBuilder,
        string? schema,
        string name,
        string[] labels)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(labels, nameof(labels));

        modelBuilder.Model.GetOrAddPostgresEnum(schema, name, labels);
        return modelBuilder;
    }

    /// <summary>
    /// Registers a user-defined enum type in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
    /// <param name="name">The name of the enum type to create.</param>
    /// <param name="labels">The enum label values.</param>
    /// <returns>
    /// The updated <see cref="ModelBuilder"/>.
    /// </returns>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
    /// </remarks>
    /// <exception cref="ArgumentNullException">builder</exception>
    public static ModelBuilder HasPostgresEnum(
        this ModelBuilder modelBuilder,
        string name,
        string[] labels)
        => modelBuilder.HasPostgresEnum(null, name, labels);

    /// <summary>
    /// Registers a user-defined enum type in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
    /// <param name="schema">The schema in which to create the enum type.</param>
    /// <param name="name">The name of the enum type to create.</param>
    /// <param name="nameTranslator">
    /// The translator for name and label inference.
    /// Defaults to <see cref="KdbndpSnakeCaseNameTranslator"/>.</param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns>
    /// The updated <see cref="ModelBuilder"/>.
    /// </returns>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
    /// </remarks>
    /// <exception cref="ArgumentNullException">builder</exception>
    public static ModelBuilder HasPostgresEnum<TEnum>(
        this ModelBuilder modelBuilder,
        string? schema = null,
        string? name = null,
        IKdbndpNameTranslator? nameTranslator = null)
        where TEnum : struct, Enum
    {
        if (nameTranslator is null)
        {
            nameTranslator = KdbndpConnection.GlobalTypeMapper.DefaultNameTranslator;
        }

        return modelBuilder.HasPostgresEnum(
            schema,
            name ?? GetTypePgName<TEnum>(nameTranslator),
            GetMemberPgNames<TEnum>(nameTranslator));
    }

    #endregion

    #region Templates

    public static ModelBuilder UseDatabaseTemplate(
        this ModelBuilder modelBuilder,
        string templateDatabaseName)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(templateDatabaseName, nameof(templateDatabaseName));

        modelBuilder.Model.SetDatabaseTemplate(templateDatabaseName);
        return modelBuilder;
    }

    #endregion

    #region Ranges

    /// <summary>
    /// Registers a user-defined range type in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder on which to create the range type.</param>
    /// <param name="schema">The schema in which to create the range type.</param>
    /// <param name="name">The name of the range type to be created.</param>
    /// <param name="subtype">The subtype (or element type) of the range</param>
    /// <param name="canonicalFunction">
    /// An optional KingbaseES function which converts range values to a canonical form.
    /// </param>
    /// <param name="subtypeOpClass">Used to specify a non-default operator class.</param>
    /// <param name="collation">Used to specify a non-default collation in the range's order.</param>
    /// <param name="subtypeDiff">
    /// An optional KingbaseES function taking two values of the subtype type as argument, and return a double
    /// precision value representing the difference between the two given values.
    /// </param>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
    /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
    /// </remarks>
    public static ModelBuilder HasPostgresRange(
        this ModelBuilder modelBuilder,
        string? schema,
        string name,
        string subtype,
        string? canonicalFunction = null,
        string? subtypeOpClass = null,
        string? collation = null,
        string? subtypeDiff = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(subtype, nameof(subtype));

        modelBuilder.Model.GetOrAddPostgresRange(
            schema,
            name,
            subtype,
            canonicalFunction,
            subtypeOpClass,
            collation,
            subtypeDiff);
        return modelBuilder;
    }

    /// <summary>
    /// Registers a user-defined range type in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder on which to create the range type.</param>
    /// <param name="name">The name of the range type to be created.</param>
    /// <param name="subtype">The subtype (or element type) of the range</param>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
    /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
    /// </remarks>
    public static ModelBuilder HasPostgresRange(
        this ModelBuilder modelBuilder,
        string name,
        string subtype)
        => HasPostgresRange(modelBuilder, null, name, subtype);

    #endregion

    #region Tablespaces

    public static ModelBuilder UseTablespace(
        this ModelBuilder modelBuilder,
        string tablespace)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(tablespace, nameof(tablespace));

        modelBuilder.Model.SetTablespace(tablespace);
        return modelBuilder;
    }

    #endregion

    #region Collation management

    /// <summary>
    /// Creates a new collation in the database.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/sql-createcollation.html.
    /// </remarks>
    /// <param name="modelBuilder">The model builder on which to create the collation.</param>
    /// <param name="name">The name of the collation to create.</param>
    /// <param name="locale">Sets LC_COLLATE and LC_CTYPE at once.</param>
    /// <param name="provider">
    /// Specifies the provider to use for locale services associated with this collation.
    /// The available choices depend on the operating system and build options.</param>
    /// <param name="deterministic">
    /// Specifies whether the collation should use deterministic comparisons.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasCollation(
        this ModelBuilder modelBuilder,
        string name,
        string locale,
        string? provider = null,
        bool? deterministic = null)
        => modelBuilder.HasCollation(schema: null, name, locale, provider: provider, deterministic: deterministic);

    /// <summary>
    /// Creates a new collation in the database.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/sql-createcollation.html.
    /// </remarks>
    /// <param name="modelBuilder">The model builder on which to create the collation.</param>
    /// <param name="schema">The schema in which to create the collation, or <c>null</c> for the default schema.</param>
    /// <param name="name">The name of the collation to create.</param>
    /// <param name="locale">Sets LC_COLLATE and LC_CTYPE at once.</param>
    /// <param name="provider">
    /// Specifies the provider to use for locale services associated with this collation.
    /// The available choices depend on the operating system and build options.</param>
    /// <param name="deterministic">
    /// Specifies whether the collation should use deterministic comparisons.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasCollation(
        this ModelBuilder modelBuilder,
        string? schema,
        string name,
        string locale,
        string? provider = null,
        bool? deterministic = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(locale, nameof(locale));

        modelBuilder.Model.GetOrAddCollation(
            schema,
            name,
            locale,
            locale,
            provider,
            deterministic);
        return modelBuilder;
    }

    /// <summary>
    /// Creates a new collation in the database.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/sql-createcollation.html.
    /// </remarks>
    /// <param name="modelBuilder">The model builder on which to create the collation.</param>
    /// <param name="schema">The schema in which to create the collation, or <c>null</c> for the default schema.</param>
    /// <param name="name">The name of the collation to create.</param>
    /// <param name="lcCollate">Use the specified operating system locale for the LC_COLLATE locale category.</param>
    /// <param name="lcCtype">Use the specified operating system locale for the LC_CTYPE locale category.</param>
    /// <param name="provider">
    /// Specifies the provider to use for locale services associated with this collation.
    /// The available choices depend on the operating system and build options.</param>
    /// <param name="deterministic">
    /// Specifies whether the collation should use deterministic comparisons.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasCollation(
        this ModelBuilder modelBuilder,
        string? schema,
        string name,
        string lcCollate,
        string lcCtype,
        string? provider = null,
        bool? deterministic = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NotEmpty(name, nameof(name));
        Check.NotEmpty(lcCollate, nameof(lcCollate));
        Check.NotEmpty(lcCtype, nameof(lcCtype));

        modelBuilder.Model.GetOrAddCollation(
            schema,
            name,
            lcCollate,
            lcCtype,
            provider,
            deterministic);
        return modelBuilder;
    }

    #endregion Collation management

    #region Default column collation

    /// <summary>
    /// Configures the default collation for all columns in the database. This causes EF Core to specify an explicit
    /// collation when creating each column (unless overridden).
    /// </summary>
    /// <remarks>
    /// <p>
    /// An alternative is to specify a database collation via <see cref="RelationalModelBuilderExtensions.UseCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string)"/>,
    /// which will specify the query on <c>CREATE DATABASE</c> instead of for each and every column. However,
    /// KingbaseES support is limited for the collations that can be specific via this mechanism; ICU collations -
    /// which include all case-insensitive collations - are currently unsupported.
    /// </p>
    /// <p>
    /// For more information, see https://www.postgresql.org/docs/current/collation.html.
    /// </p>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static ModelBuilder UseDefaultColumnCollation(this ModelBuilder modelBuilder, string? collation)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        Check.NullButNotEmpty(collation, nameof(collation));

        modelBuilder.Model.SetDefaultColumnCollation(collation);

        return modelBuilder;
    }

    /// <summary>
    /// Configures the default collation for all columns in the database. This causes EF Core to specify an explicit
    /// collation when creating each column (unless overridden).
    /// </summary>
    /// <remarks>
    /// <p>
    /// An alternative is to specify a database collation via <see cref="RelationalModelBuilderExtensions.UseCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string)"/>,
    /// which will specify the query on <c>CREATE DATABASE</c> instead of for each and every column. However,
    /// KingbaseES support is limited for the collations that can be specific via this mechanism; ICU collations -
    /// which include all case-insensitive collations - are currently unsupported.
    /// </p>
    /// <p>
    /// For more information, see https://www.postgresql.org/docs/current/collation.html.
    /// </p>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static IConventionModelBuilder? UseDefaultColumnCollation(
        this IConventionModelBuilder modelBuilder,
        string? collation,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetDefaultColumnCollation(collation, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetDefaultColumnCollation(collation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    /// Returns a value indicating whether the given value can be set as the default column collation.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><c>true</c> if the given value can be set as the collation.</returns>
    public static bool CanSetDefaultColumnCollation(
        this IConventionModelBuilder modelBuilder,
        string? collation,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));

        return modelBuilder.CanSetAnnotation(
            KdbndpAnnotationNames.DefaultColumnCollation, collation, fromDataAnnotation);
    }

    #endregion Default column collation

    #region Helpers

    // See: https://github.com/npgsql/npgsql/blob/dev/src/Kdbndp/TypeMapping/TypeMapperBase.cs#L132-L138
    private static string GetTypePgName<TEnum>(IKdbndpNameTranslator nameTranslator) where TEnum : struct, Enum
        => typeof(TEnum).GetCustomAttribute<KbNameAttribute>()?.KbName ??
            nameTranslator.TranslateTypeName(typeof(TEnum).Name);

    // See: https://github.com/npgsql/npgsql/blob/dev/src/Kdbndp/TypeHandlers/EnumHandler.cs#L118-L129
    private static string[] GetMemberPgNames<TEnum>(IKdbndpNameTranslator nameTranslator) where TEnum : struct, Enum
        => typeof(TEnum)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(x => x.GetCustomAttribute<KbNameAttribute>()?.KbName ??
                nameTranslator.TranslateMemberName(x.Name))
            .ToArray();

    #endregion
}
