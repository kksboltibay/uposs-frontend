using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UPOSS.Controls
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : Window
    {
        public Action Worker { get; set; }
        public ProgressBar(Action worker)
        {
            InitializeComponent();

            Loaded += ProgressBar_Loaded;

            if (worker == null) throw new ArgumentNullException();

            Worker = worker;
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Worker).ContinueWith(t => { this.Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        //public void Hit()
        //{
        //    var progress = new Progress<int>((value) =>
        //    {
        //        progressBar.Value = value;
        //    });
        //}

        //public void LoopThroughNumbers(int count, IProgress<int> progress)
        //{
        //    for (int x = 0; x < count; x++)
        //    {
        //        Thread.Sleep(100);
        //        var percentageComplete = (x * 100) / count;
        //        progress.Report(percentageComplete);
        //    }
        //}

        //public void CloseWindow()
        //{
        //    Close();
        //}
    }
}
