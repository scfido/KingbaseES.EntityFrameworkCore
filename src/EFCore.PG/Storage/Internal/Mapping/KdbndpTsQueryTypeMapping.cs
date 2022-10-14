using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTsQueryTypeMapping : KdbndpTypeMapping
{
    public KdbndpTsQueryTypeMapping() : base("tsquery", typeof(KdbndpTsQuery), KdbndpDbType.TsQuery) { }

    protected KdbndpTsQueryTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.TsQuery) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTsQueryTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        Check.NotNull(value, nameof(value));
        var query = (KdbndpTsQuery)value;
        var builder = new StringBuilder();
        builder.Append("TSQUERY  ");
        var indexOfFirstQuote = builder.Length - 1;
        query.Write(builder);
        builder.Replace("'", "''");
        builder[indexOfFirstQuote] = '\'';
        builder.Append("'");
        return builder.ToString();
    }
}