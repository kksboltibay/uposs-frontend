using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using UPOSS.Commands;
using UPOSS.Controls;
using UPOSS.Controls.Dialog;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        APIService ObjProductService;
        private string _Path;

        public ProductViewModel()
        {
            ObjProductService = new APIService();
            _Path = "product";
            GetActiveBranchList();
            LoadStatusList();

            searchCommand = new AsyncRelayCommand(Search, this);
            addCommand = new AsyncRelayCommand(Add, this);
            updateCommand = new AsyncRelayCommand(Update, this);
            //deleteCommand = new AsyncRelayCommand(Delete, this);
            //restockCommand = new AsyncRelayCommand(Restock, this);
            scanRestockCommand = new AsyncRelayCommand(ScanRestock, this);
            activateCommand = new AsyncRelayCommand(Activate, this);
            deactivateCommand = new AsyncRelayCommand(Deactivate, this);
            previousPageCommand = new AsyncRelayCommand(PrevPage, this);
            nextPageCommand = new AsyncRelayCommand(NextPage, this);
            printBarcodeCommand = new AsyncRelayCommand(PrintBarcode, this);

            SelectedBranch = "";
            SelectedStatus = "Active";
            SelectedProduct = new Product();
            SelectedQuantity = new ProductQuantity();
            InputProduct = new Product();
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

        private Product inputProduct;
        public Product InputProduct
        {
            get { return inputProduct; }
            set { inputProduct = value; OnPropertyChanged("InputProduct"); }
        }

        //Main content section
        private ObservableCollection<Product> productList;
        public ObservableCollection<Product> ProductList
        {
            get { return productList; }
            set { productList = value; OnPropertyChanged("ProductList"); }
        }

        private ObservableCollection<ProductQuantity> quantityList;
        public ObservableCollection<ProductQuantity> QuantityList
        {
            get { return quantityList; }
            set { quantityList = value; OnPropertyChanged("QuantityList"); }
        }

        private Product selectedProduct;
        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set
            { 
                selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
                LoadProductQuantity();
            }
        }

        private ProductQuantity selectedQuantity;
        public ProductQuantity SelectedQuantity
        {
            get { return selectedQuantity; }
            set { selectedQuantity = value; OnPropertyChanged("SelectedQuantity"); }
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

                RootBranchObject Response = await ObjProductService.PostAPI("getBranchList", param, "branch");

                if (Response.Status == "ok")
                {
                    BranchList = new ObservableCollection<string>(Response.Data.OrderBy(property => property.Name).Select(item => item.Name));
                    BranchList.Add("All");
                }
                else
                {
                    BranchList = new ObservableCollection<string>{"- Fail to get branch list -"};
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
            InputProduct = new Product
            {
                Name = "",
                Category = "",
                Design_code = "",
                Colour_code = "",
                Price = ""
            };

            SelectedBranch = "";
            SelectedStatus = "Active";

        }

        private void LoadProductQuantity()
        {
            if (ProductList != null && SelectedProduct != null)
            {
                var product = ProductList.FirstOrDefault(e => e.Id == SelectedProduct.Id);

                if (product.Stock != null)
                    QuantityList = new ObservableCollection<ProductQuantity>(product.Stock);
                else
                    QuantityList = null;

                //QuantityList = new ObservableCollection<ProductQuantity>(ProductList.Where(e => e.Id == SelectedProduct.Id).Select(s => s.Stock).FirstOrDefault());
            }
            else
            {
                QuantityList = null;
            }
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

                dynamic param = new 
                { 
                    page = currentPage, 
                    productNo = InputProduct.Product_no, 
                    name = InputProduct.Name, 
                    category = InputProduct.Category, 
                    design_code = InputProduct.Design_code,
                    colour_code = InputProduct.Colour_code,
                    price = InputProduct.Price,
                    barcode = InputProduct.Barcode,
                    branchName = SelectedBranch == "All" ? null : SelectedBranch,
                    is_active = SelectedStatus == "All" ? null : SelectedStatus
                };

                RootProductObject Response = await ObjProductService.PostAPI("getProductList", param, _Path);

                if (Response.Status != "ok")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
                    ProductList = null;
                    QuantityList = null;
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
                    ProductList = new ObservableCollection<Product>(Response.Data.OrderBy(property => property.Product_no));

                    for (int i = 0; i < ProductList.Count; i++)
                    {
                        ProductList[i].Id = i + 1;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                ProductList = null;
                QuantityList = null;
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
                ProductInputDialog _defaultInputDialog = new ProductInputDialog("Please fill in the details of new product", mode: "add");

                if (_defaultInputDialog.ShowDialog() == true)
                {
                    if (
                        _defaultInputDialog.ProductResult is null || 
                        _defaultInputDialog.ProductResult.Name == "" ||
                        _defaultInputDialog.ProductResult.Category == "" ||
                        _defaultInputDialog.ProductResult.Design_code == "" ||
                        _defaultInputDialog.ProductResult.Colour_code == "" ||
                        _defaultInputDialog.ProductResult.Price == ""
                        )
                    {
                        IsLoading = false;
                        MessageBox.Show("Empty column detected, all columns can't be empty", "UPO$$");
                    }
                    else
                    {
                        dynamic param = new
                        {
                            name = _defaultInputDialog.ProductResult.Name,
                            category = _defaultInputDialog.ProductResult.Category,
                            designCode = _defaultInputDialog.ProductResult.Design_code,
                            colourCode = _defaultInputDialog.ProductResult.Colour_code,
                            price = _defaultInputDialog.ProductResult.Price
                        };

                        RootProductObject Response = await ObjProductService.PostAPI("addProduct", param, _Path);

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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive")
                {
                    IsLoading = false;
                    MessageBox.Show("Product is Inactive, only active product is allowed to be updated", "UPO$$");
                }
                else
                {
                    if (SelectedQuantity != null)
                    {
                        if (SelectedQuantity.Branch_name != null && SelectedQuantity.Branch_name != Properties.Settings.Default.CurrentBranch && Properties.Settings.Default.CurrentUserRole != "Super Admin")
                        {
                            IsLoading = false;
                            MessageBox.Show("Only superadmin is allowed to update other branch's stock", "UPO$$");
                            return;
                        }
                    }

                    ProductInputDialog _defaultInputDialog = new ProductInputDialog("Please fill in the details of product", mode: "update", SelectedProduct, SelectedQuantity);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (
                            _defaultInputDialog.ProductResult is null ||
                            _defaultInputDialog.ProductResult.Product_no == "" ||
                            _defaultInputDialog.ProductResult.Name == "" ||
                            _defaultInputDialog.ProductResult.Category == "" ||
                            _defaultInputDialog.ProductResult.Design_code == "" ||
                            _defaultInputDialog.ProductResult.Colour_code == "" ||
                            _defaultInputDialog.ProductResult.Price == ""
                            )
                        {
                            IsLoading = false;
                            MessageBox.Show("Empty column detected, all columns can't be empty", "UPO$$");
                        }
                        else
                        {
                            dynamic param;

                            if (_defaultInputDialog.QuantityResult.Branch_name != null && _defaultInputDialog.QuantityResult.Quantity != null)
                            {
                                param = new
                                {
                                    productNo = _defaultInputDialog.ProductResult.Product_no,
                                    name = _defaultInputDialog.ProductResult.Name,
                                    category = _defaultInputDialog.ProductResult.Category,
                                    designCode = _defaultInputDialog.ProductResult.Design_code,
                                    colourCode = _defaultInputDialog.ProductResult.Colour_code,
                                    price = _defaultInputDialog.ProductResult.Price,
                                    is_active = SelectedProduct.Is_active,
                                    branchName = _defaultInputDialog.QuantityResult.Branch_name,
                                    quantity = _defaultInputDialog.QuantityResult.Quantity
                                };
                            } 
                            else
                            {
                                param = new
                                {
                                    productNo = _defaultInputDialog.ProductResult.Product_no,
                                    name = _defaultInputDialog.ProductResult.Name,
                                    category = _defaultInputDialog.ProductResult.Category,
                                    designCode = _defaultInputDialog.ProductResult.Design_code,
                                    colourCode = _defaultInputDialog.ProductResult.Colour_code,
                                    price = _defaultInputDialog.ProductResult.Price,
                                    is_active = SelectedProduct.Is_active
                                };
                            }

                            RootProductObject Response = await ObjProductService.PostAPI("updateProduct", param, _Path);

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
        //        if (SelectedProduct is null || SelectedProduct.Id == 0)
        //        {
        //            MessageBox.Show("Please select a branch from the list", "UPO$$");
        //        }
        //        else
        //        {
        //            ProductInputDialog _defaultInputDialog = new ProductInputDialog
        //                (
        //                    "Do you want to delete \"" + SelectedProduct.Product_no + "\" ?\n" +
        //                    "Note: Please make sure all stocks of the Product No. : \"" + SelectedProduct.Product_no + "\" have already been cleared",
        //                    mode: "delete", 
        //                    SelectedProduct
        //                );

        //            if (_defaultInputDialog.ShowDialog() == true)
        //            {
        //                var param = new { productNo = SelectedProduct.Product_no };

        //                RootProductObject Response = await ObjProductService.PostAPI("deleteProduct", param, _Path);

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


        #region RestockOperation
        //private AsyncRelayCommand restockCommand;
        //public AsyncRelayCommand RestockCommand
        //{
        //    get { return restockCommand; }
        //}
        //private async Task Restock()
        //{
        //    try
        //    {
        //        if (SelectedProduct is null || SelectedProduct.Id == 0)
        //        {
        //            IsLoading = false;
        //            MessageBox.Show("Please select a product from the list", "UPO$$");
        //        }
        //        else if (SelectedProduct.Is_active == "Inactive" || QuantityList == null)
        //        {
        //            IsLoading = false;
        //            MessageBox.Show("Only active product is allowed to be restocked", "UPO$$");
        //        }
        //        else
        //        {
        //            ProductRestockDialog _defaultInputDialog = new ProductRestockDialog("Please fill in restock price and quantity", mode: "restock", QuantityList);

        //            if (_defaultInputDialog.ShowDialog() == true)
        //            {
        //                if (_defaultInputDialog.ProductResult is null)
        //                {
        //                    IsLoading = false;
        //                    MessageBox.Show("Empty column detected, all columns can't be empty", "UPO$$");
        //                }
        //                else
        //                {
        //                    dynamic param;

        //                    param = new { productNo = SelectedProduct.Product_no, price = _defaultInputDialog.Price, restockList = _defaultInputDialog.ProductResult };

        //                    RootProductObject Response = await ObjProductService.PostAPI("restock", param, _Path);

        //                    MessageBox.Show(Response.Msg, "UPO$$");

        //                    if (Response.Status is "ok")
        //                    {
        //                        RefreshTextBox();
        //                        await Search();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message.ToString(), "UPO$$");
        //        await Search();
        //    }
        //}
        #endregion


        #region ScanRestockOperation
        private AsyncRelayCommand scanRestockCommand;
        public AsyncRelayCommand ScanRestockCommand
        {
            get { return scanRestockCommand; }
        }
        private async Task ScanRestock()
        {
            try
            {
                if (SelectedProduct is null || SelectedProduct.Id == 0 || SelectedQuantity is null || SelectedQuantity.Branch_name is null)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select product and quantity on both list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive")
                {
                    IsLoading = false;
                    MessageBox.Show("Product is Inactive, only active product is allowed to be restock", "UPO$$");
                }
                else
                {
                    if (SelectedQuantity != null)
                    {
                        if (SelectedQuantity.Branch_name != null && SelectedQuantity.Branch_name != Properties.Settings.Default.CurrentBranch && Properties.Settings.Default.CurrentUserRole != "Super Admin")
                        {
                            IsLoading = false;
                            MessageBox.Show("Only superadmin is allowed to update other branch's stock", "UPO$$");
                            return;
                        }
                    }

                    ProductScanRestockDialog _defaultInputDialog = new ProductScanRestockDialog("Please use scanner to restock", mode: "scanRestock", SelectedProduct, SelectedQuantity);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        dynamic param;

                        param = new
                        {
                            productNo = SelectedProduct.Product_no,
                            branchName = SelectedQuantity.Branch_name,
                            addQuantity = _defaultInputDialog.tbQuantity.Text
                        };
                        
                        RootProductObject Response = await ObjProductService.PostAPI("scanRestockProduct", param, _Path);

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
                await Search();
            }
        }
        #endregion


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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Active")
                {
                    IsLoading = false;
                    MessageBox.Show("Product is active", "UPO$$");
                }
                else
                {
                    ProductInputDialog _defaultInputDialog = new ProductInputDialog
                        (
                            "Do you want to activate \"" + SelectedProduct.Product_no + "\" ?\n",
                            mode: "activate",
                            SelectedProduct
                        );

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        var param = new { productNo = SelectedProduct.Product_no };

                        RootProductObject Response = await ObjProductService.PostAPI("activateProduct", param, _Path);

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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive")
                {
                    IsLoading = false;
                    MessageBox.Show("Product is Inactive", "UPO$$");
                }
                else
                {
                    ProductInputDialog _defaultInputDialog = new ProductInputDialog
                        (
                            "Do you want to deactivate \"" + SelectedProduct.Product_no + "\" ?\n" +
                            "Note: Please make sure all stocks of the Product No. : \"" + SelectedProduct.Product_no + "\" have already been cleared",
                            mode: "deactivate",
                            SelectedProduct
                        );

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        var param = new { productNo = SelectedProduct.Product_no };

                        RootProductObject Response = await ObjProductService.PostAPI("deactivateProduct", param, _Path);

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


        #region PrintBarcodeOperation
        private AsyncRelayCommand printBarcodeCommand;
        public AsyncRelayCommand PrintBarcodeCommand
        {
            get { return printBarcodeCommand; }
        }
        private async Task PrintBarcode()
        {
            try
            {
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    IsLoading = false;
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else
                {
                    if (SelectedProduct.Barcode is null || SelectedProduct.Barcode == "")
                    {
                        IsLoading = false;
                        MessageBox.Show("Barcode not found", "UPO$$");
                    }
                    else
                    {
                        ProductPrintBarcodeDialog _defaultPrintDialog = new ProductPrintBarcodeDialog(SelectedProduct);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                IsLoading = false;
            }
        }
        #endregion
    }
}
