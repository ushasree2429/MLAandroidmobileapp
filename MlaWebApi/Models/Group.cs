using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MlaWebApi.EFModels;
using System.Configuration;
//using System.Data.SqlServerCe;
using System.Data;
using System.Data.SqlClient;
namespace MlaWebApi.Models
{
    public class Group
    {
        public string groupName { get; set; }
        public int userId { get; set; }
        public string groupKey { get; set; }

        public List<long> friend_ids { get; set; }

        public List<string> group_keys { get; set; }

    }
}
