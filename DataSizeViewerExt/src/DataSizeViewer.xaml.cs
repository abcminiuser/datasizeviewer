using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using Atmel.Studio.Extensibility.Toolchain;
using Atmel.Studio.Services;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Text;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public partial class DataSizeViewerUI : UserControl
    {
        private readonly DTE myDTE;

        private readonly IToolchainService mToolchainService;

        private BuildEvents mBuildEvents;
        private SolutionEvents mSolutionEvents;

        private SymbolSizeParser mSymbolParser;

        public DataSizeViewerUI()
        {
            InitializeComponent();

            myDTE = Package.GetGlobalService(typeof(DTE)) as DTE;

            mBuildEvents = myDTE.Events.BuildEvents;
            mBuildEvents.OnBuildDone += (Scope, Action) => ReloadProjectSymbols();

            mSolutionEvents = myDTE.Events.SolutionEvents;
            mSolutionEvents.Opened += () => UpdateProjectList();
            mSolutionEvents.AfterClosing += () => UpdateProjectList();
            mSolutionEvents.ProjectAdded += (p) => UpdateProjectList();
            mSolutionEvents.ProjectRemoved += (p) => UpdateProjectList();
            mSolutionEvents.ProjectRenamed += (p, s) => UpdateProjectList();

            mToolchainService = ATServiceProvider.ToolchainService;

            projectList.SelectionChanged += (s, e) => ReloadProjectSymbols();

            mSymbolParser = new SymbolSizeParser();
            symbolSize.ItemsSource = mSymbolParser.symbolSizes;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(mSymbolParser.symbolSizes);
            dataView.GroupDescriptions.Add(new PropertyGroupDescription("Storage"));
        }

        private void ReloadProjectSymbols()
        {
            mSymbolParser.ClearSymbols();

            if (projectList.SelectedItem == null)
                return;

            String projectName = (String)projectList.SelectedItem.ToString();
            if (String.IsNullOrEmpty(projectName))
                return;

            Project project = null;
            try
            {
                project = myDTE.Solution.Item(projectName);
            }
            catch { }

            if (project == null)
                return;

            IProjectHandle projectNode = project.Object as IProjectHandle;
            if (projectNode == null)
                return;

            if (mToolchainService == null)
                return;

            string elfPath = null;
            string toolchainNMPath = null;

            ICCompilerToolchain toolchainC = mToolchainService.GetCCompilerToolchain(projectNode.ToolchainName, projectNode.GetProperty("ToolchainFlavour"));
            if (toolchainC != null)
            {
                elfPath = Path.Combine(projectNode.GetProperty("OutputDirectory"), projectNode.GetProperty("OutputFilename") + ".elf");
                toolchainNMPath = Path.Combine(Path.GetDirectoryName(toolchainC.Compiler.FullPath), Path.GetFileName(toolchainC.Compiler.FullPath).Replace("gcc", "nm"));
            }

            ICppCompilerToolchain toolchainCpp = mToolchainService.GetCppCompilerToolchain(projectNode.ToolchainName, projectNode.GetProperty("ToolchainFlavour"));
            if (toolchainCpp != null)
            {
                elfPath = Path.Combine(projectNode.GetProperty("OutputDirectory"), projectNode.GetProperty("OutputFilename") + ".elf");
                toolchainNMPath = Path.Combine(Path.GetDirectoryName(toolchainC.Compiler.FullPath), Path.GetFileName(toolchainCpp.CppCompiler.FullPath).Replace("g++", "nm"));
            }

            if (File.Exists(elfPath) && File.Exists(toolchainNMPath))
                Dispatcher.Invoke(new Action(() => mSymbolParser.ReloadSymbols(elfPath, toolchainNMPath, DataSizeViewerPackage.Options.VerifyLocations)));
        }

        private void UpdateProjectList()
        {
            projectList.Items.Clear();
            foreach (Project p in myDTE.Solution.Projects)
                projectList.Items.Add(p.UniqueName);

            try
            {
                projectList.SelectedItem = myDTE.Solution.Projects.Item(GetStartupProjectName(myDTE)).UniqueName;
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

            Array startprojects = (Array)solutionBuild.StartupProjects;
            if (startprojects.GetLength(0) > 1)
                return startupProjectName;

            foreach (string el in (Array)solutionBuild.StartupProjects)
                startupProjectName += el;

            return startupProjectName;
        }

        private void symbolSize_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (symbolSize.SelectedItem == null)
                return;

            string symbolLocation = (symbolSize.SelectedItem as ItemSize).Location;

            if ((symbolLocation == null) || (symbolLocation.Contains(':') == false))
                return;

            string fileName = symbolLocation.Substring(0, symbolLocation.LastIndexOf(':'));

            int fileLine = -1;
            bool fileLineValid = int.TryParse(symbolLocation.Substring(symbolLocation.LastIndexOf(':') + 1), out fileLine);

            if (File.Exists(fileName) && fileLineValid)
            {
                EnvDTE.Window w = myDTE.ItemOperations.OpenFile(Path.GetFullPath(fileName));

                if (w != null)
                    (w.Selection as EnvDTE.TextSelection).GotoLine(fileLine, true);
            }
            else
            {
                myDTE.StatusBar.Text = String.Format("Could not open file {0}", symbolLocation);
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
            e.CanExecute = (symbolSize.Items.Count > 0);
        }

        private void RefreshSymbolTable_Click(object sender, RoutedEventArgs e)
        {
            ReloadProjectSymbols();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            DataSizeViewerPackage.Package.ShowOptionPage(typeof(OptionsPage));
        }    
    }
}
