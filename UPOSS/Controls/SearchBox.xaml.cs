using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UPOSS.Models;

namespace UPOSS.Controls
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {         
        public SearchBox()
        {
            InitializeComponent();

            DataContext = this;

            SuggestionList = new ObservableCollection<string> { "item 1", "product 1", "item 2" };
        }

        #region Define
        public ObservableCollection<string> SuggestionList { get; set; }

        public Product SelectedProduct { get; set; }

        #endregion


        #region Open Auto Suggestion box method  
        private void OpenAutoSuggestionBox()
        {
            try
            {
                // Enable.  
                autoListPopup.Visibility = Visibility.Visible;
                autoListPopup.IsOpen = true;
                autoList.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.Write(ex);
            }
        }
        #endregion


        #region Close Auto Suggestion box method  
        private void CloseAutoSuggestionBox()
        {
            try
            {
                // Enable.  
                autoListPopup.Visibility = Visibility.Collapsed;
                autoListPopup.IsOpen = false;
                autoList.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.Write(ex);
            }
        }
        #endregion


        #region Auto Text Box text changed the method
        private void AutoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Verification.  
                if (string.IsNullOrEmpty(autoTextBox.Text))
                {
                    // Disable.  
                    CloseAutoSuggestionBox();

                    return;
                }

                // Enable.  
                OpenAutoSuggestionBox();

                // Settings.  
                autoList.ItemsSource = SuggestionList.Where(p => p.ToLower().Contains(autoTextBox.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.Write(ex);
            }
        }
        #endregion


        #region Auto list selection changed method
        private void AutoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine(autoList.SelectedIndex.ToString());

                // Verification.  
                if (autoList.SelectedIndex <= -1)
                {
                    // Disable.  
                    CloseAutoSuggestionBox();

                    // Info.  
                    return;
                }

                // Disable.  
                CloseAutoSuggestionBox();

                // Settings.  
                autoTextBox.Text = autoList.SelectedItem.ToString();
                autoList.SelectedIndex = -1;

                SelectedProduct = new Product { Barcode = autoTextBox.Text };
            }
            catch (Exception ex)
            {
                // Info.  
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.Write(ex);
            }
        }
        #endregion
    }
}
