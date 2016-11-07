using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHConfigurator.ViewModels;
using MugenMvvmToolkit;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace MHConfigurator
{
    public class Starter : MvvmApplication
    {
        



        public override Type GetStartViewModelType()
        {
            return typeof(MainViewModel);

            //return typeof(TemplateViewerViewModel);


            //return typeof (MailPropertiesViewModel);
            //return typeof(MailEditorViewModel);
        }
    }
}
