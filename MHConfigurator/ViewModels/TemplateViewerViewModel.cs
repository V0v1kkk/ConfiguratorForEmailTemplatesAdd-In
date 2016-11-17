using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MHConfigurator.Models;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Presenters;
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
        private Visibility _windowVisible = Visibility.Visible;
        private readonly IToastPresenter _toastPresenter;
        private string _busyMessage;
        private bool _viewBusy;


        #region Propertyes
        /// <summary>
        /// Поле отвечает за видимость окна
        /// </summary>
        public Visibility WindowVisibility
        {
            get { return _windowVisible== Visibility.Visible ? Visibility.Visible : Visibility.Collapsed; }
            private set
            {
                if (value == _windowVisible ) return;
                _windowVisible = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Коллекция всех шаблонов из БД
        /// </summary>
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

        /// <summary>
        /// Текущий шаблон с которым ведётся работа
        /// </summary>
        public MailTemplate CurrentTemplate
        {
            get { return _currentTemplate; }

            set
            {
                if(_currentTemplate==value) return;
                _currentTemplate = value;

                if(value!=null) CurrentHtml = ProcessingMacrosesOn ? Helper.ReplaceMacros(value.TemplateBodyRusFix) : value.TemplateBodyRusFix;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Настройка: вкл/выкл обработка максосов в шаблонах
        /// </summary>
        public bool ProcessingMacrosesOn
        {
            get { return _processingMacrosesOn; }
            set
            {
                if(_processingMacrosesOn==value) return;
                _processingMacrosesOn = value;
                RefrashCurrentTemplate();
                OnPropertyChanged();
            }
        }

        

        /// <summary>
        /// Html тело текущего шаблона
        /// </summary>
        public string CurrentHtml
        {
            get { return _currentHtml; }
            private set
            {
                if(_currentHtml==value) return;
                _currentHtml = value;
                OnPropertyChanged();
            }
        }

        public bool ViewBusy
        {
            get { return _viewBusy; }
            set
            {
                if (value == _viewBusy) return;
                _viewBusy = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ICommand AddCommand;
        public ICommand EditCommand;
        public ICommand DeleteCommand;
        


        public TemplateViewerViewModel(IToastPresenter toastPresenter)
        {
            //MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().MailTemplates); //test

            AddCommand = new RelayCommand(AddExecute, ()=>!ViewBusy, this);
            EditCommand = new RelayCommand(EditExecute, ()=> (CurrentTemplate != null) && !ViewBusy, this );
            DeleteCommand = new RelayCommand(DeleteExecute, () => (CurrentTemplate != null) && !ViewBusy, this);

            _toastPresenter = toastPresenter;
        }

        public void Setup() ////test
        {
            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().MailTemplates);
        }




        private async void AddExecute()
        {
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (var htmlEditor = GetViewModel<MailEditorViewModel>())
                    {
                        htmlEditor.ProcessMacroses = ProcessingMacrosesOn; //Настройка дочерней viewmodel'и

                        WindowVisibility = Visibility.Collapsed;
                        await htmlEditor.ShowAsync();

                        if (htmlEditor.TemplateChanged) //В случае изменений переимпортируем шаблоны (вдруг изменилось описание)
                        {
                            RefrashTemplates();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo: Залогировать
                }
                finally
                {
                    WindowVisibility = Visibility.Visible;
                }
            });
        }


        private async void EditExecute()
        {
            await Task.Factory.StartNew(async () =>
            {
                ViewBusy = true;
                try
                {
                    using (var htmlEditor = GetViewModel<MailEditorViewModel>())
                    {
                        htmlEditor.MailEditorViewModelSetup(CurrentTemplate.TemplateId, ProcessingMacrosesOn); //Настройка дочерней viewmodel'и

                        WindowVisibility = Visibility.Collapsed;
                        ViewBusy = false;
                        IAsyncOperation<bool> asyncOperation = htmlEditor.ShowAsync();
                        await asyncOperation;
                        WindowVisibility = Visibility.Visible;
                        if (htmlEditor.TemplateChanged == true) //В случае изменений переимпортируем шаблоны (вдруг изменилось описание)
                        {
                            RefrashTemplates();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo: Залогировать
                }
                finally
                {
                    WindowVisibility = Visibility.Visible;
                }
            });
        }

        private async void DeleteExecute()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (CurrentTemplate != null)
                    {
                        if (DAL.GetDAL().DeleteMailTemplate(CurrentTemplate)) //Если удаление из базы успешно
                        {
                            _toastPresenter?.ShowAsync($"HTML-шаблон {CurrentTemplate.FullDescription} удален", ToastDuration.Short, ToastPosition.Center);
                            MailsTemplates.Remove(CurrentTemplate);
                            CurrentTemplate = null;
                        }
                        else
                        {
                            _toastPresenter?.ShowAsync("Не удалось удалить HTML-шаблон", ToastDuration.Short, ToastPosition.Center);
                        }
                    }
                }
                catch (Exception exception)
                {
                    //todo: Залогировать
                }
            });
        }

        private void RefrashTemplates()
        {
            WindowVisibility = Visibility.Visible; //При скрытом окне переотрисовка не происходит

            long tempcurrenttemplate = 0;
            if (CurrentTemplate != null)
            {
                tempcurrenttemplate = CurrentTemplate.TemplateId;
                CurrentTemplate = null;
            }

            //todo: Обернуть в багдрауд воркер и выводит сообщение о занятости
            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().MailTemplates);


            if (tempcurrenttemplate != 0) CurrentTemplate = MailsTemplates.FirstOrDefault(template => template.TemplateId == tempcurrenttemplate);
            OnPropertyChanged("MailsTemplates");
            OnPropertyChanged("CurrentTemplate");
        }

        private void RefrashCurrentTemplate()
        {
            WindowVisibility = Visibility.Visible; //При скрытом окне переотрисовка не происходит

            if (CurrentTemplate==null) return;
            long tempcurrenttemplate = CurrentTemplate.TemplateId;
            CurrentTemplate = null;
            CurrentTemplate = MailsTemplates.FirstOrDefault(template => template.TemplateId == tempcurrenttemplate);
            OnPropertyChanged("CurrentTemplate");
        }


    }
}