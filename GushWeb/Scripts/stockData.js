function getStockData(getCodes, func) {
    var codes = getCodes();
    $.ajax({
        dataType: 'script',
        async: true,
        url: 'http://hq.sinajs.cn/list=' + getStockStr(codes),
        cache: true,
        success: function (msg) {
            codes.forEach(function(item, index) {
                var elements = eval("hq_str_s_" + item);
                if (typeof (elements) != "undefined") {
                    var eleArray = elements.split(",");
                    func(item, eleArray[1]);
                }
            });
        }
    });
}

function getStockStr(codes) {
    var codesArray = new Array();
    codes.forEach(function (item, index) {
        codesArray.push("s_" + item);
    });
    return codesArray.join();
}