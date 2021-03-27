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
                case NetbuyMode.主力放量买入:
                    expression = expression.And(d => d.State == ForceState.主力放量买入);
                    break;
                case NetbuyMode.主力放量卖出:
                    expression = expression.And(d => d.State == ForceState.主力放量卖出);
                    break;
                case NetbuyMode.主力缩量买入:
                    expression = expression.And(d => d.State == ForceState.主力缩量买入);
                    break;
                case NetbuyMode.主力缩量卖出:
                    expression = expression.And(d => d.State == ForceState.主力缩量卖出);
                    break;
                case NetbuyMode.散户放量买入:
                    expression = expression.And(d => d.State == ForceState.散户放量买入);
                    break;
                case NetbuyMode.散户放量卖出:
                    expression = expression.And(d => d.State == ForceState.散户放量卖出);
                    break;
                case NetbuyMode.散户缩量买入:
                    expression = expression.And(d => d.State == ForceState.散户缩量买入);
                    break;
                case NetbuyMode.散户缩量卖出:
                    expression = expression.And(d => d.State == ForceState.散户缩量卖出);
                    break;
                case NetbuyMode.昨天主力放量买入:
                case NetbuyMode.昨天主力放量卖出:
                case NetbuyMode.昨天主力缩量买入:
                case NetbuyMode.昨天主力缩量卖出:
                case NetbuyMode.昨天散户放量买入:
                case NetbuyMode.昨天散户放量卖出:
                case NetbuyMode.昨天散户缩量买入:
                case NetbuyMode.昨天散户缩量卖出:
                    return GetYestodayNetbuyList(date, col, odcol, mode, index, isPage, isOns);
                default:
                    break;
            }

            Expression<Func<t_foam, decimal?>> odby = d => d.State == ForceState.主力放量买入 ? 1 : 0;
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
                    case NetbuyMode.昨天主力放量买入:
                        expression = expression.And(d => d.State == ForceState.主力放量买入);
                        break;
                    case NetbuyMode.昨天主力放量卖出:
                        expression = expression.And(d => d.State == ForceState.主力放量卖出);
                        break;
                    case NetbuyMode.昨天主力缩量买入:
                        expression = expression.And(d => d.State == ForceState.主力缩量买入);
                        break;
                    case NetbuyMode.昨天主力缩量卖出:
                        expression = expression.And(d => d.State == ForceState.主力缩量卖出);
                        break;
                    case NetbuyMode.昨天散户放量买入:
                        expression = expression.And(d => d.State == ForceState.散户放量买入);
                        break;
                    case NetbuyMode.昨天散户放量卖出:
                        expression = expression.And(d => d.State == ForceState.散户放量卖出);
                        break;
                    case NetbuyMode.昨天散户缩量买入:
                        expression = expression.And(d => d.State == ForceState.散户缩量买入);
                        break;
                    case NetbuyMode.昨天散户缩量卖出:
                        expression = expression.And(d => d.State == ForceState.散户缩量卖出);
                        break;
                    default:
                        break;
                }

                var samples = SetSamplesByDate(date);
                var codes = groups.OrderByDescending(d => d.date).Skip(1).FirstOrDefault()?.queue.AsQueryable()
                    .Where(expression).Select(d => d.Code);
                pd = groups.OrderByDescending(d => d.date).FirstOrDefault()?.queue.AsQueryable()
                    .Where(d => codes.Contains(d.Code)).ConvertAll(d => samples.ContainsKey(d.Code) ? d.SetNum(samples[d.Code]) : d);

                Expression<Func<t_foam, decimal?>> odby = d => d.State == ForceState.主力放量买入 ? 1 : 0;

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
                    case "0":
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
                case NetbuyMode.主力放量买入:
                    expression = expression.And(d => d.State == ForceState.主力放量买入);
                    break;
                case NetbuyMode.主力放量卖出:
                    expression = expression.And(d => d.State == ForceState.主力放量卖出);
                    break;
                case NetbuyMode.主力缩量买入:
                    expression = expression.And(d => d.State == ForceState.主力缩量买入);
                    break;
                case NetbuyMode.主力缩量卖出:
                    expression = expression.And(d => d.State == ForceState.主力缩量卖出);
                    break;
                case NetbuyMode.散户放量买入:
                    expression = expression.And(d => d.State == ForceState.散户放量买入);
                    break;
                case NetbuyMode.散户放量卖出:
                    expression = expression.And(d => d.State == ForceState.散户放量卖出);
                    break;
                case NetbuyMode.散户缩量买入:
                    expression = expression.And(d => d.State == ForceState.散户缩量买入);
                    break;
                case NetbuyMode.散户缩量卖出:
                    expression = expression.And(d => d.State == ForceState.散户缩量卖出);
                    break;
                case NetbuyMode.昨天主力放量买入:
                    expression = expression.And(d => d.State == ForceState.主力放量买入);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天主力放量卖出:
                    expression = expression.And(d => d.State == ForceState.主力放量卖出);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天主力缩量买入:
                    expression = expression.And(d => d.State == ForceState.主力缩量买入);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天主力缩量卖出:
                    expression = expression.And(d => d.State == ForceState.主力缩量卖出);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天散户放量买入:
                    expression = expression.And(d => d.State == ForceState.散户放量买入);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天散户放量卖出:
                    expression = expression.And(d => d.State == ForceState.散户放量卖出);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天散户缩量买入:
                    expression = expression.And(d => d.State == ForceState.散户缩量买入);
                    isYestoday = true;
                    break;
                case NetbuyMode.昨天散户缩量卖出:
                    expression = expression.And(d => d.State == ForceState.散户缩量卖出);
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

            if (!groups.Any())
            {

            }
            else if (groups.SelectMany(d => d.queue).GroupBy(d => d.Code).Count() == 1)
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
                dicModes.Add("主力", new ForceState[] { ForceState.主力放量买入, ForceState.主力放量卖出, ForceState.主力缩量买入, ForceState.主力缩量卖出 });
                dicModes.Add("散户", new ForceState[] { ForceState.散户放量买入, ForceState.散户放量卖出, ForceState.散户缩量买入, ForceState.散户缩量卖出 });
                dicModes.Add("放量", new ForceState[] { ForceState.主力放量买入, ForceState.主力放量卖出, ForceState.散户放量买入, ForceState.散户放量卖出 });
                dicModes.Add("缩量", new ForceState[] { ForceState.主力缩量买入, ForceState.主力缩量卖出, ForceState.散户缩量买入, ForceState.散户缩量卖出 });
                dicModes.Add("买入", new ForceState[] { ForceState.主力放量买入, ForceState.主力缩量买入, ForceState.散户放量买入, ForceState.散户缩量买入 });
                dicModes.Add("卖出", new ForceState[] { ForceState.主力放量卖出, ForceState.主力缩量卖出, ForceState.散户放量卖出, ForceState.散户缩量卖出 });

                foreach (var mode in modes)
                {
                    if (dicModes.ContainsKey(mode))
                    {
                        if (!pp.Any())
                        {
                            pp.AddRange(dicModes[mode]);
                        }
                        else
                        {
                            pp = pp.Intersect(dicModes[mode]).ToList();
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
                NetbuyMode.主力放量买入,
                NetbuyMode.主力放量卖出,
                NetbuyMode.主力缩量买入,
                NetbuyMode.主力缩量卖出,
                NetbuyMode.散户放量买入,
                NetbuyMode.散户放量卖出,
                NetbuyMode.散户缩量买入,
                NetbuyMode.散户缩量卖出,
                NetbuyMode.全部
            };

            NetbuyMode[] subArray =
            {
                NetbuyMode.昨天主力放量买入,
                NetbuyMode.昨天主力放量卖出,
                NetbuyMode.昨天主力缩量买入,
                NetbuyMode.昨天主力缩量卖出,
                NetbuyMode.昨天散户放量买入,
                NetbuyMode.昨天散户放量卖出,
                NetbuyMode.昨天散户缩量买入,
                NetbuyMode.昨天散户缩量卖出
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

                if (!group.Any())
                {

                }
                else if (group.SelectMany(d => d.queue).GroupBy(d => d.Code).Count() == 1)
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
                    case NetbuyMode.主力放量买入:
                        count = pd.Where(d => d.State == ForceState.主力放量买入).Count();
                        break;
                    case NetbuyMode.主力放量卖出:
                        count = pd.Where(d => d.State == ForceState.主力放量卖出).Count();
                        break;
                    case NetbuyMode.主力缩量买入:
                        count = pd.Where(d => d.State == ForceState.主力缩量买入).Count();
                        break;
                    case NetbuyMode.主力缩量卖出:
                        count = pd.Where(d => d.State == ForceState.主力缩量卖出).Count();
                        break;
                    case NetbuyMode.散户放量买入:
                        count = pd.Where(d => d.State == ForceState.散户放量买入).Count();
                        break;
                    case NetbuyMode.散户放量卖出:
                        count = pd.Where(d => d.State == ForceState.散户放量卖出).Count();
                        break;
                    case NetbuyMode.散户缩量买入:
                        count = pd.Where(d => d.State == ForceState.散户缩量买入).Count();
                        break;
                    case NetbuyMode.散户缩量卖出:
                        count = pd.Where(d => d.State == ForceState.散户缩量卖出).Count();
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
                    case NetbuyMode.昨天主力放量买入:
                        count = subpd.Where(d => d.State == ForceState.主力放量买入).Count();
                        break;
                    case NetbuyMode.昨天主力放量卖出:
                        count = subpd.Where(d => d.State == ForceState.主力放量卖出).Count();
                        break;
                    case NetbuyMode.昨天主力缩量买入:
                        count = subpd.Where(d => d.State == ForceState.主力缩量买入).Count();
                        break;
                    case NetbuyMode.昨天主力缩量卖出:
                        count = subpd.Where(d => d.State == ForceState.主力缩量卖出).Count();
                        break;
                    case NetbuyMode.昨天散户放量买入:
                        count = subpd.Where(d => d.State == ForceState.散户放量买入).Count();
                        break;
                    case NetbuyMode.昨天散户放量卖出:
                        count = subpd.Where(d => d.State == ForceState.散户放量卖出).Count();
                        break;
                    case NetbuyMode.昨天散户缩量买入:
                        count = subpd.Where(d => d.State == ForceState.散户缩量买入).Count();
                        break;
                    case NetbuyMode.昨天散户缩量卖出:
                        count = subpd.Where(d => d.State == ForceState.散户缩量卖出).Count();
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
    }
}
