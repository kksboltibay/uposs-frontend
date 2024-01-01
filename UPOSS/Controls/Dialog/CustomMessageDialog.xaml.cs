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
    /// Interaction logic for CustomMessageDialog.xaml
    /// </summary>
    public partial class CustomMessageDialog : Window
    {
        public CustomMessageDialog(string text)
        {
            InitializeComponent();

            tbkMessage.Text = text;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
