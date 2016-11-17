using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvmToolkit;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHConfigurator.Models;
using Microsoft.Win32;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MHConfigurator.ViewModels
{

    //class MailEditorViewModel : ViewModelBase, INavigableViewModel, IMailEditorViewModel
    class MailEditorViewModel : ValidatableViewModel, INavigableViewModel
    {
        private MailTemplateEditable _templateEditable;

        public ICommand OpenFileCommand;
        public ICommand SaveCommand;

        private string _filePath;
        private List<MailTemplate> _mailsTemplatesEmpty;
        private MailTemplate _currentEmptyTemplate;
        private bool _workWithExistingTemplate;
        private bool _workmodeAlredySet;
        private int _newNumberForTemplate;

        private readonly IToastPresenter _toastPresenter;

        


        #region Properties

        /// <summary>
        /// Обсулжаевает поле, в которое пользователь может ввести желаемый номер шаблона, который
        /// будет использоваться при сохранении шаблона
        /// todo: При работе с имеющимся шаблоном поле не должно быть доступно для ввода!!!
        /// </summary>
        public string NewEmptyMailTemplate
        {
            private get { return _newNumberForTemplate.ToString(); }
            set
            {
                if (CurrentEmptyTemplate != null)
                {
                    _newNumberForTemplate = 0;
                    return;
                }

                int id;
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out id))
                {
                    _newNumberForTemplate = id;
                }
                else
                {
                    _newNumberForTemplate = 0;
                }
            }
        }

        /// <summary>
        /// Список оъектов-шаблонов писем без html тела
        /// Лучше использовать в ComboBox'e
        /// </summary>
        public List<MailTemplate> MailsTemplatesEmpty
        {
            get { return _mailsTemplatesEmpty; }
            private set
            {
                if (Equals(value, _mailsTemplatesEmpty)) return;
                _mailsTemplatesEmpty = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Поле для обуслуживания указателя на конкретны имеющийся MailTemplate 
        /// Используется при сохранении шаблона
        /// Лучше использовать в ComboBox'e
        /// </summary>
        public MailTemplate CurrentEmptyTemplate
        {
            get { return _currentEmptyTemplate; }
            set
            {
                if (Equals(value, _currentEmptyTemplate)) return;
                _currentEmptyTemplate = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Строка отображающая путь к текущему выбранному для загрузки файлу, или показывающа "загружен файл из базы" если редактируется готовый шаблон
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            private set
            {
                if (value == _filePath) return;
                _filePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// !!!
        /// Текущий задекарированный класс MailTemplate.
        /// С ним и производится работа в данной форме.
        /// </summary>
        public MailTemplateEditable TemplateEditable
        {
            get { return _templateEditable; }
            set
            {
                _templateEditable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Обслуживает информацию о том идёт работа с имеющимся шаблоном или новым
        /// Значение устаналивается 1 раз за жизненый цикл
        /// </summary>
        public bool WorkWithExistingTemplate
        {
            get { return _workWithExistingTemplate; }
            set
            {
                if (value == _workWithExistingTemplate) return;
                if(_workmodeAlredySet) return; //Если режим работы уже выбран, то изменять его уже нельзя

                FilePath = value ? "Письмо загружено из базы данных" : "";
                _workWithExistingTemplate = value;
                OnPropertyChanged();

                _workmodeAlredySet = true; //Фиксируем информацию о том, что режим работы выбран
            }
        }

        /// <summary>
        /// Свойство для зранения резервной копии объекта TemplateEditable,
        /// с которым и производится работа в окне
        /// </summary>
        private MailTemplate OriginlTemplate { get; set; }

        /// <summary>
        /// Настройка пользователя: нужно ли обрабатывать макросы
        /// </summary>
        public bool ProcessMacroses { get; set; }

        /// <summary>
        /// Свойство только для чтения!
        /// Текущий html-код шаблона
        /// todo: Нужна проброска информации о том, что нужно использовать поле медели с региональным фиксом (неренести региональное поле сюда)
        /// </summary>
        public string Html
        {
            get
            {
                if (TemplateEditable?.TemplateBodyRusFix == null) return "";
                if (ProcessMacroses) return Helper.ReplaceMacros(TemplateEditable.TemplateBodyRusFix);
                return TemplateEditable.TemplateBodyRusFix;   
            }
        }
       
        /// <summary>
        /// Пробрасывает во viewmodel описание HTML-шаблона для возможности валидации
        /// </summary>
        public string TemplateDescription
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TemplateEditable?.TemplateDescription))
                {
                    Validator.ClearErrors("TemplateDescription");
                    return TemplateEditable.TemplateDescription;
                }
                Validator.SetErrors("TemplateDescription", "Имя не должно быть пустым");
                return "";
            }
            set
            {
                if(string.IsNullOrWhiteSpace(value)) Validator.SetErrors("TemplateDescription", "Имя не должно быть пустым");
                else Validator.ClearErrors("TemplateDescription");

                if (TemplateEditable != null) TemplateEditable.TemplateDescription = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Хранит информацию о том, были ли записанные изменения в БД в текущем сеансе
        /// </summary>
        public bool TemplateChanged { get; private set; }

        #endregion

        #region Constructor

        public MailEditorViewModel(IToastPresenter toastPresenter)
        {

            OpenFileCommand=new RelayCommand(OpenFileExecute);
            SaveCommand = new RelayCommand(SaveExecute, SaveCanExexute, this);
            //При сохранении будет производится проверка кастомного имени шаблона, если оно не 0 ofc

            OriginlTemplate = new MailTemplate();
            TemplateEditable = new MailTemplateEditable();
            TemplateEditable.PropertyChanged += TemplateEditableOnChanged;

            MailsTemplatesEmpty = DAL.GetDAL().EmptyMailTemplates;

            _toastPresenter = toastPresenter;

            TemplateDescription = ""; // Запускаем валидатор на поле TemplateDescription
        }

        #endregion


        #region Settings methods

        /// <summary>
        /// Публичный метод для настройки формы на работу и с имеющимся объектом
        /// </summary>
        /// <param name="templateId">ID объекта для работы</param>
        /// <param name="enableMacrosesProcessing">Нужно ли включить обработку максросов</param>
        public void MailEditorViewModelSetup(int templateId, bool enableMacrosesProcessing = false)
        {
            MailTemplate mailTemplate = DAL.GetDAL().GetMailTemplateById(templateId);
            if (mailTemplate == null)
            {
                MessageBox.Show("Не удалось получить объект");
                return;
            }


            OriginlTemplate = Helper.DeepClone(mailTemplate);
            TemplateEditable.PropertyChanged -= TemplateEditableOnChanged;

            TemplateEditable = new MailTemplateEditable(mailTemplate);

            TemplateEditable.PropertyChanged += TemplateEditableOnChanged;
            CommandManager.InvalidateRequerySuggested();

            ProcessMacroses = enableMacrosesProcessing;
            WorkWithExistingTemplate = true;

            OnPropertyChanged("TemplateDescription");
        }


        //Вкл/выкл обработки макросов пока будет задаваться через поле при создании нового шаблона

        #endregion

        /// <summary>
        /// Метод для обработки события изменения модели
        /// (информирует view, что следует обновить данные)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void TemplateEditableOnChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OnPropertyChanged("Html");
            OnPropertyChanged();
        }


        #region Commands

        private bool SaveCanExexute()
        {
            if (string.IsNullOrWhiteSpace(TemplateDescription)) return false;
            if (OriginlTemplate == TemplateEditable) return false;
            if (string.IsNullOrWhiteSpace(Html)) return false;
            return true;
        }

        #region Execute
        private void SaveExecute()
        {
            //Сохраняем копию т.к. оригинал ещё в работе и в его теле могут быть спец. вставки
            var clone = Helper.DeepClone(TemplateEditable);
            clone.TemplateBody = TemplateEditable.GetCleanHtmlForSave();

            if (WorkWithExistingTemplate) //Если работаем с имеющимся HTML-шаблоном
            {
                if (CurrentEmptyTemplate != null) //Выбран шаблон поверх которого сохраняем (это норма)
                {
                    clone.TemplateId = CurrentEmptyTemplate.TemplateId; //Нужно быть увернным, что данные корректны
                    SaveToBase(clone);
                }
                else // Исключительная ситуация
                {
                    _toastPresenter?.ShowAsync("Не удаётся сохранить HTML-шаблон. Попробуйте перезапустить редактор", ToastDuration.Long, ToastPosition.Center);
                    //todo: Залогировать
                }
            }
            else //Если это новый шаблон
            {
                if (CurrentEmptyTemplate != null) //Если записываем поверх имеющегося шаблона
                {
                    if (
                        MessageBox.Show($"Перезаписать шаблон \"{TemplateEditable.FullDescription}\"",
                            "Перезаписать HTML-шаблон", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                    {
                        clone.TemplateId = CurrentEmptyTemplate.TemplateId; //Нужно быть увернным, что данные корректны
                        SaveToBase(clone);
                    }
                }
                else if(int.Parse(NewEmptyMailTemplate) > 0) //Если создаём новый объект и у нас есть ID под которым нужно сохранить
                {
                    clone.TemplateId = int.Parse(NewEmptyMailTemplate); //Указываем новый ID шаблону
                    if(!SaveToBase(clone)) return; //Если не удалось сохранить в базу, то в изменении режима нет смысла

                    _workmodeAlredySet = false; //Грязный хак для переключения в режим редактирования имеющегося шаблона
                    MailEditorViewModelSetup(clone.TemplateId);

                    var temp = TemplateEditable; //Перезагружаем список имеющихся номеров шаблонов
                    TemplateEditable = null;
                    MailsTemplatesEmpty = DAL.GetDAL().EmptyMailTemplates;
                    TemplateEditable = temp;
                }
                else //Исключительная ситуация
                {
                    _toastPresenter?.ShowAsync("Не удаётся сохранить HTML-шаблон. Не указан ID под которым сохранять HTML-шаблон", ToastDuration.Long, ToastPosition.Center);
                    //todo: Залогировать
                }
            }      
        }

        private bool SaveToBase(MailTemplate template)
        {
            if (DAL.GetDAL().SaveMailTemplate(template))
            {
                MailTemplate temp = TemplateEditable;

                _toastPresenter?.ShowAsync("Сохранено", ToastDuration.Short, ToastPosition.Center);

                OriginlTemplate = Helper.DeepClone(temp);
                TemplateChanged = true; //Указываем, во viewmodel'и что объект в базе изменялся
                OnPropertyChanged();
                return true;
            }
            _toastPresenter?.ShowAsync("Не удалось сохранить шаблон", ToastDuration.Short, ToastPosition.Center);
            return false;
        }
        


        private void OpenFileExecute()
        {
            OpenFileDialog myDialog = new OpenFileDialog
            {
                Filter = "Письма сохранённые в html(*.htm;*.html)|*.htm;*.html" + "|Все файлы (*.*)|*.* ",
                CheckFileExists = true,
                Multiselect = false
            };

            if (myDialog.ShowDialog() == true)
            {
                FilePath = myDialog.FileName;
                TemplateEditable.TemplateBody = Helper.DeleteLinksInHtml(DAL.GetDAL().GetHtmlFromFile(myDialog.FileName, null));
                TemplateEditable.ClearIndents();
                OnPropertyChanged();
            }
        }

        #endregion
        #endregion

        #region INavigableViewModel realization

        public void OnNavigatedTo(INavigationContext context) //При заходе
        {
            //if (context.ViewModelFrom != null) MessageBox.Show(context.ViewModelFrom.ToString());
        }

        public Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            return Empty.TrueTask;
        }

        public void OnNavigatedFrom(INavigationContext context)
        {
            
        }

        #endregion


    }

}
