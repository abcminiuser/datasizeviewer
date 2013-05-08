using System.ComponentModel;
using System.Windows.Controls;
using FourWalledCubicle.DataSizeViewerExt.Interfaces;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public class ListViewSortItem
    {
        public ListViewSortItem(IListViewCustomComparer comparer, GridViewColumnHeader lastColumnHeaderClicked, ListSortDirection lastSortDirection)
        {
            Comparer = comparer;
            LastColumnHeaderClicked = lastColumnHeaderClicked;
            LastSortDirection = lastSortDirection;
        }

        public IListViewCustomComparer Comparer { get; private set; }

        public GridViewColumnHeader LastColumnHeaderClicked { get; set; }

        public ListSortDirection LastSortDirection { get; set; }
    }
}
