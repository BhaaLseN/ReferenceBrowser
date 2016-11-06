using GalaSoft.MvvmLight;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public abstract class NodeBase : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private NodeBase[] _childNodes = new NodeBase[0];
        public NodeBase[] ChildNodes
        {
            get { return _childNodes; }
            set { Set(ref _childNodes, value ?? new NodeBase[0]); }
        }
        protected NodeBase(string name)
        {
            Name = name;
        }
    }
}
