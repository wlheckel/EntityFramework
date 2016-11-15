// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalValueGeneratorConvention : ValueGeneratorConvention
    {
        public RelationalValueGeneratorConvention([NotNull] IRelationalAnnotationProvider annotationProvider)
        {
            AnnotationProvider = annotationProvider;
        }

        protected virtual IRelationalAnnotationProvider AnnotationProvider { get; }

        public override Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var property = propertyBuilder.Metadata;
            var providerAnnotations = (AnnotationProvider.For(property) as RelationalPropertyAnnotations)
                ?.ProviderFullAnnotationNames;
            if (name == RelationalFullAnnotationNames.Instance.DefaultValue
                || name == RelationalFullAnnotationNames.Instance.DefaultValueSql
                || name == RelationalFullAnnotationNames.Instance.ComputedColumnSql
                || (providerAnnotations != null
                    && (name == providerAnnotations.DefaultValue
                        || name == providerAnnotations.DefaultValueSql
                        || name == providerAnnotations.ComputedColumnSql)))
            {
                propertyBuilder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                propertyBuilder.RequiresValueGenerator(GetRequiresValueGenerator(propertyBuilder.Metadata), ConfigurationSource.Convention);
                return annotation;
            }

            return base.Apply(propertyBuilder, name, annotation, oldAnnotation);
        }

        protected override ValueGenerated? GetValueGenerated(Property property)
        {
            var valueGenerated = base.GetValueGenerated(property);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            var relationalProperty = AnnotationProvider.For(property);
            return relationalProperty.ComputedColumnSql != null
                ? ValueGenerated.OnAddOrUpdate
                : relationalProperty.DefaultValue != null
                  || relationalProperty.DefaultValueSql != null
                    ? ValueGenerated.OnAdd
                    : (ValueGenerated?)null;
        }

        protected override bool? GetRequiresValueGenerator(Property property)
        {
            var requiresValueGenerator = base.GetRequiresValueGenerator(property);
            if (requiresValueGenerator != null)
            {
                return requiresValueGenerator;
            }

            var relationalProperty = AnnotationProvider.For(property);
            return relationalProperty.ComputedColumnSql != null
                   || relationalProperty.DefaultValue != null
                   || relationalProperty.DefaultValueSql != null
                ? false
                : (bool?)null;
        }
    }
}
