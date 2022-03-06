using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for ProductInputDialog.xaml
    /// </summary>
    public partial class ProductInputDialog : Window
    {
        public ProductInputDialog(string question, string mode = "", Product product = null, ProductQuantity quantity = null)
        {
            InitializeComponent();
            ProductResult = new Product();
            QuantityResult = new ProductQuantity();

            tbkQuestion.Text = question;

            if (mode == "add")
            {
                tbkProductNo.Visibility = Visibility.Collapsed;
                tbProductNo.Visibility = Visibility.Collapsed;
                
                tbkBarcode.Visibility = Visibility.Collapsed;
                tbBarcode.Visibility = Visibility.Collapsed;

                tbkBranch.Visibility = Visibility.Collapsed;
                tbBranch.Visibility = Visibility.Collapsed;

                tbkQuantity.Visibility = Visibility.Collapsed;
                tbQuantity.Visibility = Visibility.Collapsed;

                tbkNote.Visibility = Visibility.Collapsed;

                tbName.Text = "Sample_product_name";
                tbCategory.Text = "Sample_category";
                tbDesignCode.Text = "Sample_design_code";
                tbColourCode.Text = "Sample_colour_code";
                tbPrice.Text = "1.00";
            }
            else if (mode == "update")
            {
                tbProductNo.Focusable = false;
                tbProductNo.Text = product.Product_no;

                tbName.Text = product.Name;
                tbCategory.Text = product.Category;
                tbDesignCode.Text = product.Design_code;
                tbColourCode.Text = product.Colour_code;
                tbPrice.Text = product.Price;

                tbBarcode.Focusable = false;
                tbBarcode.Text = product.Barcode;

                if (quantity != null)
                {
                    if (quantity.Branch_name != null)
                    {
                        tbBranch.Focusable = false;
                        tbBranch.Text = quantity.Branch_name;
                        tbQuantity.Text = quantity.Quantity;

                        tbkNote.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //hide branch & quantity column
                        tbkBranch.Visibility = Visibility.Collapsed;
                        tbBranch.Visibility = Visibility.Collapsed;

                        tbkQuantity.Visibility = Visibility.Collapsed;
                        tbQuantity.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    //hide branch & quantity column
                    tbkBranch.Visibility = Visibility.Collapsed;
                    tbBranch.Visibility = Visibility.Collapsed;

                    tbkQuantity.Visibility = Visibility.Collapsed;
                    tbQuantity.Visibility = Visibility.Collapsed;
                }
            }
            else if (mode == "activate")
            {
                tbProductNo.Text = product.Product_no;
                tbProductNo.Focusable = false;

                tbName.Text = product.Name;
                tbName.Focusable = false;
                tbCategory.Text = product.Category;
                tbCategory.Focusable = false;
                tbDesignCode.Text = product.Design_code;
                tbDesignCode.Focusable = false;
                tbColourCode.Text = product.Colour_code;
                tbColourCode.Focusable = false;
                tbPrice.Text = product.Price;
                tbPrice.Focusable = false;
                tbBarcode.Text = product.Barcode;
                tbBarcode.Focusable = false;

                //hide branch & quantity column
                tbkBranch.Visibility = Visibility.Collapsed;
                tbBranch.Visibility = Visibility.Collapsed;

                tbkQuantity.Visibility = Visibility.Collapsed;
                tbQuantity.Visibility = Visibility.Collapsed;

                tbkNote.Visibility = Visibility.Collapsed;
            }
            else if (mode == "deactivate")
            {
                tbProductNo.Text = product.Product_no;
                tbProductNo.Focusable = false;

                tbName.Text = product.Name;
                tbName.Focusable = false;
                tbCategory.Text = product.Category;
                tbCategory.Focusable = false;
                tbDesignCode.Text = product.Design_code;
                tbDesignCode.Focusable = false;
                tbColourCode.Text = product.Colour_code;
                tbColourCode.Focusable = false;
                tbPrice.Text = product.Price;
                tbPrice.Focusable = false;
                tbBarcode.Text = product.Barcode;
                tbBarcode.Focusable = false;

                //hide branch & quantity column
                tbkBranch.Visibility = Visibility.Collapsed;
                tbBranch.Visibility = Visibility.Collapsed;

                tbkQuantity.Visibility = Visibility.Collapsed;
                tbQuantity.Visibility = Visibility.Collapsed;

                tbkNote.Visibility = Visibility.Collapsed;
            }
        }

        #region Define
        private Product productResult;
        public Product ProductResult { get { return productResult; } set { productResult = value; } }

        private ProductQuantity quantityResult;
        public ProductQuantity QuantityResult { get { return quantityResult; } set { quantityResult = value; } }
        #endregion

        #region Custom
        //private void GetRoleList()
        //{
        //    string[] roleList = { "Admin", "Staff" };

        //    cbRoleList.ItemsSource = roleList;
        //}

        //private async void GetActiveBranchList()
        //{
        //    try
        //    {
        //        dynamic param = new { page = 0 };

        //        RootBranchObject Response = await ObjBranchService.BranchPostAPI("getBranchList", param);

        //        if (Response.Status == "ok")
        //        {
        //            cbBranchList.ItemsSource = (Response.Data.OrderBy(property => property.Name).Select(item => item.Name));
        //        }
        //        else
        //        {
        //            cbBranchList.Items.Add("- Fail to get branch list -");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        cbBranchList.Items.Add("- Fail to get branch list -");
        //    }
        //}
        #endregion


        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (tbPrice.Text == null && tbPrice.Text == "")
            {
                MessageBox.Show("Price cannot be empty", "UPO$$");
            }
            else
            {
                try
                {
                    ProductResult = new Product
                    {
                        Product_no = tbProductNo.Text,
                        Name = tbName.Text,
                        Category = tbCategory.Text,
                        Design_code = tbDesignCode.Text,
                        Colour_code = tbColourCode.Text,
                        Price = string.IsNullOrWhiteSpace(tbPrice.Text) ? "0.00" : Math.Round(Convert.ToDecimal(tbPrice.Text), 2, MidpointRounding.AwayFromZero).ToString("0.00")
                    };

                    QuantityResult = new ProductQuantity
                    {
                        Branch_name = tbBranch.Text,
                        Quantity = string.IsNullOrWhiteSpace(tbQuantity.Text) ? "0.00" : Math.Round(Convert.ToDecimal(tbQuantity.Text), 2, MidpointRounding.AwayFromZero).ToString("0.00")
                    };

                    this.DialogResult = true;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message.ToString(), "UPO$$");
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            tbName.SelectAll();
            tbName.Focus();
        }

        private void tbQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);

            // check decimal
                
        }
    }
}
