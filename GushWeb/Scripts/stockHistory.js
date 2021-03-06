﻿function getKLine(action, mformId, chartId) {
    $.postJson(action, $("#"+mformId).serialize(), function (result) {
        var values = [];
        var stockName;
        var coordDate;
        var zeroDate;
        $.each(result, function (i, item) {
            var fruits = [item.Date, item.Opening, item.Price, item.Lower, item.Highest, item.Closed, item.bPrice, item.cPrice, item.Volume];
            //categoryData.push(fruits.splice(0, 1)[0]);
            //数据数组，即数组中除日期外的数据
            //alert(rawData[i]);
            values.push(fruits);
            stockName = item.Name;
            coordDate = item.CoordDate;
            zeroDate = item.ZeroDate;
        });
        window.data0 = {
            stockName: stockName,
            coordDate: coordDate,
            zeroDate: zeroDate,
            values: values              //数组中的数据 y轴对应的数据
        };
        var myChart = echarts.init(document.getElementById(chartId));
        splitData(myChart);
    });
}

//计算MA平均线，N日移动平均线=N日收盘价之和/N  dayCount要计算的天数(5,10,20,30)
function calculateMA(dayCount, data) {
    var result = [];
    for (var i = 0, len = data.values.length; i < len; i++) {
        if (i < dayCount) {
            result.push('-');
            continue;
        }
        var sum = 0;
        for (var j = 0; j < dayCount; j++) {
            sum += data.values[i - j][1];
        }
        result.push(+(sum / dayCount).toFixed(3));
    }
    return result;
}

function calculateBp() {
    var result = [];
    result.push(data0.values[0][5]);//加多一行数据
    for (var i = 0, len = data0.values.length; i < len; i++) {
        result.push(data0.values[i][5]);
        // alert(result);
    }
    return result;
}

function calculateCp() {
    var result = [];
    result.push(data0.values[0][6]);//加多一行数据
    for (var i = 0, len = data0.values.length; i < len; i++) {
        result.push(data0.values[i][6]);
        // alert(result);
    }
    return result;
}

function calculateZT() {
    var result = [];
    for (var i = 0, len = data0.values.length; i < len; i++) {
        result.push((data0.values[i][4] * 1.1).toFixed(2));
        // alert(result);
    }
    return result;
}

function calculateChange() {
    var result = [];
    for (var i = 0, len = data0.values.length; i < len; i++) {
        result.push(((data0.values[i][1] - data0.values[i][4]) * 100 / data0.values[i][4]).toFixed(2));
        // alert(result);
    }
    return result;
}

var upColor = '#ec0000';
var downColor = '#00da3c';

function toKLine(rawData) {
    var categoryData = [];
    var values = [];
    var volumes = [];
    for (var i = 0; i < rawData.length; i++) {
        categoryData.push(rawData[i].splice(0, 1)[0]);
        values.push(rawData[i]);
        volumes.push([i, rawData[i][7], rawData[i][0] > rawData[i][1] ? 1 : -1]);
    }

    return {
        categoryData: categoryData,
        values: values,
        volumes: volumes
    };
}

