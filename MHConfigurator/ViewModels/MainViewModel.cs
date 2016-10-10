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

namespace MHConfigurator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {
            _mailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys);
            foreach (var property in _mailProperties)
            {
                property.PropertyChanged += MailPropertyInCollection_Changed;
            }
            _mailProperties.CollectionChanged += _mailProperties_CollectionChanged;

            MailsTemplates = DAL.GetDAL().GetEmptyMailTemplates();

            CancelCommand = new RelayCommand(CancelCommandExecute,CancelCanExecute,this);
            SaveCommand = new RelayCommand(SaveCommandExecute, SaveCanExecute, this);
            NewCommand = new RelayCommand(NewCommandExecute, NewCanExecute, this);
        }

        private void _mailProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (MailProperty item in e.NewItems)
                    item.PropertyChanged += MailPropertyInCollection_Changed;
            if (e.OldItems != null)
                foreach (MailProperty item in e.OldItems)
                    item.PropertyChanged -= MailPropertyInCollection_Changed;
        }

        private void MailPropertyInCollection_Changed(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged();
            //MessageBox.Show("!!!");
        }


        #region Fields

        #region Backing Fields

        private ObservableCollection<MailProperty> _mailProperties;
        private MailProperty _currentProperty;
        private string _searchString;

        private MailProperty _originalCurrentProperty;
        //private bool _currentPropertyChanged = false;
        
        private List<MailsTemplate> _mailsTemplates;
        private int _selectedMailTemplate;

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
                //OnPropertyChanged(new PropertyChangedEventArgs("MailProperties"));
                OnPropertyChanged();
            }
        }


        private bool _currentPropertyAlredyChange; 
        public MailProperty CurrentProperty
        {
            get { return _currentProperty; }
            set
            {
                //TODO: Предусмотреть возможность прилетания нового объекта
                if ((_originalCurrentProperty != null)&&(_originalCurrentProperty!=_currentProperty)&&!_currentPropertyAlredyChange) //Если выбран другой объект и есть несохранённые изменения
                {
                    MessageBoxResult result = MessageBox.Show("Есть несохранённые изменения. Сохранить?", "Несохранённые изменения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DAL.GetDAL().SaveMailProperty(_currentProperty);
                    }
                    else
                    {
                        _currentPropertyAlredyChange = true;
                        MailProperties[MailProperties.IndexOf(CurrentProperty)] = _originalCurrentProperty; //Находим в коллекции изменённый объект и заменить его оригиналом
                    }
                    //Выдать сообщение, что изменения не сохранены
                    //В зависимости от результата или:
                    //1. Найти в коллекции изменённый объект и заменить его оригиналом
                    //2. Вызвать метод DAL для сохранения _currentProperty
                }
                _currentPropertyAlredyChange = false;
                
                _originalCurrentProperty = value!=null ? Helper.DeepClone(value) : null; //Делаем резервную копию

                //SelectedMailTemplate = value.BodyID; //выставляем id шаблона


                _currentProperty = value;
                
                //OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
                OnPropertyChanged();
            }
        }





        public List<MailsTemplate> MailsTemplates
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

        #endregion

        #region Commands

        public ICommand NewCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #region Execute

        private void CancelCommandExecute(object cmdParameter)
        {
            if (!NewModeOn && (_originalCurrentProperty != null) && (CurrentProperty != null) &&
                (_originalCurrentProperty != CurrentProperty))
            {
                _currentPropertyAlredyChange = true; //Нужно, чтобы подавить выдачу popup'a при изменении свойства
                MailProperties[MailProperties.IndexOf(CurrentProperty)] = _originalCurrentProperty;
            }
            else if(NewModeOn)
            {
                NewModeOn = false;
                _currentPropertyAlredyChange = true;
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
            DAL.GetDAL().SaveMailProperty(CurrentProperty);
            _originalCurrentProperty = Helper.DeepClone(CurrentProperty);
            if (NewModeOn)
            {
                NewModeOn = false;
                _currentPropertyAlredyChange = true;
                MailProperties.Add(CurrentProperty);
                CurrentProperty = null;

                MailProperties = new ObservableCollection<MailProperty>(MailProperties.OrderBy(x => x.ButtonID));
                /*
                var temp = DAL.GetDAL().MailPropertys;
                foreach (var property in temp)
                {
                    if (!MailProperties.Contains(property))
                    {
                        MailProperties.Add(property);
                        MailProperties = new ObservableCollection<MailProperty>(MailProperties.OrderBy(x=>x.ButtonID));
                    }
                }*/   
            }
           
            
            OnPropertyChanged(new PropertyChangedEventArgs("MailProperties"));
            OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
            CommandManager.InvalidateRequerySuggested();
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
        private bool _newModeOn = false;
        


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
