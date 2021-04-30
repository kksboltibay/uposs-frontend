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

namespace UPOSS.Controls
{
    /// <summary>
    /// Interaction logic for DefaultInputDialog.xaml
    /// </summary>
    public partial class BranchInputDialog : Window
    {
		public BranchInputDialog(string question, string mode = "", Branch branch = null)
		{
			InitializeComponent();
			tbkQuestion.Text = question;
			Result = new Branch();

			if (mode == "add")
            {
				tbBranchName.Text = "Sample Branch";
			}
			else if (mode == "update")
            {
				tbBranchName.Text = branch.Name;
			}
            //else if (mode == "delete")
            //{
            //tbBranchName.Text = branch.Name;
            //tbBranchName.IsReadOnly = true;
            //tbBranchName.Focusable = false;
            //}
            else if (mode == "activate")
            {
                tbBranchName.Text = branch.Name;
                tbBranchName.IsReadOnly = true;
                tbBranchName.Focusable = false;
            }
            else if (mode == "deactivate")
            {
				tbBranchName.Text = branch.Name;
				tbBranchName.IsReadOnly = true;
				tbBranchName.Focusable = false;
			}
        }

		#region Define
		private Branch result;
		public Branch Result
		{
			get { return result; }
			set { result = value; }
		}
		#endregion

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			Result = new Branch
			{
				Name = tbBranchName.Text
			};
			this.DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			tbBranchName.SelectAll();
			tbBranchName.Focus();
		}
	}
}
