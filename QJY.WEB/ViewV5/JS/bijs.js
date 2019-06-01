
var ComFunJS = new Object({
    getQueryString: function (name, defauval) {//获取URL参数,如果获取不到，返回默认值，如果没有默认值，返回空格
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) { return unescape(r[2]); }
        else {
            return defauval || "";
        }
    },//获取参数中数据
    setCookie: function (name, value) {
        var Days = 30;
        var exp = new Date();
        exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
        document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString() + ";path=/";
    },
    getCookie: function (name) {
        var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");

        if (arr = document.cookie.match(reg))

            return unescape(arr[2]);
        else
            return null;
    },
    isPC: function () {
        var userAgentInfo = navigator.userAgent;
        var Agents = ["Android", "iPhone",
                    "SymbianOS", "Windows Phone",
                    "iPad", "iPod"];
        var flag = true;
        for (var v = 0; v < Agents.length; v++) {
            if (userAgentInfo.indexOf(Agents[v]) > 0) {
                flag = false;
                break;
            }
        }
        return flag;
    },
    loadScriptString: function (code) {
        var script = document.createElement("script");
        script.type = "text/javascript";
        try {
            // firefox、safari、chrome和Opera
            script.appendChild(document.createTextNode(code));
        } catch (ex) {
            // IE早期的浏览器 ,需要使用script的text属性来指定javascript代码。
            script.text = code;
        }
        document.getElementsByTagName("head")[0].appendChild(script);
    },
    JSONToExcelConvertor: function (JSONData, FileName, ShowLabel) {

        var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;

        var excel = '<table>';

        //设置表头
        var row = "<tr>";
        for (var i = 0, l = ShowLabel.length; i < l; i++) {
            row += "<td>" + ShowLabel[i].value + '</td>';
        }

        //换行
        excel += row + "</tr>";

        //设置数据
        for (var i = 0; i < arrData.length; i++) {
            var row = "<tr>";

            for (var j = 0; j < arrData[i].length; j++) {
                var value = arrData[i][j].value === "." ? "" : arrData[i][j].value;
                row += '<td>' + value + '</td>';
            }

            excel += row + "</tr>";
        }

        excel += "</table>";

        var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>";
        excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">';
        excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel';
        excelFile += '; charset=UTF-8">';
        excelFile += "<head>";
        excelFile += "<!--[if gte mso 9]>";
        excelFile += "<xml>";
        excelFile += "<x:ExcelWorkbook>";
        excelFile += "<x:ExcelWorksheets>";
        excelFile += "<x:ExcelWorksheet>";
        excelFile += "<x:Name>";
        excelFile += "{worksheet}";
        excelFile += "</x:Name>";
        excelFile += "<x:WorksheetOptions>";
        excelFile += "<x:DisplayGridlines/>";
        excelFile += "</x:WorksheetOptions>";
        excelFile += "</x:ExcelWorksheet>";
        excelFile += "</x:ExcelWorksheets>";
        excelFile += "</x:ExcelWorkbook>";
        excelFile += "</xml>";
        excelFile += "<![endif]-->";
        excelFile += "</head>";
        excelFile += "<body>";
        excelFile += excel;
        excelFile += "</body>";
        excelFile += "</html>";


        var uri = 'data:application/vnd.ms-excel;charset=utf-8,' + encodeURIComponent(excelFile);

        var link = document.createElement("a");
        link.href = uri;

        link.style = "visibility:hidden";
        link.download = FileName + ".xls";

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },
    DateAdd: function (date, strInterval, Number) {
        var dtTmp = date;
        switch (strInterval) {
            case 's': return new Date(Date.parse(dtTmp) + (1000 * Number));

            case 'n': return new Date(Date.parse(dtTmp) + (60000 * Number));

            case 'h': return new Date(Date.parse(dtTmp) + (3600000 * Number));

            case 'd': return new Date(Date.parse(dtTmp) + (86400000 * Number));

            case 'w': return new Date(Date.parse(dtTmp) + ((86400000 * 7) * Number));

            case 'q': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number * 3, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());

            case 'm': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());

            case 'y': return new Date((dtTmp.getFullYear() + Number), dtTmp.getMonth(), dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
        }
        return date;
    },

    StringToDate: function (DateStr) {
        var converted = Date.parse(DateStr);
        var myDate = new Date(converted);
        if (isNaN(myDate)) {
            //var delimCahar = DateStr.indexOf('/')!=-1?'/':'-';
            var arys = DateStr.split('-');
            myDate = new Date(arys[0], --arys[1], arys[2]);
        }
        return myDate;
    },

    format: function (date, str) {
        str = str.replace(/yyyy|YYYY/, date.getFullYear());
        str = str.replace(/MM/, date.getMonth() >= 9 ? ((date.getMonth() + 1) * 1).toString() : '0' + (date.getMonth() + 1) * 1);
        str = str.replace(/dd|DD/, date.getDate() > 9 ? date.getDate().toString() : '0' + date.getDate());
        return str;
    },
    loadStyleString: function (cssText) {
        var script = document.createElement("script");
        script.type = "text/javascript";
        try {
            // firefox、safari、chrome和Opera
            script.appendChild(document.createTextNode(code));
        } catch (ex) {
            // IE早期的浏览器 ,需要使用script的text属性来指定javascript代码。
            script.text = code;
        }
        document.getElementsByTagName("head")[0].appendChild(script);
    },
    isOffice: function (extname) {
        return $.inArray(extname.toLowerCase(), ['doc', 'docx', 'ppt', 'pptx', 'pdf']) != -1
    },
    isPic: function (extname) {
        return $.inArray(extname.toLowerCase(), ['jpg', 'jpeg', 'gif', 'png', 'bmp']) != -1
    },
    getfile: function (fileid) {
        var url = "/ToolS/DownFile.aspx?szhlcode=" + ComFunJS.getCookie("szhlcode");
        if (fileid) {
            url = url + "&fileId=" + fileid;
        }
        return url;
    },
    requestFullScreen: function () {
        var de = document.documentElement;
        if (de.requestFullscreen) {
            de.requestFullscreen();
        } else if (de.mozRequestFullScreen) {
            de.mozRequestFullScreen();
        } else if (de.webkitRequestFullScreen) {
            de.webkitRequestFullScreen();
        }
    },
    exitFullscreen: function () {
        var de = document;
        if (de.exitFullscreen) {
            de.exitFullscreen();
        } else if (de.mozCancelFullScreen) {
            de.mozCancelFullScreen();
        } else if (de.webkitCancelFullScreen) {
            de.webkitCancelFullScreen();
        }
    },
    //弹出帮助函数
    winsuccess: function (content) {
        app.$notify({
            title: '警告',
            message: content,
            type: 'success'
        });


    },//成功窗口

    winwarning: function (content) {
        app.$notify({
            title: '警告',
            message: content,
            type: 'warning'
        });
    },//警告窗口
    //formcomponents: {
    //    'qjDate': httpVueLoader('../../AppPage/FORMBI/vue/qjDate.vue'),
    //    'qjInput': httpVueLoader('../../AppPage/FORMBI/vue/qjInput.vue'),
    //    'qjSN': httpVueLoader('../../AppPage/FORMBI/vue/qjSN.vue'),
    //    'qjInputNum': httpVueLoader('../../AppPage/FORMBI/vue/qjInputNum.vue'),
    //    'qjSelect': httpVueLoader('../../AppPage/FORMBI/vue/qjSelect.vue?v=2'),
    //    'qjMSelect': httpVueLoader('../../AppPage/FORMBI/vue/qjMSelect.vue'),
    //    'qjSelbranch': httpVueLoader('../../AppPage/FORMBI/vue/qjSelbranch.vue'),
    //    'qjSeluser': httpVueLoader('../../AppPage/FORMBI/vue/qjSeluser.vue'),
    //    'qjLine': httpVueLoader('../../AppPage/FORMBI/vue/qjLine.vue'),
    //    'qjTable': httpVueLoader('../../AppPage/FORMBI/vue/qjTable.vue'),
    //    'qjFile': httpVueLoader('../../AppPage/FORMBI/vue/qjFile.vue')
    //},
    winviewform: function (url, title, width, height, callbact) {
        var width = width || $("body").width() * 2 / 3;
        var height = height || $(window).height() - 140; //$("body").height();
        var optionwin = {
            type: 2,
            fix: true, //不固定
            area: [width + 'px', height + 'px'],
            maxmin: true,
            content: url,
            title: title,
            shadeClose: false, //加上边框
            scrollbar: false,
            shade: 0.4,
            shift: 0,
            success: function (layero, index) {

            },
            end: function () {
                if (callbact) {
                    return callbact.call(this);
                }
            }
        }
        layer.open(optionwin);
    },
    winbtnwin: function (url, title, width, height, option, btcallbact) {
        var width = width || $("body").width() - 300;
        var height = height || $(window).height() * 0.8; //var height = height || $("#main").height();
        var optionwin = {
            type: 2,
            fix: true, //不固定
            area: [width + 'px', height + 'px'],
            maxmin: true,
            content: url,
            title: title,
            shade: 0.4,
            shift: 0,
            shadeClose: false,
            scrollbar: false,
            success: function (layero, index) {
                if ($(layero).find(".successfoot").length == 0) {
                    var footdv = $('<div class="successfoot" style="border-bottom-width: 1px; padding: 0 20px 0 10px;margin-top: -3px;height:50px;background: #fff;"></div>');
                    var btnConfirm = $("<a href='javascript:void(0)' class='btn btn-sm btn-success' style='float:right; margin-top: 10px;width: 140px;'><i class='fa fa-spinner fa-spin' style='display:none'></i> 确   认</a>");
                    var btnCancel = $("<a href='javascript:void(0)' class='btn btn-sm btn-danger' style='float:right; margin-top: 10px;margin-right: 10px;width: 80px;'>取  消</a>");
                    var msg = $("<input type='hidden' class='r_data' >");

                    btnConfirm.appendTo(footdv).bind('click', function () {
                        return btcallbact.call(this, layero, index, btnConfirm);
                    })
                    btnCancel.appendTo(footdv).bind('click', function () {
                        layer.close(index)
                    })
                    $(layero).append(footdv).append(msg);

                    try {
                    } catch (e) { }
                }

            }
        }
        layer.open(optionwin);
    },//带确认框的窗口
    getnowdate: function (format, date) {
        var now = new Date();
        if (date) {
            now = new Date(Date.parse(date.replace(/-/g, '/')));
        }
        format = format.toLowerCase();
        var year = now.getFullYear();       //年
        var month = now.getMonth() + 1;     //月
        var day = now.getDate();            //日
        var hh = now.getHours();
        var mm = now.getMinutes();
        var ss = now.getSeconds();

        var clock = year + "-";
        if (format == "yyyy") {
            clock = year;
            return clock + "";
        }
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
})





