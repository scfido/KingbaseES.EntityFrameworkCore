using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

//public class KdbndpLTreeTypeMapping : KdbndpStringTypeMapping
//{
//    private static readonly ConstructorInfo Constructor = typeof(LTree).GetConstructor(new[] { typeof(string) })!;

//    public KdbndpLTreeTypeMapping()
//        : base(
//            new RelationalTypeMappingParameters(
//                new CoreTypeMappingParameters(
//                    typeof(LTree),
//                    new ValueConverter<LTree, string>(l => l, s => new(s))),
//                "ltree"),
//            KdbndpDbType.LTree)
//    {
//    }

//    protected KdbndpLTreeTypeMapping(RelationalTypeMappingParameters parameters)
//        : base(parameters, KdbndpDbType.LTree)
//    {
//    }

//    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
//        => new KdbndpLTreeTypeMapping(parameters);

//    public override Expression GenerateCodeLiteral(object value)
//        => Expression.New(Constructor, Expression.Constant((string)(LTree)value, typeof(string)));
//}
