using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class OrderDetail
    {
        [Key]
        public Guid Id { get; set; }
        public virtual OrderHeader OrderHeader { get; set; }
    }
}