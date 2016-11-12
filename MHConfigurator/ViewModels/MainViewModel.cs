using System;
using System.IO;
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

        private readonly IToastPresenter _toastPresenter;
        public ICommand OpenPorpertiesEditorCommand;
        public ICommand OpenTemplatesViewerCommand;
        public ICommand CheckConnectionCommand;
        public ICommand OpenDbCommand;
        public ICommand CreateDbCommand;
        private bool _connectionSuccess;
        private bool _windowVisible=true;

        public MainViewModel(IToastPresenter toastPresenter)
        {
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


            _toastPresenter = toastPresenter;

            OpenPorpertiesEditorCommand = new RelayCommand(OpenPorpertiesEditorExecute, () => ConnectionSuccess);
            OpenTemplatesViewerCommand = new RelayCommand(OpenTemplatesViewerExecute, () => ConnectionSuccess);
            CheckConnectionCommand = new RelayCommand(CheckConnectionExecute);
            OpenDbCommand = new RelayCommand(OpenDbExecute);
            CreateDbCommand = new RelayCommand(CreateDbExecute);
        }

        private async void OpenPorpertiesEditorExecute()
        {
            WindowVisibility=Visibility.Collapsed;
            using (var properiesEditor = GetViewModel<MailPropertiesViewModel>())
            {
                await properiesEditor.ShowAsync();
            }
            WindowVisibility = Visibility.Visible;
        }

        private async void OpenTemplatesViewerExecute()
        {
            WindowVisibility = Visibility.Collapsed;
            using (var properiesEditor = GetViewModel<TemplateViewerViewModel>())
            {
                await properiesEditor.ShowAsync();
            }
            WindowVisibility = Visibility.Visible;
        }

        private void CheckConnectionExecute()
        {
            ConnectionSuccess = DAL.GetDAL().TestConnection(Properties.Settings.Default.databasePath);
            if (ConnectionSuccess)
            {
                _toastPresenter?.ShowAsync("Соединение с БД успешно", ToastDuration.Short, ToastPosition.Center);
                CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
            }
        }

        private void OpenDbExecute()
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
                    if (DAL.GetDAL().TestConnection(myDialog.FileName))
                    {
                        Properties.Settings.Default.databasePath = myDialog.FileName;
                        Properties.Settings.Default.Save();
                        OnPropertyChanged("CurrentDatabasePath");

                        DAL.GetDAL().DatabasePath = myDialog.FileName;
                        ConnectionSuccess = true;
                        DAL.GetDAL().MakeBackUpDb(myDialog.FileName);
                        CommandManager.InvalidateRequerySuggested(); //Refrash canexecute on buttons
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключится к указанной БД. Попробуйте выбрать другую БД",
                        "Ожибка подключения БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (System.IO.FileNotFoundException exceptionex)
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