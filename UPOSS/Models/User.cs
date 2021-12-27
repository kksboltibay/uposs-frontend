using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Models
{
    public class RootUserObject
    {
        public string Status { get; set; }

        public string Msg { get; set;}

        public List<User> Data { get; set; }

        public int Total { get; set; }
    }

    public class User : ObservableObject
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string UpdatedAt { get; set; }

        public string Role { get; set; }

        public string Branch_name { get; set; }

        public string Is_log_in { get; set; }
    }


}
