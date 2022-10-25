using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace Walkenscraper
{
    public class Attribute
    {
        public string trait_type { get; set; }
        public object value { get; set; }
    }

    public class Collection
    {
        public string family { get; set; }
        public string name { get; set; }
    }

    public class Creator
    {
        public string address { get; set; }
        public int share { get; set; }
    }

    public class File
    {
        public string uri { get; set; }
        public string type { get; set; }
        public bool cdn { get; set; }
    }

    public class Properties
    {
        public List<File> files { get; set; }
        public string category { get; set; }
        public List<Creator> creators { get; set; }
        public string id { get; set; }
    }

    public class WLKNC
    {
        public string name { get; set; }
        public string description { get; set; }
        public string symbol { get; set; }
        public string image { get; set; }
        public string external_url { get; set; }
        public List<Attribute> attributes { get; set; }
        public Collection collection { get; set; }
        public Properties properties { get; set; }
        public int seller_fee_basis_points { get; set; }
    }    
}
