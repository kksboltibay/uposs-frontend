using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    public class RootAnalyticsObject
    {
        public string Status { get; set; }

        public string Msg { get; set; }

        public List<Analytics> Data { get; set; }

        public int Total { get; set; }
    }

    public class Analytics : ObservableObject
    {
        public int Id { get; set; }

        public Datetime Filter_created_at { get; set; }

        public Datetime Filter_updated_at { get; set; }

        public string Created_at { get; set; }

        public string Updated_at { get; set; }

        public string Receipt_no { get; set; }

        public string Total_discount { get; set; } = "0.00";

        public string Total_tax { get; set; } = "0.00";

        public string Total_amount { get; set; } = "0.00";

        public string Total_item { get; set; } = "0.00";

        public string Branch { get; set; }

        public string Cashier_username { get; set; }

        public string Payment_method { get; set; }

        public string Total_paid_amount { get; set; } = "0.00";

        public string Card_no { get; set; } = "-";

        public string Bank_name { get; set; } = "-";

        public string Card_type { get; set; } = "-";

        public string Change { get; set; } = "0.00";

        public string Status { get; set; } = "-";

        public List<Product> ProductList { get; set; }

        // print sales report
        public string Total_void_qty { get; set; } = "0.00";

        public string Total_void_amount { get; set; } = "0.00";

        public string Total_cash_sales { get; set; } = "0.00";

        public string Total_card_sales { get; set; } = "0.00";

        public string Total_cash_sales_qty { get; set; } = "0.00";

        public string Total_card_sales_qty { get; set; } = "0.00";
    }

    public class Datetime : ObservableObject
    {
        public string From { get; set; } = "";

        public string To { get; set; } = "";
    }
}
