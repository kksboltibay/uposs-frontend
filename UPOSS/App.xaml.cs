using Squirrel;
using System;
using System.Windows;
using UPOSS.ViewModels;

namespace UPOSS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Window window = new MainWindow();
            window.DataContext = new MainViewModel();
            window.Show();
            base.OnStartup(e);

            CheckAvailableUpdate();
        }


        #region Check For Update
        private async void CheckAvailableUpdate()
        {
            UpdateManager manager;
            try
            {
                manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/kksboltibay/UPOSS");

                UPOSS.Properties.Settings.Default.CurrentApplicationVersion = manager.CurrentlyInstalledVersion().ToString();
                UPOSS.Properties.Settings.Default.Save();

                var updateInfo = await manager.CheckForUpdate();

                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    var update = MessageBox.Show("There is an update available, do you want to update?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (update == MessageBoxResult.Yes)
                    {
                        await manager.UpdateApp();

                        MessageBox.Show("Updated succesfuly!");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString() + "\n\nUpdate checking process error, please contact IT support", "UPO$$");
            }
        }
        #endregion
    }
}
