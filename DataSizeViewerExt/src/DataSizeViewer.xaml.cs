using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Atmel.Studio.Services;
using EnvDTE;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public partial class DataSizeViewerUI : UserControl
    {
        private readonly DTE mDTE;

        private BuildEvents mBuildEvents;
        private SolutionEvents mSolutionEvents;

        private SymbolSizeParser mSymbolParser;

        private ListViewSortArrowAdorner mSymbolListArrowAdorner = null;
        private GridViewColumnHeader mSymbolListSortColumn = null;

        private bool mShowDataSegmentState = true;
        private bool mShowTextSegmentState = true;
        private string mFilterStringValue = string.Empty;

        public bool mShowDataSegment
        {
            get
            {
                return mShowDataSegmentState;
            }

            set
            {
                mShowDataSegmentState = value;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(symbolSize.ItemsSource);
                dataView.Refresh();
            }
        }

        public bool mShowTextSegment
        {
            get
            {
                return mShowTextSegmentState;
            }

            set
            {
                mShowTextSegmentState = value;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(symbolSize.ItemsSource);
                dataView.Refresh();
            }
        }

        public string mFilterString
        {
            get
            {
                return mFilterStringValue;
            }

            set
            {
                mFilterStringValue = value;

                ICollectionView dataView = CollectionViewSource.GetDefaultView(symbolSize.ItemsSource);
                dataView.Refresh();
            }
        }

        public DataSizeViewerUI()
        {
            InitializeComponent();

            mDTE = Package.GetGlobalService(typeof(DTE)) as DTE;

            mBuildEvents = mDTE.Events.BuildEvents;
            mBuildEvents.OnBuildDone += (Scope, Action) => ReloadProjectSymbols();

            mSolutionEvents = mDTE.Events.SolutionEvents;
            mSolutionEvents.Opened += () => UpdateProjectList();
            mSolutionEvents.AfterClosing += () => UpdateProjectList();
            mSolutionEvents.ProjectAdded += (p) => UpdateProjectList();
            mSolutionEvents.ProjectRemoved += (p) => UpdateProjectList();
            mSolutionEvents.ProjectRenamed += (p, s) => UpdateProjectList();

            projectList.SelectionChanged += (s, e) => ReloadProjectSymbols();

            mSymbolParser = new SymbolSizeParser();
            symbolSize.ItemsSource = mSymbolParser.SymbolSizes;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(symbolSize.ItemsSource);
            dataView.GroupDescriptions.Add(new PropertyGroupDescription((String)(storageColumn.Header as GridViewColumnHeader).Tag));
            dataView.Filter = FilterSymbolEntries;
            symbolSize.Items.SortDescriptions.Add(new SortDescription((String)(sizeColumn.Header as GridViewColumnHeader).Tag, ListSortDirection.Descending));

            UpdateProjectList();
        }

        private bool FilterSymbolEntries(Object currentEntry)
        {
            ItemSize currentItem = (currentEntry as ItemSize);
            bool isMatch = false;

            try
            {
                if (DataSizeViewerPackage.Options.UseRegExFiltering)
                    isMatch = (new Regex(mFilterString, RegexOptions.Compiled)).IsMatch(currentItem.Name);
                else
                    isMatch = currentItem.Name.Contains(mFilterString);
            }
            catch { }

            return isMatch &&
                    ((mShowDataSegment && currentItem.Storage.Contains("Data")) ||
                    (mShowTextSegment && currentItem.Storage.Contains("Text")));
        }

        private void ReloadProjectSymbols()
        {
            mSymbolParser.ClearSymbols();

            ShowError("No project is currently selected.", "Select a project from the toolbar to display symbols.");

            if (projectList.SelectedItem == null)
                return;

            String projectName = (String)projectList.SelectedItem.ToString();
            if (String.IsNullOrEmpty(projectName))
                return;

            ShowError("An internal error has occurred.", "Project could not be loaded.");

            Project project = null;
            try
            {
                project = mDTE.Solution.Item(projectName);
            }
            catch { }

            if (project == null)
                return;

            IProjectHandle projectNode = project.Object as IProjectHandle;
            if (projectNode == null)
                return;

            ShowError("An internal error has occurred.", "Toolchain service could not be loaded.");

            string elfPath = null;

            string previousDirectory = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(project.FullName));
                elfPath = Path.GetFullPath(Path.Combine(projectNode.GetProperty("OutputDirectory"), projectNode.GetProperty("OutputFilename") + ".elf"));
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }

            string toolchainNMPath = GetContentLocation("GNU-NM");

            if (File.Exists(elfPath) && File.Exists(toolchainNMPath))
            {
                ShowError("No symbols have been loaded.", "Ensure you are compiling in Debug mode, and have debug symbols enabled in your toolchain options.");

                DispatcherOperation dispatcher = Dispatcher.BeginInvoke(
                        new Action(
                                () => mSymbolParser.ReloadSymbols(elfPath, toolchainNMPath, DataSizeViewerPackage.Options.VerifyLocations)
                            )
                    );

                dispatcher.Completed += (s, e) =>
                    {
                        if (symbolSize.Items.Count != 0)
                            ShowSymbolTable();
                    };

                if (dispatcher.Status == DispatcherOperationStatus.Completed)
                {
                    if (symbolSize.Items.Count != 0)
                        ShowSymbolTable();
                }
            }
            else
            {
                if (File.Exists(elfPath))
                    ShowError("Could not find toolchain executable.", "Verify that your project toolchain configuration is correctly set.");
                else
                    ShowError("Could not find project ELF file.", "Verify that the ELF file output specified in your project options exists.");
            }
        }

        private void ShowError(String messagePrimary, String messageSecondary)
        {
            errorMessagePrimary.Text = messagePrimary;
            errorMessageSecondary.Text = messageSecondary;

            errorMessagePanel.Visibility = Visibility.Visible;
        }

        private void ShowSymbolTable()
        {
            errorMessagePanel.Visibility = Visibility.Hidden;
        }

        private void UpdateProjectList()
        {
            string currentSelection = string.Empty;

            if (projectList.SelectedItem != null)
                currentSelection = projectList.SelectedItem.ToString();

            projectList.Items.Clear();
            foreach (Project p in mDTE.Solution.Projects)
            {
                if (File.Exists(p.FullName))
                    projectList.Items.Add(p.UniqueName);
            }

            if (string.IsNullOrEmpty(currentSelection))
                currentSelection = GetStartupProjectName(mDTE);

            try
            {
                projectList.SelectedItem = mDTE.Solution.Projects.Item(currentSelection).UniqueName;
            }
            catch { }

            ReloadProjectSymbols();
        }

        private static string GetStartupProjectName(DTE myDTE)
        {
            string startupProjectName = string.Empty;
            SolutionBuild solutionBuild = myDTE.Solution.SolutionBuild;

            if ((solutionBuild == null) || (solutionBuild.StartupProjects == null))
                return startupProjectName;

            foreach (string el in (Array)solutionBuild.StartupProjects)
                startupProjectName += el;

            return startupProjectName;
        }

        private static string GetContentLocation(string contentName)
        {
            IVsExtensionManager extensionManagerService = Package.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;
            if (extensionManagerService == null)
                return null;

            IInstalledExtension dsExt = null;
            if (extensionManagerService.TryGetInstalledExtension(GuidList.guidDataSizeViewerPkgString, out dsExt) == false)
                return null;

            string contentPath = null;

            try
            {
                string contentRelativePath = dsExt.Content.Where(c => c.ContentTypeName == contentName).First().RelativePath;
                contentPath = Path.Combine(dsExt.InstallPath, contentRelativePath);
            }
            catch { }

            return contentPath;
        }

        private void refreshSymbolTable_Click(object sender, RoutedEventArgs e)
        {
            UpdateProjectList();
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            DataSizeViewerPackage.Package.ShowOptionPage(typeof(OptionsPage));
        }

        private void symbolSize_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            String field = column.Tag as String;

            if (mSymbolListSortColumn != null)
                AdornerLayer.GetAdornerLayer(mSymbolListSortColumn).Remove(mSymbolListArrowAdorner);

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (mSymbolListSortColumn == column && mSymbolListArrowAdorner.Direction == newDir)
                newDir = (newDir == ListSortDirection.Descending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            mSymbolListSortColumn = column;

            mSymbolListArrowAdorner = new ListViewSortArrowAdorner(mSymbolListSortColumn, newDir);
            AdornerLayer.GetAdornerLayer(mSymbolListSortColumn).Add(mSymbolListArrowAdorner);

            symbolSize.Items.SortDescriptions.Clear();
            symbolSize.Items.SortDescriptions.Add(new SortDescription(field, newDir));
        } 

        private void symbolSize_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tuple<String, int?> symbolFileAndLine = mSymbolParser.GetSymbolLocationAndLine(symbolSize.SelectedItem as ItemSize);

            if (File.Exists(symbolFileAndLine.Item1) && symbolFileAndLine.Item2.HasValue)
            {
                EnvDTE.Window w = mDTE.ItemOperations.OpenFile(symbolFileAndLine.Item1);

                if (w != null)
                    (w.Selection as EnvDTE.TextSelection).GotoLine(symbolFileAndLine.Item2.Value, true);
            }
            else
            {
                mDTE.StatusBar.Text = String.Format("Could not open file {0}", symbolFileAndLine.Item1);
            }
        }

        void symbolSize_CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            StringBuilder copyContent = new StringBuilder();

            foreach (ItemSize i in symbolSize.SelectedItems)
              copyContent.AppendLine(String.Format("{0}, {1}, {2}", i.Size, i.Storage, i.Name));

            Clipboard.SetText(copyContent.ToString());
        }

        void symbolSize_CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (symbolSize.Items.IsEmpty == false);
        }
    }
}
