using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPOSS.Custom.ShortcutKey;
using UPOSS.Models;

namespace UPOSS.Views
{
    /// <summary>
    /// Interaction logic for CashierView.xaml
    /// </summary>
    public partial class CashierView : UserControl
    {
        public CashierView()
        {
            InitializeComponent();
            this.Loaded += Load;

            togglebuttonScanner.IsChecked = Properties.Settings.Default.Setting_ScannerIsUsed;

            // set hotkey
            // key: +
            HotkeysManager.AddHotkey(ModifierKeys.None, Key.Add, () => {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(btnPayment);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            });

            // key: ` (below esc)
            HotkeysManager.AddHotkey(ModifierKeys.None, Key.OemTilde, () => {
                tbBarcode.Focus();
            });
        }


        #region Define
        public ObservableCollection<string> SuggestionList { get; set; }
        #endregion


        #region Search Section (Barcode)
        private void tbBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            { 
                if (string.IsNullOrEmpty(tbBarcode.Text))
                {
                    //Close pop up
                    popupBarcode.Visibility = Visibility.Collapsed;
                    popupBarcode.IsOpen = false;
                    lbBarcode.Visibility = Visibility.Collapsed;
                    return;
                }
                //Open pop up
                popupBarcode.Visibility = Visibility.Visible;
                popupBarcode.IsOpen = true;
                lbBarcode.Visibility = Visibility.Visible;

                SuggestionList = new ObservableCollection<string>();

                // Search from local DB
                using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
                {
                    connection.Open();
                    try
                    {
                        using var command = new SQLiteCommand(connection);

                        // get product barcode
                        command.CommandText = "SELECT barcode FROM products WHERE barcode LIKE '%"+ tbBarcode.Text.ToUpper() + "%' AND is_active = '1' LIMIT 5";
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                SuggestionList.Add(rdr[0].ToString());
                            }
                            rdr.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString(), "UPO$$");
                    }
                    connection.Close();
                }

