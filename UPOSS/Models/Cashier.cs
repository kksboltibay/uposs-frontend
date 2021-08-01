using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace UPOSS.Models
{
    public class RootCashierObject
    {
        public string Status { get; set; }

        public string Msg { get; set; }

        //public Cashier Data { get; set; }
    }

    public class Cashier : ObservableObject
    {
        public List<Product> ProductList { get; set; }

        public string Barcode { get; set; }

        public string Total_item { get; set; }

        public string Subtotal { get; set; } = "0.0";

        public string Discount { get; set; } = "0.0";

        public string GST { get; set; } = "0.0";

        public string Total_amount { get; set; } = "0.0";

        public string Payment_method { get; set; }

        public string Card_no { get; set; }
    }
}
