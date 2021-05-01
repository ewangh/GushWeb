using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GushLibrary.Models;
using GushWeb.Utility;
using ServiceStack.Common.Extensions;

namespace GushWeb.Models
{
    public class t_foamState
    {
        private const int size = 30;
        public t_foamState(IEnumerable<t_settlement> list)
        {
            if (list == null)
            {
                _list = new List<t_settlement>();
            }
            else
            {
                _list = list.OrderByDescending(d => d.Date).Take(size);
            }

        }

        private IEnumerable<t_settlement> _list;

        public string Code => _list.FirstOrDefault()?.Code;

        public string Name => _list.FirstOrDefault()?.Name;
        [DisplayName("累积量(手)")]
        public long? Volume => _list.OrderBy(d => d.Volume).MinusAbs(d => d.Volume);
        [DisplayName("资金量(亿)")]
        public decimal? Funds => Volume * Price / 100;
        public decimal? Closed => _list.OrderByDescending(d => d.Date).FirstOrDefault()?.Closed;
        public decimal? Price => _list.OrderByDescending(d => d.Date).FirstOrDefault()?.Price;
        public string Date => _list.Max(d => d.Date);
    }
}
