function getChanges(action, mformId, chartId, func_begin, func_success, func_failure) {
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
            //toolbox: {
            //    feature: {
            //        saveAsImage: {}
            //    }
            //},
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