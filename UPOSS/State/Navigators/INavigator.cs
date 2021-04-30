using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using UPOSS.ViewModels;

namespace UPOSS.State.Navigators
{
    public enum ViewType
    {
        User,
        Branch,
        Product,
        Cashier,
        Sales,
        Help
    }
    public interface INavigator
    {
        ViewModelBase CurrentViewModel { get; set; }
        ICommand UpdateCurrentViewModelCommand { get; }
    }
}
