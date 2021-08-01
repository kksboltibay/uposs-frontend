using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UPOSS.Commands;
using UPOSS.Controls;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    public class BranchViewModel : ViewModelBase
    {
        APIService ObjBranchService;
        private string _Path;

        public BranchViewModel()
        {
            ObjBranchService = new APIService();
            _Path = "branch";

            LoadStatusList();

            searchCommand = new AsyncRelayCommand(Search, this);
            addCommand = new AsyncRelayCommand(Add, this);
            updateCommand = new AsyncRelayCommand(Update, this);
            //deleteCommand = new AsyncRelayCommand(Delete, this);
            activateCommand = new AsyncRelayCommand(Activate, this);
            deactivateCommand = new AsyncRelayCommand(Deactivate, this);
            previousPageCommand = new AsyncRelayCommand(PrevPage, this);
            nextPageCommand = new AsyncRelayCommand(NextPage, this);

            InputBranch = new Branch();
            SelectedBranch = new Branch();
            SelectedStatus = "Active";
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
        }


        #region Define
        //Loading Screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }


        private ObservableCollection<Branch> branchList;
        public ObservableCollection<Branch> BranchList
        {
            get { return branchList; }
            set { branchList = value; OnPropertyChanged("BranchList"); }
        }

        private ObservableCollection<string> statusList;
        public ObservableCollection<string> StatusList
        {
            get { return statusList; }
            set { statusList = value; OnPropertyChanged("StatusList"); }
        }

        private Branch inputBranch;
        public Branch InputBranch
        {
            get { return inputBranch; }
            set { inputBranch = value; OnPropertyChanged("InputBranch"); }
        }

        private Branch selectedBranch;
        public Branch SelectedBranch
        {
            get { return selectedBranch; }
            set 
            { 
                selectedBranch = value; 
                OnPropertyChanged("SelectedBranch");
            }
        }

        private string selectedStatus;
        public string SelectedStatus
        {
            get { return selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); }
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
            InputBranch = new Branch
            {
                Name = ""
            };

            SelectedStatus = "Active";
        }

        private void LoadStatusList()
        {
            StatusList = new ObservableCollection<string>();
            StatusList.Add("Active");
            StatusList.Add("Inactive");
            StatusList.Add("All");
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

                dynamic param = new { page = currentPage, branchName = InputBranch.Name, is_active = SelectedStatus == "All" ? null : SelectedStatus };

                RootBranchObject Response = await ObjBranchService.PostAPI("getBranchList", param, _Path);

                if (Response.Status != "ok")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                    BranchList = null;
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
                    BranchList = new ObservableCollection<Branch>(Response.Data.OrderBy(property => property.Id));

                    for (int i = 0; i < BranchList.Count; i++)
                    {
                        BranchList[i].Id = i + 1;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                BranchList = null;
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
                BranchInputDialog _defaultInputDialog = new BranchInputDialog("What is the new branch name to be added ?", "add");

                if (_defaultInputDialog.ShowDialog() == true)
                {
                    if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Name == "")
                    {
                        IsLoading = false;
                        MessageBox.Show("New branch name can't be empty", "UPO$$");
                    }
                    else
                    {
                        dynamic param = new { branchName = _defaultInputDialog.Result.Name };

                        RootBranchObject Response = await ObjBranchService.PostAPI("addBranch", param, _Path);

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
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else
                {
                    BranchInputDialog _defaultInputDialog = new BranchInputDialog("Please fill in new branch name", "update", SelectedBranch);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        dynamic param = new { oldBranchName = SelectedBranch.Name, newBranchName = _defaultInputDialog.Result.Name };

                        RootBranchObject Response = await ObjBranchService.PostAPI("updateBranch", param, _Path);

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


        //#region DeleteOperation
        //private AsyncRelayCommand deleteCommand;
        //public AsyncRelayCommand DeleteCommand
        //{
        //    get { return deleteCommand; }
        //}
        //private async void Delete()
        //{
        //    try
        //    {
        //        if (SelectedBranch is null || SelectedBranch.Id == 0)
        //        {
        //            MessageBox.Show("Please select a branch from the list", "UPO$$");
        //        }
        //        else if (SelectedBranch.Name == Properties.Settings.Default.CurrentBranch)
        //        {
        //            MessageBox.Show("Current branch can't be deleted, please switch to another branch first", "UPO$$");
        //        }
        //        else
        //        {
        //            BranchInputDialog _defaultInputDialog = new BranchInputDialog
        //                    (
        //                        "Do you want to delete \"" + SelectedBranch.Name + "\" ?\n" +
        //                        "The deleted branch name will not be able to use until the system fully deletes it. (Estimated time: several months) \n\n" +
        //                        "Note: All users assigned under \"" + SelectedBranch.Name + "\" will also be deleted", "delete", SelectedBranch
        //                    );

        //            if (_defaultInputDialog.ShowDialog() == true)
        //            {
        //                dynamic param = new { branchID = SelectedBranch.Id, branchName = SelectedBranch.Name };

        //                RootBranchObject Response = await ObjBranchService.PostAPI("deleteBranch", param, _Path);

        //                MessageBox.Show(Response.Msg, "UPO$$");

        //                if (Response.Status is "ok")
        //                {
        //                    RefreshTextBox();
        //                    Search();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message.ToString(), "UPO$$");
        //        RefreshTextBox();
        //        Search();
        //    }
        //}
        //#endregion


        #region ActivateOperation
        private AsyncRelayCommand activateCommand;
        public AsyncRelayCommand ActivateCommand
        {
            get { return activateCommand; }
        }
        private async Task Activate()
        {
            try
            {
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else if (SelectedBranch.Is_active == "Active")
                {
                    IsLoading = false;
                    MessageBox.Show("Branch is active", "UPO$$");
                }
                else
                {
                    BranchInputDialog _defaultInputDialog = new BranchInputDialog
                        (
                            "Do you want to activate \"" + SelectedBranch.Name + "\" ?\n",
                            mode: "activate",
                            SelectedBranch
                        );

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        var param = new { branchName = SelectedBranch.Name };

                        RootBranchObject Response = await ObjBranchService.PostAPI("activateBranch", param, _Path);

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


        #region DeactivateOperation
        private AsyncRelayCommand deactivateCommand;
        public AsyncRelayCommand DeactivateCommand
        {
            get { return deactivateCommand; }
        }
        private async Task Deactivate()
        {
            try
            {
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else if (SelectedBranch.Is_active == "Inactive")
                {
                    IsLoading = false;
                    MessageBox.Show("Branch is Inactive", "UPO$$");
                }
                else if (SelectedBranch.Name == Properties.Settings.Default.CurrentBranch)
                {
                    IsLoading = false;
                    MessageBox.Show("Current branch can't be deactivated, please switch to another branch first", "UPO$$");
                }
                else
                {
                    BranchInputDialog _defaultInputDialog = new BranchInputDialog
                        (
                            "Do you want to deactivate \"" + SelectedBranch.Name + "\" ?\n" +
                            "Note: Please make sure all stocks of the Branch : \"" + SelectedBranch.Name + "\" have already been cleared \n" +
                            "           (All users assigned under \"" + SelectedBranch.Name + "\" will also be -Deleted-)",
                            mode: "deactivate",
                            SelectedBranch
                        );

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        var param = new { branchName = SelectedBranch.Name };

                        RootBranchObject Response = await ObjBranchService.PostAPI("deactivateBranch", param, _Path);

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
