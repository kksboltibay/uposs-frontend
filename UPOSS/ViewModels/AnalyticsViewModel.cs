using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    class AnalyticsViewModel : ViewModelBase
    {
        APIService ObjAnalyticsService;
        private string _Path;

        public AnalyticsViewModel()
        {
            ObjAnalyticsService = new APIService();
            _Path = "analytics";
            GetActiveBranchList();

            // Sales Report
            searchCommand = new AsyncRelayCommand(Search, this);
            //addCommand = new AsyncRelayCommand(Add, this);
            //updateCommand = new AsyncRelayCommand(Update, this);
            //deactivateCommand = new AsyncRelayCommand(Deactivate, this);
            //previousPageCommand = new AsyncRelayCommand(PrevPage, this);
            //nextPageCommand = new AsyncRelayCommand(NextPage, this);

            InputSales = new Analytics();
            InputSales.Filter_created_at = new Datetime();
            InputSales.Filter_updated_at = new Datetime();
            SelectedSales = new Analytics();
            SelectedProduct = new Product();
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };

            // default date = today
        }

        #region Define
        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }

        //Filter section
        private Analytics inputSales;
        public Analytics InputSales
        {
            get { return inputSales; }
            set { inputSales = value; OnPropertyChanged("InputSales"); }
        }

        private ObservableCollection<string> branchList;
        public ObservableCollection<string> BranchList
        {
            get { return branchList; }
            set { branchList = value; OnPropertyChanged("BranchList"); }
        }

        private string selectedBranch;
        public string SelectedBranch
        {
            get { return selectedBranch; }
            set { selectedBranch = value; OnPropertyChanged("SelectedBranch"); }
        }

        //Main section
        private ObservableCollection<Analytics> salesList;
        public ObservableCollection<Analytics> SalesList
        {
            get { return salesList; }
            set { salesList = value; OnPropertyChanged("SalesList"); }
        }

        private Analytics selectedSales;
        public Analytics SelectedSales
        {
            get { return selectedSales; }
            set { selectedSales = value; OnPropertyChanged("SelectedSales"); }
        }

        private Product selectedProduct;
        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set { selectedProduct = value; OnPropertyChanged("SelectedProduct"); }
        }

        private Pagination pagination;
        public Pagination Pagination
        {
            get { return pagination; }
            set { pagination = value; OnPropertyChanged("Pagination"); }
        }
        #endregion

        #region CustomOperation
        private async void GetActiveBranchList()
        {
            try
            {
                dynamic param = new { page = 0 };

                RootBranchObject Response = await ObjAnalyticsService.PostAPI("getBranchList", param, "branch");

                if (Response.Status == "ok")
                {
                    BranchList = new ObservableCollection<string>(Response.Data.OrderBy(property => property.Name).Select(item => item.Name));
                    BranchList.Add("All");
                }
                else
                {
                    BranchList = new ObservableCollection<string> { "- Fail to get branch list -" };
                    BranchList.Add("All");
                }
            }
            catch (Exception e)
            {
                BranchList = new ObservableCollection<string> { "- Fail to get branch list -" };
                BranchList.Add("All");
            }
        }

        //private void RefreshTextBox()
        //{
        //    InputProduct = new Product
        //    {
        //        Name = "",
        //        Category = "",
        //        Design_code = "",
        //        Colour_code = "",
        //        Price = ""
        //    };

        //    SelectedBranch = "";
        //    SelectedStatus = "Active";

        //}
        #endregion

        #region SearchOperation
        private AsyncRelayCommand searchCommand;
        public AsyncRelayCommand SearchCommand
        {
            get { return searchCommand; }
        }
        private async Task Search()
        {
            var currentPage = Pagination.CurrentPage;

            try
            {
                dynamic param = new
                {
                    page = currentPage,
                    createdAt = new 
                    {
                        from = InputSales.Filter_created_at.From != "" ? DateTime.ParseExact(InputSales.Filter_created_at.From, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSales.Filter_created_at.To != "" ? DateTime.ParseExact(InputSales.Filter_created_at.To, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    updatedAt = new
                    {
                        from = InputSales.Filter_updated_at.From != "" ? DateTime.ParseExact(InputSales.Filter_updated_at.From, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSales.Filter_updated_at.To != "" ? DateTime.ParseExact(InputSales.Filter_updated_at.To, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    //receiptNo = InputSales.Receipt_no,

                    //productNo = InputSales.Product_no,
                    //name = InputSales.Name,
                    //category = InputSales.Category,
                    //design_code = InputSales.Design_code,
                    //colour_code = InputSales.Colour_code,
                    //price = InputSales.Price,
                    //barcode = InputSales.Barcode,
                    //branchName = SelectedBranch == "All" ? null : SelectedBranch,

                };

                RootAnalyticsObject Response = await ObjAnalyticsService.PostAPI("getSalesList", param, _Path);

                if (Response.Status != "ok" || Response.Msg == "No result found")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                    return;
                }

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
                SalesList = new ObservableCollection<Analytics>(Response.Data);

                for (int i = 0; i < SalesList.Count; i++)
                {
                    SalesList[i].Id = i + 1;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }

        #endregion
    }
}
