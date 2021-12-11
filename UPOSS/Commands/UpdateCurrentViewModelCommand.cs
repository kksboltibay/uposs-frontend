using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using UPOSS.State.Navigators;
using UPOSS.ViewModels;

namespace UPOSS.Commands
{
    public class UpdateCurrentViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private INavigator _navigator;

        public UpdateCurrentViewModelCommand(INavigator navigator)
        {
            _navigator = navigator;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is ViewType)
            {
                ViewType viewType = (ViewType)parameter;
                switch(viewType)
                {
                    case ViewType.User:
                        _navigator.CurrentViewModel = new UserViewModel();
                        break;

                    case ViewType.Branch:
                        _navigator.CurrentViewModel = new BranchViewModel();
                        break;

                    case ViewType.Product:
                        _navigator.CurrentViewModel = new ProductViewModel();
                        break;

                    case ViewType.Cashier:
                        _navigator.CurrentViewModel = new CashierViewModel();
                        break;

                    case ViewType.Analytics:
                        _navigator.CurrentViewModel = new AnalyticsViewModel();
                        break;

                    case ViewType.Setting:
                        _navigator.CurrentViewModel = new SettingViewModel();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}