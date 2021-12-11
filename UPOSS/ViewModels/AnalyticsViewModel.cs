using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UPOSS.Commands;
using UPOSS.Models;
using UPOSS.Services;

namespace UPOSS.ViewModels
{
    class AnalyticsViewModel : ViewModelBase
    {
        APIService ObjAnalyticsService;
        private string _Path;

        public AnalyticsViewModel()
        {
            ObjAnalyticsService = new APIService();
            _Path = "analytics";


            // Sales Report
            //searchCommand = new AsyncRelayCommand(Search, this);

            InputSalesReport = new Analytics();
            InputSalesReport.Created_at = new Datetime();
            InputSalesReport.Updated_at = new Datetime();

            // default date = today
        }

        //Loding screen
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged("IsLoading"); }
        }

        #region Sale Report

        // Define
        private Analytics inputSalesReport;
        public Analytics InputSalesReport
        {
            get { return inputSalesReport; }
            set { inputSalesReport = value; OnPropertyChanged("InputSalesReport"); }
        }

        // Search Operation
        private AsyncRelayCommand searchCommand;
        public AsyncRelayCommand SearchCommand
        {
            get { return searchCommand; }
        }
        private async Task Search()
        {
            try
            {
                dynamic param = new
                {
                    createdAt = new 
                    {
                        from = InputSalesReport.Created_at.From != "" ? DateTime.ParseExact(InputSalesReport.Created_at.From, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSalesReport.Created_at.To != "" ? DateTime.ParseExact(InputSalesReport.Created_at.To, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    updatedAt = new
                    {
                        from = InputSalesReport.Updated_at.From != "" ? DateTime.ParseExact(InputSalesReport.Updated_at.From, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : "",
                        to = InputSalesReport.Updated_at.To != "" ? DateTime.ParseExact(InputSalesReport.Updated_at.To, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    },
                    page = 1
                };

                RootAnalyticsObject Response = await ObjAnalyticsService.PostAPI("getSalesList", param, _Path);

                if (Response.Status != "ok")
                {
                    IsLoading = false;
                    MessageBox.Show(Response.Msg, "UPO$$");
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
