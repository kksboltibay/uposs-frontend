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
using UPOSS.Controls.Dialog;
using UPOSS.Custom;
using UPOSS.LocalDatabase;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    public class CashierViewModel : ViewModelBase
    {
        APIService ObjCashierService;
        private readonly string _Path;
        SQLiteDatabase DB;

        public CashierViewModel()
        {
            ObjCashierService = new APIService();
            _Path = "cashier";


            rowEditEndingCommand = new AsyncRelayCommand(CheckRowEdit, this);
            removeSelectedItemCommand = new AsyncRelayCommand(RemoveItem, this);
            syncCommand = new AsyncRelayCommand(Sync, this);
            clearAllCommand = new AsyncRelayCommand(ClearAll, this);
            onHoldCommand = new AsyncRelayCommand(OnHold, this);
            recallCommand = new AsyncRelayCommand(Recall, this);
            paymentCommand = new AsyncRelayCommand(Payment, this);


            DB = new SQLiteDatabase(this);

            IsSynced = false;
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

        private bool isSynced;
        public bool IsSynced
        {
            get { return isSynced; }
            set { isSynced = value; OnPropertyChanged("IsSynced"); }
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
            try
            {
                string syncResult = "";
                // check if system is synced
                if (IsSynced == false)
                {
                    // auto sync
                    syncResult = await DB.SyncLocalDB();

                    if (syncResult == "error")
                    {
                        return;
                    }
                    else
                    {
                        IsSynced = true;
                    }
                }
                
                if (IsSynced == true)
                {
                    // check if sync status time out
                    if (await CheckPaymentDuration())
                    {
                        // time in ( <= 15 mins)
                        // Search from local DB
                        using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                        {
                            connection.Open();

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
                                        // item += 1
                                        var item = ProductList.FirstOrDefault(i => i.Product_no == rdr["product_no"].ToString());
                                        if (item != null)
                                        {
                                            item.Total_stock = Math.Round(Convert.ToDecimal( Math.Round(Convert.ToDecimal(item.Total_stock), 2, MidpointRounding.AwayFromZero) + 1 ), 2, MidpointRounding.AwayFromZero).ToString();
                                        }
                                    }
                                    else if (rdr.IsDBNull(rdr.GetOrdinal("remaining_stock")))
                                    {
                                        MessageBox.Show("Product :" + rdr["product_no"].ToString() + " is out of stock", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else if (Math.Round(Convert.ToDecimal(rdr["remaining_stock"].ToString()), 2, MidpointRounding.AwayFromZero) <= 0)
                                    {
                                        MessageBox.Show("Product :" + rdr["product_no"].ToString() + " is out of stock", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        ProductList.Add(new Product
                                        {
                                            Product_no = rdr["product_no"].ToString(),
                                            Name = rdr["name"].ToString(),
                                            Barcode = rdr["barcode"].ToString(),
                                            Price = Math.Round(Convert.ToDecimal(rdr["price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                            Total_stock = "1.00",
                                            Original_price = Math.Round(Convert.ToDecimal(rdr["price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                            Remaining_stock = Math.Round(Convert.ToDecimal(rdr["remaining_stock"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        });
                                    }

                                }
                                rdr.Close();
                            }
                            Refresh();
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "UPO$$");
            }
        }

        private void Refresh()
        {
            string totalSubtotal = "0.00";
            string totalDiscount = "0.00";
            string tax = "0.00";
            string netTotal = "0.00";

            //Calculate
            for (int i = 0; i < ProductList.Count; i++)
            {
                //Id
                ProductList[i].Id = i + 1;

                //Discount (ori_price - price)
                ProductList[i].Discount = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2, MidpointRounding.AwayFromZero) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2, MidpointRounding.AwayFromZero)
                    -
                    Math.Round(Convert.ToDecimal(ProductList[i].Price), 2, MidpointRounding.AwayFromZero) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                //Subtotal
                ProductList[i].Subtotal = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(ProductList[i].Price), 2, MidpointRounding.AwayFromZero) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                // Calculation Section
                //Total Original Subtotal
                totalSubtotal = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalSubtotal), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2, MidpointRounding.AwayFromZero) * Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                //Total Discount
                totalDiscount = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalDiscount), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(ProductList[i].Discount), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();
            }

            //Tax - GST/SST ((Total Subtotal - Total Discount) * Tax_Setting )
            tax = Math.Round(Convert.ToDecimal(
                    (Math.Round(Convert.ToDecimal(totalSubtotal), 2, MidpointRounding.AwayFromZero) - Math.Round(Convert.ToDecimal(totalDiscount), 2, MidpointRounding.AwayFromZero)) * Math.Round(Convert.ToDecimal(Properties.Settings.Default.Setting_GovChargesValue), 2, MidpointRounding.AwayFromZero)
            ), 2, MidpointRounding.AwayFromZero).ToString();

            //Net Total
            netTotal = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(totalSubtotal), 2, MidpointRounding.AwayFromZero) - Math.Round(Convert.ToDecimal(totalDiscount), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(tax), 2, MidpointRounding.AwayFromZero)
            ), 2, MidpointRounding.AwayFromZero).ToString();

            Cart = new Cashier
            {
                Subtotal = totalSubtotal,
                Discount = totalDiscount,
                Tax = tax,
                Total_amount = netTotal
            };

            // Refresh product list
            var temp = ProductList;
            ProductList = new ObservableCollection<Product>(temp);
        }

        private async Task<bool> CheckPaymentDuration()
        {
            // check if payment duration exited 15 mins
            bool isPass = true;
            List<string> recordList = new List<string>();

            // check with local db
            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();

                using var command = new SQLiteCommand(connection);

                command.CommandText = "SELECT last_update FROM update_record WHERE last_update <= Datetime('now', '-15 minutes', 'localtime')";
                using (SQLiteDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        recordList.Add(rdr[0].ToString());
                        //System.Diagnostics.Trace.WriteLine(rdr[0].ToString());
                    }
                    rdr.Close();
                }

                if (recordList.Count > 0)
                {
                    // if exited 15 mins, reset whole cashier status
                    MessageBox.Show("Payment Time Out, please try again within 15 minutes.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                    // save failure payment
                    await OnHold();

                    isPass = false;
                    IsSynced = false;
                    await ClearAll();
                }
                connection.Close();
            }
            return isPass;
        }
        #endregion



        #region ProductList row end edit detection
        private AsyncRelayCommand rowEditEndingCommand;
        public AsyncRelayCommand RowEditEndingCommand
        {
            get { return rowEditEndingCommand; }
        }
        private async Task<bool> CheckRowEdit()
        {
            try
            {
                bool isCorrect = true;
                for (int i = 0; i < ProductList.Count; i++)
                {
                    // update remaining_stock with local db
                    using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                    {
                        connection.Open();

                        using var command = new SQLiteCommand(connection);

                        string sql = "";
                        if (!string.IsNullOrEmpty(ProductList[i].Product_no))
                        {
                            sql = "SELECT remaining_stock FROM products WHERE product_no = '" + ProductList[i].Product_no + "' LIMIT 1";
                        }

                        command.CommandText = sql;
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                if (rdr.IsDBNull(rdr.GetOrdinal("remaining_stock")))
                                {
                                    ProductList[i].Remaining_stock = "0.00";
                                }
                                else
                                {
                                    ProductList[i].Remaining_stock = Math.Round(Convert.ToDecimal(rdr["remaining_stock"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00");
                                }

                            }
                            rdr.Close();
                        }
                        connection.Close();
                    }

                    // check for empty price or qty
                    if (string.IsNullOrEmpty(ProductList[i].Price))
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Price] can't be empty", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Price = ProductList[i].Original_price;

                        isCorrect = false;
                    }

                    // check for empty price or qty
                    else if (string.IsNullOrEmpty(ProductList[i].Total_stock))
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Qty] can't be empty", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Total_stock = "1.00";

                        isCorrect = false;
                    }

                    // check for quantity
                    else if (Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2) <= 0)
                    {
                        // check if qty <= 0
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Qty] can't be smaller or equal to 0", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Total_stock = "1.00";

                        isCorrect = false;
                    }
                    else if (Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2) > Math.Round(Convert.ToDecimal(ProductList[i].Remaining_stock), 2))
                    {
                        // check if qty > remaining_stock
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Qty] only has " + ProductList[i].Remaining_stock + " left.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Total_stock = ProductList[i].Remaining_stock;

                        isCorrect = false;
                    }

                    // check for price
                    else if (Math.Round(Convert.ToDecimal(ProductList[i].Price), 2) > Math.Round(Convert.ToDecimal(ProductList[i].Original_price), 2))
                    {
                        MessageBox.Show("Product: " + ProductList[i].Product_no + " [Price] can't be greater than its original price", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);

                        ProductList[i].Price = ProductList[i].Original_price;

                        isCorrect = false;
                    }

                    // round up price and qty
                    ProductList[i].Total_stock = Math.Round(Convert.ToDecimal(ProductList[i].Total_stock), 2, MidpointRounding.AwayFromZero).ToString("0.00");
                    ProductList[i].Price = Math.Round(Convert.ToDecimal(ProductList[i].Price), 2, MidpointRounding.AwayFromZero).ToString("0.00");
                }

                // refresh product list
                Refresh();

                if (!isCorrect) return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                await ClearAll();
                return false;
            }

            return true;
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


        #region Sync
        private AsyncRelayCommand syncCommand;
        public AsyncRelayCommand SyncCommand
        {
            get { return syncCommand; }
        }
        private async Task Sync()
        {
            try
            {
                IsLoading = true;

                if (ProductList.Count > 0)
                {
                    var msgResult = MessageBox.Show("Are you sure to sync? \n\n*Note: Cart will be cleared if update detected!*", "UPO$$", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (msgResult == MessageBoxResult.Cancel)
                    {
                        IsLoading = false;
                        return;
                    }
                }

                string syncResult = await DB.SyncLocalDB();

                if (syncResult == "updateRequired")
                {
                    // update required

                    // compare updated product list with cart product list
                    if (ProductList.Count > 0 && DB.ProductList.Count > 0)
                    {
                        MessageBox.Show("Products have just been updated, please try again within 15 minutes.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Warning);
                        await ClearAll();
                    }
                    else
                    {
                        MessageBox.Show("Products synced successfully.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    IsSynced = true;
                }
                else if (syncResult == "upToDate")
                {
                    MessageBox.Show("Products synced successfully.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsSynced = true;
                }
                else
                {
                    // db sync exception error
                }
                IsLoading = false;
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
            // on hold limit <= 3
            try
            {
                IsLoading = true;

                if (ProductList.Count > 0)
                {
                    if (await CheckRowEdit())
                    {
                        List<string> recordList = new List<string>();
                        var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                        {
                            connection.Open();

                            using var command = new SQLiteCommand(connection);

                            command.CommandText = "SELECT created_at FROM temp_products GROUP BY created_at ORDER BY created_at DESC";
                            using (SQLiteDataReader rdr = command.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    recordList.Add(rdr[0].ToString());
                                    System.Diagnostics.Trace.WriteLine(rdr[0].ToString());
                                }
                                rdr.Close();
                            }

                            if (recordList.Any())
                            {
                                // check if record <= 3
                                if (recordList.Count >= 3)
                                {
                                    // remove oldest record
                                    string oldestRecordDate = recordList[2];

                                    command.CommandText = "DELETE FROM temp_products WHERE created_at = '" + oldestRecordDate + "'";
                                    command.ExecuteNonQuery();
                                }
                            }

                            // insert into temp_products table
                            foreach (var product in ProductList)
                            {
                                command.CommandText = "INSERT INTO temp_products(product_no, name, barcode, price, qty, original_price, created_at)" +
                                        " VALUES(@product_no, @name, @barcode, @price, @qty, @original_price, @created_at)";
                                command.Parameters.AddWithValue("@product_no", product.Product_no);
                                command.Parameters.AddWithValue("@name", product.Name);
                                command.Parameters.AddWithValue("@barcode", product.Barcode);
                                command.Parameters.AddWithValue("@price", product.Price);
                                command.Parameters.AddWithValue("@qty", product.Total_stock);
                                command.Parameters.AddWithValue("@original_price", product.Original_price);
                                command.Parameters.AddWithValue("@created_at", currentDateTime);
                                command.Prepare();
                                command.ExecuteNonQuery();
                            }

                            connection.Close();
                        }

                        await ClearAll();
                    }
                }
                IsLoading = false;
            }
            catch (Exception e)
            {
                IsLoading = false;
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion


        #region Recall
        private AsyncRelayCommand recallCommand;
        public AsyncRelayCommand RecallCommand
        {
            get { return recallCommand; }
        }
        private async Task Recall()
        {
            try
            {
                IsLoading = true;

                if (ProductList.Count == 0)
                {
                    string syncResult = "";
                    // check if system is synced
                    if (IsSynced == false)
                    {
                        // auto sync
                        syncResult = await DB.SyncLocalDB();

                        if (syncResult == "error")
                        {
                            return;
                        }
                        else
                        {
                            IsSynced = true;
                        }
                    }

                    if (IsSynced == true)
                    {
                        // check if sync status time out
                        if (await CheckPaymentDuration())
                        {
                            // time in ( <= 15 mins)
                            CashierRecallDialog _productRecallDialog = new CashierRecallDialog();

                            if (_productRecallDialog.ShowDialog() == true)
                            {
                                if (_productRecallDialog.dgProduct.SelectedItem != null)
                                {
                                    Product product = (Product)_productRecallDialog.dgProduct.SelectedItem;
                                    ProductList = new ObservableCollection<Product>(product.List);

                                    Refresh();
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please clear all first", "UPO$$");
                }
                IsLoading = false;
            }
            catch (Exception e)
            {
                IsLoading = false;
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
                // check if product list is not empty
                if (ProductList.Count > 0)
                {
                    string syncResult = "";
                    // check if system is synced
                    if (IsSynced == false)
                    {
                        // auto sync
                        syncResult = await DB.SyncLocalDB();

                        if (syncResult == "error")
                        {
                            return;
                        }
                        else
                        {
                            IsSynced = true;
                        }
                    }

                    if (IsSynced == true)
                    {
                        if (await CheckRowEdit())
                        {
                            if (await CheckPaymentDuration())
                            {
                                // call payment dialog
                                CashierPaymentDialog _paymentDialog = new CashierPaymentDialog(Cart.Total_amount);

                                if (_paymentDialog.ShowDialog() == true)
                                {
                                    if (await CheckPaymentDuration())
                                    {
                                        // call api
                                        dynamic param = new
                                        {
                                            cartList = ProductList,
                                            totalItem = ProductList.Count,
                                            totalSubtotal = Cart.Subtotal,
                                            totalDiscount = Cart.Discount,
                                            totalTax = Cart.Tax,
                                            totalAmount = Cart.Total_amount,
                                            paymentMethod = _paymentDialog.Payment.Payment_method,
                                            cashPay = _paymentDialog.Payment.Cash_pay,
                                            cardNo = _paymentDialog.Payment.Card_no,
                                            cardPay = _paymentDialog.Payment.Card_pay,
                                            cardType = _paymentDialog.Payment.Card_type,
                                            bankName = _paymentDialog.Payment.Bank_name,
                                            branchName = Properties.Settings.Default.CurrentBranch,
                                            cashierUsername = Properties.Settings.Default.CurrentUsername,
                                            change = _paymentDialog.Payment.Change
                                        };

                                        RootCashierObject Response = await ObjCashierService.PostAPI("payment", param, _Path);

                                        MessageBox.Show(Response.Msg, "UPO$$");

                                        if (Response.Status != "ok")
                                        {
                                            await OnHold();
                                            MessageBox.Show("Cart has been [On Hold].", "UPO$$");
                                        }
                                        else
                                        {
                                            //changes
                                            MessageBox.Show("Changes: $" + _paymentDialog.Payment.Change, "UPO$$");

                                            //print receipt (x2 everytime)
                                            for (var i = 0; i < 2; i++)
                                            {
                                                CashierPrintReceiptDialog _cashierPrintReceiptDialog = new CashierPrintReceiptDialog(param, Response.Data, Properties.Settings.Default.CurrentUsername);
                                            }
                                        }

                                        await ClearAll();
                                        IsSynced = false;
                                        IsLoading = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
                await OnHold();
            }
        }
        #endregion
    }
}
