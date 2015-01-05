using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Poc.ServiceDataProvider.Models
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string JobTitle { get; set; }
    }
}