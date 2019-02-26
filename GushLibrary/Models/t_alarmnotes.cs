using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GushLibrary.Models
{
    public class t_alarmnotes
    {
        /// <summary>
        ///获取转换类
        /// </summary>
        public t_alarmnotes()
        { }

        #region Model
        private Guid _id;
        private string _code;
        private string _name;
        private decimal? _opening;
        private decimal? _closed;
        private decimal? _price;
        private decimal? _highest;
        private decimal? _lower;
        private string _date;
        private string _time;
        private decimal? _closing;
        private double? _amplitude;
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
        /// 当前价
        /// </summary>
        [DisplayName("Change")]
        public decimal? Price
        {
            set { _price = value; }
            get { return _price; }
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
        /// 收盘价
        /// </summary>
        public decimal? Closing
        {
            set { _closing = value; }
            get { return _closing; }
        }
        
        [NotMapped]
        /// <summary>
        /// 涨跌幅
        /// </summary>
        public double? Amplitude
        {
            set { _amplitude = value; }
            get { return _amplitude; }
        }
        #endregion Model
    }
}
