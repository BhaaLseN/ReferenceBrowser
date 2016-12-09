using System;
using System.Linq;
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

        private bool? _isChecked = true;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (Set(ref _isChecked, value) && _isChecked.HasValue)
                {
                    // tick/untick all child nodes, unless we're indeterminate
                    Array.ForEach(_childNodes, n => n.IsChecked = value);

                    // tick/untick/indeterminate the parent node, if there is one
                    _parentNode?.CheckSelfAndParents();
                }
            }
        }

        private void CheckSelfAndParents()
        {
            bool? isParentChecked;
            if (_childNodes.Any())
            {
                if (_childNodes.All(n => n.IsChecked.IsFalse()))
                    isParentChecked = false;
                else if (_childNodes.All(n => n.IsChecked.IsTrue()))
                    isParentChecked = true;
                else
                    isParentChecked = null;
            }
            else
            {
                // no nodes at all
                isParentChecked = false;
            }

            // no need to traverse further if we didn't change state
            if (!Set(nameof(IsChecked), ref _isChecked, isParentChecked))
                return;

            _parentNode?.CheckSelfAndParents();
        }

        private NodeBase[] _childNodes = new NodeBase[0];
        public NodeBase[] ChildNodes
        {
            get { return _childNodes; }
            set
            {
                if (Set(ref _childNodes, value ?? new NodeBase[0]))
                    Array.ForEach(_childNodes, n => n.ParentNode = this);
            }
        }

        private NodeBase _parentNode;
        public NodeBase ParentNode
        {
            get { return _parentNode; }
            set { Set(ref _parentNode, value); }
        }

        protected NodeBase(string name)
        {
            Name = name;
        }
    }
}
