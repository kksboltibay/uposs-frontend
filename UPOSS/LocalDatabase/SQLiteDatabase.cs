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
            using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "CREATE TABLE IF NOT EXISTS update_record(last_update TEXT)";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS products(" +
                    "id INTEGER, " +
                    "product_no TEXT UNIQUE, " +
                    "name TEXT, " +
                    "category TEXT, " +
                    "design_code TEXT," +
                    "colour_code TEXT," +
                    "price REAL," +
                    "barcode TEXT," +
                    "is_active INTEGER" +
                    ")";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS update_record(last_update TEXT)";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
                _CurrentViewModel.IsLoading = false;
            }
        }

        
        public async Task SyncLocalDB()
        {
            _CurrentViewModel.IsLoading = true;

            DateTime now = DateTime.Now;
            var currentDateTime = DateTime.Now;
            string lastUpdate = null;

            string createProductsTableSQL = "CREATE TABLE IF NOT EXISTS products(" +
            "id INTEGER, " +
            "product_no TEXT UNIQUE, " +
            "name TEXT, " +
            "category TEXT, " +
            "design_code TEXT," +
            "colour_code TEXT," +
            "price REAL," +
            "barcode TEXT," +
            "is_active INTEGER" +
            ")";

            using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
            {
                connection.Open();

                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "CREATE TABLE IF NOT EXISTS update_record(last_update TEXT)";
                    command.ExecuteNonQuery();

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

                    if (lastUpdate != null)
                    {
                        //last update record exists
                        System.Diagnostics.Trace.WriteLine("Record found");

                        // calculate time span
                        TimeSpan timeSpan = now - DateTime.Parse(lastUpdate);
                        System.Diagnostics.Trace.WriteLine("Updated " + timeSpan.Days.ToString() + " days ago");

                        if (timeSpan.Days > 6)
                        {
                            // last update record > 6 days
                            System.Diagnostics.Trace.WriteLine(">6");
                            // drop products table
                            command.CommandText = "DROP TABLE IF EXISTS products";
                            command.ExecuteNonQuery();

                            // create products table
                            command.CommandText = createProductsTableSQL;
                            command.ExecuteNonQuery();

                            // request all products data from remote server
                            dynamic param = new { page = 0, is_active = "All", productListOnly = 1 };

                            RootProductObject Response = await ObjProductService.PostAPI("getProductList", param, _Path);

                            if (Response.Status != "ok")
                            {
                                //API Error 1
                                MessageBox.Show("Failed to sync local database, please contact IT support [AE1]", "UPO$$");
                            }
                            else
                            {
                                ProductList = new ObservableCollection<Product>(Response.Data);

                                if (ProductList != null)
                                {
                                    // insert into products table
                                    foreach (var product in ProductList)
                                    {
                                        command.CommandText = "INSERT INTO products(id, product_no, name, category, design_code, colour_code, price, barcode, is_active)" +
                                            " VALUES(@id, @product_no, @name, @category, @design_code, @colour_code, @price, @barcode, @is_active)";
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
                                // update last_update record
                                command.CommandText = "UPDATE update_record SET last_update = @currentDateTime";
                                command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                command.Prepare();
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // last update record <= 6 days
                            System.Diagnostics.Trace.WriteLine("<=6");

                            // request products data that need to be updated from remote server
                            dynamic param = new { lastUpdate = lastUpdate };
                            RootProductObject Response = await ObjProductService.PostAPI("getProductActivity", param, _Path);

                            if (Response.Status != "ok")
                            {
                                //API Error 2
                                MessageBox.Show("Failed to sync local database, please contact IT support [AE2]", "UPO$$");
                            }
                            else if (Response.Msg == "Product activity not found")
                            {
                                //MessageBox.Show("Local database is up-to-date", "UPO$$");
                                System.Diagnostics.Trace.WriteLine("Local database is up-to-date");
                            }
                            else
                            {
                                ProductList = new ObservableCollection<Product>(Response.Data);

                                // update products table
                                if (ProductList != null)
                                {
                                    foreach (var product in ProductList)
                                    {
                                        command.CommandText = "INSERT OR REPLACE INTO products(id, product_no, name, category, design_code, colour_code, price, barcode, is_active)" +
                                            " VALUES(@id, @product_no, @name, @category, @design_code, @colour_code, @price, @barcode, @is_active)";
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
                                        System.Diagnostics.Trace.WriteLine("Upsert");
                                    }
                                }
                                // update last_update record
                                command.CommandText = "UPDATE update_record SET last_update = @currentDateTime";
                                command.Parameters.AddWithValue("@currentDateTime", currentDateTime);
                                command.Prepare();
                                command.ExecuteNonQuery();
                                System.Diagnostics.Trace.WriteLine("Update last record");
                            }
                        }
                    }
                    else
                    {
                        // first time using system
                        // sync db with remote server
                        System.Diagnostics.Trace.WriteLine("No record found");

                        // drop products table
                        command.CommandText = "DROP TABLE IF EXISTS products";
                        command.ExecuteNonQuery();

                        // create products table
                        command.CommandText = createProductsTableSQL;
                        command.ExecuteNonQuery();

                        // request all products data from remote server
                        dynamic param = new { page = 0, is_active = "All", productListOnly = 1 };

                        RootProductObject Response = await ObjProductService.PostAPI("getProductList", param, _Path);

                        if (Response.Status != "ok")
                        {
                            //API Error 3
                            MessageBox.Show("Failed to sync local database, please contact IT support [AE3]", "UPO$$");
                        }
                        else
                        {
                            ProductList = new ObservableCollection<Product>(Response.Data);

                            if (ProductList != null)
                            {
                                // insert into products table
                                foreach (var product in ProductList)
                                {
                                    command.CommandText = "INSERT INTO products(id, product_no, name, category, design_code, colour_code, price, barcode, is_active)" +
                                        " VALUES(@id, @product_no, @name, @category, @design_code, @colour_code, @price, @barcode, @is_active)";
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
                            // insert last_update record
                            command.CommandText = "INSERT INTO update_record(last_update) VALUES(@currentDateTime)";
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
                _CurrentViewModel.IsLoading = false;
            }
        }
}
}
