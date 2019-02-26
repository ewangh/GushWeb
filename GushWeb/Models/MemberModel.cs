using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.Web.Mvc.CompareAttribute;

namespace GushWeb.Models
{
    [DisplayName("会员信息")]
    [DisplayColumn("Name")]
    public class Member
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("用户名")]
        [Required(ErrorMessage = "{0}必填")]
        [Description("{0}说明")]
        [MaxLength(50, ErrorMessage = "{0}超过长度{1}位")]
        public string Name { get; set; }


        [DisplayName("昵称")]
        [Required(ErrorMessage = "{0}必填")]
        [Description("{0}说明")]
        [MaxLength(50, ErrorMessage = "{0}超过长度{1}位")]
        public string Nickname { get; set; }

        [DisplayName("邮箱")]
        [Required(ErrorMessage = "{0}必填")]
        [Description("{0}作为登陆账号")]
        [MaxLength(250, ErrorMessage = "{0}超过长度{1}位")]
        [DataType(DataType.EmailAddress)]
        [Remote("CheckDup", "Member",HttpMethod ="POST", ErrorMessage = "邮箱已被注册")]
        public string Email { get; set; }

        [DisplayName("密码")]
        [Required(ErrorMessage = "{0}必填")]
        [Description("{0}说明")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "{0}长度大于{2}位小于{1}位")]
        [RegularExpression(@"^\w+$", ErrorMessage = "密码格式有误,只能是字母数字或者下划线")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped]
        [DisplayName("确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        [DataType(DataType.Password)]
        public virtual string ConfirmPassword { get; set; }

        [DisplayName("注册时间")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime RegisterOn { get; set; }

        [DisplayName("会员启用认证码")]
        [Description("Null则为已经通过Email验证")]
        [MaxLength(36, ErrorMessage = "超过长度{1}位")]
        public string AuthCode { get; set; }

        public virtual ICollection<OrderHeader> Orders { get; set; }
    }
}