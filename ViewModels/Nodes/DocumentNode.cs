using Microsoft.CodeAnalysis;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public class DocumentNode : NodeBase
    {
        public Document Document { get; }

        public DocumentNode(Document document)
            : base(document?.Name)
        {
            Document = document;
        }
    }
}
