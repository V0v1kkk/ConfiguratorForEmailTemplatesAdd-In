using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MugenMvvmToolkit.ViewModels;

namespace MHConfigurator.ViewModels
{
    class MailEditorViewModel : ViewModelBase
    {
        private string _bodyLink;
        private byte _upMargin;
        private byte _downMargin;
        private string _templateDescription;

        public string BodyLink
        {
            get { return _bodyLink; }
            set { _bodyLink = value; }
        }

        public byte UpMargin    
        {
            get { return _upMargin; }
            set { _upMargin = value; }
        }

        public byte DownMargin
        {
            get { return _downMargin; }
            set { _downMargin = value; }
        }

        public string TemplateDescription
        {
            get { return _templateDescription; }
            set { _templateDescription = value; }
        }

        public MailEditorViewModel()
        {
            
        }
    }

}
