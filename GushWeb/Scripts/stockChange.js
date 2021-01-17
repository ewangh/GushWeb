function getChangesByForm(action, mformId, chartId, func_begin, func_success, func_failure) {
    func_begin();
    $.postJson(action, $("#" + mformId).serialize(), function (result) {
        var axis = [];
        if (result.length > 0) {
            axis = result[0].axis;
        }
        option = {
            title: {
                text: ''
            },
            tooltip: {
                trigger: 'axis'
            },
            legend: {
                //data: ['邮件营销', '联盟广告', '视频广告', '直接访问', '搜索引擎']
            },
            grid: {
                //left: '3%',
                //right: '4%',
                //bottom: '3%',
                containLabel: false
            },
            toolbox: {
                feature: {
                    saveAsImage: {}
                }
            },
            xAxis: {
                type: 'category',
                boundaryGap: false,
                data: axis
            },
            yAxis: {
                type: 'value',
                scale: true
            },
            series: result
        };

        var myChart = echarts.init(document.getElementById(chartId));
        myChart.setOption(option, true);
        func_success();
    });
}

function getChangesByData(action, data, chartId, func_begin, func_success, func_failure) {
    func_begin();
    $.postJson(action, data, function (result) {
        var axis = [];
        if (result.length > 0) {
            axis = result[0].axis;
        }
        option = {
            title: {
                text: ''
            },
            tooltip: {
                trigger: 'axis'
            },
            legend: {
                //data: ['邮件营销', '联盟广告', '视频广告', '直接访问', '搜索引擎']
            },
            grid: {
                //left: '3%',
                //right: '4%',
                //bottom: '3%',
                containLabel: true
            },
            toolbox: {
                feature: {
                    saveAsImage: {}
                }
            },
            xAxis: {
                type: 'category',
                boundaryGap: false,
                data: axis
            },
            yAxis: {
                type: 'value',
                scale: true
            },
            series: result
        };

        var myChart = echarts.init(document.getElementById(chartId));
        myChart.setOption(option, true);
        func_success();
    });
}

$.postJson = function (url, data, callback) {
    $.post(url, data, callback, "json");
};

function getRises(action, data, mformId, func_link) {
    $.postJson(action, data, function (result) {
        $("#" + mformId).empty();
        if (result.length > 0) {
            $.each(result,
                function (i, item) {
                    var _href = "javascript:$(this).css({'border':'solid'});" + func_link + "('" + item.Ptype + "');"
                    var a_rise = $('<a>',
                        {
                            text: item.Text + "[" + item.Change + "]",
                            href: "javascript:;",
                            onclick: _href
                            //target: "_blank",
                            //title: "goto baidu"
                        }).css({"border":item.IsCheck?"solid":""});
                    var span_tab = $('<span>',
                        {
                            text: " "
                        });
                    $("#" + mformId).append(a_rise).append(span_tab);
                });

        }
    });
}