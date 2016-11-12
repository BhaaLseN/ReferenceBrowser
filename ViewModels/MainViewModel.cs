﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Win32;
using ReferenceBrowser.ViewModels.Nodes;

namespace ReferenceBrowser.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand BrowseSolutionFile { get; }
        public RelayCommand OpenSolutionFile { get; }
        public RelayCommand RunUnusedSolutionAnalysis { get; }

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

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set { Set(ref _statusText, value); }
        }
        private double _statusPercentage;
        public double StatusPercentage
        {
            get { return _statusPercentage; }
            set { Set(ref _statusPercentage, value); }
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
            RunUnusedSolutionAnalysis = new RelayCommand(OnRunUnusedSolutionAnalysis);
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
        private static bool DontCareAboutThis(ISymbol symbol)
        {
            if (symbol == null)
                return true;
            // at this point, we only care about types being declared
            return !new[] { SymbolKind.NamedType }
                .Contains(symbol.Kind);
        }
        private void OnRunUnusedSolutionAnalysis()
        {
            var solutionNode = RootNodes?.OfType<SolutionNode>().FirstOrDefault();
            if (solutionNode == null)
                return;
            var solution = solutionNode.Solution;
            var documentNodes = GetAllSelectedDocumentNodes(RootNodes).ToArray();
            var documentSet = documentNodes.Select(n => n.Document).ToImmutableHashSet();
            int currentSymbol = 0;
            StatusText = $"{currentSymbol}/{documentSet.Count}";
            StatusPercentage = 0;
            Parallel.ForEach(documentNodes, async documentNode =>
            {
                var document = documentNode.Document;
                var documentsExceptSelf = documentSet.Except(new[] { document });
                var model = await document.GetSemanticModelAsync();
                var root = await document.GetSyntaxRootAsync();
                var documentReferences = new List<NodeBase>();
                foreach (var node in root.DescendantNodesAndSelf(n => !(n is ClassDeclarationSyntax) && !(n is InterfaceDeclarationSyntax)).OfType<TypeDeclarationSyntax>())
                {
                    var symbol = model.GetDeclaredSymbol(node);
                    if (DontCareAboutThis(symbol))
                        continue;
                    // exclude the current document to limit the search to "external" references;
                    // this assumes one class per file (otherwise results will be missing)
                    var allReferenceSymbols = await SymbolFinder.FindReferencesAsync(symbol, solution, documentsExceptSelf);
                    // it will still return matches inside the document (since there are members inside it),
                    // but their locations will be empty.
                    // TODO: search all documents and simply exclude the ones from the same class
                    //        to allow more than one class per file.
                    var referenceSymbols = allReferenceSymbols.Where(r => r.Locations.Any()).ToArray();
                    documentReferences.Add(new ReferenceSymbolNode(symbol, referenceSymbols));
                }
                // keep all existing nodes, except the reference nodes from a previous run
                documentNode.ChildNodes = documentNode.ChildNodes.Except(documentNode.ChildNodes.OfType<ReferenceSymbolNode>()).Concat(documentReferences).ToArray();
                IncrementStatus(documentSet, ref currentSymbol);
            });
        }

        private void IncrementStatus(ImmutableHashSet<Document> documentSet, ref int currentSymbol)
        {
            Interlocked.Increment(ref currentSymbol);
            StatusText = $"{currentSymbol}/{documentSet.Count}";
            StatusPercentage = currentSymbol * 100 / documentSet.Count;
        }

        private IEnumerable<DocumentNode> GetAllSelectedDocumentNodes(NodeBase[] nodes)
        {
            if (nodes == null)
                yield break;
            foreach (var node in nodes)
            {
                var documentNode = node as DocumentNode;
                if (documentNode?.Document != null)
                    yield return documentNode;
                foreach (var childDocumentNode in GetAllSelectedDocumentNodes(node.ChildNodes))
                    yield return childDocumentNode;
            }
        }
    }
}
