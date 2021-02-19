async function asyncGetStockData(getCodes) {
    var codes = getCodes();
    var dataArray = new Array();
    if (codes.length == 0)
        return dataArray;
    var mlength = 300;
    for (var i = 0; i <= codes.length;) {
        var end = i + mlength;
        var codeArray = codes.slice(i, end);
        await $.ajax({
            dataType: 'script',
            async: false,
            url: 'http://hq.sinajs.cn/list=' + getStockStr(codeArray),
            cache: true,
            success: function (msg) {
                codeArray.forEach(function (item, index) {
                    var elements = eval("hq_str_s_" + item);
                    if (typeof (elements) != "undefined") {
                        var eleArray = elements.split(",");
                        eleArray.push(item);
                        dataArray.push(eleArray);
                    }
                });
            }
        });
        i = end;
    }

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


class Url {
    /**
     * 传入对象返回url参数
     * @param {Object} data {a:1}
     * @returns {string}
     */
    getParam(data) {
        let url = '';
        for (var k in data) {
            let value = data[k] !== undefined ? data[k] : '';
            url += `&${k}=${encodeURIComponent(value)}`
        }
        return url ? url.substring(1) : ''
    }

    /**
     * 将url和参数拼接成完整地址
     * @param {string} url url地址
     * @param {Json} data json对象
     * @returns {string}
     */
    getUrl(url, data) {
        //看原始url地址中开头是否带?，然后拼接处理好的参数
        return url += (url.indexOf('?') < 0 ? '?' : '') + this.getParam(data)
    }
}

function jumpRoute(url, param) {
    let URL = new Url();
    let newUrl = URL.getUrl(url, param);
    location.href = newUrl;
}

//var today = now.getFullYear() +
//    "-" +
//    ((now.getMonth() + 1) < 10 ? "0" : "") +
//    (now.getMonth() + 1) +
//    "-" +
//    (now.getDate() < 10 ? "0" : "") +
//    now.getDate();