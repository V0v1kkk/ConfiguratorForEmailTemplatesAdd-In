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

namespace MHConfigurator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {
            _mailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys);

            MailsTemplates = DAL.GetDAL().GetEmptyMailTemplates();

            CancelCommand = new RelayCommand(CancelCommandExecute,CancelCanExecute,this);
            SaveCommand = new RelayCommand(SaveCommandExecute, SaveCanExecute, this);
            NewCommand = new RelayCommand(NewCommandExecute, NewCanExecute, this);
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
                        MailProperties[MailProperties.IndexOf(_currentProperty)] = _originalCurrentProperty; //Находим в коллекции изменённый объект и заменить его оригиналом
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
            if (!_newModeOn && (_originalCurrentProperty != null) && (CurrentProperty != null) &&
                (_originalCurrentProperty != CurrentProperty))
            {
                _currentPropertyAlredyChange = true; //Нужно, чтобы подавить выдачу popup'a при изменении свойства
                MailProperties[MailProperties.IndexOf(_currentProperty)] = _originalCurrentProperty;
            }
            else if(_newModeOn)
            {
                _currentProperty = null;
                _newModeOn = false;
            }
        }
        private void NewCommandExecute(object cmdParameter)
        {
            
        }

        private void SaveCommandExecute(object cmdParameter)
        {
            
        }

        #endregion

        #region CanExecute

        private bool _newModeOn = false;
        private bool _cancelCanExecute = true;
        private bool _saveCanExecute = true;

        private bool CancelCanExecute(object cmdParameter)
        {
            return _cancelCanExecute;
        }
        private bool NewCanExecute(object cmdParameter)
        {
            return !_newModeOn;
        }
        private bool SaveCanExecute(object cmdParameter)
        {
            return _newModeOn ||(_currentProperty!=_originalCurrentProperty);
        }

        #endregion

        #endregion
    }
}
