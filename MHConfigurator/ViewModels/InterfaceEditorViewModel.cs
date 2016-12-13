using MHConfigurator.Models;
using MugenMvvmToolkit.ViewModels;
using System.Collections.ObjectModel;

namespace MHConfigurator.ViewModels
{
     

    public class InterfaceEditorViewModel : ViewModelBase
    {
        private ObservableCollection<PanelElement> _panelElements;

        public ObservableCollection<PanelElement> PanelElements
        {
            get { return _panelElements; }
            set
            {
                if (Equals(value, _panelElements)) return;
                _panelElements = value;
                OnPropertyChanged();
            }
        }


        public InterfaceEditorViewModel()
        {
            _panelElements = new ObservableCollection<PanelElement>(DAL.GetDAL().PanelElements);
        }
    }
}