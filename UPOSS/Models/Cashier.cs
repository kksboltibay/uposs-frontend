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

        public Cashier Data { get; set; }
    }

    public class Cashier : ObservableObject
    {
        public List<Product> CartList { get; set; }

        public string Total_item { get; set; }

        public string Subtotal { get; set; } = "0.00";

        public string Discount { get; set; } = "0.00";

        public string Tax { get; set; } = "0.00";

        public string Total_amount { get; set; } = "0.00";

        public string Payment_method { get; set; }

        public string Cash_pay { get; set; } = "0.00";

        public string Card_no { get; set; }

        public string Card_pay { get; set; } = "0.00";

        public string Bank_name { get; set; }

        public string Card_type { get; set; }

        public string Change { get; set; } = "0.00";

        //from api
        public string Receipt_no { get; set; }

        public string Datetime { get; set; }

    }
}
