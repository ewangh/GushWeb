using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class WebDBContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<OwnModel> Owns { get; set; }

        //public System.Data.Entity.DbSet<GushLibrary.Models.t_alarmnotes> t_alarmnotes { get; set; }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException exception)
            {
                var errorMessages =
                    exception.EntityValidationErrors
                        .SelectMany(validationResult => validationResult.ValidationErrors)
                        .Select(m => m.ErrorMessage);

                var fullErrorMessage = string.Join(", ", errorMessages);
                //记录日志
                //Log.Error(fullErrorMessage);
                var exceptionMessage = string.Concat(exception.Message, " 验证异常消息是：", fullErrorMessage);

                throw new DbEntityValidationException(exceptionMessage, exception.EntityValidationErrors);
            }

            //其他异常throw到上层
        }
    }
}