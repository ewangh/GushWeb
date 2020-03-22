﻿function getKLine(action, mformId, chartId) {
    $.postJson(action, $("#"+mformId).serialize(), function (result) {
        var categoryData = [];
        var values = [];
        var stockName;
        $.each(result, function (i, item) {
            var fruits = [item.Date, item.Opening, item.Price, item.Lower, item.Highest, item.Closed, item.bPrice, item.cPrice];
            categoryData.push(fruits.splice(0, 1)[0]);
            //数据数组，即数组中除日期外的数据
            //alert(rawData[i]);
            values.push(fruits);
            stockName = item.Name
        });
        window.data0 = {
            stockName: stockName,
            categoryData: categoryData, //数组中的日期 x轴对应的日期
            values: values              //数组中的数据 y轴对应的数据
        };
        var myChart = echarts.init(document.getElementById(chartId));
        splitData(myChart);
    });
}

//计算MA平均线，N日移动平均线=N日收盘价之和/N  dayCount要计算的天数(5,10,20,30)
function calculateMA(dayCount) {
    var result = [];
    for (var i = 0, len = data0.values.length; i < len; i++) {
        if (i < dayCount) {
            result.push('-');
            //alert(result);
            continue;   //结束单次循环，即不输出本次结果
        }
        var sum = 0;
        for (var j = 0; j < dayCount; j++) {
            //收盘价总和
            sum += data0.values[i - j][1];
            //alert(sum);
        }
        result.push((sum / dayCount).toFixed(2));
        // alert(result);
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

//function calculateOp() {
//    var result = [];
//    for (var i = 0, len = data0.values.length; i < len; i++) {
//        result.push(data0.values[i][0]);
//        // alert(result);
//    }
//    return result;
//}

//function calculateLp() {
//    var result = [];
//    for (var i = 0, len = data0.values.length; i < len; i++) {
//        result.push(data0.values[i][2]);
//        // alert(result);
//    }
//    return result;
//}

//function calculateHp() {
//    var result = [];
//    for (var i = 0, len = data0.values.length; i < len; i++) {
//        result.push(data0.values[i][3]);
//        // alert(result);
//    }
//    return result;
//}

//function calculateCl() {
//    var result = [];
//    for (var i = 0, len = data0.values.length; i < len; i++) {
//        result.push(data0.values[i][1]);
//        // alert(result);
//    }
//    return result;
//}

function splitData(myChart) {
    option = {
        title: {    //标题
            text: data0.stockName,
            left: 0
        },
        tooltip: {  //提示框
            trigger: 'axis',    //触发类型：坐标轴触发
            axisPointer: {  //坐标轴指示器配置项
                type: 'cross'   //指示器类型，十字准星
            }
        },
        legend: {   //图例控件，点击图例控制哪些系列不现实
            data: ['日K', 'MA5', 'MA10', 'MA20', 'MA30', 'cP', 'bP']//, 'ZT', 'oP', 'lP', 'cL', 'hP'
        },
        grid: {     //直角坐标系
            show: true,
            left: '10%',    //grid组件离容器左侧的距离
            right: '10%',
            bottom: '15%',
            //backgroundColor:'#ccc'
        },
        xAxis: {
            type: 'category',   //坐标轴类型，类目轴
            data: data0.categoryData,
            //scale: true,  //只在数字轴中有效
            boundaryGap: false,    //刻度作为分割线，标签和数据点会在两个刻度上
            axisLine: { onZero: false },
            splitLine: { show: false },   //是否显示坐标轴轴线
            //splitNumber: 20,    //坐标轴的分割段数，预估值，在类目轴中无效
            min: 'dataMin', //特殊值，数轴上的最小值作为最小刻度
            max: 'dataMax'  //特殊值，数轴上的最大值作为最大刻度
        },
        yAxis: {
            scale: true,    //坐标刻度不强制包含零刻度
            splitArea: {
                show: true  //显示分割区域
            }
        },
        dataZoom: [     //用于区域缩放
            {
                filterMode: 'filter',    //当前数据窗口外的数据被过滤掉来达到数据窗口缩放的效果  默认值filter
                type: 'inside', //内置型数据区域缩放组件
                start: 50,      //数据窗口范围的起始百分比
                end: 100        //数据窗口范围的结束百分比
            },
            {
                show: true,
                type: 'slider', //滑动条型数据区域缩放组件
                y: '90%',
                start: 50,
                end: 100
            }
        ],
        series: [   //图表类型
            {
                name: '日K',
                type: 'candlestick',    //K线图
                data: data0.values,     //y轴对应的数据

                ////////////////////////图标标注/////////////////////////////

                markPoint: {    //图表标注
                    label: {    //标注的文本
                        normal: {   //默认不显示标注
                            show: true,
                            //position:['20%','30%'],
                            formatter: function (param) {   //标签内容控制器
                                return param != null ? Math.round(param.value) : '';
                            }
                        }
                    },
                    data: [     //标注的数据数组
                        {
                            name: 'XX标点',
                            coord: ['2013/5/31', 2300], //指定数据的坐标位置
                            value: 2300,
                            itemStyle: {    //图形样式
                                normal: { color: 'rgb(41,60,85)' }
                            }
                        },
                        //{
                        //    name: 'highest value',
                        //    type: 'max',    //最大值
                        //    valueDim: 'highest'     //在highest维度上的最大值 最高价
                        //},
                        //{
                        //    name: 'lowest value',
                        //    type: 'min',
                        //    valueDim: 'lowest'  //最低价
                        //},
                        //{
                        //    name: 'average value on close',
                        //    type: 'average',
                        //    valueDim: 'close'   //收盘价
                        //}
                    ],
                    tooltip: {      //提示框
                        formatter: function (param) {
                            return param.name + '<br>' + (param.data.coord || '');
                        }
                    }
                },

                /////////////////////////////////图标标线///////////////////////////

                markLine: {
                    symbol: ['none', 'none'],   //标线两端的标记类型
                    data: [
                        [
                            {
                                name: 'from lowest to highest',
                                type: 'min',    //设置该标线为最小值的线
                                valueDim: 'lowest', //指定在哪个维度上的最小值
                                symbol: 'circle',
                                symbolSize: 10, //起点标记的大小
                                label: {    //normal默认，emphasis高亮
                                    normal: { show: false },  //不显示标签
                                    emphasis: { show: false } //不显示标签
                                }
                            },
                            {
                                type: 'max',
                                valueDim: 'highest',
                                symbol: 'circle',
                                symbolSize: 10,
                                label: {
                                    normal: { show: false },
                                    emphasis: { show: false }
                                }
                            },
                        ],

                        {
                            name: 'min line on close',
                            type: 'min',
                            valueDim: 'close'
                        },
                        {
                            name: 'max line on close',
                            type: 'max',
                            valueDim: 'close'
                        },
                    ]

                }

            },

            //{   //MA5 5天内的收盘价之和/5
            //    name: 'MA5',
            //    type: 'line',
            //    data: calculateMA(5),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'MA10',
            //    type: 'line',
            //    data: calculateMA(10),
            //    smooth: true,
            //    lineStyle: {    //标线的样式
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'MA20',
            //    type: 'line',
            //    data: calculateMA(20),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'MA30',
            //    type: 'line',
            //    data: calculateMA(30),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            {
                name: 'cP',
                type: 'line',
                data: calculateCp(),
                smooth: true,
                lineStyle: {
                    normal: { opacity: 0.5 }
                }
            },
            {
                name: 'bP',
                type: 'line',
                data: calculateBp(),
                smooth: true,
                lineStyle: {
                    normal: { opacity: 0.5 }
                }
            },
            {
                name: 'ZT',
                type: 'line',
                data: calculateZT(),
                smooth: true,
                lineStyle: {
                    normal: { opacity: 0.5 }
                }
            },
            //{
            //    name: 'oP',
            //    type: 'line',
            //    data: calculateOp(),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'lP',
            //    type: 'line',
            //    data: calculateLp(),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'cL',
            //    type: 'line',
            //    data: calculateCl(),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
            //{
            //    name: 'hP',
            //    type: 'line',
            //    data: calculateHp(),
            //    smooth: true,
            //    lineStyle: {
            //        normal: { opacity: 0.5 }
            //    }
            //},
        ]
    };
    // 使用刚指定的配置项和数据显示图表
    myChart.setOption(option);
}