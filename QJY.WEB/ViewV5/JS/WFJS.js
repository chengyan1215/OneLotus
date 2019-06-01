var pmodel = avalon.define({
    $id: "APP_ADD",
    nowuser: ComFunJS.getnowuser(),//当前用户
    PathCode: "Loading",
    FormCode: ComFunJS.getQueryString("FormCode"),
    DataID: ComFunJS.getQueryString("ID", ""),//数据ID
    PIID: ComFunJS.getQueryString("PIID"),//流程ID
    PDID: ComFunJS.getQueryString("PDID"),//流程配置ID
    ExtData: [],//扩展数据
    PIMODEL: {},//流程数据
    PDMODEL: {},//流程数据
    TASKDATA: [],//任务数据
    USERDATA: [],//可选审核人数据
    CSUser: "",//抄送人
    CSQKData: [],//抄送人接收情况数据
    iscansp: false,//是否有处理单据权限
    isedit: "N",//是否是流程中得可编辑状态
    ISCANCEL:"N",
    isHasDataQX: "N",//是否有修改数据得权限（只有当数据创建人是当前人并且是普通表单时才为Y）
    isPC: true,
    isDraft: false,
    lctype: ComFunJS.getQueryString("LCTYPE", "-1"),//流程类型-1:没有流程,0:自由流程1:固定流程
    spReason: "同意",//默认审批意见
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
            pmodel.getwfdata(function () {
                if (pmodel.lctype == 0) {//不是自定义流程审批获取审核人用FormCode,否则用pmodel.PIID或者pmodel.PDID
                    pmodel.getSHUser();//如果有流程,则获取审核人
                }

                //获取扩展数据
                $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_GETEXTDATA", { P1: pmodel.FormCode, P2: pmodel.DataID, PDID: pmodel.PDID }, function (result) {
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
            });
            //获取草稿
            if (pmodel.isPC) {
                pmodel.GetDraft();
            }
        }


    },
    getwfdata: function (callback) {
        $.getJSON("/API/VIEWAPI.ashx?ACTION=LCSP_GETWFDATA", { P1: pmodel.PIID, DataID: pmodel.DataID, ModelCode: pmodel.FormCode, P2: pmodel.PDID }, function (result) {
            if (result.ErrorMsg == "") {//流程数据
                if (result.Result5) { //流程定义数据
                    pmodel.PDMODEL = result.Result5;
                    pmodel.CSUser = pmodel.PDMODEL.ChaoSongUser;

                }
                if (result.Result) {
                    pmodel.PIMODEL = result.Result;
                    pmodel.CSUser = pmodel.PIMODEL.ChaoSongUser;
                }
                if (result.Result1)//任务数据
                {
                    pmodel.TASKDATA = result.Result1;
                }
                if (result.Result2) {//判断当前用户是否具有审批权限
                    pmodel.iscansp = $.parseJSON(result.Result2).ISCANSP == "Y";
                    pmodel.ISCANCEL = $.parseJSON(result.Result2).ISCANCEL;

                }
                if (result.Result3) {
                    pmodel.lctype = result.Result3;//流程类型 
                    if (pmodel.lctype == 0 || pmodel.lctype == 1) {
                        $(".btnSucc").html('<i class="fa fa-plus"></i>送审');
                    }
                }
                if (result.Result4) {
                    pmodel.isedit = result.Result4;//是否可编辑 
                    if (pmodel.isedit == "Y") {
                        pmodel.isHasDataQX = "Y";
                    }
                }
                if (result.Result6) {
                    pmodel.CSQKData = result.Result6;
                }

                pmodel.LoadWFData();
                return callback.call(this);
            }
        })
    },
    getSHUser: function () {
        $.getJSON("/API/VIEWAPI.ashx?ACTION=LCSP_GETSPUSERLIST", { P1: pmodel.FormCode, PDID: pmodel.PDID, PIID: pmodel.PIID }, function (result) {
            if (result.ErrorMsg == "") {
                pmodel.USERDATA = result.Result;
            }
        });
    },//获取具有审核权限的人员
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
            //如果是自由流程,并且审核人为空,则提示选择审核人
            if (pmodel.lctype == "0" && !$("#conshr").val()) {
                top.ComFunJS.winwarning("请选择审核人");
                $("table").show();
                return;
            }
            if (pmodel.isPC) {
                $(dom).attr("disabled", true).find(".fa").show();//加上转圈样式
            }
            tempmodel.SaveData(function (result1) {
                if ($.trim(result1.ErrorMsg) == "") {
                    pmodel.SaveExtData(result1.Result.ID);
                    //删除草稿
                    if (pmodel.DraftData.ID != "0") {
                        pmodel.DelDraft();
                    }
                    
                    //如果MODELCODE有流程,开始流程数据
                    $.getJSON("/API/VIEWAPI.ashx?ACTION=LCSP_STARTWF", { P1: pmodel.FormCode, P2: $("#conshr").val(), PDID: pmodel.PDID, DATAID: result1.Result.ID, LCTYPE: pmodel.lctype, csr: pmodel.CSUser }, function (result) {
                        if ($.trim(result.ErrorMsg) == "") {
                            top.ComFunJS.winsuccess("操作成功");
                            if (tempmodel && $.isFunction(tempmodel.Complate)) {
                                setTimeout("tempmodel.Complate();", 1000);
                            } else {
                                if (type == 'addlxr') {
                                    top.ComFunJS.winviewform("/ViewV5/AppPage/APP_ADD_WF.html?FormCode=CRM_KHLXR&khid=" + result1.Result.ID, "客户联系人", "1000");
                                    var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
                                    top.layer.close(index);
                                }
                                else {
                                    pmodel.refiframe();
                                }
                            }

                        }
                    });
                    //将新的表单数据存到草稿中去
                    pmodel.SaveDraft(result1.Result.ID);

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
    DraftData: { "ID": "0", "FormCode": "", "FormID": "", "JsonData": "", "ExtData": "", "DataID": "" },
    DraftList: [],
    //存草稿
    SaveDraft: function (DATAID) {
        if (tempmodel) {
            pmodel.DraftData.FormCode = pmodel.FormCode;
            if (typeof (tempmodel.GetDraftData) == 'function') {
                pmodel.DraftData.JsonData = JSON.stringify(tempmodel.GetDraftData());
            } else {
                pmodel.DraftData.JsonData = JSON.stringify(tempmodel.modelData.$model);
            }
            pmodel.DraftData.ExtData = JSON.stringify(pmodel.ExtData.$model);
            if (pmodel.PDMODEL) {
                pmodel.DraftData.FormID = pmodel.PDMODEL.ID;
            }
            if (DATAID) {
                pmodel.DraftData.DataID = DATAID;
            }
            $.post("/API/VIEWAPI.ashx?ACTION=XTGL_SAVEDRAFT", { P1: JSON.stringify(pmodel.DraftData.$model) }, function (result) {
                if (result.ErrorMsg == "") {
                    pmodel.DraftData = result.Result;
                    pmodel.GetDraft();
                    top.ComFunJS.winsuccess("存草稿成功");
                }
            })
        }
    },
    //获取草稿
    GetDraft: function () {
        $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_GETDRAFT", { P1: pmodel.FormCode, P2: pmodel.PDID }, function (r) {
            if (r.ErrorMsg == "") {
                pmodel.DraftList = r.Result;
            }

        })
    },
    //选择草稿
    SelDraft: function (el) {
        pmodel.DraftData = el;
        if (el.JsonData) {
            if (typeof (tempmodel.SetDraftData) == 'function') {
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
        $.getJSON("/API/VIEWAPI.ashx?ACTION=XTGL_DELDRAFT", { P1: ID }, function (r) {
            if (el) {
                pmodel.DraftList.remove(el);
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
    rebackform: function () {

        top.ComFunJS.winconfirm("确定要退回吗?", function () {
            if (pmodel.spReason == "同意") {
                pmodel.spReason = "退回";
            }
            $.post("/API/VIEWAPI.ashx?ACTION=LCSP_REBACKWF", { P1: pmodel.PIID, P2: pmodel.spReason, ID: pmodel.DataID, formcode: pmodel.FormCode }, function (result) {
                if ($.trim(result.ErrorMsg) == "") {
                    top.ComFunJS.winsuccess("退回成功");
                    if (tempmodel && $.isFunction(tempmodel.rebackform)) {
                        tempmodel.rebackform(pmodel.DataID);
                    }
                    pmodel.refiframe();
                }
            });
        }, function () { })

    },//退回表单
    refiframe: function () {//刷新父框架
        if (pmodel.isPC) {
            if (typeof (top.tempindex.GetLIST) == 'function') {
                setTimeout("top.tempindex.GetLIST()", 1000)
            } else {
                setTimeout("top.model.refpage()", 1000)
            }
            setTimeout("parent.layer.closeAll()", 3000)

        } else {

            if (ComFunJS.getQueryString("mpid")) {
                setTimeout("window.history.back();", 1000)
            }
            else {
                setTimeout("window.location.replace(location.href);", 1000)
            }
        }

    },
    showcsdata: function () {
        var $dom = $("#csdata").html();
        ComFunJS.AlertMsg($dom, function () {
            ComFunJS.closeAll();
        })
    },
    managetask: function () { //固定流程的处理
        $.getJSON("/API/VIEWAPI.ashx?ACTION=LCSP_MANAGEWF", { P1: pmodel.PIID, P2: pmodel.spReason, ID: pmodel.DataID, formcode: pmodel.FormCode, csr: pmodel.CSUser }, function (result) {
            if ($.trim(result.ErrorMsg) == "") {
                top.ComFunJS.winsuccess("处理成功");

                if (pmodel.isedit == "Y") {//如果可编辑，就保存数据
                    tempmodel.SaveData(function (result) { }, $(".btnSucc")[0]);                
                    pmodel.SaveExtData(pmodel.DataID);
                }
                if (result.Result == "Y") {
                    if (tempmodel && $.isFunction(tempmodel.WFComplate)) {
                        tempmodel.WFComplate();
                    }
                }//流程结束
                pmodel.refiframe();

            }
        });
    },
    mobsh: function () {
        top.ComFunJS.showComment("", function (coment) {
            alert(coment)
        })
    },
    transferform: function () {//转审
        if (pmodel.nowuser == $("#conshr1").val()) {
            top.ComFunJS.winwarning("您不能转审给自己哦");
            return;
        }
        if (!$("#conshr1").val()) {
            top.ComFunJS.winwarning("请选择审核人");
            $("table").show();
            return;
        }
        $.post("/API/VIEWAPI.ashx?ACTION=LCSP_MANAGEWF", { P1: pmodel.PIID, P2: pmodel.spReason, SHUser: $("#conshr1").val(), ID: pmodel.DataID, formcode: pmodel.FormCode }, function (result) {
            top.ComFunJS.winsuccess("转审成功");
            pmodel.refiframe();
        });


    },
    CancelWF: function () {//撤回表单到草稿箱
        ComFunJS.winconfirm("确认要撤回此流程吗？", function () {
            $.getJSON("/API/VIEWAPI.ashx?ACTION=LCSP_CANCELWF", { P1: pmodel.PIID, DataID: pmodel.DataID, ModelCode: pmodel.FormCode, P2: pmodel.PIMODEL.PDID }, function (result) {
                if (result.ErrorMsg == "") {//流程数据
                    pmodel.ISCANCEL = "N";
                    if (pmodel.isPC) {
                        top.ComFunJS.winconfirm("操作成功,该表单已撤回到草稿箱,是否要重新发起该表单", function () {
                            location.href = "/ViewV5/AppPage/APP_ADD_WF.html?FormCode=" + pmodel.FormCode + "&PDID=" + pmodel.PIMODEL.PDID + "&lctype=" + pmodel.lctype;
                            if (tempindex && $.isFunction(tempindex.CancelWF)) {
                                tempindex.CancelWF(pmodel.strId);
                            }

                        }, function () {
                            top.layer.closeAll();
                        })
                    } else {
                        top.ComFunJS.winsuccess("操作成功")
                        if (tempmodel && $.isFunction(tempmodel.CancelWF)) {
                            tempmodel.CancelWF(pmodel.strId);
                        }
                    }
                }
            })
        }, function () { })
    },
    qx: function () {
        parent.layer.closeAll();
    },
    jptj: function (event, dom) {
        if (event.ctrlKey && (event.keyCode == 13 || event.keyCode == 10)) {
            pmodel.SaveData(dom.find(".btnSucc")[0], true);
        }
    },
    MobileWFData: [],//手机流程数据
    LoadWFData: function () {
        if (pmodel.TASKDATA.size() > 0) {
            var lcspTaskData = [];
            var lcspTask = { title: "", content: "" }
            var lcspTaskm = { title: "", content: "", Date: "" };//手机流程数据
            var array = ["第一步", "第二步", "第三步", "第四步", "第五步", "第六步", "第七步", "第八步"];
            var stepCount = 0;
            $(pmodel.TASKDATA).each(function (i, item) {
                var reason = "";
                if (pmodel.PIMODEL.IsCanceled == 'Y') {
                    reason = item.EndTime && item.TaskUserView ? "[" + item.TaskUserView + "]\r\n" : "";
                } else {
                    reason = item.EndTime && item.TaskUserView ? "[" + item.TaskUserView + "]\r\n" : "待处理"
                }
                var tt = pmodel.lctype == 1 ? item.TaskName : array[i];
                if (pmodel.isPC) {
                    lcspTask.title = (tt ? tt : item.TaskUserView) + (pmodel.lctype == 1 && item.TaskAssInfo ? "(" + item.TaskAssInfo + ")" : "");
                    lcspTask.content = item.userrealname + reason + (item.EndTime ? item.EndTime : "");
                    stepCount += item.EndTime ? 1 : 0;
                    var itemData = $.extend({}, lcspTask);
                    lcspTaskData.push(itemData);
                } else {
                    lcspTaskm.title = (tt ? tt : item.TaskUserView) + (pmodel.lctype == 1 && item.TaskAssInfo ? "(" + item.TaskAssInfo + ")" : ""); //lcspTask.title;
                    lcspTaskm.content = item.userrealname + reason;
                    lcspTaskm.Date = item.EndTime ? item.EndTime : "";
                    pmodel.MobileWFData.push($.extend({}, lcspTaskm));
                }

            })
            if (pmodel.isPC) {
                //loadStep 方法可以初始化ystep 
                $(".lcspstep").loadStep({
                    size: "large",
                    color: "green",
                    steps: lcspTaskData

                });
                $(".lcspstep").setStep(stepCount);
                if (pmodel.PIMODEL.IsCanceled == 'Y') {
                    var dom;
                    if (pmodel.lctype == 0) {
                        dom = $(".ystep-container-steps .ystep-step-undone").last();
                    } else {
                        dom = $(".ystep-container-steps .ystep-step-undone").eq(stepCount - 1);
                    }
                    $(dom).css("color", "red");
                    $(dom).text($(dom).text() + "(退回)")
                }
            }
        }
    },
    shzt: function () {
        var zt = '正在审批';

        if (pmodel.PIMODEL.isComplete == "Y") {
            zt = '已审批';
        }
        else if (pmodel.PIMODEL.IsCanceled == "Y") {
            zt = '已退回';
        }
        return zt;
    },
    init: function () {
        if (pmodel.FormCode.indexOf("_") > 0) {
            pmodel.PathCode = pmodel.FormCode.split('_')[0] + '/' + pmodel.FormCode.split('_')[1];
            pmodel.FormCode = pmodel.FormCode.split('_')[1];
        } else {
            pmodel.PathCode = pmodel.FormCode + '/' + pmodel.FormCode;
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

