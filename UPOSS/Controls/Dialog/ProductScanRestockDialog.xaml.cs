using System;
using System.Collections.Generic;
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
    /// Interaction logic for ProductScanRestockDialog.xaml
    /// </summary>
    public partial class ProductScanRestockDialog : Window
    {
        public ProductScanRestockDialog(string question, string mode = "", Product product = null, ProductQuantity quantity = null)
        {
            InitializeComponent();

            tbkQuestion.Text = question;
            tbScanner.Text = "";

            if (mode == "scanRestock")
            {
                tbBarcode.Text = product.Barcode;
                tbBranch.Text = quantity.Branch_name;
                tbkOriginalQuantity.Text = quantity.Quantity;
                tbQuantity.Text = "0";
            }
        }

        private void tbScanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (!string.IsNullOrEmpty(tbScanner.Text))
                    {
                        if (tbScanner.Text == tbBarcode.Text)
                        {
                            tbQuantity.Text = Math.Round(Convert.ToDecimal(Math.Round( Convert.ToDecimal(tbQuantity.Text), 2, MidpointRounding.AwayFromZero) + 1 ), 2, MidpointRounding.AwayFromZero).ToString();
                            tbScanner.Text = "";
                            tbScanner.Focus();
                        }
                        else
                        {
                            MessageBox.Show("Incorrect barcode, please try again.".ToString(), "UPO$$");
                            tbScanner.Text = "";
                            tbScanner.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "UPO$$");
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            tbScanner.SelectAll();
            tbScanner.Focus();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
