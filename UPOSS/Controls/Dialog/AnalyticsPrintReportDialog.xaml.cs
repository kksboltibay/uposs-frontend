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
    /// Interaction logic for AnalyticsPrintReportDialog.xaml
    /// </summary>
    public partial class AnalyticsPrintReportDialog : Window
    {
        public AnalyticsPrintReportDialog(Analytics analytics, string selectedDateFrom, string selectedDateTo)
        {
            InitializeComponent();

            LoadReceipt(analytics, selectedDateFrom, selectedDateTo);

            Print();
        }

        private void LoadReceipt(Analytics analytics, string selectedDateFrom, string selectedDateTo)
        {
            try
            {
                tbkBranch.Text = Properties.Settings.Default.CurrentBranch;
                tbkGeneratedDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                tbkDateOfReport.Text = selectedDateFrom + " - " + selectedDateTo;

                // main section
                // cash
                tbkCashAmount.Text = analytics.Total_cash_sales;
                tbkCashQty.Text = analytics.Total_cash_sales_qty;

                // card
                tbkCardAmount.Text = analytics.Total_card_sales;
                tbkCardQty.Text = analytics.Total_card_sales_qty;

                // collection
                tbkTotalCollectionQty.Text = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(analytics.Total_cash_sales_qty), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(analytics.Total_card_sales_qty), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                tbkTotalCollectionAmount.Text = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(analytics.Total_cash_sales), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(analytics.Total_card_sales), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                tbkCashierUsername.Text = analytics.Cashier_username;
                tbkCashierQty.Text = tbkTotalCollectionQty.Text;
                tbkCashierAmount.Text = tbkTotalCollectionAmount.Text;

                tbkCashierTotalQty.Text = tbkCashierQty.Text;
                tbkCashierTotalAmount.Text = tbkCashierAmount.Text;

                // net sales after ++
                tbkNetSalesAfterAmount.Text = tbkCashierTotalAmount.Text;

                // tax
                tbkGovTaxValue.Text = Math.Round(Convert.ToDecimal(Properties.Settings.Default.Setting_GovChargesValue) * 100, 2).ToString() + "%";
                tbkTaxQty.Text = tbkCashierTotalQty.Text;
                tbkTaxAmount.Text = analytics.Total_tax;

                tbkTotalTaxQty.Text = tbkTaxQty.Text;
                tbkTotalTaxAmount.Text = tbkTaxAmount.Text;

                // net sales before ++
                tbkNetSalesBeforeAmount.Text = Math.Round(Convert.ToDecimal(
                    Math.Round(Convert.ToDecimal(tbkNetSalesAfterAmount.Text), 2, MidpointRounding.AwayFromZero) - Math.Round(Convert.ToDecimal(tbkTaxAmount.Text), 2, MidpointRounding.AwayFromZero)
                ), 2, MidpointRounding.AwayFromZero).ToString();

                // gross sales
                //tbkGrossSalesAmount.Text = tbkNetSalesBeforeAmount.Text;

                // discount
                //tbkItemDiscountAmount.Text = analytics.Total_discount;
                //tbkTotalDiscountAmount.Text = tbkItemDiscountAmount.Text;

                // total revenue
                //tbkTotalRevenueAmount.Text = Math.Round(Convert.ToDecimal(
                //    Math.Round(Convert.ToDecimal(tbkGrossSalesAmount.Text), 2, MidpointRounding.AwayFromZero) - Math.Round(Convert.ToDecimal(tbkTotalDiscountAmount.Text), 2, MidpointRounding.AwayFromZero)
                //), 2, MidpointRounding.AwayFromZero).ToString();

                // total revenue v2
                tbkTotalRevenueAmount.Text = tbkNetSalesBeforeAmount.Text;

                // void transaction
                tbkVoidQty.Text = analytics.Total_void_qty;
                tbkVoidAmount.Text = analytics.Total_void_amount;

                // sales tender count
                tbkSalesTenderQty.Text = tbkCashierTotalQty.Text;
                tbkSalesTenderAmount.Text = tbkTotalRevenueAmount.Text;
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
