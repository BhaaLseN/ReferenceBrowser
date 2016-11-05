using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

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
            }
        }
        private void OnOpenSolutionFile()
        {
            // TODO: load SolutionFilePath and create a tree
        }
        private bool CanOpenSolutionFile()
        {
            return !string.IsNullOrWhiteSpace(SolutionFilePath) && File.Exists(SolutionFilePath);
        }
    }
}
