using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    public class RootBranchObject
    {
        public string Status { get; set; }

        public string Msg { get; set; }

        public List<Branch> Data { get; set; }

        public int Total { get; set; }
    }

    public class Branch : ObservableObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Is_active { get; set; }
    }
}
