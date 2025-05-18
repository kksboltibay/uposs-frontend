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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UPOSS.Views
{
    /// <summary>
    /// Interaction logic for AnalyticsSalesView.xaml
    /// </summary>
    public partial class AnalyticsSalesView : UserControl
    {
        public AnalyticsSalesView()
        {
            InitializeComponent();
        }

        private void dtPickerFrom_Loaded(object sender, RoutedEventArgs e)
        {
            // default date = today
            dtPickerFrom.SelectedDate = DateTime.Today;
            dtPickerTo.SelectedDate = DateTime.Now;
        }
    }
}
