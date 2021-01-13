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
        主力缩量卖出,
        主力缩量买入,
        散户缩量卖出,
        散户缩量买入,
        主力放量卖出,
        主力放量买入,
        散户放量卖出,
        散户放量买入
    }
}