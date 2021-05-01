using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using GushLibrary.Models;
using GushWeb.ActionFilters;
using GushWeb.Helpers;
using GushWeb.Models;
using GushWeb.Utility;
using PagedList;
using ServiceStack.Common.Extensions;

namespace GushWeb.Controllers
{
    [AlarmnotesActionFilters]
    public class TransactionController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        private GushProcContext proc = new GushProcContext();
        const string SZprefix = "sz";
        const string SHprefix = "sh";
        private const int pageSize = 300;

        [AllowAnonymous]
        public ActionResult Netbuy(string date, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (date.IsDateTime())
            {
                return NetbuyAsynByDate(date, col, odcol, mode, index, false, "");
            }
            return NetbuyAsynByCodeOrName(date, mode, index);
        }

        [AllowAnonymous]
        public ActionResult NetbuyPage(string date, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (date.IsDateTime())
            {
                return NetbuyAsynByDate(date, col, odcol, mode, index, true, "");
            }
            return NetbuyAsynByCodeOrName(date, mode, index);
        }

        [AllowAnonymous]
        public ActionResult NetbuyAsynByDate(string date, int col, int odcol, NetbuyMode mode, int index, bool isPage, string daytype, bool isOns = false)
        {
            Expression<Func<t_foam, bool>> expression = d => d.Date.CompareTo(date) == 0;

            switch (mode)
            {
                case NetbuyMode.只看上涨:
                    expression = expression.And(d => d.Change >= 0m);
                    break;
                case NetbuyMode.只看下跌:
                    expression = expression.And(d => d.Change < 0m);
                    break;
                case NetbuyMode.只看净买:
                    expression = expression.And(d => d.Netbuy >= 0m);
                    break;
                case NetbuyMode.只看净卖:
                    expression = expression.And(d => d.Netbuy < 0m);
                    break;
                case NetbuyMode.主力洗筹初期:
                    expression = expression.And(d => d.State == ForceState.XI);
                    break;
                case NetbuyMode.主力大互盘:
                    expression = expression.And(d => d.State == ForceState.HU);
                    break;
                case NetbuyMode.主力洗筹末期:
                    expression = expression.And(d => d.State == ForceState.xi);
                    break;
                case NetbuyMode.主力小互盘:
                    expression = expression.And(d => d.State == ForceState.hu);
                    break;
                case NetbuyMode.主力大砸盘:
                    expression = expression.And(d => d.State == ForceState.ZA);
                    break;
                case NetbuyMode.主力拉伸末期:
                    expression = expression.And(d => d.State == ForceState.LA);
                    break;
                case NetbuyMode.主力小砸盘:
                    expression = expression.And(d => d.State == ForceState.za);
                    break;
                case NetbuyMode.主力拉伸初期:
                    expression = expression.And(d => d.State == ForceState.la);
                    break;
                case NetbuyMode.昨日主力洗筹初期:
                case NetbuyMode.昨日主力大互盘:
                case NetbuyMode.昨日主力洗筹末期:
                case NetbuyMode.昨日主力小互盘:
                case NetbuyMode.昨日主力大砸盘:
                case NetbuyMode.昨日主力拉伸末期:
                case NetbuyMode.昨日主力小砸盘:
                case NetbuyMode.昨日主力拉伸初期:
                    return GetYestodayNetbuyList(date, col, odcol, mode, index, isPage, isOns);
                default:
                    break;
            }

            Expression<Func<t_foam, decimal?>> odby = d => d.State == ForceState.XI ? 2 : (d.State == ForceState.la ? 1 : 0);
            switch (col)
            {
                case 1:
                    odby = d => d.Netbuy;
                    break;
                case 2:
                    odby = d => d.Change;
                    break;
                case 3:
                    odby = d => d.Ltotal;
                    break;
                case 4:
                    //odby = d => d.Foam;
                    break;
                case 5:
                    odby = d => d.Num;
                    break;
                default:
                    break;
            }

            var samples = isOns ? OnsSamples(date, daytype) : SetSamplesByDate(date);


            IEnumerable<t_foam> t_foams = db.FoamList.Where(expression).ConvertAll(d => samples.ContainsKey(d.Code) ? d.SetNum(samples[d.Code]) : d);

            if (isPage || index > 1)
            {
                ViewData["odcol"] = odcol;

                if (col == odcol)
                {
                    t_foams = t_foams.AsQueryable().OrderBy(odby);
                }
                else
                {
                    t_foams = t_foams.AsQueryable().OrderByDescending(odby);
                }
            }
            else
            {
                if (col == odcol)
                {
                    ViewData["odcol"] = 0;
                    t_foams = t_foams.AsQueryable().OrderByDescending(odby);
                }
                else
                {
                    ViewData["odcol"] = col;
                    t_foams = t_foams.AsQueryable().OrderBy(odby);
                }
            }

            var pd = t_foams.ToPagedList(index, pageSize);
            return View(isOns ? "NetbuyOns" : "Netbuy", pd);
        }