function splitData(myChart) {
    var data = toKLine(data0.values);

    myChart.setOption(option = {
        backgroundColor: '#fff',
        animation: false,
        title: {    //标题
            text: data0.stockName,
            left: 0
        },
        legend: {
            bottom: 10,
            left: 'center',
            data: ['日K', 'bPrice', 'cPrice', 'ZT']
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'cross'
            },
            backgroundColor: 'rgba(245, 245, 245, 0.8)',
            borderWidth: 1,
            borderColor: '#ccc',
            padding: 10,
            textStyle: {
                color: '#000'
            },
            position: function (pos, params, el, elRect, size) {
                var obj = { top: 10 };
                obj[['left', 'right'][+(pos[0] < size.viewSize[0] / 2)]] = 30;
                return obj;
            }
            // extraCssText: 'width: 170px'
        },
        axisPointer: {
            link: { xAxisIndex: 'all' },
            label: {
                backgroundColor: '#777'
            }
        },
        toolbox: {
            feature: {
                dataZoom: {
                    yAxisIndex: false
                },
                brush: {
                    type: ['lineX', 'clear']
                }
            }
        },
        brush: {
            xAxisIndex: 'all',
            brushLink: 'all',
            outOfBrush: {
                colorAlpha: 0.1
            }
        },
        visualMap: {
            show: false,
            seriesIndex: 5,
            dimension: 2,
            pieces: [{
                value: 1,
                color: downColor
            }, {
                value: -1,
                color: upColor
            }]
        },
        grid: [
            {
                left: '10%',
                right: '8%',
                height: '50%'
            },
            {
                left: '10%',
                right: '8%',
                top: '63%',
                height: '16%'
            }
        ],
        xAxis: [
            {
                type: 'category',
                data: data.categoryData,
                scale: true,
                boundaryGap: false,
                axisLine: { onZero: false },
                splitLine: { show: false },
                splitNumber: 20,
                min: 'dataMin',
                max: 'dataMax',
                axisPointer: {
                    z: 100
                }
            },
            {
                type: 'category',
                gridIndex: 1,
                data: data.categoryData,
                scale: true,
                boundaryGap: false,
                axisLine: { onZero: false },
                axisTick: { show: false },
                splitLine: { show: false },
                axisLabel: { show: false },
                splitNumber: 20,
                min: 'dataMin',
                max: 'dataMax'
            }
        ],
        yAxis: [
            {
                scale: true,
                splitArea: {
                    show: true
                }
            },
            {
                scale: true,
                gridIndex: 1,
                splitNumber: 2,
                axisLabel: { show: false },
                axisLine: { show: false },
                axisTick: { show: false },
                splitLine: { show: false }
            }
        ],
        dataZoom: [
            {
                type: 'inside',
                xAxisIndex: [0, 1],
                start: 98,
                end: 100
            },
            {
                show: true,
                xAxisIndex: [0, 1],
                type: 'slider',
                top: '85%',
                start: 98,
                end: 100
            }
        ],
        series: [
            {
                name: '日K',
                type: 'candlestick',
                data: data.values,
                itemStyle: {
                    color: upColor,
                    color0: downColor,
                    borderColor: null,
                    borderColor0: null
                },
                tooltip: {
                    formatter: function (param) {
                        param = param[0];
                        return [
                            'Date: ' + param.name + '<hr size=1 style="margin: 3px 0">',
                            'Open: ' + param.data[0] + '<br/>',
                            'Close: ' + param.data[1] + '<br/>',
                            'Lowest: ' + param.data[2] + '<br/>',
                            'Highest: ' + param.data[3] + '<br/>'
                        ].join('');
                    }
                }
            },
            {
                name: 'MA5',
                type: 'line',
                data: calculateMA(5, data),
                smooth: true,
                lineStyle: {
                    opacity: 0
                }
            },
            {
                name: 'bPrice',
                type: 'line',
                data: calculateBp(),
                smooth: true,
                lineStyle: {
                    opacity: 0.5
                }
            },
            {
                name: 'cPrice',
                type: 'line',
                data: calculateCp(),
                smooth: true,
                lineStyle: {
                    opacity: 0.5
                }
            },
            {
                name: 'ZT',
                type: 'line',
                data: calculateZT(),
                smooth: true,
                lineStyle: {
                    opacity: 0.5
                }
            },
            {
                name: 'Volume',
                type: 'bar',
                xAxisIndex: 1,
                yAxisIndex: 1,
                data: data.volumes
            }
        ]
    }, true);

    myChart.dispatchAction({
        type: 'brush',
        areas: [
            {
                brushType: 'lineX',
                coordRange: [data0.zeroDate, data0.coordDate],
                xAxisIndex: 0
            }
        ]
    });
}