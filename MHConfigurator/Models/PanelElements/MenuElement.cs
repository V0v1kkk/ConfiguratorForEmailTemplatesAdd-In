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
    public class MenuElement : PanelElement
    {
        public Image Image { get; private set; }
        public string ImageAsString
        {
            get
            {
                return Image==null ? null : ImageToBase64(Image, ImageFormat.Png);
            }
        }

        public string Label { get; private set; }
        public string SuperTip { get; private set; }
        public string ScreenTip { get; private set; }

        public MenuElement(string name, string parentname, string label, string superTip, string screenTip, string imagestring)  : base(name, parentname)
        {
            Image = Base64ToImage(imagestring);
            SuperTip = superTip;
            ScreenTip = screenTip;
            Label = label;
        }
    }
}
