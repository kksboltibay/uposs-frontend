using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

            searchCommand = new RelayCommand(Search);
            addCommand = new RelayCommand(Add);
            updateCommand = new RelayCommand(Update);
            deleteCommand = new RelayCommand(Delete);
            previousPageCommand = new RelayCommand(PrevPage);
            nextPageCommand = new RelayCommand(NextPage);

            SelectedUser = new User();
            InputUser = new User();
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
        }


        #region Define
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
        private RelayCommand searchCommand;
        public RelayCommand SearchCommand
        {
            get { return searchCommand; }
        }
        private async void Search()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                dynamic param = new { page = currentPage, username = InputUser.Username };

                RootUserObject Response = await ObjUserService.PostAPI("getUserList", param, _Path);

                if (Response.Status != "ok")
                {
                    MessageBox.Show(Response.Msg, "UPO$$");
                    UserList = null;
                    Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
                }
                else
                {
                    //record section
                    var totalRecord = Response.Total;
                    var fromRecord = (currentPage * 70) - 69;
                    var toRecord = totalRecord - (currentPage * 70) <= 0 ? totalRecord : currentPage * 70;


                    //page section
                    var totalPage = Convert.ToInt32(Math.Ceiling((double)totalRecord / 70));

                    Pagination = new Pagination
                    {
                        CurrentRecord = fromRecord.ToString() + " ~ " + toRecord.ToString(),
                        TotalRecord = totalRecord,

                        CurrentPage = currentPage,
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
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get { return addCommand; }
        }

        private async void Add()   
        {
            try
            {
                UserInputDialog _defaultInputDialog = new UserInputDialog("Please fill in the details of new account", mode: "add");

                if (_defaultInputDialog.ShowDialog() == true)
                {
                    if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                    {
                        MessageBox.Show("New username can't be empty", "UPO$$");
                    }
                    else if (_defaultInputDialog.Result.Password == "")
                    {
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
                            Search();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                Search();
            }
        }
        #endregion


        #region UpdateOperation
        private RelayCommand updateCommand;
        public RelayCommand UpdateCommand
        {
            get { return updateCommand; }
        }
        private async void Update()
        {
            try
            {
                if (SelectedUser is null || SelectedUser.Id == 0)
                {
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else
                {
                    UserInputDialog _defaultInputDialog = new UserInputDialog("Please fill in the details of the account", mode: "update", SelectedUser);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                        {
                            MessageBox.Show("Dialog error, please contact IT suppport", "UPO$$");
                        }
                        else if (_defaultInputDialog.Result.Password == "")
                        {
                            MessageBox.Show("New password can't be empty", "UPO$$");
                        }
                        else
                        {
                            dynamic param = new
                            {
                                userID = SelectedUser.Id,
                                //username = _defaultInputDialog.Result.Username, // username cannot be changed
                                password = _defaultInputDialog.Result.Password,
                                role = _defaultInputDialog.Result.Role,
                                branchName = _defaultInputDialog.Result.Branch_name
                            };

                            RootUserObject Response = await ObjUserService.PostAPI("updateUser", param, _Path);

                            MessageBox.Show(Response.Msg, "UPO$$");

                            if (Response.Status is "ok")
                            {
                                RefreshTextBox();
                                Search();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                Search();
            }

        }
        #endregion


        #region DeleteOperation
        private RelayCommand deleteCommand;
        public RelayCommand DeleteCommand
        {
            get { return deleteCommand; }
        }
        private async void Delete()
        {
            try
            {
                if (SelectedUser is null || SelectedUser.Id == 0)
                {
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else if (SelectedUser.Role == "Super Admin")
                {
                    MessageBox.Show("Super admin account can't be deleted", "UPO$$");
                }
                else
                {
                    UserInputDialog _defaultInputDialog = new UserInputDialog("Are you sure you want to delete this account ?", mode: "delete", SelectedUser);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Username == "")
                        {
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
                                Search();
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
                Search();
            }

        }
        #endregion


        #region PrevPageOperation
        private RelayCommand previousPageCommand;
        public RelayCommand PreviousPageCommand
        {
            get { return previousPageCommand; }
        }
        private void PrevPage()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                if (currentPage > 1)
                {
                    Pagination = new Pagination { CurrentPage = --currentPage };

                    Search();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                Search();
            }
        }
        #endregion


        #region NextPageOperation
        private RelayCommand nextPageCommand;
        public RelayCommand NextPageCommand
        {
            get { return nextPageCommand; }
        }
        private void NextPage()
        {
            try
            {
                var currentPage = Pagination.CurrentPage;

                if (currentPage > 0 && (currentPage * 70) < Pagination.TotalRecord)
                {
                    Pagination = new Pagination { CurrentPage = ++currentPage };

                    Search();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                RefreshTextBox();
                Search();
            }
        }
        #endregion
    }
}
