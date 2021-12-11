using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    public class RootAnalyticsObject
    {
        public string Status { get; set; }

        public string Msg { get; set; }

        public Analytics Data { get; set; }

        public int Total { get; set; }
    }

    public class Analytics : ObservableObject
    {
        // Sales Report Tab
        public Datetime Created_at { get; set; }

        public Datetime Updated_at { get; set; }

        public string Receipt_no { get; set; }

        public string Total_discount { get; set; } = "0.00";

        public string Total_tax { get; set; } = "0.00";

        public string Total_amount { get; set; } = "0.00";

        public string Total_item { get; set; } = "0.00";

        public string Branch { get; set; }

        public string Payment_method { get; set; }

        public string Cash_pay { get; set; } = "0.00";

        public string Card_no { get; set; } = "-";

        public string Card_pay { get; set; } = "0.00";

        public string Bank_name { get; set; } = "-";

        public string Card_type { get; set; } = "-";

        public string Change { get; set; } = "0.00";

        public List<Product> ProductList { get; set; }

        
        // Purchase Report Tab
    }

    public class Datetime : ObservableObject
    {
        public string From { get; set; } = "";

        public string To { get; set; } = "";
    }
}
