using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using UPOSS.Commands;
using UPOSS.State;
using UPOSS.State.Navigators;

namespace UPOSS.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public INavigator Navigator { get; set; } = new Navigator();
    }
}