                lbBarcode.ItemsSource = SuggestionList.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Check if scanner is used
                if (Properties.Settings.Default.Setting_ScannerIsUsed)
                {
                    if (lbBarcode.Items.Count > 0)
                    {
                        lbBarcode.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Barcode not found", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Warning);
                        tbBarcode.Text = "";
                    }
                }
            }
        }

        private void lbBarcode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbBarcode.SelectedIndex <= -1)
                {
                    //Close pop up
                    popupBarcode.Visibility = Visibility.Collapsed;
                    popupBarcode.IsOpen = false;
                    lbBarcode.Visibility = Visibility.Collapsed;
                    return;
                }
                //Close pop up
                popupBarcode.Visibility = Visibility.Collapsed;
                popupBarcode.IsOpen = false;
                lbBarcode.Visibility = Visibility.Collapsed;

                tbBarcode.Text = "";
                tbProductNo.Text = "";
                tbProductName.Text = "";
                lbBarcode.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        #region Search Section (Product No)
        private void tbProductNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(tbProductNo.Text))
                {
                    //Close pop up
                    popupProductNo.Visibility = Visibility.Collapsed;
                    popupProductNo.IsOpen = false;
                    lbProductNo.Visibility = Visibility.Collapsed;
                    return;
                }
                //Open pop up
                popupProductNo.Visibility = Visibility.Visible;
                popupProductNo.IsOpen = true;
                lbProductNo.Visibility = Visibility.Visible;

                SuggestionList = new ObservableCollection<string>();

                // Search from local DB
                using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
                {
                    connection.Open();
                    try
                    {
                        using var command = new SQLiteCommand(connection);

                        // get product_no
                        command.CommandText = "SELECT product_no FROM products WHERE product_no LIKE '%" + tbProductNo.Text.ToUpper() + "%' AND is_active = '1' LIMIT 5";
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                SuggestionList.Add(rdr[0].ToString());
                            }
                            rdr.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString(), "UPO$$");
                    }
                    connection.Close();
                }

                lbProductNo.ItemsSource = SuggestionList.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lbProductNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbProductNo.SelectedIndex <= -1)
                {
                    //Close pop up
                    popupProductNo.Visibility = Visibility.Collapsed;
                    popupProductNo.IsOpen = false;
                    lbProductNo.Visibility = Visibility.Collapsed;
                    return;
                }
                //Close pop up
                popupProductNo.Visibility = Visibility.Collapsed;
                popupProductNo.IsOpen = false;
                lbProductNo.Visibility = Visibility.Collapsed;

                tbBarcode.Text = "";
                tbProductNo.Text = "";
                tbProductName.Text = "";
                lbProductNo.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        #region Search Section (Product Name)
        private void tbProductName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(tbProductName.Text))
                {
                    //Close pop up
                    popupProductName.Visibility = Visibility.Collapsed;
                    popupProductName.IsOpen = false;
                    lbProductName.Visibility = Visibility.Collapsed;
                    return;
                }
                //Open pop up
                popupProductName.Visibility = Visibility.Visible;
                popupProductName.IsOpen = true;
                lbProductName.Visibility = Visibility.Visible;

                SuggestionList = new ObservableCollection<string>();

                // Search from local DB
                using (var connection = new SQLiteConnection("Data Source=SQLiteDatabase.db"))
                {
                    connection.Open();
                    try
                    {
                        using var command = new SQLiteCommand(connection);

                        // get product name
                        command.CommandText = "SELECT name FROM products WHERE name LIKE '%" + tbProductName.Text.ToUpper() + "%' AND is_active = '1' LIMIT 5";
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                SuggestionList.Add(rdr[0].ToString());
                            }
                            rdr.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString(), "UPO$$");
                    }
                    connection.Close();
                }

                lbProductName.ItemsSource = SuggestionList.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lbProductName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbProductName.SelectedIndex <= -1)
                {
                    //Close pop up
                    popupProductName.Visibility = Visibility.Collapsed;
                    popupProductName.IsOpen = false;
                    lbProductName.Visibility = Visibility.Collapsed;
                    return;
                }
                //Close pop up
                popupProductName.Visibility = Visibility.Collapsed;
                popupProductName.IsOpen = false;
                lbProductName.Visibility = Visibility.Collapsed;

                tbBarcode.Text = "";
                tbProductNo.Text = "";
                tbProductName.Text = "";
                lbProductName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion



        #region Datagrid Product List
        private void dgProduct_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            grid.SelectedItem = null;
            grid.CommitEdit(DataGridEditingUnit.Row, true);
            //grid.Items.Refresh();
        }

        private void dgProduct_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;

            if (e.Key == Key.Enter)
            {
                DataGrid grid = sender as DataGrid;
                grid.SelectedItem = null;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                //tbBarcode.Focus();
            }
        }

        private void dgProduct_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);

            if (e.Handled)
            {
                MessageBox.Show("Only number with decimal is allowed in Product [Price] and [Qty]", "UPO$$");
            }
        }
        #endregion



        #region Other
        public void Load(object sender, RoutedEventArgs e)
        {
            // or FocusManager.FocusedElement = tbBarcode;

            tbBarcode.Focus();

            //searchBox.tbBarcode.Focus();
        }

        private void togglebuttonScanner_Checked(object sender, RoutedEventArgs e)
        {
            if (togglebuttonScanner.IsChecked == true)
            {
                Properties.Settings.Default.Setting_ScannerIsUsed = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Setting_ScannerIsUsed = false;
                Properties.Settings.Default.Save();
            }
        }

        private void searchSection_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // only allowed alphanumeric input
            // others than alphanumeric will be ignored
            e.Handled = new Regex("[^0-9a-zA-Z]+").IsMatch(e.Text);
        }
        #endregion
    }
}
