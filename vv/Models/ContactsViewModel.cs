using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vv.Models
{
    public class ContactsViewModel
    {
        public int id { set; get; }
        public String Name { set; get; }
        public String Number { set; get; }
        public String Avatar { set; get; }
        public String LastSeen { set; get; }

        public bool inAppStatus { set; get; }
    }
}