        [AllowAnonymous]
        private ActionResult GetYestodayNetbuyList(string date, int col, int odcol, NetbuyMode mode, int index,
            bool isPage, bool isOns)
        {
            IEnumerable<t_foam> pd = new List<t_foam>();

            //IEnumerable<t_foam> t_foams = db.FoamList.Where(expression).GroupBy(d => d.Date).Select(g => new { date = g.Key, queue = g }).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ConvertAll(d => samples.ContainsKey(d.Code) ? d.SetNum(samples[d.Code]) : d);
            var groups = db.FoamList.Where(d => d.Date.CompareTo(date) <= 0).GroupBy(d => d.Date)
                .Select(g => new { date = g.Key, queue = g });

            if (groups.Count() > 1)
            {
                Expression<Func<t_foam, bool>> expression = d => true;

                switch (mode)
                {
                    case NetbuyMode.昨日主力洗筹初期:
                        expression = expression.And(d => d.State == ForceState.XI);
                        break;
                    case NetbuyMode.昨日主力大互盘:
                        expression = expression.And(d => d.State == ForceState.HU);
                        break;
                    case NetbuyMode.昨日主力洗筹末期:
                        expression = expression.And(d => d.State == ForceState.xi);
                        break;
                    case NetbuyMode.昨日主力小互盘:
                        expression = expression.And(d => d.State == ForceState.hu);
                        break;
                    case NetbuyMode.昨日主力大砸盘:
                        expression = expression.And(d => d.State == ForceState.ZA);
                        break;
                    case NetbuyMode.昨日主力拉伸末期:
                        expression = expression.And(d => d.State == ForceState.LA);
                        break;
                    case NetbuyMode.昨日主力小砸盘:
                        expression = expression.And(d => d.State == ForceState.za);
                        break;
                    case NetbuyMode.昨日主力拉伸初期:
                        expression = expression.And(d => d.State == ForceState.la);
                        break;
                    default:
                        break;
                }

                var samples = SetSamplesByDate(date);
                var codes = groups.OrderByDescending(d => d.date).Skip(1).FirstOrDefault()?.queue.AsQueryable()
                    .Where(expression).Select(d => d.Code);
                pd = groups.OrderByDescending(d => d.date).FirstOrDefault()?.queue.AsQueryable()
                    .Where(d => codes.Contains(d.Code)).ConvertAll(d => samples.ContainsKey(d.Code) ? d.SetNum(samples[d.Code]) : d);

                Expression<Func<t_foam, decimal?>> odby = d => d.State == ForceState.XI ? 1 : 0;

                switch (col)
                {
                    case 1:
                        odby = d => d.Netbuy;
                        break;
                    case 2:
                        odby = d => d.Change;
                        break;
                    case 3:
                        odby = d => d.Ltotal;
                        break;
                    //case 4:
                    //    odby = d => d.Foam;
                    //    break;
                    case 5:
                        odby = d => d.Num;
                        break;
                    default:
                        break;
                }

                if (isPage || index > 1)
                {
                    ViewData["odcol"] = odcol;

                    if (col == odcol)
                    {
                        pd = pd.AsQueryable().OrderBy(odby);
                    }
                    else
                    {
                        pd = pd.AsQueryable().OrderByDescending(odby);
                    }
                }
                else
                {
                    if (col == odcol)
                    {
                        ViewData["odcol"] = 0;
                        pd = pd.AsQueryable().OrderByDescending(odby);
                    }
                    else
                    {
                        ViewData["odcol"] = col;
                        pd = pd.AsQueryable().OrderBy(odby);
                    }
                }
            }
            return View(isOns ? "NetbuyOns" : "Netbuy", pd.ToPagedList(index, pageSize));
        }

