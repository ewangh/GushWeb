using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class Rise
    {
        public Rise(string text, string ptype, decimal? change, int index = 0)
        {
            Text = text;
            Ptype = ptype;
            Change = change;
            Index = index;
        }

        public int Index { get; set; }
        public string Text { get; set; }
        public string Ptype { get; set; }
        public decimal? Change { get; set; }
        public bool? IsCheck { get; set; }
    }
}