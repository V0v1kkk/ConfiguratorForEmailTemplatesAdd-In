using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MHConfigurator.Models
{
    public class ButtonElement : PanelElement
    {
        public Image Image { get; private set; }
        public string ImageAsString
        {
            get
            {
                return Image == null ? null : ImageToBase64(Image, ImageFormat.Png);
            }
        }
        public string Label { get; private set; }
        public string SuperTip { get; private set; }
        public string ScreenTip { get; private set; }
        public int? TemplateNo { get; private set; }

        public ButtonElement(string name, string parentname, string label, string superTip, string screenTip, string imagestring, int? binding) : base(name, parentname)
        {
            Image = Base64ToImage(imagestring);
            Label = label;
            SuperTip = superTip;
            ScreenTip = screenTip;
            TemplateNo = binding;
        }
    }
}
