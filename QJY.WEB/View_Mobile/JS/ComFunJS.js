define = null;
require = null;

//微信预览图片
var myPhotoBrowserCaptions;
var urlData = [];

(function ($, window, undefined) {
    $.timeago = function (timestamp) {
        if (timestamp instanceof Date) {
            return inWords(timestamp);
        } else if (typeof timestamp === "string") {
            return inWords($.timeago.parse(timestamp));
        } else if (typeof timestamp === "number") {
            return inWords(new Date(timestamp));
        } else {
            return inWords($.timeago.datetime(timestamp));
        }
    };
    var $t = $.timeago;

    $.extend($.timeago, {
        settings: {
            refreshMillis: 60000,
            allowFuture: true,
            strings: {
                prefixAgo: null,
                prefixFromNow: "还有",
                suffixAgo: "前",
                suffixFromNow: null,
                seconds: "不到 1 分钟",
                minute: "大约 1 分钟",
                minutes: "%d 分钟",
                hour: "大约 1 小时",
                hours: "大约 %d 小时",
                day: "1 天",
                days: "%d 天",
                month: "大约 1 个月",
                months: "%d 个月",
                year: "大约 1 年",
                years: "%d 年",
                numbers: [],
                wordSeparator: ""
            }
        },
        inWords: function (distanceMillis) {
            var $l = this.settings.strings;
            var prefix = $l.prefixAgo;
            var suffix = $l.suffixAgo;
            if (this.settings.allowFuture) {
                if (distanceMillis < 0) {
                    prefix = $l.prefixFromNow;
                    suffix = $l.suffixFromNow;
                }
            }

            var seconds = Math.abs(distanceMillis) / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var days = hours / 24;
            var years = days / 365;

            function substitute(stringOrFunction, number) {
                var string = $.isFunction(stringOrFunction) ? stringOrFunction(number, distanceMillis) : stringOrFunction;
                var value = ($l.numbers && $l.numbers[number]) || number;
                return string.replace(/%d/i, value);
            }

            var words = seconds < 45 && substitute($l.seconds, Math.round(seconds)) ||
              seconds < 90 && substitute($l.minute, 1) ||
              minutes < 45 && substitute($l.minutes, Math.round(minutes)) ||
              minutes < 90 && substitute($l.hour, 1) ||
              hours < 24 && substitute($l.hours, Math.round(hours)) ||
              hours < 42 && substitute($l.day, 1) ||
              days < 30 && substitute($l.days, Math.round(days)) ||
              days < 45 && substitute($l.month, 1) ||
              days < 365 && substitute($l.months, Math.round(days / 30)) ||
              years < 1.5 && substitute($l.year, 1) ||
              substitute($l.years, Math.round(years));

            var separator = $l.wordSeparator === undefined ? " " : $l.wordSeparator;
            return $.trim([prefix, words, suffix].join(separator));
        },
        parse: function (iso8601) {
            var s = $.trim(iso8601);
            s = s.replace(/\.\d+/, ""); // remove milliseconds
            s = s.replace(/-/, "/").replace(/-/, "/");
            s = s.replace(/T/, " ").replace(/Z/, " UTC");
            s = s.replace(/([\+\-]\d\d)\:?(\d\d)/, " $1$2"); // -04:00 -> -0400
            return new Date(s);
        },
        datetime: function (elem) {
            var iso8601 = $t.isTime(elem) ? $(elem).attr("datetime") : $(elem).attr("title");
            return $t.parse(iso8601);
        },
        isTime: function (elem) {
            // jQuery's `is()` doesn't play well with HTML5 in IE
            return $(elem).get(0).tagName.toLowerCase() === "time"; // $(elem).is("time");
        }
    });

    $.fn.timeago = function () {
        var self = this;
        self.each(refresh);

        var $s = $t.settings;
        if ($s.refreshMillis > 0) {
            setInterval(function () { self.each(refresh); }, $s.refreshMillis);
        }
        return self;
    };

    function refresh() {
        var data = prepareData(this);
        if (!isNaN(data.datetime)) {
            $(this).text(inWords(data.datetime));
        }
        return this;
    }

    function prepareData(element) {
        element = $(element);
        if (!element.data("timeago")) {
            element.data("timeago", { datetime: $t.datetime(element) });
            var text = $.trim(element.text());
            if (text.length > 0 && !($t.isTime(element) && element.attr("title"))) {
                element.attr("title", text);
            }
        }
        return element.data("timeago");
    }

    function inWords(date) {
        return $t.inWords(distance(date));
    }

    function distance(date) {
        return (new Date().getTime() - date.getTime());
    }

    // fix for IE6 suckage
    document.createElement("abbr");
    document.createElement("time");
})(window.jQuery || window.Zepto, window);

