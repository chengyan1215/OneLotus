var model = avalon.define({
    $id: "IndexV5",
    userName: ComFunJS.getnowuser(),
    CommonData: [],//消息中心
    yytype: "WORK",
    UserData: {},//用户信息
    UserInfo: {},//用户缓存数据
    CompanyData: {},//企业信息
    isshowload: true,
    isiframe: "N",
    isgdkd: "N",//是否固定宽度
    XXCount: 0,//消息数量
    QYGGData: [],//企业公告
    YYData: [],
    LMData: [],
    QYHDData: [], //企业活动
    wigetdata: [],//工作台组件数据CompanyData
    initobj: "",//初始化要传给组件的数据
    ishasRight: true,//是否要隐藏左侧菜单
    FunData: [],//选中模块
    isnull: false,//是否有数据
    SelModelMenu: function (item) {
        var nowTime = new Date().getTime();
        var clickTime = $("body").data("ctime");
        if (clickTime != 'undefined' && (nowTime - clickTime < 1000) && item) {
            console.debug('操作过于频繁，稍后再试');
            return false;
        } else {
            $("body").data("ctime", nowTime);
            model.FunData.clear();
            if (item) {
                model.SelModel = item;
                model.FunData.pushArray(item.FunData.$model);
                model.ishasRight = false;
            } else {
                model.SelModel = null;
                if (localStorage.getItem("WIGETDATAV5")) {
                    model.FunData.pushArray(JSON.parse(localStorage.getItem("WIGETDATAV5")));
                } else {
                    // model.FunData = [{ code: "RWGL", name: "任务管理", wigetpath: "RWGL/RWGLLIST", issel: true, isshow: true, order: 0 }, { code: "LCSP", name: "流程管理", wigetpath: "RWGL/RWGLLIST", issel: false, isshow: true, order: 2 }, { code: "NOTE", name: "记事本", wigetpath: "RWGL/RWGLLIST", issel: false, isshow: true, order: 3 }, { code: "KJFS", name: "快捷网址", wigetpath: "RWGL/RWGLLIST", issel: false, isshow: true, order: 4 }];
                    model.FunData.pushArray([
                        { PageCode: "/ViewV5/AppPage/FORMBI/BDSHLIST", ExtData: "", code: "LCSP", PageName: "待办及审批", issel: true, isshow: true, order: 0 }
                        //{ PageCode: "/ViewV5/AppPage/RWGL/RWGLLIST", ExtData: "", code: "RWGL", PageName: "任务管理", issel: true, isshow: true, order: 0 },
                        //{ PageCode: "/ViewV5/TempWiget/KJFS", ExtData: "", code: "KJFS", PageName: "快捷网址", issel: true, isshow: true, order: 0 },
                        //{ PageCode: "/ViewV5/TempWiget/NOTE", ExtData: "", PageName: "记事本", code: "NOTE", issel: true, isshow: true, order: 0 }
                    ]);
                    localStorage.setItem("WIGETDATAV5", JSON.stringify(model.FunData.$model));

                }
                model.ishasRight = true;
            }
            model.selmenulev2(model.FunData[0]);
            $('body,html').animate({ scrollTop: 0 }, '500');
        }

    },//选中最左侧事件
    SelModelXX: function () {
        model.FunData.clear();
        model.SelModel = null;
        model.FunData.pushArray([{ PageCode: "/ViewV5/AppPage/XXZX/XXZXLIST", PageName: "消息管理", issel: true, isshow: true, order: 0 }]);
        model.ishasRight = false;
        model.selmenulev2(model.FunData[0]);
    },//选中最左侧事件
    SelUserInfo: function () {
        model.FunData.clear();
        model.SelModel = null;
        model.FunData.pushArray([{ PageCode: "/ViewV5/AppPage/XTGL/UserCenter", PageName: "个人中心", issel: true, isshow: true, order: 0 }]);
        model.ishasRight = false;
        model.selmenulev2(model.FunData[0]);
    },//选中消息的事件
    SelModelBZ: function () {
        model.FunData.clear();
        model.SelModel = null;
        model.FunData.pushArray([{ PageCode: "/ViewV5/AppPage/BZZX/BZZXLIST", PageName: "帮助中心", issel: true, isshow: true, order: 0 }]);
        model.ishasRight = false;
        model.selmenulev2(model.FunData[0]);
    },
    SelModelWTFK: function () {
        model.FunData.clear();
        model.SelModel = null;
        model.FunData.pushArray([{ PageCode: "/ViewV5/AppPage/WTFK/WTFKLIST", PageName: "问题反馈", issel: true, isshow: true, order: 0 }]);
        model.ishasRight = false;
        model.selmenulev2(model.FunData[0]);
    },
    selmenulev2: function (item, dom) {
        model.isiframe = item.isiframe;
        if (model.isiframe == 'Y') {
            var pagecode = item.PageCode.indexOf("html") > -1 ? item.PageCode : item.PageCode + ".html";
            $("#main").attr("src", pagecode).css("min-height", (window.innerHeight - 150) + 'px').parent().css("height", $("#main").height());
        } else {
            var nowTime = new Date().getTime();
            var clickTime = $("body").data("me2time");
            if (clickTime != 'undefined' && (nowTime - clickTime < 1000) && dom) {
                console.debug('操作过于频繁，稍后再试');
                return false;
            } else {
                $("body").data("me2time", nowTime);
                model.isnull = false;
                model.initobj = null;//先清空数据
                model.PageCode = "/ViewV5/Base/Loading";
                gomenu = function () {
                    model.PageCode = item.PageCode;
                    if (localStorage.getItem(model.PageCode + "pagecount")) {
                        model.page.pagecount = localStorage.getItem(model.PageCode + "pagecount");
                    } else {
                        model.page.pagecount = 10;
                    }
                    model.initobj = item.ExtData;
                    model.page.pageindex = 1;
                    model.page.total = 0;
                    model.ShowColumns = [];
                    model.TypeData = [];
                    model.ListData = [];
                    model.search.seartype = '1';
                    model.search.searchcontent = '';

                    //清除日历样式
                    $(".datetimepicker").remove()
                }
                setTimeout("gomenu()", 500)
            }
        }

    },
    //选中二级菜单事件
    ChangePage: function (pagedata) {
        model.selmenulev2(pagedata);
    },
    refpage: function (pagecode) {
        model.rdm = ComFunJS.getnowdate('yyyy-mm-dd hh:mm:ss');
        if (model.isiframe == 'Y') {
            $('#main').attr('src', $('#main').attr('src'));
        } else {
            if (pagecode) {

                for (var i = 0; i < model.FunData.length; i++) {
                    if (model.FunData[i].PageCode.indexOf(pagecode) > -1) {
                        model.selmenulev2(model.FunData[i]);
                        return;
                    }
                }
            } else {
                model.refpage(model.PageCode)
            }
        }


    },//刷新页面
    initwork: function () {
        localStorage.removeItem("WIGETDATAV5");
        location.reload();
    },
    setwork: function () {
        if (model.SelModel) {
            var temp = JSON.parse(localStorage.getItem("WIGETDATAV5"));
            temp.forEach(function (el) {
                if (el.PageCode == model.PageCode) {
                    return;
                }
            })
            model.FunData.forEach(function (item) {
                if (model.PageCode == item.PageCode) {
                    var fun = { PageCode: item.PageCode, ExtData: item.ExtData, code: model.SelModel.ModelCode, PageName: item.PageName, issel: true, isshow: true, order: temp.length * 1 + 1 }
                    temp.push(fun)
                }
            })
            localStorage.setItem("WIGETDATAV5", JSON.stringify(temp));
            top.ComFunJS.winsuccess("操作成功");
        }
    },//设置当前模块到控制台
    PageCode: "/ViewV5/Base/Loading",//需要加载的模板
    rdm: ComFunJS.getnowdate('yyyy-mm-dd hh:mm'),//随机数
    Temprender: function () {
        if (typeof (tempindex) != "undefined" && model.PageCode != "/ViewV5/Base/Loading") {
            tempindex.InitWigetData(model.initobj);
            if (model.ShowColumns.size() > 0) {
                setTimeout("model.GetExtColumns('" + model.rdm + "')", 1500);
            }
        }
    },//组件加载完成事件
    exit: function () {
        ComFunJS.winconfirm("确认要退出吗？", function () {
            ComFunJS.delCookie('szhlcode');

            location.href = "/ViewV5/login.html"
        })
    },//退出事件
    refiframe: function () {
        location.reload();

    }, //刷新当前页面
    selyyType: function (item, dom) {
        $(".yytype").removeClass("active")
        $(dom).children("a").addClass("active")
        model.yytype = item.TYPE;

        var yycount = 0;
        for (var i = 0; i < model.UseYYList.length; i++) {
            if (model.UseYYList[i].PModelCode == model.yytype && model.yytype != "WORK") {
                yycount++;
            }
        }
        if (yycount == 1) {
            $(".nav-list ul li:visible").eq(0).click();
            $(".leftlayout ").hide()
            $(".rightlayout ").css({ "margin-left": "0px" })

        } else {
            $(".leftlayout ").show()
            $(".rightlayout ").css({ "margin-left": "210px" })
            $(".nav-list ul li:visible").eq(0).click();

        }
        if (model.yytype == "ZXPXQT") {
            model.isgdkd = 'Y'
        } else {
            model.isgdkd = 'N'
        }

    },//应用类别
    menutype: "WORK",
    GetXXZXList: function () {
        $.getJSON('/API/VIEWAPI.ashx?Action=XTGL_GETXXZXIST', {}, function (resultData) {
            if (resultData.ErrorMsg == "") {
                model.CommonData = resultData.Result;
                model.XXCount = resultData.Result1;
            }
        })
    },//加载消息中心
    GetUserData: function () {
        $.getJSON('/API/VIEWAPI.ashx?Action=XTGL_GETUSERBYUSERNAME', { P1: ComFunJS.getnowuser() }, function (resultData) {
            if (resultData.ErrorMsg == "" && resultData.Result) {
                model.UserInfo = resultData.Result;
                model.UserData = resultData.Result.User;
                model.CompanyData = resultData.Result.QYinfo;
                $(document).attr("title", model.CompanyData.QYName);//修改title值
                ComFunJS.setCookie('fileapi', resultData.Result.QYinfo.FileServerUrl);
                ComFunJS.setCookie('qycode', resultData.Result.QYinfo.QYCode);
                ComFunJS.setCookie('userinfo', model.UserData.UserName + "," + model.UserData.UserRealName + "," + model.UserData.BranchCode + "," + model.UserInfo.BranchInfo.DeptName);

            }
        })

        $.getJSON('/API/VIEWAPI.ashx?Action=XXFB_GETXXFBLIST', { "type": "1" }, function (resultData) {
            if (resultData.ErrorMsg == "") {
                model.QYGGData = resultData.Result;
            }
        })
    }, //获取用户信息
    AuthList: [],//用户有权限的菜单
    KJFSData: [],//可以设置快捷方式的功能

    SETKJFS: function (item, event) {
        if (event) {
            event.stopPropagation();
        }
        $.post('/API/VIEWAPI.ashx?Action=INIT_SETAPPINDEX', { "P1": item.ModelCode, "Status": item.issel ? "N" : "Y", type: "PCKJFS", name: item.ModelName }, function (result) {
            if (result.ErrorMsg == "") {
                item.issel = item.issel ? false : true;
                ComFunJS.winsuccess("设置成功")
                model.refiframe();
            }
        })


    },//设置快捷方式
    AddView: function (code, Name, ID, pcode, event) {
        if (event) {
            event.stopPropagation();
        }
        if (code == "QYTX" || code == "DXGL") {
            ComFunJS.winviewform("/View/Base/APP_ADD_WF.html?FormCode=" + code, Name, "1000");
        }
        else {
            if (!ID) {
                ID = "";
            }
            if (pcode == "CRM") {
                code = pcode + "_" + code;
            }
            ComFunJS.winviewform("/ViewV5/AppPage/APP_ADD_WF.html?FormCode=" + code + "&ID=" + ID, Name, "1000");

        }
    },//添加表格
    AddViewNOWF: function (code, Name, ID, pcode, event) {
        if (event) {
            event.stopPropagation();
        }
        if (!ID) {
            ID = "";
        }
        ComFunJS.winviewform("/ViewV5/AppPage/APP_ADD.html?FormCode=" + code + "&ID=" + ID, Name, "1000");

    },
    EditViewNOWF: function (code, ID, pid, event) {
        if (event) {
            event.stopPropagation();
        }
        event = event ? event : window.event
        var obj = event.srcElement ? event.srcElement : event.target;
        if ($(obj).hasClass("icon-check") || $(obj).attr("type") == "checkbox") {
            return;
        } else {
            ComFunJS.winviewform("/ViewV5/AppPage/APP_ADD.html?FormCode=" + code + "&ID=" + ID, "查看");
        }
    },
    ViewForm: function (code, ID, PIID, event) {
        event = event ? event : window.event
        var obj = event.srcElement ? event.srcElement : event.target;
        if ($(obj).hasClass("icon-check") || $(obj).attr("type") == "checkbox") {
            return;
        } else {
            ComFunJS.winviewform("/ViewV5/AppPage/APPVIEW.html?FormCode=" + code + "&ID=" + ID + "&PIID=" + PIID + "&r=" + Math.random(), "查看");

        }
    },//查看表格方法
    ViewFormNew: function (code, ID, PIID, event) {
        event = event ? event : window.event
        var obj = event.srcElement ? event.srcElement : event.target;
        if ($(obj).hasClass("lk")) {
            ComFunJS.winviewform("/ViewV5/AppPage/APPVIEW.html?FormCode=" + code + "&ID=" + ID + "&PIID=" + PIID + "&r=" + Math.random(), "查看");

        }
    },//查看表格方法
    EditForm: function (code, ID, PIID, event) {
        if (event) {
            event.stopPropagation();
        }
        ComFunJS.winviewform("/ViewV5/AppPage/APP_ADD_WF.html?FormCode=" + code + "&ID=" + ID + "&PIID=" + PIID + "&r=" + Math.random(), "修改", "1000");
    },
    UseYYList: [],
    GetYYList: function () {
        $.getJSON('/API/VIEWAPI.ashx?Action=INIT_GETINDEXMENUNEW', { P1: "PCINDEX" }, function (resultData) {
            if (resultData.ErrorMsg == "") {
                resultData.Result.forEach(function (val, i) {
                    val.issel = val.issel == "True";
                })
                if (resultData.Result) {
                    resultData.Result.forEach(function (val) {
                        val.FunData.forEach(function (c) {
                            c.isshow = true;
                        })
                    })
                }
                // YYData

                var temp = [];
                for (var i = 0; i < resultData.Result.length; i++) {
                    if ($.inArray(resultData.Result[i].ModelType, temp) == -1) {
                        temp.push(resultData.Result[i].ModelType)
                        model.LMData.push({ "TYPE": resultData.Result[i].PModelCode, "NAME": resultData.Result[i].ModelType, "ISSEL": "N" });
                    }

                }
                model.UseYYList = resultData.Result;
                model.yytype = model.LMData[0].TYPE;

                var yycount = 0;
                for (var i = 0; i < model.UseYYList.length; i++) {
                    if (model.UseYYList[i].PModelCode == model.yytype && model.yytype != "WORK") {
                        yycount++;
                    }
                }
                if (yycount == 1) {
                    $(".nav-list ul li:visible").eq(0).click();
                    $(".leftlayout ").hide()
                    $(".rightlayout ").css({ "margin-left": "0px" })
                } else {
                    $(".leftlayout ").show()
                    $(".rightlayout ").css({ "margin-left": "210px" })
                    $(".nav-list ul li:visible").eq(0).click();

                }

                $(".nav-list ul li:visible").eq(0).click();
                if (model.LMData[0].$model.TYPE == "ZXPXQT") {
                    model.isgdkd = 'Y'
                }
            }
        })
    },
    SaveYY: function (item) {
        var strContent = "";
        if (item.ISSY == 1) {
            strContent += item.ModelCode + ":N";
        }
        else {
            strContent += item.ModelCode + ":Y";
        }
        if (item.ISSY == 1) {
            item.ISSY = 0;
        }
        else {
            item.ISSY = 1;
        }
        if (strContent) {
            $.post("/API/VIEWAPI.ashx?Action=INIT_SETAPPINDEX", { P1: strContent, type: "PCINDEX" }, function (jsonresult) {
                if ($.trim(jsonresult.ErrorMsg) == "") {
                    top.ComFunJS.winsuccess("操作成功");
                }
            });
        }
    },
    UploadHeadImage: function () {
        ComFunJS.winviewform("/ViewV5/Base/UploadTX.html", "头像上传", "700", "570");
    },  //上传头像
    ModifyPwd: function (dom) {
        var pwd = $("#newPwd").val();
        var pwd2 = $("#newPwd2").val();
        var retmsg = "";
        if ($("#UpdatePDModal .szhl_require")) {
            $("#UpdatePDModal .szhl_require").each(function () {
                if ($(this).val() == "") {
                    retmsg = $(this).parent().find("label").text() + "不能为空";
                }
            })
        }
        if (retmsg !== "") {
            top.layer.tips(retmsg, dom);
            return;
        }
        if (pwd != pwd2) {
            retmsg = "确认密码不一致";
            top.layer.tips(retmsg, dom);
            return;
        }
        $.post("/API/VIEWAPI.ashx?Action=XTGL_MODIFYPWD", { P1: pwd, P2: pwd2 }, function (jsonresult) {
            $(dom).removeClass("disabled").find("i").hide();
            if ($.trim(jsonresult.ErrorMsg) == "") {
                top.ComFunJS.winsuccess("操作成功");
                $('#UpdatePDModal').modal('hide');
            }
        });
        $("#newPwd").val("");
        $("#newPwd2").val("");
    },  //修改密码  待完善
    SaveWigetdata: function () {
        localStorage.setItem("WIGETDATAV5", JSON.stringify(model.FunData.$model));
        model.SelModelMenu('');
    },
    ChangeModelIsShow: function (item, dom, event) {
        if (event) {
            event.stopPropagation();
        }
        item.isshow = !item.isshow;
        model.SaveWigetdata();
    },

    //***通用列表页需要的数据***//
    TypeData: [], //类型数据
    GetTypeData: function (P1, callback) {//P1:字典类别，callback:回调函数,p2:字典类别ID
        $.getJSON('/API/VIEWAPI.ashx?Action=XTGL_GETZIDIANSLIST', { P1: P1 }, function (resultData) {
            if (resultData.ErrorMsg == "") {
                if (callback) {
                    return callback.call(this, resultData.Result);
                }
                else {
                    model.TypeData = resultData.Result;
                }
            }
        })
    },
    UserCustomData: [],
    GetCustomData: function (P1, callback) {//P1:类型，callback:回调函数
        $.getJSON('/API/VIEWAPI.ashx?Action=XTGL_GETUSERGROUP', { P1: P1 }, function (resultData) {
            if (resultData.ErrorMsg == "") {
                if (callback) {
                    return callback.call(this, resultData.Result);
                }
                else {
                    model.UserCustomData = resultData.Result;
                }
            }
        })
    },
    DelCustomData: function (item, event) {
        if (event.stopPropagation) {
            event.stopPropagation();
        }
        top.ComFunJS.winconfirm("确认要删除自定义搜索“" + item.DataContent + "”吗？", function () {
            $.post("/API/VIEWAPI.ashx", { Action: "XTGL_DELUSERGROUP", P1: item.ID }, function (jsonresult) {
                if ($.trim(jsonresult.ErrorMsg) == "") {
                    model.UserCustomData.remove(item);
                    top.ComFunJS.winsuccess("删除成功");
                }
            })
        })
    },
    ShowColumns: [],  //显示的列名，数据在模板中填充
    ListData: [], //列表页数据
    page: { pageindex: 1, pagecount: 10, total: 0 }, //分页参数
    pageNum: [{ "num": 10 }, { "num": 20 }, { "num": 30 }, { "num": 50 }, { "num": 100 }],
    GetExtColumns: function (str) {  //获取扩展字段

        if (model.SelModel && model.rdm == str) {
            $.getJSON('/API/VIEWAPI.ashx?Action=XTGL_GETEXTENDFIELD', { P1: model.SelModel.ModelCode }, function (resultData) {
                if (resultData.ErrorMsg == "") {
                    $(resultData.Result).each(function (idx, itm) {
                        model.ShowColumns.push({ "ColName": itm.TableFiledName, "ColText": itm.TableFiledName, "IsSel": false, "format": "" });
                    })

                    var ext = localStorage.getItem(model.SelModel.ModelCode + "ShowColumns");
                    if (ext) {
                        $(model.ShowColumns).each(function (idx, itm) {
                            $(JSON.parse(ext)).each(function (index, ele) {
                                if (itm.ColName == ele.ColName) {
                                    itm.IsSel = ele.IsSel;
                                }
                            })
                        })

                    }
                }
            })
        }
    },
    ReSetShow: function (el) { //控制扩展字段展示
        if (el && model.SelModel) {
            if (model.SelModel.ModelCode != 'LCSP' && el.type != "link") {
                el.IsSel = !el.IsSel;
                localStorage.setItem(model.SelModel.ModelCode + "ShowColumns", JSON.stringify(model.ShowColumns));
            }
            else {
                if (typeof (tempindex) != "undefined") {
                    tempindex.ReSetShow(el);
                }
            }
        }
    },
    search: { seartype: "1", searchcontent: "" },
    ViewXXFB: function (xxitem) {
        ComFunJS.winviewform("/ViewV5/AppPage/XXFB/XXFBVIEW.html?ID=" + xxitem.ID + "&r=" + Math.random(), "新闻公告");
    },
    //***通用列表页需要的数据***//
    mouseover: function () {
        $(this).find(".tool").css("display", "block");
    },
    mouseout: function () {
        $(this).find(".tool").css("display", "none");
    },//鼠标移动控制显示和隐藏
    KQGZData: {},
    QDData: { QDDate: "未签到", Status: -1 },
    QTData: { QTDate: "未签退", Status: -1 },
    QDStatus: 0,
    GetKQGZ: function () {
        $.getJSON('/API/VIEWAPI.ashx?Action=KQGL_GETKQGZ', {}, function (resultData) {
            if (resultData.ErrorMsg == "") {
                if (resultData.Result.length > 0) {
                    model.KQGZData = resultData.Result[0];
                }
                if (resultData.Result1.length > 0) {
                    model.QDData.QDDate = resultData.Result1[0].KQDate;
                    model.QDData.Status = resultData.Result1[0].Status;
                    model.QDStatus = 1;
                }
                if (resultData.Result2.length > 0) {
                    model.QTData.QTDate = resultData.Result2[0].KQDate;
                    model.QTData.Status = resultData.Result2[0].Status;
                    model.QDStatus = 2;
                }
            }
        })
    },
    GetQDStausName: function () {
        var strName = "";
        switch (model.QDStatus) {
            case 0:
                strName = "签到";
                break;
            case 1:
                strName = "签退";
                break;
            case 2:
                strName = "考勤结束";
                break;
        }
        return strName;
    },
    SaveQD: function () {
        if (model.QDStatus == 2) {
            return;
        }
        $.post("/API/VIEWAPI.ashx", { Action: "KQGL_ADDKQJL", P1: model.QDStatus }, function (jsonresult) {
            if ($.trim(jsonresult.ErrorMsg) == "") {
                if (model.QDStatus == 0) {
                    model.QDData.QDDate = jsonresult.Result.KQDate;
                    model.QDData.Status = jsonresult.Result.Status;
                    model.QDStatus = 1;
                } else if (model.QDStatus == 1) {
                    model.QTData.QTDate = jsonresult.Result.KQDate;
                    model.QTData.Status = jsonresult.Result.Status;
                    model.QDStatus = 2;
                }
                top.ComFunJS.winsuccess((model.QDStatus == 1 ? "签到" : "签退") + "成功");
            }
        })
    }, OpenSet: function (event) {
        if (event) {
            event.stopPropagation();
        }
        $("#myModal").modal("show");
    }
})
avalon.ready(function () {
    $.ajaxSettings.async = false;
    model.GetYYList();
    model.GetUserData();
    model.GetXXZXList();

})

model.page.$watch("pagecount", function () {
    localStorage.setItem(model.PageCode + "pagecount", model.page.pagecount);
})

