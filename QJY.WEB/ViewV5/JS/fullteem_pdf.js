$(function () {
        var Code = getQueryString("code");


        if (Code) {
            $.getJSON("/API/VIEWAPI.ashx?Action=Commanage_GETWDZY&r=" + Math.random(), { "P1": Code }, function (r) {

                if (r.ErrorMsg == "") {
                    if (r.Result && r.Result1 && r.Result2) {
                        $(".lnk-file-title").attr('TITLE', r.Result2);
                        $(".lnk-file-title").text(r.Result2);
                        for (var i = 0; i < r.Result1 * 1; i++) {
                            var html = '<DIV class="container-fluid container-fluid-content">'
                            + '<DIV class="row-fluid">'
                            + '<DIV class="span12">'
                            + '<DIV class="word-page" STYLE="max-width:793px">'
                            + '<DIV class="word-content">'
                            + '<img data-original="' + r.Result + '/' + (i + 1) + '.png" width="100%" height="100%">'
                            + '</DIV></DIV></DIV></DIV></DIV>';
                            $("body").append(html);
                        }

                        $("img").lazyload({
                            placeholder: "/images/loading.gif",
                            effect: "fadeIn"
                        });
                    }
                }
                else {
                    $("body").empty().append(r.ErrorMsg);
                }
            })

        }
    //}
})

function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg))
        return unescape(arr[2]);
    else
        return null;
}

function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(decodeURI(r[2])); return null;
}

//setCookie = function (nm, val, y) {
//    var exp = ''; if (y) {
//        var dt = new Date(); dt.setTime(dt.getTime() + (y * 86400000)); exp = '; expires=' + dt.toGMTString();
//    }
//    document.cookie = nm + '=' + escape(val) + exp + ';path=/;domain=ithome.com';
//}
//getCookie = function (nm) {
//    var m = ''; if (window.RegExp)
//    {
//        var re = new RegExp(';\\s*' + nm + '=([^;]*)', 'i'); m = re.exec(';' + document.cookie);
//    }
//    return (m ? unescape(m[1]) : '');
//}