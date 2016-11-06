using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Win32;
using ReferenceBrowser.ViewModels.Nodes;

namespace ReferenceBrowser.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand BrowseSolutionFile { get; }
        public RelayCommand OpenSolutionFile { get; }

        private string _solutionFilePath;
        public string SolutionFilePath
        {
            get { return _solutionFilePath; }
            set
            {
                if (Set(ref _solutionFilePath, value))
                    OpenSolutionFile.RaiseCanExecuteChanged();
            }
        }

        private NodeBase[] _rootNodes;
        public NodeBase[] RootNodes
        {
            get { return _rootNodes; }
            set { Set(ref _rootNodes, value); }
        }

        public MainViewModel()
        {
            BrowseSolutionFile = new RelayCommand(OnBrowseSolutionFile);
            OpenSolutionFile = new RelayCommand(OnOpenSolutionFile, CanOpenSolutionFile);
        }
        private void OnBrowseSolutionFile()
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".sln",
                Filter = "Visual Studio Solution file (*.sln)|*.sln",
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Select a Visual Studio Solution file to analyze",
            };
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                SolutionFilePath = ofd.FileName;
                Task.Run(new Action(OnOpenSolutionFile));
            }
        }
        private async void OnOpenSolutionFile()
        {
            var solutionWorkspace = MSBuildWorkspace.Create();
            var solution = await solutionWorkspace.OpenSolutionAsync(SolutionFilePath);
            var solutionNode = new SolutionNode(solution);
            RootNodes = new[] { solutionNode };
        }
        private bool CanOpenSolutionFile()
        {
            return !string.IsNullOrWhiteSpace(SolutionFilePath) && File.Exists(SolutionFilePath);
        }
    }
}
