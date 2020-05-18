using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MlaWebApi.Models
{
    public class Register
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
         public string publickey { get; set; }
        public string groupkey { get; set; }

    }
}