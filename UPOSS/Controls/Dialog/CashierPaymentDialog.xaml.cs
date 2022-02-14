using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CashierPaymentDialog.xaml
    /// </summary>
    public partial class CashierPaymentDialog : Window
    {
        public CashierPaymentDialog(string totalAmount)
        {
            InitializeComponent();

            _totalAmount = Math.Round(Convert.ToDecimal(totalAmount), 2);
            tbkTotal.Text = _totalAmount.ToString();
            
            SetPaymentMethodStatus(false, "cash");
            SetPaymentMethodStatus(false, "card");

            CalculateRecommendButtonPrice();

            LoadBankNameList();
        }

        #region Define
        private decimal _totalAmount = 0;

        public Cashier Payment { get; set; } = new Cashier();
        #endregion

        #region Custom
        private void SetPaymentMethodStatus(bool status, string type)
        {
            if (type == "cash")
            {
                tbCashPay.IsEnabled = status;
                btnChoice1.IsEnabled = status;
                btnChoice2.IsEnabled = status;

                if (status)
                {
                    tbCashPay.Text = _totalAmount.ToString();
                }
                else
                {
                    tbCashPay.Text = "0.00";
                    checkboxCash.IsChecked = false;
                }
            }

            if (type == "card")
            {
                tbCardPay.IsEnabled = status;
                tbCardNo.IsEnabled = status;
                comboboxBankNameList.IsEnabled = status;
                comboboxCardType.IsEnabled = status;

                if (status)
                {
                    tbCardPay.Text = _totalAmount.ToString();
                }
                else
                {
                    tbCardPay.Text = "0.00";
                    checkboxCard.IsChecked = false;
                }
            }
        }

        private void CalculateRecommendButtonPrice()
        {
            decimal wholeNumber = Math.Ceiling(_totalAmount);

            tbkChoice1Value.Text = wholeNumber.ToString("0.00");

            tbkChoice2Value.Text = (Math.Ceiling(wholeNumber / 10) * 10).ToString("0.00");
        }

        private void LoadBankNameList()
        {
            List<string> bankNameList = new List<string>();

            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
            {
                connection.Open();
                try
                {
                    using var command = new SQLiteCommand(connection);

                    command.CommandText = "SELECT name FROM banks";
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                           bankNameList.Add(rdr[0].ToString());
                        }
                        rdr.Close();
                    }

                    comboboxBankNameList.ItemsSource = bankNameList;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "UPO$$");
                }
                connection.Close();
            }
        }

        private bool ValidatePayment()
        {
            if (checkboxCash.IsChecked == false && checkboxCard.IsChecked == false)
            {
                MessageBox.Show("Error: Please select payment method.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                decimal totalPayment = Math.Round(Convert.ToDecimal(tbCashPay.Text), 2, MidpointRounding.AwayFromZero) + Math.Round(Convert.ToDecimal(tbCardPay.Text), 2, MidpointRounding.AwayFromZero);

                if (totalPayment < _totalAmount)
                {
                    // check if payment is enough
                    MessageBox.Show("Error: Insufficient fund.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (checkboxCash.IsChecked == true)
                {
                    // pay by cash
                    Payment.Cash_pay = Math.Round(Convert.ToDecimal(tbCashPay.Text), 2, MidpointRounding.AwayFromZero).ToString("0.00");
                    Payment.Payment_method = "cash";
                }
                else
                {
                    // pay by card
                    if (comboboxCardType.SelectedValue.ToString() != "E-Wallet" && tbCardNo.Text.Length != 16)
                    {
                        MessageBox.Show("Error: Incorrect [Card No]", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    Payment.Card_no = tbCardNo.Text;
                    Payment.Card_pay = Math.Round(Convert.ToDecimal(tbCardPay.Text), 2, MidpointRounding.AwayFromZero).ToString("0.00");
                    Payment.Bank_name = comboboxBankNameList.SelectedItem.ToString();
                    Payment.Card_type = comboboxCardType.SelectedValue.ToString();
                    Payment.Payment_method = "card";
                }

                // change
                Payment.Change = Math.Round(totalPayment - _totalAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00");
            }

            return true;
        }
        #endregion


        private void btnDialogConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (ValidatePayment())
                this.DialogResult = true;
        }

        private void btnChoice1_Click(object sender, RoutedEventArgs e)
        {
            tbCashPay.Text = tbkChoice1Value.Text;
        }

        private void btnChoice2_Click(object sender, RoutedEventArgs e)
        {
            tbCashPay.Text = tbkChoice2Value.Text;
        }

        private void checkboxCash_Changed(object sender, RoutedEventArgs e)
        {
            if (checkboxCash.IsChecked == true)
            {
                SetPaymentMethodStatus(true, "cash");
                SetPaymentMethodStatus(false, "card");
            }
            else
            {
                SetPaymentMethodStatus(false, "cash");
            }
        }

        private void checkboxCard_Changed(object sender, RoutedEventArgs e)
        {
            if (checkboxCard.IsChecked == true)
            {
                SetPaymentMethodStatus(true, "card");
                SetPaymentMethodStatus(false, "cash");
            }
            else
            {
                SetPaymentMethodStatus(false, "card");
            }
        }

        private void checkTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);

            if (e.Handled)
            {
                MessageBox.Show("Only number with decimal is allowed.", "UPO$$");
            }
        }

        private void checkPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        private void tbCardNo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);

            if (e.Handled)
            {
                MessageBox.Show("Only number is allowed.", "UPO$$");
            }
        }

        private void tbCashPay_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbCashPay.Text))
            {
                tbCashPay.Text = "0.00";
            }
        }

        private void tbCardPay_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbCardPay.Text))
            {
                tbCardPay.Text = "0.00";
            }
        }

        private void comboboxCardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // check if eWallet is selected
            if (comboboxCardType.SelectedValue.ToString() == "E-Wallet")
            {
                // mute other columns
                tbCardNo.Text = "";
                comboboxBankNameList.SelectedValue = "Other";
                tbCardNo.IsHitTestVisible = false;
                comboboxBankNameList.IsHitTestVisible = false;
            }
            else
            {
                // reopen
                tbCardNo.IsHitTestVisible = true;
                comboboxBankNameList.IsHitTestVisible = true;
            }
        }
    }
}
