using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Internal;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Metadata.Conventions;

/// <summary>
/// A convention that configures store value generation as <see cref="ValueGenerated.OnAdd"/> on properties that are
/// part of the primary key and not part of any foreign keys, were configured to have a database default value
/// or were configured to use a <see cref="KdbndpValueGenerationStrategy"/>.
/// It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate"/> if they were configured as computed columns.
/// </summary>
public class KdbndpValueGenerationConvention : RelationalValueGenerationConvention
{
    /// <summary>
    /// Creates a new instance of <see cref="KdbndpValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public KdbndpValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    /// Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == KdbndpAnnotationNames.ValueGenerationStrategy)
        {
            propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
            return;
        }

        if (name == KdbndpAnnotationNames.TsVectorConfig &&
            propertyBuilder.Metadata.GetTsVectorConfig() is not null)
        {
            propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate);
            return;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <summary>
    /// Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var tableName = property.DeclaringEntityType.GetTableName();
        if (tableName is null)
        {
            return null;
        }

        return GetValueGenerated(
            property,
            StoreObjectIdentifier.Table(tableName, property.DeclaringEntityType.GetSchema()),
            Dependencies.TypeMappingSource);
    }

    /// <summary>
    /// Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns>The store value generation strategy to set for the given property.</returns>
    public new static ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject) != KdbndpValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : (ValueGenerated?)null);

    private ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource typeMappingSource)
        => RelationalValueGenerationConvention.GetValueGenerated(property, storeObject)
            ?? (property.GetValueGenerationStrategy(storeObject, typeMappingSource) != KdbndpValueGenerationStrategy.None
                ? ValueGenerated.OnAdd
                : (ValueGenerated?)null);
}