using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
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

            searchCommand = new RelayCommand(Search);
            addCommand = new RelayCommand(Add);
            updateCommand = new RelayCommand(Update);
            //deleteCommand = new RelayCommand(Delete);
            activateCommand = new RelayCommand(Activate);
            deactivateCommand = new RelayCommand(Deactivate);
            previousPageCommand = new RelayCommand(PrevPage);
            nextPageCommand = new RelayCommand(NextPage);

            InputBranch = new Branch();
            SelectedBranch = new Branch();
            SelectedStatus = "Active";
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
        }

        #region Define
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

                dynamic param = new { page = currentPage, branchName = InputBranch.Name, is_active = SelectedStatus == "All" ? null : SelectedStatus };

                RootBranchObject Response = await ObjBranchService.PostAPI("getBranchList", param, _Path);

                if (Response.Status != "ok")
                {
                    MessageBox.Show(Response.Msg, "UPO$$");
                    BranchList = null;
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
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get { return addCommand; }
        }

        private async void Add()
        {
            try
            {
                BranchInputDialog _defaultInputDialog = new BranchInputDialog("What is the new branch name to be added ?", "add");

                if (_defaultInputDialog.ShowDialog() == true)
                {
                    if (_defaultInputDialog.Result is null || _defaultInputDialog.Result.Name == "")
                    {
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
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
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


        //#region DeleteOperation
        //private RelayCommand deleteCommand;
        //public RelayCommand DeleteCommand
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
        private RelayCommand activateCommand;
        public RelayCommand ActivateCommand
        {
            get { return activateCommand; }
        }
        private async void Activate()
        {
            try
            {
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else if (SelectedBranch.Is_active == "Active")
                {
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


        #region DeactivateOperation
        private RelayCommand deactivateCommand;
        public RelayCommand DeactivateCommand
        {
            get { return deactivateCommand; }
        }
        private async void Deactivate()
        {
            try
            {
                if (SelectedBranch is null || SelectedBranch.Id == 0)
                {
                    MessageBox.Show("Please select a branch from the list", "UPO$$");
                }
                else if (SelectedBranch.Is_active == "Inactive")
                {
                    MessageBox.Show("Branch is Inactive", "UPO$$");
                }
                else if (SelectedBranch.Name == Properties.Settings.Default.CurrentBranch)
                {
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
