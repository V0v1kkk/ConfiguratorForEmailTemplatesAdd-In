using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MHConfigurator.Models
{
    public class GroupElement : PanelElement
    {
        public string Label { get; private set; }
        public GroupElement(string name, string parentname, string label) : base(name, parentname)
        {
            Label = label;
        }
    }
}
