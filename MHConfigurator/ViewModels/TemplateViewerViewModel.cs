using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
        private bool _windowVisible = true;
        private readonly IToastPresenter _toastPresenter;


        #region Propertyes
        /// <summary>
        /// Поле отвечает за видимость окна
        /// </summary>
        public Visibility WindowVisibility
        {
            get { return _windowVisible ? Visibility.Visible : Visibility.Collapsed; }
            private set
            {
                if (value == Visibility.Visible && _windowVisible) return;
                if (value != Visibility.Visible && !_windowVisible) return;

                if (value == Visibility.Visible) _windowVisible = true;
                else if (value == Visibility.Collapsed) _windowVisible = false;
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

        #endregion

        public ICommand AddCommand;
        public ICommand EditCommand;
        public ICommand DeleteCommand;
        


        public TemplateViewerViewModel(IToastPresenter toastPresenter)
        {
            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().GetMailTemplates());

            AddCommand = new RelayCommand(AddExecute, ()=>true, this);
            EditCommand = new RelayCommand(EditExecute, ()=>CurrentTemplate!=null, this);
            DeleteCommand = new RelayCommand(DeleteExecute, () => CurrentTemplate != null, this);

            _toastPresenter = toastPresenter;
        }




        private async void AddExecute()
        {
            try
            {
                using (var htmlEditor = GetViewModel<MailEditorViewModel>())
                {
                    htmlEditor.ProcessMacroses = ProcessingMacrosesOn; //Настройка дочерней viewmodel'и

                    WindowVisibility = Visibility.Collapsed;
                    //IAsyncOperation<bool> asyncOperation = htmlEditor.ShowAsync();
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
        }

        private async void EditExecute()
        {
            //var htmlEditor = GetViewModel<IMailEditorViewModel>(); // todo: Найти способ управлять DI-контейнером, обернутым в MugenFamework
            try
            {
                using (var htmlEditor = GetViewModel<MailEditorViewModel>())
                {
                    htmlEditor.MailEditorViewModelSetup(CurrentTemplate.TemplateId, ProcessingMacrosesOn); //Настройка дочерней viewmodel'и

                    WindowVisibility = Visibility.Collapsed;
                    IAsyncOperation<bool> asyncOperation = htmlEditor.ShowAsync();
                    await asyncOperation;

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
                if(WindowVisibility != Visibility.Visible) WindowVisibility = Visibility.Visible;
            }
        }

        

        private void DeleteExecute()
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
        }


        private void RefrashTemplates()
        {
            WindowVisibility = Visibility.Visible; //При скрытом окне переотрисовка не произойдёт

            long tempcurrenttemplate = 0;
            if (CurrentTemplate != null)
            {
                tempcurrenttemplate = CurrentTemplate.TemplateId;
                CurrentTemplate = null;
            }
            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().GetMailTemplates());
            if (tempcurrenttemplate != 0) CurrentTemplate = MailsTemplates.FirstOrDefault(template => template.TemplateId == tempcurrenttemplate);
            OnPropertyChanged("MailsTemplates");
            OnPropertyChanged("CurrentTemplate");
        }

        private void RefrashCurrentTemplate()
        {
            WindowVisibility = Visibility.Visible; //При скрытом окне переотрисовка не произойдёт

            if (CurrentTemplate==null) return;
            long tempcurrenttemplate = CurrentTemplate.TemplateId;
            CurrentTemplate = null;
            CurrentTemplate = MailsTemplates.FirstOrDefault(template => template.TemplateId == tempcurrenttemplate);
            OnPropertyChanged("CurrentTemplate");
        }


    }
}