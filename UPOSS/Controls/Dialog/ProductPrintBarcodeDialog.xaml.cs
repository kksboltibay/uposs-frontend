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
using ZXing;
using ZXing.Common;
using UPOSS.Models;
using System.Linq;

namespace UPOSS.Controls.Dialog
{
    /// <summary>
    /// Interaction logic for ProductPrintBarcodeDialog.xaml
    /// </summary>
    public partial class ProductPrintBarcodeDialog : Window
    {
        public ProductPrintBarcodeDialog(Product _product)
        {
            InitializeComponent();

            //GenerateBarcode(barcodeString);
            //Print();

            GenerateAndPrintBarcode(_product);
        }

        private void GenerateBarcode(string bString)
        {
            Barcode b = new Barcode();
            b.IncludeLabel = true;
            System.Drawing.Image img = b.Encode(TYPE.CODE128, bString, System.Drawing.Color.Black, System.Drawing.Color.White, 200, 90);
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

        public static void GenerateAndPrintBarcode(Product product)
        {
            // define
            string barcode = product.Barcode;
            int width = 100;
            int height = 100;
            string textBelowBarcode = product.Barcode + "\n" + product.Name + "\nRM" + product.Price;
            string fontName = "Aerial";
            float fontSize = 10;

            // Create barcode writer and set encoding options
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 0,
                    PureBarcode = true
                }
            };

            // Generate barcode image
            var barcodeBitmap = writer.Write(barcode);

            // Create graphics object
            using (var graphics = System.Drawing.Graphics.FromImage(barcodeBitmap))
            {
                // Define font and brush for text
                var font = new System.Drawing.Font(fontName, fontSize);
                var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

                // Define string format for text
                var stringFormat = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center
                };

                // Calculate text size
                var textSize = graphics.MeasureString(textBelowBarcode, font, barcodeBitmap.Width, stringFormat);

                // Draw text
                graphics.DrawString(textBelowBarcode, font, brush, new System.Drawing.RectangleF(0, barcodeBitmap.Height, barcodeBitmap.Width, textSize.Height), stringFormat);
            }

            // Print barcode image
            var printDocument = new System.Drawing.Printing.PrintDocument();
            printDocument.PrintPage += (s, e) =>
            {
                // Calculate position of barcode on page
                var x = (e.PageBounds.Width - barcodeBitmap.Width) / 2;
                var y = (e.PageBounds.Height - barcodeBitmap.Height - (int)fontSize * 2) / 2;

                // Define font and brush for text
                var font = new System.Drawing.Font(fontName, fontSize);
                var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

                // Define string format for text
                var stringFormat = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center
                };

                // Calculate text size
                var textSize = e.Graphics.MeasureString(textBelowBarcode, font, e.PageBounds.Width, stringFormat);

                // Calculate text position
                var textX = e.PageBounds.Left + (e.PageBounds.Width - textSize.Width) / 2;
                var textY = y + barcodeBitmap.Height + (int)fontSize;

                // Draw barcode and text at calculated position
                e.Graphics.DrawImage(barcodeBitmap, x, y);
                e.Graphics.DrawString(textBelowBarcode, font, brush, new System.Drawing.RectangleF(e.PageBounds.Left, textY, e.PageBounds.Width, textSize.Height), stringFormat);
            };

            //printDocument.Print(); //direct print without choosing printer

            // Show print dialog and print if user clicks "Print"
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDocument.Print();
            }
        }
    }
}
