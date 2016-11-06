using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public class SolutionNode : NodeBase
    {
        private readonly Solution _solution;

        public SolutionNode(Solution solution)
            : base($"Solution '{Path.GetFileNameWithoutExtension(solution?.FilePath)}' ({solution?.Projects.Count()} project(s))")
        {
            _solution = solution;
            Task.Run(new Action(PopulateProjects));
        }

        private void PopulateProjects()
        {
            if (_solution == null)
                return;

            var projects = new List<ProjectNode>();
            foreach (var solutionProject in _solution.Projects)
            {
                var project = new ProjectNode(solutionProject);
                projects.Add(project);
            }
            ChildNodes = projects.ToArray();
        }
    }
}
