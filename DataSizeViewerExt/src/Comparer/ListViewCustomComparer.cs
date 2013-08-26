using System;
using System.ComponentModel;
using FourWalledCubicle.DataSizeViewerExt.Interfaces;

namespace FourWalledCubicle.DataSizeViewerExt.Comparer
{
    public abstract class ListViewCustomComparer<T> : IListViewCustomComparer where T : class
    {
        private String sortBy = String.Empty;
        private ListSortDirection direction = ListSortDirection.Ascending;

        public String SortBy
        {
            get { return sortBy; }
            set { sortBy = value; }
        }
 
        public ListSortDirection SortDirection
        {
            get { return direction; }
            set { direction = value; }
        }
 
        public Int32 Compare(Object x, Object y)
        {
            T item1 = x as T;
            T item2 = y as T;
 
            if (item1 == null || item2 == null)
            {
                System.Diagnostics.Trace.Write("Either x or y is null in compare(x,y)" + Environment.NewLine);
                return 0;
            }
 
            return Compare(item1, item2);
         }
 
         public abstract Int32 Compare(T x, T y);
    }
}

