using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using ServiceStack.Common.Extensions;

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
            Stocks = new List<Stock>();
        }

        public int Index { get; set; }
        public string Text { get; set; }
        public string Ptype { get; set; }
        public decimal? Change { get; set; }
        public bool? IsCheck { get; set; }
        public int Length { get; set; }
        public string Remark => String.Join(",", Stocks.ConvertAll(d => d.NumTotal));
        public decimal? MaxChange => Stocks.Max(d => d.Change);
        public decimal? TotalChange => Stocks.OrderBy(d => d.NumTotal).FirstOrDefault()?.Change;
        public IEnumerable<Stock> Stocks { get; set; }
    }

    public class Stock
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [ScriptIgnore]
        public decimal? Total { get; set; }
        public int? NumTotal { get; set; }
        [ScriptIgnore]
        public decimal? Ltotal { get; set; }
        [ScriptIgnore]
        public int? NumLtotal { get; set; }
        public decimal? Change { get; set; }
        public string Date { get; set; }
    }
}