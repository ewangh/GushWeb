using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class Rise
    {
        public string Text { get; set; }
        public string Ptype { get; set; }
        public decimal? Change { get; set; }
        public bool? IsCheck { get; set; }
    }
}