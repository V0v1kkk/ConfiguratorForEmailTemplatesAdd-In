using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

// ReSharper disable CheckNamespace

namespace MHConfigurator.Models
{
    public abstract class PanelElement
    {
        protected PanelElement()
        { }
        protected PanelElement (string name, string parentname)
        {
            Name = name;
            ParentName = parentname;
            Childs = new List<PanelElement>();
        }

        public PanelElement Parent { get; protected set; }
        public List<PanelElement> Childs { get; protected set; }
        public string Name { get; protected set; }
        public string ParentName { get; protected set; }

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        protected static Image Base64ToImage(string base64String)
        {
            if (base64String == null) return null;
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    Image image = Image.FromStream(ms, true);
                    return image;
                }
            }
            catch (FormatException exception)
            {
                Logger.Error("Ошибка преобразования изображения, прочитанного из базы. Ошибка: " + exception.Message);
                return null;
            }
        }

        protected static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, format);
                    byte[] imageBytes = ms.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка преобразования изображения элемента интерфейса в base64 строку. Ошибка: "+ ex.Message);
                return null;
            }
            
        }

        static public List<PanelElement> CreateTree(List<PanelElement> flatList)
        {
            if (flatList == null) return null;

            var dic = flatList.ToDictionary(n => n.Name, n => n);
            var rootNodes = new List<PanelElement>();
            foreach(var node in flatList)
            {
                if (node.ParentName!=null)
                {
                    try
                    {
                        PanelElement parent = dic[node.ParentName];
                        node.Parent = parent;
                        parent.Childs.Add(node);
                    }
                    catch (KeyNotFoundException)
                    {
                        Logger.Error("Элемент интерфейса в базе данных не имеет родителя");
                    }
                }
                else
                {
                    rootNodes.Add(node);
                }
            }
            return rootNodes;
        }
    }
}
