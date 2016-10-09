using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary;
using MugenMvvmToolkit.ViewModels;
//using MailProperty = DataAccessLibrary.MailProperty;

namespace Configurator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<MailProperty> _mailProperties;


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

        private MailProperty _currentProperty;

        public MailProperty CurrentProperty
        {
            get { return _currentProperty; }
            set
            {/*
                if ((_originalCurrentProperty != null)&&(_currentPropertyChanged)&&(_originalCurrentProperty!=_currentProperty)) //Если выбран другой объект и есть несохранённые изменения
                {
                    MessageBoxResult result = MessageBox.Show("Есть несохранённые изменения. Сохранить?", "Несохранённые изменения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        
                    }
                    else
                    {
                        MailProperties[MailProperties.IndexOf(_currentProperty)] = _originalCurrentProperty; //Находим в коллекции изменённый объект и заменить его оригиналом
                    }
                    //Выдать сообщение, что изменения не сохранены
                    //В зависимости от результата или:
                    //1. Найти в коллекции изменённый объект и заменить его оригиналом
                    //2. Вызвать метод DAL для сохранения _currentProperty
                }
                
                _originalCurrentProperty = Helper.DeepClone(_currentProperty); //Делаем резервную копию
                SelectedMailTemplate = value.BodyID; //выставляем id шаблона
                */
                _currentProperty = value;

                // OnPropertyChanged(new PropertyChangedEventArgs("CurrentProperty"));
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            
           //var t = new List<MailProperty>() {new MailProperty(), new MailProperty(), new MailProperty()};
           //_mailProperties = new ObservableCollection<MailProperty>(t);
            

            _mailProperties = new ObservableCollection<MailProperty>(DAL.GetDAL().MailPropertys);
           
            
            /*
           using (var uow = new UnitOfWork())
           {
               try
               {
                   _mailProperties = new ObservableCollection<MailProperty>(uow.Propertys.GetAll());
               }
               catch (Exception)
               {
                   //todo: Залогировать
               }
           }
           */
           
        }
    }
}
