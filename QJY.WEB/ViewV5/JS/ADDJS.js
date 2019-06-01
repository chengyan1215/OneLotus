var pmodel = avalon.define({
    $id: "APP_ADD",
    nowuser: ComFunJS.getnowuser(),//当前用户
    PathCode: "Loading",
    FormCode: ComFunJS.getQueryString("FormCode"),
    DataID: ComFunJS.getQueryString("ID", ""),//数据ID

    ExtData: [],//扩展数据
    isPC: true,
    isDraft: false,
    pmtitle: "表单",//手机端标题
    rdm: Math.random(),
    render: function () {
        if (!pmodel.isPC) {
            $("table").hide();
        }
        if (typeof (tempmodel) != "undefined" && tempmodel) {
            if (pmodel.DataID) {
                tempmodel.inittemp(pmodel.DataID);
            } else {
                tempmodel.inittemp();
            }
            if (pmodel.isPC && parent.layer) {//调整标题
                var index = parent.layer.getFrameIndex(window.name)
                parent.layer.title(tempmodel.name, index)
            } else {
                pmodel.pmtitle = tempmodel.name;
                document.title = tempmodel.name;
                $("table").show();
            }
            avalon.templateCache = null;
            //获取扩展数据
            $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_GETEXTDATA", { P1: pmodel.FormCode, P2: pmodel.DataID }, function (result) {
                if (result.ErrorMsg == "") {
                    if (!pmodel.DataID) {
                        $(result.Result).each(function (inx, itm) {
                            itm.ExtendDataValue = itm.DefaultValue;
                        })
                    }
                    pmodel.ExtData = result.Result;

                    if (pmodel.ExtData.size() > 0) {
                        if ($(".extdiv").length > 1) {
                            $(".extdiv").each(function () {
                                if ($(this).width() == 0) {
                                    $(this).remove();
                                }
                            })
                        }
                        $(".extdiv").append($("#extdiv"));
                    }
                    setTimeout("ComFunJS.initForm()", 500)
                }
            });
            //获取草稿
            pmodel.GetDraftData();
        }


    },
    SaveData: function (dom, isjp, type) {
        if (!pmodel.isPC) {
            $("table").hide();
        }
        var errmsg = "";
        errmsg = pmodel.CheckData();//验证错误
        if (errmsg) {
            top.ComFunJS.winwarning(errmsg);
            if (!pmodel.isPC) {
                $("table").show();
            }
            return;
        }
        else {

            if (pmodel.isPC) {
                $(dom).attr("disabled", true).find(".fa").show();//加上转圈样式
            }
            tempmodel.SaveData(function (result1) {
                if ($.trim(result1.ErrorMsg) == "") {

                    top.ComFunJS.winsuccess("操作成功");
                    if (tempmodel && $.isFunction(tempmodel.Complate)) {
                        setTimeout("tempmodel.Complate();", 1000);
                    } else {
                        pmodel.refiframe();
                    }

                    pmodel.SaveExtData(result1.Result.ID);
                    //删除草稿
                    pmodel.DelDraft();
                }
                else {
                    if (pmodel.isPC) {
                        $(dom).attr("disabled", false).find(".fa").hide();//加上转圈样式
                    }
                    if (!pmodel.isPC) {
                        $("table").show();
                    }
                }
            }, dom);
        }
    },
    SaveExtData: function (DATAID) {
        //保存扩展数据
        if (pmodel.ExtData.size() > 0) {
            $.post("/API/VIEWAPI.ashx?ACTION=XTGL_UPDATEEXTDATA", { P1: pmodel.FormCode, P2: DATAID, ExtData: JSON.stringify(pmodel.ExtData.$model) }, function (result) {
            })
        }
    },
    //存草稿
    DraftData: { "ID": "0", "FormCode": "", "FormID": "", "JsonData": "", "ExtData": "" },
    DraftList: [],
    //存草稿
    SaveDraft: function (dom) {
        if (tempmodel) {
            pmodel.DraftData.FormCode = pmodel.FormCode;
            if (pmodel.FormCode == "JFBX") {
                pmodel.DraftData.JsonData = JSON.stringify(tempmodel.GetDraftData());
            } else if (pmodel.FormCode == "QYHD") {
                pmodel.DraftData.JsonData = JSON.stringify(tempmodel.GetDraftData());
            } else {
                pmodel.DraftData.JsonData = JSON.stringify(tempmodel.modelData.$model);
            }
            pmodel.DraftData.ExtData = JSON.stringify(pmodel.ExtData.$model);

            $.post("/API/VIEWAPI.ashx?ACTION=XTGL_SAVEDRAFT", { P1: JSON.stringify(pmodel.DraftData.$model) }, function (result) {
                if (result.ErrorMsg == "") {
                    pmodel.DraftData = result.Result;
                    pmodel.GetDraftData();
                    top.ComFunJS.winsuccess("存草稿成功");
                }
            })
        }
    },
    //获取草稿
    GetDraftData: function () {
        $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_GETDRAFT", { P1: pmodel.FormCode, P2: "0" }, function (r) {
            if (r.ErrorMsg == "") {
                pmodel.DraftList = r.Result;
            }

        })
    },
    //选择草稿
    SelDraft: function (el) {
        pmodel.DraftData = el;
        if (el.JsonData) {
            if (pmodel.FormCode == "JFBX") {
                tempmodel.SetDraftData(JSON.parse(el.JsonData));
            } else if (pmodel.FormCode == "QYHD") {
                tempmodel.SetDraftData(JSON.parse(el.JsonData));
            } else {
                tempmodel.modelData = JSON.parse(el.JsonData);
            }

        }
        if (el.ExtData) {
            pmodel.ExtData = JSON.parse(el.ExtData);
        }
        setTimeout("ComFunJS.initForm()", 500);
    },
    //删除草稿
    DelDraft: function (el, event) {
        if (event) {
            event.stopPropagation();
        }
        var ID = 0;
        if (el) {
            ID = el.ID;
        } else {
            ID = pmodel.DraftData.ID;
        }
        $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_DELDRAFT", { P1: ID }, function (resultData) {
            if (resultData.ErrorMsg == "") {

                if (el) {
                    pmodel.DraftList.remove(el);
                }
            }
        })
    },
    CheckData: function () { //验证代码块
        var retmsg = "";
        if (pmodel.isPC) {
            if ($(".szhl_require")) {

                $(".szhl_require:visible, .szhl_Int:visible, .szhl_Phone:visible").each(function () {
                    var title = $(this).attr("title") ? $(this).attr("title") : "";
                    if ($(this).hasClass("szhl_UEEDIT") && $(this).hasClass("szhl_require") && ($(this).prop("tagName") == "DIV" && ($(this).text() == "" && $(this).find("img").length == 0))) {
                        retmsg = title + $(this).parent().parent().parent().parent().find("label").text() + "不能为空";
                    }
                    else if (!$(this).val() && $(this).hasClass("szhl_require") && !$(this).hasClass("szhl_UEEDIT")) {
                        retmsg = title + $(this).parent().prev("label").text().replace('*', '') + "不能为空";
                    } else if ($(this).hasClass("szhl_Int")) {
                        if ($(this).val() == "") {
                            retmsg = title + $(this).parent().prev("label").text().replace('*', '') + "不能为空";
                        }
                        if (!(/^[0-9]*$/.test($(this).val()))) {
                            retmsg = title + $(this).parent().prev("label").text().replace('*', '') + "必须是正整数";
                        }
                    }
                    else if ($(this).hasClass("szhl_Phone")) {
                        if ($(this).val() == "") {
                            retmsg = title + $(this).parent().prev("label").text().replace('*', '') + "不能为空";
                        }
                        if (!(/^0?1[3|4|7|5|8][0-9]\d{8}$/.test($(this).val()))) {
                            retmsg = title + $(this).parent().prev("label").text().replace('*', '') + "填写不正确";
                        }
                    }
                    if (retmsg != "") {
                        return false;
                    }

                })
            }
        }
        else {
            $(".szhl").each(function () {
                var title = $(this).attr("title") ? $(this).attr("title") : "";
                if ($(this).hasClass("szhl_require") && $(this).val() == "") {
                    var str = "请输入";
                    if ($(this).find("select").length > 0) {
                        str = "请选择";
                    }
                    retmsg = str + title + $(this).parent().parent().find(".label").text();
                }
                else if ($(this).hasClass("szhl_Int") && !(/^\+?[1-9][0-9]*$/.test($(this).val()))) {
                    retmsg = title + $(this).parent().parent().find(".label").text() + "必须是正整数";
                }
                else if ($(this).hasClass("szhl_Time") && ComFunJS.compareTime($(this).val(), "")) {
                    retmsg = title + $(this).parent().parent().find(".label").text() + "必须大于当前时间";
                }
                else if ($(this).hasClass("szhl_Phone")) {
                    if ($(this).val()) {
                        if (!(/^0?1[3|4|7|5|8][0-9]\d{8}$/.test($(this).val()))) {
                            retmsg = title + $(this).parent().parent().find(".label").text() + "填写不正确";
                        }
                    }
                }
                else if ($(this).hasClass("szhl_Float")) {
                    if ($(this).val()) {
                        if (!(/^(?!0+(?:\.0+)?$)(?:[1-9]\d*|0)(?:\.\d{1,2})?$/.test($(this).val()))) {
                            retmsg = title + $(this).parent().parent().find(".label").text() + "填写不正确";
                        }
                    }
                }
                if (retmsg != "") {
                    return false;
                }

            })

        }
        return retmsg;
    },
    refiframe: function () {//刷新父框架
        if (pmodel.isPC) {
            if (typeof (top.tempindex.GetLIST) == 'function') {
                setTimeout("top.tempindex.GetLIST()", 1000)
            } else {
                setTimeout("top.model.refpage()", 1000)
            }
            setTimeout("parent.layer.closeAll()", 1000)

        } else {

            if (ComFunJS.getQueryString("mpid")) {
                setTimeout("window.history.back();", 1000)
            }
            else {
                setTimeout("window.location.replace(location.href);", 1000)
            }
        }

    },
    qx: function () {
        parent.layer.closeAll();
    },
    jptj: function (event, dom) {
        if (event.ctrlKey && (event.keyCode == 13 || event.keyCode == 10)) {
            pmodel.SaveData(dom.find(".btnSucc")[0], true);
        }
    },
    init: function () {
        if (ComFunJS.getQueryString("PathCode")) {
            pmodel.PathCode = ComFunJS.getQueryString("PathCode");
        } else {
            if (pmodel.FormCode.indexOf("_") > 0) {
                pmodel.PathCode = pmodel.FormCode.split('_')[0] + '/' + pmodel.FormCode.split('_')[1];
                pmodel.FormCode = pmodel.FormCode.split('_')[1];
            } else {
                pmodel.PathCode = pmodel.FormCode + '/' + pmodel.FormCode;
            }
        }

    }
})

avalon.ready(function () {
    setTimeout("pmodel.init()", 500)

})

//微信预览图片
var myPhotoBrowserCaptions;
var urlData = [];
function fdtp(obj) {
    var str = $(obj).attr("urlid");
    if (!str) {

        $(".mall_pcp").each(function (index, ele) {
            if ($(ele).attr("src")) {
                $(ele).attr("urlid", urlData.length);
                urlData.push($(ele).attr("src"));
            }
        });
        myPhotoBrowserCaptions = $.photoBrowser({
            photos: urlData,
            theme: 'dark'
        });
    }

    myPhotoBrowserCaptions.open($(obj).attr("urlid") * 1);
}
//微信预览文件
function ylwj(YLUrl) {
    if (YLUrl) {
        window.location = YLUrl;
    }
}

