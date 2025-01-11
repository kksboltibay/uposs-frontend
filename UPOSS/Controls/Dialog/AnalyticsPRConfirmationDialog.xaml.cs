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

namespace UPOSS.Controls.Dialog
{
    /// <summary>
    /// Interaction logic for AnalyticsPRConfirmationDialog.xaml
    /// </summary>
    public partial class AnalyticsPRConfirmationDialog : Window
    {
        public AnalyticsPRConfirmationDialog()
        {
            InitializeComponent();
        }

        private void datePickerFrom_Loaded(object sender, RoutedEventArgs e)
        {
            // default date = today
            datePickerFrom.SelectedDate = DateTime.Today;
        }

        private void datePickerTo_Loaded(object sender, RoutedEventArgs e)
        {
            // default date = today
            datePickerTo.SelectedDate = DateTime.Today;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(datePickerFrom.Text) || datePickerFrom.SelectedDate == null || string.IsNullOrWhiteSpace(datePickerTo.Text) || datePickerTo.SelectedDate == null)
            {
                MessageBox.Show("Please select the date", "UPO$$");
                return;
            }

            this.DialogResult = true;
        }
    }
}
