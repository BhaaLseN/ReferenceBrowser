using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public class ProjectNode : NodeBase
    {
        private readonly Project _project;

        public ProjectNode(Project project)
            : base(project?.Name)
        {
            _project = project;
            Task.Run(new Action(PopulateFolders));
        }

        private void PopulateFolders()
        {
            if (_project == null)
                return;

            var folders = new Dictionary<string, FolderHolder>();
            var allDocumentsWithPath = _project.Documents.GroupBy(p => MakeFullPath(p.Folders)).OrderBy(k => k.Key).ToArray();
            foreach (var folderGroup in allDocumentsWithPath)
            {
                var subFolders = folderGroup.First().Folders;
                var subFolderPath = new List<string>();
                FolderHolder parentFolder = null;
                foreach (string subFolderName in subFolders)
                {
                    subFolderPath.Add(subFolderName);
                    string fullPath = MakeFullPath(subFolderPath);
                    FolderHolder thisFolder;
                    if (!folders.TryGetValue(fullPath, out thisFolder))
                    {
                        thisFolder = new FolderHolder(subFolderName, fullPath);
                        folders[fullPath] = thisFolder;
                        if (parentFolder != null)
                            parentFolder.SubFolders.Add(thisFolder);
                        else
                            thisFolder.IsRoot = true;
                    }
                    parentFolder = thisFolder;
                }
                if (parentFolder != null)
                    parentFolder.Documents.AddRange(folderGroup);
            }

            var allRootFolders = folders.Values.Where(f => f.IsRoot).ToArray();
            var rootFolders = new List<NodeBase>();
            foreach (var rootFolder in allRootFolders)
            {
                rootFolders.Add(rootFolder.ToFolderNode());
            }
            var rootDocuments = allDocumentsWithPath.FirstOrDefault(g => g.Key == "");
            if (rootDocuments != null)
            {
                foreach (var rootDocument in rootDocuments)
                {
                    rootFolders.Add(new DocumentNode(rootDocument));
                }
            }
            ChildNodes = rootFolders.ToArray();
        }

        private static string MakeFullPath(IEnumerable<string> subFolderPath)
        {
            return string.Join("\\", subFolderPath);
        }

        private sealed class FolderHolder
        {
            public string Name { get; }
            public string FullPath { get; }
            public bool IsRoot { get; set; }
            public List<FolderHolder> SubFolders { get; } = new List<FolderHolder>();
            public List<Document> Documents { get; } = new List<Document>();

            public FolderHolder(string name, string fullPath)
            {
                Name = name;
                FullPath = fullPath;
            }
            public NodeBase ToFolderNode()
            {
                var result = new FolderNode(Name);
                var childNodes = new List<NodeBase>();

                foreach (var childNode in SubFolders)
                {
                    childNodes.Add(childNode.ToFolderNode());
                }
                foreach (var document in Documents)
                {
                    childNodes.Add(new DocumentNode(document));
                }

                result.ChildNodes = childNodes.ToArray();

                return result;
            }
        }
    }
}
