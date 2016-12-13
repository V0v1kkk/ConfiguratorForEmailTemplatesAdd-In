using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardcodet.Wpf.GenericTreeView;
using MHConfigurator.Models;

namespace MHConfigurator.AdditionalCalsses
{
    public class PanelElementsTree : TreeViewBase<PanelElement>
    {
        public override string GetItemKey(PanelElement item)
        {
            return item.Name;
        }

        public override ICollection<PanelElement> GetChildItems(PanelElement parent)
        {
            return parent.Childs;
        }

        public override PanelElement GetParentItem(PanelElement item)
        {
            return item.Parent;
        }
    }
}