        private Dictionary<string, int?> SetSamplesByDate(string date)
        {
            Dictionary<string, int?> samples = db.ChangesList.GroupBy(d => d.Date_9).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_9);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_8).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_8);


            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_7).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_7);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_6).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_6);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_5).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_5);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_4).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_4);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_3).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_3);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_2).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_2);

            if (samples != null)
            {
                return samples;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_1).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num_1);

            if (samples != null)
            {
                return samples;
            }

            samples = new Dictionary<string, int?>();
            return samples;
        }

        private Dictionary<string, int?> SetSamplesByDateByCodeOrName(string code)
        {
            Dictionary<string, int?> samples = new Dictionary<string, int?>();
            var queue = db.ChangesList.Where(d => code.Contains(d.Code) || code.Contains(d.Name)).Distinct();

            queue.ForEach(obj =>
            {
                if (!String.IsNullOrEmpty(obj.Date_9) && !samples.ContainsKey(obj.Code + obj.Date_9))
                {
                    samples.Add(obj.Code + obj.Date_9, obj.Num_9);
                }
                if (!String.IsNullOrEmpty(obj.Date_8) && !samples.ContainsKey(obj.Code + obj.Date_8))
                {
                    samples.Add(obj.Code + obj.Date_8, obj.Num_8);
                }
                if (!String.IsNullOrEmpty(obj.Date_7) && !samples.ContainsKey(obj.Code + obj.Date_7))
                {
                    samples.Add(obj.Code + obj.Date_7, obj.Num_7);
                }
                if (!String.IsNullOrEmpty(obj.Date_6) && !samples.ContainsKey(obj.Code + obj.Date_6))
                {
                    samples.Add(obj.Code + obj.Date_6, obj.Num_6);
                }
                if (!String.IsNullOrEmpty(obj.Date_5) && !samples.ContainsKey(obj.Code + obj.Date_5))
                {
                    samples.Add(obj.Code + obj.Date_5, obj.Num_5);
                }
                if (!String.IsNullOrEmpty(obj.Date_4) && !samples.ContainsKey(obj.Code + obj.Date_4))
                {
                    samples.Add(obj.Code + obj.Date_4, obj.Num_4);
                }
                if (!String.IsNullOrEmpty(obj.Date_3) && !samples.ContainsKey(obj.Code + obj.Date_3))
                {
                    samples.Add(obj.Code + obj.Date_3, obj.Num_3);
                }
                if (!String.IsNullOrEmpty(obj.Date_2) && !samples.ContainsKey(obj.Code + obj.Date_2))
                {
                    samples.Add(obj.Code + obj.Date_2, obj.Num_2);
                }
                if (!String.IsNullOrEmpty(obj.Date_1) && !samples.ContainsKey(obj.Code + obj.Date_1))
                {
                    samples.Add(obj.Code + obj.Date_1, obj.Num_1);
                }
            });

            return samples;
        }

        private Dictionary<string, int?> OnsSamples(string date, string skip)
        {
            Dictionary<string, int?> samples = new Dictionary<string, int?>();

            var list = db.ChangesList.Where(d => (d.Date_x ?? d.Date_9).CompareTo(date) == 0).OrderByDescending(d => d.Change_x);
            int? num = 0;
            decimal? prevChange = decimal.MaxValue;

            foreach (var obj in list)
            {
                if (samples.ContainsKey(obj.Code))
                {
                    continue;
                }

                if (obj.Change_x.HasValue && obj.Change_x < prevChange)
                {
                    prevChange = obj.Change_x;
                    num++;
                }

                switch (skip)
                {
                    case "-1":
                        samples.Add(obj.Code, num);//-1
                        break;
                    case "1":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_7 - num : obj.Num_8 - num);//2
                        break;
                    case "2":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_6 - num : obj.Num_7 - num);//3
                        break;
                    case "3":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_5 - num : obj.Num_6 - num);//4
                        break;
                    case "4":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_4 - num : obj.Num_5 - num);//5
                        break;
                    case "5":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_3 - num : obj.Num_4 - num);//6
                        break;
                    case "6":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_2 - num : obj.Num_3 - num);//7
                        break;
                    case "7":
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_1 - num : obj.Num_2 - num);//8
                        break;
                    default:
                        samples.Add(obj.Code, String.IsNullOrEmpty(obj.Date_x) ? obj.Num_6 - num : obj.Num_7 - num);//3
                        break;
                }
            }

            return samples;
        }

        [AllowAnonymous]
        public ActionResult NetbuyAsynByCodeOrName(string date, NetbuyMode mode, int index, bool isOns = false)
        {
            Expression<Func<t_foam, bool>> expression = d => true;
            bool isYestoday = false;

            switch (mode)
            {
                case NetbuyMode.只看上涨:
                    expression = expression.And(d => d.Change >= 0m);
                    break;
                case NetbuyMode.只看下跌:
                    expression = expression.And(d => d.Change < 0m);
                    break;
                case NetbuyMode.只看净买:
                    expression = expression.And(d => d.Netbuy >= 0m);
                    break;
                case NetbuyMode.只看净卖:
                    expression = expression.And(d => d.Netbuy < 0m);
                    break;
                case NetbuyMode.主力洗筹初期:
                    expression = expression.And(d => d.State == ForceState.XI);
                    break;
                case NetbuyMode.主力大互盘:
                    expression = expression.And(d => d.State == ForceState.HU);
                    break;
                case NetbuyMode.主力洗筹末期:
                    expression = expression.And(d => d.State == ForceState.xi);
                    break;
                case NetbuyMode.主力小互盘:
                    expression = expression.And(d => d.State == ForceState.hu);
                    break;
                case NetbuyMode.主力大砸盘:
                    expression = expression.And(d => d.State == ForceState.ZA);
                    break;
                case NetbuyMode.主力拉伸末期:
                    expression = expression.And(d => d.State == ForceState.LA);
                    break;
                case NetbuyMode.主力小砸盘:
                    expression = expression.And(d => d.State == ForceState.za);
                    break;
                case NetbuyMode.主力拉伸初期:
                    expression = expression.And(d => d.State == ForceState.la);
                    break;
                case NetbuyMode.昨日主力洗筹初期:
                    expression = expression.And(d => d.State == ForceState.XI);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力大互盘:
                    expression = expression.And(d => d.State == ForceState.HU);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力洗筹末期:
                    expression = expression.And(d => d.State == ForceState.xi);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力小互盘:
                    expression = expression.And(d => d.State == ForceState.hu);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力大砸盘:
                    expression = expression.And(d => d.State == ForceState.ZA);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力拉伸末期:
                    expression = expression.And(d => d.State == ForceState.LA);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力小砸盘:
                    expression = expression.And(d => d.State == ForceState.za);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨日主力拉伸初期:
                    expression = expression.And(d => d.State == ForceState.la);
                    isYestoday = true;
                    break;
                default:
                    break;
            }

            var samples = SetSamplesByDateByCodeOrName(date);
            //var pd = db.FoamList.Where(expression).ConvertAll(d => samples.ContainsKey(d.Date) ? d.SetNum(samples[d.Date]) : d).OrderBy(d => d.Code).ThenByDescending(d => d.Date).ToPagedList(index, pageSize);
            var groups = db.FoamList.Where(d => date.Contains(d.Code) || date.Contains(d.Name)).GroupBy(d => d.Date)
                .Select(g => new { date = g.Key, queue = g });
            IEnumerable<t_foam> pd = new List<t_foam>();

            if (groups.Any())
            {
                if (groups.SelectMany(d => d.queue).GroupBy(d => d.Code).Count() == 1)
                {
                    pd = groups.SelectMany(d => d.queue).OrderByDescending(d => d.Date)
                        .ConvertAll(d => samples.ContainsKey(d.Code + d.Date) ? d.SetNum(samples[d.Code + d.Date]) : d);
                }
                else if (isYestoday)
                {
                    if (groups.Count() > 1)
                    {
                        var codes = groups.OrderByDescending(d => d.date).Skip(1).FirstOrDefault()?.queue.AsQueryable()
                            .Where(expression).Select(d => d.Code);
                        pd = groups.OrderByDescending(d => d.date).FirstOrDefault()?.queue.AsQueryable().Where(d => codes.Contains(d.Code))
                            .ConvertAll(d => samples.ContainsKey(d.Code + d.Date) ? d.SetNum(samples[d.Code + d.Date]) : d);
                    }
                }
                else
                {
                    pd = groups.OrderByDescending(d => d.date).FirstOrDefault()?.queue.AsQueryable().Where(expression)
                        .ConvertAll(d => samples.ContainsKey(d.Code + d.Date) ? d.SetNum(samples[d.Code + d.Date]) : d);
                }
            }

            return View(isOns ? "NetbuyOns" : "Netbuy", pd.ToPagedList(index, pageSize));
        }

        public ActionResult NetbuyHistory()
        {
            var pd = new List<t_netbuyHistory>().ToPagedList(1, 3);
            return View(pd);
        }


        public async Task<ActionResult> NetbuyHistoryAsyn(string[] modes, int index = 1)
        {
            List<ForceState> pp = new List<ForceState>();

            if (modes != null)
            {
                if (modes.Length == 1)
                {
                    ViewData["modes"] = modes[0];
                    modes = modes[0].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    ViewData["modes"] = String.Join(",", modes);
                }


                Dictionary<string, ForceState[]> dicModes = new Dictionary<string, ForceState[]>();
                dicModes.Add("主力", new ForceState[] { ForceState.XI, ForceState.HU, ForceState.xi, ForceState.hu });
                dicModes.Add("散户", new ForceState[] { ForceState.ZA, ForceState.LA, ForceState.za, ForceState.la });
                dicModes.Add("放量", new ForceState[] { ForceState.XI, ForceState.HU, ForceState.ZA, ForceState.LA });
                dicModes.Add("缩量", new ForceState[] { ForceState.xi, ForceState.hu, ForceState.za, ForceState.la });
                dicModes.Add("买入", new ForceState[] { ForceState.XI, ForceState.xi, ForceState.ZA, ForceState.za });
                dicModes.Add("卖出", new ForceState[] { ForceState.HU, ForceState.hu, ForceState.LA, ForceState.la });

                foreach (var mode in modes)
                {
                    if (dicModes.ContainsKey(mode))
                    {
                        if (pp.Any())
                        {
                            pp = pp.Intersect(dicModes[mode]).ToList();
                        }
                        else
                        {
                            pp.AddRange(dicModes[mode]);
                        }
                    }
                }
            }

            IEnumerable<t_netbuyHistory> catapultList = await proc.ProcServer.ExecNetbuyHistoryProc(pp.ToArray());
            var pd = catapultList.ToPagedList(index, pageSize);
            return PartialView("pview_netbuyHistory", pd);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult GetModes(NetbuyMode? ptype, string date)
        {
            List<Rise> rises = new List<Rise>();

            NetbuyMode[] modeArray =
            {
                NetbuyMode.只看上涨,
                NetbuyMode.只看下跌,
                NetbuyMode.只看净买,
                NetbuyMode.只看净卖,
                NetbuyMode.主力洗筹初期,
                NetbuyMode.主力大互盘,
                NetbuyMode.主力洗筹末期,
                NetbuyMode.主力小互盘,
                NetbuyMode.主力大砸盘,
                NetbuyMode.主力拉伸末期,
                NetbuyMode.主力小砸盘,
                NetbuyMode.主力拉伸初期,
                NetbuyMode.全部
            };

            NetbuyMode[] subArray =
            {
                NetbuyMode.昨日主力洗筹初期,
                NetbuyMode.昨日主力大互盘,
                NetbuyMode.昨日主力洗筹末期,
                NetbuyMode.昨日主力小互盘,
                NetbuyMode.昨日主力大砸盘,
                NetbuyMode.昨日主力拉伸末期,
                NetbuyMode.昨日主力小砸盘,
                NetbuyMode.昨日主力拉伸初期
            };

            IEnumerable<t_foam> pd = new List<t_foam>();
            IEnumerable<t_foam> subpd = new List<t_foam>();

            if (date.IsDateTime())
            {
                var group = db.FoamList.GroupBy(d => d.Date).Select(g => new { date = g.Key, queue = g })
                    .Where(d => d.date.CompareTo(date) <= 0).OrderByDescending(d => d.date);
                pd = (group.FirstOrDefault()?.queue) ?? pd;

                if (group.Count() > 1)
                {
                    subpd = group.Skip(1).FirstOrDefault()?.queue;
                }
            }
            else
            {
                var group = db.FoamList.Where(d => date.Contains(d.Code) || date.Contains(d.Name)).GroupBy(d => d.Date)
                    .Select(g => new { date = g.Key, queue = g }).OrderByDescending(d => d.date);

                if (group.Any())
                {
                    if (group.SelectMany(d => d.queue).GroupBy(d => d.Code).Count() == 1)
                    {
                        pd = group.SelectMany(d => d.queue);
                    }
                    else
                    {
                        pd = group.FirstOrDefault()?.queue;

                        if (group.Count() > 1)
                        {
                            subpd = group.Skip(1).FirstOrDefault()?.queue;
                        }
                    }
                }
            }

            foreach (var mode in modeArray)
            {
                int count = 0;

                switch (mode)
                {
                    case NetbuyMode.只看上涨:
                        count = pd.Where(d => d.Change >= 0m).Count();
                        break;
                    case NetbuyMode.只看下跌:
                        count = pd.Where(d => d.Change < 0m).Count();
                        break;
                    case NetbuyMode.只看净买:
                        count = pd.Where(d => d.Netbuy >= 0m).Count();
                        break;
                    case NetbuyMode.只看净卖:
                        count = pd.Where(d => d.Netbuy < 0m).Count();
                        break;
                    case NetbuyMode.主力洗筹初期:
                        count = pd.Where(d => d.State == ForceState.XI).Count();
                        break;
                    case NetbuyMode.主力大互盘:
                        count = pd.Where(d => d.State == ForceState.HU).Count();
                        break;
                    case NetbuyMode.主力洗筹末期:
                        count = pd.Where(d => d.State == ForceState.xi).Count();
                        break;
                    case NetbuyMode.主力小互盘:
                        count = pd.Where(d => d.State == ForceState.hu).Count();
                        break;
                    case NetbuyMode.主力大砸盘:
                        count = pd.Where(d => d.State == ForceState.ZA).Count();
                        break;
                    case NetbuyMode.主力拉伸末期:
                        count = pd.Where(d => d.State == ForceState.LA).Count();
                        break;
                    case NetbuyMode.主力小砸盘:
                        count = pd.Where(d => d.State == ForceState.za).Count();
                        break;
                    case NetbuyMode.主力拉伸初期:
                        count = pd.Where(d => d.State == ForceState.la).Count();
                        break;
                    case NetbuyMode.全部:
                        count = pd.Count();
                        break;
                    default:
                        break;
                }

                var riseObj = new Rise(mode.ToString(), Convert.ToInt32(mode).ToString(), count, Convert.ToInt32(mode)) { IsCheck = mode == ptype };
                rises.Add(riseObj);
            }

            foreach (var mode in subArray)
            {
                int count = 0;

                switch (mode)
                {
                    case NetbuyMode.昨日主力洗筹初期:
                        count = subpd.Where(d => d.State == ForceState.XI).Count();
                        break;
                    case NetbuyMode.昨日主力大互盘:
                        count = subpd.Where(d => d.State == ForceState.HU).Count();
                        break;
                    case NetbuyMode.昨日主力洗筹末期:
                        count = subpd.Where(d => d.State == ForceState.xi).Count();
                        break;
                    case NetbuyMode.昨日主力小互盘:
                        count = subpd.Where(d => d.State == ForceState.hu).Count();
                        break;
                    case NetbuyMode.昨日主力大砸盘:
                        count = subpd.Where(d => d.State == ForceState.ZA).Count();
                        break;
                    case NetbuyMode.昨日主力拉伸末期:
                        count = subpd.Where(d => d.State == ForceState.LA).Count();
                        break;
                    case NetbuyMode.昨日主力小砸盘:
                        count = subpd.Where(d => d.State == ForceState.za).Count();
                        break;
                    case NetbuyMode.昨日主力拉伸初期:
                        count = subpd.Where(d => d.State == ForceState.la).Count();
                        break;
                    default:
                        break;
                }

                var riseObj = new Rise(mode.ToString(), Convert.ToInt32(mode).ToString(), count, Convert.ToInt32(mode)) { IsCheck = mode == ptype };
                rises.Add(riseObj);
            }

            return Json(rises.OrderBy(d => d.Index));
        }

        public ActionResult NetbuyOns(string date, string daytype, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (String.IsNullOrEmpty(date))
            {
                return NetbuyAsynByDate(Today, col, odcol, mode, index, false, daytype, true);
            }
            return NetbuyAsynByCodeOrName(date, mode, index, true);
        }

        public ActionResult NetbuyOnsPage(string date, string daytype, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (String.IsNullOrEmpty(date))
            {
                return NetbuyAsynByDate(Today, col, odcol, mode, index, true, daytype, true);
            }
            return NetbuyAsynByCodeOrName(date, mode, index, true);
        }

        private ActionResult NetbuyStatusByDate(string date, int col, bool isPage, int index)
        {
            //Expression<Func<t_foamState, bool>> expression = d => true;

            //if (!String.IsNullOrEmpty(status))
            //{
            //    List<ForceState?> s = new List<ForceState?>();

            //    status.Split(',').ForEach(d =>
            //    {
            //        if (!Enum.IsDefined(typeof(ForceState), d))
            //        {
            //            s.Add(null);
            //        }
            //        else
            //        {
            //            s.Add((ForceState)Enum.Parse(typeof(ForceState), d));
            //        }
            //    });

            //    expression = d => d.Status.IndexOf(s.ToArray()) == 0;

            //}

            var queue = db.SettlementList.GroupBy(d => d.Code).Select(g => new
            { code = g.Key, queue = g.Where(d => d.Date.CompareTo(date) <= 0).OrderByDescending(d => d.Date) }).ConvertAll(d => new t_foamState(d.queue));
            Expression<Func<t_foamState, decimal?>> odby = d => d.Volume;

            switch (col)
            {
                case 2:
                case 3:
                    odby = d => d.Funds;
                    break;
                default:
                    break;
            }

            var pd = col % 2 == 0 ? queue.AsQueryable().OrderBy(odby).Take(pageSize).ToPagedList(index, pageSize) : queue.AsQueryable().OrderByDescending(odby).Take(pageSize).ToPagedList(index, pageSize);

            if (isPage)
            {
                return PartialView("pview_netbuyState", pd);
            }

            return View(pd);
        }

        private ActionResult NetbuyStatusByCodeOrName(string date, int col, bool isPage, int index)
        {
            //Expression<Func<t_foamState, bool>> expression = d => true;

            //if (!String.IsNullOrEmpty(status))
            //{
            //    List<ForceState?> s = new List<ForceState?>();

            //    status.Split(',').ForEach(d =>
            //    {
            //        if (!Enum.IsDefined(typeof(ForceState), d))
            //        {
            //            s.Add(null);
            //        }
            //        else
            //        {
            //            s.Add((ForceState)Enum.Parse(typeof(ForceState), d));
            //        }
            //    });

            //    expression = d => d.Status.IndexOf(s.ToArray()) == 0;

            //}

            var queue = db.SettlementList.Where(d=> date.Contains(d.Code) || date.Contains(d.Name)).GroupBy(d => d.Code).Select(g => new
            { code = g.Key, queue = g }).ConvertAll(d => new t_foamState(d.queue));
            Expression<Func<t_foamState, decimal?>> odby = d => d.Volume;

            switch (col)
            {
                case 2:
                case 3:
                    odby = d => d.Funds;
                    break;
                default:
                    break;
            }

            var pd = col % 2 == 0 ? queue.AsQueryable().OrderBy(odby).Take(pageSize).ToPagedList(index, pageSize) : queue.AsQueryable().OrderByDescending(odby).Take(pageSize).ToPagedList(index, pageSize);

            if (isPage)
            {
                return PartialView("pview_netbuyState", pd);
            }

            return View(pd);
        }

        public ActionResult NetbuyStatus()
        {
            string date = Today;
            int col = 0;

            ViewData["date"] = date;
            ViewData["col"] = col;

            if (date.IsDateTime())
            {
                return NetbuyStatusByDate(date, col, false, 1);
            }

            return NetbuyStatusByCodeOrName(date, col, false, 1);
        }

        [HttpPost]
        public ActionResult NetbuyStatusAsycn(string date, int col = 0, int index = 1)
        {
            if (!Request.IsAjaxRequest())
            {
                return HttpNotFound();
            }

            ViewData["date"] = date;
            ViewData["col"] = col;

            if (date.IsDateTime())
            {
                return NetbuyStatusByDate(date, col, true, index);
            }

            return NetbuyStatusByCodeOrName(date, col, true, index);
        }
    }
}
