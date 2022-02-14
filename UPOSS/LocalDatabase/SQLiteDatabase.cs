using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.LocalDatabase
{
    public class SQLiteDatabase
    {
        APIService ObjProductService;
        private string _Path;
        private dynamic _CurrentViewModel;

        public SQLiteDatabase(dynamic currentViewModel)
        {
            ObjProductService = new APIService();
            _Path = "product";
            _CurrentViewModel = currentViewModel;
        }


        #region Define
        public ObservableCollection<Product> ProductList;
        #endregion


        public async Task SetupLocalDB()
        {
            System.Diagnostics.Trace.WriteLine("Setup Local Database");
            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    // drop all tables
                    command.CommandText = "DROP TABLE IF EXISTS products";
                    command.ExecuteNonQuery();
                    command.CommandText = "DROP TABLE IF EXISTS users";
                    command.ExecuteNonQuery();
                    command.CommandText = "DROP TABLE IF EXISTS update_record";
                    command.ExecuteNonQuery();
                    command.CommandText = "DROP TABLE IF EXISTS temp_products";
                    command.ExecuteNonQuery();
                    command.CommandText = "DROP TABLE IF EXISTS banks";
                    command.ExecuteNonQuery();

                    // create tables
                    command.CommandText = "CREATE TABLE IF NOT EXISTS update_record(type TEXT, last_update TEXT)";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS temp_products(" +
                    "product_no TEXT, " +
                    "name TEXT, " +
                    "barcode TEXT, " +
                    "price REAL, " +
                    "qty REAL, " +
                    "original_price REAL, " +
                    "created_at TEXT" +
                    ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS products(" +
                    "id INTEGER, " +
                    "product_no TEXT UNIQUE, " +
                    "name TEXT, " +
                    "category TEXT, " +
                    "design_code TEXT, " +
                    "colour_code TEXT, " +
                    "price REAL, " +
                    "barcode TEXT, " +
                    "is_active INTEGER, " +
                    "remaining_stock REAL" +
                    ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS users(" +
                    "username TEXT, " +
                    "branch TEXT, " +
                    "user_role TEXT," +
                    "version TEXT" +
                    ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS banks(name TEXT)";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS settings(" +
                    "id INTEGER DEFAULT 1, " +
                    "address TEXT DEFAULT '-', " +
                    "phone_no TEXT DEFAULT '-', " +
                    "gov_charge_name TEXT DEFAULT '-', " +
                    "gov_charge_value TEXT DEFAULT '-', " +
                    "gov_charge_no TEXT DEFAULT '-', " +
                    "scanner_is_used INTEGER DEFAULT 1, " +
                    "is_first_login INTEGER DEFAULT 0, " +
                    "is_update INTEGER DEFAULT 0" +
                    ")";
                    command.ExecuteNonQuery();

                    // insert user
                    command.CommandText = "INSERT INTO users(username, branch, user_role, version)" +
                                            " VALUES(@username, @branch, @user_role, @version)";
                    command.Parameters.AddWithValue("@username", Properties.Settings.Default.CurrentUsername);
                    command.Parameters.AddWithValue("@branch", Properties.Settings.Default.CurrentBranch);
                    command.Parameters.AddWithValue("@user_role", Properties.Settings.Default.CurrentUserRole);
                    command.Parameters.AddWithValue("@version", Properties.Settings.Default.CurrentApplicationVersion);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    // insert bank
                    string[] bankAry = { "Affin Bank", "Alliance Bank", "AmBank", "Agrobank", "Bank Islam Malaysia", "Bank Rakyat", "Bank Simpanan Nasional (BSN)", "CIMB", "Hong Leong Bank", "Maybank", "Public Bank", "RHB Bank", "Standard Chartered Bank Malaysia", "Other" };

                    for (var i = 0; i < bankAry.Length; i++)
                    {
                        command.CommandText = "INSERT INTO banks(name) VALUES(@name)";
                        command.Parameters.AddWithValue("@name", bankAry[i]);
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }

                    // insert product
                    await ResetProductsTable();

                    Properties.Settings.Default.Setting_System_IsFirstLogin = false;
                    Properties.Settings.Default.Save();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }

        
        public async Task<string> SyncLocalDB()
        {
            System.Diagnostics.Trace.WriteLine("Sync DB");

            string productLastUpdate = null;
            string stockLastUpdate = null;
            var currentDateTime = DateTime.Now;
            TimeSpan timeSpan;
            RootProductObject Response;
            dynamic param;
            bool isProductUpToDate = false;
            bool isStockUpToDate = false;

            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();

                try
                {
                    using var command = new SQLiteCommand(connection);

                    // get product last update record
                    command.CommandText = "SELECT last_update FROM update_record WHERE type = 'product'";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            productLastUpdate = rdr[0].ToString();
                        }
                        rdr.Close();
                    }

                    // get stock last update record
                    command.CommandText = "SELECT last_update FROM update_record WHERE type = 'remaining_stock'";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            stockLastUpdate = rdr[0].ToString();
                        }
                        rdr.Close();
                    }


                    // update
                    if (productLastUpdate != null && stockLastUpdate != null)
                    {
                        //last update record exists
                        System.Diagnostics.Trace.WriteLine("product record: " + productLastUpdate);
                        System.Diagnostics.Trace.WriteLine("stock record: " + stockLastUpdate);

                        // update product
                        // calculate time span
                        timeSpan = currentDateTime - DateTime.Parse(productLastUpdate);
                        System.Diagnostics.Trace.WriteLine("Product Updated " + timeSpan.Days.ToString() + " days ago");

                        if (timeSpan.Days > 6)
                        {
                            // update whole local product db including stock
                            System.Diagnostics.Trace.WriteLine("Update whole local product db including stock");
                            await ResetProductsTable();
                        }
                        else
                        {
                            // sync product
                            // last update record <= 6 days
                            System.Diagnostics.Trace.WriteLine("Product last update <=6");

                            // request products data that need to be updated from remote server
                            param = new { lastUpdate = productLastUpdate };
                            Response = await ObjProductService.PostAPI("getProductActivity", param, _Path);

                            if (Response.Status != "ok")
                            {
                                //API Error 2
                                MessageBox.Show("Failed to sync local database, please contact IT support [AE2]", "UPO$$");
                            }
                            else if (Response.Msg == "Product activity not found")
                            {
                                System.Diagnostics.Trace.WriteLine("Local product is up-to-date");

                                // update product last_update record
                                command.CommandText = "UPDATE update_record SET last_update = @currentDateTime WHERE type = 'product'";
                                command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                command.Prepare();
                                command.ExecuteNonQuery();

                                isProductUpToDate = true;
                            }
                            else
                            {
                                ProductList = new ObservableCollection<Product>(Response.Data);

                                // update products table
                                if (ProductList != null)
                                {
                                    foreach (var product in ProductList)
                                    {
                                        command.CommandText = "INSERT OR REPLACE INTO products(id, product_no, name, category, design_code, colour_code, price, barcode, is_active, remaining_stock)" +
                                            " VALUES(@id, @product_no, @name, @category, @design_code, @colour_code, @price, @barcode, @is_active, (SELECT remaining_stock FROM products WHERE id = " + product.Id + "))";
                                        command.Parameters.AddWithValue("@id", product.Id);
                                        command.Parameters.AddWithValue("@product_no", product.Product_no);
                                        command.Parameters.AddWithValue("@name", product.Name);
                                        command.Parameters.AddWithValue("@category", product.Category);
                                        command.Parameters.AddWithValue("@design_code", product.Design_code);
                                        command.Parameters.AddWithValue("@colour_code", product.Colour_code);
                                        command.Parameters.AddWithValue("@price", product.Price);
                                        command.Parameters.AddWithValue("@barcode", product.Barcode);
                                        command.Parameters.AddWithValue("@is_active", product.Is_active);
                                        command.Prepare();
                                        command.ExecuteNonQuery();
                                    }
                                }

                                // update product last_update record
                                command.CommandText = "UPDATE update_record SET last_update = @currentDateTime WHERE type = 'product'";
                                command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                command.Prepare();
                                command.ExecuteNonQuery();
                                System.Diagnostics.Trace.WriteLine("product last update record updated");
                            }

                            // update stock
                            // calculate time span
                            timeSpan = currentDateTime - DateTime.Parse(stockLastUpdate);
                            System.Diagnostics.Trace.WriteLine("Stock Updated " + timeSpan.Days.ToString() + " days ago");

                            if (timeSpan.Days > 3)
                            {
                                // update whole local stock db
                                System.Diagnostics.Trace.WriteLine("update whole local stock db");

                                // request current branch stocks data from remote server
                                param = new { currentBranch = Properties.Settings.Default.CurrentBranch };

                                Response = await ObjProductService.PostAPI("getBranchStock", param, _Path);

                                if (Response.Status != "ok")
                                {
                                    //API Error 1
                                    MessageBox.Show("Failed to renew product stock, please contact IT support [AE1]", "UPO$$");
                                }
                                else
                                {
                                    ProductList = new ObservableCollection<Product>(Response.Data);

                                    if (ProductList != null)
                                    {
                                        // insert into products table
                                        foreach (var product in ProductList)
                                        {
                                            command.CommandText = "UPDATE products SET remaining_stock = " + product.Remaining_stock + " WHERE id = " + product.Id;
                                            command.ExecuteNonQuery();
                                        }
                                    }

                                    // update stock last_update record
                                    command.CommandText = "UPDATE update_record SET last_update = @currentDateTime WHERE type = 'remaining_stock'";
                                    command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                    command.Prepare();
                                    command.ExecuteNonQuery();
                                    System.Diagnostics.Trace.WriteLine("stock last update record updated");
                                }
                            }
                            else
                            {
                                // sync remaining_stock
                                // last update record <= 3 days
                                System.Diagnostics.Trace.WriteLine("Stock last update <=3");

                                // request products data that need to be updated from remote server
                                param = new { lastUpdate = productLastUpdate, currentBranch = Properties.Settings.Default.CurrentBranch };
                                Response = await ObjProductService.PostAPI("getStockActivity", param, _Path);

                                if (Response.Status != "ok")
                                {
                                    //API Error 2
                                    MessageBox.Show("Failed to sync local database, please contact IT support [AE2]", "UPO$$");
                                }
                                else if (Response.Msg == "Stock activity not found")
                                {
                                    System.Diagnostics.Trace.WriteLine("Local stock is up-to-date");

                                    // update stock last_update record
                                    command.CommandText = "UPDATE update_record SET last_update = @currentDateTime WHERE type = 'remaining_stock'";
                                    command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                    command.Prepare();
                                    command.ExecuteNonQuery();

                                    isStockUpToDate = true;
                                }
                                else
                                {
                                    ProductList = new ObservableCollection<Product>(Response.Data);

                                    // update remaining_stock
                                    if (ProductList != null)
                                    {
                                        // insert into products table
                                        foreach (var product in ProductList)
                                        {
                                            command.CommandText = "UPDATE products SET remaining_stock = " + product.Remaining_stock + " WHERE id = " + product.Id;
                                            command.ExecuteNonQuery();
                                        }
                                    }

                                    // update stock last_update record
                                    command.CommandText = "UPDATE update_record SET last_update = @currentDateTime WHERE type = 'remaining_stock'";
                                    command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                    command.Prepare();
                                    command.ExecuteNonQuery();
                                    System.Diagnostics.Trace.WriteLine("stock last update record updated");
                                }
                            }
                        }
                    }
                    else
                    {
                        // productLastUpdate == null && stockLastUpdate == null
                        // sync whole local product and stock db with remote server
                        System.Diagnostics.Trace.WriteLine("No record found");

                        await ResetProductsTable();
                    }

                    // sync current user info
                    command.CommandText = "UPDATE users SET username = @username, branch = @branch, user_role = @user_role, version = @version";
                    command.Parameters.AddWithValue("@username", Properties.Settings.Default.CurrentUsername);
                    command.Parameters.AddWithValue("@branch", Properties.Settings.Default.CurrentBranch);
                    command.Parameters.AddWithValue("@user_role", Properties.Settings.Default.CurrentUserRole);
                    command.Parameters.AddWithValue("@version", Properties.Settings.Default.CurrentApplicationVersion);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString() + " (Please contact IT support)", "UPO$$");
                    return "error";
                }
                connection.Close();

                if (isProductUpToDate && isStockUpToDate)
                {
                    return "upToDate";
                }
                else
                {
                    return "updateRequired";
                }
            }
        }


        public async Task CheckBranch()
        {
            System.Diagnostics.Trace.WriteLine("Check User Branch");

            string currentBranch = "";
            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "SELECT branch FROM users";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            currentBranch = rdr[0].ToString();
                        }
                        rdr.Close();
                    }

                    if (currentBranch != Properties.Settings.Default.CurrentBranch)
                    {
                        // update remaining_stock to match current branch
                        // request current branch stocks data from remote server
                        dynamic param = new { currentBranch = Properties.Settings.Default.CurrentBranch };

                        RootProductObject Response = await ObjProductService.PostAPI("getBranchStock", param, _Path);

                        if (Response.Status != "ok")
                        {
                            //API Error 1
                            MessageBox.Show("Failed to renew product stock, please contact IT support [AE1]", "UPO$$");
                        }
                        else
                        {
                            ProductList = new ObservableCollection<Product>(Response.Data);

                            if (ProductList.Count > 0)
                            {
                                // insert into products table
                                foreach (var product in ProductList)
                                {
                                    command.CommandText = "UPDATE products SET remaining_stock = " + product.Remaining_stock + " WHERE id = " + product.Id;
                                    command.ExecuteNonQuery();
                                }
                            }

                            // change current local db branch
                            command.CommandText = "UPDATE users SET branch = @branch";
                            command.Parameters.AddWithValue("@branch", Properties.Settings.Default.CurrentBranch);
                            command.Prepare();
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }


        private async Task ResetProductsTable()
        {
            System.Diagnostics.Trace.WriteLine("Reset Products Table");

            var currentDateTime = DateTime.Now;
            string lastUpdate = null;

            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    // drop products table
                    command.CommandText = "DROP TABLE IF EXISTS products";
                    command.ExecuteNonQuery();

                    // create products table
                    command.CommandText = "CREATE TABLE IF NOT EXISTS products(" +
                    "id INTEGER, " +
                    "product_no TEXT UNIQUE, " +
                    "name TEXT, " +
                    "category TEXT, " +
                    "design_code TEXT, " +
                    "colour_code TEXT, " +
                    "price REAL, " +
                    "barcode TEXT, " +
                    "is_active INTEGER, " +
                    "remaining_stock REAL" +
                    ")";
                    command.ExecuteNonQuery();

                    // request all products and stocks data from remote server
                    dynamic param = new { currentBranch = Properties.Settings.Default.CurrentBranch };

                    RootProductObject Response = await ObjProductService.PostAPI("getProductAndStockList", param, _Path);

                    if (Response.Status != "ok")
                    {
                        //API Error 1
                        MessageBox.Show("Failed to reset products table, please contact IT support [AE1]", "UPO$$");
                    }
                    else
                    {
                        ProductList = new ObservableCollection<Product>(Response.Data);

                        if (ProductList.Count > 0)
                        {
                            // insert into products table
                            foreach (var product in ProductList)
                            {
                                command.CommandText = "INSERT INTO products(id, product_no, name, category, design_code, colour_code, price, barcode, is_active, remaining_stock)" +
                                    " VALUES(@id, @product_no, @name, @category, @design_code, @colour_code, @price, @barcode, @is_active, @remaining_stock)";
                                command.Parameters.AddWithValue("@id", product.Id);
                                command.Parameters.AddWithValue("@product_no", product.Product_no);
                                command.Parameters.AddWithValue("@name", product.Name);
                                command.Parameters.AddWithValue("@category", product.Category);
                                command.Parameters.AddWithValue("@design_code", product.Design_code);
                                command.Parameters.AddWithValue("@colour_code", product.Colour_code);
                                command.Parameters.AddWithValue("@price", product.Price);
                                command.Parameters.AddWithValue("@barcode", product.Barcode);
                                command.Parameters.AddWithValue("@is_active", product.Is_active);
                                command.Parameters.AddWithValue("@remaining_stock", product.Remaining_stock);
                                command.Prepare();
                                command.ExecuteNonQuery();
                            }
                        }

                        // get last update record
                        command.CommandText = "SELECT * FROM update_record";
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                lastUpdate = rdr[0].ToString();
                            }
                            rdr.Close();
                        }

                        if (lastUpdate == null)
                        {
                            // create product update record
                            command.CommandText = "INSERT INTO update_record(type, last_update) VALUES(@type, @currentDateTime)";
                            command.Parameters.AddWithValue("@type", "product");
                            command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                            command.Prepare();
                            command.ExecuteNonQuery();

                            // create remaining_stock update record
                            command.CommandText = "INSERT INTO update_record(type, last_update) VALUES(@type, @currentDateTime)";
                            command.Parameters.AddWithValue("@type", "remaining_stock");
                            command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                            command.Prepare();
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            // update last_update record
                            command.CommandText = "UPDATE update_record SET last_update = @currentDateTime";
                            command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                            command.Prepare();
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
                System.Diagnostics.Trace.WriteLine("finished Reset Products Table");
            }
        }


        public async Task CheckRecallList()
        {
            System.Diagnostics.Trace.WriteLine("Check Recall List");

            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    // remove records stored more than 12hours
                    command.CommandText = "DELETE FROM temp_products WHERE created_at <= Datetime('now','-12 hour','localtime')";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }


        public async Task LoadLocalSettings()
        {
            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                dynamic result = new { };
                bool isLocalDbEmpty = false;

                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "SELECT * FROM settings";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            result = new
                            {
                                id = rdr["id"].ToString(),
                                address = rdr["address"].ToString(),
                                phone_no = rdr["phone_no"].ToString(),
                                gov_charge_name = rdr["gov_charge_name"].ToString(),
                                gov_charge_value = rdr["gov_charge_value"].ToString(),
                                gov_charge_no = rdr["gov_charge_no"].ToString(),
                                scanner_is_used = rdr["scanner_is_used"].ToString(),
                                is_first_login = rdr["is_first_login"].ToString(),
                                is_update = rdr["is_update"].ToString()
                            };
                        }
                        else
                        {
                            isLocalDbEmpty = true;
                            result = new
                            {
                                is_update = "0"
                            };
                        }
                        rdr.Close();
                    }

                    // insert 1 default record if empty
                    if (isLocalDbEmpty)
                    {
                        // insert user
                        command.CommandText = "INSERT INTO settings(id, address, phone_no, gov_charge_name, gov_charge_value, gov_charge_no, scanner_is_used, is_first_login, is_update)" +
                                                " VALUES(@id, @address, @phone_no, @gov_charge_name, @gov_charge_value, @gov_charge_no, @scanner_is_used, @is_first_login, @is_update)";
                        command.Parameters.AddWithValue("@id", 1);
                        command.Parameters.AddWithValue("@address", "-");
                        command.Parameters.AddWithValue("@phone_no", "-");
                        command.Parameters.AddWithValue("@gov_charge_name", "-");
                        command.Parameters.AddWithValue("@gov_charge_value", "-");
                        command.Parameters.AddWithValue("@gov_charge_no", "-");
                        command.Parameters.AddWithValue("@scanner_is_used", 1);
                        command.Parameters.AddWithValue("@is_first_login", 0);
                        command.Parameters.AddWithValue("@is_update", 0);
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }

                    if (result.is_update == "1")
                    {
                        Properties.Settings.Default.Setting_SystemAddress = result.address;
                        Properties.Settings.Default.Setting_SystemPhoneNo = result.phone_no;
                        Properties.Settings.Default.Setting_GovChargesName = result.gov_charge_name;
                        Properties.Settings.Default.Setting_GovChargesValue = result.gov_charge_value;
                        Properties.Settings.Default.Setting_GovChargesNo = result.gov_charge_no;
                        Properties.Settings.Default.Setting_ScannerIsUsed = result.scanner_is_used == "0" ? false : true;
                        Properties.Settings.Default.Setting_System_IsFirstLogin = result.is_first_login == "0" ? false : true;
                        Properties.Settings.Default.Save();

                        // change back to default value
                        command.CommandText = "UPDATE users SET id = @id, address = @address, phone_no = @phone_no, gov_charge_name = @gov_charge_name, gov_charge_value = @gov_charge_value, gov_charge_no = @gov_charge_no, sacnner_is_used = @sacnner_is_used, is_first_login = @is_first_login, is_update = @is_update";
                        command.Parameters.AddWithValue("@id", 1);
                        command.Parameters.AddWithValue("@address", "-");
                        command.Parameters.AddWithValue("@phone_no", "-");
                        command.Parameters.AddWithValue("@gov_charge_name", "-");
                        command.Parameters.AddWithValue("@gov_charge_value", "-");
                        command.Parameters.AddWithValue("@gov_charge_no", "-");
                        command.Parameters.AddWithValue("@scanner_is_used", 1);
                        command.Parameters.AddWithValue("@is_first_login", 0);
                        command.Parameters.AddWithValue("@is_update", 0);
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }


        public async Task LoadLocalDatabase()
        {
            //Load local settings
            await LoadLocalSettings();

            //Setup Database if first time login
            if (Properties.Settings.Default.Setting_System_IsFirstLogin)
            {
                await SetupLocalDB();

                //check if user change branch
                await CheckBranch();
            }
            else
            {
                //check if user change branch
                await CheckBranch();

                //Sync Database
                await SyncLocalDB();
            }

            //check cashier recall product list
            await CheckRecallList();
        }
    }
}
