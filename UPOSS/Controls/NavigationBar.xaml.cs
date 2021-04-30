using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using UPOSS.Services;

namespace UPOSS.Controls
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        APIService ObjLogoutService;
        public NavigationBar()
        {
            InitializeComponent();

            ObjLogoutService = new APIService();
        }

        private async void LV_ExitBtn(object sender, MouseButtonEventArgs e)
        {
            bool logoutFail = false;
            var msgBoxResult = MessageBox.Show("Do you want to exit UPO$$ ?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (msgBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    dynamic param = new { username = Properties.Settings.Default.CurrentUsername };

                    RootUserObject Response = await ObjLogoutService.PostAPI("logout", param, "user");

                    if (Response.Status == "ok")
                    {
                        Properties.Settings.Default.CurrentUsername = "";
                        Properties.Settings.Default.CurrentUserRole = "";
                        Properties.Settings.Default.Save();

                        // shut down
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        logoutFail = true;
                    }
                }
                catch (Exception error)
                {
                    logoutFail = true;
                }
            }

            if (logoutFail)
            {
                var exitResult = MessageBox.Show("Logout error, there might be internet connection problem, do you still want to exit UPO$$ ?" +
                    "\b(Note: Logout like this may cause some server issues, please contact IT support)", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (exitResult == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.CurrentUsername = "";
                    Properties.Settings.Default.CurrentUserRole = "";
                    Properties.Settings.Default.Save();

                    // shut down
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
