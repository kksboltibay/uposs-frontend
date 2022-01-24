using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Printing;
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
    /// Interaction logic for CashierPrintReceiptDialog.xaml
    /// </summary>
    public partial class CashierPrintReceiptDialog : Window
    {
        public CashierPrintReceiptDialog(dynamic param, Cashier response, string cashierUsername)
        {
            InitializeComponent();

            LoadReceipt(param, response, cashierUsername);

            Print();
        }

        private void LoadReceipt(dynamic param, Cashier response, string cashierUsername)
        {
            ObservableCollection<Product> cartList = new ObservableCollection<Product>(param.cartList);

            try
            {
                // Gov Charges Reg No.
                if (Properties.Settings.Default.Setting_GovChargesName != "" && Properties.Settings.Default.Setting_GovChargesNo != "")
                {
                    wpGovChargesRegNo.Visibility = Visibility.Visible;
                }

                tbkReceiptNo.Text = response.Receipt_no;
                tbkCashierUsername.Text = cashierUsername;
                tbkDatetime.Text = response.Datetime;

                // Cart List
                for (int i = 0; i < cartList.Count; i++)
                {
                    // Row line 1
                    TextBlock tbk = new TextBlock();
                    tbk.Text = cartList[i].Name.Substring(0, Math.Min(cartList[i].Name.Length, 20));
                    tbk.HorizontalAlignment = HorizontalAlignment.Left;
                    tbk.Margin = new Thickness(1, 0, 1, 0);

                    // add into stack panel
                    spCartList.Children.Add(tbk);

                    // Row line 2
                    // create grid with columns
                    Grid gridCartList = new Grid();

                    ColumnDefinition gridCol0 = new ColumnDefinition();
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    ColumnDefinition gridCol2 = new ColumnDefinition();
                    ColumnDefinition gridCol3 = new ColumnDefinition();

                    gridCol0.Width = new GridLength(80, GridUnitType.Pixel);
                    gridCol1.Width = new GridLength(50, GridUnitType.Pixel);
                    gridCol2.Width = new GridLength(55, GridUnitType.Pixel);
                    gridCol3.Width = new GridLength(60, GridUnitType.Pixel);

                    gridCartList.ColumnDefinitions.Add(gridCol0);
                    gridCartList.ColumnDefinitions.Add(gridCol1);
                    gridCartList.ColumnDefinitions.Add(gridCol2);
                    gridCartList.ColumnDefinitions.Add(gridCol3);

                    // column 0
                    TextBlock tbk0 = new TextBlock();
                    tbk0.Text = cartList[i].Product_no;
                    tbk0.HorizontalAlignment = HorizontalAlignment.Left;
                    tbk0.Margin = new Thickness(1, 0, 1, 0);
                    Grid.SetColumn(tbk0, 0);
                    gridCartList.Children.Add(tbk0);

                    // column 1
                    TextBlock tbk1 = new TextBlock();
                    tbk1.Text = cartList[i].Total_stock;
                    tbk1.HorizontalAlignment = HorizontalAlignment.Right;
                    tbk1.Margin = new Thickness(1, 0, 1, 0);
                    Grid.SetColumn(tbk1, 1);
                    gridCartList.Children.Add(tbk1);

                    // column 2
                    TextBlock tbk2 = new TextBlock();
                    tbk2.Text = cartList[i].Price;
                    tbk2.HorizontalAlignment = HorizontalAlignment.Right;
                    tbk2.Margin = new Thickness(1, 0, 1, 0);
                    Grid.SetColumn(tbk2, 2);
                    gridCartList.Children.Add(tbk2);

                    // column 3
                    TextBlock tbk3 = new TextBlock();
                    tbk3.Text = cartList[i].Subtotal;
                    tbk3.HorizontalAlignment = HorizontalAlignment.Right;
                    tbk3.Margin = new Thickness(1, 0, 1, 0);
                    Grid.SetColumn(tbk3, 3);
                    gridCartList.Children.Add(tbk3);

                    // add into stack panel
                    spCartList.Children.Add(gridCartList);
                }

                // Total
                tbkTotal.Text = param.totalAmount;

                // Gov Charges
                if (Properties.Settings.Default.Setting_GovChargesName != "" && Properties.Settings.Default.Setting_GovChargesValue != "")
                {
                    tbkGovChargesValue.Text = (Math.Round(Convert.ToDecimal(Properties.Settings.Default.Setting_GovChargesValue), 2) * 100).ToString("0.##");
                    tbkGovTax.Text = param.totalTax;

                    gridGovCharges.Visibility = Visibility.Visible;
                }

                // Payment Method
                if (param.paymentMethod == "cash")
                {
                    tbkPaymentMethod.Text = "Cash : ";
                    tbkCustomerPay.Text = param.cashPay;
                }
                else
                {
                    tbkPaymentMethod.Text = "Card : ";
                    tbkCustomerPay.Text = param.cardPay;
                }

                // Change
                tbkChange.Text = param.change;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }

        private void Print()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();

                //direct print without choosing printer
                printDialog.PrintVisual(printGrid, "");


                //if (printDialog.ShowDialog() == true)
                //{
                //    printDialog.PrintVisual(printGrid, "");
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
    }
}
