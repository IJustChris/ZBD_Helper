using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBDHelper.Models
{
    public class AlbumResponseModel
    {
        public int _id { get; set; }
        public string tytul { get; set; }
        public int arists_id { get; set; }
        public int rok { get; set; }
        public string gatunek { get; set; }
        public string nosnik { get; set; } = "CD";
    }

}
