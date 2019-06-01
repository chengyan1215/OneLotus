$(function () {
        var mobileAgent = new Array("iphone", "ipod", "ipad", "android", "mobile", "blackberry", "webos", "incognito", "webmate", "bada", "nokia", "lg", "ucweb", "skyfire");
        var browser = navigator.userAgent.toLowerCase();

        for (var i = 0; i < mobileAgent.length; i++) {
            if (browser.indexOf(mobileAgent[i]) != -1) {
                $("body").css("padding-top", "0")
                break;
            }
        }

        var Code = getQueryString("code");
        if (Code) {

            $.ajax({
                type: "get",
                url: "/API/VIEWAPI.ashx?Action=Commanage_GETHTML&P1=" + Code + "&r=" + Math.random(),
                success: function (data) {
                    var dart=JSON.parse(data);
                    if (dart.ErrorMsg == "") {
                        if (dart.Result1) {
                            $("<link>").attr({
                                rel: "stylesheet",
                                type: "text/css",
                                href: dart.Result1 + ".files/stylesheet.css"
                            }).appendTo("head");
                        }
                        if (dart.Result) {
                            var html = dart.Result.substring(dart.Result.indexOf("<body>") + 6, dart.Result.lastIndexOf("</body>"));
                            $("body").append(html);
                        }
                    }
                    else {
                        $("body").empty().append(dart.ErrorMsg);
                    }
                },
                error: function (data) {

                }
            })
        }
    //}
   
})

function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(decodeURI(r[2])); return null;
}

function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg))
        return unescape(arr[2]);
    else
        return null;
}