using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using ThicknessConverter = Xceed.Wpf.DataGrid.Converters.ThicknessConverter;

namespace MHConfigurator.ViewModels
{
    public class MainViewModel : MultiViewModel, INavigableViewModel
    {
        public bool ConnectionSuccess
        {
            get { return _connectionSuccess; }
            set
            {
                if (value == _connectionSuccess) return;
                _connectionSuccess = value;
                OnPropertyChanged();
                OnPropertyChanged("ConnectionIndicatorColor");
                CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
            }
        }

        public Color ConnectionIndicatorColor => ConnectionSuccess ? Colors.Green : Colors.Red;

        public string CurrentDatabasePath => Properties.Settings.Default.databasePath;

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

        public string ViewBusyMessage
        {
            get
            {
                return _viewBusyMessage;
            }

            set
            {
                if (value == _viewBusyMessage) return;
                _viewBusyMessage = value;
                OnPropertyChanged();
            }
        }

        private readonly IToastPresenter _toastPresenter;
        public ICommand OpenPorpertiesEditorCommand;
        public ICommand OpenTemplatesViewerCommand;
        public ICommand CheckConnectionCommand;
        public ICommand OpenDbCommand;
        public ICommand CreateDbCommand;
        private bool _connectionSuccess;
        private bool _windowVisible=true;
        private bool _viewBusy;
        private string _viewBusyMessage;


        //todo: На будующее сделать поддержку режима ReadOnly (проверять права на файл и т.д.)
        public MainViewModel(IToastPresenter toastPresenter)
        {
            TestConnection();

            /*
            //BGW construction for correctly work Busy control
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                ea.Result = DAL.GetDAL().TestConnection();
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                ViewBusy = false;
                ConnectionSuccess = (bool)ea.Result;
                if (ConnectionSuccess)
                {
                    _toastPresenter?.ShowAsync("Соединение с БД установлено", ToastDuration.Short, ToastPosition.Center);
                    CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
                    DAL.GetDAL().MakeBackUpDb(Properties.Settings.Default.databasePath); // Make backup current DB
                }
                else
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Func<MessageBoxResult>(
                    () => MessageBox.Show("Не удалось подключится к БД. Укажите путь к БД или создайтей новую.",
                    "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error)));
                }
            };

            DAL.GetDAL().DatabasePath = Properties.Settings.Default.databasePath;
            ViewBusyMessage = "Проверка связи с БД";
            ViewBusy = true;
            worker.RunWorkerAsync();
            */

            /*


            DAL.GetDAL().DatabasePath = Properties.Settings.Default.databasePath;
            if (!DAL.GetDAL().TestConnection())
            {
                //show messageBox async
                Dispatcher.CurrentDispatcher.BeginInvoke(new Func<MessageBoxResult>(
                    () => MessageBox.Show("Не удалось подключится к БД. Укажите путь к БД или создайтей новую.",
                    "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error)));
            }
            else
            {
                ConnectionSuccess = true;
                DAL.GetDAL().MakeBackUpDb(Properties.Settings.Default.databasePath);
            }
            */

            _toastPresenter = toastPresenter;

            OpenPorpertiesEditorCommand = new RelayCommand(OpenPorpertiesEditorExecute, ButtonsForWorkWithDbCanBeActive, this);
            OpenTemplatesViewerCommand = new RelayCommand(OpenTemplatesViewerExecute, ButtonsForWorkWithDbCanBeActive, this);
            CheckConnectionCommand = new RelayCommand(CheckConnectionExecute);
            OpenDbCommand = new RelayCommand(OpenDbExecute);
            CreateDbCommand = new RelayCommand(CreateDbExecute);
        }

