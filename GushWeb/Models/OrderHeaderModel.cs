using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class OrderHeader
    {
        [Key]
        public Guid Id { get; set; }
        public virtual ICollection<OrderDetail> OrderDetailItems { get; set; }

        public virtual Member Member { get; set; }
    }
}