using System.Windows;
using UPOSS.Custom;
using UPOSS.LocalDatabase;
using UPOSS.State.Navigators;

namespace UPOSS.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public INavigator Navigator { get; set; } = new Navigator();

        public DashboardViewModel()
        {
            SyncDB();
        }

        #region Definition
        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }
        #endregion


        private async void SyncDB()
        {
            IsLoading = true;

            SQLiteDatabase DB = new SQLiteDatabase(this);
            await DB.LoadLocalDatabase();

            IsLoading = false;
        }
    }
}
