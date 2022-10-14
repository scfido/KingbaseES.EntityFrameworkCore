using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

public class KdbndpRandomTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo _methodInfo
        = typeof(DbFunctionsExtensions).GetRuntimeMethod(nameof(DbFunctionsExtensions.Random), new[] { typeof(DbFunctions) })!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public KdbndpRandomTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(method, nameof(method));
        Check.NotNull(arguments, nameof(arguments));
        Check.NotNull(logger, nameof(logger));

        return _methodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                "random",
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: Array.Empty<bool>(),
                method.ReturnType)
            : null;
    }
}