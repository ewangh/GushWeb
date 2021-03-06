﻿
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
                        }).css({ "border": item.IsCheck ? "solid" : "" });
                    var span_tab = $('<span>',
                        {
                            text: " "
                        });
                    $("#" + mformId).append(a_rise).append(span_tab);
                });

        }
    });
}

function getQueryVariable(variable) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == variable) { return pair[1]; }
    }
    return (false);
}

function enableShiftCheck(checkboxs) {
    let startChecked;
    function handleCheck(e) {
        if (e.shiftKey) {
            let thisIndex = checkboxs.index(this);
            let startIndex = checkboxs.index(startChecked);
            let startNum = thisIndex < startIndex ? thisIndex : startIndex;
            let endNum = thisIndex > startIndex ? thisIndex : startIndex;
            for (let i = startNum; i <= endNum; i++) {
                if (this.checked) {
                    checkboxs.eq(i).prop("checked", true);
                } else {
                    checkboxs.eq(i).prop("checked", false);
                }
            }
        }
        startChecked = this;
    }
    checkboxs.click(handleCheck);
}

function sleep(time) {
    return new Promise((resolve) => setTimeout(resolve, time));
}