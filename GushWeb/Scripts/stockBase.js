
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