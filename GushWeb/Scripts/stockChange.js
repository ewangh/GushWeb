﻿function getChangesByForm(action, mformId, chartId, func_begin, func_success, func_failure) {
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
    }).fail(function (error) {
        func_failure();
    });
}

function getChangesByData(action, value, chartId, func_begin, func_success, func_failure) {
    func_begin();
    var data = { ptype: value };
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
    }).fail(function (error) {
        func_failure();
    });
}

function getRises(action, mformId, func_link) {
    $("#" + mformId).empty();
    $.postJson(action, '', function (result) {
        if (result.length > 0) {
            $.each(result, function (i, item) {
                var _href = "javascript:" + func_link + "('" + item + "');"
                var a_rise = $('<a>', {
                    text: item,
                    href: "javascript:;",
                    onclick: _href
                    //target: "_blank",
                    //title: "goto baidu"
                });
                var span_tab = $('<span>', {
                    text: " "
                });
                $("#" + mformId).append(a_rise).append(span_tab);
            });

        }
    }).fail(function (error) {

    });
}