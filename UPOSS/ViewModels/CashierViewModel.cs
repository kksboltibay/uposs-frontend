using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SQLite;
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
    public class CashierViewModel : ViewModelBase
    {
        APIService ObjProductService;
        private readonly string _Path;

        public CashierViewModel()
        {
            ObjProductService = new APIService();
            _Path = "product";

            paymentCommand = new AsyncRelayCommand(Payment, this);
            //updateCommand = new AsyncRelayCommand(Update, this);

            rowEditEndingCommand = new AsyncRelayCommand(RowEdit, this);
            removeSelectedItemCommand = new AsyncRelayCommand(RemoveItem, this);
            clearAllCommand = new AsyncRelayCommand(ClearAll, this);
            onHoldCommand = new AsyncRelayCommand(OnHold, this);


            //cellValueChangedCommand = new AsyncRelayCommand(RowTextChangedEvent, this);
            //ProductList.CollectionChanged += ProductList_CollectionChanged;
        }


        #region Define
        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }

        //Main content section
        private Product selectedProduct = new Product();
        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set { selectedProduct = value; OnPropertyChanged("SelectedProduct"); }
        }

        private Cashier cart = new Cashier();
        public Cashier Cart
        {
            get { return cart; }
            set { cart = value; OnPropertyChanged("Cart"); }
        }

        private ObservableCollection<Product> productList = new ObservableCollection<Product>();
        public ObservableCollection<Product> ProductList
        {
            get { return productList; }
            set { productList = value; OnPropertyChanged("ProductList"); }
        }


        //Search section
        private string selectedBarcode;
        public string SelectedBarcode
        {
            get { return selectedBarcode; }
            set
            {
                selectedBarcode = value;
                OnPropertyChanged("SelectedBarcode");

                if (!string.IsNullOrEmpty(SelectedBarcode))
                {
                    AddItemIntoProductList(SelectedBarcode);
                }
            }
        }

        private string selectedProductNo;
        public string SelectedProductNo
        {
            get { return selectedProductNo; }
            set
            {
                selectedProductNo = value;
                OnPropertyChanged("SelectedProductNo");

                if (!string.IsNullOrEmpty(SelectedProductNo))
                {
                    AddItemIntoProductList("", SelectedProductNo);
                }
            }
        }

        private string selectedProductName;
        public string SelectedProductName
        {
            get { return selectedProductName; }
            set
            {
                selectedProductName = value;
                OnPropertyChanged("SelectedProductName");

                if (!string.IsNullOrEmpty(SelectedProductName))
                {
                    AddItemIntoProductList("", "", SelectedProductName);
                }
            }
        }

        private Product inputProduct = new Product();
        public Product InputProduct
        {
            get { return inputProduct; }
            set { inputProduct = value; OnPropertyChanged("InputProduct"); }
        }


        //event
        //private void ProductList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    System.Diagnostics.Trace.WriteLine("change event");

        //    for (int i = 0; i < ProductList.Count; i++)
        //    {
        //        ProductList[i].Id = i + 1;
        //        ProductList[i].Subtotal = (Math.Round(Convert.ToDecimal(
        //            Math.Round(Convert.ToDecimal(ProductList[i].Price), 2) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2)
        //        ), 2)).ToString();
        //    }
        //}
        #endregion


        #region Custom
        private async Task AddItemIntoProductList(string barcode = "", string productNo = "", string productName = "")
        {
            // Search from local DB
            using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    string sql = "";
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        sql = "SELECT * FROM products WHERE barcode = '" + barcode + "' AND is_active = '1' LIMIT 1";
                    }
                    else if (!string.IsNullOrEmpty(productNo))
                    {
                        sql = "SELECT * FROM products WHERE product_no = '" + productNo + "' AND is_active = '1' LIMIT 1";
                    }
                    else if (!string.IsNullOrEmpty(productName))
                    {
                        sql = "SELECT * FROM products WHERE name = '" + productName + "' AND is_active = '1' LIMIT 1";
                    }

                    // get product
                    command.CommandText = sql;
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (ProductList == null)
                            {
                                ProductList = new ObservableCollection<Product>();
                            }

                            if (ProductList.Any(p => p.Product_no == rdr["product_no"].ToString()))
                            {
                                MessageBox.Show("Product :" + rdr["product_no"].ToString() + " already exists", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                ProductList.Add(new Product
                                {
                                    Product_no = rdr["product_no"].ToString(),
                                    Name = rdr["name"].ToString(),
                                    Barcode = rdr["barcode"].ToString(),
                                    Price = rdr["price"].ToString(),
                                    Total_stock = "1.0",
                                    Original_price = rdr["price"].ToString(),
                                });
                            }
                            
                        }
                        rdr.Close();
                    }

                    Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }

        private void Refresh()
        {
            string totalSubtotal = "0";
            string totalDiscount = "0";
            string gst = "0";
            string netTotal = "0";

            //Calculate
            for (int i = 0; i < ProductList.Count; i++)
            {
                //Id
                ProductList[i].Id = i + 1;

                //Discount (ori_price - price)
                ProductList[i].Discount = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2)
                    -
                    Math.Round(Convert.ToDecimal(ProductList[i].Price), 2) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2)
                ), 2).ToString();

                //Subtotal
                ProductList[i].Subtotal = (Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(ProductList[i].Price), 2) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2)
                ), 2)).ToString();

                // Calculation Section
                //Total Original Subtotal
                totalSubtotal = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalSubtotal), 2) + Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2)
                ), 2).ToString();

                //Total Discount
                totalDiscount = (Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalDiscount), 2) + Math.Round(Convert.ToDecimal(ProductList[i].Discount), 2)
                ), 2)).ToString();
            }

            //GST((Total Subtotal - Total Discount) * 0.06 )
            gst = (Math.Round(Convert.ToDecimal(
                    (Math.Round(Convert.ToDecimal(totalSubtotal), 2) - Math.Round(Convert.ToDecimal(totalDiscount), 2)) * Math.Round(Convert.ToDecimal(Properties.Settings.Default.Setting_GST), 2)
            ), 2)).ToString();

            //Net Total
            netTotal = (Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalSubtotal), 2) - Math.Round(Convert.ToDecimal(totalDiscount), 2) + Math.Round(Convert.ToDecimal(gst), 2)
            ), 2)).ToString();

            Cart = new Cashier
            {
                Subtotal = totalSubtotal,
                Discount = totalDiscount,
                GST = gst,
                Total_amount = netTotal
            };

            // Refresh product list
            var temp = ProductList;
            ProductList = new ObservableCollection<Product>(temp);
        }
        #endregion



        #region ProductList row end edit detection
        private AsyncRelayCommand rowEditEndingCommand;
        public AsyncRelayCommand RowEditEndingCommand
        {
            get { return rowEditEndingCommand; }
        }
        private async Task RowEdit()
        {
            try
            {
                for (int i = 0; i < ProductList.Count; i++)
                {
                    // check for empty price or qty
                    if (string.IsNullOrEmpty(ProductList[i].Price) || string.IsNullOrEmpty(ProductList[i].Total_stock))
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Price] or [Qty] can't be empty", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Price = ProductList[i].Original_price;
                        ProductList[i].Total_stock = "1.0";
                    }

                    // check for 0 qty
                    if (Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2) <= 0)
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Qty] can't be smaller or equal to 0", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Total_stock = "1.0";
                    }

                    if (Math.Round(Convert.ToDecimal(ProductList[i].Price), 2) > Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2))
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Price] can't be greater than its original price", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Price = ProductList[i].Original_price;
                    }
                }

                // refresh product list
                Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                await ClearAll();
            }
        }
        #endregion


        #region Remove selected item
        private AsyncRelayCommand removeSelectedItemCommand;
        public AsyncRelayCommand RemoveSelectedItemCommand
        {
            get { return removeSelectedItemCommand; }
        }
        private async Task RemoveItem()
        {
            System.Diagnostics.Trace.WriteLine("Remove Item");
            try
            {
                if (SelectedProduct != null)
                {
                    ProductList.Remove(SelectedProduct);
                }
                Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion


        #region Clear All
        private AsyncRelayCommand clearAllCommand;
        public AsyncRelayCommand ClearAllCommand
        {
            get { return clearAllCommand; }
        }
        private async Task ClearAll()
        {
            try
            {
                InputProduct = new Product
                {
                    Barcode = "",
                    Product_no = "",
                    Name = "",
                };

                ProductList = new ObservableCollection<Product>();
                
                Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion


        #region On Hold
        private AsyncRelayCommand onHoldCommand;
        public AsyncRelayCommand OnHoldCommand
        {
            get { return onHoldCommand; }
        }
        private async Task OnHold()
        {
            try
            {
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion


        #region PaymentOperation
        private AsyncRelayCommand paymentCommand;
        public AsyncRelayCommand PaymentCommand
        {
            get { return paymentCommand; }
        }
        private async Task Payment()
        {
            try
            {
                //ProductInputDialog _defaultInputDialog = new ProductInputDialog("Please fill in the details of new product", mode: "add");

                //if (_defaultInputDialog.ShowDialog() == true)
                //{
                //    if (
                //        _defaultInputDialog.ProductResult is null ||
                //        _defaultInputDialog.ProductResult.Name == "" ||
                //        _defaultInputDialog.ProductResult.Category == "" ||
                //        _defaultInputDialog.ProductResult.Design_code == "" ||
                //        _defaultInputDialog.ProductResult.Colour_code == "" ||
                //        _defaultInputDialog.ProductResult.Price == ""
                //        )
                //    {
                //        IsLoading = false;
                //        MessageBox.Show("Empty column detected, all columns can't be empty", "UPO$$");
                //    }
                //    else
                //    {
                //        dynamic param = new
                //        {
                //            name = _defaultInputDialog.ProductResult.Name,
                //            category = _defaultInputDialog.ProductResult.Category,
                //            designCode = _defaultInputDialog.ProductResult.Design_code,
                //            colourCode = _defaultInputDialog.ProductResult.Colour_code,
                //            price = _defaultInputDialog.ProductResult.Price
                //        };

                //        RootProductObject Response = await ObjProductService.PostAPI("addProduct", param, _Path);

                //        MessageBox.Show(Response.Msg, "UPO$$");

                //        if (Response.Status is "ok")
                //        {
                //            //RefreshTextBox();
                //            //await Search();
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                //await Search();
            }
        }
        #endregion















        #region ProductList row edit detection
        //private AsyncRelayCommand cellValueChangedCommand;
        //public AsyncRelayCommand CellValueChangedCommand
        //{
        //    get { return cellValueChangedCommand; }
        //}
        //private async Task RowTextChangedEvent()
        //{
        //    System.Diagnostics.Trace.WriteLine("Cell Value Changed");
        //    try
        //    {

        //    }
        //    catch (Exception e)
        //    {
        //        //MessageBox.Show(e.Message.ToString(), "UPO$$");
        //    }
        //}
        #endregion
    }
}