var ComFunJS = {
    yuming: "",
    iswx: function () {
        var ua = navigator.userAgent.toLowerCase();
        if (ua.match(/MicroMessenger/i) == "micromessenger") {
            return true;
        } else {
            return false;
        }
    },
    getfileapi: function () {
        return ComFunJS.getCookie("fileapi");
    },
    trim: function (str) {
        return str.replace(/^,+/, "").replace(/,+$/, "");
    },
    loadJs: function (url, callback) {
        var done = false;
        var script = document.createElement('script');
        script.type = 'text/javascript';
        script.language = 'javascript';
        script.src = url;
        script.onload = script.onreadystatechange = function () {
            if (!done && (!script.readyState || script.readyState == 'loaded' || script.readyState == 'complete')) {
                done = true;
                script.onload = script.onreadystatechange = null;
                if (callback) {
                    callback.call(script);
                }
            }
        }
        document.getElementsByTagName("head")[0].appendChild(script);
    },
    seelp: function (numberMillis) {
        var now = new Date();
        var exitTime = now.getTime() + numberMillis;
        while (true) {
            now = new Date();
            if (now.getTime() > exitTime)
                return;
        }
    },
    getQueryString: function (name, defauval) {//获取URL参数,如果获取不到，返回默认值，如果没有默认值，返回空格
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) { return unescape(decodeURI(r[2])); }
        else {
            return defauval || "";
        }
    },//获取参数
    daysBetween: function (start, end) {
        var OneMonth = start.substring(5, start.lastIndexOf('-'));
        var OneDay = start.substring(start.length, start.lastIndexOf('-') + 1);
        var OneYear = start.substring(0, start.indexOf('-'));
        var TwoMonth = end.substring(5, end.lastIndexOf('-'));
        var TwoDay = end.substring(end.length, end.lastIndexOf('-') + 1);
        var TwoYear = end.substring(0, end.indexOf('-'));
        var cha = ((Date.parse(TwoMonth + '/' + TwoDay + '/' + TwoYear) - Date.parse(OneMonth + '/' + OneDay + '/' + OneYear)) / 86400000);
        return cha;
    },//比较日期
    AlertMsg: function (content) {
        layer.open({
            content: content,
            btn: ['确认']
        });
    },//弹框提示
    AlertMsg: function (content, callback) {
        layer.open({
            content: content,
            btn: ['确认'],
            yes: function () {
                return callback.call(this);
            },
        });
    },//弹框提示
    getfile: function (fileid) {
        var url = ComFunJS.yuming + "/ToolS/DownFile.aspx?szhlcode=" + ComFunJS.getCookie("szhlcode");
        if (fileid) {
            url = url + "&fileId=" + fileid;
        }
        return url;
    },
    confirm: function (content, callback) {
        var content = content || "确定要删除吗？";
        layer.open({
            content: content,
            btn: ['确定', '取消'],
            shadeClose: false,
            yes: function () {
                return callback.call(this);
            }, no: function () {
            }
        })
    },//弹框确认取消
    winconfirm: function (content, callbackyes, callbackno) {
        var content = content || "确定要删除吗？";
        var lyindex = layer.open({
            content: content,
            btn: ['确定', '取消'],
            shadeClose: false,
            yes: function () {
                layer.close(lyindex);
                return callbackyes.call(this);
            }, no: function () {
                return callbackno.call(this);
            }
        });
    },//弹框确认取消
    winsuccess: function (msg) {
        if (typeof (layer) != "undefined") {
            layer.open({
                content: msg,
                style: 'background-color:#09C1FF; color:#fff; border:none;',
                time: 2
            });
        }
    },//成功提示
    showload: function () {
        if (typeof (layer) != "undefined") {
            layer.open({
                type: 2
            });
        }
    },//成功提示
    winwarning: function (msg) {
        if (typeof (layer) != "undefined") {
            layer.open({
                content: msg,
                style: 'background-color:RED; color:#fff; border:none;',
                time: 2
            });
        }
    },//错误提示
    closeAll: function () {
        if (typeof (layer) != "undefined") {

            setTimeout("layer.closeAll()", 1500);
        }
    },//关闭所有弹框
    showComment: function (height, callback) {
        var height = height | "200";
        var pagei = layer.open({
            type: 1,
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment" autofocus="autofocus" placeholder="请输入评论" style="border:none;width:100%;height:120px;"></textarea></li><li class="ui-border-t" style="display: -webkit-box;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:90%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    if (comment) {
                        var plpoints = 0;
                        if ($('#pc').raty('score')) {
                            plpoints = parseInt($('#pc').raty('score')) * 20;
                        }

                        layer.close(pagei);
                        return callback.call(this, comment, plpoints);
                    }
                }
            }
        })
    },//评论评分编辑框有评分（无用）
    //评论
    //ispoint:true:带评分 false:不带评分
    showCommentNew: function (callback, ispoint, sObject, height) {
        var height = height | "200";
        var ispoint = ispoint || false;
        var type = "评论";
        var content = "";
        if (sObject) {
            type = sObject.type || "评论";
            content = sObject.content || "";
        }
        var html = '<div class="list-block" style="margin:0;"><ul><li class="item-content"><div class="item-inner">'
            + '<textarea id="ar_comment" style="height:140px" placeholder="请输入' + type + '">' + content + '</textarea></div></li><li class="item-content"><div class="item-inner">'
            + '<div class="item-title" style="-webkit-box-flex: 1;"><a class="imgBtn" href="javascript:void(0);" external></a></div><div class="item-title" style="-webkit-box-flex: 1;">';
        if (ispoint) {
            html += '<div id="pc" >评分：</div>';
        }
        html += '</div><div class="item-after"><button class="closediy button button-fill button-danger">取消</button><button class="closediycc button button-fill button-success" style="margin-left:10px;">确认</button></div></div></li></ul><div class="faceDiv" style="overflow:auto"></div></div>';
        var pagei = layer.open({
            type: 1,
            content: html,
            style: 'width:94%;height:' + height + 'px;',
            //fixed: true,
            fixed: false,
            top: '40',
            shadeClose: false,
            success: function (olayer) {
                if (ispoint) {
                    $("#pc").raty();//.layermmain .section
                }
                $(".faceDiv").hide();
                var isShowImg = false;
                $(".imgBtn").click(function () {
                    if (isShowImg == false) {
                        isShowImg = true;
                        //$(".faceDiv").animate({ marginTop: "-125px" }, 300);
                        //$(".faceDiv").attr("marginTop", "-125px");
                        $(".faceDiv").show();
                        if ($(".faceDiv").children().length == 0) {
                            for (var i = 0; i < ComFunJS.facePath.length; i++) {
                                $(".faceDiv").append("<img title=\"" + ComFunJS.facePath[i].faceName + "\" src=\"/ViewV5/Images/face/" + ComFunJS.facePath[i].facePath + "\" />");
                            }
                            $(".faceDiv>img").click(function () {
                                //$(this).parent().animate({ marginTop: "0px" }, 300);
                                //$(".faceDiv").attr("marginTop", "0px");
                                //$(".faceDiv").hide();
                                ComFunJS.insertAtCursor($("#ar_comment")[0], "[" + $(this).attr("title") + "]");
                            });
                        }
                    } else {
                        isShowImg = false;
                        //$(".faceDiv").animate({ marginTop: "0px" }, 300);
                        //$(".faceDiv").attr("marginTop", "0px");
                        $(".faceDiv").hide();
                    }
                });
                //$(".layermmain .section").css("vertical-align", "bottom");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    var plpoints = 0;
                    if (ispoint) {
                        if ($('#pc').raty('score')) {
                            plpoints = parseInt($('#pc').raty('score')) * 20;
                        }
                    }


                    if (comment) {

                        var rObject = { comment: comment, point: plpoints };
                        layer.close(pagei);
                        return callback.call(this, rObject);
                    }
                    else {
                        if (!comment) { ComFunJS.winwarning("请输入" + type) }
                        //if (ispoint && plpoints != 0) { ComFunJS.winwarning("请选择评分") }
                    }
                }
            }
        })
    },//编辑框
    showAnswer: function (height, callback) {
        var height = height | "360";
        var pagei = layer.open({
            type: 1,//<img id="1" src="" style="width:60px;height:60px;padding-right: 5px;"/>
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment" autofocus="autofocus" placeholder="请输入回复" style="border:none;width:100%;height:120px;"></textarea></li><li class="ui-border-t"><div style="margin-top: 5px;height:120px" ><ul id="imglist"><li class="imgxl scli"><img id="add_pit" src="/View_Mobile/Images/icon-add.png" /></li></ul><div style="clear:both"></div></div></li><li class="ui-border-t" ><div class="ui-btn-wrap" style="float:right;padding:0px;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:100%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                ComFunJS.uploadimg();
                $(".layermmain .section").css("vertical-align", "top");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    if (comment) {
                        var tp = '';

                        $("#imglist .tpli").each(function () {
                            if (tp) {
                                tp = tp + ',' + $(this).attr("itemid");
                            }
                            else {
                                tp = $(this).attr("itemid");
                            }
                        })
                        layer.close(pagei);
                        return callback.call(this, comment, tp);
                    }
                }
            }
        })
    },//回复编辑框有上传图片（无用）
    showPFBJK: function (height, content, pf, type, callback) {
        var height = height | "200";
        var pagei = layer.open({
            type: 1,
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment"  placeholder="请输入' + type + '" style="border:none;width:100%;height:120px;">' + content + '</textarea></li><li class="ui-border-t" style="display: -webkit-box;">评分：<div id="pc" style="padding-top: 5px;-webkit-box-flex: 1;" ></div><div class="ui-btn-wrap" style="float:right;padding:0px;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:100%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                $("#pc").raty({ score: pf * 1 / 20 });//.layermmain .section
                $(".layermmain .section").css("vertical-align", "top");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    if (comment) {
                        var plpoints = 0;
                        if ($('#pc').raty('score')) {
                            plpoints = parseInt($('#pc').raty('score')) * 20;
                        }

                        layer.close(pagei);
                        return callback.call(this, comment, plpoints);
                    }
                }
            }
        })
    },//评分编辑框有评分（无用）
    showBJK: function (height, content, type, callback, isbx) {
        var height = height | "200";
        var pagei = layer.open({
            type: 1,
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment"  placeholder="请输入' + type + '" style="border:none;width:100%;height:120px;">' + content + '</textarea></li><li class="ui-border-t" style="display: -webkit-box;"><div id="pc" style="padding-top: 5px;-webkit-box-flex: 1;" ></div><div class="ui-btn-wrap" style="float:right;padding:0px;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:100%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                $(".layermmain .section").css("vertical-align", "top");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    if (!isbx) { isbx = "Y"; }
                    if ((isbx == "Y" && comment) || isbx == "N") {
                        layer.close(pagei);
                        return callback.call(this, comment);
                    }
                }
            }
        })
    },//编辑框（无用）
    showSH: function (height, content, callback) {
        var height = height | "200";
        var pagei = layer.open({
            type: 1,
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment"  placeholder="请输入审核意见" style="border:none;width:100%;height:120px;">' + content + '</textarea></li><li class="ui-border-t" style="display: -webkit-box;"><div id="pc" style="padding-top: 5px;-webkit-box-flex: 1;" ></div><div class="ui-btn-wrap" style="float:right;padding:0px;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:100%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                $(".layermmain .section").css("vertical-align", "top");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    if (comment) {
                        layer.close(pagei);
                        return callback.call(this, comment);
                    }
                }
            }
        })
    },//审核编辑框（无用）
    showZSSH: function (height, content, callback) {
        var height = height | "200";
        var pagei = layer.open({
            type: 1,
            content: '<ul class="ui-list ui-list-pure"><li class="ui-border-t"><textarea id="ar_comment" autofocus="autofocus" placeholder="请输入审核意见" style="border:none;width:100%;height:120px;">同意</textarea></li><li class="ui-border-t" style="display: -webkit-box;">转审人：' + content + '<div class="ui-btn-wrap" style="float:right;padding:0px;"><button class="closediy ui-btn ui-btn-progress">取消</button><button class="closediycc ui-btn ui-btn-primary" style="margin-left:10px;">确认</button></div></li></ul>',
            style: 'width:100%;height:' + height + 'px;',
            fixed: true,
            shadeClose: false,
            success: function (olayer) {
                $(".layermmain .section").css("vertical-align", "top");
                olayer.getElementsByClassName('closediy')[0].onclick = function () {
                    layer.close(pagei);
                }
                olayer.getElementsByClassName('closediycc')[0].onclick = function () {
                    var comment = $("#ar_comment").val();
                    var zsr = $(".ui-list-pure").find("select").val();
                    if (zsr) {
                        layer.close(pagei);
                        return callback.call(this, comment, zsr);
                    }
                }
            }
        })
    },//转审编辑框有审核人（无用）
    getnowdate: function (format) {
        var now = new Date();

        var year = now.getFullYear();       //年
        var month = now.getMonth() + 1;     //月
        var day = now.getDate();            //日
        var hh = now.getHours();
        var mm = now.getMinutes();
        var ss = now.getSeconds();

        var clock = year + "-";

        if (format == "yyyy-mm") {
            if (month < 10)
                clock += "0";
            clock += month + "-";
        }

        if (format == "yyyy-mm-dd") {
            if (month < 10)
                clock += "0";
            clock += month + "-";
            if (day < 10) {
                clock += "0";
            }
            clock += day + "-";
        }
        if (format == "yyyy-mm-dd hh:mm") {
            if (month < 10)
                clock += "0";
            clock += month + "-";
            if (day < 10) {
                clock += "0";
            }
            clock += day + " ";

            if (hh < 10)
                clock += "0";
            clock += hh + ":";
            if (mm < 10)
                clock += "0";
            clock += mm + ":";

        }

        if (format == "yyyy-mm-dd-hh-mm") {
            if (month < 10)
                clock += "0";
            clock += month + "-";
            if (day < 10) {
                clock += "0";
            }
            clock += day + "-";

            //if (hh < 10)
            //    clock += "0";
            clock += hh + "-";
            if (mm < 10)
                clock += "0";
            clock += mm + "-";

        }
        if (format == "yyyy-mm-dd hh:mm:ss") {
            if (month < 10)
                clock += "0";
            clock += month + "-";
            if (day < 10) {
                clock += "0";
            }
            clock += day + " ";

            if (hh < 10)
                clock += "0";
            clock += hh + ":";
            if (mm < 10)
                clock += "0";
            clock += mm + ":";
            if (ss < 10)
                clock += "0";
            clock += ss + ":";

        }
        return (clock.substr(0, clock.length - 1));
    },//获取当前时间
    setCookie: function (name, value, t) {
        var Days = 30;
        var exp = new Date();
        if (t) {
            exp.setTime(exp.getTime() + parseInt(t) * 24 * 60 * 60 * 1000);
        } else {
            exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
        }
        document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString() + ";path=/";
    },//设置cookie
    getCookie: function (name) {
        var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");

        if (arr = document.cookie.match(reg))

            return unescape(arr[2]);
        else
            return null;
    },//获取cookie
    delCookie: function (name) {
        var exp = new Date();
        exp.setTime(exp.getTime() - 1);
        var cval = ComFunJS.getCookie(name);
        if (cval != null) {
            document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString() + ";path=/;";
        }
    },//删除cookie
    convstr: function (str, len) {
        str = str.replace(/&nbsp;/ig, "").replace(/<[^>]+>/g, "").replace(new RegExp('&quot;', 'gm'), '"');
        if (len) {
            return str.length > len ? str.substr(0, len) + "..." : str;
        }
        else {
            return str.length > 10 ? str.substr(0, 10) + "..." : str;
        }
    },//截取字符串
    compareDate: function (strdate1, strdate2) {
        var arr1 = strdate1.split("-");
        var time1 = new Date(arr1[0], arr1[1] - 1, arr1[2]);
        var times1 = time1.getTime();

        var times2 = 0;
        if (strdate2 == "") {
            times2 = new Date().getTime();
        }
        else {
            var arr2 = strdate2.split("-");
            var time2 = new Date(arr2[0], arr2[1] - 1, arr2[2]);
            times2 = time2.getTime();
        }

        if (times1 > times2) {
            return false;
        }
        else {
            return true;
        }
    },//比较日期（yyyy-MM-dd）
    compareTime: function (strdate1, strdate2) {
        var sarr1 = strdate1.split(" ");

        var arr1 = sarr1[0].split("-");
        var arr11 = sarr1[1].split(":");
        if (arr11.length < 3) {
            arr11.push("00");
        }
        var time1 = new Date(arr1[0], arr1[1] - 1, arr1[2], arr11[0], arr11[1], arr11[2]);
        var times1 = time1.getTime();

        var times2 = 0;
        if (strdate2 == "") {
            times2 = new Date().getTime();
        }
        else {
            var sarr2 = strdate2.split(" ");

            var arr2 = sarr2[0].split("-");
            var arr21 = sarr2[1].split(":");
            if (arr21.length < 3) {
                arr21.push("00");
            }
            var time2 = new Date(arr2[0], arr2[1] - 1, arr2[2], arr21[0], arr21[1], arr21[2]);
            times2 = time2.getTime();
        }

        if (times1 > times2) {
            return false;
        }
        else {
            return true;
        }
    },//比较时间（yyyy-MM-dd HH:mm:ss）

    getqjstatus: function (status, status1, isforbid) {
        var sts = "";
        if ((isforbid == "Y" && status1 == 1) || (isforbid == "N" && status == 1)) {
            sts = '1';

        } else if (status1 == -1 || status == -1) {
            sts = '2';
        } else {
            sts = '0';
        }
        return sts;
    },//获取请假状态（无用）
    initwxConfig: function () {
        if (ComFunJS.iswx()) {
            $.getJSON("/API/VIEWAPI.ashx?action=JSSDK_GETSIGNAGURE&r=" + Math.random(), { "P1": window.location.href }, function (r) {
                if (r.ErrorMsg == "") {
                    wx.config({
                        debug: false,
                        appId: r.Result.appId,
                        timestamp: r.Result.timestamp,
                        nonceStr: r.Result.noncestr,
                        signature: r.Result.signature,
                        jsApiList: [
                            'checkJsApi',
                            'openEnterpriseChat',
                            'openEnterpriseContact',
                            'onMenuShareTimeline',
                            'onMenuShareAppMessage',
                            'onMenuShareQQ',
                            'onMenuShareWeibo',
                            'onMenuShareQZone',
                            'startRecord',
                            'stopRecord',
                            'onVoiceRecordEnd',
                            'playVoice',
                            'pauseVoice',
                            'stopVoice',
                            'onVoicePlayEnd',
                            'uploadVoice',
                            'downloadVoice',
                            'chooseImage',
                            'previewImage',
                            'uploadImage',
                            'downloadImage',
                            'translateVoice',
                            'getNetworkType',
                            'openLocation',
                            'getLocation',
                            'hideOptionMenu',
                            'showOptionMenu',
                            'hideMenuItems',
                            'showMenuItems',
                            'hideAllNonBaseMenuItem',
                            'showAllNonBaseMenuItem',
                            'closeWindow',
                            'scanQRCode'
                        ]
                    })
                }
            })
        }

    },//微信插件初始化
    uploadimg: function () {
        $.getJSON("/API/VIEWAPI.ashx?action=JSSDK_GETSIGNAGURE&r=" + Math.random(), { "P1": window.location.href }, function (r) {
            if (r.ErrorMsg == "") {

                wx.config({
                    debug: false,
                    appId: r.Result.appId,
                    timestamp: r.Result.timestamp,
                    nonceStr: r.Result.noncestr,
                    signature: r.Result.signature,
                    jsApiList: [
                      'checkJsApi',
                      'onMenuShareTimeline',
                      'onMenuShareAppMessage',
                      'onMenuShareQQ',
                      'onMenuShareWeibo',
                      'hideMenuItems',
                      'showMenuItems',
                      'hideAllNonBaseMenuItem',
                      'showAllNonBaseMenuItem',
                      'translateVoice',
                      'startRecord',
                      'stopRecord',
                      'onRecordEnd',
                      'playVoice',
                      'pauseVoice',
                      'stopVoice',
                      'uploadVoice',
                      'downloadVoice',
                      'chooseImage',
                      'previewImage',
                      'uploadImage',
                      'downloadImage',
                      'getNetworkType',
                      'openLocation',
                      'getLocation',
                      'hideOptionMenu',
                      'showOptionMenu',
                      'closeWindow',
                      'scanQRCode',
                      'chooseWXPay',
                      'openProductSpecificView',
                      'addCard',
                      'chooseCard',
                      'openCard'
                    ]
                })

                if ($("#add_pit").length > 0) {
                    $("#add_pit").click(function () {
                        wx.chooseImage({
                            success: function (res) {
                                $(res.localIds).each(function (index) {

                                    //$("#imglist").append("<img id='img_" + index + "' src='" + res.localIds[index] + "' src_wx=''  />");


                                    wx.uploadImage({
                                        localId: res.localIds[index], // 需要上传的图片的本地ID，由chooseImage接口获得
                                        isShowProgressTips: 1,// 默认为1，显示进度提示
                                        success: function (s_res) {
                                            var serverId = s_res.serverId; // 返回图片的服务器端ID

                                            var $html = $('<li id="img_' + index + '" itemid="' + serverId + '" class="imgxl tpli wximg"><span style="background-image:url(' + res.localIds[index] + ');" class="tpxs"></span><i class="ui-icon-close-progress sctp"></i></li>');

                                            $html.insertBefore($("#imglist .scli"));
                                            $html.find(".sctp").bind("click", function () {
                                                $html.remove();
                                            })
                                        }
                                    });


                                })

                            }
                        });
                    })
                }


            }
        })
    },//上传图片（神皖）
    uploadimgnew: function (tpdata) {

        //微信选图片
        if ($(".wximgupload")) {

            var zhtml = '<div class="weui_uploader">'
                     + '    <div class="weui_uploader_bd">'
                     + '        <ul class="weui_uploader_files" id="imglist">'

                     + '        </ul>'
                     + '        <div class="weui_uploader_input_wrp">'
                     + '            <input class="weui_uploader_input" type="button" id="tpadd" >'
                     + '        </div>'
                     + '    </div>'
                     + '    <div style="clear:both"></div>'
                     + '</div>';
            $(zhtml).insertBefore($(".wximgupload")).find(".weui_uploader_input").click(function () {
                if (ComFunJS.iswx()) {
                    wx.chooseImage({
                        success: function (res) {
                            $(res.localIds).each(function (index) {
                                if (window.__wxjs_is_wkwebview) {
                                    wx.getLocalImgData({
                                        localId: res.localIds[index],
                                        success: function (resimg) {
                                            ComFunJS.seelp(1000)//不得不加，不然苹果没法上传一次性多张图片
                                            var localData = resimg.localData;// 返回图片本地数据
                                            wx.uploadImage({
                                                localId: res.localIds[index], // 需要上传的图片的本地ID，由chooseImage接口获得
                                                isShowProgressTips: 1,// 默认为1，显示进度提示
                                                success: function (s_res) {
                                                    var serverId = s_res.serverId; // 返回图片的服务器端ID
                                                    var $html = $('<li id="img_' + index + '" itemid="' + serverId + '" class="weui_uploader_file wximg mall_pcp tpli" onclick="ComFunJS.viewbigimg(this)" style="background-image:url(' + localData + ')" src="' + localData + '"><i><img src="/View_Mobile/Images/close2.png"></i></li>');
                                                    $html.appendTo($("#imglist"));
                                                    $html.find("i").bind("click", function (event) {
                                                        event.stopPropagation();
                                                        $html.remove();
                                                    })
                                                }
                                            });
                                            //var serverId = res.localIds[index]; 
                                            //var localData = resimg.localData;// 返回图片本地数据
                                            //var $html = $('<li id="img_' + index + '" itemid="' + serverId + '" class="weui_uploader_file wximg mall_pcp tpli" onclick="ComFunJS.viewbigimg(this)" style="background-image:url(' + localData + ')" src="' + localData + '"><i><img src="/View_Mobile/Images/close2.png"></i></li>');
                                            //$html.appendTo($("#imglist"));
                                            //$html.find("i").bind("click", function (event) {
                                            //    event.stopPropagation();
                                            //    $html.remove();
                                            //})
                                        }
                                    })
                                }
                                else {
                                    wx.uploadImage({
                                        localId: res.localIds[index], // 需要上传的图片的本地ID，由chooseImage接口获得
                                        isShowProgressTips: 1,// 默认为1，显示进度提示
                                        success: function (s_res) {
                                            var serverId = s_res.serverId; // 返回图片的服务器端ID
                                            var $html = $('<li id="img_' + index + '" itemid="' + serverId + '" class="weui_uploader_file wximg mall_pcp tpli" onclick="ComFunJS.viewbigimg(this)" style="background-image:url(' + res.localIds[index] + ')" src="' + res.localIds[index] + '"><i><img src="/View_Mobile/Images/close2.png"></i></li>');
                                            $html.appendTo($("#imglist"));
                                            $html.find("i").bind("click", function (event) {
                                                event.stopPropagation();
                                                $html.remove();
                                            })
                                        }
                                    });
                                }



                            })
                        }
                    });
                }
            })
            if (!ComFunJS.iswx()) {
                ComFunJS.upfilenowx($(".wximgupload").eq(0))
            }
            $(tpdata).each(function (index, ele) {
                var html = '';
                if (ComFunJS.xstp(ele.FileExtendName)) {
                    html = '<li itemid="' + ele.ID + '" class="weui_uploader_file mall_pcp tpli" onclick="ComFunJS.viewbigimg(this,1)" style="background-image:url(' + ComFunJS.getfile(ele.ID) + ')" src="' + ComFunJS.getfile(ele.ID) + '"><i><img src="/View_Mobile/Images/close2.png"></i></li>';
                }
                else {
                    html = '<li itemid="' + ele.ID + '" class="weui_uploader_file tpli" onclick="ComFunJS.viewfile(\'' + ele.YLUrl + '\')" style="background-image:url(/View_Mobile/Images/qywd/' + ele.FileExtendName + '.png)"><i><img src="/View_Mobile/Images/close2.png"></i></li>';
                }
                $html1 = $(html);
                $html1.appendTo($("#imglist")).find("i").bind("click", function (event) {
                    event.stopPropagation();
                    $(this).parent().remove();
                })
            })
        }
    },//上传图片
    upfilenowx: function (dom) {
        ComFunJS.loadJs(ComFunJS.getfileapi() + "/Web/qj_upload.js", function () {
            var $input = dom;
            var upload = null;
            var obj = {
                uploadButtton: 'tpadd',
                fileapiurl: ComFunJS.getfileapi(),
                usercode: "qycode",
                secret: "qycode",
                upinfo: "上传组件",
                width: "90%",
                left: "5%",
                webupconfig: {
                    fileNumLimit: 5,
                },
                closeupwin: function (fileData) {
                    var fjids = "";
                    $.getJSON('/API/VIEWAPI.ashx?Action=QYWD_ADDFILE', { "P1": fileData, "P2": 3 }, function (result) {
                        if (result.ErrorMsg == "") {
                            var fjdata = result.Result;//给filedata赋值,供页面使用
                            for (var i = 0; i < fjdata.length; i++) {
                                var $html = $('<li id="img_' + i + '" itemid="' + fjdata[i].ID + '" class="weui_uploader_file  mall_pcp tpli" onclick="ComFunJS.viewbigimg(this)" style="background-image:url(' + ComFunJS.getfile(fjdata[i].ID) + ')" src="' + ComFunJS.getfile(fjdata[i].ID) + '"><i><img src="/View_Mobile/Images/close2.png"></i></li>');
                                $html.appendTo($("#imglist"));
                                $html.find("i").bind("click", function (event) {
                                    event.stopPropagation();
                                    $html.remove();
                                })
                            }

                            $("#imglist .tpli").each(function () {
                                fjids = fjids + $(this).attr("itemid") + ",";
                            })
                            if (fjids.length > 0) {
                                fjids = fjids.substring(0, fjids.length - 1)
                            }
                            $input.val(fjids);
                        }
                    })

                }
            };
            upload = new QJUpload(obj);
        })
    },
    uploadimgnewbak: function (tpdata) {

        //微信选图片
        if ($(".wximgupload")) {
            var zhtml = '<div class="media-list">'
                     + '    <ul style="padding:0;" id="imglist">'
                     + '        <li class="imgxl scli">'
                     + '            <div class="item-media">'
                     + '                <img src="/View_Mobile/Images/icon-add.png" width="44">'
                     + '            </div>'
                     + '        </li>'
                     + '    </ul>'
                     + '    <div style="clear:both"></div>'
                     + '</div>';
            $(zhtml).insertBefore($(".wximgupload")).find(".scli").click(function () {
                wx.chooseImage({
                    success: function (res) {
                        $(res.localIds).each(function (index) {

                            wx.uploadImage({
                                localId: res.localIds[index], // 需要上传的图片的本地ID，由chooseImage接口获得
                                isShowProgressTips: 1,// 默认为1，显示进度提示
                                success: function (s_res) {
                                    var serverId = s_res.serverId; // 返回图片的服务器端ID

                                    var $html = $('<li id="img_' + index + '" itemid="' + serverId + '" class="imgxl tpli wximg"><div class="item-media"><img class="mall_pcp" onclick="ComFunJS.viewbigimg(this)" src="' + res.localIds[index] + '" style="width:44px;height:44px;"></div><span class="badge sctp"><img  src="/View_Mobile/Images/close.png" style="width:100%" /></span></li>');

                                    $html.insertBefore($("#imglist .scli"));
                                    $html.find(".sctp").bind("click", function () {
                                        $html.remove();
                                    })
                                }
                            });


                        })

                    }
                });
            })

            $(tpdata).each(function (index, ele) {
                var html = '';
                if (ComFunJS.xstp(ele.FileExtendName)) {
                    html = '<li itemid="' + ele.ID + '" class="imgxl tpli"><div class="item-media"><img class="mall_pcp" onclick="ComFunJS.viewbigimg(this,1)" src="' + ComFunJS.getfile(ele.ID) + '" style="width:44px;height:44px;"></div><span class="badge sctp"><img src="/View_Mobile/Images/close.png" style="width:100%" /></span></li>';
                }
                else {
                    html = '<li itemid="' + ele.ID + '" class="imgxl tpli"><div class="item-media"><img onclick="ComFunJS.viewfile(' + ele.YLUrl + ')" src="/View_Mobile/Images/qywd/' + ele.FileExtendName + '.png" onerror="javascript: this.src = \'/View_Mobile/Images/qywd/file.png\'" style="width:44px;height:44px;"></div><span class="badge sctp"><img src="/View_Mobile/Images/close.png" style="width:100%" /></span></li>';
                }
                $html1 = $(html);
                $html1.insertBefore($("#imglist .scli")).find(".sctp").bind("click", function () {
                    $(this).parent().remove();
                })
            })
        }
    },//上传图片
    openEntChat: function (usrname, groupname) {

        var groupname = groupname || "";
        if (usrname.indexOf(",") > 0) {
            if (!groupname) {
                groupname = usrname;
            }
            usrname = ComFunJS.replaceAll(usrname, ',', ';');
        }
        wx.openEnterpriseChat({
            userIds: usrname,    // 必填，参与会话的成员列表。格式为userid1;userid2;...，用分号隔开，最大限制为1000个。userid单个时为单聊，多个时为群聊。
            groupName: groupname,  // 必填，会话名称。单聊时该参数传入空字符串""即可。
            success: function (res) {
                // 回调
                //alert(JSON.stringify(res));
            },
            fail: function (res) {
                if (res.errMsg.indexOf('function not exist') > 0) {
                    alert('版本过低请升级')
                }
                else {
                    //alert(usrname + ',' + JSON.stringify(res));
                }
            }
        })
    },//打开微信聊天


    viewimg: function (tpdata) {
        var zhtml = '<div class="weui_uploader">'
                     + '    <div class="weui_uploader_bd">'
                     + '        <ul class="weui_uploader_files" id="imglist1">'
                     + '        </ul>'
                     + '    </div>'
                     + '    <div style="clear:both"></div>'
                     + '</div>';

        $(".viewimg").html(zhtml);
        $(tpdata).each(function (index, ele) {
            var html = '';
            if (ComFunJS.xstp(ele.FileExtendName)) {
                html = '<li itemid="' + ele.ID + '" class="weui_uploader_file mall_pcp" onclick="ComFunJS.viewbigimg(this,2)" style="background-image:url(' + ComFunJS.getfile(ele.ID) + ')" src="/ToolS/DownFile.aspx?fileId=' + ele.ID + '"></li>';
            }
            else {
                html = '<li itemid="' + ele.ID + '" class="weui_uploader_file" onclick="ComFunJS.viewfile(\'' + ele.YLUrl + '\')" style="background-image:url(/View_Mobile/Images/qywd/' + ele.FileExtendName + '.png)"></li>';
            }
            $html1 = $(html);
            $html1.appendTo($("#imglist1"));
        })

    },//获取用户对象
    viewimgbak: function (tpdata) {
        var zhtml = '<div class="media-list">'
                             + '    <ul style="padding:0;" id="imglist1">'
                             + '    </ul>'
                             + '    <div style="clear:both"></div>'
                             + '</div>';

        $(".viewimg").html(zhtml);
        $(tpdata).each(function (index, ele) {
            var html = '';
            if (ComFunJS.xstp(ele.FileExtendName)) {
                html = '<li itemid="' + ele.ID + '" class="imgxl tpli"><div class="item-media"><img class="mall_pcp" onclick="ComFunJS.viewbigimg(this,2)" src="' + ComFunJS.getfile(ele.ID) + '" style="width:44px;height:44px;"></div></li>';
            }
            else {
                html = '<li itemid="' + ele.ID + '" class="imgxl tpli" ><div class="item-media"><img onclick="ComFunJS.viewfile(' + ele.YLUrl + ')" src="/View_Mobile/Images/qywd/' + ele.FileExtendName + '.png" onerror="javascript: this.src = \'/View_Mobile/Images/qywd/file.png\'" style="width:44px;height:44px;"></div></li>';
            }
            $html1 = $(html);
            $html1.appendTo($("#imglist1"));
        })

    },//获取用户对象
    chooseUser: function () {

        $.getJSON("/API/VIEWAPI.ashx?action=JSSDK_GETSIGNAGURE&r=" + Math.random(), { "P1": window.location.href, "P2": 2 }, function (r) {
            if (r.ErrorMsg == "") {
                wx.config({
                    debug: false,
                    appId: r.Result.appId,
                    timestamp: r.Result.timestamp,
                    nonceStr: r.Result.noncestr,
                    signature: r.Result.signature,
                    jsApiList: [
                      'checkJsApi',
                      'onMenuShareTimeline',
                      'onMenuShareAppMessage',
                      'onMenuShareQQ',
                      'onMenuShareWeibo',
                      'hideMenuItems',
                      'showMenuItems',
                      'hideAllNonBaseMenuItem',
                      'showAllNonBaseMenuItem',
                      'translateVoice',
                      'startRecord',
                      'stopRecord',
                      'onRecordEnd',
                      'playVoice',
                      'pauseVoice',
                      'stopVoice',
                      'uploadVoice',
                      'downloadVoice',
                      'chooseImage',
                      'previewImage',
                      'uploadImage',
                      'downloadImage',
                      'getNetworkType',
                      'openLocation',
                      'getLocation',
                      'hideOptionMenu',
                      'showOptionMenu',
                      'closeWindow',
                      'scanQRCode',
                      'chooseWXPay',
                      'openProductSpecificView',
                      'addCard',
                      'chooseCard',
                      'openCard',
                      'openEnterpriseContact'
                    ]
                })

                var evalWXjsApi = function (jsApiFun) {
                    if (typeof WeixinJSBridge == "object" && typeof WeixinJSBridge.invoke == "function") {
                        jsApiFun();
                    } else {
                        document.attachEvent && document.attachEvent("WeixinJSBridgeReady", jsApiFun);
                        document.addEventListener && document.addEventListener("WeixinJSBridgeReady", jsApiFun);
                    }
                }

                var elList = document.querySelectorAll('.openEnterpriseContact_invoke');
                for (var i = 0; i < elList.length; i++) {
                    elList[i].onclick = function () {
                        var obj = $(this);
                        evalWXjsApi(function () {
                            WeixinJSBridge.invoke("openEnterpriseContact", {
                                "groupId": r.Result1.group_id,    // 必填，管理组权限验证步骤1返回的group_id
                                "timestamp": r.Result1.timestamp,    // 必填，管理组权限验证步骤2使用的时间戳
                                "nonceStr": r.Result1.noncestr,    // 必填，管理组权限验证步骤2使用的随机字符串
                                "signature": r.Result1.signature,  // 必填，管理组权限验证步骤2生成的签名
                                "params": {
                                    'departmentIds': [0],    // 非必填，可选部门ID列表（如果ID为0，表示可选管理组权限下所有部门）
                                    'tagIds': [0],    // 非必填，可选标签ID列表（如果ID为0，表示可选所有标签）
                                    'userIds': [],    // 非必填，可选用户ID列表
                                    'mode': 'multi',    // 必填，选择模式，single表示单选，multi表示多选
                                    'type': ['department', 'tag', 'user'],    // 必填，选择限制类型，指定department、tag、user中的一个或者多个
                                    'selectedDepartmentIds': [],    // 非必填，已选部门ID列表
                                    'selectedTagIds': [],    // 非必填，已选标签ID列表
                                    'selectedUserIds': [],    // 非必填，已选用户ID列表
                                },
                            }, function (res) {
                                //alert(res.err_msg);
                                if (res.err_msg.indexOf('function_not_exist') > 0) {
                                    alert('版本过低请升级');
                                } else if (res.err_msg.indexOf('openEnterpriseContact:fail') > 0) {
                                    return;
                                }
                                var result = JSON.parse(res.result);    // 返回字符串，开发者需自行调用JSON.parse解析
                                var selectAll = result.selectAll;     // 是否全选（如果是，其余结果不再填充）
                                if (!selectAll) {
                                    var selectedDepartmentList = result.departmentList;    // 已选的部门列表
                                    for (var i = 0; i < selectedDepartmentList.length; i++) {
                                        var department = selectedDepartmentList[i];
                                        var departmentId = department.id;    // 已选的单个部门ID
                                        var departemntName = department.name;    // 已选的单个部门名称
                                    }
                                    var selectedTagList = result.tagList;    // 已选的标签列表
                                    for (var i = 0; i < selectedTagList.length; i++) {
                                        var tag = selectedTagList[i];
                                        var tagId = tag.id;    // 已选的单个标签ID
                                        var tagName = tag.name;    // 已选的单个标签名称
                                    }
                                    var selectedUserList = result.userList;    // 已选的成员列表
                                    for (var i = 0; i < selectedUserList.length; i++) {
                                        var user = selectedUserList[i];
                                        var userId = user.id;    // 已选的单个成员ID
                                        var userName = user.name;    // 已选的单个成员名称
                                    }
                                    //alert(JSON.stringify(selectedUserList));

                                    $(obj).val(JSON.stringify(selectedUserList));
                                }
                            })
                        });
                    }


                }



            }

        })
    },//微信选择用户（无用）
    xstp: function (str) {
        var bl = false;
        var gs = 'jpg|jpeg|png|bmp|gif';
        var gss = gs.split('|');
        if (gss.indexOf(str) >= 0) {
            bl = true;
        }
        return bl;
    },//是否显示图片
    initForm: function () {
        //if ($(".wximgupload").length > 0) {  //微信上传图片,必须引用微信JS才起作用
        //    ComFunJS.uploadimgnew();
        //}
        if ($(".fileupload").length > 0) {   //普通文件上传，需要在当前页面添加控件<input type="file" class="fileupload" />
            ComFunJS.loadfile();
        }
        if ($(".szhl_getPeoples").length > 0) {

            ComFunJS.initpeo();
        }
        if ($(".szhl_getKH").length > 0) {
            ComFunJS.initkh();
        }
        if ($(".szhl_timeago")) {
            $(".szhl_timeago").timeago();
        }
        if ($(".szhl_ui_date")) {
            $(".szhl_ui_date").each(function () {
                if ($(this).attr("readonly") != "") {
                    var str = ComFunJS.getnowdate("yyyy-mm-dd");
                    if ($(this).val()) {
                        str = $(this).val();
                    }
                    $(this).calendar({
                        value: [str]
                    });

                    $(this).val(str)
                }
            })
        }
        return "";
    },//页面初始化
    initpeo: function () {
        peomodel.peopcode = "TEMP_INDEX_TXL";
        $(".szhl_getPeoples").each(function () {
            //初始化选人插件
            $(this).parent().find('.selpeo').remove();
            if ($(this).parent().find('.selpeo').length == 0) {
                var inputdom = $(this);
                var $peodiv;
                if (inputdom.val()) {
                    $peodiv = $('<div class="selpeo" style="margin-left: 5px;">' + ComFunJS.convusers(inputdom.val()) + '</div> ').attr("id", "dvname" + $(this).attr("id"));
                }
                else {
                    var msg = inputdom.parent().parent().find(".label").text();
                    $peodiv = $('<div class="color-gray selpeo" >请选择' + msg + '</div> ').attr("id", "dvname" + $(this).attr("id"));
                }
                $peodiv.bind('click', function () {
                    if (peomodel.peopcode != "TEMP_INDEX_TXL") {
                        peomodel.peopcode = "TEMP_INDEX_TXL";
                    }
                    peomodel.nowpeodomid = inputdom.attr("id");//保存当前选中控件的ID供选人插件回调
                    if (inputdom.hasClass("single")) {
                        peomodel.singleSelect = true;
                    }
                    else {
                        peomodel.singleSelect = false;
                    }
                    if (tempmodeltxl) {
                        tempmodeltxl.defSetUsers();
                    }
                    $("#pageindex1").hide();
                    $("#selpeople").show();//
                })
                $(this).parent().append($peodiv);
            }
        })
    },//微信选择用户插件
    initkh: function () {
        khmodel.khcode = "TEMP_INDEX_KH";
        $(".szhl_getKH").each(function () {
            //初始化选人插件
            $(this).parent().find('.selpeo').remove();
            if ($(this).parent().find('.selpeo').length == 0) {
                var inputdom = $(this);
                var $peodiv;
                if (inputdom.val() && inputdom.val() != '0') {
                    $peodiv = $('<div class="selpeo" >' + ComFunJS.convkh(inputdom.val()) + '</div> ').attr("id", "dvname" + $(this).attr("id"));
                }
                else {
                    var msg = inputdom.parent().parent().find(".label").text();
                    $peodiv = $('<div class="color-gray selpeo" >请选择' + msg + '</div> ').attr("id", "dvname" + $(this).attr("id"));
                }
                $peodiv.bind('click', function () {
                    khmodel.nowpeodomid = inputdom.attr("id");//保存当前选中控件的ID供选人插件回调
                    if (inputdom.hasClass("single")) {
                        khmodel.singleSelect = true;
                    }
                    else {
                        khmodel.singleSelect = false;
                    }
                    if (tempmodelkh) {
                        tempmodelkh.defSetUsers();
                    }
                    $("#pageindex1").hide();
                    $("#selkh").show();//
                })
                $(this).parent().append($peodiv);
            }
        })
    },//微信选择用户插件
    getnowuser: function () {
        return ComFunJS.getCookie("username");
    },//获取当前用户名
    getnowuserobj: function () {
        return ComFunJS.convuserobj(ComFunJS.getnowuser("username"));
    },//获取当前用户对象
    convuserobj: function (str) {
        var obj = {};
        if (str) {
            var users = JSON.parse(window.localStorage.getItem("alluserinfo"));
            $(users).each(function (index, ele) {
                if (ele.UserName == str) {
                    obj = ele;
                    return false;
                }
            });
        }
        return obj;
    },//获取用户对象
    convertuser: function (str) {
        return ComFunJS.convuser(str);
    },//获取用户中文名
    convuser: function (str) {
        var username = '';
        if (str) {
            var users = JSON.parse(window.localStorage.getItem("alluserinfo"));
            $(users).each(function (index, ele) {
                if (ele.UserName == str) {
                    username = ele.UserRealName;
                }
            });
        }
        return username;
    },//获取用户中文名
    convusers: function (str) {
        var username = '';
        if (str) {
            var users = JSON.parse(window.localStorage.getItem("alluserinfo"));
            $(users).each(function (index, ele) {
                $(str.split(',')).each(function (inx, el) {
                    if (ele.UserName == el) {
                        if (username) {
                            username = username + ',' + ele.UserRealName;
                        }
                        else {
                            username = ele.UserRealName;
                        }
                    }
                })

            });
        }
        return username;
    },//获取多个用户中文名
    convkh: function (str) {
        var username = '';
        if (str) {
            $.ajaxSettings.async = false;
            $.getJSON('/API/VIEWAPI.ashx?Action=CRM_GETKHGLMODEL', { P1: str }, function (resultData) {

                if (resultData.ErrorMsg == "") {
                    username = resultData.Result[0].KHName;

                }
            })
        }
        return username;
    },//获取客户名称
    loadfile: function (data) {
        $("#file").change(function () {
            ComFunJS.UploadLoad(3);
        });
        var html = '<button type="button" class="ui-border-l" >上传</button>';
        $(html).insertBefore($("#file")).click(function () {
            $("#file").trigger("click");
        })

        var html1 = '<ul class="ui-list filelist"></ul>'

        $(html1).insertAfter($("#file").parent());

        if (data) {
            $(data).each(function (index, ele) {
                var src = '../Images/qywd/' + ele.FileExtendName + '.png';
                if (ComFunJS.xstp(ele.FileExtendName)) {
                    src = ComFunJS.getfile(r.Result.ID);
                }
                var html2 = '<li class="ui-border-t" fileid=' + ele.ID + '  style="padding: 5px;margin: 3px; font-size: 14px; border: 1px solid #C8CACD;">'
                         + '  <div style="padding-right:10px">'
                         + '      <img  style="height:40px;width:40px" src="' + src + '" />'
                         + '  </div>'
                         + '  <div class="ui-list-info" ms-on-click="viewwxx(el)" style="padding:0;">'
                         + '      <h4 class="ui-nowrap">' + ele.Name + '.' + ele.FileExtendName + '</h4>'
                         + '      <p class="ui-nowrap">' + Math.round(ele.FileSize / 1024) + 'kb</p>'
                         + '  </div>'
                         + '  <div>'
                         + '      <a>'
                         + '         <img style="height: 20px; width: 20px; float: right; padding: 10px;" src="../Images/delete.png" />'
                         + '      </a>'
                         + '  </div>'
                         + '</li>';
                $(html2).appendTo($(".filelist")).find("a").click(function () {
                    $(this).parent().parent().remove();
                });
            })
        }
    },//初始化上传文件
    UploadLoad: function (str) {
        if (document.getElementById("file").files.length > 0) {
            var formData = new FormData();
            formData.append("upFile", document.getElementById("file").files[0]);
            $.ajax({
                url: "/API/VIEWAPI.ashx?ACTION=XTGL_UPLOADFILE&P1=" + str + "&r=" + Math.random(),
                type: "POST",
                data: formData,
                /**
                *必须false才会自动加上正确的Content-Type
                */
                contentType: false,
                /**
                * 必须false才会避开jQuery对 formdata 的默认处理
                * XMLHttpRequest会对 formdata 进行正确的处理
                */
                processData: false,
                success: function (result) {
                    var r = $.parseJSON(result);
                    if (r.ErrorMsg == "") {
                        document.getElementById("file").outerHTML = document.getElementById("file").outerHTML;
                        $("#file").change(function () {
                            ComFunJS.UploadLoad(str);
                        });
                        var rt = r.Result[0];
                        var src = '../Images/qywd/' + rt.FileExtendName + '.png';
                        if (ComFunJS.xstp(rt.FileExtendName)) {
                            src = ComFunJS.getfile(rt.ID);
                        }
                        var html = '<li class="ui-border-t" fileid=' + rt.ID + '  style="padding: 5px;margin: 3px; font-size: 14px; border: 1px solid #C8CACD;">'
                                 + '  <div style="padding-right:10px">'
                                 + '      <img  style="height:40px;width:40px" src="' + src + '" />'
                                 + '  </div>'
                                 + '  <div class="ui-list-info" ms-on-click="viewwxx(el)" style="padding:0;">'
                                 + '      <h4 class="ui-nowrap">' + rt.Name + '.' + rt.FileExtendName + '</h4>'
                                 + '      <p class="ui-nowrap">' + Math.round(rt.FileSize / 1024) + 'kb</p>'
                                 + '  </div>'
                                 + '  <div>'
                                 + '      <a>'
                                 + '         <img style="height: 20px; width: 20px; float: right; padding: 10px;" src="../Images/delete.png" />'
                                 + '      </a>'
                                 + '  </div>'
                                 + '</li>';
                        $(html).appendTo($(".filelist")).find("a").click(function () {
                            $(this).parent().parent().remove();
                        });
                    }
                    else {
                        $.tips({
                            content: "上传失败",
                            stayTime: 2000,
                            type: "warn"
                        })
                    }
                }
            });
        }
    },//上传文件
    viewbigimg: function (obj, str) {

        var imgobj = $(".mall_pcp");
        if (str == "1") {
            imgobj = $("#imglist .mall_pcp");
        }
        else if (str == "2") {
            imgobj = $("#imglist1 .mall_pcp")
        }
        var str = $(obj).attr("urlid");
        if (!str) {

            imgobj.each(function (index, ele) {
                if ($(ele).attr("src") && !$(ele).attr("urlid")) {
                    $(ele).attr("urlid", urlData.length);
                    urlData.push($(ele).attr("src"));
                }
            });
        }
        else {
            if (imgobj.length > urlData.length) {
                imgobj.each(function (index, ele) {
                    if ($(ele).attr("src") && !$(ele).attr("urlid")) {
                        $(ele).attr("urlid", urlData.length);
                        urlData.push($(ele).attr("src"));
                    }
                });
            }
        }

        myPhotoBrowserCaptions = $.photoBrowser({
            photos: urlData,
            theme: 'dark'
        });

        myPhotoBrowserCaptions.open($(obj).attr("urlid") * 1);
    },//放大图片
    viewfile: function (YLUrl) { //微信预览文件
        if (YLUrl) {
            window.location = YLUrl;
        }
    },//预览文件
    isOffice: function (extname) {
        return $.inArray(extname.toLowerCase(), ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx', 'pdf']) != -1
    },//是否预览
    isPic: function (extname) {
        return $.inArray(extname.toLowerCase(), ['jpg', 'jpeg', 'gif', 'png', 'bmp']) != -1
    },//是否图片
    replaceAll: function (source, s1, s2) {
        return source.replace(new RegExp(s1, 'gm'), s2);
    },//替换所有的
    facePath: [//表情json
                { faceName: "微笑", facePath: "0_微笑.gif" },
                { faceName: "撇嘴", facePath: "1_撇嘴.gif" },
                { faceName: "色", facePath: "2_色.gif" },
                { faceName: "发呆", facePath: "3_发呆.gif" },
                { faceName: "得意", facePath: "4_得意.gif" },
                { faceName: "流泪", facePath: "5_流泪.gif" },
                { faceName: "害羞", facePath: "6_害羞.gif" },
                { faceName: "闭嘴", facePath: "7_闭嘴.gif" },
                { faceName: "睡", facePath: "8_睡.gif" },
                { faceName: "大哭", facePath: "9_大哭.gif" },
                { faceName: "尴尬", facePath: "10_尴尬.gif" },
                { faceName: "发怒", facePath: "11_发怒.gif" },
                { faceName: "调皮", facePath: "12_调皮.gif" },
                { faceName: "龇牙", facePath: "13_龇牙.gif" },
                { faceName: "惊讶", facePath: "14_惊讶.gif" },
                { faceName: "难过", facePath: "15_难过.gif" },
                { faceName: "酷", facePath: "16_酷.gif" },
                { faceName: "冷汗", facePath: "17_冷汗.gif" },
                { faceName: "抓狂", facePath: "18_抓狂.gif" },
                { faceName: "吐", facePath: "19_吐.gif" },
                { faceName: "偷笑", facePath: "20_偷笑.gif" },
                { faceName: "可爱", facePath: "21_可爱.gif" },
                { faceName: "白眼", facePath: "22_白眼.gif" },
                { faceName: "傲慢", facePath: "23_傲慢.gif" },
                { faceName: "饥饿", facePath: "24_饥饿.gif" },
                { faceName: "困", facePath: "25_困.gif" },
                { faceName: "惊恐", facePath: "26_惊恐.gif" },
                { faceName: "流汗", facePath: "27_流汗.gif" },
                { faceName: "憨笑", facePath: "28_憨笑.gif" },
                { faceName: "大兵", facePath: "29_大兵.gif" },
                { faceName: "奋斗", facePath: "30_奋斗.gif" },
                { faceName: "咒骂", facePath: "31_咒骂.gif" },
                { faceName: "疑问", facePath: "32_疑问.gif" },
                { faceName: "嘘", facePath: "33_嘘.gif" },
                { faceName: "晕", facePath: "34_晕.gif" },
                { faceName: "折磨", facePath: "35_折磨.gif" },
                { faceName: "衰", facePath: "36_衰.gif" },
                { faceName: "骷髅", facePath: "37_骷髅.gif" },
                { faceName: "敲打", facePath: "38_敲打.gif" },
                { faceName: "再见", facePath: "39_再见.gif" },
                { faceName: "擦汗", facePath: "40_擦汗.gif" },

                { faceName: "抠鼻", facePath: "41_抠鼻.gif" },
                { faceName: "鼓掌", facePath: "42_鼓掌.gif" },
                { faceName: "糗大了", facePath: "43_糗大了.gif" },
                { faceName: "坏笑", facePath: "44_坏笑.gif" },
                { faceName: "左哼哼", facePath: "45_左哼哼.gif" },
                { faceName: "右哼哼", facePath: "46_右哼哼.gif" },
                { faceName: "哈欠", facePath: "47_哈欠.gif" },
                { faceName: "鄙视", facePath: "48_鄙视.gif" },
                { faceName: "委屈", facePath: "49_委屈.gif" },
                { faceName: "快哭了", facePath: "50_快哭了.gif" },
                { faceName: "阴险", facePath: "51_阴险.gif" },
                { faceName: "亲亲", facePath: "52_亲亲.gif" },
                { faceName: "吓", facePath: "53_吓.gif" },
                { faceName: "可怜", facePath: "54_可怜.gif" },
                { faceName: "菜刀", facePath: "55_菜刀.gif" },
                { faceName: "西瓜", facePath: "56_西瓜.gif" },
                { faceName: "啤酒", facePath: "57_啤酒.gif" },
                { faceName: "篮球", facePath: "58_篮球.gif" },
                { faceName: "乒乓", facePath: "59_乒乓.gif" },
                { faceName: "拥抱", facePath: "78_拥抱.gif" },
                { faceName: "握手", facePath: "81_握手.gif" }
    ],//编辑框表情对象
    insertAtCursor: function (myField, myValue) {
        if (document.selection) {
            myField.focus();
            sel = document.selection.createRange();
            sel.text = myValue;
            sel.select();
        } else if (myField.selectionStart || myField.selectionStart == "0") {
            var startPos = myField.selectionStart;
            var endPos = myField.selectionEnd;
            var restoreTop = myField.scrollTop;
            myField.value = myField.value.substring(0, startPos) + myValue + myField.value.substring(endPos, myField.value.length);
            if (restoreTop > 0) {
                myField.scrollTop = restoreTop;
            }
            myField.focus();
            myField.selectionStart = startPos + myValue.length;
            myField.selectionEnd = startPos + myValue.length;
        } else {
            myField.value += myValue;
            //myField.focus();
        }
    },//编辑框插入表情时操作
    bqhContent: function (str) {
        if (str) {
            var regx = /(\[[\u4e00-\u9fa5]*\w*\]){1}/g;
            var rs = str.match(regx);
            if (rs) {
                for (i = 0; i < rs.length; i++) {
                    for (n = 0; n < ComFunJS.facePath.length; n++) {
                        if (ComFunJS.facePath[n].faceName == rs[i].substring(1, rs[i].length - 1)) {
                            var t = "<img title=\"" + ComFunJS.facePath[n].faceName + "\" style='display: initial;width: auto !important;' src=\"/ViewV5/Images/face/" + ComFunJS.facePath[n].facePath + "\" />";
                            str = str.replace(rs[i], t);
                            break;
                        }
                    }
                }
            }
        }
        return str;
    },//表情化内容
    converfilesize: function (size) {
        if (size) {
            var size = parseFloat(size);
            var rank = 0;
            var rankchar = 'Bytes';
            while (size > 1024) {
                size = size / 1024;
                rank++;
            }
            if (rank == 1) {
                rankchar = "KB";
            }
            else if (rank == 2) {
                rankchar = "MB";
            }
            else if (rank == 3) {
                rankchar = "GB";
            }
            return size.toFixed(2) + " " + rankchar;
        }

    },
    initsetajax: function (isload) { /// 配置AJAX
        $(document).on('ajaxBeforeSend', function (e, xhr, options) {
            var code = ComFunJS.getCookie("szhlcode");
            options.url = options.url + "&szhlcode=" + code
        })
        $(document).on('ajaxStart', function () {
            if (!isload) {
                ComFunJS.showload()
            }
        })
        $(document).on('ajaxSuccess', function (e, jqXHR, s, data) {
            try {
                if (s.type == "POST") {
                    data = $.parseJSON(data);
                }
                if (s.type == "GET") {
                    data = data;
                }
                if (data.ErrorMsg == "NOSESSIONCODE" || data.ErrorMsg == "WXTIMEOUT") {
                    ComFunJS.winwarning("未登录或登录已过期");
                }
                else if (data.ErrorMsg != "") {
                    ComFunJS.winwarning(data.ErrorMsg);
                }
            } catch (e) {
                ComFunJS.closeAll()
            }
        })
        $(document).on('ajaxStop', function () {
            ComFunJS.closeAll()
        })
        $(document).on('ajaxError', function (event, xhr, options, exc) {
            ComFunJS.winwarning("请求失败！")
        })
    },
}

; (function ($) {
    $.extend($, {
        getJSON: function (url, data, success, opt) {
            data.szhlcode = ComFunJS.getCookie("szhlcode");
            var fn = {
                success: function (data, textStatus) { }
            }
            if (success) {
                fn.success = success;
            }
            $.ajax({
                url: url + "&szhlcode=" + data.szhlcode,
                data: data,
                dataType: "json",
                type: "post",
                success: function (data, textStatus) {
                    if (data.ErrorMsg == "NOSESSIONCODE" || data.ErrorMsg == "WXTIMEOUT") {
                        top.ComFunJS.winwarning("页面超时!")
                        return;
                    }
                    if (data.ErrorMsg) {
                        top.ComFunJS.winwarning(data.ErrorMsg)
                    }
                    fn.success(data, textStatus);
                },

                beforeSend: function (XHR) {
                    //ComFunJS.showload()
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    ComFunJS.winwarning("请求失败！")
                },
                complete: function (XHR, TS) {
                    ComFunJS.closeAll()
                }
            });
        }
    });
})(Zepto);
//ComFunJS.initsetajax();

