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
        主力小互盘,//hu
        主力洗筹末期,//xi
        主力拉伸初期,//la
        主力小砸盘,//za
        主力大互盘,//HU
        主力洗筹初期,//XI
        主力拉伸末期,//LA
        主力大砸盘,//ZA
        昨日上涨,
        昨日下跌,
        昨日净买,
        昨日净卖,
        昨日主力小互盘, //hu
        昨日主力洗筹末期, //xi
        昨日主力拉伸初期, //la
        昨日主力小砸盘, //za
        昨日主力大互盘, //HU
        昨日主力洗筹初期, //XI
        昨日主力拉伸末期, //LA
        昨日主力大砸盘, //ZA
        全部
    }
}