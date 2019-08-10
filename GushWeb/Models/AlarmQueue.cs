using GushLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public class AlarmQueue
    {
        public AlarmQueue()
        {

        }

        public AlarmQueue(IEnumerable<t_alarmnotes> list)
        {
            alarmList = list;
        }

        public IEnumerable<t_alarmnotes> alarmList { get; set; }
    }
}