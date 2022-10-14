using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;
using KdbndpTypes;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

public class KdbndpRangeTranslator : IMethodCallTranslator, IMemberTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;
    private readonly IModel _model;
    private readonly bool _supportsMultiranges;

    private static readonly MethodInfo EnumerableAnyWithoutPredicate =
        typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    public KdbndpRangeTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory npgsqlSqlExpressionFactory,
        IModel model,
        IKdbndpSingletonOptions npgsqlSingletonOptions)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = npgsqlSqlExpressionFactory;
        _model = model;
        _supportsMultiranges = npgsqlSingletonOptions.PostgresVersionWithoutDefault is null
            || npgsqlSingletonOptions.PostgresVersionWithoutDefault.AtLeast(14);
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Any() over multirange -> NOT isempty(). KdbndpRange<T> has IsEmpty which is translated below.
        if (_supportsMultiranges
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableAnyWithoutPredicate
            && arguments[0].Type.TryGetMultirangeSubtype(out _))
        {
            return _sqlExpressionFactory.Not(
                _sqlExpressionFactory.Function(
                    "isempty",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)));
        }

        if (method.DeclaringType != typeof(KdbndpRangeDbFunctionsExtensions)
            && (method.DeclaringType != typeof(KdbndpMultirangeDbFunctionsExtensions) || !_supportsMultiranges))
        {
            return null;
        }

        if (method.Name == nameof(KdbndpRangeDbFunctionsExtensions.Merge))
        {
            if (method.DeclaringType == typeof(KdbndpRangeDbFunctionsExtensions))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    inferredMapping);
            }

            if (method.DeclaringType == typeof(KdbndpMultirangeDbFunctionsExtensions))
            {

                //var returnTypeMapping = arguments[0].TypeMapping is KdbndpMultirangeTypeMapping multirangeTypeMapping
                //    ? multirangeTypeMapping.RangeMapping
                //    : null;

                //return _sqlExpressionFactory.Function(
                //    "range_merge",
                //    new[] { arguments[0] },
                //    nullable: true,
                //    argumentsPropagateNullability: TrueArrays[1],
                //    method.ReturnType,
                //    returnTypeMapping);

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType,
                    null);

            }
        }

        return method.Name switch
        {
            nameof(KdbndpRangeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsStrictlyLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyLeftOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsStrictlyRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyRightOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.DoesNotExtendRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendRightOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.DoesNotExtendLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendLeftOf, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.IsAdjacentTo)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsAdjacentTo, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeUnion, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIntersect, arguments[0], arguments[1]),
            nameof(KdbndpRangeDbFunctionsExtensions.Except)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeExcept, arguments[0], arguments[1]),

            _ => null
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var type = member.DeclaringType;
        if (type is null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(KdbndpRange<>))
        {
            return null;
        }

        if (member.Name == nameof(KdbndpRange<int>.LowerBound) || member.Name == nameof(KdbndpRange<int>.UpperBound))
        {
            var typeMapping = instance!.TypeMapping is KdbndpRangeTypeMapping rangeMapping
                ? rangeMapping.SubtypeMapping
                : _typeMappingSource.FindMapping(returnType, _model);

            return _sqlExpressionFactory.Function(
                member.Name == nameof(KdbndpRange<int>.LowerBound) ? "lower" : "upper",
                new[] { instance },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                returnType,
                typeMapping);
        }

        return member.Name switch
        {
            nameof(KdbndpRange<int>.IsEmpty)               => SingleArgBoolFunction("isempty", instance!),
            nameof(KdbndpRange<int>.LowerBoundIsInclusive) => SingleArgBoolFunction("lower_inc", instance!),
            nameof(KdbndpRange<int>.UpperBoundIsInclusive) => SingleArgBoolFunction("upper_inc", instance!),
            nameof(KdbndpRange<int>.LowerBoundInfinite)    => SingleArgBoolFunction("lower_inf", instance!),
            nameof(KdbndpRange<int>.UpperBoundInfinite)    => SingleArgBoolFunction("upper_inf", instance!),

            _ => null
        };

        SqlFunctionExpression SingleArgBoolFunction(string name, SqlExpression argument)
            => _sqlExpressionFactory.Function(
                name,
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(bool));
    }

    private static readonly ConcurrentDictionary<Type, object> _defaults = new();

    private static object? GetDefaultValue(Type type)
        => type.IsValueType ? _defaults.GetOrAdd(type, Activator.CreateInstance!) : null;
}
