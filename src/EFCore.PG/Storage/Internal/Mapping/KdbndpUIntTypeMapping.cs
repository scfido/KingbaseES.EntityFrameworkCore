using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpUintTypeMapping : KdbndpTypeMapping
{
    public KdbndpUintTypeMapping(string storeType, KdbndpDbType npgsqlDbType)
        : base(storeType, typeof(uint), npgsqlDbType) {}

    protected KdbndpUintTypeMapping(RelationalTypeMappingParameters parameters, KdbndpDbType npgsqlDbType)
        : base(parameters, npgsqlDbType) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpUintTypeMapping(parameters, KdbndpDbType);
}