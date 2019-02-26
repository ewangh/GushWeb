using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GushWeb.Models
{
    [Bind(Include="code,name,sex")]
    public class OwnModel
    {
        [Key]
        public Guid Id { get; set; }
        public int code { get; set; }
        public string name { get; set; }
        public int sex { get; set; }
    }
}