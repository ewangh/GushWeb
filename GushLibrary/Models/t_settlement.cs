using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GushLibrary.Models
{
    public partial class t_settlement
    {
        public t_settlement()
        { }
        #region Model
        private Guid _id;
        private string _code;
        private string _name;
        private decimal? _opening;
        private decimal? _closed;
        private decimal? _highest;
        private decimal? _lower;
        private decimal? _price;
        private long? _volume;
        private decimal? _bprice;
        private decimal? _cprice;
        private string _date;
        private string _time;
        /// <summary>
        /// 
        /// </summary>
        public Guid Id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Code
        {
            set { _code = value; }
            get { return _code; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("* cPrice greater or equal to bPrice")]
        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 开盘价
        /// </summary>
        public decimal? Opening
        {
            set { _opening = value; }
            get { return _opening; }
        }
        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal? Closed
        {
            set { _closed = value; }
            get { return _closed; }
        }
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal? Highest
        {
            set { _highest = value; }
            get { return _highest; }
        }
        /// <summary>
        /// 最低价
        /// </summary>
        public decimal? Lower
        {
            set { _lower = value; }
            get { return _lower; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("前一交易日涨幅")]
        [DisplayName("LastChange")]
        public decimal? Price
        {
            set { _price = value; }
            get { return _price; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long? Volume
        {
            set { _volume = value; }
            get { return _volume; }
        }
        /// <summary>
        /// 
        /// </summary>
        public decimal? bPrice
        {
            set { _bprice = value; }
            get { return _bprice; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("阻力位")]
        [DisplayName("Resistance")]
        public decimal? cPrice
        {
            set { _cprice = value; }
            get { return _cprice; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("前一交易日期")]
        [DisplayName("LastDate")]
        public string Date
        {
            set { _date = value; }
            get { return _date; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Time
        {
            set { _time = value; }
            get { return _time; }
        }
        #endregion Model

    }
}