        private async void TestConnection()
        {
            await Task.Factory.StartNew(() =>
            {
                DAL.GetDAL().DatabasePath = Properties.Settings.Default.databasePath;
                ViewBusyMessage = "Проверка связи с БД";
                ViewBusy = true;
                ConnectionSuccess = DAL.GetDAL().TestConnection();
                ViewBusy = false;
                if (ConnectionSuccess)
                {
                    _toastPresenter?.ShowAsync("Соединение с БД установлено", ToastDuration.Short, ToastPosition.Center);
                    CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
                    DAL.GetDAL().MakeBackUpDb(Properties.Settings.Default.databasePath); // Make backup current DB
                }
                else
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Func<MessageBoxResult>(
                    () => MessageBox.Show("Не удалось подключится к БД. Укажите путь к БД или создайтей новую.",
                    "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error)));
                }
            });
        }

        //test
        private bool ButtonsForWorkWithDbCanBeActive(object cmdParameter)
        {
            return ConnectionSuccess;
        }


        private async void OpenPorpertiesEditorExecute(object cmdParameter)
        {
            await Task.Factory.StartNew(async () =>
            {
                ViewBusyMessage = "Загрузка шаблонов";
                ViewBusy = true;
                var properiesEditor = GetViewModel<MailPropertiesViewModel>();
                ViewBusy = false;
                WindowVisibility = Visibility.Collapsed;
                await properiesEditor.ShowAsync();
                WindowVisibility = Visibility.Visible;
                ViewBusy = false;
                properiesEditor.Dispose();
            });
        }


        private async void OpenTemplatesViewerExecute(object cmdParameter)
        {
            await Task.Factory.StartNew(async () =>
            {
                ViewBusyMessage = "Загрузка HTML-шаблонов";
                ViewBusy = true;
                var templatesEditor = GetViewModel<TemplateViewerViewModel>();
                templatesEditor.Setup();
                ViewBusy = false;
                WindowVisibility = Visibility.Collapsed;
                await templatesEditor.ShowAsync();
                WindowVisibility = Visibility.Visible;
                templatesEditor.Dispose();
            });
        }

        /* old and long version
        private void OpenTemplatesViewerExecute1(object cmdParameter)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                var templatesEditor = GetViewModel<TemplateViewerViewModel>();
                ea.Result = templatesEditor;
            };
            worker.RunWorkerCompleted += async (o, ea) =>
            {
                ViewBusy = false;
                if (!(ea.Result is TemplateViewerViewModel)) return;

                var templatesEditor = (TemplateViewerViewModel)ea.Result;
                WindowVisibility = Visibility.Collapsed;
                await templatesEditor.ShowAsync();
                WindowVisibility = Visibility.Visible;
                ViewBusy = false;
                templatesEditor.Dispose();
            };


            ViewBusyMessage = "Загрузка HTML-шаблонов";
            ViewBusy = true;
            worker.RunWorkerAsync();
        }*/

        private void CheckConnectionExecute()
        {
            //BGW construction for correctly work Busy control
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                ea.Result = DAL.GetDAL().TestConnection(Properties.Settings.Default.databasePath);
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                ViewBusy = false;
                ConnectionSuccess = (bool)ea.Result;
                if (ConnectionSuccess)
                {
                    _toastPresenter?.ShowAsync("Соединение с БД успешно", ToastDuration.Short, ToastPosition.Center);
                    CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
                }
            };

            ViewBusyMessage = "Проверка связи с БД";
            ViewBusy = true;
            worker.RunWorkerAsync();



            /*
            ConnectionSuccess = DAL.GetDAL().TestConnection(Properties.Settings.Default.databasePath);
            if (ConnectionSuccess)
            {
                _toastPresenter?.ShowAsync("Соединение с БД успешно", ToastDuration.Short, ToastPosition.Center);
                CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
            }*/
        }

        private async void OpenDbExecute()
        {
            try
            {
                OpenFileDialog myDialog = new OpenFileDialog
                {
                    Filter = "БД (*.sqlite3)|*.sqlite3;",
                    CheckFileExists = true,
                    Multiselect = false
                };

                //On case folder no more exist
                var oldFolder = new FileInfo(Properties.Settings.Default.databasePath).DirectoryName;
                if (Directory.Exists(oldFolder)) myDialog.InitialDirectory = oldFolder;

                if (myDialog.ShowDialog() == true)
                {
                    await Task.Factory.StartNew(() =>
                    {
                        ViewBusyMessage = "Подключение к БД";
                        ViewBusy = true;
                        var tryConnectionResult = DAL.GetDAL().TestConnection(myDialog.FileName);
                        ViewBusy = false;
                        if (tryConnectionResult)
                        {
                            Properties.Settings.Default.databasePath = myDialog.FileName;
                            Properties.Settings.Default.Save();
                            OnPropertyChanged("CurrentDatabasePath");

                            DAL.GetDAL().DatabasePath = myDialog.FileName;
                            ConnectionSuccess = true;
                            DAL.GetDAL().MakeBackUpDb(myDialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось подключится к указанной БД. Попробуйте выбрать другую БД",
                                            "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });

                    /*
                    //BGW construction for correctly work Busy control
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (o, ea) =>
                    {
                        ea.Result = DAL.GetDAL().TestConnection(myDialog.FileName);
                    };
                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        ViewBusy = false;
                        var tryConnectionResult = (bool)ea.Result;
                        if (tryConnectionResult)
                        {
                            Properties.Settings.Default.databasePath = myDialog.FileName;
                            Properties.Settings.Default.Save();
                            OnPropertyChanged("CurrentDatabasePath");

                            DAL.GetDAL().DatabasePath = myDialog.FileName;
                            ConnectionSuccess = true;
                            DAL.GetDAL().MakeBackUpDb(myDialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось подключится к указанной БД. Попробуйте выбрать другую БД",
                                            "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    ViewBusyMessage = "Подключение к БД";
                    ViewBusy = true;
                    worker.RunWorkerAsync();


                    /*
                    if (DAL.GetDAL().TestConnection(myDialog.FileName))
                    {
                        Properties.Settings.Default.databasePath = myDialog.FileName;
                        Properties.Settings.Default.Save();
                        OnPropertyChanged("CurrentDatabasePath");

                        DAL.GetDAL().DatabasePath = myDialog.FileName;
                        ConnectionSuccess = true;
                        DAL.GetDAL().MakeBackUpDb(myDialog.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключится к указанной БД. Попробуйте выбрать другую БД",
                        "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    */

                }
            }
            catch (FileNotFoundException exception)
            {

                //todo: Залогировать
            }
        }

        private void CreateDbExecute()
        {
            SaveFileDialog myDialog = new SaveFileDialog
            {
                Filter = "БД (*.sqlite3)|*.sqlite3;",
            };

            if (myDialog.ShowDialog() == true)
            {
                throw new NotImplementedException();
            }
        }


        public void OnNavigatedTo(INavigationContext context)
        {
            
        }

        public Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            return Empty.TrueTask;
        }

        public void OnNavigatedFrom(INavigationContext context)
        {
            
        }
    }
}