using System.Windows;
using System.Windows.Data;

namespace MHConfigurator.Views  
{
    public partial class MailPropertiesView : Window
    {
        public MailPropertiesView()
        {
            InitializeComponent();
            /* �� ������ ��������� �� ���������� ���������
            this.Loaded += delegate
            {
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    window.SetBinding(Window.MinHeightProperty, new Binding() { Source = this.MinHeight+100 });
                    window.SetBinding(Window.MinWidthProperty, new Binding() { Source = this.MinWidth+100 });
                }
            };*/
        }
    }
}
