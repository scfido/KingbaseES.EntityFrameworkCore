using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

public class KdbndpTimeSpanMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public KdbndpTimeSpanMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    private static readonly bool[] FalseTrueArray = { false, true };

    public virtual SqlExpression? Translate(SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(member, nameof(member));
        Check.NotNull(returnType, nameof(returnType));

        if (member.DeclaringType == typeof(TimeSpan) && instance is not null)
        {
            return member.Name switch
            {
                nameof(TimeSpan.Days) => Floor(DatePart("day", instance)),
                nameof(TimeSpan.Hours) => Floor(DatePart("hour", instance)),
                nameof(TimeSpan.Minutes) => Floor(DatePart("minute", instance)),
                nameof(TimeSpan.Seconds) => Floor(DatePart("second", instance)),
                nameof(TimeSpan.Milliseconds) => _sqlExpressionFactory.Modulo(
                    Floor(DatePart("millisecond", instance!)),
                    _sqlExpressionFactory.Constant(1000)),

                nameof(TimeSpan.TotalDays) => TranslateDurationTotalMember(instance, 86400),
                nameof(TimeSpan.TotalHours) => TranslateDurationTotalMember(instance, 3600),
                nameof(TimeSpan.TotalMinutes) => TranslateDurationTotalMember(instance, 60),
                nameof(TimeSpan.TotalSeconds) => DatePart("epoch", instance),
                nameof(TimeSpan.TotalMilliseconds) => TranslateDurationTotalMember(instance, 0.001),

                _ => null
            };
        }

        return null;

        SqlExpression Floor(SqlExpression value)
            => _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "floor",
                    new[] { value },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(double)),
                typeof(int));

        SqlFunctionExpression DatePart(string part, SqlExpression value)
            => _sqlExpressionFactory.Function("date_part", new[]
                {
                    _sqlExpressionFactory.Constant(part),
                    value
                },
                nullable: true,
                argumentsPropagateNullability: FalseTrueArray,
                returnType);

        SqlBinaryExpression TranslateDurationTotalMember(SqlExpression instance, double divisor)
            => _sqlExpressionFactory.Divide(DatePart("epoch", instance), _sqlExpressionFactory.Constant(divisor));
    }
}