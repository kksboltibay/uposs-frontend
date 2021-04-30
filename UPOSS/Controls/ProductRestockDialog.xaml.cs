using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ProductRestockDialog.xaml
    /// </summary>
    public partial class ProductRestockDialog : Window
    {
        public ProductRestockDialog(string question, string mode = "", ObservableCollection<ProductQuantity> quantityList = null)
        {
            InitializeComponent();
            ProductResult = new ObservableCollection<ProductQuantity>();

            tbkQuestion.Text = question;
            tbPrice.Text = "0";

            if (mode == "restock")
            {
                ProductQuantityList = quantityList;

                //System.Diagnostics.Trace.WriteLine("here");
                ////System.Diagnostics.Trace.WriteLine(quantityList);
                //foreach (var item in quantityList)
                //{
                //    System.Diagnostics.Trace.WriteLine(item.Branch_name);
                //    System.Diagnostics.Trace.WriteLine(item.Quantity);
                //}

                //Generate labels and text boxes
                for (int i = 0; i < quantityList.Count; i++)
                {
                    // Create a Content WrapPanel
                    WrapPanel contentWrapPanel = new WrapPanel();
                    contentWrapPanel.Margin = new Thickness(0, 5, 0, 10);
                    contentWrapPanel.Orientation = Orientation.Horizontal;
                    contentWrapPanel.VerticalAlignment = VerticalAlignment.Center;

                    //Branch name Section
                    // Create Branch name WrapPanel
                    WrapPanel branchNameWrapPanel = new WrapPanel();
                    branchNameWrapPanel.Margin = new Thickness(5, 0, 5, 0);
                    branchNameWrapPanel.Orientation = Orientation.Horizontal;

                    //Create label Textblock
                    TextBlock labelTextblock = new TextBlock();
                    labelTextblock.Text = "Branch : ";
                    labelTextblock.FontSize = 16;
                    labelTextblock.FontWeight = FontWeights.Bold;
                    labelTextblock.VerticalAlignment = VerticalAlignment.Center;

                    //Create Branch name Textblock
                    TextBlock branchNameTextblock = new TextBlock();
                    branchNameTextblock.Text = quantityList[i].Branch_name;
                    branchNameTextblock.FontSize = 16;
                    branchNameTextblock.FontWeight = FontWeights.Bold;
                    branchNameTextblock.VerticalAlignment = VerticalAlignment.Center;
                    branchNameTextblock.Margin = new Thickness(5, 0, 5, 0);
                    branchNameTextblock.FontSize = 15;
                    branchNameTextblock.MinWidth = 150;

                    // Put into wrappanel
                    branchNameWrapPanel.Children.Add(labelTextblock);
                    branchNameWrapPanel.Children.Add(branchNameTextblock);
                    contentWrapPanel.Children.Add(branchNameWrapPanel);

                    //Quantity Section
                    // Create Quantity WrapPanel
                    WrapPanel quantityWrapPanel = new WrapPanel();
                    //quantityWrapPanel.Margin = new Thickness(5, 0, 5, 0);
                    quantityWrapPanel.Orientation = Orientation.Horizontal;

                    //Create label Textblock
                    TextBlock labelTitleTextblock = new TextBlock();
                    labelTitleTextblock.Text = "Quantity : ";
                    labelTitleTextblock.FontSize = 16;
                    labelTitleTextblock.FontWeight = FontWeights.Bold;
                    labelTitleTextblock.VerticalAlignment = VerticalAlignment.Center;

                    //Create label old quantity Textblock
                    TextBlock labelQuantityTextblock = new TextBlock();
                    labelQuantityTextblock.Text = quantityList[i].Quantity;
                    labelQuantityTextblock.FontSize = 16;
                    labelQuantityTextblock.FontWeight = FontWeights.Bold;
                    labelQuantityTextblock.VerticalAlignment = VerticalAlignment.Center;

                    //Create label symbol Textblock
                    TextBlock labelsymbolTextblock = new TextBlock();
                    labelsymbolTextblock.Text = " + ";
                    labelsymbolTextblock.FontSize = 16;
                    labelsymbolTextblock.FontWeight = FontWeights.Bold;
                    labelsymbolTextblock.VerticalAlignment = VerticalAlignment.Center;

                    //Create Quantity TextBox
                    TextBox quantityTextbox = new TextBox();
                    quantityTextbox.Name = "quantity_" + i;
                    quantityTextbox.Text = "0";
                    quantityTextbox.FontSize = 16;
                    quantityTextbox.FontWeight = FontWeights.Bold;
                    quantityTextbox.VerticalAlignment = VerticalAlignment.Center;
                    //quantityTextbox.Margin = new Thickness(10, 0, 10, 0);
                    quantityTextbox.FontSize = 15;
                    quantityTextbox.MinWidth = 80;

                    textboxControls.Add(quantityTextbox);

                    // Put into wrappanel
                    quantityWrapPanel.Children.Add(labelTitleTextblock);
                    quantityWrapPanel.Children.Add(labelQuantityTextblock);
                    quantityWrapPanel.Children.Add(labelsymbolTextblock);
                    quantityWrapPanel.Children.Add(quantityTextbox);
                    contentWrapPanel.Children.Add(quantityWrapPanel);

                    // Put into stackpanel
                    spContent.Children.Add(contentWrapPanel);
                }
            }
        }

        #region Define
        private ObservableCollection<ProductQuantity> productQuantityList;
        public ObservableCollection<ProductQuantity> ProductQuantityList { get { return productQuantityList; } set { productQuantityList = value; } }

        private ObservableCollection<ProductQuantity> productResult;
        public ObservableCollection<ProductQuantity> ProductResult { get { return productResult; } set { productResult = value; } }

        private string price;
        public string Price { get { return price; } set { price = value; } }

        List<TextBox> textboxControls = new List<TextBox>();
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
            for (var i = 0; i < ProductQuantityList.Count; i++)
            {
                ProductResult.Add(new ProductQuantity
                {
                    Branch_name = ProductQuantityList[i].Branch_name,
                    Quantity = textboxControls[i].Text == ""? "0" : textboxControls[i].Text
                });
            }

            Price = tbPrice.Text == "" ? "0" : tbPrice.Text;

            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            tbPrice.SelectAll();
            tbPrice.Focus();
        }
    }
}
