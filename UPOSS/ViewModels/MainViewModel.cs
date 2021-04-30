using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using UPOSS.Commands;
using UPOSS.State;
using UPOSS.State.Navigators;

namespace UPOSS.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            if (SelectedViewModel == null)
            {
                var vm = new AuthViewModel();

                // if login successful, navigate to Dashboard
                vm.LoginCompleted += (sender, e) => SelectedViewModel = new DashboardViewModel();
                SelectedViewModel = vm;
            }
        }

        private ViewModelBase _selectedViewModel;
        public ViewModelBase SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged(nameof(SelectedViewModel)); }
        }
    }
}
