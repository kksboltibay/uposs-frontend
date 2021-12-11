using BarcodeLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Size = System.Windows.Size;

namespace UPOSS.Controls.Dialog
{
    /// <summary>
    /// Interaction logic for ProductPrintBarcodeDialog.xaml
    /// </summary>
    public partial class ProductPrintBarcodeDialog : Window
    {
        public ProductPrintBarcodeDialog(string barcodeString)
        {
            InitializeComponent();

            GenerateBarcode(barcodeString);

            Print();
        }

        private void GenerateBarcode(string bString)
        {
            Barcode b = new Barcode();
            b.IncludeLabel = true;
            System.Drawing.Image img = b.Encode(TYPE.CODE128, bString, System.Drawing.Color.Black, System.Drawing.Color.White, 150, 80);
            BitmapImage bimg = new BitmapImage();

            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Position = 0;
                bimg.BeginInit();
                bimg.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bimg.CacheOption = BitmapCacheOption.OnLoad;
                bimg.UriSource = null;
                bimg.StreamSource = ms;
                bimg.EndInit();

                imgBc.Source = bimg;
            }
        }

        private void Print()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(printGrid, "");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "UPO$$");
            }
        }
    }
}
