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
    public class t_delta
    {
        public t_delta()
        {

        }

        public string Code { get; set; }

        public string Name { get; set; }
        [DisplayName("累计涨幅")]
        public decimal? Delta { get; set; }
        [DisplayName("涨幅(%)")]
        public decimal? Change { get; set; }
        [DisplayName("排名")]
        public int? Num { get; set; }
        [DisplayName("开始日期")]
        public string BeginDate { get; set; }
        [DisplayName("结束日期")]
        public string EndDate { get; set; }
    }
}
