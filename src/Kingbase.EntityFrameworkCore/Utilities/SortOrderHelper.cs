using System.Collections.Generic;
using System.Linq;
using Kdbndp.EntityFrameworkCore.KingbaseES.Metadata;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Utilities;

internal static class SortOrderHelper
{
    public static bool IsDefaultSortOrder(IReadOnlyList<SortOrder>? sortOrders)
        => sortOrders?.All(sortOrder => sortOrder == SortOrder.Ascending) ?? true;

    public static bool IsDefaultNullSortOrder(
        IReadOnlyList<NullSortOrder>? nullSortOrders,
        IReadOnlyList<SortOrder>? sortOrders)
    {
        if (nullSortOrders is null)
        {
            return true;
        }

        for (var i = 0; i < nullSortOrders.Count; i++)
        {
            var nullSortOrder = nullSortOrders[i];

            // We need to consider the ASC/DESC sort order to determine the default NULLS FIRST/LAST sort order.
            var sortOrder = i < sortOrders?.Count ? sortOrders[i] : SortOrder.Ascending;

            if (sortOrder == SortOrder.Descending)
            {
                // NULLS FIRST is the default when DESC is specified.
                if (nullSortOrder != NullSortOrder.NullsFirst)
                {
                    return false;
                }
            }
            else
            {
                // NULLS LAST is the default when DESC is NOT specified.
                if (nullSortOrder != NullSortOrder.NullsLast)
                {
                    return false;
                }
            }
        }

        return true;
    }
}