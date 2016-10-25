using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MHConfigurator.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MHConfigurator.ViewModels
{
    public class TemplateViewerViewModel: ViewModelBase 
    {
        private ObservableCollection<MailTemplate> _mailsTemplates;
        private MailTemplate _currentTemplate;
        private bool _processingMacrosesOn;
        private string _currentHtml;

        public ObservableCollection<MailTemplate> MailsTemplates
        {
            get { return _mailsTemplates; }
            private set
            {
                if(_mailsTemplates==value) return;
                _mailsTemplates = value;
                OnPropertyChanged();
            }
        }

        public MailTemplate CurrentTemplate
        {
            get { return _currentTemplate; }

            set
            {
                if(_currentTemplate==value) return;
                _currentTemplate = value;

                CurrentHtml = ProcessingMacrosesOn ? Helper.ReplaceMacros(value.TemplateBodyRusFix) : value.TemplateBodyRusFix;

                OnPropertyChanged();
            }
        }

        public bool ProcessingMacrosesOn
        {
            get { return _processingMacrosesOn; }
            set
            {
                if(_processingMacrosesOn==value) return;
                _processingMacrosesOn = value;
                OnPropertyChanged();
            }
        }


        public string CurrentHtml
        {
            get { return _currentHtml; }
            set
            {
                if(_currentHtml==value) return;
                _currentHtml = value;
                OnPropertyChanged();
            }
        }


        public ICommand AddCommand;
        public ICommand EditCommand;
        public ICommand DeleteCommand;
        

        public TemplateViewerViewModel()
        {
            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().GetMailTemplates());

            AddCommand = new RelayCommand(AddExecute, ()=>true, this);
            EditCommand = new RelayCommand(EditExecute, ()=>CurrentTemplate!=null, this);
            DeleteCommand = new RelayCommand(DeleteExecute, () => CurrentTemplate != null, this);
        }




        private void AddExecute()
        {
            
        }

        private void EditExecute()
        {
            
        }

        private void DeleteExecute()
        {
            
        }


    }
}