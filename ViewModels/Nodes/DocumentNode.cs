using Microsoft.CodeAnalysis;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public class DocumentNode : NodeBase
    {
        private readonly Document _document;

        public DocumentNode(Document document)
            : base(document?.Name)
        {
            _document = document;
        }
    }
}
