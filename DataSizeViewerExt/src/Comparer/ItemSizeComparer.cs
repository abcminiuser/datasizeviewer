using System;
using System.ComponentModel;
using System.Globalization;

namespace FourWalledCubicle.DataSizeViewerExt.Comparer
{
    public class ItemSizeComparer : ListViewCustomComparer<ItemSize>
    {
        public override int Compare(ItemSize x, ItemSize y)
        {
            switch (SortBy)
            {
                case "Name":
                    int nameCompRes = String.Compare(x.Name, y.Name, CultureInfo.CurrentUICulture, CompareOptions.IgnoreCase);
                    return (SortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1) * nameCompRes;

                case "Storage":
                    int storageCompRes = String.Compare(x.Storage, y.Storage, CultureInfo.CurrentUICulture, CompareOptions.IgnoreCase);
                    return (SortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1) * storageCompRes;

                case "Size":
                    int sizeCompRes = uintCompareAscending(x.Size, y.Size);
                    return (SortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1) * sizeCompRes;

                default:
                    return 0;
            }
        }

        private int uintCompareAscending(uint x, uint y)
        {
            if (x == y)
                return 0;
            else
                return (x > y) ? 1 : -1;
        }
    }
}