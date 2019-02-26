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
    public partial class t_temporary
    {
        /// <summary>
        ///获取转换类
        /// </summary>
        public t_temporary()
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
        private long? _volume;
        private string _date;
        private string _time;
        private decimal? _closing;
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
        #endregion Model
        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public long? Volume
        {
            set { _volume = value; }
            get { return _volume; }
        }
        public t_errornotes toErrorNote(string sign)
        {
            return new t_errornotes()
            {
                Id = Guid.NewGuid(),
                Code = this.Code,
                Name = this.Name,
                Price = this.Price,
                Volume = this.Volume,
                Date = this.Date,
                Time = this.Time,
                Sign = sign,
            };
        }

        public t_temporary Clone()
        {
            //return new t_temporary()
            //{
            //    Id = this.Id,
            //    Code = this.Code,
            //    Name = this.Name,
            //    Opening = this.Opening,
            //    Closed = this.Closed,
            //    Price = this.Price,
            //    Highest = this.Highest,
            //    Lower = this.Lower,
            //    HighestVolume = this.HighestVolume,
            //    LowerVolume = this.LowerVolume,
            //    Date = this.Date,
            //    Time = this.Time,
            //    Volume=this.Volume,
            //    Closing = this.Closing,
            //};
            return this.MemberwiseClone() as t_temporary;
        }
    }
}
