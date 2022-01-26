using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UPOSS.Models;

namespace UPOSS.Controls.Dialog
{
    /// <summary>
    /// Interaction logic for CashierRecallDialog.xaml
    /// </summary>
    public partial class CashierRecallDialog : Window
    {
        public CashierRecallDialog()
        {
            InitializeComponent();
            ProductRecallList = new ObservableCollection<Product>();

            LoadRecallList();
        }


        #region Define
        public ObservableCollection<Product> ProductRecallList { get; set; }
        #endregion


        private void LoadRecallList()
        {
            //var currentDateTime = DateTime.Now;
            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "SELECT * FROM temp_products ORDER BY created_at DESC";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        int i = 0;
                        int recordNo = 1;
                        string datetime = "";
                        while (rdr.Read())
                        {
                            datetime = rdr[6].ToString();

                            if (ProductRecallList.Count > 0 && i > 0)
                            {
                                if (datetime == ProductRecallList[i - 1].Datetime)
                                {
                                    ProductRecallList[i - 1].List.Add(new Product
                                    {
                                        Product_no = rdr["product_no"].ToString(),
                                        Name = rdr["name"].ToString(),
                                        Barcode = rdr["barcode"].ToString(),
                                        Price = Math.Round(Convert.ToDecimal(rdr["price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        Total_stock = Math.Round(Convert.ToDecimal(rdr["qty"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        Original_price = Math.Round(Convert.ToDecimal(rdr["original_price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                    });
                                }
                                else
                                {
                                    ++recordNo;

                                    ProductRecallList.Add(new Product
                                    {
                                        Id = recordNo,
                                        Datetime = datetime,
                                        List = new List<Product>() { new Product
                                        {
                                            Product_no = rdr["product_no"].ToString(),
                                            Name = rdr["name"].ToString(),
                                            Barcode = rdr["barcode"].ToString(),
                                            Price = Math.Round(Convert.ToDecimal(rdr["price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                            Total_stock = Math.Round(Convert.ToDecimal(rdr["qty"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                            Original_price = Math.Round(Convert.ToDecimal(rdr["original_price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        }}
                                    });

                                    ++i;
                                }
                            }
                            else
                            {
                                ProductRecallList.Add(new Product
                                {
                                    Id = recordNo,
                                    Datetime = datetime,
                                    List = new List<Product>() { new Product
                                    {
                                        Product_no = rdr["product_no"].ToString(),
                                        Name = rdr["name"].ToString(),
                                        Barcode = rdr["barcode"].ToString(),
                                        Price = Math.Round(Convert.ToDecimal(rdr["price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        Total_stock = Math.Round(Convert.ToDecimal(rdr["qty"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                        Original_price = Math.Round(Convert.ToDecimal(rdr["original_price"].ToString()), 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                    }}
                                });

                                ++i;
                            }
                        }
                        rdr.Close();
                    }
                    connection.Close();

                    dgProduct.ItemsSource = ProductRecallList.ToList();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
            }
        }


        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
