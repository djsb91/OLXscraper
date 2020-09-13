using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OLXscraper.Models
{
    public class Product
    {
      

        public string Title { get; set; }
        public string Price { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }
        
    }
}

//Convert.ToDecimal(Price.Replace("ZŁ", "").Trim());