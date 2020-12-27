async function asyncGetStockData(getCodes) {
    var codes = getCodes();
    var dataArray = new Array();
    await $.ajax({
        dataType: 'script',
        async: false,
        url: 'http://hq.sinajs.cn/list=' + getStockStr(codes),
        cache: true,
        success: function (msg) {
            codes.forEach(function (item, index) {
                var elements = eval("hq_str_s_" + item);
                if (typeof (elements) != "undefined") {
                    var eleArray = elements.split(",");
                    eleArray.push(item);
                    dataArray.push(eleArray);
                }
            });
        }
    });
    return dataArray;
}

function getStockStr(codes) {
    var codesArray = new Array();
    codes.forEach(function (item, index) {
        codesArray.push("s_" + item);
    });
    return codesArray.join();
}

function morethan(num) {
    $("a[name='num']").each(function () {
        var i = parseInt($(this).attr("value"));
        if (i <= parseInt(num)) {
            $(this).css({ "border": "solid" })
        } else {
            $(this).css({ "border": "" })
        }
    });
}