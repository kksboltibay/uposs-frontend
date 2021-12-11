using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Models;

namespace UPOSS.ViewModels
{
    class SettingViewModel : ViewModelBase
    {
        public SettingViewModel()
        {
            saveCommand = new RelayCommand(Save);

            InputSetting = new Setting();

            InputSetting.GovChargesName = Properties.Settings.Default.Setting_GovChargesName;
            InputSetting.GovChargesValue = Math.Round(Convert.ToDecimal(Properties.Settings.Default.Setting_GovChargesValue) * 100, 2).ToString();

            InputSetting.System_address = Properties.Settings.Default.Setting_SystemAddress;

            InputSetting.Phone_no = Properties.Settings.Default.Setting_SystemPhoneNo;
            
            InputSetting.GovChargesNo = Properties.Settings.Default.Setting_GovChargesNo;
        }

        #region Define
        private Setting inputSetting;
        public Setting InputSetting
        {
            get { return inputSetting; }
            set { inputSetting = value; OnPropertyChanged("InputSetting"); }
        }
        #endregion

        #region SaveOperation
        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get { return saveCommand; }
        }
        private void Save()
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(InputSetting.GovChargesName) || string.IsNullOrWhiteSpace(InputSetting.GovChargesValue) || 
                //    string.IsNullOrWhiteSpace(InputSetting.System_address) || string.IsNullOrWhiteSpace(InputSetting.Phone_no))
                //{
                //    // check if empty
                //    MessageBox.Show("Error: empty column detected, please try again.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                
                if (Regex.IsMatch(InputSetting.GovChargesValue, "[^0-9.]+"))
                {
                    // check GovChargesValue
                    // check only positive decimal allow
                    MessageBox.Show("The value for " + InputSetting.GovChargesName + " should only be positive number with decimal.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (InputSetting.System_address.Length > 200)
                {
                    // check address length
                    MessageBox.Show("The length of the address is too long.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (InputSetting.Phone_no.Length > 40)
                {
                    // check address length
                    MessageBox.Show("The length of the Phone is too long.", "UPO$$", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    // save changes
                    var msgBoxResult = MessageBox.Show("Are you sure to save all the changes?", "UPO$$", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        Properties.Settings.Default.Setting_GovChargesName = InputSetting.GovChargesName;
                        Properties.Settings.Default.Setting_GovChargesValue = (Math.Round(Convert.ToDecimal(InputSetting.GovChargesValue), 2) / 100).ToString();
                        Properties.Settings.Default.Setting_GovChargesNo = InputSetting.GovChargesNo;
                        Properties.Settings.Default.Setting_SystemAddress = InputSetting.System_address;
                        Properties.Settings.Default.Setting_SystemPhoneNo = InputSetting.Phone_no;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Changes have been saved successfully.", "UPO$$", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
        #endregion
    }
}
