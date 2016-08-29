using System;
using System.Collections.Generic;
using System.Linq;

namespace DTI.SourceControl.Svn
{
    public class HierarchyNode
    {
        private HierarchyNode _parent;

        public FileStatus Value;
        public List<HierarchyNode> Children;
        public HierarchyNode Meta;
        public bool Foldout = true;
        public bool Committable = true;

        public HierarchyNode Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                if (value.Children == null)
                    value.Children = new List<HierarchyNode>();
                value.Children.Add(this);
            }
        }

        public bool Commit
        {
            get { return Value.Commit; }
            set
            {
                if (Value.Commit != value)
                {
                    Value.Commit = value;
                    if (value)
                    {
                        if (Parent != null)
                            Parent.Commit = true;
                    }
                    else
                    {
                        if (Children != null)
                            Children = Children.Select(x =>
                            {
                                x.Commit = false;
                                return x;
                            }).ToList();
                    }
                }
            }
        }

        public HierarchyNode(FileStatus value)
        {
            Value = value;
        }

        public HierarchyNode(FileStatus value, HierarchyNode parent)
        {
            Value = value;
            Parent = parent;
        }

        public HierarchyNode(FileStatus value, HierarchyNode parent, bool committable)
        {
            Value = value;
            Parent = parent;
            Committable = committable;
        }
    }
}