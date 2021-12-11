using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UPOSS.Commands
{
    public class AsyncRelayCommand : AsyncCommandBase
    {
        private readonly Func<Task> _callback;
        private dynamic _CurrentViewModel;

        public AsyncRelayCommand(Func<Task> callback, dynamic currentViewModel)
        {
            _callback = callback;
            _CurrentViewModel = currentViewModel;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            _CurrentViewModel.IsLoading = true;
            await _callback();
            _CurrentViewModel.IsLoading = false;
        }
    }
}
