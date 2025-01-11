using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Controls.Dialog;
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

            searchCommand = new AsyncRelayCommand(Search, this);
            voidCommand = new AsyncRelayCommand(Void, this);
            reprintReceiptCommand = new AsyncRelayCommand(ReprintReceipt, this);
            printReportCommand = new AsyncRelayCommand(PrintReport, this);
            previousPageCommand = new AsyncRelayCommand(PrevPage, this);
            nextPageCommand = new AsyncRelayCommand(NextPage, this);

            InputSales = new Analytics();
            InputSales.Filter_created_at = new Datetime();
            InputSales.Filter_updated_at = new Datetime();
            InputProduct = new Product();
            SelectedBranch = Properties.Settings.Default.CurrentBranch;
            SelectedStatus = "All";
            SelectedSales = new Analytics();
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

        //Filter section
        private Analytics inputSales;
        public Analytics InputSales
        {
            get { return inputSales; }
            set { inputSales = value; OnPropertyChanged("InputSales"); }
        }

        private Product inputProduct;
        public Product InputProduct
        {
            get { return inputProduct; }
            set { inputProduct = value; OnPropertyChanged("InputProduct"); }
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

        private ObservableCollection<string> statusList;
        public ObservableCollection<string> StatusList
        {
            get { return statusList; }
            set { statusList = value; OnPropertyChanged("StatusList"); }
        }

        private string selectedStatus;
        public string SelectedStatus
        {
            get { return selectedStatus; }
            set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); }
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

        private void RefreshTextBox()
        {
            InputSales.Receipt_no = "";
            InputProduct.Product_no = "";
            InputProduct.Name = "";
            InputProduct.Barcode = "";
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
            var currentPage = Pagination.CurrentPage;

            try
            {
                dynamic param = new
                {
                    page = currentPage,
                    createdAt = new 
                    {
                        from = InputSales.Filter_created_at.From != "" ? DateTime.ParseExact(InputSales.Filter_created_at.From, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSales.Filter_created_at.To != "" ? DateTime.ParseExact(InputSales.Filter_created_at.To, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    updatedAt = new
                    {
                        from = InputSales.Filter_updated_at.From != "" ? DateTime.ParseExact(InputSales.Filter_updated_at.From, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSales.Filter_updated_at.To != "" ? DateTime.ParseExact(InputSales.Filter_updated_at.To, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    receiptNo = InputSales.Receipt_no,
                    productNo = InputProduct.Product_no,
                    name = InputProduct.Name,
                    barcode = InputProduct.Barcode,
                    branchName = SelectedBranch == "All" ? null : SelectedBranch,
                    is_void = SelectedStatus == "All" ? null : SelectedStatus
                };

                RootAnalyticsObject Response = await ObjAnalyticsService.PostAPI("getSalesList", param, _Path);

                if (Response.Status != "ok" || Response.Msg == "No result found")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                    SalesList = null;
                    Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
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
                SalesList = null;
                Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
            }
        }
        #endregion

        #region VoidOperation
        private AsyncRelayCommand voidCommand;
        public AsyncRelayCommand VoidCommand
        {
            get { return voidCommand; }
        }
        private async Task Void()
        {
            try
            {
                if (SelectedSales is null || SelectedSales.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a sales from the list", "UPO$$");
                }
                else if (SelectedSales.Status == "Voided")
                {
                    IsLoading = false;
                    MessageBox.Show("Selected sales is voided", "UPO$$");
                }
                else
                {
                    var msgBoxResult = MessageBox.Show("Do you want to void \"" + SelectedSales.Receipt_no + "\" ?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        var param = new { receiptNo = SelectedSales.Receipt_no };

                        RootAnalyticsObject Response = await ObjAnalyticsService.PostAPI("voidSales", param, _Path);

                        MessageBox.Show(Response.Msg, "UPO$$");

                        await Search();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                await Search();
            }
        }
        #endregion

        #region ReprintReceiptOperation
        private AsyncRelayCommand reprintReceiptCommand;
        public AsyncRelayCommand ReprintReceiptCommand
        {
            get { return reprintReceiptCommand; }
        }
        private async Task ReprintReceipt()
        {
            try
            {
                if (SelectedSales is null || SelectedSales.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a sales from the list", "UPO$$");
                }
                else
                {
                    if (SelectedSales.Status == "Voided")
                    {
                        var msgBoxResult = MessageBox.Show("Selected sales is already voided, do you want to continue?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (msgBoxResult == MessageBoxResult.No)
                        {
                            IsLoading = false;
                            return;
                        }
                    }

                    //print
                    dynamic firstParam = new
                    {
                        cartList = SelectedSales.ProductList,
                        totalItem = SelectedSales.ProductList.Count,
                        totalSubtotal = Math.Round(Convert.ToDecimal(
                                Math.Round(Convert.ToDecimal(SelectedSales.Total_amount), 2, MidpointRounding.AwayFromZero) - 
                                Math.Round(Convert.ToDecimal(SelectedSales.Total_tax), 2, MidpointRounding.AwayFromZero) + 
                                Math.Round(Convert.ToDecimal(SelectedSales.Total_discount), 2, MidpointRounding.AwayFromZero)
                        ), 2, MidpointRounding.AwayFromZero).ToString(),
                        totalDiscount = SelectedSales.Total_discount,
                        totalTax = SelectedSales.Total_tax,
                        totalAmount = SelectedSales.Total_amount,
                        paymentMethod = SelectedSales.Payment_method,
                        cashPay = SelectedSales.Total_paid_amount,
                        cardNo = SelectedSales.Card_no,
                        cardPay = SelectedSales.Total_paid_amount,
                        cardType = SelectedSales.Card_type,
                        bankName = SelectedSales.Bank_name,
                        branchName = SelectedSales.Branch,
                        change = Math.Round(Convert.ToDecimal(
                                Math.Round(Convert.ToDecimal(SelectedSales.Total_paid_amount), 2, MidpointRounding.AwayFromZero) -
                                Math.Round(Convert.ToDecimal(SelectedSales.Total_amount), 2, MidpointRounding.AwayFromZero)
                        ), 2, MidpointRounding.AwayFromZero).ToString()
                    };

                    Cashier secondParam = new Cashier
                    {
                        Receipt_no = SelectedSales.Receipt_no,
                        Datetime = SelectedSales.Created_at
                    };

                    CashierPrintReceiptDialog _cashierPrintReceiptDialog = new CashierPrintReceiptDialog(firstParam, secondParam, SelectedSales.Cashier_username);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion

        #region PrintReportOperation
        private AsyncRelayCommand printReportCommand;
        public AsyncRelayCommand PrintReportCommand
        {
            get { return printReportCommand; }
        }
        private async Task PrintReport()
        {
            try
            {
                AnalyticsPRConfirmationDialog _analyticsPRConfirmationDialog = new AnalyticsPRConfirmationDialog();

                if (_analyticsPRConfirmationDialog.ShowDialog() != true)
                {
                    return;
                }
                
                var param = new
                { 
                    datetimeFrom = DateTime.ParseExact(_analyticsPRConfirmationDialog.datePickerFrom.SelectedDate.ToString(), "d/M/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"),
                    datetimeTo = DateTime.ParseExact(_analyticsPRConfirmationDialog.datePickerTo.SelectedDate.ToString(), "d/M/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"),
                    branchName = Properties.Settings.Default.CurrentBranch
                };

                RootAnalyticsObject Response = await ObjAnalyticsService.PostAPI("getSalesSummary", param, _Path);

                if (Response.Status != "ok")
                {
                    MessageBox.Show(Response.Msg, "UPO$$");
                    return;
                }

                string selectedDateFrom = DateTime.ParseExact(_analyticsPRConfirmationDialog.datePickerFrom.SelectedDate.ToString(), "d/M/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                string selectedDateTo = DateTime.ParseExact(_analyticsPRConfirmationDialog.datePickerTo.SelectedDate.ToString(), "d/M/yyyy h:m:s tt", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");

                AnalyticsPrintReportDialog _analyticsPrintReportDialog = new AnalyticsPrintReportDialog(Response.Data[0], selectedDateFrom, selectedDateTo);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
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
                await Search();
            }
        }
        #endregion
    }
}
