using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GushLibrary.Models
{
    public partial class t_errornotes
    {
        public t_errornotes()
        { }

        #region Model
        private Guid _id;
        private string _code;
        private string _name;
        private decimal? _price;
        private long? _volume;
        private string _date;
        private string _time;
        private string _sign;
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Guid Id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 代码
        /// </summary>
        public string Code
        {
            set { _code = value; }
            get { return _code; }
        }
        /// <summary>
        /// 名
        /// </summary>
        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 当前价
        /// </summary>
        public decimal? Price
        {
            set { _price = value; }
            get { return _price; }
        }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date
        {
            set { _date = value; }
            get { return _date; }
        }
        /// <summary>
        /// 时间
        /// </summary>
        public string Time
        {
            set { _time = value; }
            get { return _time; }
        }
        /// <summary>
        /// 错误标志
        /// </summary>
        public string Sign
        {
            set { _sign = value; }
            get { return _sign; }
        }
        #endregion Model
        /// <summary>
        /// 成交量
        /// </summary>
        public long? Volume
        {
            set { _volume = value; }
            get { return _volume; }
        }
    }
}
