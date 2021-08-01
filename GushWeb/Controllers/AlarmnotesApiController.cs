using GushLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GushWeb.Utility;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Entity;
using Newtonsoft.Json.Linq;

namespace GushWeb.Controllers
{
    public class AlarmnotesApiController : BaseApiController
    {
        private GushDBContext db = new GushDBContext();

        private Expression<Func<t_alarmnotes, bool>> getExpression(string date, string[] array = null)
        {
            Expression<Func<t_alarmnotes, bool>> expression = t => true;
            expression = expression.And(d => d.Date.CompareTo(date) == 0);
            //expression = expression.And(d => !d.Name.ToLower().Contains(Pre.pre_st) || !d.Name.ToLower().Contains(Pre.pre_xst));
            //expression = expression.And(d => d.Price < d.Closed * 1.097m);
            //expression = expression.And(d => d.Time.CompareTo("09:32:03") < 0);
            if (!array.IsNullOrEmpty())
            {
                expression = expression.And(p => !array.Contains(p.Code));
            }

            return expression;
        }

        [HttpGet,HttpPost]
        [Route("getalarm")]
        public async Task<IHttpActionResult> GetAlarm([FromBody]JObject data)
        {
            try
            {
                var date = data["date"].ToObject<DateTime>();
                var expression = getExpression(date.ToYYYYMMDD());
                var pageData = await db.AlarmNotesList.Where(expression).OrderBy(d => d.Time).ToListAsync();

                return Json(new { ret = 1, data = pageData });
            }
            catch (Exception e)
            {
                return Json(new { ret = 0, data = new List<t_alarmnotes>(), msg = e.Message });
            }
        }
    }
}
