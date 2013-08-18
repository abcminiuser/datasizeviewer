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
            mSolutionEvents.Opened += () => ReloadProjectSymbols();
            mSolutionEvents.AfterClosing += () => ReloadProjectSymbols();

            mToolchainService = ATServiceProvider.ToolchainService;

            mSymbolParser = new SymbolSizeParser();
            symbolSize.ItemsSource = mSymbolParser.symbolSizes;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(mSymbolParser.symbolSizes);
            dataView.GroupDescriptions.Add(new PropertyGroupDescription("Storage"));

            this.Loaded += (sender, e) => ReloadProjectSymbols();
        }

        private void ReloadProjectSymbols()
        {
            mSymbolParser.ClearSymbols();

            string startupProjectName = GetStartupProjectName(myDTE);
            if (string.IsNullOrEmpty(startupProjectName))
                return;

            Project project = myDTE.Solution.Projects.Item(startupProjectName);
            if (project == null)
                return;

            IProjectHandle projectNode = project.Object as IProjectHandle;
            if (projectNode == null)
                return;

            projectName.Text = projectNode.Name;

            if (mToolchainService == null)
                return;

            ICCompilerToolchain toolchainC = mToolchainService.GetCCompilerToolchain(projectNode.ToolchainName, projectNode.GetProperty("ToolchainFlavour"));
            if (toolchainC != null)
            {
                string elfPath = Path.Combine(projectNode.GetProperty("OutputDirectory"), projectNode.GetProperty("OutputFilename") + ".elf");
                string toolchainPath = Path.GetDirectoryName(toolchainC.Compiler.FullPath);
                string toolchainNMPath = Path.Combine(toolchainPath, Path.GetFileName(toolchainC.Compiler.FullPath).Replace("gcc", "nm"));

                if (File.Exists(elfPath) && File.Exists(toolchainNMPath))
                    mSymbolParser.ReloadSymbols(elfPath, toolchainNMPath);
            }

            ICppCompilerToolchain toolchainCpp = mToolchainService.GetCppCompilerToolchain(projectNode.ToolchainName, projectNode.GetProperty("ToolchainFlavour"));
            if (toolchainCpp != null)
            {
                string elfPath = Path.Combine(projectNode.GetProperty("OutputDirectory"), projectNode.GetProperty("OutputFilename") + ".elf");
                string toolchainPath = Path.GetDirectoryName(toolchainCpp.CppCompiler.FullPath);
                string toolchainNMPath = Path.Combine(toolchainPath, Path.GetFileName(toolchainCpp.CppCompiler.FullPath).Replace("g++", "nm"));

                if (File.Exists(elfPath) && File.Exists(toolchainNMPath))
                    mSymbolParser.ReloadSymbols(elfPath, toolchainNMPath);
            }
        }

        private static string GetStartupProjectName(DTE myDTE)
        {
            string startupProjectName = string.Empty;
            SolutionBuild solutionBuild = myDTE.Solution.SolutionBuild;

            if ((solutionBuild == null)|| (solutionBuild.StartupProjects == null))
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
            string copyContent = String.Empty;

            foreach (ItemSize i in symbolSize.SelectedItems)
            {
                copyContent += String.Format("{0}, {1}, {2}", i.Size, i.Storage, i.Name);
                copyContent += Environment.NewLine;
            }

            Clipboard.SetText(copyContent);
        }

        void symbolSize_CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (symbolSize.Items.Count > 0);
        }
    }
}
