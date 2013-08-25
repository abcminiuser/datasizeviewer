using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FourWalledCubicle.DataSizeViewerExt.Interfaces;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public static class ListViewSorter
    {
        private static Dictionary<String, ListViewSortItem> _listViewDefinitions = new Dictionary<String, ListViewSortItem>();

        public static readonly DependencyProperty CustomListViewSorterProperty = DependencyProperty.RegisterAttached(
            "CustomListViewSorter",
            typeof(IComparer),
            typeof(ListViewSorter),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRegisterSortableGrid)));

        public static IComparer GetCustomListViewSorter(DependencyObject obj)
        {
            return obj.GetValue(CustomListViewSorterProperty) as IComparer;
        }

        public static void SetCustomListViewSorter(DependencyObject obj, IComparer value)
        {
            obj.SetValue(CustomListViewSorterProperty, value);
        }

        public static void GridViewColumnHeaderClickedHandler(Object sender, RoutedEventArgs e)
        {
            ListView view = sender as ListView;
            if (view == null) return;

            ListViewSortItem listViewSortItem = (_listViewDefinitions.ContainsKey(view.Name)) ? _listViewDefinitions[view.Name] : null;
            if (listViewSortItem == null) return;

            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null) return;

            ListCollectionView collectionView = CollectionViewSource.GetDefaultView(view.ItemsSource) as ListCollectionView;
            if (collectionView == null) return;

            ListSortDirection sortDirection = GetSortingDirection(headerClicked, listViewSortItem);

            // get header name
            String header = (headerClicked.Column.DisplayMemberBinding as Binding).Path.Path as String;
            if (String.IsNullOrEmpty(header)) return;

            // sort listview
            if (listViewSortItem.Comparer != null)
            {
                listViewSortItem.Comparer.SortBy = header;
                listViewSortItem.Comparer.SortDirection = sortDirection;
                collectionView.CustomSort = listViewSortItem.Comparer;
            }
            else
            {
                view.Items.SortDescriptions.Clear();
                view.Items.SortDescriptions.Add(new SortDescription(headerClicked.Column.Header.ToString(), sortDirection));
            }

            view.Items.Refresh();

            // change datatemplate of previous and current column header
            headerClicked.Column.HeaderTemplate = GetHeaderColumnsDataTemplate(view, listViewSortItem, sortDirection);

            // Set current sort values as last sort values
            listViewSortItem.LastColumnHeaderClicked = headerClicked;
            listViewSortItem.LastSortDirection = sortDirection;
        }

        private static void OnRegisterSortableGrid(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Check if we are in design mode, if so don't do anything.
            if ((Boolean)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) return;

            ListView view = obj as ListView;

            if (view != null)
            {
                _listViewDefinitions.Add(view.Name, new ListViewSortItem(GetCustomListViewSorter(obj) as IListViewCustomComparer, null, ListSortDirection.Ascending));
                view.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            }
        }

        private static DataTemplate GetHeaderColumnsDataTemplate(ListView view, ListViewSortItem listViewSortItem, ListSortDirection sortDirection)
        {
            // remove mark from previous sort column
            if (listViewSortItem.LastColumnHeaderClicked != null)
                listViewSortItem.LastColumnHeaderClicked.Column.HeaderTemplate = view.TryFindResource("ListViewHeaderTemplateNoSorting") as DataTemplate;

            // set correct mark to current column
            switch (sortDirection)
            {
                case ListSortDirection.Ascending:
                    return view.TryFindResource("ListViewHeaderTemplateAscendingSorting") as DataTemplate;
                case ListSortDirection.Descending:
                    return view.TryFindResource("ListViewHeaderTemplateDescendingSorting") as DataTemplate;
                default:
                    return null;
            }
        }

        private static ListSortDirection GetSortingDirection(GridViewColumnHeader headerClicked, ListViewSortItem listViewSortItem)
        {
            if (headerClicked != listViewSortItem.LastColumnHeaderClicked)
                return ListSortDirection.Descending;
            else
                return (listViewSortItem.LastSortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }
    }
}
