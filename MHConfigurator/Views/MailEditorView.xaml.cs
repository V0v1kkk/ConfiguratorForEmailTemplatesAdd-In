using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MHConfigurator.Views
{
    /// <summary>
    /// Логика взаимодействия для MailEditorView.xaml
    /// </summary>
    public partial class MailEditorView : Window
    {
        public MailEditorView()
        {
            InitializeComponent();
            /* На случай переделки на страничную навигацию
            this.Loaded += delegate
            {
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    window.SetBinding(Window.MinHeightProperty, new Binding() { Source = this.MinHeight + 100 });
                    window.SetBinding(Window.MinWidthProperty, new Binding() { Source = this.MinWidth + 100 });
                }
            };*/
        }
    }
}
