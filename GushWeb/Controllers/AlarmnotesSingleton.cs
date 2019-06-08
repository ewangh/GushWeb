using GushLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GushWeb.Utility;

namespace GushWeb.Controllers
{
    public class DatePage
    {
        public string PrevDate { get; set; }
        public string CurrentDate { get; set; }
        public string NextDate { get; set; }
        
    }

    public class AlarmnotesSingleton
    {
        private static AlarmnotesSingleton _as=new AlarmnotesSingleton();

        private List<string> _timeList=new List<string>();
        private DateTime _currentDate;
        

        public static AlarmnotesSingleton GetObj()
        {
            return _as;
        }

        public DatePage GetDatePage(ref string date)
        {
            if(date.IsNullOrEmpty())
                return new DatePage();
            if (_currentDate != DateTime.Today)
            {
                _currentDate = DateTime.Today;
                lock (_timeList)
                {
                    using (GushDBContext db = new GushDBContext())
                    {
                        _timeList = db.AlarmNotesList.OrderBy(d => d.Date).ToList().ConvertAll(d => d.Date).Distinct()
                            .ToList();
                    }
                }
            }
            if(_timeList.IsNullOrEmpty())return new DatePage();
            if (!_timeList.Contains(date)) date = _timeList.LastOrDefault();
            return _getDatePage(date);
        }

        private DatePage _getDatePage(string date)
        {
            try
            {
                var tL = new List<string>(_timeList);
                tL.Add(null);
                tL.Insert(0, null);
                int i = tL.IndexOf(date);
                var tA = tL.ToArray();
                var prev = tA[i - 1];
                var current = tA[i];
                var next = tA[i+1];
                return new DatePage()
                {
                    PrevDate = prev,
                    CurrentDate = current,
                    NextDate = next,
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}