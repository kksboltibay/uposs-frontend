using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using UPOSS.Commands;
using UPOSS.Controls;
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

            searchCommand = new RelayCommand(Search);
            addCommand = new RelayCommand(Add);
            updateCommand = new RelayCommand(Update);
            //deleteCommand = new RelayCommand(Delete);
            restockCommand = new RelayCommand(Restock);
            activateCommand = new RelayCommand(Activate);
            deactivateCommand = new RelayCommand(Deactivate);
            previousPageCommand = new RelayCommand(PrevPage);
            nextPageCommand = new RelayCommand(NextPage);

            SelectedBranch = "";
            SelectedStatus = "Active";
            SelectedProduct = new Product();
            SelectedQuantity = new ProductQuantity();
            InputProduct = new Product();
            Pagination = new Pagination { CurrentPage = 1, CurrentRecord = "0 - 0", TotalPage = 1, TotalRecord = 0 };
        }

        #region Debug
        //System.Diagnostics.Trace.WriteLine(x.Name);
        //System.Diagnostics.Trace.WriteLine(x.Stock);
        #endregion

        #region Define
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

        private Product inputProduct;
        public Product InputProduct
        {
            get { return inputProduct; }
            set { inputProduct = value; OnPropertyChanged("InputProduct"); }
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
                    MessageBox.Show(Response.Msg, "UPO$$");
                    ProductList = null;
                    QuantityList = null;
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
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get { return addCommand; }
        }

        private async void Add()
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
                            Search();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive")
                {
                    MessageBox.Show("Product is Inactive, only active product is allowed to be updated", "UPO$$");
                }
                else
                {
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
                                Search();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
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
        private RelayCommand restockCommand;
        public RelayCommand RestockCommand
        {
            get { return restockCommand; }
        }
        private async void Restock()
        {
            try
            {
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive" || QuantityList == null)
                {
                    MessageBox.Show("Only active product is allowed to be restocked", "UPO$$");
                }
                else
                {
                    ProductRestockDialog _defaultInputDialog = new ProductRestockDialog("Please fill in restock price and quantity", mode: "restock", QuantityList);

                    if (_defaultInputDialog.ShowDialog() == true)
                    {
                        if (_defaultInputDialog.ProductResult is null)
                        {
                            MessageBox.Show("Empty column detected, all columns can't be empty", "UPO$$");
                        }
                        else
                        {
                            dynamic param;

                            param = new { productNo = SelectedProduct.Product_no, price = _defaultInputDialog.Price, restockList = _defaultInputDialog.ProductResult };

                            RootProductObject Response = await ObjProductService.PostAPI("restock", param, _Path);

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
            }
        }
        #endregion


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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Active")
                {
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
                if (SelectedProduct is null || SelectedProduct.Id == 0)
                {
                    MessageBox.Show("Please select a product from the list", "UPO$$");
                }
                else if (SelectedProduct.Is_active == "Inactive")
                {
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
