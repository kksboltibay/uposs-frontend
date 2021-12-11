using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    class Setting : ObservableObject
    {
        public string GovChargesName { get; set; }

        public string GovChargesValue { get; set; }

        public string GovChargesNo { get; set; }

        public string System_address { get; set; }

        public string Phone_no { get; set; }
    }
}
