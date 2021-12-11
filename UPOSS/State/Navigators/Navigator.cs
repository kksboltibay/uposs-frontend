using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using UPOSS.Commands;
using UPOSS.Models;
using UPOSS.ViewModels;

namespace UPOSS.State.Navigators
{
    public class Navigator : ObservableObject, INavigator
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel 
        {
            get  { return _currentViewModel; }  
            set { _currentViewModel = value; OnPropertyChanged(nameof(CurrentViewModel)); } 
        }

        public ICommand UpdateCurrentViewModelCommand => new UpdateCurrentViewModelCommand(this);
    }
}
