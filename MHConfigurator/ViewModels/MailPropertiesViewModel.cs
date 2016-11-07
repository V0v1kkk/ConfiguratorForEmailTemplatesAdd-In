using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MugenMvvmToolkit;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Models;
using System.Windows.Input;
using MHConfigurator.Models;
using System.Windows;
using System.Windows.Navigation;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MHConfigurator.ViewModels
{
    public class MailPropertiesViewModel : ViewModelBase
    {

        public MailPropertiesViewModel()
        {
            _mailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys);
            foreach (var property in _mailProperties)
            {
                property.PropertyChanged += MailPropertyInCollection_Changed;
            }
            _mailProperties.CollectionChanged += _mailProperties_CollectionChanged;

            MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().GetEmptyMailTemplates());

            CancelCommand = new RelayCommand(CancelCommandExecute,CancelCanExecute,this);
            SaveCommand = new RelayCommand(SaveCommandExecute, SaveCanExecute, this);
            NewCommand = new RelayCommand(NewCommandExecute, NewCanExecute, this);
            OpenTemplateCommand = new RelayCommand(OpenTemplateExecute, ()=>(CurrentProperty!=null)&&(CurrentProperty.BodyID!=0),this);
        }

        private void _mailProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MailProperty item in e.NewItems)
                {
                    item.PropertyChanged += MailPropertyInCollection_Changed;
                }
            }
            if (e.OldItems != null)
            {
                foreach (MailProperty item in e.OldItems)
                {
                    item.PropertyChanged -= MailPropertyInCollection_Changed;
                }
            }
        }

        private void MailPropertyInCollection_Changed(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged();
        }


        #region Properties

        #region Backing Fields

        private ObservableCollection<MailProperty> _mailProperties;
        private MailProperty _currentProperty;
        private string _searchString;

        private MailProperty _originalCurrentProperty;
        //private bool _currentPropertyChanged = false;
        
        private ObservableCollection<MailTemplate> _mailsTemplates;
        private int _selectedMailTemplate;

        private bool _windowVisible = true;

        #endregion

        public string SearchString
        {
            get
            {
                if (_searchString.IsNullOrEmpty()) return "";
                return _searchString;
            }
            set
            {
                bool stringContains = _searchString != null && value.Contains(_searchString);
                _searchString = value;
                CurrentProperty = null;


                if (stringContains)
                {
                    var temp = new ObservableCollection<MailProperty>(MailProperties.Where(
                    property =>
                        (property.ButtonID.ToString().SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.Description.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.TO.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.Copy.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.HideCopy.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.Subject.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.BodyID.ToString().SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        ||
                        (property.Zametka1.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                        &&
                        (property.ButtonID > 0)
                    ));
                    
                    if (!MailProperties.SequenceEqual(temp))
                    {
                        MailProperties = temp;
                    }
                }
                else if (_searchString.IsNullOrEmpty())
                {
                    MailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys.Where(property => property.ButtonID>0).ToList());
                }
                else
                {
                    MailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys.Where(property => property.ButtonID > 0).ToList());
                    var temp = new ObservableCollection<MailProperty>(MailProperties.Where(
                        property =>
                            (property.ButtonID.ToString().SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.Description.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.TO.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.Copy.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.HideCopy.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.Subject.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.BodyID.ToString().SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase)) 
                            ||
                            (property.Zametka1.SafeContains(SearchString, StringComparison.InvariantCultureIgnoreCase))
                            &&
                            (property.ButtonID>0)
                        ));

                    if (!MailProperties.SequenceEqual(temp))
                    {
                        MailProperties = temp;
                    }
                }      

            }
        }
        public ObservableCollection<MailProperty> MailProperties
        {
            get { return _mailProperties; }
            set
            {
                _mailProperties = value;
                OnPropertyChanged();
            }
        }
        private bool _currentPropertyAlredyChanged; 
        public MailProperty CurrentProperty
        {
            get { return _currentProperty; }
            set
            {
                if ((_originalCurrentProperty != null) && 
                    (_originalCurrentProperty!=_currentProperty) && 
                    !_currentPropertyAlredyChanged) //Если выбран другой объект и есть несохранённые изменения
                {
                    MessageBoxResult result = MessageBox.Show("Есть несохранённые изменения. Сохранить?", "Несохранённые изменения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DAL.GetDAL().SaveMailProperty(_currentProperty);
                    }
                    else
                    {
                        _currentPropertyAlredyChanged = true; //При замене элемента в коллекции мы повторно попадаем в этот сеттер. Для этого нужна эта переменная (по сути костыль, но лучше не придумал).
                        MailProperties[MailProperties.IndexOf(CurrentProperty)] = _originalCurrentProperty; //Находим в коллекции изменённый объект и замяем его оригиналом
                    }
                }
                _currentPropertyAlredyChanged = false; //объяснение этого костыля немного выше
                
                _originalCurrentProperty = value!=null ? Helper.DeepClone(value) : null; //Делаем резервную копию
                _currentProperty = value;
                
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
            }
        }
        public ObservableCollection<MailTemplate> MailsTemplates
        {
            get { return _mailsTemplates; }
            set
            {
                _mailsTemplates = value; 
                OnPropertyChanged();
            }
        }
        public int SelectedMailTemplate
        {
            get { return _selectedMailTemplate; }
            set
            {
                _selectedMailTemplate = value;
                CurrentProperty.BodyID = value;
                OnPropertyChanged();
            }
        }
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

        #endregion

        #region Commands

        public ICommand NewCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand OpenTemplateCommand { get; private set; }

        #region Execute

        private void CancelCommandExecute(object cmdParameter)
        {
            if (!NewModeOn && (_originalCurrentProperty != null) && (CurrentProperty != null) &&
                (_originalCurrentProperty != CurrentProperty)) //
            {
                _currentPropertyAlredyChanged = true; // Нужно, чтобы подавить выдачу popup'a при изменении свойства
                MailProperties[MailProperties.IndexOf(CurrentProperty)] = _originalCurrentProperty;
            }
            else if(NewModeOn)
            {
                NewModeOn = false;
                _currentPropertyAlredyChanged = true;
                CurrentProperty = null;
            }

            OnPropertyChanged(new PropertyChangedEventArgs("MailProperties"));
            OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
            CommandManager.InvalidateRequerySuggested();
        }
        private void NewCommandExecute(object cmdParameter)
        {
            NewModeOn = true;
            _originalCurrentProperty = null;
            CurrentProperty = new MailProperty();
            OnPropertyChanged();
        }
        private void SaveCommandExecute(object cmdParameter)
        {
            //TODO: Проверка на существование элемента (задаётся доп. вопрос, другая добавка в список) 
            if (NewModeOn && DAL.GetDAL().GetMailPropertyById(CurrentProperty.ButtonID) != null) //Если сохраняем новый объект, а объект с таким же id уже есть в базе
            {
                MessageBoxResult result = MessageBox.Show("Шаблон с таким id уже есть в базе. Хотите заменить?", "Замена шаблона", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    NewModeOn = false;

                    DAL.GetDAL().SaveMailProperty(CurrentProperty);
                    _originalCurrentProperty = Helper.DeepClone(CurrentProperty);

                    var localProp = MailProperties.FirstOrDefault(property => property.ButtonID == CurrentProperty.ButtonID);
                    if (localProp != null)
                    {
                        _currentPropertyAlredyChanged = true;
                        MailProperties[MailProperties.IndexOf(localProp)] = CurrentProperty;
                    }
                }
            }
            else if(NewModeOn)
            {
                DAL.GetDAL().SaveMailProperty(CurrentProperty);
                _originalCurrentProperty = Helper.DeepClone(CurrentProperty);

                NewModeOn = false;
                _currentPropertyAlredyChanged = true;
                MailProperties.Add(CurrentProperty);
                CurrentProperty = null;

                MailProperties = new ObservableCollection<MailProperty>(MailProperties.OrderBy(x => x.ButtonID));
            }
            else
            {
                DAL.GetDAL().SaveMailProperty(CurrentProperty);
                _originalCurrentProperty = Helper.DeepClone(CurrentProperty);
            }


            //DAL.GetDAL().SaveMailProperty(CurrentProperty);
            //_originalCurrentProperty = Helper.DeepClone(CurrentProperty);
            
            OnPropertyChanged(new PropertyChangedEventArgs("MailProperties"));
            OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
            CommandManager.InvalidateRequerySuggested();
        }

        
        

        private async void OpenTemplateExecute()
        {
            using(var htmlEditor = GetViewModel<MailEditorViewModel>())
            {
                htmlEditor.MailEditorViewModelSetup(CurrentProperty.BodyID);

                WindowVisibility = Visibility.Collapsed;
                IAsyncOperation<bool> asyncOperation = htmlEditor.ShowAsync();
                await asyncOperation;
                if(htmlEditor.TemplateChanged)
                {
                    MailsTemplates = new ObservableCollection<MailTemplate>(DAL.GetDAL().GetEmptyMailTemplates()); //В случае изменений переимпортируем пустые шаблоны (вдруг изменилось описание)

                    var temp = CurrentProperty; //Костыль для обновления значения в combobox'е
                    CurrentProperty = null;
                    CurrentProperty = temp;
                    OnPropertyChanged();
                }
                WindowVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region CanExecute

        public bool NewModeOn
        {
            get { return _newModeOn; }
            private set
            {
                _newModeOn = value;
                OnPropertyChanged();
                OnPropertyChanged(new PropertyChangedEventArgs("ListViewEnabled"));
            }
        }

        private bool _cancelCanExecute = true;
        private bool _saveCanExecute = true;
        private bool _newModeOn;
        


        private bool CancelCanExecute(object cmdParameter)
        {
            if (NewModeOn || 
                (!NewModeOn && (_originalCurrentProperty != null) && (CurrentProperty != null) && (_originalCurrentProperty != CurrentProperty)))
            {
                return true;
            }
            return false;
        }
        private bool NewCanExecute(object cmdParameter)
        {
            return !NewModeOn;
        }
        private bool SaveCanExecute(object cmdParameter)
        {
            return NewModeOn || ((_originalCurrentProperty != null) && (CurrentProperty != null) && (CurrentProperty != _originalCurrentProperty));
        }

        #endregion

        #endregion

        public bool ListViewEnabled => !NewModeOn;

        public void DeleteCommand(MailProperty property)
        {
            MessageBoxResult result = MessageBox.Show("Удаление необратимо. Удалить шаблон?", "Удаление шаблона", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (!DAL.GetDAL().DeleteMailProperty(property))
                {
                    MessageBox.Show("Не удалось удалить шаблон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MailProperties.Remove(property);
                }
            }
        }
    }
}
