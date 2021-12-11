using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Controls;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        APIService ObjUserService;
        private string _Path;

        public UserViewModel()
        {
            ObjUserService = new APIService();
            _Path = "user";

            searchCommand = new AsyncRelayCommand(Search, this);
            addCommand = new AsyncRelayCommand(Add, this);
            updateCommand = new AsyncRelayCommand(Update, this);
            deleteCommand = new AsyncRelayCommand(Delete, this);
            previousPageCommand = new AsyncRelayCommand(PrevPage, this);
            nextPageCommand = new AsyncRelayCommand(NextPage, this);

            SelectedUser = new User();
            InputUser = new User();
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
        }


        #region Define
        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }


        private ObservableCollection<User> userList;
        public ObservableCollection<User> UserList
        {
            get { return userList; }
            set { userList = value; OnPropertyChanged("UserList"); }
        }

        private User inputUser;
        public User InputUser
        {
            get { return inputUser; }
            set { inputUser = value; OnPropertyChanged("InputUser"); }
        }

        private User selectedUser;
        public User SelectedUser
        {
            get { return selectedUser; }
            set { selectedUser = value; OnPropertyChanged("SelectedUser"); }
        }

        private Pagination pagination;
        public Pagination Pagination
        {
            get { return pagination; }
            set { pagination = value; OnPropertyChanged("Pagination"); }
        }
        #endregion


        #region CustomOperation
        private void RefreshTextBox()
        {
            InputUser = new User
            {
                Username = ""
            };
        }
        #endregion


        #region SearchOperation
        private AsyncRelayCommand searchCommand;
        public AsyncRelayCommand SearchCommand
        {
            get { return searchCommand; }
        }
        private async Task Search()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                dynamic param = new { page = currentPage, username = InputUser.Username };

                RootUserObject Response = await ObjUserService.PostAPI("getUserList", param, _Path);

                if (Response.Status != "ok")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                    UserList = null;
                    Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
                }
                else
                {
                    //record section
                    var totalRecord = Response.Total;
                    var fromRecord = currentPage == 0 ? 1 : (currentPage * 70) - 69;
                    var toRecord = currentPage == 0 ? totalRecord : (fromRecord + 69 < totalRecord ? fromRecord + 69 : totalRecord);

                    //page section
                    var totalPage = currentPage == 0 ? 1 : Convert.ToInt32(Math.Ceiling((double)totalRecord / 70));

                    Pagination = new Pagination
                    {
                        CurrentRecord = fromRecord.ToString() + " ~ " + toRecord.ToString(),
                        TotalRecord = totalRecord,

                        CurrentPage = currentPage == 0 ? 1 : currentPage,
                        TotalPage = totalPage
                    };

                    //datagrid
                    UserList = new ObservableCollection<User>(Response.Data.OrderBy(property => property.Id));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                UserList = null;
                Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
            }

            RefreshTextBox();
        }
        #endregion


        #region AddOperation
        private AsyncRelayCommand addCommand;
        public AsyncRelayCommand AddCommand
        {
            get { return addCommand; }
        }

        private async Task Add()   
        {
            try
            {
                UserInputDialog _defaultInputDialog = new UserInputDialog("Please fill in the details of new account", mode: "add");

                if (_defaultInputDialog.ShowDialog() == true)
                {
                    if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                    {
                        IsLoading = false;
                        MessageBox.Show("New username can't be empty", "UPO$$");
                    }
                    else if (_defaultInputDialog.Result.Password == "")
                    {
                        IsLoading = false;
                        MessageBox.Show("New password can't be empty", "UPO$$");
                    }
                    else
                    {
                        dynamic param = new 
                        { 
                            username = _defaultInputDialog.Result.Username, 
                            password = _defaultInputDialog.Result.Password, 
                            role = _defaultInputDialog.Result.Role,
                            branchName = _defaultInputDialog.Result.Branch_name 
                        };

                        RootUserObject Response = await ObjUserService.PostAPI("addUser", param, _Path);

                        MessageBox.Show(Response.Msg, "UPO$$");

                        if (Response.Status is "ok")
                        {
                            RefreshTextBox();
                            await Search();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                await Search();
            }
        }
        #endregion


        #region UpdateOperation
        private AsyncRelayCommand updateCommand;
        public AsyncRelayCommand UpdateCommand
        {
            get { return updateCommand; }
        }
        private async Task Update()
        {
            try
            {
                if (SelectedUser is null || SelectedUser.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select an item from the list", "UPO$$");
                }
                else
                {
                    UserInputDialog _defaultInputDialog = new UserInputDialog("Please fill in the details of the account", mode: "update", SelectedUser);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                        {
                            IsLoading = false;
                            MessageBox.Show("Dialog error, please contact IT suppport", "UPO$$");
                        }
                        else if (_defaultInputDialog.Result.Password == "")
                        {
                            IsLoading = false;
                            MessageBox.Show("New password can't be empty", "UPO$$");
                        }
                        else
                        {
                            dynamic param = new
                            {
                                currentUsername = Properties.Settings.Default.CurrentUsername,
                                userID = SelectedUser.Id,
                                password = _defaultInputDialog.Result.Password,
                                role = _defaultInputDialog.Result.Role,
                                branchName = _defaultInputDialog.Result.Branch_name
                            };

                            RootUserObject Response = await ObjUserService.PostAPI("updateUser", param, _Path);

                            MessageBox.Show(Response.Msg, "UPO$$");

                            if (Response.Status is "ok")
                            {
                                RefreshTextBox();
                                await Search();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                await Search();
            }

        }
        #endregion


        #region DeleteOperation
        private AsyncRelayCommand deleteCommand;
        public AsyncRelayCommand DeleteCommand
        {
            get { return deleteCommand; }
        }
        private async Task Delete()
        {
            try
            {
                if (SelectedUser is null || SelectedUser.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select an item from the list", "UPO$$");
                }
                else if (SelectedUser.Role == "Super Admin")
                {
                    IsLoading = false;
                    MessageBox.Show("Super admin account can't be deleted", "UPO$$");
                }
                else
                {
                    UserInputDialog _defaultInputDialog = new UserInputDialog("Are you sure you want to delete this account ?", mode: "delete", SelectedUser);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                        {
                            IsLoading = false;
                            MessageBox.Show("Dialog error, please contact IT suppport", "UPO$$");
                        }
                        else
                        {
                            dynamic param = new { userID = SelectedUser.Id };

                            RootUserObject Response = await ObjUserService.PostAPI("deleteUser", param, _Path);

                            MessageBox.Show(Response.Msg, "UPO$$");

                            if (Response.Status is "ok")
                            {
                                RefreshTextBox();
                                await Search();
                                SelectedUser = new User();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                await Search();
            }

        }
        #endregion


        #region PrevPageOperation
        private AsyncRelayCommand previousPageCommand;
        public AsyncRelayCommand PreviousPageCommand
        {
            get { return previousPageCommand; }
        }
        private async Task PrevPage()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                if (currentPage > 1 && currentPage <= Pagination.TotalPage)
                {
                    Pagination = new Pagination { CurrentPage = --currentPage };

                    await Search();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                await Search();
            }
        }
        #endregion


        #region NextPageOperation
        private AsyncRelayCommand nextPageCommand;
        public AsyncRelayCommand NextPageCommand
        {
            get { return nextPageCommand; }
        }
        private async Task NextPage()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                if (currentPage > 0 && currentPage < Pagination.TotalPage)
                {
                    Pagination = new Pagination { CurrentPage = ++currentPage };

                    await Search();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                await Search();
            }
        }
        #endregion
    }
}