$(function () {



    $.getJSON = function (url, data, success, opt) {
        data.szhlcode = ComFunJS.getCookie("szhlcode");
        var fn = {
            success: function (data, textStatus) { }
        }
        if (success) {
            fn.success = success;
        }
        $.ajax({
            url: url,
            data: data,
            dataType: "json",
            type: "post",
            success: function (data, textStatus) {
                if (data.ErrorMsg == "NOSESSIONCODE" || data.ErrorMsg == "WXTIMEOUT") {
                    //top.ComFunJS.winwarning("页面超时!")
                    top.window.location.href = "/ViewV5/Login.html";
                    return;
                }
                if (data.ErrorMsg) {
                    //top.ComFunJS.winwarning(data.ErrorMsg)
                    app.$notify({
                        title: '警告',
                        message: data.ErrorMsg,
                        type: 'warning'
                    });
                }
                fn.success(data, textStatus);
            },

            beforeSend: function (XHR) {
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            },
            complete: function (XHR, TS) {

            }
        });
    };
    $.getJSONAsync = function (url, data, success, opt) {
        data.szhlcode = ComFunJS.getCookie("szhlcode");
        var fn = {
            success: function (data, textStatus) { }
        }
        if (success) {
            fn.success = success;
        }
        $.ajax({
            url: url,
            data: data,
            dataType: "json",
            type: "post",
            async: false,
            success: function (data, textStatus) {
                if (data.ErrorMsg == "NOSESSIONCODE" || data.ErrorMsg == "WXTIMEOUT") {
                    //top.ComFunJS.winwarning("页面超时!")
                    top.window.location.href = "/ViewV5/Login.html";
                    return;
                }
                if (data.ErrorMsg) {
                    //top.ComFunJS.winwarning(data.ErrorMsg)
                    app.$notify({
                        title: '警告',
                        message: data.ErrorMsg,
                        type: 'warning'
                    });
                }
                fn.success(data, textStatus);
            },

            beforeSend: function (XHR) {
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            },
            complete: function (XHR, TS) {

            }
        });
    };



})