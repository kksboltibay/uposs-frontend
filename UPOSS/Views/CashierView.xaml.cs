using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            // key: ctrl +
            // payment
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.Add, () => {
                try {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(btnPayment);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                } catch (Exception e) {

                }
            });

            // key: ctrl 1
            // focus on search barcode column
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.D1, () => {
                tbBarcode.Focus();
            });

            // key: ctrl 2
            // focus on search product category column
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.D2, () => {
                tbProductCategory.Focus();
            });

            // key: ctrl 3
            // focus on search product name column
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.D3, () => {
                tbProductName.Focus();
            });

            // key: ctrl ` (below esc)
            // edit latest price
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.OemTilde, () => {
                if (dgProduct.Items.Count > 0)
                {
                    dgProduct.SelectionUnit = DataGridSelectionUnit.Cell;
                    dgProduct.CurrentCell = new DataGridCellInfo(dgProduct.Items[dgProduct.Items.Count - 1], dgProduct.Columns[4]);
                    dgProduct.BeginEdit();
                    dgProduct.SelectionUnit = DataGridSelectionUnit.FullRow;
                }
            });

            // key: ctrl *
            // edit latest quantity
            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.Multiply, () => {
                if (dgProduct.Items.Count > 0)
                {
                    dgProduct.SelectionUnit = DataGridSelectionUnit.Cell;
                    dgProduct.CurrentCell = new DataGridCellInfo(dgProduct.Items[dgProduct.Items.Count - 1], dgProduct.Columns[5]);
                    dgProduct.BeginEdit();
                    dgProduct.SelectionUnit = DataGridSelectionUnit.FullRow;
                }
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
                // Check if scanner is not used
                if (!Properties.Settings.Default.Setting_ScannerIsUsed)
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
                    using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                    {
                        connection.Open();
                        try
                        {
                            using var command = new SQLiteCommand(connection);

                            // get product barcode
                            command.CommandText = "SELECT barcode FROM products WHERE barcode LIKE '%" + tbBarcode.Text.ToUpper() + "%' AND is_active = '1' LIMIT 5";
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lbBarcode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Check if scanner is not used
                if (!Properties.Settings.Default.Setting_ScannerIsUsed) 
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
                    tbProductCategory.Text = "";
                    tbProductName.Text = "";
                    lbBarcode.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    // Check if scanner is not used
                    if (!Properties.Settings.Default.Setting_ScannerIsUsed)
                    {
                        if (lbBarcode.Items.Count > 0)
                        {
                            lbBarcode.SelectedIndex = 0;
                        }
                        else
                        {
                            MessageBox.Show("Result not found", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Warning);
                            tbBarcode.Text = "";
                        }
                    } 
                    else
                    {
                        if (!string.IsNullOrEmpty(tbBarcode.Text))
                        {
                            // Search from local DB
                            using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                            {
                                connection.Open();
                                try
                                {
                                    using var command = new SQLiteCommand(connection);

                                    SuggestionList = new ObservableCollection<string>();

                                    // get product category
                                    command.CommandText = "SELECT barcode FROM products WHERE barcode = '" + tbBarcode.Text.ToUpper() + "' AND is_active = '1' LIMIT 1";
                                    using (SQLiteDataReader rdr = command.ExecuteReader())
                                    {
                                        if (rdr.Read())
                                        {
                                            SuggestionList.Add(rdr[0].ToString());
                                        }
                                        rdr.Close();
                                    }

                                    if (SuggestionList.Count > 0)
                                    {
                                        lbBarcode.ItemsSource = SuggestionList.ToList();

                                        lbBarcode.SelectedIndex = 0;

                                        // refresh list box and binding
                                        lbBarcode.SelectedIndex = -1;

                                        //refresh all search columns
                                        tbBarcode.Text = "";
                                        tbProductCategory.Text = "";
                                        tbProductName.Text = "";
                                    }
                                    else
                                    {
                                        MessageBox.Show("Result not found", "UPO$$");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message.ToString(), "UPO$$");
                                }
                                connection.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion


        #region Search Section (Product Category)
        //private void tbProductCategory_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(tbProductCategory.Text))
        //        {
        //            //Close pop up
        //            popupProductCategory.Visibility = Visibility.Collapsed;
        //            popupProductCategory.IsOpen = false;
        //            lbProductCategory.Visibility = Visibility.Collapsed;
        //            return;
        //        }
        //        //Open pop up
        //        popupProductCategory.Visibility = Visibility.Visible;
        //        popupProductCategory.IsOpen = true;
        //        lbProductCategory.Visibility = Visibility.Visible;

        //        SuggestionList = new ObservableCollection<string>();

        //        // Search from local DB
        //        using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
        //        {
        //            connection.Open();
        //            try
        //            {
        //                using var command = new SQLiteCommand(connection);

        //                // get product category
        //                command.CommandText = "SELECT category FROM products WHERE category LIKE '%" + tbProductCategory.Text.ToUpper() + "%' AND is_active = '1' LIMIT 5";
        //                using (SQLiteDataReader rdr = command.ExecuteReader())
        //                {
        //                    while (rdr.Read())
        //                    {
        //                        SuggestionList.Add(rdr[0].ToString());
        //                    }
        //                    rdr.Close();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show(ex.Message.ToString(), "UPO$$");
        //            }
        //            connection.Close();
        //        }

        //        lbProductCategory.ItemsSource = SuggestionList.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private void lbProductCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (lbProductCategory.SelectedIndex <= -1)
        //        {
        //            //Close pop up
        //            popupProductCategory.Visibility = Visibility.Collapsed;
        //            popupProductCategory.IsOpen = false;
        //            lbProductCategory.Visibility = Visibility.Collapsed;
        //            return;
        //        }
        //        //Close pop up
        //        popupProductCategory.Visibility = Visibility.Collapsed;
        //        popupProductCategory.IsOpen = false;
        //        lbProductCategory.Visibility = Visibility.Collapsed;

        //        tbBarcode.Text = "";
        //        tbProductCategory.Text = "";
        //        tbProductName.Text = "";
        //        lbProductCategory.SelectedIndex = -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void tbProductCategory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (!string.IsNullOrEmpty(tbProductCategory.Text))
                    {
                        // Search from local DB
                        using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
                        {
                            connection.Open();
                            try
                            {
                                using var command = new SQLiteCommand(connection);

                                SuggestionList = new ObservableCollection<string>();

                                // get product category
                                command.CommandText = "SELECT category FROM products WHERE category = '" + tbProductCategory.Text + "' AND is_active = '1' LIMIT 1";
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        SuggestionList.Add(rdr[0].ToString());
                                    }
                                    rdr.Close();
                                }

                                if (SuggestionList.Count > 0)
                                {
                                    lbProductCategory.ItemsSource = SuggestionList.ToList();

                                    lbProductCategory.SelectedIndex = 0;

                                    // refresh list box and binding
                                    lbProductCategory.SelectedIndex = -1;

                                    //refresh all search columns
                                    tbBarcode.Text = "";
                                    tbProductCategory.Text = "";
                                    tbProductName.Text = "";
                                }
                                else
                                {
                                    MessageBox.Show("Result not found", "UPO$$");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message.ToString(), "UPO$$");
                            }
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                using (var connection = new SQLiteConnection("Data Source=../SQLiteDatabase.db"))
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
                tbProductCategory.Text = "";
                tbProductName.Text = "";
                lbProductName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbProductName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (lbProductName.Items.Count > 0)
                {
                    lbProductName.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Result not found", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
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
            IEditableCollectionView itemsView = dgProduct.Items;
            if (itemsView.IsAddingNew || itemsView.IsEditingItem)
            {
                e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);

                if (e.Handled)
                {
                    MessageBox.Show("Only number with decimal is allowed in Product [Price] and [Qty]", "UPO$$");
                }
            }
        }
        #endregion



        #region Other
        public void Load(object sender, RoutedEventArgs e)
        {
            // or FocusManager.FocusedElement = tbBarcode;

            //tbBarcode.Focus();

            //searchBox.tbBarcode.Focus();

            tbProductCategory.Focus();
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
