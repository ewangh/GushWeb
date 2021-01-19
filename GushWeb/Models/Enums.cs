using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Models
{
    public enum NetbuyMode
    {
        只看上涨=1,
        只看下跌,
        只看净买,
        只看净卖,
        主力缩量卖出,
        主力缩量买入,
        散户缩量卖出,
        散户缩量买入,
        主力放量卖出,
        主力放量买入,
        散户放量卖出,
        散户放量买入,
        昨天主力缩量卖出,
        昨天主力缩量买入,
        昨天散户缩量卖出,
        昨天散户缩量买入,
        昨天主力放量卖出,
        昨天主力放量买入,
        昨天散户放量卖出,
        昨天散户放量买入,
        全部
    }
}