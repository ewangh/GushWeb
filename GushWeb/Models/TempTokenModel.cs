using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace GushWeb.Models
{
    public class TempToken
    {
        [DisplayName("Token")]
        [Required(ErrorMessage = "{0} can not be empty")]
        [Description("{0}作为临时Token")]
        public string Token { get; set; }

        [DisplayName("IsUsed")]
        public bool IsUsed { get; set; }

        [DisplayName("邮箱地址")]
        [Required(ErrorMessage = "请输入 {0}")]
        [Description("{0}接收Token")]
        [MaxLength(250, ErrorMessage = "{0}超过长度{1}位")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DisplayName("过期时间")]
        //[DataType(DataType.DateTime)]
        public DateTime ExpireDate { get; set; }
    }
}