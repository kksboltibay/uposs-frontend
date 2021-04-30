using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    public class Pagination : ObservableObject
    {
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public string CurrentRecord { get; set; }
        public int TotalRecord { get; set; }
    }
}
