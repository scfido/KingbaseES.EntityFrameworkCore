using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTimeTypeMapping : KdbndpTypeMapping
{
    public KdbndpTimeTypeMapping(Type clrType) : base("time without time zone", clrType, KdbndpDbType.Time) {}

    protected KdbndpTimeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Time) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTimeTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIME '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => value switch
        {
            TimeSpan ts => ts.Ticks % 10000000 == 0
                ? ts.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : ts.ToString(@"hh\:mm\:ss\.FFFFFF", CultureInfo.InvariantCulture),
            TimeOnly t => t.Ticks % 10000000 == 0
                ? t.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture)
                : t.ToString(@"HH\:mm\:ss\.FFFFFF", CultureInfo.InvariantCulture),
            _ => throw new InvalidCastException($"Can't generate a time SQL literal for CLR type {value.GetType()}")
        };
}