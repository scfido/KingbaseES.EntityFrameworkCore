using System;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTimeTzTypeMapping : KdbndpTypeMapping
{
    public KdbndpTimeTzTypeMapping() : base("time with time zone", typeof(DateTimeOffset), KdbndpDbType.TimeTz) {}

    protected KdbndpTimeTzTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.TimeTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTimeTzTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => FormattableString.Invariant($"TIMETZ '{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}'");

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => FormattableString.Invariant(@$"""{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}""");
}
