using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Models;
using UPOSS.Services;
using Squirrel;

namespace UPOSS.ViewModels
{
    public class AuthViewModel : ViewModelBase
    {
        APIService ObjAuthService;
        private string _UserPath;
        private string _BranchPath;

        public AuthViewModel()
        {
            ObjAuthService = new APIService();
            _UserPath = "user";
            _BranchPath = "branch";

            loginCommand = new AsyncRelayCommand(Login, this);
            exitCommand = new RelayCommand(Exit);

            InputUser = new User();
            ActiveBranchList = new ObservableCollection<string>();
            SelectedBranch = "";

            GetLoginBranchList();
            CheckAvailableUpdate();

            // for reset local db
            //Properties.Settings.Default.Setting_System_IsFirstLogin = true;
            //Properties.Settings.Default.Save();
        }


        #region Define
        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }

        public event EventHandler LoginCompleted;
        protected virtual void OnLoginCompleted(EventArgs e)
        {
            EventHandler handler = LoginCompleted;
            handler?.Invoke(this, e);
        }

        private ObservableCollection<string> activeBranchList;
        public ObservableCollection<string> ActiveBranchList
        {
            get { return activeBranchList; }
            set { activeBranchList = value; OnPropertyChanged("ActiveBranchList"); }
        }

        private User inputUser;
        public User InputUser
        {
            get { return inputUser; }
            set { inputUser = value; OnPropertyChanged("InputUser"); }
        }

        private string selectedBranch;
        public string SelectedBranch
        {
            get { return selectedBranch; }
            set { selectedBranch = value; OnPropertyChanged("SelectedBranch"); }
        }
        #endregion


        #region CustomOperation
        private async void GetLoginBranchList()
        {
            try
            {
                dynamic param = new { page = 0 };

                RootBranchObject Response = await ObjAuthService.PostAPI("getLoginBranchList", param, _BranchPath);

                if (Response.Status == "ok")
                {
                    ActiveBranchList = new ObservableCollection<string>(Response.Data.OrderBy(property => property.Name).Select(item => item.Name));

                    if (ActiveBranchList.Contains(Properties.Settings.Default.CurrentBranch))
                    {
                        SelectedBranch = Properties.Settings.Default.CurrentBranch;
                    } else
                    {
                        SelectedBranch = ActiveBranchList[0];
                    }
                }
                else
                {
                    MessageBox.Show(Response.Msg, "UPO$$");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }

        
        private async void CheckAvailableUpdate()
        {
            IsLoading = true;
            try 
            {
                using (var updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/kksboltibay/uposs-frontend"))
                {
                    UPOSS.Properties.Settings.Default.CurrentApplicationVersion = updateManager.CurrentlyInstalledVersion()  == null ? "-" : updateManager.CurrentlyInstalledVersion().ToString();
                    UPOSS.Properties.Settings.Default.Save();

                    var updateInfo = await updateManager.CheckForUpdate();

                    if (updateInfo.ReleasesToApply.Count > 0)
                    {
                        var update = MessageBox.Show("There is an update available, do you want to update?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        if (update == MessageBoxResult.Yes)
                        {
                            // backup local setting before update
                            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                            {
                                connection.Open();
                                using var command = new SQLiteCommand(connection);
                                
                                command.CommandText = "UPDATE settings SET address = @address, phone_no = @phone_no, gov_charge_name = @gov_charge_name, gov_charge_value = @gov_charge_value, gov_charge_no = @gov_charge_no, scanner_is_used = @scanner_is_used, is_first_login = @is_first_login, is_update = @is_update WHERE id = 1";
                                command.Parameters.AddWithValue("@address", Properties.Settings.Default.Setting_SystemAddress);
                                command.Parameters.AddWithValue("@phone_no", Properties.Settings.Default.Setting_SystemPhoneNo);
                                command.Parameters.AddWithValue("@gov_charge_name", Properties.Settings.Default.Setting_GovChargesName);
                                command.Parameters.AddWithValue("@gov_charge_value", Properties.Settings.Default.Setting_GovChargesValue);
                                command.Parameters.AddWithValue("@gov_charge_no", Properties.Settings.Default.Setting_GovChargesNo);
                                command.Parameters.AddWithValue("@scanner_is_used", Properties.Settings.Default.Setting_ScannerIsUsed);
                                command.Parameters.AddWithValue("@is_first_login", Properties.Settings.Default.Setting_System_IsFirstLogin);
                                command.Parameters.AddWithValue("@is_update", 1);
                                command.Prepare();
                                command.ExecuteNonQuery();

                                connection.Close();
                            }

                            // proceed update
                            await updateManager.UpdateApp();

                            MessageBox.Show("Updated succesfuly. Please restart the application.");

                            // shut down
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString() != "Response status code does not indicate success: 404 (Not Found).")
                {
                    MessageBox.Show(e.Message.ToString() + "\n\nUpdate checking process error, please contact IT support", "UPO$$");
                }
            }
            IsLoading = false;
        }
        #endregion


        #region Login
        private AsyncRelayCommand loginCommand;
        public AsyncRelayCommand LoginCommand
        {
            get { return loginCommand; }
        }
        private async Task Login()
        {
            try
            {
                dynamic param = new { username = InputUser.Username, password = InputUser.Password, branchName = SelectedBranch };

                RootUserObject Response = await ObjAuthService.PostAPI("login", param, _UserPath);

                if (Response.Status != "ok")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                }
                else
                {
                    Properties.Settings.Default.CurrentUsername = Response.Data[0].Username;
                    Properties.Settings.Default.CurrentBranch = SelectedBranch;
                    Properties.Settings.Default.CurrentUserRole = Response.Data[0].Role;
                    Properties.Settings.Default.Save();

                    IsLoading = false;

                    //change viewModel to Dashboard screen
                    OnLoginCompleted(EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion


        #region Exit
        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get { return exitCommand; }
        }
        private void Exit()
        {
            var msgBoxResult = MessageBox.Show("Do you want to exit UPO$$ ?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (msgBoxResult == MessageBoxResult.Yes)
            {
                // shut down
                Application.Current.Shutdown();
            }
        }
        #endregion
    }
}
