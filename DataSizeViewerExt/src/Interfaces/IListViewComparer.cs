using System;
using System.Collections;
using System.ComponentModel;

namespace FourWalledCubicle.DataSizeViewerExt.Interfaces
{
    public interface IListViewCustomComparer : IComparer
    {
        String SortBy { get; set; }
 
        ListSortDirection SortDirection { get; set; }
    }
}
