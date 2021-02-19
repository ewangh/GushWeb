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
using GushWeb.Helpers;
using GushWeb.Models;
using GushWeb.Utility;
using Microsoft.Ajax.Utilities;
using PagedList;
using ServiceStack.Common.Extensions;

namespace GushWeb.Controllers
{
    public class TransactionController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        private GushProcContext proc = new GushProcContext();
        const string SZprefix = "sz";
        const string SHprefix = "sh";
        private const int pageSize = 300;
        private Dictionary<string, int?> samples;

        public ActionResult Netbuy(string date, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (date.IsDateTime())
            {
                return NetbuyAsynByDate(date, col, odcol, mode, index, false);
            }
            return NetbuyAsynByCodeOrName(date, mode, index);
        }

        public ActionResult NetbuyPage(string date, int col = 0, int odcol = 0, NetbuyMode mode = 0, int index = 1)
        {
            IEnumerable<t_foam> t_foams = new List<t_foam>();
            Expression<Func<t_foam, bool>> expression = d => true;

            if (date.IsDateTime())
            {
                return NetbuyAsynByDate(date, col, odcol, mode, index, true);
            }
            return NetbuyAsynByCodeOrName(date, mode, index);
        }

        public ActionResult NetbuyAsynByDate(string date, int col, int odcol, NetbuyMode mode, int index, bool isPage)
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
                    return GetYestodayNetbuyList(date, col, odcol, mode, index, isPage);
                default:
                    break;
            }

            Expression<Func<t_foam, decimal?>> odby = d => d.Foam;
            switch (col)
            {
                case 1:
                    odby = d => d.Netbuy;
                    break;
                case 2:
                    odby = d => d.Change;
                    break;
                case 3:
                    odby = d => d.Total;
                    break;
                case 4:
                    odby = d => d.Num;
                    break;
                default:
                    break;
            }

            SetSamples(date);

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
            return View("Netbuy", pd);
        }

        private ActionResult GetYestodayNetbuyList(string date, int col, int odcol, NetbuyMode mode, int index, bool isPage)
        {
            ForceState state = ForceState.Unknow;
            switch (mode)
            {
                case NetbuyMode.昨天主力放量买入:
                    state = ForceState.主力放量买入;
                    break;
                case NetbuyMode.昨天主力放量卖出:
                    state = ForceState.主力放量卖出;
                    break;
                case NetbuyMode.昨天主力缩量买入:
                    state = ForceState.主力缩量买入;
                    break;
                case NetbuyMode.昨天主力缩量卖出:
                    state = ForceState.主力缩量卖出;
                    break;
                case NetbuyMode.昨天散户放量买入:
                    state = ForceState.散户放量买入;
                    break;
                case NetbuyMode.昨天散户放量卖出:
                    state = ForceState.散户放量卖出;
                    break;
                case NetbuyMode.昨天散户缩量买入:
                    state = ForceState.散户缩量买入;
                    break;
                case NetbuyMode.昨天散户缩量卖出:
                    state = ForceState.散户缩量卖出;
                    break;
                default:
                    break;
            }

            SetSamples(date);

            IEnumerable<t_foam> t_foams =
                from f1 in db.FoamList.GroupBy(d => d.Date).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault().queue
                join f2 in db.FoamList.Where(d => d.Date.CompareTo(date) == 0) on f1.Code equals f2.Code into ftemp
                from f3 in ftemp
                select f3.SetNum(samples.ContainsKey(f3.Code) ? samples[f3.Code] : null);

            Expression<Func<t_foam, decimal?>> odby = d => d.Foam;
            switch (col)
            {
                case 1:
                    odby = d => d.Netbuy;
                    break;
                case 2:
                    odby = d => d.Change;
                    break;
                case 3:
                    odby = d => d.Total;
                    break;
                case 4:
                default:
                    break;
            }

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
            return View("Netbuy", pd);
        }

        private void SetSamples(string date)
        {
            samples = db.ChangesList.GroupBy(d => d.Date_9).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return; 
            }
            samples = db.ChangesList.GroupBy(d => d.Date_8).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);


            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_7).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_6).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_5).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_4).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_3).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_2).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = db.ChangesList.GroupBy(d => d.Date_1).Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue.ToDictionary(pair => pair.Code, pair => pair.Num);

            if (samples != null)
            {
                return;
            }
            samples = new Dictionary<string, int?>();
        }

        public ActionResult NetbuyAsynByCodeOrName(string codename, NetbuyMode mode, int index)
        {
            Expression<Func<t_foam, bool>> expression = d => codename.Contains(d.Code) || codename.Contains(d.Name);

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
                default:
                    break;
            }

            var pd = db.FoamList.Where(expression).OrderBy(d => d.Code).ThenByDescending(d => d.Date).ToPagedList(index, pageSize);
            return View("Netbuy", pd);
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
                        if (pp.Count == 0)
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
                NetbuyMode.昨天主力放量买入,
                NetbuyMode.昨天主力放量卖出,
                NetbuyMode.昨天主力缩量买入,
                NetbuyMode.昨天主力缩量卖出,
                NetbuyMode.昨天散户放量买入,
                NetbuyMode.昨天散户放量卖出,
                NetbuyMode.昨天散户缩量买入,
                NetbuyMode.昨天散户缩量卖出,
                NetbuyMode.全部
            };

            foreach (var mode in modeArray)
            {
                Expression<Func<t_foam, bool>> expression = d => true;
                var yestoday = string.Empty;
                int count = 0;

                if (date.IsDateTime())
                {
                    expression = expression.And(d => d.Date.CompareTo(date) == 0);
                }
                else
                {
                    expression = expression.And(d => date.Contains(d.Code) || date.Contains(d.Name));
                }
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
                        yestoday = db.FoamList.Where(d => d.Date.CompareTo(date) < 0).Max(d => d.Date);
                        break;
                    default:
                        break;
                }

                count = db.FoamList.Where(expression).Count();

                if (yestoday.IsDateTime())
                {
                    switch (mode)
                    {
                        case NetbuyMode.昨天主力放量买入:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.主力放量买入).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天主力放量卖出:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.主力放量卖出).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天主力缩量买入:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.主力缩量买入).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天主力缩量卖出:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.主力缩量卖出).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天散户放量买入:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.散户放量买入).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天散户放量卖出:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.散户放量卖出).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天散户缩量买入:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.散户缩量买入).Select(d => d.Code)
                                .Count();
                            break;
                        case NetbuyMode.昨天散户缩量卖出:
                            count = db.FoamList.Where(d =>
                                    d.Date.CompareTo(yestoday) == 0 && d.State == ForceState.散户缩量卖出).Select(d => d.Code)
                                .Count();
                            break;
                        default:
                            break;
                    }
                }

                var riseObj = new Rise()
                {
                    Text = mode.ToString(),
                    Ptype = Convert.ToInt32(mode).ToString(),
                    Change = count,
                    IsCheck = mode == ptype
                };
                rises.Add(riseObj);
            }

            return Json(rises);
        }
    }
}
