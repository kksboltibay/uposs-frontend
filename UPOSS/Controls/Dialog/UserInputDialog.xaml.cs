using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UPOSS.Models;
using UPOSS.Services;
using UPOSS.ViewModels;

namespace UPOSS.Controls
{
    /// <summary>
    /// Interaction logic for UserInputDialog.xaml
    /// </summary>
    public partial class UserInputDialog : Window
    {
		APIService ObjBranchService;

        public UserInputDialog(string question, string mode = "", User user = null)
		{
			InitializeComponent();
            ObjBranchService = new APIService();
            Result = new User();

            tbkQuestion.Text = question;
            GetRoleList();
            GetActiveBranchList();

            if (mode == "add")
            {
                tbUsername.Text = "Sample_username";
                tbPassword.Text = "Sample_password";
                cbRoleList.SelectedItem = "Staff";
                cbBranchList.SelectedItem = Properties.Settings.Default.CurrentBranch;
            } 
            else if (mode == "update")
            {
                tbUsername.IsReadOnly = true;
                tbUsername.Focusable = false;
                tbUsername.Text = user.Username;

                tbPassword.Text = "Sample_password";
                tbPassword.SelectAll();
                tbPassword.Focus();
                
                if (user.Role == "Super Admin")
                {
                    RoleList.Add("Super Admin");
                    cbRoleList.SelectedItem = "Super Admin";
                    cbRoleList.IsHitTestVisible = false;
                    cbRoleList.Focusable = false;

                    tbkBranch.Visibility = Visibility.Collapsed;
                    cbBranchList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cbRoleList.SelectedItem = user.Role;
                    cbBranchList.SelectedItem = user.Branch_name != null ? user.Branch_name : "";
                }
            }
            else if (mode == "delete")
            {
                tbUsername.Text = user.Username;
                tbUsername.IsReadOnly = true;
                tbUsername.Focusable = false;

                tbPassword.Text = "{Encrypted}";
                tbPassword.IsReadOnly = true;
                tbPassword.Focusable = false;

                cbRoleList.SelectedItem = user.Role;
                cbRoleList.IsReadOnly = true;
                cbRoleList.IsEnabled = false;

                cbBranchList.SelectedItem = user.Branch_name;
                cbBranchList.IsReadOnly = true;
                cbBranchList.IsEnabled = false;
            }
            else if (mode == "forceLogout")
            {
                tbUsername.Text = user.Username;
                tbUsername.IsReadOnly = true;
                tbUsername.Focusable = false;

                tbPassword.Text = "{Encrypted}";
                tbPassword.IsReadOnly = true;
                tbPassword.Focusable = false;

                RoleList.Add("Super Admin");
                cbRoleList.SelectedItem = user.Role;
                cbRoleList.IsReadOnly = true;
                cbRoleList.IsEnabled = false;

                cbBranchList.SelectedItem = user.Branch_name;
                cbBranchList.IsReadOnly = true;
                cbBranchList.IsEnabled = false;
            }
        }

        #region Define
        private User result;
        public User Result 
        {
            get { return result; }
            set { result = value; }
        }

        private ObservableCollection<string> RoleList;
        #endregion

        #region Custom
        private void GetRoleList()
        {
            RoleList = new ObservableCollection<string>();
            RoleList.Add("Admin");
            RoleList.Add("Staff");

            cbRoleList.ItemsSource = RoleList;
        }

        private async void GetActiveBranchList()
        {
            try
            {
                dynamic param = new { page = 0 };

                RootBranchObject Response = await ObjBranchService.PostAPI("getBranchList", param, "branch");

                if (Response.Status == "ok")
                {
                    cbBranchList.ItemsSource = Response.Data.OrderBy(property => property.Name).Select(item => item.Name);
                }
                else
                {
                    cbBranchList.Items.Add("- Fail to get branch list -");
                }
            }
            catch (Exception e)
            {
                cbBranchList.Items.Add("- Fail to get branch list -");
            }
        }
        #endregion


        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
            Result = new User 
            { 
                Username = tbUsername.Text, 
                Password = tbPassword.Text, 
                Role = cbRoleList.SelectedItem != null? cbRoleList.SelectedItem.ToString() : "",
                Branch_name = cbBranchList.SelectedItem != null ? cbBranchList.SelectedItem.ToString() : ""
            };

            this.DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			tbUsername.SelectAll();
			tbUsername.Focus();
		}
    }
}
