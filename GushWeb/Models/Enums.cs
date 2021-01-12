using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public enum NetbuyMode
    {
        Up=1,
        Down,
        Buy,
        Sell,
        Lockup,
        WaitSee,
        UnLockup,
        UnWaitSee
    }
}